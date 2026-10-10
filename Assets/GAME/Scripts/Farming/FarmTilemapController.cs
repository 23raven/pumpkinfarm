using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
public class FarmTilemapController : MonoBehaviour, IInteractable
{
    [Header("Tiles")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase tilledTile;
    [SerializeField] private TileBase plantedTile;

    [Header("Crops")]
    [SerializeField] private CropDefinition[] availableCrops;

    [Header("Target Preview")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer targetPreview;

    private readonly HashSet<Vector3Int> tilledCells =
        new HashSet<Vector3Int>();

    private readonly Dictionary<Vector3Int, CropDefinition> plantedCells =
        new Dictionary<Vector3Int, CropDefinition>();

    private void Awake()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        if (targetPreview != null && targetPreview.sprite != null)
        {
            Vector3 cellSize = tilemap.cellSize;
            Vector3 spriteSize = targetPreview.sprite.bounds.size;

            targetPreview.transform.localScale = new Vector3(
                cellSize.x / spriteSize.x,
                cellSize.y / spriteSize.y,
                1f
            );

            targetPreview.enabled = false;
        }
    }

    private void Update()
    {
        UpdateTargetPreview();
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

        Vector3Int cell = GetTargetCell(
            player.position,
            directional.FacingDirection
        );

        bool canInteract = CanInteractWithCell(cell);

        targetPreview.enabled = canInteract;

        if (canInteract)
            targetPreview.transform.position = tilemap.GetCellCenterWorld(cell);
    }

    private bool CanInteractWithCell(Vector3Int cell)
    {
        if (!tilemap.HasTile(cell))
            return false;

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<InventoryService>(
                out InventoryService inventory))
            return false;

        ItemDefinition selectedItem = inventory.SelectedItem;

        if (selectedItem == null)
            return false;

        if (selectedItem.ItemId == "hoe")
            return !tilledCells.Contains(cell);

        CropDefinition crop = FindCropForSeed(selectedItem);

        return crop != null
            && tilledCells.Contains(cell)
            && !plantedCells.ContainsKey(cell);
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

        ItemDefinition selectedItem = inventory.SelectedItem;

        if (selectedItem == null)
        {
            Debug.Log("Select a tool or seeds first.");
            return;
        }

        IDirectionalInteractor directional =
            interactor.GetComponent(typeof(IDirectionalInteractor))
            as IDirectionalInteractor;

        if (directional == null)
            return;

        Vector3Int cell = GetTargetCell(
            interactor.transform.position,
            directional.FacingDirection
        );

        if (!tilemap.HasTile(cell))
        {
            Debug.Log("There is no tillable soil here.");
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

        if (crop.SeedItem == null || crop.HarvestItem == null)
        {
            Debug.LogWarning("Crop definition is incomplete.", crop);
            return;
        }

        if (!inventory.RemoveItem(crop.SeedItem, 1))
        {
            Debug.Log("You don't have the required seeds.");
            return;
        }

        plantedCells.Add(cell, crop);

        if (plantedTile != null)
        {
            tilemap.SetTile(cell, plantedTile);
            tilemap.RefreshTile(cell);
        }

        Debug.Log($"Planted {crop.SeedItem.DisplayName}.");
    }

    private CropDefinition FindCropForSeed(ItemDefinition item)
    {
        if (availableCrops == null)
            return null;

        foreach (CropDefinition crop in availableCrops)
        {
            if (crop != null && crop.SeedItem == item)
                return crop;
        }

        return null;
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