using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Дозаполняет сумку игрока предметами для обучения (например тыква или свеча).
/// Держите объект ВЫКЛЮЧЕННЫМ: он сработает один раз, когда шаг обучения его включит
/// (через Objects To Activate On Start). Выдаёт только недостающее количество.
/// Перед выдачей может очистить сумку (кроме предметов из списка Items To Keep).
/// </summary>
public class TutorialItemGiver : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public ItemDefinition item;
        [Min(1)] public int quantity = 1;
    }

    [Header("Before giving")]
    [Tooltip("Если включено, перед выдачей из сумки удаляются все предметы.")]
    [SerializeField] private bool clearInventoryFirst;
    [Tooltip("Эти предметы НЕ удаляются при очистке (например мотыга и лейка).")]
    [SerializeField] private List<ItemDefinition> itemsToKeep = new List<ItemDefinition>();

    [Header("Items to give")]
    [SerializeField] private List<Entry> items = new List<Entry>();

    private bool given;

    private void Start()
    {
        Give();
    }

    private void Give()
    {
        if (given) return;
        given = true;

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<InventoryService>(out InventoryService inventory))
        {
            Debug.LogError("TutorialItemGiver: InventoryService is unavailable.", this);
            return;
        }

        if (clearInventoryFirst)
            ClearInventory(inventory);

        foreach (Entry entry in items)
        {
            if (entry == null || entry.item == null)
            {
                Debug.LogWarning("TutorialItemGiver: an item is not assigned.", this);
                continue;
            }

            // Если предмет уже есть, добавляем только недостающее
            int missing = entry.quantity - inventory.GetItemCount(entry.item);
            if (missing <= 0) continue;

            int leftover = inventory.AddItem(entry.item, missing);
            if (leftover > 0)
            {
                Debug.LogWarning(
                    "TutorialItemGiver: no room for " + entry.item.DisplayName + ".", this);
            }
        }
    }

    // Удаляет все предметы из доступных слотов, кроме тех, что в списке Items To Keep
    private void ClearInventory(InventoryService inventory)
    {
        IReadOnlyList<InventorySlot> slots = inventory.Slots;

        for (int i = 0; i < slots.Count; i++)
        {
            if (!inventory.TryGetSlotContents(i, out ItemDefinition item, out int quantity) ||
                item == null || quantity <= 0)
            {
                continue;
            }

            if (itemsToKeep.Contains(item))
                continue;

            inventory.TryTakeFromSlot(i, out _, out _);
        }
    }
}

