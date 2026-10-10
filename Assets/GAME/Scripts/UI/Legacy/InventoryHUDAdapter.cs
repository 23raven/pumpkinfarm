using UnityEngine;

public class InventoryHUDAdapter : MonoBehaviour
{
    private InventoryHUDView view;

    private void Awake()
    {
        view = GetComponent<InventoryHUDView>();
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
        Refresh();
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    private void OnInventoryChanged(InventoryChangedEvent message)
    {
        if (view == null)
            return;

        InventorySlotData[] slots = message.Slots;

        for (int i = 0; i < view.SlotCount; i++)
        {
            if (i >= slots.Length)
            {
                view.SetSlotText(i, $"Slot {i + 1}\nUnavailable");
                continue;
            }

            InventorySlotData slot = slots[i];

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

    private void Refresh()
    {
        if (GameEvents.TryGetLatest<InventoryChangedEvent>(
            out InventoryChangedEvent message))
        {
            OnInventoryChanged(message);
        }
    }
}