using UnityEngine;

// Вешается на станок резьбы в мире. По взаимодействию открывает окно резьбы.
public class PumpkinCarvingStation : MonoBehaviour, IInteractable
{
    [SerializeField] private PumpkinCarvingCatalog catalog;

    public void Interact(GameObject interactor)
    {
        if (catalog == null)
        {
            Debug.LogWarning("PumpkinCarvingStation: Catalog is not assigned.", this);
            return;
        }

        GameEvents.Publish(new PumpkinCarvingOpenedEvent(catalog));
    }
}
