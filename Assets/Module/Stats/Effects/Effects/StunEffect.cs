using System;
using UnityEngine;

// value = chance to inflict poison, for 0% to 100%

[CreateAssetMenu (fileName = "StunEffect", menuName = "Effect/AttackEffect/StunEffect", order = 1)]
public class StunEffect : Effect
{
    private const int STUN_DURATION = 1;

    public StunTempEffect StunTempEffect;

    protected override void ApplyOnTargetInternal (AttackableInfosPackage userInfo, AttackableInfosPackage targetInfo, AttackType attackType, int inflictedDamage, FightRegistry fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (userDescription.DisplayName, userTransform.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description.DisplayName, target.tag);

        if (TargetHasTempEffect (target, StunTempEffect, TempEffect.Timeline.StartTurn))
        {
            fightDescription.Report (coloredUserName + " can't stun " + coloredTargetName + " as he is already stunned.");
            onAnimeEnded?.Invoke ();
            return;
        }

        if (UnityEngine.Random.Range (0, 100f) < _alteredValue)
        {
            if (!target.Stats.AttackableState.HasState (State.Stunned))
                target.Stats.AttackableState.AddState (State.Stunned);

            fightDescription.Report (coloredUserName + " <b>stunned</b> " + coloredTargetName + " for <b>" + STUN_DURATION + "</b> turns!");

            TempEffect tmpEffect = ScriptableObject.Instantiate (StunTempEffect);
            tmpEffect.Init (STUN_DURATION);
            target.AddTempEffect (tmpEffect);
            PlayAnimation (target.GetSpriteBounds ().center, onAnimeEnded);
        }
        else
        {
            fightDescription.Report (coloredUserName + " failed to <b>stun</b> " + coloredTargetName + ".");
            onAnimeEnded?.Invoke ();
        }
    }

}