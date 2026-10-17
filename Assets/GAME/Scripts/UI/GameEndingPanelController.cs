using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Opens the ending panel and hides gameplay UI when the storage goal completes.
/// Keep this component on an always-active object outside hidden UI roots.
/// </summary>
public sealed class GameEndingPanelController : MonoBehaviour
{
    [Header("Ending Screen")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private StorageCollectionGameCondition gameCondition;

    [Header("UI To Hide When Game Ends")]
    [Tooltip("Assign normal UI roots only, not the whole Canvas or the Ending Panel's parent.")]
    [SerializeField] private GameObject[] gameplayUiRootsToHide;

    [Header("Gameplay Components To Disable (Optional)")]
    [Tooltip("Assign player movement and other gameplay input components here for extra safety.")]
    [SerializeField] private Behaviour[] gameplayBehavioursToDisable;

    // Track only objects/components that this controller actually disabled, so
    // the restart button can restore them if they survive a scene reload.
    private static readonly HashSet<Behaviour> disabledBehaviours = new HashSet<Behaviour>();
    private static readonly HashSet<GameObject> hiddenUiRoots = new HashSet<GameObject>();
    private static readonly HashSet<GameEndingPanelController> activeControllers =
        new HashSet<GameEndingPanelController>();

    private bool gameHasEnded;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        activeControllers.Add(this);
        GameEvents.Subscribe<StorageCollectionCompletedEvent>(OnGameCompleted);
    }

    private void Start()
    {
        if (gameCondition != null && gameCondition.IsComplete)
            ShowEndingPanel();
    }

    private void OnDisable()
    {
        activeControllers.Remove(this);
        GameEvents.Unsubscribe<StorageCollectionCompletedEvent>(OnGameCompleted);
    }

    private void Update()
    {
        if (!gameHasEnded && gameCondition != null && gameCondition.IsComplete)
            ShowEndingPanel();

        // Modal panels cannot unlock input while the ending screen is active.
        if (gameHasEnded)
            GameplayInputGate.Lock();
    }

    private void OnGameCompleted(StorageCollectionCompletedEvent message)
    {
        ShowEndingPanel();
    }

    private void ShowEndingPanel()
    {
        if (gameHasEnded)
        {
            GameplayInputGate.Lock();
            return;
        }

        gameHasEnded = true;
        GameplayInputGate.Lock();
        DisableGameplayBehaviours();
        HideGameplayUI();

        if (panelRoot != null)
            panelRoot.SetActive(true);
        else
            Debug.LogWarning("GameEndingPanelController: Panel Root is not assigned.", this);
    }

    private void DisableGameplayBehaviours()
    {
        if (gameplayBehavioursToDisable == null)
            return;

        foreach (Behaviour behaviour in gameplayBehavioursToDisable)
        {
            if (behaviour != null && behaviour != this && behaviour.enabled)
            {
                behaviour.enabled = false;
                disabledBehaviours.Add(behaviour);
            }
        }
    }

    private void HideGameplayUI()
    {
        if (gameplayUiRootsToHide == null)
            return;

        foreach (GameObject uiRoot in gameplayUiRootsToHide)
        {
            if (uiRoot == null)
                continue;

            if (panelRoot != null &&
                (uiRoot == panelRoot || panelRoot.transform.IsChildOf(uiRoot.transform)))
            {
                Debug.LogWarning(
                    "GameEndingPanelController skipped a UI root that contains the Ending Panel. Assign a separate GameplayUI root.",
                    uiRoot);
                continue;
            }

            if (uiRoot.activeSelf)
            {
                uiRoot.SetActive(false);
                hiddenUiRoots.Add(uiRoot);
            }
        }
    }

    /// <summary>
    /// Restores objects hidden/disabled by the ending screen before a scene restart.
    /// Safe if those objects are about to be destroyed as part of the reload.
    /// </summary>
    public static void RestoreGameplayForSceneReload()
    {
        foreach (GameEndingPanelController controller in activeControllers)
        {
            if (controller == null)
                continue;

            controller.gameHasEnded = false;
            if (controller.panelRoot != null)
                controller.panelRoot.SetActive(false);
        }

        foreach (Behaviour behaviour in disabledBehaviours)
        {
            if (behaviour != null)
                behaviour.enabled = true;
        }
        disabledBehaviours.Clear();

        foreach (GameObject uiRoot in hiddenUiRoots)
        {
            if (uiRoot != null)
                uiRoot.SetActive(true);
        }
        hiddenUiRoots.Clear();
    }
}
