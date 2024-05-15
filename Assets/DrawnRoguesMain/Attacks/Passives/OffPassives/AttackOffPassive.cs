using System;
using System.Collections.Generic;
using UnityEngine;

// Attack passive that is usually used to make the character boost his attack (ex : +10% fire damage)
[CreateAssetMenu (fileName = "AttackOffPassive", menuName = "Passive/AttackOffPassive", order = 1)]
public class AttackOffPassive : AttackPassive
{
    public AttackStatsType AttackStatToAlter;

    public override void AlterAttack (AttackInstance attack)
    {
        base.AlterAttack (attack);

        switch (AttackStatToAlter)
        {
            case AttackStatsType.Damage:
                AlterDamage (attack);
                break;
            case AttackStatsType.Range:
                AlterRange (attack);
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