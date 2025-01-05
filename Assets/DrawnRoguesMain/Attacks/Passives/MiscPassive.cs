using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "MiscPassive", menuName = "Passive/MiscPassive", order = 1)]
public class MiscPassive : Passive
{
    public enum MiscPassiveType
    {
        PixelDropMultiplier
    }

    [SerializeField, BoxGroup ("Settings")]
    private MiscPassiveType _miscPassiveType;

    public void AlterStats (AttackableStats stats) // effect value is suplied separately because it may be altered by other passives
    {
        switch (_miscPassiveType)
        {
            case MiscPassiveType.PixelDropMultiplier:
                float pixelDropMultiplier = stats.MiscStats.PixelDropMultiplier;
                AlterPropertyValue (ref pixelDropMultiplier);
                stats.MiscStats.PixelDropMultiplier = pixelDropMultiplier;
                break;
        }
    }
}

[Serializable]
public class MiscPassiveSerialized : PassiveSerialized<MiscPassive> { }