using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MainStatType
{
    Life,
    Strength,
    Intelligence,
    Mobility
}

public class AttackableStats
{
    #region Main stats
    private int _baseLife;
    public int Life
    {
        get
        {
            AlterMainStatMethod (_lifeModifiersById, ref _baseLife);
            return _baseLife;
        }
        set
        {
            _baseLife = value;
            _attackableState = new AttackableState (Life);
        }
    }
    private Dictionary < int, (OperationTypeEnum, float) > _lifeModifiersById = new Dictionary < int, (OperationTypeEnum, float) > ();
    private AttackableState _attackableState = null;
    public AttackableState AttackableState
    {
        get
        {
            if (_attackableState == null)
                _attackableState = new AttackableState (Life);
            return _attackableState;
        }
    }

    private int _baseIntelligence;
    public int Intelligence
    {
        get
        {
            AlterMainStatMethod (_intelligenceModifiersById, ref _baseIntelligence);
            return _baseIntelligence;
        }
        set
        {
            _baseIntelligence = value;
        }
    }
    private Dictionary < int, (OperationTypeEnum, float) > _intelligenceModifiersById = new Dictionary < int, (OperationTypeEnum, float) > ();

    private int _baseStrenght;
    public int Strenght
    {
        get
        {
            AlterMainStatMethod (_strenghtModifiersById, ref _baseStrenght);
            return _baseStrenght;
        }
        set
        {
            _baseStrenght = value;
        }
    }
    private Dictionary < int, (OperationTypeEnum, float) > _strenghtModifiersById = new Dictionary < int, (OperationTypeEnum, float) > ();

    private int _baseMobility;
    public int Mobility
    {
        get
        {
            AlterMainStatMethod (_mobilityModifiersById, ref _baseMobility);
            return _baseMobility;
        }
        set
        {
            _baseMobility = value;
        }
    }
    private Dictionary < int, (OperationTypeEnum, float) > _mobilityModifiersById = new Dictionary < int, (OperationTypeEnum, float) > ();

    private void AlterMainStatMethod (Dictionary < int, (OperationTypeEnum, float) > modifiersById, ref int valueToAlter)
    {
        foreach ((OperationTypeEnum, float) mainStatModifier in GetSortedListOfModifiers (modifiersById))
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
    }

