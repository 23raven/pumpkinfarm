using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Completes when the amount of at least one item in the Cart inventory group
/// increases after this tutorial step becomes active.
/// </summary>
public class PutItemsInCartCondition : TutorialCondition
{
    private TutorialStep activeStep;
    private Dictionary<ItemDefinition, int> initialCartCounts =
        new Dictionary<ItemDefinition, int>();

    private bool initialized;

    public override bool IsMet(TutorialStep step)
    {
        if (step == null || !step.isActive)
            return false;

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<InventoryService>(out InventoryService inventory))
        {
            return false;
        }

        if (activeStep != step)
        {
            activeStep = step;
            initialCartCounts = GetCartItemCounts(inventory);
            initialized = true;
            return false;
        }

        if (!initialized)
        {
            initialCartCounts = GetCartItemCounts(inventory);
            initialized = true;
            return false;
        }

        Dictionary<ItemDefinition, int> currentCartCounts =
            GetCartItemCounts(inventory);

        foreach (KeyValuePair<ItemDefinition, int> entry in currentCartCounts)
        {
            initialCartCounts.TryGetValue(entry.Key, out int initialAmount);
            if (entry.Value > initialAmount)
                return true;
        }

        return false;
    }

    private Dictionary<ItemDefinition, int> GetCartItemCounts(
        InventoryService inventory)
    {
        var counts = new Dictionary<ItemDefinition, int>();

        if (inventory == null || inventory.Slots == null)
            return counts;

        foreach (InventorySlot slot in inventory.Slots)
        {
            if (slot == null ||
                slot.Group != InventorySlotGroup.Cart ||
                slot.IsEmpty ||
                slot.Item == null)
            {
                continue;
            }

            counts.TryGetValue(slot.Item, out int existingAmount);
            counts[slot.Item] = existingAmount + slot.Quantity;
        }

        return counts;
    }
}
