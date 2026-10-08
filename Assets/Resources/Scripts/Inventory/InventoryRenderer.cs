using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryRenderer : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private TMP_Text hotbarText;

    // В инспекторе закинь сюда ВСЕ возможные слоты UI (минимум 6 штук на будущее)
    [SerializeField] private GameObject[] hotBarObjects;

    private void Start()
    {
        // При старте принудительно обновляем UI, чтобы спрятать лишние слоты
        if (inventoryManager != null)
        {
            inventoryManager.UpdUI();
        }
    }

    public void DrawCSlot()
    {
        for (int i = 0; i < hotBarObjects.Length; i++)
        {
            // Если этот UI слот превышает текущий размер инвентаря — выключаем его отображение
            if (i >= inventoryManager.hotbar.Length)
            {
                hotBarObjects[i].SetActive(false);
                continue;
            }

            // Если слот доступен игроку — включаем его
            hotBarObjects[i].SetActive(true);

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

        // Проверка на случай, если текущий выбранный слот вдруг пустой или за границами массива
        if (inventoryManager.CSlot < inventoryManager.hotbar.Length && inventoryManager.hotbar[inventoryManager.CSlot].item != null)
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
        for (int i = 0; i < hotBarObjects.Length; i++)
        {
            // Защита: не рендерим UI слоты, если инвентарь игрока до них ещё не дорос
            if (i >= inventoryManager.hotbar.Length)
            {
                continue;
            }

            Image slotImg = hotBarObjects[i].transform.GetChild(0).GetComponent<Image>();
            TMP_Text quantityText = hotBarObjects[i].transform.GetChild(2).GetComponent<TMP_Text>();

            if (inventoryManager.hotbar[i].item != null)
            {
                slotImg.sprite = inventoryManager.hotbar[i].item.icon;
                quantityText.text = inventoryManager.hotbar[i].quantity.ToString();
            }
            else
            {
                slotImg.sprite = Resources.Load<Sprite>("Sprites/Emptiness");
                quantityText.text = "";
            }
        }
    }
}