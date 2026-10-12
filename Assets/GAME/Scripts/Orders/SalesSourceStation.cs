using UnityEngine;

public class SalesSourceStation : MonoBehaviour, IInteractable
{
    [SerializeField] private DemandSourceDefinition source;
    [SerializeField] private GameObject activeOrderIndicator;

    private void OnEnable()
    {
        GameEvents.Subscribe<DailyOrdersChangedEvent>(OnOrdersChanged);
        RefreshIndicator();
    }

    private void Start()
    {
        RefreshIndicator();
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<DailyOrdersChangedEvent>(OnOrdersChanged);
    }

    public void Interact(GameObject interactor)
    {
        if (interactor == null || source == null)
        {
            Debug.LogWarning("SalesSourceStation: Interactor or source is missing.", this);
            return;
        }

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<DailyOrderService>(out DailyOrderService orders))
        {
            Debug.LogWarning("DailyOrderService is unavailable.", this);
            return;
        }

        orders.TryFulfill(source.SourceId);
    }

    private void OnOrdersChanged(DailyOrdersChangedEvent message)
    {
        RefreshIndicator();
    }

    private void RefreshIndicator()
    {
        if (activeOrderIndicator == null)
            return;

        GameServices services = GameServices.Instance;
        bool hasOrder = source != null &&
                        services != null &&
                        services.TryGet<DailyOrderService>(out DailyOrderService orders) &&
                        orders.HasActiveOrder(source.SourceId);

        activeOrderIndicator.SetActive(hasOrder);
    }
}
