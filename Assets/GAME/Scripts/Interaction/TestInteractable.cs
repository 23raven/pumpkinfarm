using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        Debug.Log(
            interactor.name + " interacted with " + gameObject.name
        );
    }
}