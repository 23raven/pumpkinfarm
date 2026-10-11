public readonly struct InventorySlotDisplayData
{
    public ItemDefinition Item { get; }
    public int Quantity { get; }
    public InventorySlotGroup Group { get; }
    public bool IsAccessible { get; }

    public bool IsEmpty => Item == null || Quantity <= 0;

    public InventorySlotDisplayData(
        ItemDefinition item,
        int quantity,
        InventorySlotGroup group,
        bool isAccessible)
    {
        Item = item;
        Quantity = quantity;
        Group = group;
        IsAccessible = isAccessible;
    }
}

public readonly struct InventoryLayoutChangedEvent
{
    public InventorySlotDisplayData[] Slots { get; }
    public int SelectedSlotIndex { get; }

    public InventoryLayoutChangedEvent(
        InventorySlotDisplayData[] slots,
        int selectedSlotIndex)
    {
        Slots = slots;
        SelectedSlotIndex = selectedSlotIndex;
    }
}
