using UnityEngine;

public class CloseCarvingUI : MonoBehaviour
{
    [SerializeField] private GameObject carvingUI;

    public void Close()
    {
        if (carvingUI != null)
            carvingUI.SetActive(false);
    }
}