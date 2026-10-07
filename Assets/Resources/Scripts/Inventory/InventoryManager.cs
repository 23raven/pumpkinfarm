using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public InvSlot[] hotbar = new InvSlot[4];
    public int CSlot;

    [SerializeField] private InventoryRenderer inventoryRenderer;

    public void AddItem(InvItem item, int quantity)
    {
        for (int i = 0; i < 4; i++)
        {
            if (hotbar[i].item != null && hotbar[i].item.Itemname == item.Itemname && hotbar[i].item.ornament == item.ornament)
            {
                hotbar[i].quantity += quantity;
                UpdUI();
                return;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (hotbar[i].item == null)
            {
                hotbar[i] = new InvSlot { item = item, quantity = quantity };
                UpdUI();
                return;
            }
        }
    }

    public void RemoveItem(InvItem item, int quantity)
    {
        for (int i = 0; i < 4; i++)
        {
            if (hotbar[i].item != null && hotbar[i].item.Itemname == item.Itemname && hotbar[i].item.ornament == item.ornament)
            {
                hotbar[i].quantity -= quantity;

                if (hotbar[i].quantity <= 0)
                {
                    hotbar[i].item = null;
                    hotbar[i].quantity = 0;
                }

                UpdUI();
                return;
            }
        }
    }

    public bool HasItem(InvItem item, int requiredQuantity)
    {
        if (item == null) return false;

        int currentCount = GetItemCount(item);

        return currentCount >= requiredQuantity;
    }

    public int GetItemCount(InvItem item)
    {
        if (item == null) return 0;

        int totalCount = 0;

        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i].item != null && hotbar[i].item.Itemname == item.Itemname && hotbar[i].item.ornament == item.ornament)
            {
                totalCount += hotbar[i].quantity;
            }
        }

        return totalCount;
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

    public void UpdUI()
    {
        inventoryRenderer.DrawCSlot();
        inventoryRenderer.DrawSlots();
    }
}

[System.Serializable]
public class InvSlot
{
    public InvItem item;
    public int quantity;
}