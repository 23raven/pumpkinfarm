using UnityEngine;

public class CropPlotController : MonoBehaviour, IInteractable
{
    private enum CropState
    {
        Empty,
        Growing,
        Ready
    }

    [Header("Crop")]
    [SerializeField] private CropDefinition crop;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer plotRenderer;
    [SerializeField] private Color emptyColor = Color.white;
    [SerializeField] private Color growingColor = Color.green;
    [SerializeField] private Color readyColor = Color.yellow;

    private CropState state = CropState.Empty;
    private int plantedOnDay;
    private int harvestRemaining;

    private void Awake()
    {
        if (plotRenderer == null)
            plotRenderer = GetComponent<SpriteRenderer>();

        UpdateVisual();
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<DayChangedEvent>(OnDayChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<DayChangedEvent>(OnDayChanged);
    }

    public void Interact(GameObject interactor)
    {
        if (crop == null)
        {
            Debug.LogWarning("CropPlotController: Crop is not assigned.", this);
            return;
        }

        switch (state)
        {
            case CropState.Empty:
                TryPlant();
                break;

            case CropState.Growing:
                Debug.Log("The crop is still growing.");
                break;

            case CropState.Ready:
                TryHarvest();
                break;
        }
    }

    private void TryPlant()
    {
        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<InventoryService>(out InventoryService inventory) ||
            !services.TryGet<DayCycleService>(out DayCycleService dayCycle))
        {
            Debug.LogWarning("Required game services are unavailable.", this);
            return;
        }

        if (crop.SeedItem == null)
        {
            Debug.LogWarning("Crop has no seed item assigned.", this);
            return;
        }

        if (!inventory.RemoveItem(crop.SeedItem, 1))
        {
            Debug.Log("You don't have the required seeds.");
            return;
        }

        plantedOnDay = dayCycle.CurrentDay;
        state = CropState.Growing;

        UpdateVisual();

        Debug.Log($"Planted {crop.SeedItem.DisplayName}.");
    }

    private void OnDayChanged(DayChangedEvent message)
    {
        if (state != CropState.Growing)
            return;

        if (message.Day - plantedOnDay < crop.DaysToGrow)
            return;

        state = CropState.Ready;
        harvestRemaining = crop.HarvestAmount;

        UpdateVisual();

        Debug.Log($"{crop.HarvestItem.DisplayName} is ready to harvest!");
    }

    private void TryHarvest()
    {
        if (crop.HarvestItem == null)
        {
            Debug.LogWarning("Crop has no harvest item assigned.", this);
            return;
        }

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<InventoryService>(out InventoryService inventory))
        {
            Debug.LogWarning("InventoryService is unavailable.", this);
            return;
        }

        int remaining = inventory.AddItem(
            crop.HarvestItem,
            harvestRemaining
        );

        int collected = harvestRemaining - remaining;

        harvestRemaining = remaining;

        if (collected > 0)
        {
            Debug.Log($"Harvested {collected} {crop.HarvestItem.DisplayName}.");
        }

        if (harvestRemaining == 0)
        {
            state = CropState.Empty;
            UpdateVisual();
        }
        else
        {
            Debug.Log("Inventory is full. Harvest remaining: " + harvestRemaining);
        }
    }

    private void UpdateVisual()
    {
        if (plotRenderer == null)
            return;

        switch (state)
        {
            case CropState.Empty:
                plotRenderer.color = emptyColor;
                break;

            case CropState.Growing:
                plotRenderer.color = growingColor;
                break;

            case CropState.Ready:
                plotRenderer.color = readyColor;
                break;
        }
    }
}