using UnityEngine;

/// <summary>
/// Выполняется, когда окно резьбы закрыто (кнопкой X или Escape).
/// Если окно и так закрыто в момент начала шага, выполняется сразу.
/// </summary>
public class CloseCarvingCondition : TutorialCondition
{
    public override bool IsMet(TutorialStep step)
    {
        return !PumpkinCarvingPanel.IsOpen;
    }
}
