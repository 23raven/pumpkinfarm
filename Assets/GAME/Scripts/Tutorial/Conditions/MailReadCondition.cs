using System;
using UnityEngine;

public class MailReadCondition : TutorialCondition
{
    private TutorialStep step;
    private bool ordersMailWasRead;

    private void Awake()
    {
        step = GetComponent<TutorialStep>();
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<MailReadEvent>(OnMailRead);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<MailReadEvent>(OnMailRead);
    }

    public override bool IsMet(TutorialStep tutorialStep)
    {
        if (step == null)
            step = tutorialStep;

        return ordersMailWasRead;
    }

    private void OnMailRead(MailReadEvent message)
    {
        if (step == null || !step.isActive)
            return;

        string messageId = message.Message.Id;

        if (!string.IsNullOrEmpty(messageId) &&
            messageId.StartsWith(
                "daily-orders-",
                StringComparison.Ordinal))
        {
            ordersMailWasRead = true;
        }
    }
}