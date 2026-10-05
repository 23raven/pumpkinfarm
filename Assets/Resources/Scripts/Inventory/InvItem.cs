using UnityEngine;

[CreateAssetMenu(fileName = "InvItem", menuName = "Scriptable Objects/InvItem")]
public class InvItem : ScriptableObject
{
    public Sprite icon;
    public string Itemname;
    public string description;
    public bool selleable;
    public int priceSell;
    public int priceBuy;
    public Ornaments ornament;
}

public enum Ornaments //это типо орнаменты для вырезания по тыкве
{
    CatFace,
    ClassicFace,
    CreepyFace,
    PsychoFace,
    SchockedFace,
    WinkingFace,
    PukingFace,
    SpititFace,
    NoFace
}
