using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialStep : MonoBehaviour
{
    [Header("Scene references")]
    public Transform StartingPosition;
    public GameObject Player;
    public GameObject Panel;
    public TMP_Text TextMeshPro;

    [Header("Instruction")]
    [TextArea(2, 5)]
    public string TutorialText;

    [Header("Objects changed when this step starts")]
    [Tooltip("These objects are enabled when this step becomes active.")]
    public List<GameObject> objectsToActivateOnStart = new List<GameObject>();

    [Tooltip("These objects are disabled when this step becomes active. Do not add this TutorialStep's own GameObject here, because it would stop checking its conditions.")]
    public List<GameObject> objectsToDeactivateOnStart = new List<GameObject>();

    [Header("Step flow")]
    public TutorialStep nextStep;
    public List<TutorialCondition> conditions = new List<TutorialCondition>();

    [HideInInspector] public bool isActive;
    [HideInInspector] public bool isDone;

    private TutorialManager tutorialManager;

    public void BindManager(TutorialManager manager)
    {
        tutorialManager = manager;
    }

    public void Activate(TutorialManager manager)
    {
        tutorialManager = manager;
        isActive = true;
        isDone = false;

        ApplyObjectsToActivate();
        ApplyObjectsToDeactivate();

        if (Panel != null)
            Panel.SetActive(true);

        if (TextMeshPro == null && Panel != null)
            TextMeshPro = Panel.GetComponentInChildren<TMP_Text>(true);

        if (TextMeshPro != null)
            TextMeshPro.text = TutorialText;

        MovePlayerToStartingPosition();
    }

    public void Deactivate()
    {
        isActive = false;

        if (Panel != null)
            Panel.SetActive(false);
    }

    private void ApplyObjectsToActivate()
    {
        if (objectsToActivateOnStart == null)
            return;

        foreach (GameObject sceneObject in objectsToActivateOnStart)
        {
            if (sceneObject == null)
                continue;

            sceneObject.SetActive(true);
        }
    }

    private void ApplyObjectsToDeactivate()
    {
        if (objectsToDeactivateOnStart == null)
            return;

        foreach (GameObject sceneObject in objectsToDeactivateOnStart)
        {
            if (sceneObject == null)
                continue;

            if (sceneObject == gameObject)
            {
                Debug.LogWarning(
                    $"TutorialStep '{name}' cannot disable its own GameObject, because its conditions would stop checking. Remove it from objectsToDeactivateOnStart.",
                    this);
                continue;
            }

            sceneObject.SetActive(false);
        }
    }

    private void Update()
    {
        CheckCondition();
    }

    public void CheckCondition()
    {
        if (!isActive || isDone || conditions == null || conditions.Count == 0)
            return;

        // The base version uses AND logic: every assigned condition must pass.
        foreach (TutorialCondition condition in conditions)
        {
            // An empty reference should never complete a step by accident.
            if (condition == null || !condition.IsMet(this))
                return;
        }

        isDone = true;
        ActivateNext(nextStep);
    }

    public void ActivateNext(TutorialStep tstep)
    {
        if (tutorialManager == null)
        {
            Debug.LogError($"TutorialStep '{name}' has no TutorialManager assigned.", this);
            return;
        }

        tutorialManager.ActivateTutorial(tstep);
    }

    private void MovePlayerToStartingPosition()
    {
        if (StartingPosition == null || Player == null)
            return;

        Vector3 targetPosition = StartingPosition.position;
        Rigidbody2D playerRigidbody = Player.GetComponent<Rigidbody2D>();

        if (playerRigidbody != null)
        {
            playerRigidbody.position = new Vector2(targetPosition.x, targetPosition.y);
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }
        else
        {
            Player.transform.position = targetPosition;
        }
    }
}
