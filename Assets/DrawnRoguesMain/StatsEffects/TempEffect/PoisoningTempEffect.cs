using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "PoisoningTempEffect", menuName = "TempEffect/PoisoningTempEffect", order = 1)]
public class PoisoningTempEffect : TempEffect
{
    [SerializeField, BoxGroup ("Specifics")]
    private float _poisonDamagePercentage = 0.07f;
    public override void Apply (Attackable attackable, FightRegistry fightDescription, Action onAnimeEnded)
    {
        Transform attackableTransform = attackable.transform;
        PlayAnimation (attackableTransform.position,
            () =>
            {
                int poisonDamage = (int) (attackable.Stats.MaxLife * _poisonDamagePercentage);
                poisonDamage = Math.Max (poisonDamage, 1);
                string attackableName = attackable.Description.DisplayName;
                fightDescription.Report (fightDescription.GetColoredAttackableName (attackableName, attackableTransform.tag) + " took <b>" + poisonDamage + "</b> damage from poisoning.");
                attackable.Stats.AttackableState.ReceiveDamage (poisonDamage);
                DecrementTurn (attackable, fightDescription);
                onAnimeEnded?.Invoke ();
            });
    }

    protected override void OnEffectWearsOff (Attackable attackable, FightRegistry fightDescription)
    {
        base.OnEffectWearsOff (attackable, fightDescription);
        if (attackable.Stats.AttackableState.HasState (State.Poisonned))
            attackable.Stats.AttackableState.RemoveState (State.Poisonned);

        fightDescription.Report (fightDescription.GetColoredAttackableName (attackable.Description.DisplayName, attackable.transform.tag) + " is no longuer poisonned!");
    }
}