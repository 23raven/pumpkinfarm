using UnityEngine;

/// <summary>
/// Completes when the player has pressed each of W, A, S and D at least once
/// while the owning TutorialStep is active.
/// </summary>
public class UseWASDCondition : TutorialCondition
{
    private bool pressedW;
    private bool pressedA;
    private bool pressedS;
    private bool pressedD;

    public override bool IsMet(TutorialStep step)
    {
        if (Input.GetKeyDown(KeyCode.W)) pressedW = true;
        if (Input.GetKeyDown(KeyCode.A)) pressedA = true;
        if (Input.GetKeyDown(KeyCode.S)) pressedS = true;
        if (Input.GetKeyDown(KeyCode.D)) pressedD = true;

        return pressedW && pressedA && pressedS && pressedD;
    }
}
