using UnityEngine;

public class InventorySelectionController : MonoBehaviour
{
    private static readonly KeyCode[] NumberKeys =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9
    };

    private static readonly KeyCode[] KeypadKeys =
    {
        KeyCode.Keypad1,
        KeyCode.Keypad2,
        KeyCode.Keypad3,
        KeyCode.Keypad4,
        KeyCode.Keypad5,
        KeyCode.Keypad6,
        KeyCode.Keypad7,
        KeyCode.Keypad8,
        KeyCode.Keypad9
    };

    private void Update()
    {
        if (GameplayInputGate.IsBlocked)
            return;

        int selectedIndex = -1;

        for (int i = 0; i < NumberKeys.Length; i++)
        {
            if (Input.GetKeyDown(NumberKeys[i]) ||
                Input.GetKeyDown(KeypadKeys[i]))
            {
                selectedIndex = i;
                break;
            }
        }

        if (selectedIndex < 0)
            return;

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<InventoryService>(
                out InventoryService inventory))
        {
            return;
        }

        inventory.SelectSlot(selectedIndex);
    }
}