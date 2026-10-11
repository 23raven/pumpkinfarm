using UnityEngine;

[CreateAssetMenu(
    fileName = "NewShopCatalog",
    menuName = "Game/Shop/Catalog"
)]
public class ShopCatalog : ScriptableObject
{
    [SerializeField] private string displayName = "General Store";
    [SerializeField] private ShopProductDefinition[] products;

    public string DisplayName => displayName;
    public ShopProductDefinition[] Products => products;
}