using System.Collections.Generic;
using UnityEngine;

public class InventoryService : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField, Min(1)] private int slotCount = 2;

    private readonly List<InventorySlot> slots = new List<InventorySlot>();
    private readonly Dictionary<string, CapacityBonus> capacityBonuses =
        new Dictionary<string, CapacityBonus>();

    private bool ownsCart;
    private bool cartHitched;

    private sealed class CapacityBonus
    {
        public int SlotCount;
        public InventorySlotGroup Group;

        public CapacityBonus(int slotCount, InventorySlotGroup group)
        {
            SlotCount = slotCount;
            Group = group;
        }
    }

    public int Capacity
    {
        get
        {
            InitializeSlots();
            return slots.Count;
        }
    }

    public bool CartIsAccessible => ownsCart && cartHitched;

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

            if (SelectedSlotIndex < 0 || SelectedSlotIndex >= slots.Count)
                return null;

            InventorySlot slot = slots[SelectedSlotIndex];
            return IsAccessible(slot) ? slot.Item : null;
        }
    }

    private void Awake()
    {
        InitializeSlots();
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<CartStateChangedEvent>(OnCartStateChanged);

        if (GameEvents.TryGetLatest<CartStateChangedEvent>(out var cartState))
        {
            ownsCart = cartState.HasCart;
            cartHitched = cartState.IsHitched;
        }

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
        GameEvents.Unsubscribe<CartStateChangedEvent>(OnCartStateChanged);

        GameServices services = GameServices.Instance;
        if (services != null)
            services.Unregister(this);
    }

    private void OnCartStateChanged(CartStateChangedEvent message)
    {
        ownsCart = message.HasCart;
        cartHitched = message.IsHitched;

        if (!CartIsAccessible &&
            SelectedSlotIndex >= 0 &&
            SelectedSlotIndex < slots.Count &&
            slots[SelectedSlotIndex].Group == InventorySlotGroup.Cart)
        {
            SelectFirstAccessibleSlot();
        }

        PublishChanged();
    }

    public bool SetCapacityBonus(string sourceId, int extraSlots)
    {
        InventorySlotGroup group = !string.IsNullOrEmpty(sourceId) &&
            sourceId.ToLowerInvariant().Contains("cart")
                ? InventorySlotGroup.Cart
                : InventorySlotGroup.Backpack;

        return SetCapacityBonus(sourceId, extraSlots, group);
    }

    public bool SetCapacityBonus(
        string sourceId,
        int extraSlots,
        InventorySlotGroup group)
    {
        if (string.IsNullOrWhiteSpace(sourceId) || extraSlots < 0)
            return false;

        if (group == InventorySlotGroup.Base)
        {
            Debug.LogWarning("Capacity bonuses must be Backpack or Cart slots.", this);
            return false;
        }

        InitializeSlots();

        bool hadPrevious = capacityBonuses.TryGetValue(sourceId, out CapacityBonus previous);

        if (extraSlots == 0)
            capacityBonuses.Remove(sourceId);
        else
            capacityBonuses[sourceId] = new CapacityBonus(extraSlots, group);

        if (!TryRebuildSlots())
        {
            if (hadPrevious)
                capacityBonuses[sourceId] = previous;
            else
                capacityBonuses.Remove(sourceId);

            TryRebuildSlots();
            Debug.LogWarning(
                "Cannot reduce inventory capacity while removed slots contain items.",
                this
            );
            return false;
        }

        PublishChanged();
        return true;
    }

    public bool SelectSlot(int index)
    {
        InitializeSlots();

        if (index < 0 || index >= slots.Count || !IsAccessible(slots[index]))
            return false;

        if (SelectedSlotIndex == index)
            return true;

        SelectedSlotIndex = index;
        PublishChanged();
        return true;
    }

    public bool SwapSlots(int firstIndex, int secondIndex)
    {
        InitializeSlots();

        if (firstIndex < 0 || firstIndex >= slots.Count ||
            secondIndex < 0 || secondIndex >= slots.Count ||
            firstIndex == secondIndex)
        {
            return false;
        }

        InventorySlot first = slots[firstIndex];
        InventorySlot second = slots[secondIndex];

        // Parked cart slots keep their contents but cannot be manipulated.
        if (!IsAccessible(first) || !IsAccessible(second))
            return false;

        ItemDefinition firstItem = first.Item;
        int firstQuantity = first.Quantity;
        ItemDefinition secondItem = second.Item;
        int secondQuantity = second.Quantity;

        if (secondItem == null || secondQuantity <= 0)
            first.Clear();
        else
            first.Set(secondItem, secondQuantity);

        if (firstItem == null || firstQuantity <= 0)
            second.Clear();
        else
            second.Set(firstItem, firstQuantity);

        PublishChanged();
        return true;
    }

    public bool SelectNextAccessibleSlot(int direction)
    {
        InitializeSlots();

        if (slots.Count == 0 || direction == 0)
            return false;

        int step = direction > 0 ? 1 : -1;
        int start = Mathf.Clamp(SelectedSlotIndex, 0, slots.Count - 1);

        for (int offset = 1; offset <= slots.Count; offset++)
        {
            int index = (start + step * offset) % slots.Count;
            if (index < 0)
                index += slots.Count;

            if (IsAccessible(slots[index]))
                return SelectSlot(index);
        }

        return false;
    }

    public bool TryRemoveSelectedSlot(out ItemDefinition item, out int quantity)
    {
        InitializeSlots();
        item = null;
        quantity = 0;

        if (SelectedSlotIndex < 0 || SelectedSlotIndex >= slots.Count)
            return false;

        InventorySlot slot = slots[SelectedSlotIndex];
        if (!IsAccessible(slot) || slot.IsEmpty)
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

        foreach (InventorySlot slot in slots)
        {
            if (!IsAccessible(slot) || slot.IsEmpty || slot.Item != item)
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

        foreach (InventorySlot slot in slots)
        {
            if (remaining == 0)
                break;
            if (!IsAccessible(slot) || !slot.IsEmpty)
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
            if (!IsAccessible(slot))
                continue;

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
            if (IsAccessible(slot) && slot.Item == item)
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
        if (item == null || amount <= 0 || !HasItem(item, amount))
            return false;

        int remaining = amount;

        foreach (InventorySlot slot in slots)
        {
            if (!IsAccessible(slot) || slot.Item != item)
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

    private bool IsAccessible(InventorySlot slot)
    {
        return slot.Group != InventorySlotGroup.Cart || CartIsAccessible;
    }

    private void SelectFirstAccessibleSlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (IsAccessible(slots[i]))
            {
                SelectedSlotIndex = i;
                return;
            }
        }

        SelectedSlotIndex = 0;
    }

    private void InitializeSlots()
    {
        if (IsLayoutCurrent())
            return;

        if (!TryRebuildSlots())
            Debug.LogWarning("Inventory layout could not be rebuilt without losing items.", this);
    }

    private bool IsLayoutCurrent()
    {
        int baseCount = Mathf.Max(1, slotCount);
        int backpackCount = 0;
        int cartCount = 0;

        foreach (CapacityBonus bonus in capacityBonuses.Values)
        {
            if (bonus.Group == InventorySlotGroup.Backpack)
                backpackCount += bonus.SlotCount;
            else if (bonus.Group == InventorySlotGroup.Cart)
                cartCount += bonus.SlotCount;
        }

        if (slots.Count != baseCount + backpackCount + cartCount)
            return false;

        int index = 0;
        for (int i = 0; i < baseCount; i++, index++)
            if (slots[index].Group != InventorySlotGroup.Base) return false;
        for (int i = 0; i < backpackCount; i++, index++)
            if (slots[index].Group != InventorySlotGroup.Backpack) return false;
        for (int i = 0; i < cartCount; i++, index++)
            if (slots[index].Group != InventorySlotGroup.Cart) return false;

        return true;
    }

    private bool TryRebuildSlots()
    {
        var targetCounts = new Dictionary<InventorySlotGroup, int>
        {
            { InventorySlotGroup.Base, Mathf.Max(1, slotCount) },
            { InventorySlotGroup.Backpack, 0 },
            { InventorySlotGroup.Cart, 0 }
        };

        foreach (CapacityBonus bonus in capacityBonuses.Values)
            targetCounts[bonus.Group] += bonus.SlotCount;

        var existing = new Dictionary<InventorySlotGroup, List<InventorySlot>>
        {
            { InventorySlotGroup.Base, new List<InventorySlot>() },
            { InventorySlotGroup.Backpack, new List<InventorySlot>() },
            { InventorySlotGroup.Cart, new List<InventorySlot>() }
        };

        foreach (InventorySlot slot in slots)
            existing[slot.Group].Add(slot);

        foreach (InventorySlotGroup group in new[]
                 { InventorySlotGroup.Base, InventorySlotGroup.Backpack, InventorySlotGroup.Cart })
        {
            List<InventorySlot> groupSlots = existing[group];
            int target = targetCounts[group];

            for (int i = target; i < groupSlots.Count; i++)
            {
                if (!groupSlots[i].IsEmpty)
                    return false;
            }
        }

        InventorySlot selectedSlot = SelectedSlotIndex >= 0 && SelectedSlotIndex < slots.Count
            ? slots[SelectedSlotIndex]
            : null;

        var rebuilt = new List<InventorySlot>();
        InventorySlotGroup[] order =
        {
            InventorySlotGroup.Base,
            InventorySlotGroup.Backpack,
            InventorySlotGroup.Cart
        };

        foreach (InventorySlotGroup group in order)
        {
            List<InventorySlot> groupSlots = existing[group];
            int target = targetCounts[group];

            for (int i = 0; i < target; i++)
            {
                rebuilt.Add(i < groupSlots.Count
                    ? groupSlots[i]
                    : new InventorySlot(group));
            }
        }

        slots.Clear();
        slots.AddRange(rebuilt);

        if (selectedSlot != null)
        {
            int selectedIndex = slots.IndexOf(selectedSlot);
            if (selectedIndex >= 0)
                SelectedSlotIndex = selectedIndex;
        }

        if (slots.Count == 0)
            SelectedSlotIndex = 0;
        else
            SelectedSlotIndex = Mathf.Clamp(SelectedSlotIndex, 0, slots.Count - 1);

        if (SelectedSlotIndex < slots.Count && !IsAccessible(slots[SelectedSlotIndex]))
            SelectFirstAccessibleSlot();

        return true;
    }

    private void PublishChanged()
    {
        InitializeSlots();

        InventorySlotData[] snapshot = new InventorySlotData[slots.Count];
        InventorySlotDisplayData[] displaySnapshot =
            new InventorySlotDisplayData[slots.Count];

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];
            snapshot[i] = new InventorySlotData(slot.Item, slot.Quantity);
            displaySnapshot[i] = new InventorySlotDisplayData(
                slot.Item,
                slot.Quantity,
                slot.Group,
                IsAccessible(slot)
            );
        }

        GameEvents.Publish(new InventoryChangedEvent(snapshot, SelectedSlotIndex));
        GameEvents.Publish(new InventoryLayoutChangedEvent(displaySnapshot, SelectedSlotIndex));
    }
}

public sealed class InventorySlot
{
    public ItemDefinition Item { get; private set; }
    public int Quantity { get; private set; }
    public InventorySlotGroup Group { get; }
    public bool IsEmpty => Item == null || Quantity <= 0;

    public InventorySlot(InventorySlotGroup group = InventorySlotGroup.Base)
    {
        Group = group;
    }

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
