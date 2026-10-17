using UnityEngine;

/// <summary>
/// Shared gate for disabling gameplay input while modal UI is open.
/// Lock() is permanent for the current scene session; only an explicit scene
/// reload reset can release it.
/// </summary>
public static class GameplayInputGate
{
    public static bool IsBlocked { get; private set; }
    public static bool IsLocked { get; private set; }

    public static void SetBlocked(bool blocked)
    {
        // Other panels call SetBlocked(false) when they close. Do not let them
        // re-enable controls after the game has ended.
        if (IsLocked && !blocked)
            return;

        IsBlocked = blocked;
    }

    public static void Lock()
    {
        IsLocked = true;
        IsBlocked = true;
    }

    /// <summary>Call immediately before intentionally reloading/changing scene.</summary>
    public static void ResetForSceneReload()
    {
        IsLocked = false;
        IsBlocked = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsLocked = false;
        IsBlocked = false;
    }
}
