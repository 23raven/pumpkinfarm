using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Взаимодействие с объектом: " + gameObject.name);
    }
}