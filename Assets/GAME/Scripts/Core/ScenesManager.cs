using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public void ChangeScene(int id)
    {
        SceneManager.LoadScene(id);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
