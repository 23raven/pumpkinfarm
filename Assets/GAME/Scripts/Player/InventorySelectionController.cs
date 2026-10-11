using UnityEngine;

public class InventorySelectionController : MonoBehaviour
{
    private static readonly KeyCode[] NumberKeys =
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
        KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
    };

    private static readonly KeyCode[] KeypadKeys =
    {
        KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3,
        KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6,
        KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9
    };

    private void Update()
    {
        if (GameplayInputGate.IsBlocked)
            return;

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<InventoryService>(out InventoryService inventory))
        {
            return;
        }

        for (int i = 0; i < NumberKeys.Length; i++)
        {
            if (Input.GetKeyDown(NumberKeys[i]) || Input.GetKeyDown(KeypadKeys[i]))
            {
                inventory.SelectSlot(i);
                return;
            }
        }

        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f)
            inventory.SelectNextAccessibleSlot(-1);
        else if (scroll < 0f)
            inventory.SelectNextAccessibleSlot(1);
    }
}
