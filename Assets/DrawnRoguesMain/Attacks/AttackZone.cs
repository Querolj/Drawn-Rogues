using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "AttackZone", menuName = "Attack/AttackZone", order = 1)]
public class AttackZone : Attack
{
    [SerializeField, BoxGroup ("Specific")]
    public Vector2 ZoneSize;

    [SerializeField, BoxGroup ("Specific")]
    public AttackableDetector AttackableDetectorTemplate;
}