using System;
using Sirenix.OdinInspector;
using UnityEngine;
/*
 /!\ No passive alter any temp effect yet (not needed for now)
*/

[CreateAssetMenu (fileName = "TempEffect", menuName = "TempEffect/TempEffect", order = 1)]
public class TempEffect : ScriptableObject
{
    public enum Timeline
    {
        EndRound,
        StartTurn,
        EndTurn
    }

    public string Name;
    public SpriteAnimation AnimationOnApplyTemplate;
    public ParticleSystemCallback ParticleOnApplyTemplate;
    public Timeline EffectApplicationTimeline;
    public Sprite Icon;

    protected int _turnDuration = 1;
    public int TurnDuration { get { return _turnDuration; } }
    protected bool _effectWoreOff = false;
    public bool EffectWoreOff { get { return _effectWoreOff; } }
    public Action _onEffectWoreOff;
    public void Init (int duration)
    {
        _turnDuration = duration;
    }

    public void Init (int duration, Action onEffectWoreOff, Timeline effectApplicationTimeline, Sprite icon)
    {
        _onEffectWoreOff = onEffectWoreOff;
        _turnDuration = duration;
        EffectApplicationTimeline = effectApplicationTimeline;
        Icon = icon;
    }

    public virtual void Apply (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription, Action onAnimeEnded)
    {
        DecrementTurn (ownerTransform, ownerName, ownerStats, fightDescription);
        onAnimeEnded?.Invoke ();
    }

    protected virtual void DecrementTurn (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription)
    {
        _turnDuration--;
        if (_turnDuration <= 0)
        {
            OnEffectWearsOff (ownerTransform, ownerName, ownerStats, fightDescription);
        }
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

    protected virtual void OnEffectWearsOff (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription)
    {
        _onEffectWoreOff?.Invoke ();
        _effectWoreOff = true;
    }
}