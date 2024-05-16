using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class AttackInstance
{
    public class Factory : PlaceholderFactory<Attack, Character, AttackInstance> { }

    public string Name { get; set; }
    public SpriteAnimation AnimationTemplate { get; set; }
    public ParticleSystemCallback ParticleTemplate { get; set; }
    public int MinDamage { get; set; }
    public int MaxDamage { get; set; }
    public bool NoDamage { get; set; } = false;
    public float Precision { get; set; }
    public float CriticalChance { get; set; }
    public float CriticalMultiplier { get; set; }
    public float Range { get; set; }
    public AttackElement AttackElement { get; set; }
    public AttackType AttackType;

    protected Character _owner;
    protected Action _onAttackEnded;
    protected Dictionary<string, Effect> _effectInstancesByName = new Dictionary<string, Effect> ();
    public Dictionary<string, Effect> EffectInstancesByName { get { return _effectInstancesByName; } }

    public Action<AttackInstance> OnAttackStarted;
    protected Character _attacker;
    protected Attack _attack;
    private bool _callbackCalled = false;
    protected int _targetToHitCount = 1;

    #region Injected
    [Inject]
    protected FightRegistry _fightDescription;
    #endregion

    public virtual void Init (Attack attack, Character owner)
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
        CriticalChance = attack.CriticalChance;
        CriticalMultiplier = attack.CriticalMultiplier;
        Range = attack.GetRangeInMeter ();
        AttackElement = attack.AttackElement;
        _owner = owner;

        MergeEffectsAndPassiveFromOwner (attack);

        // apply strenght influence
        if (_owner.Stats.Strenght > 0 && !NoDamage)
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
        foreach (EffectSerialized effectWithValue in attack.EffectsSerialized)
        {
            Effect effectInstance = effectWithValue.GetInstance ();
            _effectInstancesByName.Add (effectInstance.Description, effectInstance);
        }

        // merge effect from owner with effects
        foreach (Effect effect in _owner.Stats.EffectByNames.Values)
        {
            if (!_effectInstancesByName.ContainsKey (effect.Description))
                _effectInstancesByName.Add (effect.Description, effect);
            else
                _effectInstancesByName[effect.Description].AddToInitialValue (effect.InitialValue);
        }

        // alter attack with owner offensive passives
        // but first, sort passives by operation type to do the Set operation after Mutliplications for example
        List<AttackOffPassive> attackOffPassives = new List<AttackOffPassive> (_owner.Stats.AttackOffPassiveByNames.Values).OrderBy (x => x.OperationType).ToList ();
        foreach (AttackOffPassive attackOffPassive in attackOffPassives)
        {
            attackOffPassive.AlterAttack (this); // sort par OperationEnum
        }
    }

    // Used by some drawn spell
    public void ApplyMultiplierToEffect (string effectName, float mult)
    {
        if (string.IsNullOrEmpty (effectName))
            throw new ArgumentNullException (nameof (effectName));

        if (!_effectInstancesByName.ContainsKey (effectName))
            throw new ArgumentException ("Effect " + effectName + " not found");

        _effectInstancesByName[effectName].SetInitialValue (_effectInstancesByName[effectName].InitialValue * mult);
    }

    protected void PlayAtkTouchedAnimation (Vector3 position, Action onAnimeEnded = null)
    {
        SpriteAnimation anime = GameObject.Instantiate<SpriteAnimation> (AnimationTemplate, position, Quaternion.identity);
        anime.Direction = _attacker.CharMovement.DirectionRight ? SpriteAnimation.AnimeDirection.Right : SpriteAnimation.AnimeDirection.Left;
        if (onAnimeEnded != null)
            anime.OnAnimationEnded += onAnimeEnded;
        anime.Play ();
    }

    protected void PlayAtkTouchedParticle (Vector3 position, Action onParticleEnded = null)
    {
        ParticleSystemCallback particle = GameObject.Instantiate<ParticleSystemCallback> (ParticleTemplate, position, Quaternion.Euler (-90, 0, 0));
        if (onParticleEnded != null)
            particle.OnParticleSystemDestroyed += onParticleEnded;
        particle.Play ();
    }

    protected void PlayAtkTouchedParticle (Vector3 position, Transform target, Action onParticleEnded = null)
    {
        ParticleSystemCallback particle = GameObject.Instantiate<ParticleSystemCallback> (ParticleTemplate, position, Quaternion.Euler (-90, 0, 0));
        particle.SetTarget (target);
        if (onParticleEnded != null)
            particle.OnParticleSystemDestroyed += onParticleEnded;
        particle.Play ();
    }

    protected T ApplyTargetAttackDefPassive<T> (Attackable target, ref T attackInstance) where T : AttackInstance
    {
        if (target.Stats.AttackDefPassiveByNames.Count == 0)
            return attackInstance;

        List<AttackDefPassive> attackDefPassives = new List<AttackDefPassive> (target.Stats.AttackDefPassiveByNames.Values).OrderBy (x => x.OperationType).ToList ();
        foreach (AttackDefPassive attackDefPassive in attackDefPassives)
        {
            attackDefPassive.AlterAttack (attackInstance);
        }

        return attackInstance;
    }

    private bool DodgeTest (AttackInstance attackInstance)
    {
        return UnityEngine.Random.Range (0f, 1f) > attackInstance.Precision;
    }

    private bool CriticalChanceRoll (AttackInstance attackInstance)
    {
        return UnityEngine.Random.Range (0f, 1f) < attackInstance.CriticalChance;
    }

    // Return true if damage is inflicted
    protected virtual bool TryInflictDamage (Vector3 attackPos, Attackable target, AttackInstance attackInstance, bool dodgeTest = true, bool criticalChanceRoll = true)
    {
        ApplyTargetAttackDefPassive (target, ref attackInstance);

        bool isDodged = dodgeTest ? DodgeTest (attackInstance) : false;
        if (isDodged)
        {
            _fightDescription.ReportAttackDodge (_attacker.Description.DisplayName, target.Description, attackInstance.Name, _attacker.tag);
            _onAttackEnded?.Invoke ();
            return false;
        }

        bool criticalChanceRollResult = criticalChanceRoll ? CriticalChanceRoll (attackInstance) : false;

        if (AnimationTemplate != null)
            PlayAtkTouchedAnimation (attackPos, () => InflictDamage (target, attackInstance, criticalChanceRollResult));
        else if (ParticleTemplate != null)
            PlayAtkTouchedParticle (attackPos, () => InflictDamage (target, attackInstance, criticalChanceRollResult));
        else
            InflictDamage (target, attackInstance, criticalChanceRollResult);

        return true;
    }

    protected virtual void InflictDamage (Attackable target, AttackInstance attackInstance, bool isCritical)
    {
        if (attackInstance == this)
            throw new ArgumentException (nameof (attackInstance) + " must be a copy of this");

        int dammageToInflict = 0;

        if (!NoDamage)
        {
            if (isCritical)
            {
                attackInstance.MinDamage = (int) (attackInstance.MinDamage * attackInstance.CriticalMultiplier);
                attackInstance.MaxDamage = (int) (attackInstance.MaxDamage * attackInstance.CriticalMultiplier);
            }
            dammageToInflict = UnityEngine.Random.Range (attackInstance.MinDamage, attackInstance.MaxDamage + 1);
            _fightDescription.ReportAttackDamage (
                _attacker.Description.DisplayName,
                target.Description.DisplayName,
                attackInstance.AttackElement,
                attackInstance.Name,
                dammageToInflict,
                _attacker.tag,
                isCritical);

            target.FadeSprite ();
            target.Stats.AttackableState.ReceiveDamage (dammageToInflict);
            ApplyEffects (target.Stats.EffectByNames, target, Effect.AttackTimeline.ReceiveAttackDamage, _attacker, attackInstance, dammageToInflict);
        }
        else
        {
            _fightDescription.ReportAttackUse (_attacker.Description.DisplayName, target.Description, attackInstance.Name, _attacker.tag);
        }

        if (_effectInstancesByName.Values.Count == 0)
        {
            TryInvokeCallback ();
            return;
        }

        ApplyEffects (_effectInstancesByName, _attacker, Effect.AttackTimeline.AfterAttackLanded, target, attackInstance, dammageToInflict);
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

    protected void ApplyEffects (Dictionary<string, Effect> effectInstancesByName, Attackable effectOwner, Effect.AttackTimeline timeline, Attackable target, AttackInstance attackInstance, int dammageToInflict = 0)
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