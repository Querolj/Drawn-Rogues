using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackInstance
{
    public string Name { get; set; }
    public SpriteAnimation AnimationTemplate { get; set; }
    public ParticleSystemCallback ParticleTemplate { get; set; }
    public int MinDamage { get; set; }
    public int MaxDamage { get; set; }
    public bool NoDamage { get; set; } = false;
    public float Precision { get; set; }
    public float Range { get; set; }
    public DamageType DamageType { get; set; }
    public AttackType AttackType;

    protected Character _owner;
    protected Action _onAttackEnded;
    protected Dictionary<string, Effect> _effectInstancesByName = new Dictionary<string, Effect> ();
    public Dictionary<string, Effect> EffectInstancesByName { get { return _effectInstancesByName; } }

    public Action<AttackInstance> OnAttackStarted;
    protected FightDescription _fightDescription;
    protected Character _attacker;
    protected Attack _attack;
    private bool _callbackCalled = false;
    protected int _targetToHitCount;

    public AttackInstance (Attack attack, Character owner, FightDescription fightDescription)
    {
        _attack = attack;
        Name = attack.Name;
        AnimationTemplate = attack.AnimationTemplate;
        ParticleTemplate = attack.ParticleTemplate;
        AttackType = attack.AttackType;
        MinDamage = attack.MinDamage;
        MaxDamage = attack.MaxDamage;
        NoDamage = attack.NoDamage;
        Precision = attack.Precision;
        Range = attack.GetRangeInMeter ();
        DamageType = attack.DamageType;
        _owner = owner;
        _fightDescription = fightDescription;

        MergeEffectsAndPassiveFromOwner (attack);

        // apply strenght influence
        if (attack.DamageType != DamageType.Heal && _owner.Stats.Strenght > 0 && !NoDamage)
        {
            MinDamage += MinDamage * (int) (_owner.Stats.Strenght / 100f);
            MaxDamage += MaxDamage * (int) (_owner.Stats.Strenght / 100f);
        }
    }

    public virtual AttackInstance GetCopy ()
    {
        throw new NotImplementedException ();
    }

    public virtual void Execute (Character attacker, Attackable target, Vector3 attackPos, Action onAttackEnded,
        List<Attackable> targetsInZone = null, List<Vector3> trajectory = null)
    {
        _onAttackEnded = onAttackEnded ??
            throw new ArgumentNullException (nameof (onAttackEnded));
        _attacker = attacker ??
            throw new ArgumentNullException (nameof (attacker));

        OnAttackStarted?.Invoke (this);
    }

    private void MergeEffectsAndPassiveFromOwner (Attack attack)
    {
        // instantiate effects
        foreach (EffectSerialized effectWithValue in attack.Effects)
        {
            Effect effectInstance = effectWithValue.GetInstance ();
            _effectInstancesByName.Add (effectInstance.name, effectInstance);
        }

        // merge effect from owner with effects
        foreach (Effect effect in _owner.Stats.EffectByNames.Values)
        {
            if (!_effectInstancesByName.ContainsKey (effect.name))
                _effectInstancesByName.Add (effect.name, effect);
            else
                _effectInstancesByName[effect.name].AddToInitialValue (effect.InitialValue);
        }

        // alter attack with owner offensive passives
        foreach (AttackOffPassive attackOffPassive in _owner.Stats.AttackOffPassiveByNames.Values)
        {
            attackOffPassive.AlterAttack (this);
        }
    }

    protected void PlayAtkTouchedAnimation (Vector3 position, Action onAnimeEnded)
    {
        SpriteAnimation anime = GameObject.Instantiate<SpriteAnimation> (AnimationTemplate, position, Quaternion.identity);
        anime.Direction = _attacker.CharMovement.DirectionRight ? SpriteAnimation.AnimeDirection.Right : SpriteAnimation.AnimeDirection.Left;
        anime.OnAnimationEnded += onAnimeEnded;
        anime.Play ();
    }

    protected void PlayAtkTouchedParticle (Vector3 position, Action onParticleEnded)
    {
        ParticleSystemCallback particle = GameObject.Instantiate<ParticleSystemCallback> (ParticleTemplate, position, Quaternion.Euler (-90, 0, 0));
        particle.OnParticleSystemDestroyed += onParticleEnded;
        particle.Play ();
    }

    protected virtual void PlayAnimationsOnTargets<T> (List<Attackable> allTargets) where T : AttackInstance
    {
        foreach (Attackable target in allTargets)
        {
            if (target == null || target.WillBeDestroyed)
                continue;

            T attackInstcopy = GetCopy () as T;
            if (AnimationTemplate != null)
                PlayAtkTouchedAnimation (target.transform.position, () => InflictDamage (target, attackInstcopy));
            else if (ParticleTemplate != null)
                PlayAtkTouchedParticle (target.transform.position, () => InflictDamage (target, attackInstcopy));
        }
    }

    protected T ApplyTargetAttackDefPassive<T> (Attackable target, ref T attackInstance) where T : AttackInstance
    {
        if (target.Stats.AttackDefPassiveByNames.Count == 0)
            return attackInstance;

        foreach (AttackDefPassive attackDefPassive in target.Stats.AttackDefPassiveByNames.Values)
        {
            attackDefPassive.AlterAttack (attackInstance);
        }

        return attackInstance;
    }

    protected bool DodgeTest (AttackInstance attackInstance)
    {
        return UnityEngine.Random.Range (0f, 100f) > attackInstance.Precision;
    }

    protected void InflictDamage (Attackable target, AttackInstance attackInstance)
    {
        if (attackInstance == this)
            throw new ArgumentException (nameof (attackInstance) + " must be a copy of this");

        int dammageToInflict = 0;

        if (!NoDamage)
        {
            dammageToInflict = UnityEngine.Random.Range (attackInstance.MinDamage, attackInstance.MaxDamage + 1);
            _fightDescription.ReportAttackDamage (_attacker, target, attackInstance, dammageToInflict);
            target.FadeSprite ();
            target.ReceiveDamage (dammageToInflict);
            ApplyEffects (target.EffectInstancesByName, target, Effect.Timeline.ReceiveAttackDamage, _attacker, attackInstance, dammageToInflict);
        }
        else
        {
            _fightDescription.ReportAttackUse (_attacker, target, attackInstance);
        }

        if (_effectInstancesByName.Values.Count == 0)
        {
            TryInvokeCallback ();
            return;
        }

        ApplyEffects (_effectInstancesByName, _attacker, Effect.Timeline.AfterAttackLanded, target, attackInstance, dammageToInflict);
    }

    protected void TryInvokeCallback ()
    {
        _targetToHitCount--;

        if (_targetToHitCount == 0 && !_callbackCalled)
        {
            _callbackCalled = true;
            _onAttackEnded?.Invoke ();
        }
    }

    protected void ApplyEffects (Dictionary<string, Effect> effectInstancesByName, Attackable effectOwner, Effect.Timeline timeline, Attackable target, AttackInstance attackInstance, int dammageToInflict = 0)
    {
        Stack<Effect> stackEffects = new Stack<Effect> ();
        foreach (Effect effect in effectInstancesByName.Values)
        {
            if (effect.EffectApplicationTimeline == timeline)
                stackEffects.Push (effect);
        }

        ApplyStackOfEffects (effectOwner, stackEffects, target, dammageToInflict, attackInstance);
    }

    private void ApplyStackOfEffects (Attackable effectOwner, Stack<Effect> stackEffects, Attackable target, int dammageToInflict, AttackInstance attackInstance)
    {
        if (stackEffects.Count == 0)
        {
            TryInvokeCallback ();
            return;
        }

        Effect effect = stackEffects.Pop ();

        effect.ApplyOnUser (_attacker, this, target, dammageToInflict, _fightDescription, () =>
        {
            if (target.WillBeDestroyed)
            {
                ApplyStackOfEffects (effectOwner, stackEffects, target, dammageToInflict, attackInstance);
                return;
            }

            effect.ApplyOnTarget (_attacker, this, target, dammageToInflict, _fightDescription, () =>
            {
                ApplyStackOfEffects (effectOwner, stackEffects, target, dammageToInflict, attackInstance);
                return;
            });
        });
    }
}