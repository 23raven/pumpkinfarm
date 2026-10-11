using UnityEngine;

[CreateAssetMenu(
    fileName = "NewShopProduct",
    menuName = "Game/Shop/Product"
)]
public class ShopProductDefinition : ScriptableObject
{
    [Header("Product")]
    [SerializeField] private string productId;
    [SerializeField] private ItemDefinition item;

    [Header("Prices")]
    [SerializeField, Min(0)] private int buyPrice = 10;
    [SerializeField, Min(0)] private int sellPrice = 5;

    [Header("Availability")]
    [SerializeField] private bool canBuy = true;
    [SerializeField] private bool canSell = true;

    public string ProductId => productId;
    public ItemDefinition Item => item;

    public int BuyPrice => buyPrice;
    public int SellPrice => sellPrice;

    public bool CanBuy => canBuy && item != null;
    public bool CanSell => canSell && item != null;
}