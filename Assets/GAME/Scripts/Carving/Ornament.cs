using System;
using UnityEngine;

// Одна "опция" в окне резьбы: что игрок может применить к тыкве.
// В зависимости от editType используется только одно из полей:
// ornament, pumpkinColor или decoration.
[Serializable]
public class Ornament
{
    public Sprite icon;
    public EditType editType;
    public Ornaments ornament;
    public PumpkinColor pumpkinColor;
    public Decoration decoration;

    // Одинаковое ли это изменение (тот же тип и то же значение)
    public bool SameChangeAs(Ornament other)
    {
        if (other == null || editType != other.editType)
            return false;

        switch (editType)
        {
            case EditType.Ornament: return ornament == other.ornament;
            case EditType.Color: return pumpkinColor == other.pumpkinColor;
            case EditType.Decoration: return decoration == other.decoration;
            default: return false;
        }
    }

    // Текст для кнопки
    public string GetLabel()
    {
        switch (editType)
        {
            case EditType.Ornament: return ornament.ToString();
            case EditType.Color: return pumpkinColor.ToString();
            case EditType.Decoration: return decoration.ToString();
            default: return string.Empty;
        }
    }
}

public enum EditType
{
    Ornament,
    Color,
    Decoration
}
