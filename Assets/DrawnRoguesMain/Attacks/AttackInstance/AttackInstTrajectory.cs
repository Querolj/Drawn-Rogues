using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstTrajectory : AttackInstance
{
    public float TrajectorySpeed = 1f;
    public float TrajectoryRadius = 1f;

    public override void Init (Attack attack, Character owner)
    {
        base.Init (attack, owner);
        AttackTrajectory attackTrajectory = attack as AttackTrajectory ??
            throw new ArgumentException (nameof (attack) + " must be of type " + nameof (AttackTrajectory));

        TrajectorySpeed = attackTrajectory.TrajectorySpeed;
        TrajectoryRadius = attackTrajectory.TrajectoryRadius;
    }

    public override AttackInstance GetCopy ()
    {
        AttackInstTrajectory attackInstTrajectory = new AttackInstTrajectory ();
        attackInstTrajectory.Init (_attack, _owner);
        return attackInstTrajectory;
    }

    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        base.Execute (attacker, target, attackPos, onAttackEnded, targetsInZone, trajectory);

        if (trajectory.Count == 0 || trajectory == null)
        {
            throw new ArgumentNullException (nameof (trajectory));
        }

        if (target == null)
            throw new ArgumentNullException (nameof (target));

        _targetToHitCount = 1;

        attacker.CharMovement.ActivateWalk = false;
        attacker.CharMovement.Jump (trajectory, () =>
        {
            AttackInstTrajectory attackInstCopy = GetCopy () as AttackInstTrajectory;
            ApplyTargetAttackDefPassive (target, ref attackInstCopy);
            bool isDodged = DodgeTest (attackInstCopy);
            if (isDodged)
            {
                _fightDescription.ReportAttackDodge (_attacker.Description.DisplayName, target.Description, attackInstCopy.Name, _attacker.tag);
                EndAttack (attacker, trajectory);
                return;
            }

            target.Squasher?.SquashHorizontally (0.6f, 0.4f, 0.2f);

            if (AnimationTemplate != null)
                PlayAtkTouchedAnimation (target.transform.position, () => InflictDamage (target, attackInstCopy));
            else if (ParticleTemplate != null)
                PlayAtkTouchedParticle (target.transform.position, () => InflictDamage (target, attackInstCopy));
            EndAttack (attacker, trajectory);
        });
    }

    private void EndAttack (Character attacker, List<Vector3> trajectory)
    {
        attacker.Squasher.SquashHorizontally (0.6f, 0.1f, 0.05f, () =>
        {
            trajectory.Reverse ();
            attacker.CharMovement.Jump (trajectory, () =>
            {
                attacker.CharMovement.ActivateWalk = true;
                TryInvokeCallback ();
            });
        });
    }
}