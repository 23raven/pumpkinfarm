using UnityEngine;

public class Npc2 : MonoBehaviour, IInteractable
{
    [Header("Dialog")]
    [SerializeField] private DialogRenderer dialogRenderer;
    [SerializeField] private Dialog dialog;

    [Header("UI Panels To Hide")]
    [SerializeField] private GameObject[] uiPanelsToHide;

    private bool[] previousPanelStates;
    private bool restorePanelsAfterDialog;

    public void Interact(GameObject interactor)
    {
        if (dialogRenderer == null || dialog == null)
            return;

        if (dialogRenderer.IsOpen || GameplayInputGate.IsBlocked)
            return;

        HidePanels();

        dialogRenderer.Current_dialog = dialog;
        dialogRenderer.CurrentPhrase = 0;
        dialogRenderer.ShowDialog();

        if (dialogRenderer.IsOpen)
            restorePanelsAfterDialog = true;
        else
            RestorePanels();
    }

    private void Update()
    {
        if (restorePanelsAfterDialog &&
            (dialogRenderer == null || !dialogRenderer.IsOpen))
        {
            RestorePanels();
        }
    }

    private void HidePanels()
    {
        if (uiPanelsToHide == null)
            return;

        previousPanelStates = new bool[uiPanelsToHide.Length];

        for (int i = 0; i < uiPanelsToHide.Length; i++)
        {
            GameObject panel = uiPanelsToHide[i];

            if (panel == null)
                continue;

            previousPanelStates[i] = panel.activeSelf;
            panel.SetActive(false);
        }
    }

    private void RestorePanels()
    {
        if (uiPanelsToHide != null && previousPanelStates != null)
        {
            for (int i = 0; i < uiPanelsToHide.Length; i++)
            {
                if (uiPanelsToHide[i] != null)
                    uiPanelsToHide[i].SetActive(previousPanelStates[i]);
            }
        }

        previousPanelStates = null;
        restorePanelsAfterDialog = false;
    }
}