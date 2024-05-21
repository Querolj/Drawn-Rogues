using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "BurnEffect", menuName = "Effect/AttackEffect/BurnEffect", order = 1)]
public class BurnEffect : Effect
{
    [SerializeField, BoxGroup ("Specifics")]
    private int _burnTurnDuration = 3;

    [SerializeField, BoxGroup ("Specifics")]
    private BurningTempEffect _burningEffect;

    protected override void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user.Description.DisplayName, user.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description.DisplayName, target.tag);

        if (target.Stats.AttackableState.HasState (State.Burn))
        {
            fightDescription.Report (coloredUserName + " can't make " + coloredTargetName + " burn as he is already burning.");
            onAnimeEnded?.Invoke ();
            return;
        }

        const string burn = "<color=\"red\"><b>burn</b></color>";
        if (UnityEngine.Random.Range (0, 1f) < _alteredValue)
        {
            if (!target.Stats.AttackableState.HasState (State.Burn))
                target.Stats.AttackableState.AddState (State.Burn);

            fightDescription.Report (coloredUserName + " make " + coloredTargetName + " " + burn + " for <b>" + _burnTurnDuration + "</b> turns!");

            BurningTempEffect tmpEffect = ScriptableObject.Instantiate (_burningEffect);
            tmpEffect.Init (_burnTurnDuration);
            target.AddTempEffect (tmpEffect);
            PlayAnimation (target.GetSpriteBounds ().center, onAnimeEnded);
        }
        else
        {
            fightDescription.Report (coloredUserName + " failed to make " + coloredTargetName + " " + burn + ".");
            onAnimeEnded?.Invoke ();
        }
    }
}