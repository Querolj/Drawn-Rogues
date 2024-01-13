using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstJump : AttackInstance
{
    public override AttackInstance GetCopy ()
    {
        AttackInstJump attackInstJump = new AttackInstJump ();
        attackInstJump.Init (_attack, _owner);
        return attackInstJump;
    }

    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        base.Execute (attacker, target, attackPos, onAttackEnded, targetsInZone, trajectory);

        if (trajectory.Count == 0 || trajectory == null)
        {
            throw new ArgumentNullException (nameof (trajectory));
        }

        attacker.CharMovement.ActivateWalk = false;
        attacker.Squasher.SquashHorizontally (0.4f, 0.25f, 0.05f, () =>
        {
            attacker.CharMovement.FollowTrajectory (trajectory, () =>
            {
                attacker.CharMovement.ActivateWalk = true;
                onAttackEnded.Invoke ();
            });
        });

    }
}