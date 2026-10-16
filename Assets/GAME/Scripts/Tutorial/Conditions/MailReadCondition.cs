using System;
using UnityEngine;

public class MailReadCondition : TutorialCondition
{
    private TutorialStep activeStep;
    private bool ordersMailWasRead;

    private void OnEnable()
    {
        GameEvents.Subscribe<MailReadEvent>(OnMailRead);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<MailReadEvent>(OnMailRead);
    }

    public override bool IsMet(TutorialStep step)
    {
        activeStep = step;
        return ordersMailWasRead;
    }

    private void OnMailRead(MailReadEvent message)
    {
        if (activeStep == null || !activeStep.isActive)
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