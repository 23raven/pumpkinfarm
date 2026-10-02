using UnityEngine;

public class InventoryRenderer : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
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
    }

}
