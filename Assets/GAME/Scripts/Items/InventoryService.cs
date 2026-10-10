using System.Collections.Generic;
using UnityEngine;

public class InventoryService : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int slotCount = 2;

    private readonly List<InventorySlot> slots =
        new List<InventorySlot>();

    public IReadOnlyList<InventorySlot> Slots
    {
        get
        {
            InitializeSlots();
            return slots;
        }
    }

    private void Awake()
    {
        InitializeSlots();
    }

    private void OnEnable()
    {
        GameServices services = GameServices.Instance;

        if (services == null)
        {
            Debug.LogError("GameServices is missing.", this);
            return;
        }

        if (services.Register(this))
            PublishChanged();
    }

    private void OnDisable()
    {
        GameServices services = GameServices.Instance;

        if (services != null)
            services.Unregister(this);
    }

    private void InitializeSlots()
    {
        if (slots.Count > 0)
            return;

        for (int i = 0; i < Mathf.Max(1, slotCount); i++)
            slots.Add(new InventorySlot());
    }

    public int AddItem(ItemDefinition item, int amount)
    {
        if (amount <= 0)
            return 0;

        if (item == null)
            return amount;

        InitializeSlots();

        int remaining = amount;
        int maxStack = Mathf.Max(1, item.MaxStackSize);

        // Fill existing stacks first.
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty || slot.Item != item)
                continue;

            int available = maxStack - slot.Quantity;

            if (available <= 0)
                continue;

            int added = Mathf.Min(available, remaining);

            slot.Set(item, slot.Quantity + added);
            remaining -= added;

            if (remaining == 0)
                break;
        }

        // Then fill empty slots.
        foreach (InventorySlot slot in slots)
        {
            if (remaining == 0)
                break;

            if (!slot.IsEmpty)
                continue;

            int added = Mathf.Min(maxStack, remaining);

            slot.Set(item, added);
            remaining -= added;
        }

        if (remaining < amount)
            PublishChanged();

        return remaining;
    }

    public int GetItemCount(ItemDefinition item)
    {
        if (item == null)
            return 0;

        InitializeSlots();

        int total = 0;

        foreach (InventorySlot slot in slots)
        {
            if (slot.Item == item)
                total += slot.Quantity;
        }

        return total;
    }

    public bool HasItem(ItemDefinition item, int amount)
    {
        return amount > 0 && GetItemCount(item) >= amount;
    }

    public bool RemoveItem(ItemDefinition item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        if (!HasItem(item, amount))
            return false;

        int remaining = amount;

        foreach (InventorySlot slot in slots)
        {
            if (slot.Item != item)
                continue;

            int removed = Mathf.Min(slot.Quantity, remaining);
            int newQuantity = slot.Quantity - removed;

            remaining -= removed;

            if (newQuantity == 0)
                slot.Clear();
            else
                slot.Set(item, newQuantity);

            if (remaining == 0)
                break;
        }

        PublishChanged();
        return true;
    }

    private void PublishChanged()
    {
        InventorySlotData[] snapshot =
            new InventorySlotData[slots.Count];

        for (int i = 0; i < slots.Count; i++)
        {
            snapshot[i] = new InventorySlotData(
                slots[i].Item,
                slots[i].Quantity
            );
        }

        GameEvents.Publish(new InventoryChangedEvent(snapshot));
    }
}

public sealed class InventorySlot
{
    public ItemDefinition Item { get; private set; }
    public int Quantity { get; private set; }

    public bool IsEmpty => Item == null || Quantity <= 0;

    internal void Set(ItemDefinition item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }

    internal void Clear()
    {
        Item = null;
        Quantity = 0;
    }
}