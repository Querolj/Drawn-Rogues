using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstCharge : AttackInstance
{
    public bool CanGoThroughAttackables;
    public float ChargeSpeed;
    public AttackableDetector AttackableDetectorTemplate;

    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        base.Execute (attacker, target, attackPos, onAttackEnded, targetsInZone, trajectory);

        AttackInstCharge attackInstCopy = GetCopy () as AttackInstCharge;
        TryInflictDamage (attackPos, target, attackInstCopy);
    }

    public override void Init (Attack attack, Character owner)
    {
        base.Init (attack, owner);
        AttackCharge attackCharge = attack as AttackCharge ??
            throw new ArgumentException (nameof (attack) + " must be of type " + nameof (AttackCharge));

        CanGoThroughAttackables = attackCharge.CanGoThroughAttackables;
        ChargeSpeed = attackCharge.ChargeSpeed;
        AttackableDetectorTemplate = attackCharge.AttackableDetectorTemplate;
    }

    public override AttackInstance GetCopy ()
    {
        AttackInstCharge attackInstCharge = new AttackInstCharge ();
        attackInstCharge.Init (_attack, _owner);

        return attackInstCharge;
    }
}