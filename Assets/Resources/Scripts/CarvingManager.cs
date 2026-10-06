using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CarvingManager : MonoBehaviour
{
    [SerializeField] private InventoryManager invManager;

    [SerializeField] private Transform PumpkinInvContent;
    [SerializeField] private Transform OrnamentsInvContent;
    [SerializeField] private GameObject SelectedItemGameObject;

    [SerializeField] private GameObject PumpkinInvPrefab;
    [SerializeField] private GameObject OrnamentsInvPrefab;

    public List<Ornament> allowed_ornaments = new(); //это лист доступных орнаментов сделал чтобы реализовать покупку орнаментов в будущем
    public InvItem SelectedItem;

    public void DrawPumkinInventory()
    {
        ClearPumpkins();

        for (int i = 0; i < invManager.hotbar.Length; i++)
        {
            if (invManager.hotbar[i].item != null && IsPumpkin(invManager.hotbar[i].item))
            {
                GameObject instance = Instantiate(PumpkinInvPrefab, PumpkinInvContent);
                instance.transform.GetChild(1).GetComponent<TMP_Text>().text = invManager.hotbar[i].item.Itemname;
                instance.transform.GetChild(2).GetComponent<TMP_Text>().text = invManager.hotbar[i].item.ornament.ToString();
                instance.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = invManager.hotbar[i].item.icon;

                instance.GetComponent<PumpkinInvButtonScript>().item = invManager.hotbar[i].item;
                instance.GetComponent<PumpkinInvButtonScript>().carvingManager = this;

                instance.GetComponent<Button>().onClick.AddListener(instance.GetComponent<PumpkinInvButtonScript>().Select);
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

    private void ClearPumpkins()
    {
        for (int i = 0; i < PumpkinInvContent.childCount; i++)
        {
            Destroy(PumpkinInvContent.GetChild(0).gameObject);
        }
    }

    private void ClearOrnaments()
    {
        for (int i = 0; i < OrnamentsInvContent.childCount; i++)
        {
            Destroy(OrnamentsInvContent.GetChild(0).gameObject);
        }
    }

    public void DrawOrnaments() //отрисовать список орнаментов
    {
        ClearOrnaments();

        for (int i = 0; i < allowed_ornaments.Count; i++)
        {
            GameObject instance = Instantiate(OrnamentsInvPrefab, OrnamentsInvContent);
            instance.transform.GetChild(1).GetComponent<TMP_Text>().text = allowed_ornaments[i].ornament.ToString();
            instance.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = allowed_ornaments[i].icon;

            instance.GetComponent<OrnamentsPrefabButtonClick>().Ornament = allowed_ornaments[i].ornament;
            instance.GetComponent<OrnamentsPrefabButtonClick>().carvingManager = this;

            instance.GetComponent<Button>().onClick.AddListener(instance.GetComponent<OrnamentsPrefabButtonClick>().ChangeOrnament);
        }
    }

    public void DrawPrototype()
    {

    }

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
