using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu (fileName = "AttackCone", menuName = "Attack/AttackCone", order = 1)]
public class AttackCone : Attack
{
    [SerializeField, BoxGroup ("Display")]
    private ParticleSystem _breathParticleSystem;
    public ParticleSystem BreathParticleSystem => _breathParticleSystem;

    [SerializeField, BoxGroup ("Specific"), Range (0f, 180f)]
    private float _coneAngle;
    public float ConeAngle => _coneAngle;
}