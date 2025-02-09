using System;
using UnityEngine;

[CreateAssetMenu (fileName = "LifeStealEffect", menuName = "Effect/AttackEffect/LifeStealEffect", order = 1)]
public class LifeStealEffect : Effect
{
    protected override void ApplyOnUserInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user.Description.DisplayName, user.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description.DisplayName, target.tag);
        int healAmount = 0;
        if (inflictedDamage > 0)
        {
            healAmount = (int) (_alteredValue * inflictedDamage);
            healAmount = Math.Max (healAmount, 1);
        }

        user.Stats.AttackableState.Heal (healAmount);
        string text = coloredUserName + " devore " + coloredTargetName + " and gain for <color=\"green\"><b>" + healAmount + "</b></color> life.";
        fightDescription.Report (text);
        PlayAnimation (user.GetSpriteBounds ().center, onAnimeEnded);
    }
}