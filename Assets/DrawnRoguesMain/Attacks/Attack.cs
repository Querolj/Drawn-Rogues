using UnityEngine;

public class Attack : ScriptableObject
{
    public string Name;
    public SpriteAnimation AnimationTemplate;
    public ParticleSystemCallback ParticleTemplate;
    public int MinDamage;
    public int MaxDamage;
    public bool NoDamage = false;
    [Range (0f, 100f)]
    public float Precision = 100;
    public DamageType DamageType;

    [SerializeField, Range (1, 10)]
    private int _range = 1;
    private const float RANGE_TO_METER = 0.4f;

    public AttackSelectionType AttackSelectionType;
    public AttackType AttackType;
    public EffectSerialized[] EffectsSerialized;

    #region Required conditions
    public bool AvailableForPlayer = true;
    public int RequiredLevel = 0;
    public WidthAdjective[] RequiredWidthAdjectives;
    public HeightAdjective[] RequiredHeightAdjectives;
    public BaseColor[] RequiredBaseColor;
    public int RequiredNumberOfArmsMin = -1;
    public int RequiredNumberOfArmsMax = -1;
    public int RequiredNumberOfLegsMin = -1;
    public int RequiredNumberOfLegsMax = -1;
    #endregion

    public float GetRangeInMeter ()
    {
        return (float) _range * RANGE_TO_METER;
    }

    public string GetEffectsString ()
    {
        string effectsString = "";
        foreach (EffectSerialized effect in EffectsSerialized)
        {
            effectsString += effect.ToString () + "\n";
        }
        return effectsString;
    }
}