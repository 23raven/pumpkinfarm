using UnityEngine;

[CreateAssetMenu(
    fileName = "NewItem",
    menuName = "Game/Items/Item Definition"
)]
public class ItemDefinition : ScriptableObject
{
    [Header("Basic Information")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;

    [Header("Visuals")]
    [SerializeField] private Sprite icon;
    [SerializeField] private PickupItem worldPrefab;

    [Header("Inventory")]
    [SerializeField, Min(1)] private int maxStackSize = 99;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public string Description => description;
    public int MaxStackSize => maxStackSize;
    public Sprite Icon => icon;
    public PickupItem WorldPrefab => worldPrefab;
}