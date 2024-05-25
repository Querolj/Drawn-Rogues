using UnityEngine;

[CreateAssetMenu (fileName = "AttackCone", menuName = "Attack/AttackCone", order = 1)]
public class AttackCone : Attack
{
    [SerializeField]
    private ParticleSystem _breathParticleSystem;
    public ParticleSystem BreathParticleSystem => _breathParticleSystem;

    [SerializeField]
    private float _coneAngle;
    public float ConeAngle => _coneAngle;
}