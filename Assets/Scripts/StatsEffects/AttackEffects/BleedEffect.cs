using System;
using UnityEngine;

[CreateAssetMenu (fileName = "BleedEffect", menuName = "Effect/AttackEffect/BleedEffect", order = 1)]
public class BleedEffect : Effect
{
    private const int BLEED_DURATION = 3;

    public BleedingTempEffect BleedingEffect;

    protected override void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightDescription fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target);

        if (TargetHasTempEffect (target, BleedingEffect, TempEffect.Timeline.EndTurn))
        {
            fightDescription.Report (coloredUserName + " can't make " + coloredTargetName + " bleed as he is already bleeding.");
            onAnimeEnded?.Invoke ();
            return;
        }

        const string bleed = "<color=\"red\"><b>bleed</b></color>";
        if (UnityEngine.Random.Range (0, 100f) < _alteredValue)
        {
            if (!target.HasState (State.Bleed))
                target.AddState (State.Bleed);

            fightDescription.Report (coloredUserName + " make " + coloredTargetName + " " + bleed + " for <b>" + BLEED_DURATION + "</b> turns!");

            BleedingTempEffect tmpEffect = ScriptableObject.Instantiate (BleedingEffect) as BleedingTempEffect;
            tmpEffect.LastOwnerPosition = target.GetSpriteBounds ().center;
            tmpEffect.Init (BLEED_DURATION);
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