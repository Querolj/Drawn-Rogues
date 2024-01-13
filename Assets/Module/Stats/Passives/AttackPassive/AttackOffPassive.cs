using System;
using System.Collections.Generic;
using UnityEngine;

// Attack passive that is usually used to make the character boost his attack (ex : +10% fire damage)
[CreateAssetMenu (fileName = "AttackOffPassive", menuName = "Passive/AttackOffPassive", order = 1)]
public class AttackOffPassive : AttackPassive
{
    public enum AttackStatsType
    {
        Damage,
        Range,
        Precision
    }
    public AttackStatsType AttackStatToAlter;

    public override void AlterAttack (AttackInstance attack)
    {
        base.AlterAttack (attack);
    }
}

[Serializable]
public class AttackOffPassiveSerialized : PassiveSerialized<AttackOffPassive> { }