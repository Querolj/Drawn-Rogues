using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstProjectile : AttackInstance
{
    public float TrajectorySpeed = 1f;
    public float TrajectoryRadius = 1f;

    private Projectile _projectileTemplate;

    public override void Init (Attack attack, Character owner)
    {
        base.Init (attack, owner);
        AttackProjectile attackProjectile = attack as AttackProjectile ??
            throw new ArgumentException (nameof (attack) + " must be of type " + nameof (AttackProjectile));

        TrajectorySpeed = attackProjectile.TrajectorySpeed;
        TrajectoryRadius = attackProjectile.TrajectoryRadius;
        _projectileTemplate = attackProjectile.ProjectileTemplate;

    }

    public override AttackInstance GetCopy ()
    {
        AttackInstProjectile attackInstProjectile = new AttackInstProjectile ();
        attackInstProjectile.Init (_attack, _owner);
        return attackInstProjectile;
    }

    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        base.Execute (attacker, target, attackPos, onAttackEnded, targetsInZone, trajectory);
        _targetToHitCount = 1;

        if (trajectory.Count == 0 || trajectory == null)
        {
            throw new ArgumentNullException (nameof (trajectory));
        }

        AttackInstProjectile attackInstCopy = GetCopy () as AttackInstProjectile;
        ApplyTargetAttackDefPassive (target, ref attackInstCopy);
        bool isDodged = DodgeTest (attackInstCopy);

        Projectile proj = GameObject.Instantiate<Projectile> (_projectileTemplate);
        proj.Init (trajectory, TrajectorySpeed, isDodged, attacker.IsEnemy, (destinationReached) =>
        {
            if (!destinationReached)
            {
                _fightDescription.Report ("The projectile was blocked by an obstacle");
                _onAttackEnded?.Invoke ();
                return;
            }

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
        });
    }
}