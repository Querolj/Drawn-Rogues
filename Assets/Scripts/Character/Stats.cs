using System;
using System.Collections.Generic;
using UnityEngine;

public enum MainStatType
{
    Life,
    Strength,
    Intelligence,
    Mobility
}

public class Stats
{
    #region Main stats
    private int _life;
    public int Life
    {
        get
        {
            return AlterMainStatMethod (_lifeModifiersById, _life);
        }
        set
        {
            _life = value;
        }
    }
    private Dictionary < int, (OperationTypeEnum, float) > _lifeModifiersById = new Dictionary < int, (OperationTypeEnum, float) > ();

    private int _intelligence;
    public int Intelligence
    {
        get
        {
            return AlterMainStatMethod (_intelligenceModifiersById, _intelligence);
        }
        set
        {
            _intelligence = value;
        }
    }
    private Dictionary < int, (OperationTypeEnum, float) > _intelligenceModifiersById = new Dictionary < int, (OperationTypeEnum, float) > ();

    private int _strenght;
    public int Strenght
    {
        get
        {
            return AlterMainStatMethod (_strenghtModifiersById, _strenght);
        }
        set
        {
            _strenght = value;
        }
    }
    private Dictionary < int, (OperationTypeEnum, float) > _strenghtModifiersById = new Dictionary < int, (OperationTypeEnum, float) > ();

    private int _mobility;
    public int Mobility
    {
        get
        {
            return AlterMainStatMethod (_mobilityModifiersById, _mobility);
        }
        set
        {
            _mobility = value;
        }
    }
    private Dictionary < int, (OperationTypeEnum, float) > _mobilityModifiersById = new Dictionary < int, (OperationTypeEnum, float) > ();

    private int AlterMainStatMethod (Dictionary < int, (OperationTypeEnum, float) > modifiersById, int valueToAlter)
    {
        foreach ((OperationTypeEnum, float) mainStatModifier in GetSortedListOfMobilityModifiers (modifiersById))
        {
            switch (mainStatModifier.Item1)
            {
                case OperationTypeEnum.Add:
                    valueToAlter += (int) mainStatModifier.Item2;
                    break;
                case OperationTypeEnum.AddPercentage:
                    valueToAlter += (int) (valueToAlter * (mainStatModifier.Item2 / 100f));
                    break;
                case OperationTypeEnum.Substract:
                    valueToAlter -= (int) mainStatModifier.Item2;
                    break;
                case OperationTypeEnum.Set:
                    valueToAlter = (int) mainStatModifier.Item2;
                    break;
                case OperationTypeEnum.PercentageResistance:
                    valueToAlter = (int) (valueToAlter * (1f - (mainStatModifier.Item2 / 100f)));
                    break;
            }
        }

        return valueToAlter;
    }

    private List < (OperationTypeEnum, float) > GetSortedListOfMobilityModifiers (Dictionary < int, (OperationTypeEnum, float) > modifiersById)
    {
        List < (OperationTypeEnum, float) > modifiers = new List < (OperationTypeEnum, float) > ();
        foreach (KeyValuePair < int, (OperationTypeEnum, float) > mobilityModifier in modifiersById)
        {
            modifiers.Add (mobilityModifier.Value);
        }
        modifiers.Sort ((x, y) => x.Item1.CompareTo (y.Item1));
        return modifiers;
    }

    public void AddMainStatModifier (int id, MainStatType mainStatType, OperationTypeEnum operationType, float value)
    {
        switch (mainStatType)
        {
            case MainStatType.Life:
                AddMainStatModifierInternal (id, operationType, value, _lifeModifiersById);
                break;
            case MainStatType.Intelligence:
                AddMainStatModifierInternal (id, operationType, value, _intelligenceModifiersById);
                break;
            case MainStatType.Strength:
                AddMainStatModifierInternal (id, operationType, value, _strenghtModifiersById);
                break;
            case MainStatType.Mobility:
                AddMainStatModifierInternal (id, operationType, value, _mobilityModifiersById);
                break;
        }
    }

    private void AddMainStatModifierInternal (int id, OperationTypeEnum operationType, float value, Dictionary < int, (OperationTypeEnum, float) > modifiersById)
    {
        if (modifiersById.ContainsKey (id))
        {
            Debug.LogWarning ("Modifier with id " + id + " already exists, overwriting it");
            modifiersById[id] = (operationType, value);
        }
        else
        {
            modifiersById.Add (id, (operationType, value));
        }
    }

    public void RemoveMainStatModifier (int id, MainStatType mainStatType)
    {
        switch (mainStatType)
        {
            case MainStatType.Life:
                RemoveMainStatModifierInternal (id, _lifeModifiersById);
                break;
            case MainStatType.Intelligence:
                RemoveMainStatModifierInternal (id, _intelligenceModifiersById);
                break;
            case MainStatType.Strength:
                RemoveMainStatModifierInternal (id, _strenghtModifiersById);
                break;
            case MainStatType.Mobility:
                RemoveMainStatModifierInternal (id, _mobilityModifiersById);
                break;
        }
    }

