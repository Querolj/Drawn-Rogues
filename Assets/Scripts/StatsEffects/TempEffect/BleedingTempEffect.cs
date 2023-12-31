using System;
using UnityEngine;

[CreateAssetMenu (fileName = "BleedingTempEffect", menuName = "TempEffect/BleedingTempEffect", order = 1)]
public class BleedingTempEffect : TempEffect
{
    const string bleeding = "<color=\"red\"><b>bleeding</b></color>";

    [HideInInspector]
    public Vector3 LastOwnerPosition;

    private const float BLEED_STRENGHT = 8f;
    public override void Apply (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription, Action onAnimeEnded)
    {
        float distance = Vector3.Distance (ownerTransform.position, LastOwnerPosition);

        PlayAnimation (ownerTransform.position,
            () =>
            {
                int bleedDamage = (int) (distance * BLEED_STRENGHT);
                bleedDamage = Mathf.Clamp (bleedDamage, 1, bleedDamage);
                fightDescription.Report (fightDescription.GetColoredAttackableName (ownerName, ownerTransform.tag) + " took <b>" + bleedDamage + "</b> damage from " + bleeding + ".");
                ownerStats.AttackableState.ReceiveDamage (bleedDamage);
                DecrementTurn (ownerTransform, ownerName, ownerStats, fightDescription);
                LastOwnerPosition = ownerTransform.position;
                onAnimeEnded?.Invoke ();
            });
    }

    protected override void OnEffectWearsOff (Transform ownerTransform, string ownerName, AttackableStats ownerStats, FightRegistry fightDescription)
    {
        base.OnEffectWearsOff (ownerTransform, ownerName, ownerStats, fightDescription);
        if (ownerStats.AttackableState.HasState (State.Bleed))
            ownerStats.AttackableState.RemoveState (State.Bleed);

        fightDescription.Report (fightDescription.GetColoredAttackableName (ownerName, ownerTransform.tag) + " is no longuer " + bleeding + "!");
    }
}