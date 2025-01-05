using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "AttackTrajectoryZone", menuName = "Attack/AttackTrajectoryZone", order = 1)]
public class AttackTrajectoryZone : AttackZone
{
    [SerializeField, BoxGroup ("Specific")]
    public float TrajectorySpeed = 1f;

    [SerializeField, BoxGroup ("Specific")]
    public float TrajectoryRadius = 1f;

    [SerializeField, BoxGroup ("Specific")]
    public float TrajectoryCurveHeight = 0.02f;

    [SerializeField, BoxGroup ("Specific")]
    public AnimationCurve TrajectoryCurve;
}