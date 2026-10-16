using UnityEngine;

/// <summary>
/// Completes when the mailbox is opened while this tutorial step is active.
/// </summary>
public class MailboxOpenedCondition : TutorialCondition
{
    private TutorialStep step;
    private bool mailboxWasOpened;

    private void Awake()
    {
        step = GetComponent<TutorialStep>();
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<MailboxOpenRequestedEvent>(OnMailboxOpenRequested);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<MailboxOpenRequestedEvent>(OnMailboxOpenRequested);
    }

    public override bool IsMet(TutorialStep tutorialStep)
    {
        if (step == null)
            step = tutorialStep;

        return mailboxWasOpened;
    }

    private void OnMailboxOpenRequested(MailboxOpenRequestedEvent message)
    {
        if (step != null && step.isActive)
            mailboxWasOpened = true;
    }
}
