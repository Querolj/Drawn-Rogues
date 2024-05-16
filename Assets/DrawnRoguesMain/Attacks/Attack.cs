using Sirenix.OdinInspector;
using UnityEngine;

public class Attack : ScriptableObject
{
    [SerializeField, BoxGroup ("Display")]
    private string _displayName;
    public string Name => _displayName;

    [SerializeField, BoxGroup ("Display")]
    private SpriteAnimation _animationTemplate;
    public SpriteAnimation AnimationTemplate => _animationTemplate;

    [SerializeField, BoxGroup ("Display")]
    private ParticleSystemCallback _particleTemplate;
    public ParticleSystemCallback ParticleTemplate => _particleTemplate;

    [SerializeField, BoxGroup ("Stats")]
    private bool _noDamage = false;
    public bool NoDamage => _noDamage;

    private bool DoesDamage => !_noDamage;
    [SerializeField, BoxGroup ("Stats"), ShowIf (nameof (DoesDamage))]
    private int _minDamage;
    public int MinDamage => _minDamage;

    [SerializeField, BoxGroup ("Stats"), ShowIf (nameof (DoesDamage))]
    private int _maxDamage;
    public int MaxDamage => _maxDamage;

    [SerializeField, Range (0f, 1f), BoxGroup ("Stats")]
    private float _precision = 1f;
    public float Precision => _precision;

    [SerializeField, Range (0f, 1f), BoxGroup ("Stats")]
    private float _criticalChance = 0f;
    public float CriticalChance => _criticalChance;

    [SerializeField, BoxGroup ("Stats")]
    private float _criticalMultiplier = 2f;
    public float CriticalMultiplier => _criticalMultiplier;

    [SerializeField, BoxGroup ("Stats")]
    private AttackElement _attackElement;
    public AttackElement AttackElement => _attackElement;

    [SerializeField, Range (1, 10), BoxGroup ("Stats")]
    private int _range = 1;
    private const float RANGE_TO_METER = 0.4f;

    [SerializeField, BoxGroup ("Stats")]
    private AttackSelectionType _attackSelectionType;
    public AttackSelectionType AttackSelectionType => _attackSelectionType;

    [SerializeField, BoxGroup ("Stats")]
    private AttackType _attackType;
    public AttackType AttackType => _attackType;

    [SerializeField, BoxGroup ("Stats")]
    private EffectSerialized[] _effectsSerialized;
    public EffectSerialized[] EffectsSerialized => _effectsSerialized;

    #region Required conditions
    [SerializeField, BoxGroup ("Required conditions")]
    private bool _isAvailableForPlayer = true;
    public bool IsAvailableForPlayer => _isAvailableForPlayer;

    [SerializeField, BoxGroup ("Required conditions")]
    private int _minimalLevelRequired = 0;
    public int MinimalLevelRequired => _minimalLevelRequired;

    [SerializeField, BoxGroup ("Required conditions")]
    private WidthAdjective[] _requiredWidthAdjectives;
    public WidthAdjective[] RequiredWidthAdjectives => _requiredWidthAdjectives;

    [SerializeField, BoxGroup ("Required conditions")]
    private HeightAdjective[] _requiredHeightAdjectives;
    public HeightAdjective[] RequiredHeightAdjectives => _requiredHeightAdjectives;

    [SerializeField, BoxGroup ("Required conditions")]
    private BaseColor[] _requiredBaseColor;
    public BaseColor[] RequiredBaseColor => _requiredBaseColor;

    [SerializeField, BoxGroup ("Required conditions")]
    private int _requiredNumberOfArmsMin = -1;
    public int RequiredNumberOfArmsMin => _requiredNumberOfArmsMin;

    [SerializeField, BoxGroup ("Required conditions")]
    private int _requiredNumberOfArmsMax = -1;
    public int RequiredNumberOfArmsMax => _requiredNumberOfArmsMax;

    [SerializeField, BoxGroup ("Required conditions")]
    private int _requiredNumberOfLegsMin = -1;
    public int RequiredNumberOfLegsMin => _requiredNumberOfLegsMin;

    [SerializeField, BoxGroup ("Required conditions")]
    private int _requiredNumberOfLegsMax = -1;
    public int RequiredNumberOfLegsMax => _requiredNumberOfLegsMax;
    #endregion

    public float GetRangeInMeter ()
    {
        return (float) _range * RANGE_TO_METER;
    }

    public string GetEffectsString ()
    {
        if (EffectsSerialized == null || EffectsSerialized.Length == 0)
            return string.Empty;

        string effectsString = string.Empty;
        foreach (EffectSerialized effect in EffectsSerialized)
        {
            effectsString += effect.ToString () + "\n";
        }
        return effectsString;
    }
}