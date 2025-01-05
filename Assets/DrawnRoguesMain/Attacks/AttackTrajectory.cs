using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "AttackTrajectory", menuName = "Attack/AttackTrajectory", order = 1)]
public class AttackTrajectory : AttackSingleTarget
{
    [SerializeField, BoxGroup ("Specific")]
    public float TrajectoryCurveHeight = 0.02f;

    [SerializeField, BoxGroup ("Specific")]
    public float TrajectorySpeedFactor = 10f;

    [SerializeField, BoxGroup ("Specific")]
    public AnimationCurve TrajectorySpeedCurve;

    [SerializeField, BoxGroup ("Specific")]
    public float TrajectoryRadius;
}