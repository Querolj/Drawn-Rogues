using System;
using UnityEngine;

[CreateAssetMenu (fileName = "HealEffect", menuName = "Effect/AttackEffect/HealEffect", order = 1)]
public class HealEffect : Effect
{
    protected override void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightDescription fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target);
        int healAmount = (int) (_alteredValue * user.MaxLife);

        user.CurrentLife = (int) Mathf.Clamp (user.CurrentLife + healAmount, 0, user.MaxLife);
        string text = coloredUserName + " heal " + coloredTargetName + " for <color=\"green\"><b>" + healAmount + "</b></color> life.";

        fightDescription.Report (text);
        PlayAnimation (target.GetSpriteBounds ().center, onAnimeEnded);
    }
}