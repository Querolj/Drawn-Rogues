using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "AttackProjectile", menuName = "Attack/AttackProjectile", order = 1)]
public class AttackProjectile : AttackSingleTarget
{
    [SerializeField, BoxGroup ("Specific")]
    public float TrajectoryCurveHeight = 0.02f;
    
    [SerializeField, BoxGroup ("Specific")]
    public float TrajectorySpeedFactor = 10f;

    [SerializeField, BoxGroup ("Specific")]
    public AnimationCurve TrajectorySpeedCurve;

    [SerializeField, BoxGroup ("Specific")]
    public Projectile ProjectileTemplate;
}