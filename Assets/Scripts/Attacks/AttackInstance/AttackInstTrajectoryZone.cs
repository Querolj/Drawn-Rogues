using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstTrajectoryZone : AttackInstance
{
    public AttackableDetector AttackableDetectorTemplate;
    public Vector2 ZoneSize;
    public float TrajectorySpeed = 1f;
    public float TrajectoryRadius = 1f;

    public AttackInstTrajectoryZone (Attack attack, Character owner, FightDescription fightDescription) : base (attack, owner, fightDescription)
    {
        AttackTrajectoryZone attackTrajectoryZone = attack as AttackTrajectoryZone ??
            throw new ArgumentException (nameof (attack) + " must be of type " + nameof (AttackTrajectoryZone));

        TrajectorySpeed = attackTrajectoryZone.TrajectorySpeed;
        TrajectoryRadius = attackTrajectoryZone.TrajectoryRadius;
        ZoneSize = attackTrajectoryZone.ZoneSize;
        AttackableDetectorTemplate = attackTrajectoryZone.AttackableDetectorTemplate;
    }

    public override AttackInstance GetCopy ()
    {
        return new AttackInstTrajectoryZone (_attack, _owner, _fightDescription);
    }

    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        base.Execute (attacker, target, attackPos, onAttackEnded, targetsInZone, trajectory);

        if (trajectory.Count == 0 || trajectory == null)
        {
            throw new ArgumentNullException (nameof (trajectory));
        }

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

        attacker.CharMovement.ActivateWalk = false;
        attacker.CharMovement.FollowTrajectory (trajectory, () =>
        {

            foreach (Attackable attackable in allTargets)
            {
                if (attackable == null || attackable.WillBeDestroyed)
                    continue;

                AttackInstTrajectoryZone attackInstCopy = GetCopy () as AttackInstTrajectoryZone;
                ApplyTargetAttackDefPassive (attackable, ref attackInstCopy);
                bool isDodged = DodgeTest (attackInstCopy);
                if (isDodged)
                {
                    _fightDescription.ReportAttackDodge (_attacker, attackable, attackInstCopy);
                    TryInvokeCallback ();
                    continue;
                }

                attackable?.Squasher.SquashHorizontally (0.6f, 0.4f, 0.2f);

                if (AnimationTemplate != null)
                    PlayAtkTouchedAnimation (attackable.transform.position, () => InflictDamage (attackable, attackInstCopy));
                else if (ParticleTemplate != null)
                    PlayAtkTouchedParticle (attackable.transform.position, () => InflictDamage (attackable, attackInstCopy));
            }

            attacker.Squasher.SquashHorizontally (0.6f, 0.1f, 0.05f, () =>
            {
                trajectory.Reverse ();
                attacker.CharMovement.FollowTrajectory (trajectory, () =>
                {
                    attacker.CharMovement.ActivateWalk = true;
                    TryInvokeCallback ();
                });
            });

        });
    }
}