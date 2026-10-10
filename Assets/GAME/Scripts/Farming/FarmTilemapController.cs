using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
public class FarmTilemapController : MonoBehaviour, IInteractable
{
    [Header("Tiles")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase tilledTile;

    [Header("Crops")]
    [SerializeField] private CropDefinition[] availableCrops;

    [Header("Target Preview")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer targetPreview;

    private readonly HashSet<Vector3Int> tilledCells =
        new HashSet<Vector3Int>();

    private readonly Dictionary<Vector3Int, PlantedCrop> plantedCells =
        new Dictionary<Vector3Int, PlantedCrop>();

    private sealed class PlantedCrop
    {
        public CropDefinition Definition;
        public int PlantedOnDay;
        public int HarvestRemaining;
        public bool IsReady;
    }

    private void Awake()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        if (tilemap != null &&
            targetPreview != null &&
            targetPreview.sprite != null)
        {
            Vector3 cellSize = tilemap.cellSize;
            Vector3 spriteSize = targetPreview.sprite.bounds.size;

            if (spriteSize.x > 0f && spriteSize.y > 0f)
            {
                targetPreview.transform.localScale = new Vector3(
                    cellSize.x / spriteSize.x,
                    cellSize.y / spriteSize.y,
                    1f
                );
            }

            targetPreview.enabled = false;
        }
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<DayChangedEvent>(OnDayChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<DayChangedEvent>(OnDayChanged);
    }

    private void Update()
    {
        UpdateTargetPreview();
    }

    public void Interact(GameObject interactor)
    {
        if (interactor == null || tilemap == null)
            return;

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<InventoryService>(
                out InventoryService inventory))
        {
            Debug.LogWarning("InventoryService is unavailable.", this);
            return;
        }

        IDirectionalInteractor directional =
            interactor.GetComponent(typeof(IDirectionalInteractor))
            as IDirectionalInteractor;

        if (directional == null)
            return;

        Vector3 interactionPosition = GetInteractionPosition(interactor);

        Vector3Int cell = GetTargetCell(
            interactionPosition,
            directional.FacingDirection
        );

        if (!tilemap.HasTile(cell))
        {
            Debug.Log("There is no tillable soil here.");
            return;
        }

        // A mature crop can be harvested with any selected item.
        if (plantedCells.TryGetValue(cell, out PlantedCrop planted) &&
            planted.IsReady)
        {
            TryHarvest(cell, planted, inventory);
            return;
        }

        ItemDefinition selectedItem = inventory.SelectedItem;

        if (selectedItem == null)
        {
            Debug.Log("Select a tool or seeds first.");
            return;
        }

        if (selectedItem.ItemId == "hoe")
        {
            TryTill(cell);
            return;
        }

        CropDefinition crop = FindCropForSeed(selectedItem);

        if (crop != null)
        {
            TryPlant(cell, crop, inventory);
            return;
        }

        Debug.Log("This item cannot be used here.");
    }

    private void TryTill(Vector3Int cell)
    {
        if (tilledCells.Contains(cell))
        {
            Debug.Log("This soil is already tilled.");
            return;
        }

        if (tilledTile == null)
        {
            Debug.LogWarning("Tilled Tile is not assigned.", this);
            return;
        }

        tilledCells.Add(cell);
        tilemap.SetTile(cell, tilledTile);
        tilemap.RefreshTile(cell);

        Debug.Log("Soil tilled.");
    }

    private void TryPlant(
        Vector3Int cell,
        CropDefinition crop,
        InventoryService inventory)
    {
        if (!tilledCells.Contains(cell))
        {
            Debug.Log("Till the soil before planting.");
            return;
        }

        if (plantedCells.ContainsKey(cell))
        {
            Debug.Log("A crop is already planted here.");
            return;
        }

        if (crop.SeedItem == null ||
            crop.HarvestItem == null ||
            crop.GrowingTile == null ||
            crop.ReadyTile == null)
        {
            Debug.LogWarning(
                "Crop definition is missing an item or visual Tile.",
                crop
            );
            return;
        }

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<DayCycleService>(
                out DayCycleService dayCycle))
        {
            Debug.LogWarning("DayCycleService is unavailable.", this);
            return;
        }

        if (!inventory.RemoveItem(crop.SeedItem, 1))
        {
            Debug.Log("You don't have the required seeds.");
            return;
        }

        plantedCells.Add(cell, new PlantedCrop
        {
            Definition = crop,
            PlantedOnDay = dayCycle.CurrentDay,
            HarvestRemaining = crop.HarvestAmount,
            IsReady = false
        });

        tilemap.SetTile(cell, crop.GrowingTile);
        tilemap.RefreshTile(cell);

        Debug.Log($"Planted {crop.SeedItem.DisplayName}.");
    }

