using System;
using UnityEngine;

[CreateAssetMenu (fileName = "PoisoningTempEffect", menuName = "TempEffect/PoisoningTempEffect", order = 1)]
public class PoisoningTempEffect : TempEffect
{
    public override void Apply (Attackable effectOwner, FightDescription fightDescription, Action onAnimeEnded)
    {
        PlayAnimation (effectOwner.transform.position,
            () =>
            {
                int poisonDamage = (int) (effectOwner.MaxLife * 0.1f);
                fightDescription.Report (fightDescription.GetColoredAttackableName (effectOwner) + " took <b>" + poisonDamage + "</b> damage from poisoning.");
                effectOwner.ReceiveDamage (poisonDamage);
                DecrementTurn (effectOwner, fightDescription);
                onAnimeEnded?.Invoke ();
            });
    }

    protected override void OnEffectWearsOff (Attackable effectOwner, FightDescription fightDescription)
    {
        base.OnEffectWearsOff (effectOwner, fightDescription);
        if (effectOwner.HasState (State.Poisonned))
            effectOwner.RemoveState (State.Poisonned);

        fightDescription.Report (fightDescription.GetColoredAttackableName (effectOwner) + " is no longuer poisonned!");
    }
}