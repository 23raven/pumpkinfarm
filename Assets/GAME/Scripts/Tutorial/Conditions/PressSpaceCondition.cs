using UnityEngine;

public class PressSpaceCondition : TutorialCondition
{
    private int firstCheckFrame = -1;

    public override bool IsMet(TutorialStep step)
    {
        // Ignore the first frame in which this condition is checked.
        // This prevents the previous step's Space press from counting.
        if (firstCheckFrame == -1)
        {
            firstCheckFrame = Time.frameCount;
            return false;
        }

        if (Time.frameCount == firstCheckFrame)
            return false;

        return Input.GetKeyDown(KeyCode.Space);
    }
}