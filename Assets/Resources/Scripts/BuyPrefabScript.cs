using UnityEngine;

public class BuyPrefabScript : MonoBehaviour
{
    public InvItem item;
    public Sell sell;
    public GameManager gameManager;
    public InventoryManager inventoryManager;

    public void Buy()
    {
        if (gameManager.money >= item.priceBuy)
        {
            gameManager.RemoveMoney(item.priceBuy);
            inventoryManager.AddItem(item, 1);
            sell.DrawBuy();
            sell.DrawSell();
        }
    }
}
