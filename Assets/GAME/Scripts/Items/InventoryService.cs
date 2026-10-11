using System.Collections.Generic;
using UnityEngine;

public class InventoryService : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField, Min(1)]
    private int slotCount = 2;

    private readonly List<InventorySlot> slots =
        new List<InventorySlot>();

    private readonly Dictionary<string, int> capacityBonuses =
        new Dictionary<string, int>();

    public int Capacity
    {
        get
        {
            InitializeSlots();
            return slots.Count;
        }
    }

    public IReadOnlyList<InventorySlot> Slots
    {
        get
        {
            InitializeSlots();
            return slots;
        }
    }

    public int SelectedSlotIndex { get; private set; }

    public ItemDefinition SelectedItem
    {
        get
        {
            InitializeSlots();

            if (SelectedSlotIndex < 0 ||
                SelectedSlotIndex >= slots.Count)
            {
                return null;
            }

            return slots[SelectedSlotIndex].Item;
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

    private int CalculateCapacity()
    {
        int capacity = Mathf.Max(1, slotCount);

        foreach (int bonus in capacityBonuses.Values)
            capacity += Mathf.Max(0, bonus);

        return capacity;
    }

    private void InitializeSlots()
    {
        int targetCapacity = CalculateCapacity();

        // Never discard occupied slots when capacity changes.
        for (int i = targetCapacity; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
                targetCapacity = i + 1;
        }

        while (slots.Count < targetCapacity)
            slots.Add(new InventorySlot());

        while (slots.Count > targetCapacity)
            slots.RemoveAt(slots.Count - 1);

        if (SelectedSlotIndex >= slots.Count)
            SelectedSlotIndex = slots.Count - 1;
    }

    public bool SetCapacityBonus(string sourceId, int extraSlots)
    {
        if (string.IsNullOrWhiteSpace(sourceId) || extraSlots < 0)
            return false;

        InitializeSlots();

        bool hadPrevious =
            capacityBonuses.TryGetValue(sourceId, out int previousBonus);

        if (extraSlots == 0)
            capacityBonuses.Remove(sourceId);
        else
            capacityBonuses[sourceId] = extraSlots;

        int targetCapacity = CalculateCapacity();

        // Refuse to shrink if occupied slots would be lost.
        for (int i = targetCapacity; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
            {
                if (hadPrevious)
                    capacityBonuses[sourceId] = previousBonus;
                else
                    capacityBonuses.Remove(sourceId);

                Debug.LogWarning(
                    "Cannot reduce inventory capacity while higher slots contain items.",
                    this
                );

                return false;
            }
        }

        InitializeSlots();
        PublishChanged();

        return true;
    }

    public bool SelectSlot(int index)
    {
        InitializeSlots();

        if (index < 0 || index >= slots.Count)
            return false;

        if (SelectedSlotIndex == index)
            return true;

        SelectedSlotIndex = index;
        PublishChanged();

        return true;
    }

    public bool TryRemoveSelectedSlot(
        out ItemDefinition item,
        out int quantity)
    {
        InitializeSlots();

        item = null;
        quantity = 0;

        if (SelectedSlotIndex < 0 ||
            SelectedSlotIndex >= slots.Count)
        {
            return false;
        }

        InventorySlot slot = slots[SelectedSlotIndex];

        if (slot.IsEmpty)
            return false;

        item = slot.Item;
        quantity = slot.Quantity;

        slot.Clear();

        PublishChanged();

        return true;
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

    public int GetAvailableSpace(ItemDefinition item)
    {
        if (item == null)
            return 0;

        InitializeSlots();

        int maxStack = Mathf.Max(1, item.MaxStackSize);
        int available = 0;

        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty)
                available += maxStack;
            else if (slot.Item == item)
                available += Mathf.Max(0, maxStack - slot.Quantity);
        }

        return available;
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
        InitializeSlots();

        InventorySlotData[] snapshot =
            new InventorySlotData[slots.Count];

        for (int i = 0; i < slots.Count; i++)
        {
            snapshot[i] = new InventorySlotData(
                slots[i].Item,
                slots[i].Quantity
            );
        }

        GameEvents.Publish(
            new InventoryChangedEvent(snapshot, SelectedSlotIndex)
        );
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