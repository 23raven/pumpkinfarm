using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public List<InvSlot> inventory = new();
    public int CSlot;

    [SerializeField] private InventoryRenderer inventoryRenderer;

    public void AddItem()
    {

    }

    public void RemoveItem()
    {

    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            CSlot = 0;
            UpdUI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CSlot = 1;
            UpdUI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CSlot = 2;
            UpdUI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CSlot = 3;
            UpdUI();
        }
    }

    private void UpdUI()
    {
        inventoryRenderer.DrawCSlot();
    }
}

[System.Serializable]
public class InvSlot
{
    public InvItem item;
    public int quantity;
}
