using System;
using UnityEngine;

[CreateAssetMenu (fileName = "StunTempEffect", menuName = "TempEffect/StunTempEffect", order = 1)]
public class StunTempEffect : TempEffect
{
    public override void Apply (Attackable attackable, FightRegistry fightDescription, Action onAnimeEnded)
    {
        DecrementTurn (attackable, fightDescription);

        if (_turnDuration >= 0) // Need to be < 0 to be sure to call OnEffectWearsOff() after the character has his attack blocked
        {
            Transform attackableTransform = attackable.transform;
            PlayAnimation (attackableTransform.position,
                () =>
                {
                    fightDescription.Report (fightDescription.GetColoredAttackableName (attackable.Description.DisplayName, attackableTransform.tag) + " is stunned, can't attack next turn.");
                    onAnimeEnded?.Invoke ();
                });
        }
        else
        {
            onAnimeEnded?.Invoke ();
        }

    }

    protected override void OnEffectWearsOff (Attackable attackable, FightRegistry fightDescription)
    {
        base.OnEffectWearsOff (attackable, fightDescription);

        if (attackable.Stats.AttackableState.HasState (State.Stunned))
        {
            attackable.Stats.AttackableState.RemoveState (State.Stunned);
            fightDescription.Report (fightDescription.GetColoredAttackableName (attackable.Description.DisplayName, attackable.transform.tag) + " is no longuer stunned!");
        }
        else
        {
            Debug.LogWarning ("StunTempEffect.OnEffectWearsOff: effectOwner.States does not contain State.Stunned");
        }
    }

    protected override void DecrementTurn (Attackable attackable, FightRegistry fightDescription)
    {
        _turnDuration--;
        if (_turnDuration <= 0) // Need to be < 0 to be sure to call OnEffectWearsOff() after the character has his attack blocked
        {
            OnEffectWearsOff (attackable, fightDescription);
        }
    }
}