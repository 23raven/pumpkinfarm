using UnityEngine;

public class OrnamentsPrefabButtonClick : MonoBehaviour
{
    public CarvingManager carvingManager;
    public Ornament Ornament;

    public void ChangeOrnament()
    {
        if (carvingManager.SelectedItem == null || carvingManager.selectedSlotIndex == -1) return;

        var slot = carvingManager.invManager.hotbar[carvingManager.selectedSlotIndex];
        if (slot.item != null && slot.quantity > 0)
        {
            slot.quantity -= 1;

            if (slot.quantity <= 0)
            {
                slot.item = null;
            }
        }

        if (Ornament.editType == EditType.Ornament)
        {
            carvingManager.SelectedItem.ornament = Ornament.ornament;
        }
        else if (Ornament.editType == EditType.Color)
        {
            carvingManager.SelectedItem.color = Ornament.pumpkinColor;
        }
        else if (Ornament.editType == EditType.Decoration)
        {
            carvingManager.SelectedItem.decoration = Ornament.decoration;
        }


            carvingManager.invManager.AddItem(carvingManager.SelectedItem, 1);

        carvingManager.SelectedItem = null;
        carvingManager.selectedSlotIndex = -1;

        carvingManager.invManager.UpdUI();
        carvingManager.DrawSelected();
        carvingManager.DrawOrnaments();
        carvingManager.DrawPumkinInventory();
    }
}