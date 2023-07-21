using System;
using UnityEngine;

[CreateAssetMenu (fileName = "LifeStealEffect", menuName = "Effect/AttackEffect/LifeStealEffect", order = 1)]
public class LifeStealEffect : Effect
{
    protected override void ApplyOnUserInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightDescription fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target);
        int healAmount = (int) (_alteredValue * inflictedDamage);

        user.CurrentLife = (int) Mathf.Clamp (user.CurrentLife + healAmount, 0, user.MaxLife);
        string text = coloredUserName + " devore " + coloredTargetName + " and gain for <color=\"green\"><b>" + healAmount + "</b></color> life.";
        fightDescription.Report (text);
        PlayAnimation (user.GetSpriteBounds ().center, onAnimeEnded);
    }
}