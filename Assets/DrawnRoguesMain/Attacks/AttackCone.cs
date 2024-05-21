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

    [SerializeField]
    private float _breathDuration = 1.5f;
    public float BreathDuration => _breathDuration;

    [SerializeField]
    private float _particuleNumberPerDegree = 1f;
    public float ParticuleNumberPerDegree => _particuleNumberPerDegree;
}