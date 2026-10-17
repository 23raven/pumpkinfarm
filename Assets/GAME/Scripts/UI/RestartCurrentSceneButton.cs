using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Call RestartCurrentScene() from the UI Button's On Click event.</summary>
public sealed class RestartCurrentSceneButton : MonoBehaviour
{
    private static bool waitingForSceneLoad;

    public void RestartCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex < 0)
        {
            Debug.LogError(
                "Cannot restart the active scene because it is not included in Build Settings / Build Profiles.",
                this);
            return;
        }

        // Restore components and UI that the ending screen hid/disabled.
        GameEndingPanelController.RestoreGameplayForSceneReload();
        GameplayInputGate.ResetForSceneReload();
        GameServices.PrepareForSceneReload();

        // Use a static callback: this button's GameObject is normally destroyed
        // during LoadScene, so an instance callback could be unsubscribed early.
        if (!waitingForSceneLoad)
        {
            waitingForSceneLoad = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        SceneManager.LoadScene(activeScene.buildIndex, LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        waitingForSceneLoad = false;

        // Reset again after Awake/OnEnable ran for the new scene. If its storage
        // goal is still satisfied, StorageCollectionGameCondition will correctly
        // trigger the ending again from Start.
        GameEndingPanelController.RestoreGameplayForSceneReload();
        GameplayInputGate.ResetForSceneReload();
    }
}
