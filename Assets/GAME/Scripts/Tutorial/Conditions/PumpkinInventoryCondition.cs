using UnityEngine;

public class PumpkinInInventoryCondition : TutorialCondition
{
    [Header("Item")]
    [SerializeField] private ItemDefinition pumpkinItem;

    [SerializeField, Min(1)] private int requiredAmount = 1;

    private InventoryService inventory;
    private bool warnedAboutMissingItem;

    public override bool IsMet(TutorialStep step)
    {
        if (pumpkinItem == null)
        {
            if (!warnedAboutMissingItem)
            {
                Debug.LogWarning(
                    $"PumpkinInInventoryCondition on '{name}' needs Pumpkin Item assigned.",
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

        return inventory.HasItem(pumpkinItem, requiredAmount);
    }
}