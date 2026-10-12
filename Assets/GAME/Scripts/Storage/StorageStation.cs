using UnityEngine;

[RequireComponent(typeof(StorageContainer))]
public class StorageStation : MonoBehaviour, IInteractable
{
    private StorageContainer container;

    private void Awake()
    {
        container = GetComponent<StorageContainer>();
    }

    public void Interact(GameObject interactor)
    {
        if (interactor == null) return;
        if (container == null) container = GetComponent<StorageContainer>();
        if (container == null)
        {
            Debug.LogWarning("StorageStation: StorageContainer is missing.", this);
            return;
        }

        GameEvents.Publish(new StorageOpenedEvent(container));
    }
}
