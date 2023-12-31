using System;
using UnityEngine;

[CreateAssetMenu (fileName = "StunTempEffect", menuName = "TempEffect/StunTempEffect", order = 1)]
public class StunTempEffect : TempEffect
{
    public override void Apply (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription, Action onAnimeEnded)
    {
        DecrementTurn (ownerTransform, ownerName, ownerStats, fightDescription);

        if (_turnDuration >= 0) // Need to be < 0 to be sure to call OnEffectWearsOff() after the character has his attack blocked
        {
            PlayAnimation (ownerTransform.position,
                () =>
                {
                    fightDescription.Report (fightDescription.GetColoredAttackableName (ownerName, ownerTransform.tag) + " is stunned, can't attack next turn.");
                    onAnimeEnded?.Invoke ();
                });
        }
        else
        {
            onAnimeEnded?.Invoke ();
        }

    }

    protected override void OnEffectWearsOff (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription)
    {
        base.OnEffectWearsOff (ownerTransform, ownerName, ownerStats, fightDescription);

        if (ownerStats.AttackableState.HasState (State.Stunned))
        {
            ownerStats.AttackableState.RemoveState (State.Stunned);
            fightDescription.Report (fightDescription.GetColoredAttackableName (ownerName, ownerTransform.tag) + " is no longuer stunned!");
        }
        else
        {
            Debug.LogWarning ("StunTempEffect.OnEffectWearsOff: effectOwner.States does not contain State.Stunned");
        }
    }

    protected override void DecrementTurn (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription)
    {
        _turnDuration--;
        if (_turnDuration < 0) // Need to be < 0 to be sure to call OnEffectWearsOff() after the character has his attack blocked
        {
            OnEffectWearsOff (ownerTransform, ownerName, ownerStats, fightDescription);
        }
    }
}