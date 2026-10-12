using System.Collections.Generic;
using UnityEngine;

public class DailyOrderService : MonoBehaviour
{
    [Header("Sales Sources")]
    [SerializeField] private DemandSourceDefinition[] sources;

    private sealed class ActiveOrder
    {
        public string SourceId;
        public string SourceName;
        public ItemDefinition Item;
        public int Quantity;
        public int RewardPerItem;
        public bool IsCompleted;
    }

    private readonly List<ActiveOrder> activeOrders = new List<ActiveOrder>();
    private int currentDay = -1;
    private bool ordersVisible;

    private void OnEnable()
    {
        GameServices services = GameServices.Instance;
        if (services == null)
        {
            Debug.LogError("DailyOrderService: GameServices is missing.", this);
            return;
        }

        if (services.Register(this))
            GameEvents.Subscribe<DayChangedEvent>(OnDayChanged);
    }

    private void Start()
    {
        // Covers the case where DayCycleService published its initial day
        // before this component subscribed to GameEvents.
        GameServices services = GameServices.Instance;
        if (services != null &&
            services.TryGet<DayCycleService>(out DayCycleService dayCycle))
        {
            GenerateForDayIfNeeded(dayCycle.CurrentDay);
        }
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<DayChangedEvent>(OnDayChanged);
        GameServices services = GameServices.Instance;
        if (services != null)
            services.Unregister(this);
    }

    private void OnDayChanged(DayChangedEvent message)
    {
        GenerateForDayIfNeeded(message.Day);
    }

    private void GenerateForDayIfNeeded(int day)
    {
        if (day < 1 || day == currentDay)
            return;

        currentDay = day;
        ordersVisible = false;
        activeOrders.Clear();

        if (sources != null)
        {
            HashSet<string> seenIds = new HashSet<string>();

            foreach (DemandSourceDefinition source in sources)
            {
                if (source == null || string.IsNullOrWhiteSpace(source.SourceId))
                    continue;

                if (!seenIds.Add(source.SourceId))
                {
                    Debug.LogWarning(
                        $"Duplicate demand source id '{source.SourceId}'. Only the first source should use each id.",
                        this
                    );
                    continue;
                }

                if (Random.value > source.DailyOrderChance)
                    continue;

                DemandOption[] options = source.PossibleOrders;
                if (options == null || options.Length == 0)
                    continue;

                List<DemandOption> validOptions = new List<DemandOption>();
                foreach (DemandOption option in options)
                {
                    if (option != null && option.Item != null)
                        validOptions.Add(option);
                }

                if (validOptions.Count == 0)
                    continue;

                DemandOption selected = validOptions[Random.Range(0, validOptions.Count)];
                int quantity = Random.Range(selected.MinQuantity, selected.MaxQuantity + 1);

                activeOrders.Add(new ActiveOrder
                {
                    SourceId = source.SourceId,
                    SourceName = source.DisplayName,
                    Item = selected.Item,
                    Quantity = quantity,
                    RewardPerItem = selected.RewardPerItem,
                    IsCompleted = false
                });
            }
        }

        SendDailyOrdersMail();
    }

    private void SendDailyOrdersMail()
    {
        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<MailboxService>(out MailboxService mailbox))
        {
            Debug.LogError(
                "DailyOrderService: MailboxService is unavailable; orders remain hidden until mail can be delivered.",
                this
            );
            return;
        }

        string subject = $"Daily orders — Day {currentDay}";
        string body;

        if (activeOrders.Count == 0)
        {
            body = "No new purchase orders arrived today. Check back tomorrow.";
        }
        else
        {
            List<string> lines = new List<string>();
            foreach (ActiveOrder order in activeOrders)
            {
                int reward = SafeReward(order.Quantity, order.RewardPerItem);
                lines.Add($"{order.SourceName} requests {order.Quantity} {order.Item.DisplayName} — ${reward} total.");
            }
            body = string.Join("\n", lines);
        }

        // Deliver mail first. Orders are only revealed after this notification.
        mailbox.Receive(new MailMessageData(
            $"daily-orders-{currentDay}",
            currentDay,
            subject,
            body
        ));

        ordersVisible = true;
        PublishOrdersChanged();
    }

    public bool TryFulfill(string sourceId)
    {
        if (!ordersVisible)
        {
            Debug.Log("Orders have not arrived by mail yet.");
            return false;
        }

        ActiveOrder order = activeOrders.Find(o => o.SourceId == sourceId);
        if (order == null)
        {
            Debug.Log("This business has no order for today.");
            return false;
        }

        if (order.IsCompleted)
        {
            Debug.Log("Today's order from this business is already completed.");
            return false;
        }

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<InventoryService>(out InventoryService inventory) ||
            !services.TryGet<WalletService>(out WalletService wallet))
        {
            Debug.LogWarning("DailyOrderService: Inventory or Wallet is unavailable.", this);
            return false;
        }

        if (!inventory.HasItem(order.Item, order.Quantity))
        {
            Debug.Log($"Bring {order.Quantity} {order.Item.DisplayName} to {order.SourceName} to complete this order.");
            return false;
        }

        if (!inventory.RemoveItem(order.Item, order.Quantity))
        {
            Debug.LogWarning("Could not remove requested items from inventory.", this);
            return false;
        }

        int reward = SafeReward(order.Quantity, order.RewardPerItem);
        order.IsCompleted = true;
        if (reward > 0)
            wallet.AddMoney(reward);

        Debug.Log($"Order completed: {order.SourceName}. Earned ${reward}.");
        PublishOrdersChanged();
        return true;
    }

    private int SafeReward(int quantity, int rewardPerItem)
    {
        long total = (long)quantity * rewardPerItem;
        return total > int.MaxValue ? int.MaxValue : Mathf.Max(0, (int)total);
    }

    private void PublishOrdersChanged()
    {
        DailyOrderDisplayData[] snapshot = new DailyOrderDisplayData[activeOrders.Count];
        int completed = 0;

        for (int i = 0; i < activeOrders.Count; i++)
        {
            ActiveOrder order = activeOrders[i];
            if (order.IsCompleted)
                completed++;

            snapshot[i] = new DailyOrderDisplayData(
                order.SourceId,
                order.SourceName,
                order.Item,
                order.Quantity,
                order.RewardPerItem,
                order.IsCompleted
            );
        }

        GameEvents.Publish(new DailyOrdersChangedEvent(
            currentDay,
            completed,
            activeOrders.Count,
            ordersVisible,
            snapshot
        ));
    }
}
