using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryRenderer : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private TMP_Text hotbarText;

    [SerializeField] private GameObject[] hotBarObjects = new GameObject[4];

    public void DrawCSlot()
    {
        for (int i = 0; i < hotBarObjects.Length; i++)
        {
            RectTransform rt = hotBarObjects[i].GetComponent<RectTransform>();

            if (i == inventoryManager.CSlot)
            {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -419f);
            }
            else
            {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -458f);
            }
        }

        if (inventoryManager.hotbar[inventoryManager.CSlot].item != null)
        {
            hotbarText.text = inventoryManager.hotbar[inventoryManager.CSlot].item.Itemname;
        }
        else
        {
            hotbarText.text = "";
        }
    }

    public void DrawSlots()
    {
        int i = 0;
        foreach (var slot in hotBarObjects)
        {
            Image slotImg = slot.transform.GetChild(0).GetComponent<Image>();

            if (inventoryManager.hotbar[i].item != null)
            {
                slotImg.sprite = inventoryManager.hotbar[i].item.icon;
            }
            else
            {
                slotImg.sprite = Resources.Load<Sprite>("Sprites/Emptiness");
            }

            i++;
        }
    }

}
