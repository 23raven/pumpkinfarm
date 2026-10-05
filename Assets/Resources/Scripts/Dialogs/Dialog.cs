using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Dialog", menuName = "Scriptable Objects/Dialog")]
public class Dialog : ScriptableObject
{
    public List<Phrase> dialog = new();
}

[System.Serializable]
public class Phrase
{
    public Sprite icon;
    public string text;
    public string Name;
    public bool Player;
}
