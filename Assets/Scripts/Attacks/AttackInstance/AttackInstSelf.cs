using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstSelf : AttackInstance
{
    public AttackInstSelf (Attack attack, Character owner, FightDescription fightDescription) : base (attack, owner, fightDescription) { }

    public override AttackInstance GetCopy ()
    {
        return new AttackInstSelf (_attack, _owner, _fightDescription);
    }

    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        base.Execute (attacker, target, attackPos, onAttackEnded, targetsInZone, trajectory);
        _targetToHitCount = 1;
        AttackInstSelf attackInstCopy = GetCopy () as AttackInstSelf;
        ApplyTargetAttackDefPassive (target, ref attackInstCopy);

        if (AnimationTemplate != null)
            PlayAtkTouchedAnimation (attackPos, () => InflictDamage (target, attackInstCopy));
        else if (ParticleTemplate != null)
            PlayAtkTouchedParticle (attackPos, () => InflictDamage (target, attackInstCopy));
    }
}