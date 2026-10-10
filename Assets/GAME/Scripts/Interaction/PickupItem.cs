using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemDefinition item;
    [SerializeField, Min(1)] private int amount = 1;

    public void Interact(GameObject interactor)
    {
        if (interactor == null || item == null)
        {
            Debug.LogWarning(
                "PickupItem: Missing interactor or item definition.",
                this
            );

            return;
        }

        InventoryService inventory =
            interactor.GetComponent<InventoryService>();

        if (inventory == null)
        {
            Debug.LogWarning(
                "PickupItem: InventoryService not found on interactor.",
                interactor
            );

            return;
        }

        int remaining = inventory.AddItem(item, amount);
        int added = amount - remaining;

        if (added <= 0)
        {
            Debug.Log("Inventory is full.");
            return;
        }

        Debug.Log(
            $"Picked up {added} {item.DisplayName}. " +
            $"Total: {inventory.GetItemCount(item)}"
        );

        if (remaining == 0)
        {
            Destroy(gameObject);
        }
        else
        {
            amount = remaining;
        }
    }
}