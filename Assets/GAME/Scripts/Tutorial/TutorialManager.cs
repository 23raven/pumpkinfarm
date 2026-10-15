using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial steps, in order")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    [SerializeField] private bool activateFirstStepOnStart = true;

    public TutorialStep CurrentStep { get; private set; }

    private void Start()
    {
        // Hide all step panels before showing the first step.
        foreach (TutorialStep step in tutorialSteps)
        {
            if (step == null)
                continue;

            step.BindManager(this);
            step.Deactivate();
        }

        if (activateFirstStepOnStart && tutorialSteps.Count > 0)
            ActivateTutorial(tutorialSteps[0]);
    }

    public void ActivateTutorial(TutorialStep step)
    {
        if (CurrentStep != null && CurrentStep != step)
            CurrentStep.Deactivate();

        if (step == null)
        {
            CurrentStep = null;
            Debug.Log("Tutorial finished: no next step was assigned.", this);
            return;
        }

        if (!tutorialSteps.Contains(step))
            Debug.LogWarning($"Tutorial step '{step.name}' is not in this manager's tutorialSteps list.", step);

        CurrentStep = step;
        step.Activate(this);
    }

    public void StopTutorial()
    {
        if (CurrentStep != null)
            CurrentStep.Deactivate();

        CurrentStep = null;
    }
}
