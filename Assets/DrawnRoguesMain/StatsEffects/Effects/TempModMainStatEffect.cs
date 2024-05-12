using System;
using UnityEngine;

[CreateAssetMenu (fileName = "TempModMainStatEffect", menuName = "Effect/AttackEffect/TempModMainStatEffect", order = 1)]
public class TempModMainStatEffect : Effect
{
    public MainStatType MainStat;
    public OperationTypeEnum OperationType;
    public int TurnDuration = 1;
    private Character _targetChar;
    public Sprite Icon;

    protected override void ApplyOnTargetInternal (Character user, AttackInstance attack, Attackable target, int inflictedDamage, FightRegistry fightDescription, Action onAnimeEnded)
    {
        _targetChar = target as Character;
        if (_targetChar == null)
        {
            Debug.LogError ("TempModMainStatEffect can only be applied on Character");
            onAnimeEnded?.Invoke ();
            return;
        }

        string coloredUserName = fightDescription.GetColoredAttackableName (user.Description.DisplayName, user.tag);
        string coloredTargetName = fightDescription.GetColoredAttackableName (target.Description.DisplayName, target.tag);
        int id = GetInstanceID ();

        if (target.TempEffects.ContainsKey (TempEffect.Timeline.EndTurn) && target.TempEffects[TempEffect.Timeline.EndTurn].Find (x => x.Name == Description) != null)
        {
            fightDescription.Report (coloredUserName + " can't alter " + coloredTargetName + " stats as its stats are already altered by the same effect.");
            onAnimeEnded?.Invoke ();
            return;
        }

        _targetChar.Stats.AddMainStatModifier (id, MainStat, OperationType, _alteredValue);
        TempEffect tmpEffect = new TempEffect ();
        tmpEffect.Name = Description;
        tmpEffect.Init (TurnDuration + 1, WearOffEffect, TempEffect.Timeline.EndTurn, Icon);
        target.AddTempEffect (tmpEffect);

        string text;
        switch (OperationType)
        {
            case OperationTypeEnum.Add:
                text = coloredUserName + " add " + _alteredValue + " of " + MainStat + " to " + coloredTargetName;
                break;
            case OperationTypeEnum.Substract:
                text = coloredUserName + " remove " + _alteredValue + " of " + MainStat + " to " + coloredTargetName;
                break;
            case OperationTypeEnum.AddPercentage:
                text = coloredUserName + " add " + _alteredValue + "% of " + MainStat + " to " + coloredTargetName;
                break;
            case OperationTypeEnum.Set:
                text = coloredUserName + " set " + coloredTargetName + " " + MainStat + " to " + _alteredValue;
                break;
            default:
                throw new Exception ("OperationType not supported");
        }

        fightDescription.Report (text);
        PlayAnimation (target.transform.position, onAnimeEnded);
    }

    private void WearOffEffect ()
    {
        _targetChar.Stats.RemoveMainStatModifier (GetInstanceID (), MainStat);
    }
}