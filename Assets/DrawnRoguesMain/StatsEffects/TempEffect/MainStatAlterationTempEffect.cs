using UnityEngine;
using UnityEngine.Localization.SmartFormat;

[CreateAssetMenu (fileName = "MainStatTempEffect", menuName = "TempEffect/MainStatTempEffect", order = 1)]
public class MainStatAlterationTempEffect : TempEffect
{
    [SerializeField]
    private MainStatType _mainStat;
    public MainStatType MainStat => _mainStat;

    [SerializeField]
    private OperationTypeEnum _operationType;
    public OperationTypeEnum OperationType => _operationType;

    [SerializeField]
    private float _value;

    [SerializeField]
    private string _wearOffText;

    private int _id;
    public int Id
    {
        set => _id = value;
    }

    protected override void OnEffectWearsOff (Attackable attackable, FightRegistry fightDescription)
    {
        attackable.Stats.RemoveMainStatModifier (_id, _mainStat);
        string coloredUserName = fightDescription.GetColoredAttackableName (attackable.Description.DisplayName, attackable.transform.tag);
        string text = Smart.Format (_wearOffText, new { targetName = coloredUserName });
        fightDescription.Report (text);
        _onEffectWoreOff?.Invoke ();
    }
}