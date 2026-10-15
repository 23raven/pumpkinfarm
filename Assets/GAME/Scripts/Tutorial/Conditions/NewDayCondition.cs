using UnityEngine;

public class NewDayCondition : TutorialCondition
{
    private DayCycleService dayCycle;
    private int startingDay;
    private bool initialized;

    public override bool IsMet(TutorialStep step)
    {
        if (!initialized)
        {
            GameServices services = GameServices.Instance;

            if (services == null ||
                !services.TryGet<DayCycleService>(out dayCycle))
            {
                return false;
            }

            startingDay = dayCycle.CurrentDay;
            initialized = true;

            return false;
        }

        return dayCycle != null &&
               dayCycle.CurrentDay > startingDay;
    }
}