using UnityEngine;

/// <summary>
/// Выполняется, когда игрок изменил тыкву в окне резьбы.
/// Можно потребовать: смену лица, добавление свечи, конкретное лицо.
/// Считается только то, что произошло ПОСЛЕ начала шага.
/// </summary>
public class CarvePumpkinCondition : TutorialCondition
{
    [Header("What counts")]
    [SerializeField] private bool requireFaceChange = true;
    [SerializeField] private bool requireCandleAdded;

    [Header("Specific face (optional)")]
    [SerializeField] private bool requireSpecificFace;
    [SerializeField] private PumpkinFace requiredFace = PumpkinFace.Cat;

    private bool started;
    private bool happened;

    private void OnEnable()
    {
        GameEvents.Subscribe<PumpkinCarvedEvent>(OnCarved);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<PumpkinCarvedEvent>(OnCarved);
    }

    public override bool IsMet(TutorialStep step)
    {
        if (!started)
        {
            started = true;
            return false;
        }

        return happened;
    }

    private void OnCarved(PumpkinCarvedEvent message)
    {
        if (!started) return;

        if (requireFaceChange && !message.FaceChanged) return;
        if (requireCandleAdded && !message.CandleAdded) return;
        if (requireSpecificFace && message.Result.Face != requiredFace) return;

        happened = true;
    }
}
