using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect : ScriptableObject
{
    public enum AttackTimeline
    {
        AfterAttackLanded,
        ReceiveAttackDamage
    }

    public enum ApplyCondition
    {
        AttackIsMelee,
        AttackIsProjectile,
    }

    [SerializeField, BoxGroup ("Display")]
    private string _description;
    public string Description => _description;

    [SerializeField, BoxGroup ("Display")]
    private SpriteAnimation _animationOnApplyTemplate;
    public SpriteAnimation AnimationOnApplyTemplate => _animationOnApplyTemplate;

    [SerializeField, BoxGroup ("Display")]
    private ParticleSystemCallback _particleOnApplyTemplate;
    public ParticleSystemCallback ParticleOnApplyTemplate => _particleOnApplyTemplate;

    [SerializeField, BoxGroup ("Display")]
    private bool _inverseDisplayedSignInDescription;
    public bool InverseDisplayedSignInDescription => _inverseDisplayedSignInDescription;

    [SerializeField, BoxGroup ("Application context")]
    private AttackTimeline _effectApplicationTimeline;
    public AttackTimeline EffectApplicationTimeline => _effectApplicationTimeline;

    [SerializeField, BoxGroup ("Application context")]
    private List<ApplyCondition> _applyConditionsOnUser;
    public List<ApplyCondition> ApplyConditionsOnUser => _applyConditionsOnUser;

    [SerializeField, BoxGroup ("Application context")]
    private List<ApplyCondition> _applyConditionsOnTarget;
    public List<ApplyCondition> ApplyConditionsOnTarget => _applyConditionsOnTarget;

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

    public void ApplyOnUser (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightRegistry, Action onAnimeEnded)
    {
        if (!CheckApplyCondition (attack, ApplyConditionsOnUser))
        {
            onAnimeEnded?.Invoke ();
            return;
        }

        _alteredValue = GetAlteredValue (user.Stats, target.Stats);
        ApplyOnUserInternal (user, attack, target, inflictedDamage, fightRegistry, onAnimeEnded);
    }

    protected virtual void ApplyOnUserInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightRegistry, Action onAnimeEnded)
    {
        onAnimeEnded?.Invoke ();
    }

    public void ApplyOnTarget (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightRegistry, Action onAnimeEnded)
    {
        if (!CheckApplyCondition (attack, ApplyConditionsOnTarget))
        {
            onAnimeEnded?.Invoke ();
            return;
        }

        _alteredValue = GetAlteredValue (user.Stats, target.Stats);
        ApplyOnTargetInternal (user, attack, target, inflictedDamage, fightRegistry, onAnimeEnded);
    }

    protected virtual void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightRegistry, Action onAnimeEnded)
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

    protected float GetAlteredValue (AttackableStats ownerStats, AttackableStats targetStats)
    {
        float moddedValue = _initialValue;

        // order passives by operation type to ensure correct order of operations
        List<EffectOffPassive> offPassives = new List<EffectOffPassive> (ownerStats.EffectOffPassiveByNames.Values).OrderBy (x => x.OperationType).ToList ();
        foreach (EffectOffPassive offPassive in offPassives)
        {
            moddedValue = offPassive.GetAlterEffectValue (this, moddedValue);
        }

        List<EffectDefPassive> defPassives = new List<EffectDefPassive> (targetStats.EffectDefPassiveByNames.Values).OrderBy (x => x.OperationType).ToList ();
        foreach (EffectDefPassive defPassive in defPassives)
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
        string descriptionWithValue = Description.Replace ("{value}", Mathf.Abs (InitialValue * 100f).ToString ());
        if (InverseDisplayedSignInDescription)
            descriptionWithValue = descriptionWithValue.Replace ("{sign}", InitialValue < 0 ? "+" : "-");
        else
            descriptionWithValue = descriptionWithValue.Replace ("{sign}", InitialValue >= 0 ? "+" : "-");
        return descriptionWithValue;
    }

    public virtual Effect GetCopy ()
    {
        Effect copy = Instantiate (this);
        copy.SetInitialValue (InitialValue);
        return copy;
    }
}

[Serializable]
public class EffectSerialized
{
    public Effect Effect;
    public float Value;

    public Effect GetInstance ()
    {
        Effect effect = ScriptableObject.Instantiate (Effect);
        effect.SetInitialValue (Value);
        return effect;
    }

    public override string ToString ()
    {
        return Effect.Description + " " + (Value * 100f);
    }

    public string ToString (float value)
    {
        return Effect.Description + " " + (value * 100f);
    }
}