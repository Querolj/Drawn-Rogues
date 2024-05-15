using System;
using UnityEngine;

// Attack passive that is usually used to make the character defend from an attack (ex : resistance to fire)
[CreateAssetMenu (fileName = "AttackDefPassive", menuName = "Passive/AttackDefPassive", order = 1)]
public class AttackDefPassive : AttackPassive
{
    public AttackStatsType AttackStatToAlter;

    public override void AlterAttack (AttackInstance attack)
    {
        if (!CanAlterAttack (attack))
            return;

        base.AlterAttack (attack);
        switch (AttackStatToAlter)
        {
            case AttackStatsType.Damage:
                AlterDamage (attack);
                break;
            case AttackStatsType.Precision:
                AlterPrecision (attack);
                break;
            case AttackStatsType.CriticalChance:
                AlterCriticalChance (attack);
                break;
            case AttackStatsType.CriticalMultipliier:
                AlterCriticalMultiplier (attack);
                break;
        }
    }
}

[Serializable]
public class AttackDefPassiveSerialized : PassiveSerialized<AttackDefPassive> { }