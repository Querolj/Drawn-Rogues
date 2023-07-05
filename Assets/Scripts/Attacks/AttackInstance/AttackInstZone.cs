using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstZone : AttackInstance
{
    public AttackableDetector AttackableDetectorTemplate;
    public Vector2 ZoneSize;

    public AttackInstZone (Attack attack, Character owner, FightDescription fightDescription) : base (attack, owner, fightDescription)
    {
        AttackZone attackZone = attack as AttackZone ??
            throw new ArgumentException (nameof (attack) + " must be of type " + nameof (AttackZone));

        AnimationTemplate = attackZone.AnimationTemplate;
        ZoneSize = attackZone.ZoneSize;
        AttackableDetectorTemplate = attackZone.AttackableDetectorTemplate;
    }

    public override AttackInstance GetCopy ()
    {
        return new AttackInstZone (_attack, _owner, _fightDescription);
    }

    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        base.Execute (attacker, target, attackPos, onAttackEnded, targetsInZone, trajectory);

        List<Attackable> allTargets = new List<Attackable> ();

        if (targetsInZone != null)
        {
            allTargets.AddRange (targetsInZone);
        }

        if (target != null && !allTargets.Contains (target))
        {
            allTargets.Add (target);
        }

        if (allTargets.Count == 0)
            throw new Exception (nameof (allTargets) + " has no targets");

        _targetToHitCount = allTargets.Count;

        foreach (Attackable attackable in allTargets)
        {
            if (attackable == null || attackable.WillBeDestroyed)
                continue;

            AttackInstZone attackInstCopy = GetCopy () as AttackInstZone;
            ApplyTargetAttackDefPassive (attackable, ref attackInstCopy);
            bool isDodged = DodgeTest (attackInstCopy);
            if (isDodged)
            {
                _fightDescription.ReportAttackDodge (_attacker, attackable, attackInstCopy);
                TryInvokeCallback ();
                continue;
            }

            if (AnimationTemplate != null)
                PlayAtkTouchedAnimation (attackable.transform.position, () => InflictDamage (attackable, attackInstCopy));
            else if (ParticleTemplate != null)
                PlayAtkTouchedParticle (attackable.transform.position, () => InflictDamage (attackable, attackInstCopy));
        }
    }
}