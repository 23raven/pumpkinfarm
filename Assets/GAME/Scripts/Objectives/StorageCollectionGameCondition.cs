using System.Text;
using UnityEngine;

/// <summary>
/// Ends the gameplay loop once every configured requirement is present in the
/// scene's StorageContainer objects. Counts items currently stored, not items
/// that were collected in the past.
/// </summary>
public sealed class StorageCollectionGameCondition : MonoBehaviour
{
    [SerializeField] private StorageCollectionGoal goal;

    private bool isComplete;
    private bool hasWarnedAboutInvalidGoal;

    public bool IsComplete => isComplete;
    public StorageCollectionGoal Goal => goal;

    private void OnEnable()
    {
        GameEvents.Subscribe<StorageChangedEvent>(OnStorageChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<StorageChangedEvent>(OnStorageChanged);
    }

    private void Start()
    {
        RecalculateAndPublish();
    }

    /// <summary>Creates the current left-column TODO text for the journal panel.</summary>
    public string BuildTodoText()
    {
        int[] counts = CalculateStoredCounts();
        bool requirementsMet = IsValidGoal() && AreRequirementsMet(counts);
        return BuildTodoText(counts, isComplete || requirementsMet);
    }

    private void OnStorageChanged(StorageChangedEvent message)
    {
        RecalculateAndPublish();
    }

    private void RecalculateAndPublish()
    {
        if (!IsValidGoal())
        {
            if (!hasWarnedAboutInvalidGoal)
            {
                Debug.LogWarning(
                    "StorageCollectionGameCondition needs a StorageCollectionGoal with at least one valid item requirement.",
                    this);
                hasWarnedAboutInvalidGoal = true;
            }

            GameEvents.Publish(new StorageCollectionProgressChangedEvent(
                BuildTodoText(new int[goal != null && goal.RequiredItems != null ? goal.RequiredItems.Length : 0], false),
                false));
            return;
        }

        int[] counts = CalculateStoredCounts();
        bool metNow = AreRequirementsMet(counts);
        bool completedForFirstTime = !isComplete && metNow;
        isComplete = isComplete || metNow;

        GameEvents.Publish(new StorageCollectionProgressChangedEvent(
            BuildTodoText(counts, isComplete),
            isComplete));

        if (completedForFirstTime)
        {
            // The goal is latched as complete. The ending cannot be undone by
            // moving items after all requirements have been satisfied.
            GameplayInputGate.SetBlocked(true);
            GameEvents.Publish(new StorageCollectionCompletedEvent(goal));
        }
    }

    private bool IsValidGoal()
    {
        if (goal == null || goal.RequiredItems == null || goal.RequiredItems.Length == 0)
            return false;

        foreach (StorageCollectionItemRequirement requirement in goal.RequiredItems)
        {
            if (requirement == null || requirement.Item == null || requirement.RequiredQuantity < 1)
                return false;
        }

        return true;
    }

    private int[] CalculateStoredCounts()
    {
        StorageCollectionItemRequirement[] requirements =
            goal != null ? goal.RequiredItems : null;

        int requirementCount = requirements != null ? requirements.Length : 0;
        int[] counts = new int[requirementCount];
        if (requirementCount == 0)
            return counts;

        // Count the contents of all StorageContainer components in this scene.
        // Items in the player's inventory or on the ground do not count.
        StorageContainer[] storages = FindObjectsOfType<StorageContainer>(true);

        foreach (StorageContainer storage in storages)
        {
            if (storage == null)
                continue;

            StorageSlotData[] slots = storage.GetSnapshot();
            if (slots == null)
                continue;

            foreach (StorageSlotData slot in slots)
            {
                if (slot.IsEmpty)
                    continue;

                for (int i = 0; i < requirementCount; i++)
                {
                    StorageCollectionItemRequirement requirement = requirements[i];
                    if (requirement != null && requirement.Item != null && slot.Item == requirement.Item)
                        counts[i] += slot.Quantity;
                }
            }
        }

        return counts;
    }

    private bool AreRequirementsMet(int[] counts)
    {
        if (!IsValidGoal() || counts == null || counts.Length != goal.RequiredItems.Length)
            return false;

        for (int i = 0; i < goal.RequiredItems.Length; i++)
        {
            if (counts[i] < goal.RequiredItems[i].RequiredQuantity)
                return false;
        }

        return true;
    }

    private string BuildTodoText(int[] counts, bool completed)
    {
        if (goal == null)
            return "TO DO\n\nAssign a Storage Collection Goal asset.";

        StorageCollectionItemRequirement[] requirements = goal.RequiredItems;
        if (requirements == null || requirements.Length == 0)
            return goal.GoalTitle + "\n\nAdd item requirements to the Storage Collection Goal asset.";

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(goal.GoalTitle);
        builder.AppendLine();

        for (int i = 0; i < requirements.Length; i++)
        {
            StorageCollectionItemRequirement requirement = requirements[i];
            if (requirement == null || requirement.Item == null)
            {
                builder.AppendLine("[!] Assign an item in the goal asset.");
                continue;
            }

            int current = i < counts.Length ? counts[i] : 0;
            bool met = current >= requirement.RequiredQuantity;
            builder.Append(met ? "[x] " : "[ ] ");
            builder.Append(requirement.Item.DisplayName);
            builder.Append(": ");
            builder.Append(current);
            builder.Append('/');
            builder.AppendLine(requirement.RequiredQuantity.ToString());
        }

        builder.AppendLine();
        if (completed)
        {
            builder.AppendLine("ALL SUPPLIES COLLECTED!");
            builder.Append("Halloween is ready!");
        }
        else if (!IsValidGoal())
        {
            builder.Append("Configure the missing item requirements in the Goal asset.");
        }
        else
        {
            builder.Append("Store all required items to finish.");
        }

        return builder.ToString();
    }
}

public readonly struct StorageCollectionProgressChangedEvent
{
    public string TodoText { get; }
    public bool IsComplete { get; }

    public StorageCollectionProgressChangedEvent(string todoText, bool isComplete)
    {
        TodoText = todoText;
        IsComplete = isComplete;
    }
}

public readonly struct StorageCollectionCompletedEvent
{
    public StorageCollectionGoal Goal { get; }

    public StorageCollectionCompletedEvent(StorageCollectionGoal goal)
    {
        Goal = goal;
    }
}
