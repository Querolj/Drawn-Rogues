using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField]
    private Character _character;

    private const float _stepHeight = 0.04f;
    private const float _stepSpeed = 10f;
    private const float _stepAngle = 12f;

    private float _currentLerp = 0f;
    private bool _sense = false; // false = left, true = right

    private bool _isPlayingNonWalkingAnime = false;
    public bool IsPlayingNonWalkingAnim
    {
        get { return _isPlayingNonWalkingAnime; }
    }

    #region Attack anime field
    private Vector3 _initialposition;
    private const float _meleeAttackDistance = 0.14f;
    private const float _meleeAttackSpeed = 20f;
    private const float _projAttackSpeed = 10f;

    private bool _attackStarted = false;
    private AttackType _attackAnimationType;
    private bool _backFromAttack = false;
    private Vector3 _attackTarget;
    private float _zAngleTarget;

    #endregion

    private enum WalkingAnimeStates
    {
        LeftDown,
        Up,
        RightDown
    }

    private WalkingAnimeStates _targetedAnimeState = WalkingAnimeStates.LeftDown;
    private Dictionary<WalkingAnimeStates, float> _heightsByWalkAnimeState = new Dictionary<WalkingAnimeStates, float> ()
    { { WalkingAnimeStates.LeftDown, 0f }, { WalkingAnimeStates.Up, _stepHeight }, { WalkingAnimeStates.RightDown, 0f }
    };

    private Dictionary<WalkingAnimeStates, float> _anglesByWalkAnimeState = new Dictionary<WalkingAnimeStates, float> ()
    { { WalkingAnimeStates.LeftDown, -_stepAngle }, { WalkingAnimeStates.Up, 0f }, { WalkingAnimeStates.RightDown, _stepAngle }
    };

    public void PlayAttackAnimation (AttackInstance attack)
    {
        if (attack == null)
            throw new System.ArgumentNullException (nameof (attack));

        if (attack.AttackType == AttackType.Melee)
            ReadyAttackAnimation ();
        else if (attack.AttackType == AttackType.Projectile)
            ReadyProjectileAnimation ();
    }

    private void ReadyAttackAnimation ()
    {
        _attackAnimationType = AttackType.Melee;
        _attackStarted = true;
        _attackTarget = transform.position + (_character.CharMovement.DirectionRight? Vector3.right : Vector3.left) * _meleeAttackDistance;
        _initialposition = transform.position;
        _currentLerp = 0f;
    }

    private const float PROJ_ANGLE_TARGET = 25f;
    private void ReadyProjectileAnimation ()
    {
        _attackAnimationType = AttackType.Projectile;
        _attackStarted = true;
        Vector3 rightDir = (Vector3.right + Vector3.up).normalized;
        Vector3 leftDir = (Vector3.left + Vector3.up).normalized;

        _attackTarget = transform.position + (_character.CharMovement.DirectionRight? rightDir : leftDir) * _meleeAttackDistance;
        _zAngleTarget = _character.CharMovement.DirectionRight? - PROJ_ANGLE_TARGET : PROJ_ANGLE_TARGET;

        _initialposition = transform.position;
        _currentLerp = 0f;
    }

    private void FixedUpdate ()
    {
        float slopeAngle = GetRotationAngleFromSlope ();
        _isPlayingNonWalkingAnime = true;

        if (_attackStarted)
        {
            switch (_attackAnimationType)
            {
                case AttackType.Melee:
                    MeleeAnimationUpdate ();
                    break;
                case AttackType.Projectile:
                    ProjectileAnimationUpdate (slopeAngle);
                    break;
            }
        }
        else if (_character.CharMovement.IsMoving)
            WalkingAnimationUpdate (slopeAngle);
        else
        {
            _isPlayingNonWalkingAnime = false;
            _currentLerp = 0f;
            _targetedAnimeState = WalkingAnimeStates.LeftDown;
            transform.localPosition = new Vector3 (0f, 0f, 0f);
            transform.localRotation = Quaternion.Euler (0f, 0f, slopeAngle);
        }
    }

    private void WalkingAnimationUpdate (float slopeAngle)
    {
        _currentLerp += Time.fixedDeltaTime * _stepSpeed;
        if (_currentLerp > 1f)
        {
            _currentLerp = 0f;
            _targetedAnimeState = SetNextWalkingAnimeState (_targetedAnimeState);
        }
        transform.localPosition = new Vector3 (0f, Mathf.Lerp (transform.localPosition.y, _heightsByWalkAnimeState[_targetedAnimeState], _currentLerp), 0f);
        transform.localRotation = Quaternion.Euler (0f, 0f, Mathf.LerpAngle (transform.localEulerAngles.z, _anglesByWalkAnimeState[_targetedAnimeState] + slopeAngle, _currentLerp));
        _isPlayingNonWalkingAnime = false;
    }

    private void MeleeAnimationUpdate ()
    {
        if (_backFromAttack)
        {
            _currentLerp += Time.fixedDeltaTime * (_meleeAttackSpeed * 0.33f);
            if (_currentLerp >= 1f)
            {
                _currentLerp = 0f;
                _backFromAttack = false;
                _attackStarted = false;
            }
            else
                transform.position = Vector3.Lerp (_attackTarget, _initialposition, _currentLerp);
        }
        else
        {
            _currentLerp += Time.fixedDeltaTime * _meleeAttackSpeed;
            if (_currentLerp >= 1f)
            {
                _currentLerp = 0f;
                _backFromAttack = true;
            }
            else
                transform.position = Vector3.Lerp (_initialposition, _attackTarget, _currentLerp);
        }
    }

    private void ProjectileAnimationUpdate (float slopeAngle)
    {
        if (_backFromAttack)
        {
            _currentLerp += Time.fixedDeltaTime * (_projAttackSpeed * 0.33f);
            if (_currentLerp >= 1f)
            {
                _currentLerp = 0f;
                _backFromAttack = false;
                _attackStarted = false;
            }
            else
            {
                transform.position = Vector3.Lerp (_attackTarget, _initialposition, _currentLerp);
                transform.localRotation = Quaternion.Euler (0f, 0f, Mathf.LerpAngle (transform.localEulerAngles.z, slopeAngle, _currentLerp));
            }
        }
        else
        {
            _currentLerp += Time.fixedDeltaTime * _projAttackSpeed;
            if (_currentLerp >= 1f)
            {
                _currentLerp = 0f;
                _backFromAttack = true;
            }
            else
            {
                transform.position = Vector3.Lerp (_initialposition, _attackTarget, _currentLerp);
                transform.localRotation = Quaternion.Euler (0f, 0f, Mathf.LerpAngle (transform.localEulerAngles.z, _zAngleTarget + slopeAngle, _currentLerp));
            }
        }
    }

    private float GetRotationAngleFromSlope ()
    {
        if (Physics.Raycast (transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            Vector3 normal = hit.normal;
            Quaternion q = Quaternion.FromToRotation (Vector3.up, normal);

            q = Quaternion.Euler (0f, 0f, q.eulerAngles.z * (_character.CharMovement.DirectionRight ? 1f : -1f));

            return q.eulerAngles.z;
        }

        throw new System.Exception ("GetRotationAngleFromSlope : No map found");
    }

    private WalkingAnimeStates SetNextWalkingAnimeState (WalkingAnimeStates current)
    {
        if ((current == WalkingAnimeStates.LeftDown && !_sense) || (current == WalkingAnimeStates.RightDown && _sense))
        {
            _sense = !_sense;
        }

        if (current == WalkingAnimeStates.LeftDown || current == WalkingAnimeStates.RightDown)
            return WalkingAnimeStates.Up;

        return _sense? WalkingAnimeStates.RightDown : WalkingAnimeStates.LeftDown;
    }
}