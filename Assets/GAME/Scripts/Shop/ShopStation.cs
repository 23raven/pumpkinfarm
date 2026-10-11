using UnityEngine;

public class ShopStation : MonoBehaviour, IInteractable
{
    [SerializeField] private ShopCatalog catalog;

    public void Interact(GameObject interactor)
    {
        if (catalog == null)
        {
            Debug.LogWarning(
                "ShopStation: Catalog is not assigned.",
                this
            );
            return;
        }

        GameEvents.Publish(new ShopOpenedEvent(catalog));
    }
}