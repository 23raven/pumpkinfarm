using UnityEngine;

public class Npc2 : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogRenderer dialogRenderer;
    [SerializeField] private Dialog dialog;

    public void Interact(GameObject interactor)
    {
        dialogRenderer.Current_dialog = dialog;
        dialogRenderer.CurrentPhrase = 0;
        dialogRenderer.ShowDialog();
    }
}
