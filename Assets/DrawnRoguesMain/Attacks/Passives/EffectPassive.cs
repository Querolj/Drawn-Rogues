using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class EffectPassive : Passive
{
    [BoxGroup ("Settings")]
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

        AlterPropertyValue (ref effectValue);

        return effectValue;
    }
}