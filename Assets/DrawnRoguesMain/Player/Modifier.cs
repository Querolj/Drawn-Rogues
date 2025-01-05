using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum ModifierType
{
    Mouth,
    Eye,
}

[CreateAssetMenu (fileName = "Modifier", menuName = "Modifier/Modifier", order = 1)]
public class Modifier : ScriptableObject
{
    [SerializeField, InfoBox ("Modifier name and asset name are not the same", InfoMessageType.Error, nameof (IsNameDifferentFromAssetName)), BoxGroup ("Display")]
    private string _displayName;
    public string DisplayName => _displayName;

    [SerializeField, BoxGroup ("Display")]
    private Sprite _sprite;
    public Sprite Sprite => _sprite;

    [SerializeField, Range (0, 1000), BoxGroup ("Display")]
    private int _orderInUI;
    public int OrderInUI => _orderInUI;

    [SerializeField, BoxGroup ("Stats")]
    private ModifierType _type;
    public ModifierType Type => _type;

    [SerializeField, BoxGroup ("Stats")]
    private StatsSerialized _stats;
    public StatsSerialized Stats => _stats;

    

    public string GetStatsDescription ()
    {
        string text = string.Empty;
        bool lineJump = false;

        foreach (EffectSerialized effect in Stats.effectValues)
        {
            text += effect.ToString (effect.Value);
            lineJump = true;
        }

        foreach (MainStatsPassiveSerialized mainStatsPassive in Stats.MainStatsPassiveValues)
        {
            if (lineJump)
                text += "\n";
            text += mainStatsPassive.Passive.ToString (mainStatsPassive.Value);
            lineJump = true;
        }

        foreach (AttackDefPassiveSerialized attackDefPassive in Stats.AttackDefPassiveValues)
        {
            if (lineJump)
                text += "\n";
            text += attackDefPassive.Passive.ToString (attackDefPassive.Value);
            lineJump = true;
        }

        foreach (AttackOffPassiveSerialized attackOffPassive in Stats.AttackOffPassiveValues)
        {
            if (lineJump)
                text += "\n";
            text += attackOffPassive.Passive.ToString (attackOffPassive.Value);
            lineJump = true;
        }

        foreach (EffectDefPassiveSerialized effectDefPassive in Stats.EffectDefPassiveValues)
        {
            if (lineJump)
                text += "\n";
            text += effectDefPassive.Passive.ToString (effectDefPassive.Value);
            lineJump = true;
        }

        foreach (EffectOffPassiveSerialized effectOffPassive in Stats.EffectOffPassiveValues)
        {
            if (lineJump)
                text += "\n";
            text += effectOffPassive.Passive.ToString (effectOffPassive.Value);
            lineJump = true;
        }

        foreach(MiscPassiveSerialized miscPassive in Stats.MiscPassiveValues)
        {
            if (lineJump)
                text += "\n";
            text += miscPassive.Passive.ToString (miscPassive.Value);
            lineJump = true;
        }

        return text;
    }

    public override string ToString ()
    {
        string s = "Modifier : " + DisplayName;
        s += "\nType : " + Type;
        s += "\nStats : " + Stats;
        return s;
    }

    private bool IsNameDifferentFromAssetName ()
    {
        return DisplayName != name;
    }
}