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

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<InventoryService>(
                out InventoryService inventory))
        {
            Debug.LogWarning(
                "PickupItem: InventoryService is unavailable.",
                this
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