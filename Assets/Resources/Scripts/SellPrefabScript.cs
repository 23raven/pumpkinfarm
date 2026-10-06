using UnityEngine;

public class SellPrefabScript : MonoBehaviour //скрипт для клика в магазине
{
    public InvItem item;
    public int quantity;
    public Sell sell;
    public GameManager gameManager;
    public InventoryManager inventoryManager;

    public void Sell()
    {
        gameManager.AddMoney(item.priceSell);
        inventoryManager.RemoveItem(item, 1);
        quantity--;
        sell.DrawSell();
    }
}
