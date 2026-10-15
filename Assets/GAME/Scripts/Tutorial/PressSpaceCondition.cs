using UnityEngine;

public class PressSpaceCondition : TutorialCondition
{
    public override bool IsMet(TutorialStep step)
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
}