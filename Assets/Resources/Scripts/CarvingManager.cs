using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CarvingManager : MonoBehaviour
{
    [SerializeField] private InventoryManager invManager;

    [SerializeField] private Transform PumpkinInvContent;
    [SerializeField] private Transform OrnamentsInvContent;

    [SerializeField] private GameObject PumpkinInvPrefab;
    [SerializeField] private GameObject OrnamentsInvPrefab;

    public List<Ornament> allowed_ornaments = new(); //это лист доступных орнаментов сделал чтобы реализовать покупку орнаментов в будущем

    public void DrawPumkinInventory()
    {
        for (int i = 0; i < invManager.hotbar.Length; i++)
        {
            if (invManager.hotbar[i].item != null && IsPumpkin(invManager.hotbar[i].item))
            {
                GameObject instance = Instantiate(PumpkinInvPrefab, PumpkinInvContent);
                instance.transform.GetChild(1).GetComponent<TMP_Text>().text = invManager.hotbar[i].item.Itemname;
                instance.transform.GetChild(2).GetComponent<TMP_Text>().text = invManager.hotbar[i].item.ornament.ToString();
                instance.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = invManager.hotbar[i].item.icon;
            }
        }
    }

    private bool IsPumpkin(InvItem item) //вспомогательна€ функци€ чтобы проверить €вл€етс€ прдемет тыквой
    {
        foreach (Tags tag in item.tags)
        {
            if (tag == Tags.Pumkin)
            {
                return true;
            }
        }

        return false;
    }

    public void DrawOrnaments() //отрисовать список орнаментов
    {
        for (int i = 0; i < allowed_ornaments.Count; i++)
        {
            GameObject instance = Instantiate(OrnamentsInvPrefab, OrnamentsInvContent);
            instance.transform.GetChild(1).GetComponent<TMP_Text>().text = allowed_ornaments[i].ornament.ToString();
            instance.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = allowed_ornaments[i].icon;
        }
    }

    public void DrawPrototype()
    {

    }

    private void OnEnable() //отрисовываем при включение обьекта
    {
        DrawOrnaments();
        DrawPrototype();
        DrawPumkinInventory();
    }
}

[System.Serializable]
public class Ornament //класс чтобы € мог добавить иконки орнаментам
{
    public Sprite icon;
    public Ornaments ornament;
}
