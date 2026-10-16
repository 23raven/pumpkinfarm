using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Кнопка выбора лица в правом меню изменения (префаб).
public class PumpkinFaceButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private GameObject currentMarker; // "сейчас выбрано", можно не задавать

    private PumpkinFace face;
    private Action<PumpkinFace> onClick;

    public PumpkinFace Face => face;

    public void Bind(PumpkinFace face, string label, Sprite icon, Action<PumpkinFace> onClick)
    {
        this.face = face;
        this.onClick = onClick;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }
        if (labelText != null) labelText.text = label;

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
    }

    // Текущее лицо тыквы подсвечиваем и делаем некликабельным
    public void SetCurrent(bool isCurrent)
    {
        if (currentMarker != null) currentMarker.SetActive(isCurrent);
        if (button != null) button.interactable = !isCurrent;
    }

    private void HandleClick()
    {
        if (onClick != null) onClick(face);
    }
}