    private void RemoveMainStatModifierInternal (int id, Dictionary < int, (OperationTypeEnum, float) > modifiersById)
    {
        if (modifiersById.ContainsKey (id))
        {
            modifiersById.Remove (id);
        }
        else
        {
            Debug.LogWarning ("Mobility modifier with id " + id + " doesn't exist");
        }
    }

    #endregion

    private Dictionary<string, AttackDefPassive> _attackDefPassiveByNames = new Dictionary<string, AttackDefPassive> ();
    public Dictionary<string, AttackDefPassive> AttackDefPassiveByNames
    {
        get { return _attackDefPassiveByNames; }
    }

    private Dictionary<string, AttackOffPassive> _attackoffPassiveByNames = new Dictionary<string, AttackOffPassive> ();
    public Dictionary<string, AttackOffPassive> AttackOffPassiveByNames
    {
        get { return _attackoffPassiveByNames; }
    }

    private Dictionary<string, EffectDefPassive> _effectDefPassiveByNames = new Dictionary<string, EffectDefPassive> ();
    public Dictionary<string, EffectDefPassive> EffectDefPassiveByNames
    {
        get { return _effectDefPassiveByNames; }
    }

    private Dictionary<string, EffectOffPassive> _effectOffPassiveByNames = new Dictionary<string, EffectOffPassive> ();
    public Dictionary<string, EffectOffPassive> EffectOffPassiveByNames
    {
        get { return _effectOffPassiveByNames; }
    }

    private Dictionary<string, Effect> _EffectByNames = new Dictionary<string, Effect> ();
    public Dictionary<string, Effect> EffectByNames
    {
        get { return _EffectByNames; }
    }

    private float _kilogram = 0f;
    public float Kilogram
    {
        get { return _kilogram; }
        set { _kilogram = value; }
    }

    public Stats () { }

    public Stats (int life, int intelligence, int strenght, int mobility, float kilogram)
    {
        _life = life;
        _intelligence = intelligence;
        _strenght = strenght;
        _mobility = mobility;
        _kilogram = kilogram;
    }

    public void Add (CharacterColouring charColouring, PixelUsage pixelUsage, int multiplicator)
    {
        if (charColouring == null)
        {
            throw new ArgumentNullException (nameof (charColouring));
        }

        if (charColouring.BaseBonusToMainStats)
        {
            AddMainStat (pixelUsage, multiplicator);
        }

        AddStats (charColouring.Stats, multiplicator);
    }

    private void AddMainStat (PixelUsage pixelUsage, int value)
    {
        switch (pixelUsage)
        {
            case PixelUsage.Head:
                Intelligence += value;
                break;
            case PixelUsage.Body:
                Life += value;
                break;
            case PixelUsage.Arm:
                Strenght += value;
                break;
            case PixelUsage.Leg:
                Mobility += value;
                break;
        }
    }

    private void OverwriteMainStat (PixelUsage pixelUsage, int value)
    {
        switch (pixelUsage)
        {
            case PixelUsage.Head:
                Intelligence = value;
                break;
            case PixelUsage.Body:
                Life = value;
                break;
            case PixelUsage.Arm:
                Strenght = value;
                break;
            case PixelUsage.Leg:
                Mobility = value;
                break;
        }
    }

    private void AddToPassiveDict<T> (Dictionary<string, T> passiveDict, PassiveSerialized<T> passiveSer, int multiplicator) where T : Passive
    {
        if (passiveDict.ContainsKey (passiveSer.Passive.Name))
        {
            passiveDict[passiveSer.Passive.Name].Add (passiveSer.Value * multiplicator);
        }
        else
        {
            T passiveInstance = passiveSer.GetInstance ();
            passiveInstance.SetValue (passiveSer.Value * multiplicator);
            passiveDict.Add (passiveSer.Passive.Name, passiveInstance);
        }
    }

    private void OverwriteToPassiveDict<T> (Dictionary<string, T> passiveDict, PassiveSerialized<T> passiveSer, int multiplicator) where T : Passive
    {
        if (passiveDict.ContainsKey (passiveSer.Passive.Name))
        {
            passiveDict[passiveSer.Passive.Name].SetValue (passiveSer.Value * multiplicator);
        }
        else
        {
            T passiveInstance = passiveSer.GetInstance ();
            passiveInstance.SetValue (passiveSer.Value * multiplicator);
            passiveDict.Add (passiveSer.Passive.Name, passiveInstance);
        }
    }

