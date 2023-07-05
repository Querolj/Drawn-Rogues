using UnityEngine;

[CreateAssetMenu (fileName = "AttackProjectile", menuName = "Attack/AttackProjectile", order = 1)]
public class AttackProjectile : AttackTrajectory
{
    public Projectile ProjectileTemplate;
}