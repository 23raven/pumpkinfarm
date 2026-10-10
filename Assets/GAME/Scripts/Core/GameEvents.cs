using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    private static readonly Dictionary<Type, Delegate> listeners =
        new Dictionary<Type, Delegate>();

    private static readonly Dictionary<Type, object> latestEvents =
        new Dictionary<Type, object>();

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        listeners.Clear();
        latestEvents.Clear();
    }

    public static void Subscribe<T>(Action<T> callback)
    {
        if (callback == null)
            return;

        Type type = typeof(T);

        if (listeners.TryGetValue(type, out Delegate existing))
            listeners[type] = Delegate.Combine(existing, callback);
        else
            listeners.Add(type, callback);
    }

    public static void Unsubscribe<T>(Action<T> callback)
    {
        if (callback == null)
            return;

        Type type = typeof(T);

        if (!listeners.TryGetValue(type, out Delegate existing))
            return;

        Delegate updated = Delegate.Remove(existing, callback);

        if (updated == null)
            listeners.Remove(type);
        else
            listeners[type] = updated;
    }

    public static void Publish<T>(T message)
    {
        Type type = typeof(T);

        latestEvents[type] = message;

        if (listeners.TryGetValue(type, out Delegate callback))
            ((Action<T>)callback).Invoke(message);
    }

    public static bool TryGetLatest<T>(out T message)
    {
        if (latestEvents.TryGetValue(typeof(T), out object value) &&
            value is T typed)
        {
            message = typed;
            return true;
        }

        message = default;
        return false;
    }
}

public readonly struct WalletBalanceEvent
{
    public int Balance { get; }

    public WalletBalanceEvent(int balance)
    {
        Balance = balance;
    }
}

public readonly struct InventorySlotData
{
    public ItemDefinition Item { get; }
    public int Quantity { get; }

    public bool IsEmpty => Item == null || Quantity <= 0;

    public InventorySlotData(ItemDefinition item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }
}

public readonly struct InventoryChangedEvent
{
    public InventorySlotData[] Slots { get; }
    public int SelectedSlotIndex { get; }

    public InventoryChangedEvent(
        InventorySlotData[] slots,
        int selectedSlotIndex)
    {
        Slots = slots;
        SelectedSlotIndex = selectedSlotIndex;
    }
}

public readonly struct DayChangedEvent
{
    public int Day { get; }

    public DayChangedEvent(int day)
    {
        Day = day;
    }
}