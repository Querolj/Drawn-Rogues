using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackPassive : Passive
{
    [Tooltip ("If empty, will apply to all damage types")]
    public List<AttackElement> DamageTypeToAlter;

    [Tooltip ("If empty, will apply to all attack types")]
    public List<AttackType> AttackTypeToAlter;

    public AttackStatsType AttackStatToAlter;

    public enum AttackStatsType
    {
        Damage,
        Range,
        Precision,
        CriticalChance,
        CriticalMultipliier
    }

    public void AlterAttack (AttackInstance attack)
    {
        if (!CanAlterAttack (attack))
            return;

        switch (AttackStatToAlter)
        {
            case AttackStatsType.Damage:
                float minDamage = attack.MinDamage;
                AlterPropertyValue (ref minDamage);
                attack.MinDamage = Mathf.Max ((int) minDamage, 0);
                float maxDamage = attack.MaxDamage;
                AlterPropertyValue (ref maxDamage);
                attack.MaxDamage = Mathf.Max ((int) maxDamage, 0);
                break;
            case AttackStatsType.Precision:
                float precision = attack.Precision;
                AlterPropertyValue (ref precision);
                attack.Precision = precision;
                break;
            case AttackStatsType.CriticalChance:
                float criticalChance = attack.CriticalChance;
                AlterPropertyValue (ref criticalChance);
                attack.CriticalChance = criticalChance;
                break;
            case AttackStatsType.CriticalMultipliier:
                float criticalMultiplier = attack.CriticalMultiplier;
                AlterPropertyValue (ref criticalMultiplier);
                attack.CriticalMultiplier = criticalMultiplier;
                break;
            case AttackStatsType.Range:
                float range = attack.Range;
                AlterPropertyValue (ref range);
                attack.Range = Mathf.Max (range, 1);
                break;
        }
    }

    protected bool CanAlterAttack (AttackInstance attack)
    {
        if (DamageTypeToAlter?.Count > 0 && !DamageTypeToAlter.Contains (attack.AttackElement))
            return false;
        if (AttackTypeToAlter?.Count > 0)
        {
            foreach (AttackType attackType in AttackTypeToAlter)
            {
                if (attack.AttackTypes.Contains (attackType))
                    return true;
            }
            return false;
        }

        return true;
    }
}