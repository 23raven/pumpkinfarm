using TMPro;
using UnityEngine;

public class InventoryHUDView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] slotLabels;

    public int SlotCount => slotLabels?.Length ?? 0;

    public void SetSlotText(int index, string text)
    {
        if (slotLabels == null)
            return;

        if (index < 0 || index >= slotLabels.Length)
            return;

        if (slotLabels[index] == null)
            return;

        slotLabels[index].text = text;
    }
}