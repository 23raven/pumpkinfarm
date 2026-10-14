using UnityEngine;

// Вешается на объект в мире (станок для резьбы). Игрок взаимодействует -> открывается окно.
public class CarvingStation : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        if (interactor == null) return;

        GameEvents.Publish(new CarvingOpenedEvent(this));
    }
}
