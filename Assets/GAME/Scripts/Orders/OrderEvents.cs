public readonly struct DailyOrderDisplayData
{
    public string SourceId { get; }
    public string SourceName { get; }
    public ItemDefinition Item { get; }
    public int Quantity { get; }
    public int RewardPerItem { get; }
    public bool IsCompleted { get; }

    public DailyOrderDisplayData(
        string sourceId,
        string sourceName,
        ItemDefinition item,
        int quantity,
        int rewardPerItem,
        bool isCompleted)
    {
        SourceId = sourceId;
        SourceName = sourceName;
        Item = item;
        Quantity = quantity;
        RewardPerItem = rewardPerItem;
        IsCompleted = isCompleted;
    }
}

public readonly struct DailyOrdersChangedEvent
{
    public int Day { get; }
    public int CompletedCount { get; }
    public int AvailableCount { get; }
    public bool IsVisible { get; }
    public DailyOrderDisplayData[] Orders { get; }

    public DailyOrdersChangedEvent(
        int day,
        int completedCount,
        int availableCount,
        bool isVisible,
        DailyOrderDisplayData[] orders)
    {
        Day = day;
        CompletedCount = completedCount;
        AvailableCount = availableCount;
        IsVisible = isVisible;
        Orders = orders;
    }
}
