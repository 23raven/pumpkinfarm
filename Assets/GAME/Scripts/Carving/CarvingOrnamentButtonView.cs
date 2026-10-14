using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Кнопка орнамента / цвета / декорации (префаб). Заменяет OrnamentsPrefabButtonClick.
public class CarvingOrnamentButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text labelText;

    private Ornament ornament;
    private Action<Ornament> onClick;

    public Ornament Ornament => ornament;

    public void Bind(Ornament ornament, Action<Ornament> onClick)
    {
        this.ornament = ornament;
        this.onClick = onClick;

        if (iconImage != null)
        {
            iconImage.sprite = ornament != null ? ornament.icon : null;
            iconImage.enabled = iconImage.sprite != null;
        }
        if (labelText != null)
            labelText.text = ornament != null ? ornament.GetLabel() : string.Empty;

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
    }

    public void SetInteractable(bool value)
    {
        if (button != null) button.interactable = value;
    }

    private void HandleClick()
    {
        if (onClick != null) onClick(ornament);
    }
}
