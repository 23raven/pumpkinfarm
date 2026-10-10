using UnityEngine;

public class DayAdvanceInteractable : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        if (interactor == null)
            return;

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<DayCycleService>(
                out DayCycleService dayCycle))
        {
            Debug.LogWarning(
                "DayCycleService is unavailable.",
                this
            );

            return;
        }

        dayCycle.AdvanceDay();
    }
}