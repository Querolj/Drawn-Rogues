using UnityEngine;

[CreateAssetMenu (fileName = "AttackZone", menuName = "Attack/AttackZone", order = 1)]
public class AttackZone : Attack
{
    public Vector2 ZoneSize;
    public AttackableDetector AttackableDetectorTemplate;
}