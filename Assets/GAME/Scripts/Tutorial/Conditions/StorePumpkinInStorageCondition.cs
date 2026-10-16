using UnityEngine;

/// <summary>
/// Completes when the amount of the assigned pumpkin item in the selected
/// storage container increases after this condition starts being checked.
/// </summary>
public class StorePumpkinInStorageCondition : TutorialCondition
{
    [Header("Storage")]
    [SerializeField] private StorageContainer targetStorage;

    [Header("Item")]
    [SerializeField] private ItemDefinition pumpkinItem;

    private int startingCount;
    private bool initialized;
    private bool warnedAboutMissingReferences;

    public override bool IsMet(TutorialStep step)
    {
        if (targetStorage == null || pumpkinItem == null)
        {
            if (!warnedAboutMissingReferences)
            {
                Debug.LogWarning(
                    $"StorePumpkinInStorageCondition on '{name}' needs both Target Storage and Pumpkin Item assigned in the Inspector.",
                    this);
                warnedAboutMissingReferences = true;
            }

            return false;
        }

        int currentCount = GetPumpkinCountInStorage();

        if (!initialized)
        {
            startingCount = currentCount;
            initialized = true;
            return false;
        }

        return currentCount > startingCount;
    }

    private int GetPumpkinCountInStorage()
    {
        StorageSlotData[] slots = targetStorage.GetSnapshot();
        int total = 0;

        if (slots == null)
            return total;

        foreach (StorageSlotData slot in slots)
        {
            if (slot.Item == pumpkinItem && slot.Quantity > 0)
                total += slot.Quantity;
        }

        return total;
    }
}
