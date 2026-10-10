using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCrop",
    menuName = "Game/Farming/Crop Definition"
)]
public class CropDefinition : ScriptableObject
{
    [Header("Planting")]
    [SerializeField] private ItemDefinition seedItem;

    [Header("Harvest")]
    [SerializeField] private ItemDefinition harvestItem;
    [SerializeField, Min(1)] private int daysToGrow = 3;
    [SerializeField, Min(1)] private int harvestAmount = 1;

    public ItemDefinition SeedItem => seedItem;
    public ItemDefinition HarvestItem => harvestItem;
    public int DaysToGrow => daysToGrow;
    public int HarvestAmount => harvestAmount;
}