    public void AddStats (StatsSerialized statsSerialized, int multiplicator = 1)
    {
        foreach (AttackDefPassiveSerialized attackdefPassiveSerialized in statsSerialized?.AttackDefPassiveValues)
        {
            AddToPassiveDict (_attackDefPassiveByNames, attackdefPassiveSerialized, multiplicator);
        }

        foreach (AttackOffPassiveSerialized attackOffPassiveSerialized in statsSerialized?.AttackOffPassiveValues)
        {
            AddToPassiveDict (_attackoffPassiveByNames, attackOffPassiveSerialized, multiplicator);
        }

        foreach (EffectDefPassiveSerialized effectDefPassiveSerialized in statsSerialized?.EffectDefPassiveValues)
        {
            AddToPassiveDict (_effectDefPassiveByNames, effectDefPassiveSerialized, multiplicator);
        }

        foreach (EffectOffPassiveSerialized effectOffPassiveSerialized in statsSerialized?.EffectOffPassiveValues)
        {
            AddToPassiveDict (_effectOffPassiveByNames, effectOffPassiveSerialized, multiplicator);
        }

        foreach (EffectSerialized effectSerialized in statsSerialized?.effectValues)
        {
            string effectName = effectSerialized.Effect.Name;
            if (_EffectByNames.ContainsKey (effectName))
            {
                _EffectByNames[effectName].AddToInitialValue (effectSerialized.Value * multiplicator);
            }
            else
            {
                Effect effect = effectSerialized.GetInstance ();
                effect.SetInitialValue (effectSerialized.Value * multiplicator);
                _EffectByNames.Add (effectSerialized.Effect.Name, effect);
            }
        }
    }

    private void OverwriteStat (StatsSerialized statsSerialized, int multiplicator = 1)
    {
        foreach (AttackDefPassiveSerialized attackdefPassiveSerialized in statsSerialized.AttackDefPassiveValues)
        {
            OverwriteToPassiveDict (_attackDefPassiveByNames, attackdefPassiveSerialized, multiplicator);
        }

        foreach (AttackOffPassiveSerialized attackOffPassiveSerialized in statsSerialized.AttackOffPassiveValues)
        {
            OverwriteToPassiveDict (_attackoffPassiveByNames, attackOffPassiveSerialized, multiplicator);
        }

        foreach (EffectDefPassiveSerialized effectDefPassiveSerialized in statsSerialized.EffectDefPassiveValues)
        {
            OverwriteToPassiveDict (_effectDefPassiveByNames, effectDefPassiveSerialized, multiplicator);
        }

        foreach (EffectOffPassiveSerialized effectOffPassiveSerialized in statsSerialized.EffectOffPassiveValues)
        {
            OverwriteToPassiveDict (_effectOffPassiveByNames, effectOffPassiveSerialized, multiplicator);
        }

        foreach (EffectSerialized effectSerialized in statsSerialized.effectValues)
        {
            string effectName = effectSerialized.Effect.Name;
            if (_EffectByNames.ContainsKey (effectName))
            {
                _EffectByNames[effectName].SetInitialValue (effectSerialized.Value * multiplicator);
            }
            else
            {
                Effect effect = effectSerialized.GetInstance ();
                effect.SetInitialValue (effectSerialized.Value * multiplicator);
                _EffectByNames.Add (effectSerialized.Effect.Name, effect);
            }
        }
    }

    public override string ToString ()
    {
        string s = "Life : " + Life + "\nIntelligence : " + Intelligence + "\nStrenght : " + Strenght + "\nMobility : " + Mobility;

        foreach (KeyValuePair<string, Effect> stat in _EffectByNames)
        {
            s += "\n" + stat.Value;
        }

        foreach (KeyValuePair<string, AttackOffPassive> stat in _attackoffPassiveByNames)
        {
            s += "\n" + stat.Value;
        }

        foreach (KeyValuePair<string, AttackDefPassive> stat in _attackDefPassiveByNames)
        {
            s += "\n" + stat.Value;
        }

        foreach (KeyValuePair<string, EffectOffPassive> stat in _effectOffPassiveByNames)
        {
            s += "\n" + stat.Value;
        }

        foreach (KeyValuePair<string, EffectDefPassive> stat in _effectDefPassiveByNames)
        {
            s += "\n" + stat.Value;
        }

        return s;
    }
}

[Serializable]
public class StatsSerialized
{
    public List<AttackDefPassiveSerialized> AttackDefPassiveValues = new List<AttackDefPassiveSerialized> ();
    public List<AttackOffPassiveSerialized> AttackOffPassiveValues = new List<AttackOffPassiveSerialized> ();
    public List<EffectDefPassiveSerialized> EffectDefPassiveValues = new List<EffectDefPassiveSerialized> ();
    public List<EffectOffPassiveSerialized> EffectOffPassiveValues = new List<EffectOffPassiveSerialized> ();
    public List<EffectSerialized> effectValues = new List<EffectSerialized> ();

    public bool HasAnyStats ()
    {
        return AttackDefPassiveValues?.Count > 0 || AttackOffPassiveValues?.Count > 0 || effectValues?.Count > 0;
    }
}