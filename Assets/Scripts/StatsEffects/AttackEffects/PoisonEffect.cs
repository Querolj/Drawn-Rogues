using System;
using UnityEngine;

// value = chance to inflict poison, for 0% to 100%

[CreateAssetMenu (fileName = "PoisonEffect", menuName = "Effect/AttackEffect/PoisonEffect", order = 1)]
public class PoisonEffect : Effect
{
    private const int POISON_DURATION = 3;

    public TempEffect TempEffect;

    protected override void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightDescription fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target);

        if (TargetHasTempEffect (target, TempEffect, TempEffect.Timeline.EndRound))
        {
            fightDescription.Report (coloredUserName + " can't poison " + coloredTargetName + " as he is already poisoned.");
            onAnimeEnded?.Invoke ();
            return;
        }

        if (UnityEngine.Random.Range (0, 100f) < _alteredValue)
        {
            if (!target.HasState (State.Poisonned))
                target.AddState (State.Poisonned);

            fightDescription.Report (coloredUserName + " <b>poisoned</b> " + coloredTargetName + " for <b>" + POISON_DURATION + "</b> turns!");

            PoisoningTempEffect tmpEffect = ScriptableObject.Instantiate (TempEffect) as PoisoningTempEffect;
            tmpEffect.Init (POISON_DURATION);
            target.AddTempEffect (tmpEffect);
            PlayAnimation (target.GetSpriteBounds ().center, onAnimeEnded);
        }
        else
        {
            fightDescription.Report (coloredUserName + " failed to <b>poison</b> " + coloredTargetName + ".");
            onAnimeEnded?.Invoke ();
        }
    }

}