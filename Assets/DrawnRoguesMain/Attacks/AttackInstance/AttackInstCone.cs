using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackInstCone : AttackInstance
{
    private ParticleSystem _breathParticleSystemTemplate;
    private float _coneAngle;
    private float _coneObjectAngle;

    public ParticleSystem BreathParticleSystem => _breathParticleSystemTemplate;
    public float ConeAngle => _coneAngle;

    public void SetConeObjectAngle (float coneObjectAngle)
    {
        _coneObjectAngle = coneObjectAngle;
    }

    public override void Init (Attack attack, Character owner)
    {
        base.Init (attack, owner);
        AttackCone attackCone = attack as AttackCone ??
            throw new ArgumentException (nameof (attack) + " must be of type " + nameof (AttackCone));

        _breathParticleSystemTemplate = attackCone.BreathParticleSystem;
        _coneAngle = attackCone.ConeAngle;
    }

    public override AttackInstance GetCopy ()
    {
        AttackInstCone attackInstCone = new AttackInstCone ();
        attackInstCone.Init (_attack, _owner);
        return attackInstCone;
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

        // setup particle system
        ParticleSystem breathParticleSystem = GameObject.Instantiate (_breathParticleSystemTemplate);
        breathParticleSystem.Stop ();
        breathParticleSystem.transform.position = attackPos;
        breathParticleSystem.transform.rotation = Quaternion.Euler (0, 0, _coneObjectAngle - _coneAngle / 2f);
        ParticleSystem.MainModule psMain = breathParticleSystem.main;
        float speed = psMain.startSpeed.constant;
        float lifetime = Range / speed;
        psMain.startLifetime = new ParticleSystem.MinMaxCurve (lifetime, lifetime);
        ParticleSystem.ShapeModule psShape = breathParticleSystem.shape;
        psShape.angle = _coneAngle;
        breathParticleSystem.Play ();
        _actionDelayer.ExecuteInSeconds (psMain.duration, () => InflictDamage (allTargets));
    }

    private void InflictDamage (List<Attackable> allTargets)
    {
        foreach (Attackable attackable in allTargets)
        {
            if (attackable == null || attackable.WillBeDestroyed)
                continue;

            AttackInstCone attackInstCopy = GetCopy () as AttackInstCone;
            TryInflictDamage (attackable.transform.position, attackable, attackInstCopy);
        }
    }
}