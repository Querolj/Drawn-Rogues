using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstSingleTarget : AttackInstance
{
    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        base.Execute (attacker, target, attackPos, onAttackEnded, targetsInZone, trajectory);
        _targetToHitCount = 1;

        AttackInstSingleTarget attackInstCopy = GetCopy () as AttackInstSingleTarget;
        ApplyTargetAttackDefPassive (target, ref attackInstCopy);
        bool isDodged = DodgeTest (attackInstCopy);
        if (isDodged)
        {
            _fightDescription.ReportAttackDodge (_attacker.Description.DisplayName, target.Description, attackInstCopy.Name, _attacker.tag);
            _onAttackEnded?.Invoke ();
            return;
        }

        if (AnimationTemplate != null)
            PlayAtkTouchedAnimation (attackPos, () => InflictDamage (target, attackInstCopy));
        else if (ParticleTemplate != null)
            PlayAtkTouchedParticle (attackPos, () => InflictDamage (target, attackInstCopy));
        else
            InflictDamage (target, attackInstCopy);
    }

    public override AttackInstance GetCopy ()
    {
        AttackInstSingleTarget attackInstSingleTarget = new AttackInstSingleTarget ();
        attackInstSingleTarget.Init (_attack, _owner);
        return attackInstSingleTarget;
    }
}