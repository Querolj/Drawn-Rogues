using UnityEngine;

[CreateAssetMenu (fileName = "AttackCharge", menuName = "Attack/AttackCharge", order = 1)]
public class AttackCharge : Attack
{
    [SerializeField]
    private bool _canGoThroughAttackables;
    public bool CanGoThroughAttackables => _canGoThroughAttackables;

    [SerializeField]
    private float _chargeSpeed;
    public float ChargeSpeed => _chargeSpeed;

    [SerializeField]
    private AttackableDetector _attackableDetectorTemplate;
    public AttackableDetector AttackableDetectorTemplate => _attackableDetectorTemplate;
}