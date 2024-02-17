using System.Collections.Generic;
using UnityEngine;

public enum ModifierType
{
    Mouth,
    Eye,
}

[CreateAssetMenu (fileName = "Modifier", menuName = "Modifier/Modifier", order = 1)]
public class Modifier : ScriptableObject
{
    [Range (0, 1000)]
    public int OrderInUI;
    public string Name;
    public ModifierType Type;
    public StatsSerialized Stats;
    public Sprite Sprite;

    [TextArea (3, 10)]
    public string Description;

    public override string ToString ()
    {
        string s = "Modifier : " + Name;
        s += "\nType : " + Type;
        s += "\nStats : " + Stats;
        return s;
    }

    private void OnValidate ()
    {
        if (Name != name)
            Debug.LogError ("Modifier name and asset name are not the same : " + Name + " != " + name);
    }
}