using UnityEngine;

public class InventorySelectionController : MonoBehaviour
{
    private void Update()
    {
        bool slot1Pressed =
            Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1);

        bool slot2Pressed =
            Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Keypad2);

        if (!slot1Pressed && !slot2Pressed)
            return;

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<InventoryService>(
                out InventoryService inventory))
        {
            Debug.LogWarning("InventoryService is unavailable.");
            return;
        }

        if (slot1Pressed)
        {
            inventory.SelectSlot(0);
        }
        else if (slot2Pressed)
        {
            inventory.SelectSlot(1);
        }
    }
}