using UnityEngine;

public enum ShopProductType
{
    Item,
    BackpackUpgrade,
    CartUpgrade
}

[CreateAssetMenu(
    fileName = "NewShopProduct",
    menuName = "Game/Shop/Product"
)]
public class ShopProductDefinition : ScriptableObject
{
    [Header("Product")]
    [SerializeField] private string productId;
    [SerializeField] private string displayName;
    [SerializeField] private ShopProductType productType;
    [SerializeField] private ItemDefinition item;

    [Header("Prices")]
    [SerializeField, Min(0)] private int buyPrice = 10;
    [SerializeField, Min(0)] private int sellPrice = 5;

    [Header("Upgrade")]
    [SerializeField, Min(0)] private int capacityBonus;

    [Header("Availability")]
    [SerializeField] private bool canBuy = true;
    [SerializeField] private bool canSell = true;

    public string ProductId => productId;
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : item != null ? item.DisplayName : name;

    public ShopProductType ProductType => productType;
    public ItemDefinition Item => item;

    public int BuyPrice => buyPrice;
    public int SellPrice => sellPrice;
    public int CapacityBonus => capacityBonus;

    public bool CanBuy =>
        canBuy && (productType != ShopProductType.Item || item != null);

    public bool CanSell =>
        canSell &&
        productType == ShopProductType.Item &&
        item != null;
}
