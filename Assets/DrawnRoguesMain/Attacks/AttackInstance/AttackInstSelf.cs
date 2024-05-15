using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstSelf : AttackInstance
{
    public override AttackInstance GetCopy ()
    {
        AttackInstSelf attackInstSelf = new AttackInstSelf ();
        attackInstSelf.Init (_attack, _owner);
        return attackInstSelf;
    }

    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        base.Execute (attacker, target, attackPos, onAttackEnded, targetsInZone, trajectory);
        AttackInstSelf attackInstCopy = GetCopy () as AttackInstSelf;
        TryInflictDamage (attackPos, target, attackInstCopy, false);
    }
}