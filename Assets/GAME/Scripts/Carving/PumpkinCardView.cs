using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Карточка тыквы в левом списке окна резьбы (префаб). Одна карточка = один слот инвентаря.
public class PumpkinCardView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject selectedMarker; // рамка/подсветка, можно не задавать

    private int slotIndex;
    private Action<int> onClick;

    public void Bind(int slotIndex, ItemDefinition item, int quantity, bool selected, Action<int> onClick)
    {
        this.slotIndex = slotIndex;
        this.onClick = onClick;

        if (iconImage != null)
        {
            iconImage.sprite = item != null ? item.Icon : null;
            iconImage.enabled = iconImage.sprite != null;
        }
        if (nameText != null) nameText.text = item != null ? item.DisplayName : string.Empty;
        if (countText != null) countText.text = quantity > 1 ? "x" + quantity : string.Empty;
        if (selectedMarker != null) selectedMarker.SetActive(selected);

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        if (onClick != null) onClick(slotIndex);
    }
}
