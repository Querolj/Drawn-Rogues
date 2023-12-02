using System;
using UnityEngine;

[CreateAssetMenu (fileName = "HealEffect", menuName = "Effect/AttackEffect/HealEffect", order = 1)]
public class HealEffect : Effect
{
    protected override void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightDescription, Action onAnimeEnded)
    {
        string coloredUserName = fightDescription.GetColoredAttackableName (user.Description, user.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description, target.tag);
        int healAmount = (int) (_alteredValue * user.Stats.Life);
        user.Stats.AttackableState.Heal (healAmount);
        
        string text = coloredUserName + " heal " + coloredTargetName + " for <color=\"green\"><b>" + healAmount + "</b></color> life.";

        fightDescription.Report (text);
        PlayAnimation (target.GetSpriteBounds ().center, onAnimeEnded);
    }
}