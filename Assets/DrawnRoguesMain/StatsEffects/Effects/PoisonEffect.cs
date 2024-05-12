using System;
using UnityEngine;

// value = chance to inflict poison, for 0% to 100%

[CreateAssetMenu (fileName = "PoisonEffect", menuName = "Effect/AttackEffect/PoisonEffect", order = 1)]
public class PoisonEffect : Effect
{
    [SerializeField]
    private int _poisonDuration = 3;

    public TempEffect TempEffect;

    protected override void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user.Description.DisplayName, user.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description.DisplayName, target.tag);

        if (target.HasTempEffect (TempEffect))
        {
            fightDescription.Report (coloredUserName + " can't poison " + coloredTargetName + " as he is already poisoned.");
            onAnimeEnded?.Invoke ();
            return;
        }

        if (UnityEngine.Random.Range (0, 100f) < _alteredValue)
        {
            if (!target.Stats.AttackableState.HasState (State.Poisonned))
                target.Stats.AttackableState.AddState (State.Poisonned);

            fightDescription.Report (coloredUserName + " <b>poisoned</b> " + coloredTargetName + " for <b>" + _poisonDuration + "</b> turns!");

            PoisoningTempEffect tmpEffect = ScriptableObject.Instantiate (TempEffect) as PoisoningTempEffect;
            tmpEffect.Init (_poisonDuration);
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