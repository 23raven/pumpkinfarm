using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Sell : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private InventoryManager inventoryManager;

    [SerializeField] private Transform SellContent;
    [SerializeField] private Transform BuyContent;
    [SerializeField] private GameObject SellPrefab;
    [SerializeField] private GameObject BuyPrefab;

    public List<InvItem> ShopItems = new();

    public void DrawSell()
    {
        ClearChildrenSell();

        for (int i = 0; i < inventoryManager.hotbar.Length; i++)
        {
            if (inventoryManager.hotbar[i].item != null && inventoryManager.hotbar[i].item.selleable)
            {
                GameObject instance = Instantiate(SellPrefab, SellContent);
                instance.transform.GetChild(0).GetComponent<TMP_Text>().text = inventoryManager.hotbar[i].item.Itemname;
                instance.transform.GetChild(1).GetComponent<TMP_Text>().text = "price: " + inventoryManager.hotbar[i].item.priceSell.ToString();
                instance.transform.GetChild(2).GetComponent<Image>().sprite = inventoryManager.hotbar[i].item.icon;
                instance.transform.GetChild(3).GetComponent<TMP_Text>().text = "x" + inventoryManager.hotbar[i].quantity;

                instance.GetComponent<SellPrefabScript>().item = inventoryManager.hotbar[i].item;
                instance.GetComponent<SellPrefabScript>().quantity = inventoryManager.hotbar[i].quantity;
                instance.GetComponent<SellPrefabScript>().sell = this;
                instance.GetComponent<SellPrefabScript>().gameManager = gameManager;
                instance.GetComponent<SellPrefabScript>().inventoryManager = inventoryManager;

                instance.GetComponent<Button>().onClick.AddListener(instance.GetComponent<SellPrefabScript>().Sell);
            }
        }
    }

    public void DrawBuy()
    {
        ClearChildrenBuy();

        for (int i = 0; i < ShopItems.Count; i++)
        {
            GameObject instance = Instantiate(BuyPrefab, BuyContent);
            instance.transform.GetChild(0).GetComponent<TMP_Text>().text = ShopItems[i].name;
            instance.transform.GetChild(1).GetComponent<TMP_Text>().text = "price: " + ShopItems[i].priceBuy.ToString();
            instance.transform.GetChild(2).GetComponent<Image>().sprite = ShopItems[i].icon;

            instance.GetComponent<BuyPrefabScript>().item = ShopItems[i];
            instance.GetComponent<BuyPrefabScript>().sell = this;
            instance.GetComponent<BuyPrefabScript>().gameManager = gameManager;
            instance.GetComponent<BuyPrefabScript>().inventoryManager = inventoryManager;

            instance.GetComponent<Button>().onClick.AddListener(instance.GetComponent<BuyPrefabScript>().Buy);
        }
    }

    private void ClearChildrenSell()
    {
        for (int i = 0; i < SellContent.childCount; i++)
        {
            Destroy(SellContent.GetChild(i).gameObject);
        }
    }

    private void ClearChildrenBuy()
    {
        for (int i = 0; i < BuyContent.childCount; i++)
        {
            Destroy(BuyContent.GetChild(i).gameObject);
        }
    }

    public void SellItem()
    {

    }
}
