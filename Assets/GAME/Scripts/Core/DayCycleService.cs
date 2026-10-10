using UnityEngine;

public class DayCycleService : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int startingDay = 1;

    public int CurrentDay { get; private set; }

    private void Awake()
    {
        CurrentDay = Mathf.Max(1, startingDay);
    }

    private void OnEnable()
    {
        GameServices services = GameServices.Instance;

        if (services == null)
        {
            Debug.LogError("GameServices is missing.", this);
            return;
        }

        if (services.Register(this))
            PublishDay();
    }

    private void OnDisable()
    {
        GameServices services = GameServices.Instance;

        if (services != null)
            services.Unregister(this);
    }

    public void AdvanceDay()
    {
        CurrentDay++;
        PublishDay();
    }

    private void PublishDay()
    {
        GameEvents.Publish(
            new DayChangedEvent(CurrentDay)
        );
    }
}