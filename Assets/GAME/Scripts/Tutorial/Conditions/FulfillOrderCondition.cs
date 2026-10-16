using UnityEngine;

public class FulfillOrderCondition : TutorialCondition
{
    [Header("Order")]
    [SerializeField] private DemandSourceDefinition targetSource;

    private TutorialStep activeStep;
    private bool orderCompleted;

    private void OnEnable()
    {
        GameEvents.Subscribe<DailyOrdersChangedEvent>(
            OnDailyOrdersChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<DailyOrdersChangedEvent>(
            OnDailyOrdersChanged);
    }

    public override bool IsMet(TutorialStep step)
    {
        if (step == null || !step.isActive)
            return false;

        if (activeStep != step)
        {
            activeStep = step;
            orderCompleted = false;
        }

        return orderCompleted;
    }

    private void OnDailyOrdersChanged(DailyOrdersChangedEvent message)
    {
        if (activeStep == null ||
            !activeStep.isActive ||
            targetSource == null ||
            !message.IsVisible ||
            message.Orders == null)
        {
            return;
        }

        foreach (DailyOrderDisplayData order in message.Orders)
        {
            if (order.SourceId != targetSource.SourceId)
                continue;

            if (order.IsCompleted)
            {
                orderCompleted = true;
                return;
            }
        }
    }
}