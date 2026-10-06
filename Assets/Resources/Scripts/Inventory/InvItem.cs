using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

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
    public List<Tags> tags = new();
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

public enum Tags //сделал специально тэги чтобы можно было выделять обьекты например в системе вырезания чтобы показывало только тыквы
{
    Pumkin,
    Item
}
