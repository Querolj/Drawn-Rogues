using System;
using UnityEngine;

public class EffectPassive : Passive
{
    public Effect[] EffectsToAlter;

    public float GetAlterEffectValue (Effect effect, float effectValue) // effect value is suplied separately because it may be altered by other passives
    {
        // check if this passive alters the effect
        bool alters = false;
        foreach (Effect effectToAlter in EffectsToAlter)
        {
            if (effectToAlter.Description == effect.Description)
            {
                alters = true;
                break;
            }
        }

        if (!alters)
            return effectValue;

        float value = effectValue;
        switch (OperationType)
        {
            case OperationTypeEnum.Add:
                value += Value;
                break;
            case OperationTypeEnum.AddPercentage:
                value += value * (Value / 100f);
                break;
            case OperationTypeEnum.Set:
                value = Value;
                break;
            case OperationTypeEnum.PercentageResistance:
                value = value * (1f - (Value / 100f));
                break;
        }

        return value;
    }
}