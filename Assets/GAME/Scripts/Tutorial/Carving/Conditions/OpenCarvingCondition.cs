using UnityEngine;

/// <summary>
/// Выполняется, когда игрок открыл окно резьбы (подошёл к станку и нажал Space).
/// Считается только то, что произошло ПОСЛЕ начала шага.
/// </summary>
public class OpenCarvingCondition : TutorialCondition
{
    private bool started;
    private bool happened;

    private void OnEnable()
    {
        GameEvents.Subscribe<PumpkinCarvingOpenedEvent>(OnOpened);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<PumpkinCarvingOpenedEvent>(OnOpened);
    }

    public override bool IsMet(TutorialStep step)
    {
        // Первый вызов = шаг только что начался
        if (!started)
        {
            started = true;
            return false;
        }

        return happened;
    }

    private void OnOpened(PumpkinCarvingOpenedEvent message)
    {
        if (started) happened = true;
    }
}
