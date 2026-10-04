using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Sell : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private InventoryManager inventoryManager;

    [SerializeField] private Transform SellContent;
    [SerializeField] private Transform BuyContent;
    [SerializeField] private GameObject SellPrefab;

    public void DrawSell()
    {
        ClearChildren();

        for (int i = 0; i < inventoryManager.hotbar.Length; i++)
        {
            if (inventoryManager.hotbar[i].item.selleable)
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

    private void ClearChildren()
    {
        for (int i = 0; i < SellContent.childCount; i++)
        {
            Destroy(SellContent.GetChild(i).gameObject);
        }
    }

    public void DrawBuy()
    {

    }

    public void SellItem()
    {

    }
}
