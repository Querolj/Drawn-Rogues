using System;
using UnityEngine;

[CreateAssetMenu (fileName = "LifeStealEffect", menuName = "Effect/AttackEffect/LifeStealEffect", order = 1)]
public class LifeStealEffect : Effect
{
    protected override void ApplyOnUserInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightDescription fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user.Description, user.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description, target.tag);
        int healAmount = (int) (_alteredValue * inflictedDamage);

        user.Stats.AttackableState.Heal (healAmount);
        string text = coloredUserName + " devore " + coloredTargetName + " and gain for <color=\"green\"><b>" + healAmount + "</b></color> life.";
        fightDescription.Report (text);
        PlayAnimation (user.GetSpriteBounds ().center, onAnimeEnded);
    }
}