using System;
using UnityEngine;

[CreateAssetMenu (fileName = "PoisoningTempEffect", menuName = "TempEffect/PoisoningTempEffect", order = 1)]
public class PoisoningTempEffect : TempEffect
{
    public override void Apply (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription, Action onAnimeEnded)
    {
        PlayAnimation (ownerTransform.position,
            () =>
            {
                int poisonDamage = (int) (ownerStats.Life * 0.1f);
                fightDescription.Report (fightDescription.GetColoredAttackableName (ownerName, ownerTransform.tag) + " took <b>" + poisonDamage + "</b> damage from poisoning.");
                ownerStats.AttackableState.ReceiveDamage (poisonDamage);
                DecrementTurn (ownerTransform, ownerName, ownerStats, fightDescription);
                onAnimeEnded?.Invoke ();
            });
    }

    protected override void OnEffectWearsOff (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription)
    {
        base.OnEffectWearsOff (ownerTransform, ownerName, ownerStats, fightDescription);
        if (ownerStats.AttackableState.HasState (State.Poisonned))
            ownerStats.AttackableState.RemoveState (State.Poisonned);

        fightDescription.Report (fightDescription.GetColoredAttackableName (ownerName, ownerTransform.tag) + " is no longuer poisonned!");
    }
}