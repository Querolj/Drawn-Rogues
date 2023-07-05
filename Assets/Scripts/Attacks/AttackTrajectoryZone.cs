using UnityEngine;

[CreateAssetMenu (fileName = "AttackTrajectoryZone", menuName = "Attack/AttackTrajectoryZone", order = 1)]
public class AttackTrajectoryZone : AttackZone
{
    public float TrajectorySpeed = 1f;
    public float TrajectoryRadius = 1f;
}