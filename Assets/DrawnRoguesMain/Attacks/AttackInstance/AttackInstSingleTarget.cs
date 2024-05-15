using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstSingleTarget : AttackInstance
{
    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        base.Execute (attacker, target, attackPos, onAttackEnded, targetsInZone, trajectory);

        AttackInstSingleTarget attackInstCopy = GetCopy () as AttackInstSingleTarget;
        TryInflictDamage (attackPos, target, attackInstCopy);
    }

    public override AttackInstance GetCopy ()
    {
        AttackInstSingleTarget attackInstSingleTarget = new AttackInstSingleTarget ();
        attackInstSingleTarget.Init (_attack, _owner);
        return attackInstSingleTarget;
    }
}