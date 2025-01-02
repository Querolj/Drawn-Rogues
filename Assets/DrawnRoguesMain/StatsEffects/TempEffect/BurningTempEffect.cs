using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "BurningTempEffect", menuName = "TempEffect/BurningTempEffect", order = 1)]
public class BurningTempEffect : TempEffect
{
    [SerializeField, BoxGroup ("Specifics")]
    private float _maxBurnStrenght = 0.1f;

    [SerializeField, BoxGroup ("Specifics")]
    private float _minBurnStrenght = 0.01f;

    [SerializeField, BoxGroup ("Specifics")]
    private float _burnStrenghtSubstractedPerMeterMoved = 0.01f;

    [SerializeField, BoxGroup ("Specifics")]
    private string burning = "<color=\"red\"><b>burning</b></color>";

    private Vector3 _lastOwnerPosition;
    private bool _firstApply = true;

    public override void Apply (Attackable attackable, FightRegistry fightDescription, Action onAnimeEnded)
    {
        Transform attackableTransform = attackable.transform;
        float distance = 0;
        if (!_firstApply)
            distance = Vector3.Distance (attackableTransform.position, _lastOwnerPosition);

        _firstApply = false;
        PlayAnimation (attackableTransform.position,
            () =>
            {
                float percentageOfLifeLost = Mathf.Clamp (_maxBurnStrenght - (distance * _burnStrenghtSubstractedPerMeterMoved), _minBurnStrenght, _maxBurnStrenght);
                int burnDamage = Mathf.RoundToInt (percentageOfLifeLost * attackable.Stats.MaxLife);
                burnDamage = Mathf.Max (burnDamage, 1);
                fightDescription.Report (fightDescription.GetColoredAttackableName (attackable.Description.DisplayName, attackableTransform.tag) + " took <b>" + burnDamage + "</b> damage from " + burning + ".");
                attackable.Stats.AttackableState.ReceiveDamage (burnDamage);
                DecrementTurn (attackable, fightDescription);
                _lastOwnerPosition = attackableTransform.position;
                onAnimeEnded?.Invoke ();
            });
    }

    protected override void OnEffectWearsOff (Attackable attackable, FightRegistry fightDescription)
    {
        base.OnEffectWearsOff (attackable, fightDescription);
        if (attackable.Stats.AttackableState.HasState (State.Bleed))
            attackable.Stats.AttackableState.RemoveState (State.Bleed);

        fightDescription.Report (fightDescription.GetColoredAttackableName (attackable.Description.DisplayName, attackable.transform.tag) + " is no longuer " + burning + "!");
    }
}