using UnityEngine;

public class OrnamentsPrefabButtonClick : MonoBehaviour
{
    public CarvingManager carvingManager;
    public Ornaments Ornament;

    public void ChangeOrnament()
    {
        // Если тыква не выбрана или индекс слота потерян, ничего не делаем
        if (carvingManager.SelectedItem == null || carvingManager.selectedSlotIndex == -1) return;

        // 1. Забираем ОДНУ обычную тыкву из слота хотбара, откуда её выбрали
        var slot = carvingManager.invManager.hotbar[carvingManager.selectedSlotIndex];
        if (slot.item != null && slot.quantity > 0)
        {
            slot.quantity -= 1;

            // Если в стаке ничего не осталось, очищаем слот
            if (slot.quantity <= 0)
            {
                slot.item = null;
            }
        }

        // 2. Меняем орнамент у нашей ОДНОЙ выбранной тыквы-клона
        carvingManager.SelectedItem.ornament = Ornament;

        // 3. Добавляем эту ОДНУ вырезанную тыкву обратно в инвентарь (она займет новый слот или стакнет такие же)
        carvingManager.invManager.AddItem(carvingManager.SelectedItem, 1);

        // 4. Сбрасываем выбор, так как эта тыква успешно вырезана и сохранена
        carvingManager.SelectedItem = null;
        carvingManager.selectedSlotIndex = -1;

        // 5. Полностью обновляем все интерфейсы
        carvingManager.invManager.UpdUI();
        carvingManager.DrawSelected();
        carvingManager.DrawOrnaments();
        carvingManager.DrawPumkinInventory();
    }
}