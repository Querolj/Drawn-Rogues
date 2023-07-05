using System;
using UnityEngine;

[CreateAssetMenu (fileName = "AttackDefPassive", menuName = "Passive/AttackDefPassive", order = 1)]
public class AttackDefPassive : AttackPassive
{
    public enum AttackStatsEnum
    {
        Damage,
        Precision
    }
    public AttackStatsEnum AttackStatToAlter;

    public override void AlterAttack (AttackInstance attack)
    {
        if (!CanAlterAttack (attack))
            return;

        base.AlterAttack (attack);
        switch (AttackStatToAlter)
        {
            case AttackStatsEnum.Damage:
                AlterDamage (attack);
                break;
            case AttackStatsEnum.Precision:
                AlterPrecision (attack);
                break;
        }
    }
}

[Serializable]
public class AttackDefPassiveSerialized : PassiveSerialized<AttackDefPassive> { }