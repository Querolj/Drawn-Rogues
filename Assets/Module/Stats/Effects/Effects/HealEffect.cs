using System;
using UnityEngine;

[CreateAssetMenu (fileName = "HealEffect", menuName = "Effect/AttackEffect/HealEffect", order = 1)]
public class HealEffect : Effect
{
    protected override void ApplyOnTargetInternal (AttackableInfosPackage userInfo, AttackableInfosPackage targetInfo, AttackType attackType, int inflictedDamage, FightRegistry fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (userDescription.DisplayName, userTransform.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description.DisplayName, target.tag);
        int healAmount = (int) (_alteredValue * attackableStats.Life);
        attackableStats.AttackableState.Heal (healAmount);

        string text = coloredUserName + " heal " + coloredTargetName + " for <color=\"green\"><b>" + healAmount + "</b></color> life.";

        fightDescription.Report (text);
        PlayAnimation (target.GetSpriteBounds ().center, onAnimeEnded);
    }
}