    private List < (OperationTypeEnum, float) > GetSortedListOfModifiers (Dictionary < int, (OperationTypeEnum, float) > modifiersById)
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
                AddMainStatModifierInternal (id, operationType, value, ref _lifeModifiersById);
                break;
            case MainStatType.Intelligence:
                AddMainStatModifierInternal (id, operationType, value, ref _intelligenceModifiersById);
                break;
            case MainStatType.Strength:
                AddMainStatModifierInternal (id, operationType, value, ref _strenghtModifiersById);
                break;
            case MainStatType.Mobility:
                AddMainStatModifierInternal (id, operationType, value, ref _mobilityModifiersById);
                break;
        }
    }

    private void AddMainStatModifierInternal (int id, OperationTypeEnum operationType, float value, ref Dictionary < int, (OperationTypeEnum, float) > modifiersById)
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

    private Dictionary<string, Effect> _effectByNames = new Dictionary<string, Effect> ();
    public Dictionary<string, Effect> EffectByNames
    {
        get { return _effectByNames; }
    }

    // public Dictionary<string, Effect> EffectByNamesCopy
    // {
    //     get
    //     {
    //         Dictionary<string, Effect> effectsInstByNames = new Dictionary<string, Effect> ();
    //         foreach (KeyValuePair<string, Effect> effectByName in _effectByNames)
    //         {
    //             effectsInstByNames.Add (effectByName.Key, effectByName.Value.GetCopy ());
    //         }

    //         return effectsInstByNames;
    //     }
    // }

    private float _kilogram = 0f;
    public float Kilogram
    {
        get { return _kilogram; }
        set { _kilogram = value; }
    }

    public AttackableStats () { }

    public AttackableStats (int life, int intelligence, int strenght, int mobility, float kilogram)
    {
        _baseLife = life;
        _baseIntelligence = intelligence;
        _baseStrenght = strenght;
        _baseMobility = mobility;
        _kilogram = kilogram;
    }

    public AttackableStats (Dictionary < (int, PixelUsage), int > pixelUsageByIds)
    {
        foreach ((int id, PixelUsage colorUsage) in pixelUsageByIds.Keys)
        {
            int pixCount = pixelUsageByIds[(id, colorUsage)];
            if (pixCount <= 0)
                continue;
            try
            {
                if (CharColouringRegistry.Instance.ColouringsSourceById.ContainsKey (id))
                {
                    if (CharColouringRegistry.Instance.ColouringsSourceById[id] is CharacterColouring characterColouring)
                    {
                        Add (characterColouring, colorUsage, pixCount);
                        _kilogram += pixCount * characterColouring.KilogramPerPixel;
                    }
                    else
                        throw new Exception ("Colouring" + CharColouringRegistry.Instance.ColouringsSourceById[id].Name + " is not a CharacterColouring");
                }
            }
            catch (Exception e)
            {
                throw new Exception (pixCount + " : " + e);
            }
        }
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
        if (passiveDict.ContainsKey (passiveSer.Passive.Description))
        {
            passiveDict[passiveSer.Passive.Description].Add (passiveSer.Value * multiplicator);
        }
        else
        {
            T passiveInstance = passiveSer.GetInstance ();
            passiveInstance.SetValue (passiveSer.Value * multiplicator);
            passiveDict.Add (passiveSer.Passive.Description, passiveInstance);
        }
    }

    private void OverwriteToPassiveDict<T> (Dictionary<string, T> passiveDict, PassiveSerialized<T> passiveSer, int multiplicator) where T : Passive
    {
        if (passiveDict.ContainsKey (passiveSer.Passive.Description))
        {
            passiveDict[passiveSer.Passive.Description].SetValue (passiveSer.Value * multiplicator);
        }
        else
        {
            T passiveInstance = passiveSer.GetInstance ();
            passiveInstance.SetValue (passiveSer.Value * multiplicator);
            passiveDict.Add (passiveSer.Passive.Description, passiveInstance);
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
            string effectName = effectSerialized.Effect.Description;
            if (_effectByNames.ContainsKey (effectName))
            {
                _effectByNames[effectName].AddToInitialValue (effectSerialized.Value * multiplicator);
            }
            else
            {
                Effect effect = effectSerialized.GetInstance ();
                effect.SetInitialValue (effectSerialized.Value * multiplicator);
                _effectByNames.Add (effectSerialized.Effect.Description, effect);
            }
        }
    }

    // private void OverwriteStat (StatsSerialized statsSerialized, int multiplicator = 1)
    // {
    //     foreach (AttackDefPassiveSerialized attackdefPassiveSerialized in statsSerialized.AttackDefPassiveValues)
    //     {
    //         OverwriteToPassiveDict (_attackDefPassiveByNames, attackdefPassiveSerialized, multiplicator);
    //     }

    //     foreach (AttackOffPassiveSerialized attackOffPassiveSerialized in statsSerialized.AttackOffPassiveValues)
    //     {
    //         OverwriteToPassiveDict (_attackoffPassiveByNames, attackOffPassiveSerialized, multiplicator);
    //     }

    //     foreach (EffectDefPassiveSerialized effectDefPassiveSerialized in statsSerialized.EffectDefPassiveValues)
    //     {
    //         OverwriteToPassiveDict (_effectDefPassiveByNames, effectDefPassiveSerialized, multiplicator);
    //     }

    //     foreach (EffectOffPassiveSerialized effectOffPassiveSerialized in statsSerialized.EffectOffPassiveValues)
    //     {
    //         OverwriteToPassiveDict (_effectOffPassiveByNames, effectOffPassiveSerialized, multiplicator);
    //     }

    //     foreach (EffectSerialized effectSerialized in statsSerialized.effectValues)
    //     {
    //         string effectName = effectSerialized.Effect.EffectName;
    //         if (_effectByNames.ContainsKey (effectName))
    //         {
    //             _effectByNames[effectName].SetInitialValue (effectSerialized.Value * multiplicator);
    //         }
    //         else
    //         {
    //             Effect effect = effectSerialized.GetInstance ();
    //             effect.SetInitialValue (effectSerialized.Value * multiplicator);
    //             _effectByNames.Add (effectSerialized.Effect.EffectName, effect);
    //         }
    //     }
    // }

    public override string ToString ()
    {
        string s = "Life : " + Life + "\nIntelligence : " + Intelligence + "\nStrenght : " + Strenght + "\nMobility : " + Mobility + "\n";

        foreach (KeyValuePair<string, Effect> stat in _effectByNames)
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
        return AttackDefPassiveValues?.Count > 0 || AttackOffPassiveValues?.Count > 0 || EffectDefPassiveValues?.Count > 0 || EffectOffPassiveValues?.Count > 0 || effectValues?.Count > 0;
    }
}