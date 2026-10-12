using System.Collections.Generic;
using UnityEngine;

public class StorageContainer : MonoBehaviour
{
    [SerializeField] private string displayName = "Storage";
    [SerializeField, Min(1)] private int slotCount = 40;

    private readonly List<StorageSlot> slots = new List<StorageSlot>();

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? "Storage" : displayName;
    public int Capacity => Mathf.Max(1, slotCount);

    private void Awake()
    {
        InitializeSlots();
    }

    public StorageSlotData[] GetSnapshot()
    {
        InitializeSlots();
        StorageSlotData[] snapshot = new StorageSlotData[slots.Count];
        for (int i = 0; i < slots.Count; i++)
            snapshot[i] = new StorageSlotData(slots[i].Item, slots[i].Quantity);
        return snapshot;
    }

    public bool TryGetSlotContents(int index, out ItemDefinition item, out int quantity)
    {
        InitializeSlots();
        item = null;
        quantity = 0;
        if (index < 0 || index >= slots.Count) return false;
        item = slots[index].Item;
        quantity = slots[index].Quantity;
        return true;
    }

    public bool CanAcceptAtSlot(int index, ItemDefinition item, int quantity)
    {
        InitializeSlots();
        if (item == null || quantity <= 0 || index < 0 || index >= slots.Count)
            return false;

        StorageSlot slot = slots[index];
        int maxStack = Mathf.Max(1, item.MaxStackSize);
        if (slot.IsEmpty) return quantity <= maxStack;
        return slot.Item == item && slot.Quantity + quantity <= maxStack;
    }

    public bool TryAddToSlot(int index, ItemDefinition item, int quantity)
    {
        if (!CanAcceptAtSlot(index, item, quantity)) return false;
        StorageSlot slot = slots[index];
        if (slot.IsEmpty) slot.Set(item, quantity);
        else slot.Set(item, slot.Quantity + quantity);
        PublishChanged();
        return true;
    }

    public bool TryTakeFromSlot(int index, out ItemDefinition item, out int quantity)
    {
        InitializeSlots();
        item = null;
        quantity = 0;
        if (index < 0 || index >= slots.Count || slots[index].IsEmpty)
            return false;

        StorageSlot slot = slots[index];
        item = slot.Item;
        quantity = slot.Quantity;
        slot.Clear();
        PublishChanged();
        return true;
    }

    public bool SwapSlots(int firstIndex, int secondIndex)
    {
        InitializeSlots();
        if (firstIndex < 0 || firstIndex >= slots.Count ||
            secondIndex < 0 || secondIndex >= slots.Count ||
            firstIndex == secondIndex)
            return false;

        StorageSlot first = slots[firstIndex];
        StorageSlot second = slots[secondIndex];
        ItemDefinition firstItem = first.Item;
        int firstQuantity = first.Quantity;
        ItemDefinition secondItem = second.Item;
        int secondQuantity = second.Quantity;

        if (second.IsEmpty) first.Clear();
        else first.Set(secondItem, secondQuantity);
        if (firstItem == null || firstQuantity <= 0) second.Clear();
        else second.Set(firstItem, firstQuantity);

        PublishChanged();
        return true;
    }

    private void InitializeSlots()
    {
        int target = Mathf.Max(1, slotCount);
        while (slots.Count < target) slots.Add(new StorageSlot());
        while (slots.Count > target)
        {
            if (!slots[slots.Count - 1].IsEmpty)
            {
                Debug.LogWarning("Storage size was reduced but the last slot contained an item. Keeping the slot to prevent item loss.", this);
                break;
            }
            slots.RemoveAt(slots.Count - 1);
        }
    }

    private void PublishChanged()
    {
        GameEvents.Publish(new StorageChangedEvent(this, GetSnapshot()));
    }
}

public sealed class StorageSlot
{
    public ItemDefinition Item { get; private set; }
    public int Quantity { get; private set; }
    public bool IsEmpty => Item == null || Quantity <= 0;

    public void Set(ItemDefinition item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }

    public void Clear()
    {
        Item = null;
        Quantity = 0;
    }
}
