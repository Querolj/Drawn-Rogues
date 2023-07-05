using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "AttackOffPassive", menuName = "Passive/AttackOffPassive", order = 1)]
public class AttackOffPassive : AttackPassive
{
    public enum AttackStatsEnum
    {
        Damage,
        Range,
        Precision
    }
    public AttackStatsEnum AttackStatToAlter;

    public override void AlterAttack (AttackInstance attack)
    {
        base.AlterAttack (attack);

        switch (AttackStatToAlter)
        {
            case AttackStatsEnum.Damage:
                AlterDamage (attack);
                break;
            case AttackStatsEnum.Range:
                AlterRange (attack);
                break;
            case AttackStatsEnum.Precision:
                AlterPrecision (attack);
                break;
        }
    }

    private void AlterRange (AttackInstance attack)
    {
        switch (OperationType)
        {
            case OperationTypeEnum.Add:
                attack.Range += Value;
                break;
            case OperationTypeEnum.AddPercentage:
                attack.Range *= Value;
                break;
            case OperationTypeEnum.Set:
                attack.Range = Value;
                break;
        }
    }
}

[Serializable]
public class AttackOffPassiveSerialized : PassiveSerialized<AttackOffPassive> { }