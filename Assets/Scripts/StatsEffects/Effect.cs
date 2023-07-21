using System;
using System.Collections.Generic;
using UnityEngine;

public class Effect : ScriptableObject
{
    public enum Timeline
    {
        AfterAttackLanded,
        ReceiveAttackDamage
    }

    public enum ApplyCondition
    {
        AttackIsMelee,
        AttackIsProjectile,
    }

    public string Name;
    public SpriteAnimation AnimationOnApplyTemplate;
    public ParticleSystemCallback ParticleOnApplyTemplate;
    public Timeline EffectApplicationTimeline;
    public List<ApplyCondition> ApplyConditionsOnUser;
    public List<ApplyCondition> ApplyConditionsOnTarget;

    protected float _initialValue;
    public float InitialValue
    {
        get { return _initialValue; }
    }

    protected float _alteredValue;
    private bool _alteredValueSet = false;

    public void SetInitialValue (float value)
    {
        _initialValue = value;
    }

    public virtual void AddToInitialValue (float value)
    {
        SetInitialValue (_initialValue + value);
    }

    public void ApplyOnUser (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightDescription fightDescription, Action onAnimeEnded)
    {
        if (!CheckApplyCondition (attack, ApplyConditionsOnUser))
        {
            onAnimeEnded?.Invoke ();
            return;
        }

        _alteredValue = GetAlteredValue (user, target);
        ApplyOnUserInternal (user, attack, target, inflictedDamage, fightDescription, onAnimeEnded);
    }

    protected virtual void ApplyOnUserInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightDescription fightDescription, Action onAnimeEnded)
    {
        onAnimeEnded?.Invoke ();
    }

    public void ApplyOnTarget (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightDescription fightDescription, Action onAnimeEnded)
    {
        if (!CheckApplyCondition (attack, ApplyConditionsOnTarget))
        {
            onAnimeEnded?.Invoke ();
            return;
        }

        _alteredValue = GetAlteredValue (user, target);
        ApplyOnTargetInternal (user, attack, target, inflictedDamage, fightDescription, onAnimeEnded);
    }

    protected virtual void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightDescription fightDescription, Action onAnimeEnded)
    {
        onAnimeEnded?.Invoke ();
    }

    private bool CheckApplyCondition (AttackInstance attack, List<ApplyCondition> conditions)
    {
        bool apply = true;
        foreach (ApplyCondition condition in conditions)
        {
            switch (condition)
            {
                case ApplyCondition.AttackIsMelee:
                    apply &= attack.AttackType == AttackType.Melee;
                    break;
                case ApplyCondition.AttackIsProjectile:
                    apply &= attack.AttackType == AttackType.Projectile;
                    break;
            }

            if (!apply)
                break;
        }

        return apply;
    }

    protected float GetAlteredValue (Attackable effectOwner, Attackable target)
    {
        float moddedValue = _initialValue;
        foreach (EffectOffPassive offPassive in effectOwner.Stats.EffectOffPassiveByNames.Values)
        {
            moddedValue = offPassive.GetAlterEffectValue (this, moddedValue);
        }

        foreach (EffectDefPassive defPassive in target.Stats.EffectDefPassiveByNames.Values)
        {
            moddedValue = defPassive.GetAlterEffectValue (this, moddedValue);
        }

        return moddedValue;
    }

    protected void PlayAnimation (Vector3 position, Action onEnded)
    {
        if (AnimationOnApplyTemplate != null)
        {
            PlaySpriteAnimation (position, onEnded);
        }
        else if (ParticleOnApplyTemplate != null)
        {
            PlayParticle (position, onEnded);
        }
        else
        {
            onEnded?.Invoke ();
        }
    }

    private void PlaySpriteAnimation (Vector3 position, Action onEnded)
    {
        SpriteAnimation anime = Instantiate<SpriteAnimation> (AnimationOnApplyTemplate, position, Quaternion.identity);
        anime.OnAnimationEnded += onEnded;
        anime.Play ();
    }

    private void PlayParticle (Vector3 position, Action onParticleEnded)
    {
        ParticleSystemCallback particle = GameObject.Instantiate<ParticleSystemCallback> (ParticleOnApplyTemplate);
        particle.transform.position = position;
        particle.OnParticleSystemDestroyed += onParticleEnded;
        particle.Play ();
    }

    public override string ToString ()
    {
        return Name + " " + InitialValue;
    }

    public virtual Effect GetCopy ()
    {
        Effect copy = Instantiate (this);
        copy.SetInitialValue (InitialValue);
        return copy;
    }

    protected static bool TargetHasTempEffect (Attackable target, TempEffect tmpEffect, TempEffect.Timeline timeline)
    {
        return target.TempEffects.ContainsKey (timeline) && target.TempEffects[timeline].Find (x => x.Name == tmpEffect.Name) != null;
    }
}

[Serializable]
public class EffectSerialized
{
    public Effect Effect;
    public float Value;

    public Effect GetInstance ()
    {
        Effect attackEffect = ScriptableObject.Instantiate (Effect);
        attackEffect.SetInitialValue (Value);
        return attackEffect;
    }

    public override string ToString ()
    {
        return Effect.Name + " " + Value;
    }
}