using UnityEngine;

public class PumpkinInvButtonScript : MonoBehaviour //это скрипт кнопки в инвенторе тыкв в столе вырезания
{
    public InvItem item;
    public CarvingManager carvingManager;

    public void Select()
    {
        if (carvingManager.SelectedItem == item)
        {
            carvingManager.SelectedItem = null;
        }
        else
        {
            carvingManager.SelectedItem = item;
        }

        carvingManager.DrawSelected();
    }
}
