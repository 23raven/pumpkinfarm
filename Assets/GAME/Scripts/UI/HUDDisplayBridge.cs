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

public readonly struct HUDRefreshRequestedEvent
{
}

public class HUDDisplayBridge : MonoBehaviour
{
    private static readonly HUDTextKey[] InventoryKeys =
    {
        HUDTextKey.InventorySlot1,
        HUDTextKey.InventorySlot2
    };

    private void OnEnable()
    {
        GameEvents.Subscribe<WalletBalanceEvent>(OnWalletChanged);
        GameEvents.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
        GameEvents.Subscribe<DayChangedEvent>(OnDayChanged);
        GameEvents.Subscribe<HUDRefreshRequestedEvent>(OnRefreshRequested);

        RefreshLatest();
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<WalletBalanceEvent>(OnWalletChanged);
        GameEvents.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
        GameEvents.Unsubscribe<DayChangedEvent>(OnDayChanged);
        GameEvents.Unsubscribe<HUDRefreshRequestedEvent>(OnRefreshRequested);
    }

    private void OnWalletChanged(WalletBalanceEvent message)
    {
        Publish(
            HUDTextKey.WalletBalance,
            $"Money: {message.Balance}"
        );
    }

    private void OnDayChanged(DayChangedEvent message)
    {
        Publish(
            HUDTextKey.CurrentDay,
            $"Day: {message.Day}"
        );
    }

    private void OnInventoryChanged(InventoryChangedEvent message)
    {
        if (message.Slots == null)
            return;

        string[] lines = new string[message.Slots.Length];

        for (int i = 0; i < message.Slots.Length; i++)
        {
            InventorySlotData slot = message.Slots[i];

            string marker =
                i == message.SelectedSlotIndex ? "> " : "  ";

            if (slot.IsEmpty)
            {
                lines[i] = $"{marker}Slot {i + 1}: Empty";
            }
            else
            {
                lines[i] =
                    $"{marker}Slot {i + 1}: " +
                    $"{slot.Item.DisplayName} x{slot.Quantity}";
            }
        }

        Publish(
            HUDTextKey.InventoryContents,
            string.Join("\n", lines)
        );
    }

    private void OnRefreshRequested(HUDRefreshRequestedEvent message)
    {
        RefreshLatest();
    }

    private void RefreshLatest()
    {
        if (GameEvents.TryGetLatest<WalletBalanceEvent>(out var wallet))
            OnWalletChanged(wallet);

        if (GameEvents.TryGetLatest<InventoryChangedEvent>(out var inventory))
            OnInventoryChanged(inventory);

        if (GameEvents.TryGetLatest<DayChangedEvent>(out var day))
            OnDayChanged(day);
    }

    private void Publish(HUDTextKey key, string text)
    {
        GameEvents.Publish(new HUDTextUpdateEvent(key, text));
    }
}