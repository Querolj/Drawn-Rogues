using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "BleedEffect", menuName = "Effect/AttackEffect/BleedEffect", order = 1)]
public class BleedEffect : Effect
{
    [SerializeField, BoxGroup ("Specifics")]
    private int _bleedTurnDuration = 3;

    [SerializeField, BoxGroup ("Specifics")]
    private BleedingTempEffect _bleedingEffect;

    protected override void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user.Description.DisplayName, user.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description.DisplayName, target.tag);

        if (target.HasTempEffect (_bleedingEffect))
        {
            fightDescription.Report (coloredUserName + " can't make " + coloredTargetName + " bleed as he is already bleeding.");
            onAnimeEnded?.Invoke ();
            return;
        }

        const string bleed = "<color=\"red\"><b>bleed</b></color>";
        if (UnityEngine.Random.Range (0, 1f) < _alteredValue)
        {
            if (!target.Stats.AttackableState.HasState (State.Bleed))
                target.Stats.AttackableState.AddState (State.Bleed);

            fightDescription.Report (coloredUserName + " make " + coloredTargetName + " " + bleed + " for <b>" + _bleedTurnDuration + "</b> turns!");

            BleedingTempEffect tmpEffect = ScriptableObject.Instantiate (_bleedingEffect);
            tmpEffect.LastOwnerPosition = target.GetSpriteBounds ().center;
            tmpEffect.Init (_bleedTurnDuration);
            target.AddTempEffect (tmpEffect);
            PlayAnimation (target.GetSpriteBounds ().center, onAnimeEnded);
        }
        else
        {
            fightDescription.Report (coloredUserName + " failed to make " + coloredTargetName + " " + bleed + ".");
            onAnimeEnded?.Invoke ();
        }
    }
}