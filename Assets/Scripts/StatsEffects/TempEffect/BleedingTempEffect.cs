using System;
using UnityEngine;

[CreateAssetMenu (fileName = "BleedingTempEffect", menuName = "TempEffect/BleedingTempEffect", order = 1)]
public class BleedingTempEffect : TempEffect
{
    const string bleeding = "<color=\"red\"><b>bleeding</b></color>";

    [HideInInspector]
    public Vector3 LastOwnerPosition;

    private const float BLEED_STRENGHT = 8f;
    public override void Apply (Attackable effectOwner, FightDescription fightDescription, Action onAnimeEnded)
    {
        float distance = Vector3.Distance (effectOwner.transform.position, LastOwnerPosition);

        PlayAnimation (effectOwner.transform.position,
            () =>
            {
                int bleedDamage = (int) (distance * BLEED_STRENGHT);
                bleedDamage = Mathf.Clamp (bleedDamage, 1, bleedDamage);
                fightDescription.Report (fightDescription.GetColoredAttackableName (effectOwner.Description, effectOwner.tag) + " took <b>" + bleedDamage + "</b> damage from " + bleeding + ".");
                effectOwner.Stats.AttackableState.ReceiveDamage (bleedDamage);
                DecrementTurn (effectOwner, fightDescription);
                LastOwnerPosition = effectOwner.transform.position;
                onAnimeEnded?.Invoke ();
            });
    }

    protected override void OnEffectWearsOff (Attackable effectOwner, FightDescription fightDescription)
    {
        base.OnEffectWearsOff (effectOwner, fightDescription);
        if (effectOwner.Stats.AttackableState.HasState (State.Bleed))
            effectOwner.Stats.AttackableState.RemoveState (State.Bleed);

        fightDescription.Report (fightDescription.GetColoredAttackableName (effectOwner.Description, effectOwner.tag) + " is no longuer " + bleeding + "!");
    }
}