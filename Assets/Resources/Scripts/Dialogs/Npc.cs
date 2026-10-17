using UnityEngine;

/// <summary>
/// Opens the ScriptableObject dialogue when the player's existing
/// PlayerInteractor selects this NPC as the nearest IInteractable.
/// </summary>
public sealed class Npc : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private Dialog dialog;
    [SerializeField] private DialogRenderer dialogRenderer;

    public void Interact(GameObject interactor)
    {
        if (GameplayInputGate.IsBlocked)
            return;

        if (dialog == null)
        {
            Debug.LogWarning($"NPC '{name}' has no Dialog asset assigned.", this);
            return;
        }

        if (dialogRenderer == null)
            dialogRenderer = FindObjectOfType<DialogRenderer>();

        if (dialogRenderer == null)
        {
            Debug.LogError(
                $"NPC '{name}' cannot open its dialogue: no active DialogRenderer exists in the scene.",
                this);
            return;
        }

        dialogRenderer.StartDialog(dialog);
    }
}
