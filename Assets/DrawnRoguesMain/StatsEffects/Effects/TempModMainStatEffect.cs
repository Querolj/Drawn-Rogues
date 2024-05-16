using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;

[CreateAssetMenu (fileName = "TempModMainStatEffect", menuName = "Effect/AttackEffect/TempModMainStatEffect", order = 1)]
public class TempModMainStatEffect : Effect
{
    [SerializeField, BoxGroup ("Specifics")]
    private MainStatAlterationTempEffect _tempEffect;

    [SerializeField, BoxGroup ("Specifics")]
    private int _turnDuration;

    [SerializeField, InfoBox ("available arguments are :\n{attackerName}\n{value}\n{targetname}\n{stat}"), BoxGroup ("Specifics")]
    private string _applyEffectText;

    [SerializeField, InfoBox ("available arguments are :\n{attackerName}\n{value}\n{targetname}"), BoxGroup ("Specifics")]
    private string _failRollText;

    private Character _targetChar;

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

        if (target.HasTempEffect (_tempEffect))
        {
            fightDescription.Report (coloredUserName + " can't alter " + coloredTargetName + " stats as its stats are already altered by the same effect.");
            onAnimeEnded?.Invoke ();
            return;
        }

        if (UnityEngine.Random.Range (0, 1f) < _alteredValue)
        {
            MainStatAlterationTempEffect tmpEffect = ScriptableObject.Instantiate (_tempEffect);
            int id = GetInstanceID ();
            _targetChar.Stats.AddMainStatModifier (id, tmpEffect.MainStat, tmpEffect.OperationType, _alteredValue);
            tmpEffect.Id = id;
            tmpEffect.Init (_turnDuration);
            target.AddTempEffect (tmpEffect);

            string text = Smart.Format (_applyEffectText, new { attackerName = coloredUserName, value = _alteredValue, targetname = coloredTargetName, stat = tmpEffect.MainStat });
            fightDescription.Report (text);
            PlayAnimation (target.transform.position, onAnimeEnded);
        }
        else
        {
            string text = Smart.Format (_failRollText, new { attackerName = coloredUserName, value = _alteredValue, targetname = coloredTargetName });
            fightDescription.Report (text);
            onAnimeEnded?.Invoke ();
        }
    }
}