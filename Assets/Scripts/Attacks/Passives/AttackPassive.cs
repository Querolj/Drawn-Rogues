using System.Collections.Generic;
using UnityEngine;

public class AttackPassive : Passive
{
    [Tooltip ("If empty, will apply to all damage types")]
    public List<DamageType> DamageTypeToAlter;

    [Tooltip ("If empty, will apply to all attack types")]
    public List<AttackType> AttackTypeToAlter;

    public virtual void AlterAttack (AttackInstance attack)
    {

    }

    protected bool CanAlterAttack (AttackInstance attack)
    {
        if (DamageTypeToAlter?.Count > 0 && !DamageTypeToAlter.Contains (attack.DamageType))
            return false;
        if (AttackTypeToAlter?.Count > 0 && !AttackTypeToAlter.Contains (attack.AttackType))
            return false;

        return true;
    }

    protected void AlterPrecision (AttackInstance attack)
    {
        switch (OperationType)
        {
            case OperationTypeEnum.Add:
                attack.Precision += Value;
                break;
            case OperationTypeEnum.AddPercentage:
                attack.Precision += attack.Precision * (Value / 100f);
                break;
            case OperationTypeEnum.Set:
                attack.Precision = Value;
                break;
            case OperationTypeEnum.PercentageResistance:
                attack.Precision = attack.Precision * (1f - (Value / 100f));
                break;
            case OperationTypeEnum.Substract:
                attack.Precision -= Value;
                break;
        }
    }

    protected void AlterDamage (AttackInstance attack)
    {
        switch (OperationType)
        {
            case OperationTypeEnum.Add:
                attack.MinDamage += (int) Value;
                attack.MaxDamage += (int) Value;
                break;
            case OperationTypeEnum.AddPercentage:
                attack.MinDamage += (int) (attack.MinDamage * (Value / 100f));
                attack.MaxDamage += (int) (attack.MaxDamage * (Value / 100f));
                break;
            case OperationTypeEnum.Set:
                attack.MinDamage = (int) Value;
                attack.MaxDamage = (int) Value;
                break;
            case OperationTypeEnum.PercentageResistance:
                attack.MinDamage = (int) (attack.MinDamage * (1f - (Value / 100f)));
                attack.MaxDamage = (int) (attack.MaxDamage * (1f - (Value / 100f)));
                break;
        }
    }
}