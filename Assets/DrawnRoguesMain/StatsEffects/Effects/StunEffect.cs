using System;
using Sirenix.OdinInspector;
using UnityEngine;

// value = chance to inflict poison, for 0% to 100%

[CreateAssetMenu (fileName = "StunEffect", menuName = "Effect/AttackEffect/StunEffect", order = 1)]
public class StunEffect : Effect
{
    [SerializeField, BoxGroup ("Specifics")]
    private StunTempEffect _stunTempEffect;

    [SerializeField, BoxGroup ("Specifics")]
    private int _stunTurnDuration = 1;

    protected override void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user.Description.DisplayName, user.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description.DisplayName, target.tag);

        if (target.HasTempEffect (_stunTempEffect))
        {
            fightDescription.Report (coloredUserName + " can't stun " + coloredTargetName + " as he is already stunned.");
            onAnimeEnded?.Invoke ();
            return;
        }

        if (UnityEngine.Random.Range (0, 1f) < _alteredValue)
        {
            if (!target.Stats.AttackableState.HasState (State.Stunned))
                target.Stats.AttackableState.AddState (State.Stunned);

            fightDescription.Report (coloredUserName + " <b>stunned</b> " + coloredTargetName + " for <b>" + _stunTurnDuration + "</b> turns!");

            TempEffect tmpEffect = ScriptableObject.Instantiate (_stunTempEffect);
            tmpEffect.Init (_stunTurnDuration);
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