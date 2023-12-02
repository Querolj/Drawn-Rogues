using System;
using UnityEngine;

[CreateAssetMenu (fileName = "StunTempEffect", menuName = "TempEffect/StunTempEffect", order = 1)]
public class StunTempEffect : TempEffect
{
    public override void Apply (Attackable effectOwner, FightRegistry fightDescription, Action onAnimeEnded)
    {
        DecrementTurn (effectOwner, fightDescription);

        if (_turnDuration >= 0) // Need to be < 0 to be sure to call OnEffectWearsOff() after the character has his attack blocked
        {
            PlayAnimation (effectOwner.transform.position,
                () =>
                {
                    fightDescription.Report (fightDescription.GetColoredAttackableName (effectOwner.Description, effectOwner.tag) + " is stunned, can't attack next turn.");
                    onAnimeEnded?.Invoke ();
                });
        }
        else
        {
            onAnimeEnded?.Invoke ();
        }

    }

    protected override void OnEffectWearsOff (Attackable effectOwner, FightRegistry fightDescription)
    {
        base.OnEffectWearsOff (effectOwner, fightDescription);

        if (effectOwner.Stats.AttackableState.HasState (State.Stunned))
        {
            effectOwner.Stats.AttackableState.RemoveState (State.Stunned);
            fightDescription.Report (fightDescription.GetColoredAttackableName (effectOwner.Description, effectOwner.tag) + " is no longuer stunned!");
        }
        else
        {
            Debug.LogWarning ("StunTempEffect.OnEffectWearsOff: effectOwner.States does not contain State.Stunned");
        }
    }

    protected override void DecrementTurn (Attackable effectOwner, FightRegistry fightDescription)
    {
        _turnDuration--;
        if (_turnDuration < 0) // Need to be < 0 to be sure to call OnEffectWearsOff() after the character has his attack blocked
        {
            OnEffectWearsOff (effectOwner, fightDescription);
        }
    }
}