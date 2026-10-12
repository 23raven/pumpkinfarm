using System;
using UnityEngine;

[Serializable]
public class DemandOption
{
    [SerializeField] private ItemDefinition item;
    [SerializeField, Min(1)] private int minQuantity = 1;
    [SerializeField, Min(1)] private int maxQuantity = 3;
    [SerializeField, Min(0)] private int rewardPerItem = 10;

    public ItemDefinition Item => item;
    public int MinQuantity => Mathf.Max(1, minQuantity);
    public int MaxQuantity => Mathf.Max(MinQuantity, maxQuantity);
    public int RewardPerItem => Mathf.Max(0, rewardPerItem);
}

[CreateAssetMenu(
    fileName = "NewDemandSource",
    menuName = "Game/Orders/Demand Source"
)]
public class DemandSourceDefinition : ScriptableObject
{
    [Header("Business")]
    [SerializeField] private string sourceId;
    [SerializeField] private string displayName = "Cafe";

    [Header("Daily Orders")]
    [SerializeField, Range(0f, 1f)] private float dailyOrderChance = 0.75f;
    [SerializeField] private DemandOption[] possibleOrders;

    public string SourceId => sourceId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public float DailyOrderChance => Mathf.Clamp01(dailyOrderChance);
    public DemandOption[] PossibleOrders => possibleOrders;
}
