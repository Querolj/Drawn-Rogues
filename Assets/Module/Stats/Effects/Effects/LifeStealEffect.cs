using System;
using UnityEngine;

[CreateAssetMenu (fileName = "LifeStealEffect", menuName = "Effect/AttackEffect/LifeStealEffect", order = 1)]
public class LifeStealEffect : Effect
{
    protected override void ApplyOnUserInternal (AttackableInfosPackage userInfo, AttackableInfosPackage targetInfo, AttackType attackType, int inflictedDamage, FightRegistry fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (userDescription.DisplayName, userTransform.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description.DisplayName, target.tag);
        int healAmount = (int) (_alteredValue * inflictedDamage);

        attackableStats.AttackableState.Heal (healAmount);
        string text = coloredUserName + " devore " + coloredTargetName + " and gain for <color=\"green\"><b>" + healAmount + "</b></color> life.";
        fightDescription.Report (text);
        PlayAnimation (user.GetSpriteBounds ().center, onAnimeEnded);
    }
}