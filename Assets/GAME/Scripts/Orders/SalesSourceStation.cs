using UnityEngine;

public class SalesSourceStation : MonoBehaviour, IInteractable
{
    [SerializeField] private DemandSourceDefinition source;

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
}
