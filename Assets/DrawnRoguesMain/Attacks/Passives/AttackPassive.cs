using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class AttackPassive : Passive
{
    [Tooltip ("If empty, will apply to all attack types"), BoxGroup ("Statistic focused by passive")]
    public List<AttackType> AttackTypeToAlter;
    
    [BoxGroup ("Statistic focused by passive")]
    public AttackStatsType AttackStatToAlter;

    [Tooltip ("If empty, will apply to all damage types"), BoxGroup ("Statistic focused by passive"), ShowIf (nameof (IsAttackStatFocusedDamage))]
    public List<AttackElement> DamageTypeToAlter;

    public enum AttackStatsType
    {
        Damage,
        Range,
        Precision,
        CriticalChance,
        CriticalMultipliier
    }

    private bool IsAttackStatFocusedDamage()
    {
        return AttackStatToAlter == AttackStatsType.Damage;
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