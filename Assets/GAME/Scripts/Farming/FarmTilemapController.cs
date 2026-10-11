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
        Vector3Int cell = tilemap.WorldToCell(position);

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
            cell.x += direction.x >= 0 ? 1 : -1;
        else
            cell.y += direction.y >= 0 ? 1 : -1;

        return cell;
    }
}
