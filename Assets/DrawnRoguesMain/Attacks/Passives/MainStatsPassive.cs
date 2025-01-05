using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "MainStatsPassive", menuName = "Passive/MainStatsPassive", order = 1)]
public class MainStatsPassive : Passive
{
    [SerializeField, BoxGroup ("Settings")]
    private MainStatType _mainStatType;

    public void AlterStats (AttackableStats stats) // effect value is suplied separately because it may be altered by other passives
    {
        switch (_mainStatType)
        {
            case MainStatType.Life:
                float baseLife = stats.BaseLife;
                AlterPropertyValue (ref baseLife);
                stats.BaseLife = (int) baseLife;
                break;
            case MainStatType.Intelligence:
                float baseIntelligence = stats.BaseIntelligence;
                AlterPropertyValue (ref baseIntelligence);
                stats.BaseIntelligence = (int) baseIntelligence;
                break;
            case MainStatType.Strenght:
                float baseStrenght = stats.BaseStrenght;
                AlterPropertyValue (ref baseStrenght);
                stats.BaseStrenght = (int) baseStrenght;
                break;
            case MainStatType.Mobility:
                float baseMobility = stats.BaseMobility;
                AlterPropertyValue (ref baseMobility);
                stats.BaseMobility = (int) baseMobility;
                break;
        }
    }
}

[Serializable]
public class MainStatsPassiveSerialized : PassiveSerialized<MainStatsPassive> { }