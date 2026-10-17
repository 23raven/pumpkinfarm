using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays one phrase at a time from a Dialog ScriptableObject.
/// Space advances the dialogue; Space on the final phrase closes it.
/// GameplayInputGate blocks movement and other gameplay input while open.
/// Keep this component on an always-active controller, outside MainObject.
/// </summary>
public sealed class DialogRenderer : MonoBehaviour
{
    [Header("Dialog State")]
    public Dialog Current_dialog;
    public int CurrentPhrase;

    [Header("UI References")]
    [SerializeField] private TMP_Text PlayerPhrases;
    [SerializeField] private TMP_Text NpcPhrases;
    [SerializeField] private Image NpcIcon;
    [SerializeField] private Image PlayerIcon;
    [SerializeField] private TMP_Text PlayerName;
    [SerializeField] private TMP_Text NpcName;
    [SerializeField] private GameObject MainObject;

    private bool dialogOpen;
    private bool previousInputGateState;
    private int openedFrame = -1;

    public bool IsOpen => dialogOpen;

    private void Awake()
    {
        if (!ValidateReferences())
            return;

        MainObject.SetActive(false);
        ClearUI();
        Current_dialog = null;
        CurrentPhrase = 0;
    }

    public void StartDialog(Dialog dialog)
    {
        if (dialogOpen)
            return;

        if (dialog == null)
        {
            Debug.LogWarning("DialogRenderer: Cannot start a null Dialog asset.", this);
            return;
        }

        if (dialog.dialog == null || dialog.dialog.Count == 0)
        {
            Debug.LogWarning($"DialogRenderer: '{dialog.name}' contains no phrases.", dialog);
            return;
        }

        if (!ValidateReferences())
            return;

        Current_dialog = dialog;
        CurrentPhrase = 0;
        OpenAndDisplayCurrentPhrase();
    }

    // Kept for compatibility with older UnityEvents that call ShowDialog.
    public void ShowDialog()
    {
        if (Current_dialog == null ||
            Current_dialog.dialog == null ||
            Current_dialog.dialog.Count == 0)
        {
            Debug.LogWarning("DialogRenderer: No valid dialogue is assigned as current.", this);
            return;
        }

        if (!dialogOpen)
        {
            if (!ValidateReferences())
                return;

            CurrentPhrase = Mathf.Clamp(CurrentPhrase, 0, Current_dialog.dialog.Count - 1);
            OpenAndDisplayCurrentPhrase();
            return;
        }

        DisplayCurrentPhrase();
    }

    public void CloseDialog()
    {
        if (MainObject != null)
            MainObject.SetActive(false);

        ClearUI();
        dialogOpen = false;
        Current_dialog = null;
        CurrentPhrase = 0;
        openedFrame = -1;

        GameplayInputGate.SetBlocked(previousInputGateState);
    }

    private void Update()
    {
        if (!dialogOpen || Current_dialog == null)
            return;

        // Do not advance on the same Space press that opened the dialogue.
        if (Time.frameCount <= openedFrame || !Input.GetKeyDown(KeyCode.Space))
            return;

        if (CurrentPhrase < Current_dialog.dialog.Count - 1)
        {
            CurrentPhrase++;
            DisplayCurrentPhrase();
        }
        else
        {
            CloseDialog();
        }
    }

    private void OpenAndDisplayCurrentPhrase()
    {
        previousInputGateState = GameplayInputGate.IsBlocked;
        GameplayInputGate.SetBlocked(true);

        dialogOpen = true;
        openedFrame = Time.frameCount;
        MainObject.SetActive(true);
        DisplayCurrentPhrase();
    }

    private void DisplayCurrentPhrase()
    {
        if (Current_dialog == null ||
            Current_dialog.dialog == null ||
            Current_dialog.dialog.Count == 0)
        {
            CloseDialog();
            return;
        }

        CurrentPhrase = Mathf.Clamp(CurrentPhrase, 0, Current_dialog.dialog.Count - 1);
        Phrase phrase = Current_dialog.dialog[CurrentPhrase];
        ClearUI();

        if (phrase.Player)
        {
            if (PlayerPhrases != null) PlayerPhrases.text = phrase.text;
            if (PlayerName != null) PlayerName.text = phrase.Name;
            SetIcon(PlayerIcon, phrase.icon);
        }
        else
        {
            if (NpcPhrases != null) NpcPhrases.text = phrase.text;
            if (NpcName != null) NpcName.text = phrase.Name;
            SetIcon(NpcIcon, phrase.icon);
        }
    }

    private void ClearUI()
    {
        if (PlayerPhrases != null) PlayerPhrases.text = string.Empty;
        if (NpcPhrases != null) NpcPhrases.text = string.Empty;
        if (PlayerName != null) PlayerName.text = string.Empty;
        if (NpcName != null) NpcName.text = string.Empty;

        SetIcon(PlayerIcon, null);
        SetIcon(NpcIcon, null);
    }

    private static void SetIcon(Image target, Sprite sprite)
    {
        if (target == null)
            return;

        target.sprite = sprite;
        target.enabled = sprite != null;
    }

    private bool ValidateReferences()
    {
        if (MainObject == null)
        {
            Debug.LogError("DialogRenderer: Main Object (dialog panel root) is not assigned.", this);
            return false;
        }

        if (MainObject == gameObject || transform.IsChildOf(MainObject.transform))
        {
            Debug.LogError(
                "DialogRenderer must stay on an always-active object outside the dialogue panel. " +
                "Assign the panel to Main Object, not this controller object.", this);
            return false;
        }

        return true;
    }

    private void OnDisable()
    {
        if (!dialogOpen)
            return;

        dialogOpen = false;
        Current_dialog = null;
        CurrentPhrase = 0;
        openedFrame = -1;
        ClearUI();

        if (MainObject != null)
            MainObject.SetActive(false);

        GameplayInputGate.SetBlocked(previousInputGateState);
    }
}
