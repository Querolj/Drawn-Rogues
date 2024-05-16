using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "BleedingTempEffect", menuName = "TempEffect/BleedingTempEffect", order = 1)]
public class BleedingTempEffect : TempEffect
{

    [SerializeField, InfoBox ("percentage of life lost per meter moved"), BoxGroup ("Specifics")]
    private float _bleedStrenght = 0.1f;

    [SerializeField, BoxGroup ("Specifics")]
    private float _maxPercentageLifeLost = 0.33f;

    [SerializeField, BoxGroup ("Specifics")]
    private string bleeding = "<color=\"red\"><b>bleeding</b></color>";

    [HideInInspector]
    public Vector3 LastOwnerPosition;

    public override void Apply (Attackable attackable, FightRegistry fightDescription, Action onAnimeEnded)
    {
        Transform attackableTransform = attackable.transform;
        float distance = Vector3.Distance (attackableTransform.position, LastOwnerPosition);

        PlayAnimation (attackableTransform.position,
            () =>
            {
                float percentageOfLifeLost = distance * _bleedStrenght;
                percentageOfLifeLost = Mathf.Clamp (percentageOfLifeLost, 0.01f, _maxPercentageLifeLost);
                int bleedDamage = (int) (distance * _bleedStrenght * attackable.Stats.Life);
                bleedDamage = Mathf.Max (bleedDamage, 1);
                fightDescription.Report (fightDescription.GetColoredAttackableName (attackable.Description.DisplayName, attackableTransform.tag) + " took <b>" + bleedDamage + "</b> damage from " + bleeding + ".");
                attackable.Stats.AttackableState.ReceiveDamage (bleedDamage);
                DecrementTurn (attackable, fightDescription);
                LastOwnerPosition = attackableTransform.position;
                onAnimeEnded?.Invoke ();
            });
    }

    protected override void OnEffectWearsOff (Attackable attackable, FightRegistry fightDescription)
    {
        base.OnEffectWearsOff (attackable, fightDescription);
        if (attackable.Stats.AttackableState.HasState (State.Bleed))
            attackable.Stats.AttackableState.RemoveState (State.Bleed);

        fightDescription.Report (fightDescription.GetColoredAttackableName (attackable.Description.DisplayName, attackable.transform.tag) + " is no longuer " + bleeding + "!");
    }
}