using UnityEngine;

/// <summary>
/// Completes when the number of the assigned pumpkin item in the player's
/// inventory increases after this condition starts being checked.
/// </summary>
public class GetPumpkinInInventoryCondition : TutorialCondition
{
    [Header("Item")]
    [SerializeField] private ItemDefinition pumpkinItem;

    private InventoryService inventory;
    private int startingCount;
    private bool initialized;
    private bool warnedAboutMissingItem;

    public override bool IsMet(TutorialStep step)
    {
        if (pumpkinItem == null)
        {
            if (!warnedAboutMissingItem)
            {
                Debug.LogWarning(
                    $"GetPumpkinInInventoryCondition on '{name}' needs Pumpkin Item assigned in the Inspector.",
                    this);
                warnedAboutMissingItem = true;
            }

            return false;
        }

        if (inventory == null)
        {
            GameServices services = GameServices.Instance;
            if (services == null ||
                !services.TryGet<InventoryService>(out inventory))
            {
                return false;
            }
        }

        int currentCount = inventory.GetItemCount(pumpkinItem);

        if (!initialized)
        {
            startingCount = currentCount;
            initialized = true;
            return false;
        }

        return currentCount > startingCount;
    }
}
