using System;
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

    public virtual void Apply (Attackable effectOwner, FightDescription fightDescription, Action onAnimeEnded)
    {
        DecrementTurn (effectOwner, fightDescription);
        onAnimeEnded?.Invoke ();
    }

    protected virtual void DecrementTurn (Attackable effectOwner, FightDescription fightDescription)
    {
        _turnDuration--;
        if (_turnDuration <= 0)
        {
            OnEffectWearsOff (effectOwner, fightDescription);
        }
    }

    protected void PlayAnimation (Vector3 position, Action onEnded)
    {
        SpriteAnimation anime = Instantiate<SpriteAnimation> (AnimationOnApplyTemplate, position, Quaternion.identity);
        anime.OnAnimationEnded += onEnded;
        anime.Play ();
    }

    protected void PlayParticle (Vector3 position, Action onParticleEnded)
    {
        ParticleSystemCallback particle = GameObject.Instantiate<ParticleSystemCallback> (ParticleOnApplyTemplate, position, Quaternion.Euler (-90, 0, 0));
        particle.OnParticleSystemDestroyed += onParticleEnded;
        particle.Play ();
    }

    protected virtual void OnEffectWearsOff (Attackable effectOwner, FightDescription fightDescription)
    {
        _onEffectWoreOff?.Invoke ();
        _effectWoreOff = true;
    }
}