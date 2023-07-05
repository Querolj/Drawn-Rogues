using System;
using UnityEngine;

[CreateAssetMenu (fileName = "StunTempEffect", menuName = "TempEffect/StunTempEffect", order = 1)]
public class StunTempEffect : TempEffect
{
    public override void Apply (Attackable effectOwner, FightDescription fightDescription, Action onAnimeEnded)
    {
        DecrementTurn (effectOwner, fightDescription);

        if (_turnDuration >= 0) // Need to be < 0 to be sure to call OnEffectWearsOff() after the character has his attack blocked
        {
            PlayAnimation (effectOwner.transform.position,
                () =>
                {
                    fightDescription.Report (fightDescription.GetColoredAttackableName (effectOwner) + " is stunned, can't attack next turn.");
                    onAnimeEnded?.Invoke ();
                });
        }
        else
        {
            onAnimeEnded?.Invoke ();
        }

    }

    protected override void OnEffectWearsOff (Attackable effectOwner, FightDescription fightDescription)
    {
        base.OnEffectWearsOff (effectOwner, fightDescription);

        if (effectOwner.HasState (State.Stunned))
        {
            effectOwner.RemoveState (State.Stunned);
            fightDescription.Report (fightDescription.GetColoredAttackableName (effectOwner) + " is no longuer stunned!");
        }
        else
        {
            Debug.LogWarning ("StunTempEffect.OnEffectWearsOff: effectOwner.States does not contain State.Stunned");
        }
    }

    protected override void DecrementTurn (Attackable effectOwner, FightDescription fightDescription)
    {
        _turnDuration--;
        if (_turnDuration < 0) // Need to be < 0 to be sure to call OnEffectWearsOff() after the character has his attack blocked
        {
            OnEffectWearsOff (effectOwner, fightDescription);
        }
    }
}