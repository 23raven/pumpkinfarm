using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StorageCollectionGoal",
    menuName = "Game/Objectives/Storage Collection Goal"
)]
public sealed class StorageCollectionGoal : ScriptableObject
{
    [SerializeField] private string goalTitle = "HALLOWEEN PREPARATIONS";
    [SerializeField] private StorageCollectionItemRequirement[] requiredItems =
        new StorageCollectionItemRequirement[0];

    public string GoalTitle => string.IsNullOrWhiteSpace(goalTitle)
        ? "TO DO"
        : goalTitle;

    public StorageCollectionItemRequirement[] RequiredItems => requiredItems;
}

[Serializable]
public sealed class StorageCollectionItemRequirement
{
    [SerializeField] private ItemDefinition item;
    [SerializeField, Min(1)] private int requiredQuantity = 1;

    public ItemDefinition Item => item;
    public int RequiredQuantity => Mathf.Max(1, requiredQuantity);
}
