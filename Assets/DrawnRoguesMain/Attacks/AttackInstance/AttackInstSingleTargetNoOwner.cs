using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstSingleTargetNoOwner : AttackInstance
{
    public override void Init (Attack attack, Character owner)
    {
        _attack = attack;
        Name = attack.Name;
        AnimationTemplate = attack.AnimationTemplate;
        ParticleTemplate = attack.ParticleTemplate;
        AttackTypes = attack.AttackType;
        MinDamage = attack.MinDamage;
        MaxDamage = attack.MaxDamage;
        NoDamage = attack.NoDamage;
        Precision = attack.Precision;
        CriticalChance = attack.CriticalChance;
        Range = attack.GetRangeInMeter ();
        AttackElement = attack.AttackElement;
    }

    public override void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        OnAttackStarted?.Invoke (this);

        AttackInstSingleTargetNoOwner attackInstCopy = GetCopy () as AttackInstSingleTargetNoOwner;
        TryInflictDamage (attackPos, target, attackInstCopy);
    }

    protected override void InflictDamage (Attackable target, AttackInstance attackInstance, bool isCritical)
    {
        if (attackInstance == this)
            throw new ArgumentException (nameof (attackInstance) + " must be a copy of this");

        if (!NoDamage)
        {
            int dammageToInflict = UnityEngine.Random.Range (attackInstance.MinDamage, attackInstance.MaxDamage + 1);
            target.FadeSprite ();
            target.Stats.AttackableState.ReceiveDamage (dammageToInflict);
        }

        TryInvokeCallback ();
    }

    public override AttackInstance GetCopy ()
    {
        AttackInstSingleTargetNoOwner attackInst = new AttackInstSingleTargetNoOwner ();
        attackInst.Init (_attack, null);
        return attackInst;
    }
}