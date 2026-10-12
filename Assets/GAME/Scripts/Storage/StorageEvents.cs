public readonly struct StorageOpenedEvent
{
    public StorageContainer Container { get; }

    public StorageOpenedEvent(StorageContainer container)
    {
        Container = container;
    }
}

public readonly struct StorageSlotData
{
    public ItemDefinition Item { get; }
    public int Quantity { get; }
    public bool IsEmpty => Item == null || Quantity <= 0;

    public StorageSlotData(ItemDefinition item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }
}

public readonly struct StorageChangedEvent
{
    public StorageContainer Container { get; }
    public StorageSlotData[] Slots { get; }

    public StorageChangedEvent(StorageContainer container, StorageSlotData[] slots)
    {
        Container = container;
        Slots = slots;
    }
}
