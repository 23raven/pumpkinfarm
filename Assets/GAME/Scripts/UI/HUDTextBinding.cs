using TMPro;
using UnityEngine;

public enum HUDTextKey
{
    None,
    WalletBalance,
    InventorySlot1,
    InventorySlot2,
    CurrentDay
}

public class HUDTextBinding : MonoBehaviour
{
    [SerializeField] private HUDTextKey key;

    private TextMeshProUGUI label;

    public HUDTextKey Key => key;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string value)
    {
        if (label == null)
            label = GetComponent<TextMeshProUGUI>();

        if (label != null)
            label.text = value;
    }
}