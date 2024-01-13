using UnityEngine;

[CreateAssetMenu (fileName = "AttackTrajectory", menuName = "Attack/AttackTrajectory", order = 1)]
public class AttackTrajectory : AttackSingleTarget
{
    public float TrajectorySpeed = 1f;
    public float TrajectoryRadius = 1f;
}