using TMPro;
using UnityEngine;

/// <summary>
/// TAB toggles the two-column objective/help panel during gameplay.
/// The separate GameEndingPanelController handles the ending screen.
/// Keep this controller on an always-active object outside panelRoot.
/// </summary>
public sealed class StorageCollectionPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text todoText;
    [SerializeField] private StorageCollectionGameCondition gameCondition;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    private bool isOpen;
    private bool gameHasEnded;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        isOpen = false;
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<StorageCollectionProgressChangedEvent>(OnProgressChanged);
        GameEvents.Subscribe<StorageCollectionCompletedEvent>(OnGameCompleted);
        RefreshTodoText();

        if (gameCondition != null && gameCondition.IsComplete)
            HideForEnding();
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<StorageCollectionProgressChangedEvent>(OnProgressChanged);
        GameEvents.Unsubscribe<StorageCollectionCompletedEvent>(OnGameCompleted);

        if (isOpen && !gameHasEnded && (gameCondition == null || !gameCondition.IsComplete))
            GameplayInputGate.SetBlocked(false);

        isOpen = false;
    }

    private void Update()
    {
        if (gameHasEnded || (gameCondition != null && gameCondition.IsComplete))
        {
            HideForEnding();
            return;
        }

        if (panelRoot == null)
            return;

        if (isOpen)
        {
            if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(closeKey))
                Close();
            return;
        }

        if (Input.GetKeyDown(toggleKey) && !GameplayInputGate.IsBlocked)
            Open();
    }

    private void Open()
    {
        if (panelRoot == null || gameHasEnded ||
            (gameCondition != null && gameCondition.IsComplete))
            return;

        RefreshTodoText();
        panelRoot.SetActive(true);
        isOpen = true;
        GameplayInputGate.SetBlocked(true);
    }

    private void Close()
    {
        if (gameHasEnded || (gameCondition != null && gameCondition.IsComplete))
            return;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        isOpen = false;
        GameplayInputGate.SetBlocked(false);
    }

    private void OnProgressChanged(StorageCollectionProgressChangedEvent message)
    {
        if (todoText != null)
            todoText.text = message.TodoText;
    }

    private void OnGameCompleted(StorageCollectionCompletedEvent message)
    {
        gameHasEnded = true;
        HideForEnding();
    }

    private void HideForEnding()
    {
        gameHasEnded = true;

        if (panelRoot != null && panelRoot.activeSelf)
            panelRoot.SetActive(false);

        isOpen = false;
        GameplayInputGate.SetBlocked(true);
    }

    private void RefreshTodoText()
    {
        if (todoText != null && gameCondition != null)
            todoText.text = gameCondition.BuildTodoText();
    }
}
