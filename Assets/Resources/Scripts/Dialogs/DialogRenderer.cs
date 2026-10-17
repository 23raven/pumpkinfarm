using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private bool waitForSpaceRelease;

    private int openedFrame = -1;
    private int closedFrame = -1;

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

        if (Time.frameCount == closedFrame || waitForSpaceRelease)
            return;

        if (dialog == null)
        {
            Debug.LogWarning("DialogRenderer: Dialog is null.", this);
            return;
        }

        if (dialog.dialog == null || dialog.dialog.Count == 0)
        {
            Debug.LogWarning($"DialogRenderer: '{dialog.name}' has no phrases.", dialog);
            return;
        }

        if (!ValidateReferences())
            return;

        Current_dialog = dialog;
        CurrentPhrase = 0;

        OpenAndDisplayCurrentPhrase();
    }

    // Compatibility with existing NPC scripts and UnityEvents.
    public void ShowDialog()
    {
        if (!dialogOpen &&
            (Time.frameCount == closedFrame || waitForSpaceRelease))
        {
            return;
        }

        if (Current_dialog == null ||
            Current_dialog.dialog == null ||
            Current_dialog.dialog.Count == 0)
        {
            Debug.LogWarning("DialogRenderer: No valid dialogue assigned.", this);
            return;
        }

        if (!dialogOpen)
        {
            if (!ValidateReferences())
                return;

            CurrentPhrase = Mathf.Clamp(
                CurrentPhrase, 0, Current_dialog.dialog.Count - 1);

            OpenAndDisplayCurrentPhrase();
        }
        else
        {
            DisplayCurrentPhrase();
        }
    }

    public void CloseDialog()
    {
        if (!dialogOpen)
            return;

        // Prevent the same Space press from reopening the dialogue.
        closedFrame = Time.frameCount;
        waitForSpaceRelease = Input.GetKey(KeyCode.Space);

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
        // Wait until Space is released before allowing another dialogue.
        if (waitForSpaceRelease)
        {
            if (Input.GetKey(KeyCode.Space))
                return;

            waitForSpaceRelease = false;
        }

        if (!dialogOpen || Current_dialog == null)
            return;

        // Ignore the Space press that opened the dialogue.
        if (Time.frameCount <= openedFrame)
            return;

        if (!Input.GetKeyDown(KeyCode.Space))
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

        CurrentPhrase = Mathf.Clamp(
            CurrentPhrase, 0, Current_dialog.dialog.Count - 1);

        Phrase phrase = Current_dialog.dialog[CurrentPhrase];

        ClearUI();

        if (phrase.Player)
        {
            if (PlayerPhrases != null)
                PlayerPhrases.text = phrase.text;

            if (PlayerName != null)
                PlayerName.text = phrase.Name;

            SetIcon(PlayerIcon, phrase.icon);
        }
        else
        {
            if (NpcPhrases != null)
                NpcPhrases.text = phrase.text;

            if (NpcName != null)
                NpcName.text = phrase.Name;

            SetIcon(NpcIcon, phrase.icon);
        }
    }

    private void ClearUI()
    {
        if (PlayerPhrases != null)
            PlayerPhrases.text = string.Empty;

        if (NpcPhrases != null)
            NpcPhrases.text = string.Empty;

        if (PlayerName != null)
            PlayerName.text = string.Empty;

        if (NpcName != null)
            NpcName.text = string.Empty;

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
            Debug.LogError(
                "DialogRenderer: Main Object is not assigned.", this);
            return false;
        }

        if (MainObject == gameObject ||
            transform.IsChildOf(MainObject.transform))
        {
            Debug.LogError(
                "DialogRenderer must be outside the dialogue panel.", this);
            return false;
        }

        return true;
    }

    private void OnDisable()
    {
        if (!dialogOpen)
            return;

        closedFrame = Time.frameCount;
        waitForSpaceRelease = Input.GetKey(KeyCode.Space);

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