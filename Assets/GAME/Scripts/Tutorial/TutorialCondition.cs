using UnityEngine;

/// <summary>
/// Base class for a condition that can complete a TutorialStep.
/// Concrete conditions will be added separately (movement, pickup, and so on).
/// </summary>
public abstract class TutorialCondition : MonoBehaviour
{
    public abstract bool IsMet(TutorialStep step);
}
