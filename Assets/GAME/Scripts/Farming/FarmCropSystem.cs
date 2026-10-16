using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class FarmCropSystem : MonoBehaviour
{
    private sealed class PlantedCrop
    {
        public CropDefinition Definition;
        public int GrowthStage;
        public int HarvestRemaining;
        public int LastWateredDay = -1;
        public bool IsWithered;

        public bool IsReady =>
            !IsWithered &&
            Definition != null &&
            GrowthStage >= Definition.GrowthStageCount - 1;
    }

    private Tilemap tilemap;
    private FarmTileVisuals visuals;
    private CropDefinition[] availableCrops;
    private bool initialized;

    private readonly HashSet<Vector3Int> tilledCells =
        new HashSet<Vector3Int>();

    private readonly Dictionary<Vector3Int, PlantedCrop> plantedCells =
        new Dictionary<Vector3Int, PlantedCrop>();

    public void Initialize(
        Tilemap sourceTilemap,
        FarmTileVisuals tileVisuals,
        CropDefinition[] crops)
    {
        tilemap = sourceTilemap;
        visuals = tileVisuals;
        availableCrops = crops ?? System.Array.Empty<CropDefinition>();
        initialized = tilemap != null && visuals != null;
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<DayChangedEvent>(OnDayChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<DayChangedEvent>(OnDayChanged);
    }

    public bool HasCropAt(Vector3Int cell) => plantedCells.ContainsKey(cell);

    public bool IsReady(Vector3Int cell)
    {
        return plantedCells.TryGetValue(cell, out PlantedCrop crop) && crop.IsReady;
    }

    public bool IsWithered(Vector3Int cell)
    {
        return plantedCells.TryGetValue(cell, out PlantedCrop crop) && crop.IsWithered;
    }

    public CropDefinition FindCropForSeed(ItemDefinition item)
    {
        if (item == null || availableCrops == null)
            return null;

        foreach (CropDefinition crop in availableCrops)
        {
            if (crop != null && crop.SeedItem == item)
                return crop;
        }

        return null;
    }

    public bool CanTill(Vector3Int cell)
    {
        return IsInitialized() &&
               tilemap.HasTile(cell) &&
               !tilledCells.Contains(cell) &&
               !plantedCells.ContainsKey(cell);
    }

    public bool TryTill(Vector3Int cell)
    {
        if (!CanTill(cell))
        {
            Debug.Log("This soil cannot be tilled.");
            return false;
        }

        if (!visuals.SetTilled(cell))
        {
            Debug.LogWarning("Tilled Tile is not assigned.", this);
            return false;
        }

        tilledCells.Add(cell);
        Debug.Log("Soil tilled.");
        return true;
    }

    public bool CanPlant(Vector3Int cell, CropDefinition crop)
    {
        return IsInitialized() &&
               crop != null &&
               tilemap.HasTile(cell) &&
               tilledCells.Contains(cell) &&
               !plantedCells.ContainsKey(cell);
    }

    public bool TryPlant(
        Vector3Int cell,
        CropDefinition crop,
        InventoryService inventory)
    {
        if (!CanPlant(cell, crop))
        {
            Debug.Log("Till the soil before planting, and use an empty plot.");
            return false;
        }

        if (!crop.HasValidGrowthSprites ||
            crop.SeedItem == null ||
            crop.HarvestItem == null ||
            crop.WitheredSprite == null)
        {
            Debug.LogWarning(
                "Crop definition needs seeds, harvest item, growth sprites and a withered sprite.",
                crop
            );
            return false;
        }

        if (inventory == null || !inventory.HasItem(crop.SeedItem, 1))
        {
            Debug.Log("You don't have the required seeds.");
            return false;
        }

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<DayCycleService>(out DayCycleService dayCycle))
        {
            Debug.LogWarning("DayCycleService is unavailable.", this);
            return false;
        }

        if (!visuals.SetSprite(cell, crop.GetGrowthSprite(0)))
        {
            Debug.LogWarning("The first growth sprite is missing.", crop);
            return false;
        }

        if (!inventory.RemoveItem(crop.SeedItem, 1))
        {
            visuals.SetTilled(cell);
            return false;
        }

        plantedCells.Add(cell, new PlantedCrop
        {
            Definition = crop,
            GrowthStage = 0,
            HarvestRemaining = crop.HarvestAmount,
            LastWateredDay = -1,
            IsWithered = false
        });

        Debug.Log($"Planted {crop.SeedItem.DisplayName}.");
        return true;
    }

    public bool CanWater(Vector3Int cell)
    {
        if (!IsInitialized() ||
            !plantedCells.TryGetValue(cell, out PlantedCrop crop) ||
            crop.IsWithered)
        {
            return false;
        }

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<DayCycleService>(out DayCycleService dayCycle))
        {
            return false;
        }

        return crop.LastWateredDay != dayCycle.CurrentDay;
    }

    public bool TryWater(Vector3Int cell)
    {
        if (!plantedCells.TryGetValue(cell, out PlantedCrop crop))
        {
            Debug.Log("There is no crop to water.");
            return false;
        }

        if (crop.IsWithered)
        {
            Debug.Log("This crop has withered.");
            return false;
        }

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<DayCycleService>(out DayCycleService dayCycle))
        {
            Debug.LogWarning("DayCycleService is unavailable.", this);
            return false;
        }


        if (crop.LastWateredDay == dayCycle.CurrentDay)
        {
            Debug.Log("This crop is already watered today.");
            return false;
        }

        crop.LastWateredDay = dayCycle.CurrentDay;
        visuals.ShowWatered(cell);
        Debug.Log("Crop watered.");
        return true;
    }

    public bool TryHarvest(Vector3Int cell, InventoryService inventory)
    {
        if (!plantedCells.TryGetValue(cell, out PlantedCrop crop) || !crop.IsReady)
            return false;

        if (inventory == null)
            return false;

        ItemDefinition harvestItem = crop.Definition.HarvestItem;
        int remaining = inventory.AddItem(harvestItem, crop.HarvestRemaining);
        int collected = crop.HarvestRemaining - remaining;
        crop.HarvestRemaining = remaining;

        if (collected > 0)
            Debug.Log($"Harvested {collected} {harvestItem.DisplayName}.");

        if (remaining > 0)
        {
            Debug.Log("Inventory is full. Harvest the remaining crop later.");
            return false;
        }

        plantedCells.Remove(cell);
        visuals.ResetColor(cell);
        visuals.SetTilled(cell);
        Debug.Log("Crop harvested.");
        return true;
    }

    public bool ClearWitheredCrop(Vector3Int cell)
    {
        if (!plantedCells.TryGetValue(cell, out PlantedCrop crop) || !crop.IsWithered)
            return false;

        if (!visuals.SetTilled(cell))
        {
            Debug.LogWarning("Tilled Tile is not assigned.", this);
            return false;
        }

        plantedCells.Remove(cell);
        visuals.ResetColor(cell);
        tilledCells.Add(cell);
        Debug.Log("Withered crop cleared.");
        return true;
    }

    private void OnDayChanged(DayChangedEvent message)
    {
        if (!IsInitialized())
            return;

        int previousDay = message.Day - 1;

        foreach (KeyValuePair<Vector3Int, PlantedCrop> entry in plantedCells)
        {
            Vector3Int cell = entry.Key;
            PlantedCrop crop = entry.Value;

            if (crop.IsWithered)
                continue;

            // Mature crops no longer need watering.
            if (crop.IsReady)
            {
                visuals.ResetColor(cell);
                continue;
            }

            if (crop.LastWateredDay != previousDay)
            {
                crop.IsWithered = true;
                visuals.ResetColor(cell);
                visuals.SetSprite(cell, crop.Definition.WitheredSprite);

                Debug.LogWarning(
                    $"{crop.Definition.HarvestItem.DisplayName} withered because it was not watered."
                );
                continue;
            }

            visuals.ResetColor(cell);

            int nextStage = Mathf.Min(
                crop.GrowthStage + 1,
                crop.Definition.GrowthStageCount - 1
            );

            if (!visuals.SetSprite(cell, crop.Definition.GetGrowthSprite(nextStage)))
                continue;

            crop.GrowthStage = nextStage;

            if (crop.IsReady)
            {
                Debug.Log(
                    $"{crop.Definition.HarvestItem.DisplayName} is ready!"
                );
            }
        }
    }

    private bool IsInitialized()
    {
        return initialized && tilemap != null && visuals != null;
    }

    public bool IsWateredToday(Vector3Int cell)
    {
        if (!plantedCells.TryGetValue(cell, out PlantedCrop crop) || crop.IsWithered)
            return false;

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<DayCycleService>(out DayCycleService dayCycle))
        {
            return false;
        }

        return crop.LastWateredDay == dayCycle.CurrentDay;
    }

}
