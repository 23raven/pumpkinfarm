using UnityEngine;

public class InventoryRenderer : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject[] hotBarObjects = new GameObject[4];

    public void DrawCSlot()
    {
        hotBarObjects[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
    }

}
