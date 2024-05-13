using System;
using UnityEngine;

[CreateAssetMenu (fileName = "PoisoningTempEffect", menuName = "TempEffect/PoisoningTempEffect", order = 1)]
public class PoisoningTempEffect : TempEffect
{
    [SerializeField]
    private float _poisonDamagePercentage = 0.07f;
    public override void Apply (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription, Action onAnimeEnded)
    {
        PlayAnimation (ownerTransform.position,
            () =>
            {
                int poisonDamage = (int) (ownerStats.Life * _poisonDamagePercentage);
                poisonDamage = Math.Max (poisonDamage, 1);
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