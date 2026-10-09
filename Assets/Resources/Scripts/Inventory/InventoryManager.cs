using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public InvSlot[] hotbar = new InvSlot[2];
    public int CSlot;

    [SerializeField] private InvItem BackpackItem;
    [SerializeField] private InventoryRenderer inventoryRenderer;

    [SerializeField] private Transform playerTransform;

    private int backpackUseCount = 0;

    private void Awake()
    {
        for (int i = 0; i < hotbar.Length; i++)
        {
            hotbar[i] = new InvSlot();
        }

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                playerTransform = transform;
            }
        }
    }

    public bool AddItem(InvItem item, int quantity)
    {
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i] != null && hotbar[i].item != null && hotbar[i].item.Itemname == item.Itemname && hotbar[i].item.ornament == item.ornament)
            {
                hotbar[i].quantity += quantity;
                UpdUI();
                return true;
            }
        }

        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i] == null || hotbar[i].item == null)
            {
                hotbar[i] = new InvSlot { item = item, quantity = quantity };
                UpdUI();
                return true;
            }
        }

        DropCurrentItem();

        hotbar[CSlot] = new InvSlot { item = item, quantity = quantity };
        UpdUI();
        return true;
    }

    private void DropCurrentItem()
    {
        if (CSlot >= hotbar.Length || hotbar[CSlot] == null || hotbar[CSlot].item == null)
        {
            return;
        }

        InvItem itemToDrop = hotbar[CSlot].item;
        int quantityToDrop = hotbar[CSlot].quantity;

        if (itemToDrop.Drop != null)
        {
            Vector3 dropPosition = playerTransform.position;

            GameObject droppedObj = Instantiate(itemToDrop.Drop, dropPosition, Quaternion.identity);

            if (droppedObj.TryGetComponent<PickUpItem>(out PickUpItem pickUp))
            {
                pickUp.item = itemToDrop;
                pickUp.quantity = quantityToDrop;
            }

            Debug.Log($"Инвентарь полон! Выброшен у ног игрока: {itemToDrop.Itemname} x{quantityToDrop}");
        }
        else
        {
            Debug.LogWarning($"У предмета {itemToDrop.Itemname} не назначен префаб 'Drop'!");
        }

        hotbar[CSlot].item = null;
        hotbar[CSlot].quantity = 0;
    }

    public void RemoveItem(InvItem item, int quantity)
    {
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i] != null && hotbar[i].item != null && hotbar[i].item.Itemname == item.Itemname && hotbar[i].item.ornament == item.ornament)
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
        return GetItemCount(item) >= requiredQuantity;
    }

    public int GetItemCount(InvItem item)
    {
        if (item == null) return 0;
        int totalCount = 0;
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i] != null && hotbar[i].item != null && hotbar[i].item.Itemname == item.Itemname && hotbar[i].item.ornament == item.ornament)
            {
                totalCount += hotbar[i].quantity;
            }
        }
        return totalCount;
    }

    public void Update()
    {
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                CSlot = i;
                UpdUI();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CSlot < hotbar.Length && hotbar[CSlot] != null && hotbar[CSlot].item != null && hotbar[CSlot].item.Itemname == BackpackItem.Itemname)
            {
                if (backpackUseCount < 2)
                {
                    ResizeInventory(hotbar.Length + 2);
                    backpackUseCount++;
                    RemoveItem(BackpackItem, 1);
                }
                else
                {
                    Debug.Log("Максимальный уровень расширения достигнут!");
                }
            }
        }
    }

    private void ResizeInventory(int newSize)
    {
        InvSlot[] newHotbar = new InvSlot[newSize];
        for (int i = 0; i < hotbar.Length; i++)
        {
            newHotbar[i] = hotbar[i];
        }
        for (int i = hotbar.Length; i < newSize; i++)
        {
            newHotbar[i] = new InvSlot();
        }
        hotbar = newHotbar;
        UpdUI();
    }

    public void UpdUI()
    {
        if (inventoryRenderer != null)
        {
            inventoryRenderer.DrawCSlot();
            inventoryRenderer.DrawSlots();
        }
    }
}

[System.Serializable]
public class InvSlot
{
    public InvItem item;
    public int quantity;
}