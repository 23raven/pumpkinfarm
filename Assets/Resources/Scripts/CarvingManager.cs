using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CarvingManager : MonoBehaviour
{
    // Ссылки на другие скрипты
    [Header("Менеджеры")]
    public InventoryManager invManager; // Изменил на public для удобства кнопок

    // Объекты UI и контейнеры
    [Header("UI Контейнеры")]
    [SerializeField] private Transform PumpkinInvContent;
    [SerializeField] private Transform OrnamentsInvContent;
    [SerializeField] private GameObject SelectedItemGameObject;

    // Префабы элементов UI
    [Header("Prefabs")]
    [SerializeField] private GameObject PumpkinInvPrefab;
    [SerializeField] private GameObject OrnamentsInvPrefab;

    [Header("Данные")]
    public List<Ornament> allowed_ornaments = new();
    public InvItem SelectedItem; // Текущая выбранная ОДНА тыква

    // Вспомогательные переменные, чтобы знать, из какого слота хотбара мы взяли тыкву
    [HideInInspector] public int selectedSlotIndex = -1;

    public void DrawPumkinInventory()
    {
        ClearPumpkins();

        for (int i = 0; i < invManager.hotbar.Length; i++)
        {
            var slot = invManager.hotbar[i];

            if (slot.item != null && IsPumpkin(slot.item))
            {
                // Рисуем каждую тыкву из стака как отдельную кнопку
                for (int q = 0; q < slot.quantity; q++)
                {
                    GameObject instance = Instantiate(PumpkinInvPrefab, PumpkinInvContent);

                    // Настройка текста и картинки
                    instance.transform.GetChild(1).GetComponent<TMP_Text>().text = slot.item.Itemname;

                    // ИСПРАВЛЕНО: Возвращаем вывод названия орнамента вместо цифры "1"
                    instance.transform.GetChild(2).GetComponent<TMP_Text>().text = slot.item.ornament.ToString();

                    instance.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = slot.item.icon;

                    if (instance.TryGetComponent<PumpkinInvButtonScript>(out var buttonScript))
                    {
                        buttonScript.item = slot.item;
                        buttonScript.carvingManager = this;
                    }

                    // Передаем в листенер индекс слота, чтобы знать, откуда забирать тыкву
                    int slotIndex = i;
                    if (instance.TryGetComponent<Button>(out var button))
                    {
                        button.onClick.AddListener(() => OnPumpkinClick(slot.item, slotIndex));
                    }
                }
            }
        }
    }

    // Логика клика по тыкве в интерфейсе стола
    private void OnPumpkinClick(InvItem clickedItem, int slotIndex)
    {
        // Если кликнули по той же тыкве (отмена выбора)
        if (SelectedItem != null && selectedSlotIndex == slotIndex && SelectedItem.ornament == clickedItem.ornament)
        {
            SelectedItem = null;
            selectedSlotIndex = -1;
        }
        else
        {
            // Выбираем ОДНУ тыкву. Клонируем её сразу, чтобы это был отдельный уникальный предмет
            SelectedItem = Instantiate(clickedItem);
            selectedSlotIndex = slotIndex;
        }

        DrawSelected();
    }

    private bool IsPumpkin(InvItem item)
    {
        foreach (Tags tag in item.tags)
        {
            if (tag == Tags.Pumkin) return true;
        }
        return false;
    }

    private void ClearPumpkins()
    {
        for (int i = PumpkinInvContent.childCount - 1; i >= 0; i--)
        {
            Destroy(PumpkinInvContent.GetChild(i).gameObject);
        }
    }

    private void ClearOrnaments()
    {
        for (int i = OrnamentsInvContent.childCount - 1; i >= 0; i--)
        {
            Destroy(OrnamentsInvContent.GetChild(i).gameObject);
        }
    }

    public void DrawOrnaments()
    {
        ClearOrnaments();

        for (int i = 0; i < allowed_ornaments.Count; i++)
        {
            GameObject instance = Instantiate(OrnamentsInvPrefab, OrnamentsInvContent);
            instance.transform.GetChild(1).GetComponent<TMP_Text>().text = allowed_ornaments[i].ornament.ToString();
            instance.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = allowed_ornaments[i].icon;

            if (instance.TryGetComponent<OrnamentsPrefabButtonClick>(out var buttonScript))
            {
                buttonScript.Ornament = allowed_ornaments[i].ornament;
                buttonScript.carvingManager = this;
            }

            if (instance.TryGetComponent<Button>(out var button))
            {
                button.onClick.AddListener(buttonScript.ChangeOrnament);
            }
        }
    }

    public void DrawPrototype() { }

    public void DrawSelected()
    {
        if (SelectedItem != null)
        {
            SelectedItemGameObject.SetActive(true);
            SelectedItemGameObject.transform.GetChild(0).GetComponent<Image>().sprite = SelectedItem.icon;
            SelectedItemGameObject.transform.GetChild(1).GetComponent<TMP_Text>().text = SelectedItem.Itemname;
            SelectedItemGameObject.transform.GetChild(2).GetComponent<TMP_Text>().text = SelectedItem.ornament.ToString();
        }
        else
        {
            SelectedItemGameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        SelectedItem = null;
        selectedSlotIndex = -1;
        DrawOrnaments();
        DrawPrototype();
        DrawPumkinInventory();
        DrawSelected();
    }
}
[System.Serializable]
public class Ornament //класс чтобы я мог добавить иконки орнаментам
{
    public Sprite icon;
    public Ornaments ornament;
}