    private void OnDayChanged(DayChangedEvent message)
    {
        foreach (KeyValuePair<Vector3Int, PlantedCrop> entry
                 in plantedCells)
        {
            PlantedCrop planted = entry.Value;

            if (planted.IsReady)
                continue;

            int elapsedDays = message.Day - planted.PlantedOnDay;

            if (elapsedDays < planted.Definition.DaysToGrow)
                continue;

            planted.IsReady = true;

            tilemap.SetTile(
                entry.Key,
                planted.Definition.ReadyTile
            );

            tilemap.RefreshTile(entry.Key);

            Debug.Log(
                $"{planted.Definition.HarvestItem.DisplayName} is ready!"
            );
        }
    }

    private void TryHarvest(
        Vector3Int cell,
        PlantedCrop planted,
        InventoryService inventory)
    {
        ItemDefinition harvestItem = planted.Definition.HarvestItem;

        int remaining = inventory.AddItem(
            harvestItem,
            planted.HarvestRemaining
        );

        int collected = planted.HarvestRemaining - remaining;

        planted.HarvestRemaining = remaining;

        if (collected > 0)
        {
            Debug.Log(
                $"Harvested {collected} {harvestItem.DisplayName}."
            );
        }

        if (remaining > 0)
        {
            Debug.Log("Inventory is full. Harvest the remaining crop later.");
            return;
        }

        plantedCells.Remove(cell);

        // The soil remains tilled and can be used again.
        tilemap.SetTile(cell, tilledTile);
        tilemap.RefreshTile(cell);
    }

    private CropDefinition FindCropForSeed(ItemDefinition item)
    {
        if (availableCrops == null || item == null)
            return null;

        foreach (CropDefinition crop in availableCrops)
        {
            if (crop != null && crop.SeedItem == item)
                return crop;
        }

        return null;
    }

    private void UpdateTargetPreview()
    {
        if (targetPreview == null || player == null || tilemap == null)
            return;

        IDirectionalInteractor directional =
            player.GetComponent(typeof(IDirectionalInteractor))
            as IDirectionalInteractor;

        if (directional == null)
        {
            targetPreview.enabled = false;
            return;
        }

        Vector3 interactionPosition = GetInteractionPosition(player.gameObject);

        Vector3Int cell = GetTargetCell(
            interactionPosition,
            directional.FacingDirection
        );

        bool canInteract = CanInteractWithCell(cell);

        targetPreview.enabled = canInteract;

        if (canInteract)
        {
            targetPreview.transform.position =
                tilemap.GetCellCenterWorld(cell);
        }
    }

    private bool CanInteractWithCell(Vector3Int cell)
    {
        if (!tilemap.HasTile(cell))
            return false;

        // Mature crops can always be targeted for harvesting.
        if (plantedCells.TryGetValue(cell, out PlantedCrop planted) &&
            planted.IsReady)
        {
            return true;
        }

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<InventoryService>(
                out InventoryService inventory))
        {
            return false;
        }

        ItemDefinition selectedItem = inventory.SelectedItem;

        if (selectedItem == null)
            return false;

        if (selectedItem.ItemId == "hoe")
            return !tilledCells.Contains(cell);

        CropDefinition crop = FindCropForSeed(selectedItem);

        return crop != null &&
               tilledCells.Contains(cell) &&
               !plantedCells.ContainsKey(cell);
    }

    private Vector3 GetInteractionPosition(GameObject interactor)
    {
        PlayerInteractor playerInteractor =
            interactor.GetComponent<PlayerInteractor>();

        return playerInteractor != null
            ? playerInteractor.InteractionPosition
            : interactor.transform.position;
    }

    private Vector3Int GetTargetCell(
        Vector3 playerPosition,
        Vector2 direction)
    {
        Vector3Int cell = tilemap.WorldToCell(playerPosition);

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
            cell.x += direction.x >= 0 ? 1 : -1;
        else
            cell.y += direction.y >= 0 ? 1 : -1;

        return cell;
    }
}