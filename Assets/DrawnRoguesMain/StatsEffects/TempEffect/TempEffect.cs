using System;
using UnityEngine;
/*
 /!\ No passive alter any temp effect yet (not needed for now)
*/

/*
    TODO : To be renamed to Status ?? It has actually nothing to do with the Effect class, the naming is confusing
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
    public bool EffectWoreOff { get { return _turnDuration < 1; } }
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

    public virtual void Apply (Attackable attackable, FightRegistry fightDescription, Action onAnimeEnded)
    {
        PlayAnimation (attackable.transform.position,
            () =>
            {
                DecrementTurn (attackable, fightDescription);
                onAnimeEnded?.Invoke ();
            });
    }

    protected virtual void DecrementTurn (Attackable attackable, FightRegistry fightDescription)
    {
        _turnDuration--;
        if (_turnDuration <= 0)
        {
            OnEffectWearsOff (attackable, fightDescription);
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

    protected virtual void OnEffectWearsOff (Attackable attackable, FightRegistry fightDescription)
    {
        _onEffectWoreOff?.Invoke ();
    }
}