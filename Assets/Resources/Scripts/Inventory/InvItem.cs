using UnityEngine;

[CreateAssetMenu(fileName = "InvItem", menuName = "Scriptable Objects/InvItem")]
public class InvItem : ScriptableObject
{
    public Sprite icon;
    public string Itemname;
    public string description;
}
