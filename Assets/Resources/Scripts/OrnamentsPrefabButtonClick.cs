using UnityEngine;

public class OrnamentsPrefabButtonClick : MonoBehaviour //этот скрипт нужен для кнопки в выборе орнаментов 
{
    public CarvingManager carvingManager;
    public Ornaments Ornament;

    public void ChangeOrnament()
    {
        if (carvingManager.SelectedItem != null)
        {
            carvingManager.SelectedItem.ornament = Ornament;
        }

        carvingManager.DrawSelected();
        carvingManager.DrawOrnaments();
        carvingManager.DrawPumkinInventory();
    }
}
