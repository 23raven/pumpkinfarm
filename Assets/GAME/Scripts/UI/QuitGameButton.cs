using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitGameButton : MonoBehaviour
{
    public void QuitGame()
    {
        GameplayInputGate.SetBlocked(false);
        SceneManager.LoadScene("MainMenu");
    }
}