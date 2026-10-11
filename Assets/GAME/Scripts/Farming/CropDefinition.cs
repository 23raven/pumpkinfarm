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
    [SerializeField, Min(1)] private int harvestAmount = 1;

    [Header("Growth Stages")]
    [Tooltip("Sprites ordered from newly planted to fully grown.")]
    [SerializeField] private Sprite[] growthSprites;

    public ItemDefinition SeedItem => seedItem;
    public ItemDefinition HarvestItem => harvestItem;
    public int HarvestAmount => harvestAmount;

    public int GrowthStageCount =>
        growthSprites == null ? 0 : growthSprites.Length;

    public Sprite GetGrowthSprite(int stage)
    {
        if (growthSprites == null ||
            stage < 0 ||
            stage >= growthSprites.Length)
        {
            return null;
        }

        return growthSprites[stage];
    }

    public bool HasValidGrowthSprites
    {
        get
        {
            if (growthSprites == null || growthSprites.Length < 2)
                return false;

            foreach (Sprite sprite in growthSprites)
            {
                if (sprite == null)
                    return false;
            }

            return true;
        }
    }
}