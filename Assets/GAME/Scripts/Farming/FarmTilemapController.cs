using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
[RequireComponent(typeof(FarmTileVisuals))]
[RequireComponent(typeof(FarmCropSystem))]
[RequireComponent(typeof(FarmTargetPreview))]
public sealed class FarmTilemapController : MonoBehaviour, IInteractable
{
    [Header("Tiles")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase tilledTile;

    [Header("Water Visual")]
    [SerializeField] private Color wateredTint =
        new Color(0.65f, 0.65f, 0.65f, 1f);

    [Header("Crops")]
    [SerializeField] private CropDefinition[] availableCrops;

    [Header("Target Preview")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer targetPreview;

    [SerializeField, Min(1f)]
    private float targetSearchRadiusInCells = 1.75f;

    private FarmTileVisuals visuals;
    private FarmCropSystem cropSystem;
    private FarmTargetPreview preview;

    private void Awake()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        visuals = GetComponent<FarmTileVisuals>();
        cropSystem = GetComponent<FarmCropSystem>();
        preview = GetComponent<FarmTargetPreview>();

        visuals.Initialize(tilemap, tilledTile, wateredTint);
        cropSystem.Initialize(tilemap, visuals, availableCrops);
        preview.Initialize(tilemap, targetPreview);
    }

    private void Update()
    {
        UpdateTargetPreview();
    }

    public void Interact(GameObject interactor)
    {
        if (interactor == null || tilemap == null || cropSystem == null)
            return;

        if (!TryGetInventory(out InventoryService inventory))
        {
            Debug.LogWarning("InventoryService is unavailable.", this);
            return;
        }

        IDirectionalInteractor directional =
            interactor.GetComponent(typeof(IDirectionalInteractor))
            as IDirectionalInteractor;

        if (directional == null)
            return;

        Vector3Int cell = GetTargetCell(
            GetInteractionPosition(interactor),
            directional.FacingDirection
        );

        if (!tilemap.HasTile(cell))
        {
            Debug.Log("There is no tillable soil here.");
            return;
        }

        ItemDefinition selectedItem = inventory.SelectedItem;

        // The watering can takes priority, including on mature crops.
        if (selectedItem != null && selectedItem.ItemId == "watering_can")
        {
            cropSystem.TryWater(cell);
            return;
        }

        if (cropSystem.IsReady(cell))
        {
            cropSystem.TryHarvest(cell, inventory);
            return;
        }

        if (cropSystem.IsWithered(cell))
        {
            if (selectedItem != null && selectedItem.ItemId == "hoe")
                cropSystem.ClearWitheredCrop(cell);
            else
                Debug.Log("Use the Hoe to clear the withered crop.");

            return;
        }

        if (selectedItem == null)
        {
            Debug.Log("Select a tool or seeds first.");
            return;
        }

        if (selectedItem.ItemId == "hoe")
        {
            cropSystem.TryTill(cell);
            return;
        }

        CropDefinition crop = cropSystem.FindCropForSeed(selectedItem);
        if (crop != null)
        {
            cropSystem.TryPlant(cell, crop, inventory);
            return;
        }

        Debug.Log("This item cannot be used here.");
    }

    private void UpdateTargetPreview()
    {
        if (preview == null || player == null || tilemap == null || cropSystem == null)
        {
            if (preview != null)
                preview.Hide();
            return;
        }

        IDirectionalInteractor directional =
            player.GetComponent(typeof(IDirectionalInteractor))
            as IDirectionalInteractor;

        if (directional == null)
        {
            preview.Hide();
            return;
        }

        Vector3Int cell = GetTargetCell(
            GetInteractionPosition(player.gameObject),
            directional.FacingDirection
        );

        if (CanInteractWithCell(cell))
            preview.ShowAt(tilemap.GetCellCenterWorld(cell));
        else
            preview.Hide();
    }

    private bool CanInteractWithCell(Vector3Int cell)
    {
        if (!tilemap.HasTile(cell))
            return false;

        if (cropSystem.IsReady(cell))
            return true;

        if (!TryGetInventory(out InventoryService inventory))
            return false;

        ItemDefinition selectedItem = inventory.SelectedItem;
        if (selectedItem == null)
            return false;

        if (cropSystem.IsWithered(cell))
            return selectedItem.ItemId == "hoe";

        if (selectedItem.ItemId == "watering_can")
            return cropSystem.CanWater(cell);

        if (selectedItem.ItemId == "hoe")
            return cropSystem.CanTill(cell);

        CropDefinition crop = cropSystem.FindCropForSeed(selectedItem);
        return crop != null && cropSystem.CanPlant(cell, crop);
    }

    private bool TryGetInventory(out InventoryService inventory)
    {
        inventory = null;
        GameServices services = GameServices.Instance;

        return services != null && services.TryGet(out inventory);
    }

    private Vector3 GetInteractionPosition(GameObject interactor)
    {
        PlayerInteractor playerInteractor =
            interactor.GetComponent<PlayerInteractor>();

        return playerInteractor != null
            ? playerInteractor.InteractionPosition
            : interactor.transform.position;
    }

    private Vector3Int GetTargetCell(Vector3 position, Vector2 direction)
    {
        Vector3Int playerCell = tilemap.WorldToCell(position);

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.left;

        bool horizontal =
            Mathf.Abs(direction.x) >= Mathf.Abs(direction.y);

        Vector3Int preferredCell = playerCell;

        if (horizontal)
            preferredCell.x += direction.x >= 0f ? 1 : -1;
        else
            preferredCell.y += direction.y >= 0f ? 1 : -1;

        // Сначала используем привычную клетку перед игроком.
        if (CanInteractWithCell(preferredCell))
            return preferredCell;

        // Если она недоступна, ищем ближайшую подходящую.
        return FindNearestAvailableTarget(
            position,
            playerCell,
            preferredCell,
            direction,
            horizontal
        );
    }

    private Vector3Int FindNearestAvailableTarget(
        Vector3 position,
        Vector3Int playerCell,
        Vector3Int preferredCell,
        Vector2 direction,
        bool horizontal)
    {
        float cellSize = Mathf.Max(
            Mathf.Abs(tilemap.cellSize.x),
            Mathf.Abs(tilemap.cellSize.y)
        );

        float maxDistance =
            cellSize * targetSearchRadiusInCells;

        int scanRadius =
            Mathf.CeilToInt(targetSearchRadiusInCells) + 1;

        Vector3Int bestCell = preferredCell;
        float bestScore = Mathf.Infinity;
        bool found = false;

        for (int x = preferredCell.x - scanRadius;
             x <= preferredCell.x + scanRadius; x++)
        {
            for (int y = preferredCell.y - scanRadius;
                 y <= preferredCell.y + scanRadius; y++)
            {
                Vector3Int candidate = new Vector3Int(x, y, playerCell.z);

                if (candidate == preferredCell)
                    continue;

                if (!CanInteractWithCell(candidate))
                    continue;

                int forwardDistance;
                int lateralDistance;

                if (horizontal)
                {
                    forwardDistance =
                        (candidate.x - playerCell.x) *
                        (direction.x >= 0f ? 1 : -1);

                    lateralDistance =
                        Mathf.Abs(candidate.y - playerCell.y);
                }
                else
                {
                    forwardDistance =
                        (candidate.y - playerCell.y) *
                        (direction.y >= 0f ? 1 : -1);

                    lateralDistance =
                        Mathf.Abs(candidate.x - playerCell.x);
                }

                // Не выбираем клетки позади игрока.
                if (forwardDistance <= 0)
                    continue;

                float distance = Vector2.Distance(
                    position,
                    tilemap.GetCellCenterWorld(candidate)
                );

                if (distance > maxDistance)
                    continue;

                int gridDistance =
                    Mathf.Abs(candidate.x - preferredCell.x) +
                    Mathf.Abs(candidate.y - preferredCell.y);

                // Ближайшие клетки имеют приоритет.
                // При равенстве предпочитаем продолжение текущего направления.
                float score =
                    gridDistance * 100f +
                    lateralDistance * 10f +
                    forwardDistance +
                    distance * 0.01f;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestCell = candidate;
                    found = true;
                }
            }
        }

        return found ? bestCell : preferredCell;
    }
}
