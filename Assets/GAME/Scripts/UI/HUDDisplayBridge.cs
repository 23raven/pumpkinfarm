using System.Collections.Generic;
using UnityEngine;

public readonly struct HUDTextUpdateEvent
{
    public HUDTextKey Key { get; }
    public string Text { get; }

    public HUDTextUpdateEvent(HUDTextKey key, string text)
    {
        Key = key;
        Text = text;
    }
}

public readonly struct HUDRefreshRequestedEvent { }

public class HUDDisplayBridge : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.Subscribe<WalletBalanceEvent>(OnWalletChanged);
        GameEvents.Subscribe<InventoryLayoutChangedEvent>(OnInventoryChanged);
        GameEvents.Subscribe<DayChangedEvent>(OnDayChanged);
        GameEvents.Subscribe<HUDRefreshRequestedEvent>(OnRefreshRequested);
        GameEvents.Subscribe<DailyOrdersChangedEvent>(OnDailyOrdersChanged);
        Publish(HUDTextKey.OrderProgress, string.Empty);
        Publish(HUDTextKey.ActiveOrders, string.Empty);
        RefreshLatest();
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<WalletBalanceEvent>(OnWalletChanged);
        GameEvents.Unsubscribe<InventoryLayoutChangedEvent>(OnInventoryChanged);
        GameEvents.Unsubscribe<DayChangedEvent>(OnDayChanged);
        GameEvents.Unsubscribe<HUDRefreshRequestedEvent>(OnRefreshRequested);
        GameEvents.Unsubscribe<DailyOrdersChangedEvent>(OnDailyOrdersChanged);
    }

    private void OnWalletChanged(WalletBalanceEvent message)
    {
        Publish(HUDTextKey.WalletBalance, $"Money: {message.Balance}");
    }

    private void OnDayChanged(DayChangedEvent message)
    {
        Publish(HUDTextKey.CurrentDay, $"Day: {message.Day}");
    }

    private void OnInventoryChanged(InventoryLayoutChangedEvent message)
    {
        if (message.Slots == null)
            return;

        var sectionIndices = new Dictionary<InventorySlotGroup, int>
        {
            { InventorySlotGroup.Base, 0 },
            { InventorySlotGroup.Backpack, 0 },
            { InventorySlotGroup.Cart, 0 }
        };

        string[] lines = new string[message.Slots.Length];

        for (int i = 0; i < message.Slots.Length; i++)
        {
            InventorySlotDisplayData slot = message.Slots[i];
            int groupIndex = sectionIndices[slot.Group] + 1;
            sectionIndices[slot.Group] = groupIndex;

            string marker = i == message.SelectedSlotIndex ? "> " : "  ";
            string groupName = slot.Group.ToString();
            string parkedSuffix = slot.Group == InventorySlotGroup.Cart && !slot.IsAccessible
                ? " (Parked)"
                : string.Empty;

            string contents = slot.IsEmpty
                ? (slot.IsAccessible ? "Empty" : "Locked")
                : $"{slot.Item.DisplayName} x{slot.Quantity}";

            string storedSuffix = !slot.IsAccessible && !slot.IsEmpty
                ? " (stored)"
                : string.Empty;

            lines[i] = $"{marker}{groupName} {groupIndex}{parkedSuffix}: {contents}{storedSuffix}";
        }

        Publish(HUDTextKey.InventoryContents, string.Join("\n", lines));
    }

    private void OnDailyOrdersChanged(DailyOrdersChangedEvent message)
    {
        // Order progress and orders are deliberately hidden until the player
        // reads the current day's order mail at the mailbox.
        if (!message.IsVisible)
        {
            Publish(HUDTextKey.OrderProgress, string.Empty);
            Publish(HUDTextKey.ActiveOrders, string.Empty);
            return;
        }

        Publish(
            HUDTextKey.OrderProgress,
            $"Orders completed: {message.CompletedCount}/{message.AvailableCount}"
        );

        if (message.Orders == null || message.Orders.Length == 0)
        {
            Publish(HUDTextKey.ActiveOrders, "No active orders today.");
            return;
        }

        List<string> lines = new List<string>();
        foreach (DailyOrderDisplayData order in message.Orders)
        {
            int totalReward = SafeReward(order.Quantity, order.RewardPerItem);
            string status = order.IsCompleted ? "[COMPLETED]" : "[OPEN]";
            lines.Add($"{status} {order.SourceName}: {order.Quantity} {order.Item.DisplayName} — ${totalReward}");
        }

        Publish(HUDTextKey.ActiveOrders, string.Join("\n", lines));
    }

    private int SafeReward(int quantity, int unitReward)
    {
        long total = (long)quantity * unitReward;
        return total > int.MaxValue ? int.MaxValue : Mathf.Max(0, (int)total);
    }

    private void OnRefreshRequested(HUDRefreshRequestedEvent message)
    {
        RefreshLatest();
    }

    private void RefreshLatest()
    {
        if (GameEvents.TryGetLatest<WalletBalanceEvent>(out var wallet))
            OnWalletChanged(wallet);
        if (GameEvents.TryGetLatest<InventoryLayoutChangedEvent>(out var inventory))
            OnInventoryChanged(inventory);
        if (GameEvents.TryGetLatest<DayChangedEvent>(out var day))
            OnDayChanged(day);
        if (GameEvents.TryGetLatest<DailyOrdersChangedEvent>(out var orders))
            OnDailyOrdersChanged(orders);
    }

    private void Publish(HUDTextKey key, string text)
    {
        GameEvents.Publish(new HUDTextUpdateEvent(key, text));
    }
}
