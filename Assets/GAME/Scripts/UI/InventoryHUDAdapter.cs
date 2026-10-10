using UnityEngine;

public class InventoryHUDAdapter : MonoBehaviour
{
    private InventoryService inventory;
    private InventoryHUDView view;
    private bool subscribed;

    private void Awake()
    {
        view = GetComponent<InventoryHUDView>();
    }

    private void OnEnable()
    {
        Connect();
    }

    private void Start()
    {
        // Retry after all active objects have initialized.
        Connect();

        if (!subscribed)
        {
            Debug.LogError(
                "InventoryHUDAdapter could not find InventoryService.",
                this
            );
        }
    }

    private void OnDisable()
    {
        Disconnect();
    }

    private void Connect()
    {
        if (subscribed)
            return;

        if (view == null)
            view = GetComponent<InventoryHUDView>();

        if (view == null)
            return;

        GameServices services = GameServices.Instance;

        if (services == null)
            return;

        if (!services.TryGet<InventoryService>(out inventory))
            return;

        inventory.InventoryChanged += Refresh;
        subscribed = true;

        Refresh();
    }

    private void Disconnect()
    {
        if (subscribed && inventory != null)
            inventory.InventoryChanged -= Refresh;

        inventory = null;
        subscribed = false;
    }

    private void Refresh()
    {
        if (view == null || inventory == null)
            return;

        var slots = inventory.Slots;

        for (int i = 0; i < view.SlotCount; i++)
        {
            if (i >= slots.Count)
            {
                view.SetSlotText(i, $"Slot {i + 1}\nUnavailable");
                continue;
            }

            InventorySlot slot = slots[i];

            if (slot.IsEmpty)
            {
                view.SetSlotText(i, $"Slot {i + 1}\nEmpty");
            }
            else
            {
                view.SetSlotText(
                    i,
                    $"Slot {i + 1}\n{slot.Item.DisplayName} x{slot.Quantity}"
                );
            }
        }
    }
}