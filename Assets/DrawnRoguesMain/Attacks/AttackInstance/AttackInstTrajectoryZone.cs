using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstTrajectoryZone : AttackInstance
{
    public AttackableDetector AttackableDetectorTemplate;
    public Vector2 ZoneSize;
    public float TrajectorySpeed = 1f;
    public float TrajectoryRadius = 1f;

    public override void Init (Attack attack, Character owner)
    {
        base.Init (attack, owner);
        AttackTrajectoryZone attackTrajectoryZone = attack as AttackTrajectoryZone ??
            throw new ArgumentException (nameof (attack) + " must be of type " + nameof (AttackTrajectoryZone));

        TrajectorySpeed = attackTrajectoryZone.TrajectorySpeed;
        TrajectoryRadius = attackTrajectoryZone.TrajectoryRadius;
        ZoneSize = attackTrajectoryZone.ZoneSize;
        AttackableDetectorTemplate = attackTrajectoryZone.AttackableDetectorTemplate;
    }

    public override AttackInstance GetCopy ()
    {
        AttackInstTrajectoryZone attackInstTrajectoryZone = new AttackInstTrajectoryZone ();
        attackInstTrajectoryZone.Init (_attack, _owner);
        return attackInstTrajectoryZone;
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
        attacker.CharMovement.Jump (trajectory, () =>
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
                    _fightDescription.ReportAttackDodge (_attacker.Description.DisplayName, attackable.Description, attackInstCopy.Name, _attacker.tag);
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
                attacker.CharMovement.Jump (trajectory, () =>
                {
                    attacker.CharMovement.ActivateWalk = true;
                    TryInvokeCallback ();
                });
            });

        });
    }
}