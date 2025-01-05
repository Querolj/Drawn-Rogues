using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "AttackCharge", menuName = "Attack/AttackCharge", order = 1)]
public class AttackCharge : Attack
{
    [SerializeField, BoxGroup ("Specific")]
    private bool _canGoThroughAttackables;
    public bool CanGoThroughAttackables => _canGoThroughAttackables;

    [SerializeField, BoxGroup ("Specific")]
    private float _chargeSpeed;
    public float ChargeSpeed => _chargeSpeed;

    [SerializeField, BoxGroup ("Specific")]
    private AttackableDetector _attackableDetectorTemplate;
    public AttackableDetector AttackableDetectorTemplate => _attackableDetectorTemplate;
}