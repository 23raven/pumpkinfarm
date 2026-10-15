using UnityEngine;

/// <summary>
/// Completes when the assigned scene PickupItem has been fully picked up
/// and its GameObject is destroyed by the existing PickupItem script.
/// </summary>
public class PickUpItemCondition : TutorialCondition
{
    [SerializeField] private PickupItem targetPickup;

    private bool hadTargetAssigned;

    private void Awake()
    {
        // Keep this information because a destroyed Unity object compares equal to null.
        hadTargetAssigned = targetPickup != null;

        if (!hadTargetAssigned)
        {
            Debug.LogWarning(
                $"PickUpItemCondition on '{name}' needs Target Pickup assigned in the Inspector.",
                this);
        }
    }

    public override bool IsMet(TutorialStep step)
    {
        // Do not complete automatically when the Inspector reference was never assigned.
        return hadTargetAssigned && targetPickup == null;
    }
}
