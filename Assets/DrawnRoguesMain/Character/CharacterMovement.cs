using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField]
    private float _walkingSpeed = 0.4f;
    private float _walkingSpeedMultiplier = 1f;

    [SerializeField]
    private bool _initialDirectionIsRight = true;
    private const float _ROTATION_SPEED = 1000f;

    [SerializeField]
    private CharacterAnimation _characterAnimation;

    public bool DirectionRight
    {
        get
        {
            return transform.right.x >= 0f;
        }
    }

    private Vector3 _eulerAngleTarget = Vector3.zero;
    private Vector3 _eulerAngleRight = Vector3.zero;
    private Vector3 _eulerAngleLeft = new Vector3 (0f, 180f, 0f);

    private Vector3 _positionTarget;
    private Vector3 _initialPosition;
    private float _currentLerpValue = 1.1f;
    private float _valueToAddToLerp = 0f;
    private Vector3 _lastPosition;
    private float _offsetY = 0f;
    public bool IsMoving
    {
        get
        {
            return _currentLerpValue < 1f;
        }
    }

    private bool _targetReached = false;
    private Action _onMovementFinished;
    private bool _isJumping = false;
    private List<Vector3> _jumpTrajectory;
    private float _lerpTrajectory = 0f;
    public bool ActivateWalk = false;

    protected virtual void Awake ()
    {
        _positionTarget = transform.position;

        if (_initialDirectionIsRight)
            _eulerAngleTarget = _eulerAngleRight;
        else
            _eulerAngleTarget = _eulerAngleLeft;
    }

    public void TurnTowardTarget (Vector3 target)
    {
        if (target.x > transform.position.x)
        {
            transform.position = transform.position + Vector3.right * 0.0001f;
        }
        else if (target.x < transform.position.x)
        {
            transform.position = transform.position + Vector3.left * 0.0001f;
        }
    }

    private void Update ()
    {
        UpdateYRotationInTime ();
        _lastPosition = transform.position;

        if (_characterAnimation.IsPlayingNonWalkingAnim)
            return;

        if (_isJumping)
        {
            UpdateJump ();
            return;
        }

        if (!ActivateWalk || _currentLerpValue >= 1f)
        {
            if (!_targetReached)
                _onMovementFinished?.Invoke ();
            _targetReached = true;
            return;
        }

        _currentLerpValue += _valueToAddToLerp;
        Vector3 newPos = Vector3.Lerp (_initialPosition, _positionTarget, _currentLerpValue);

        float mapHeight = Utils.GetMapHeight (transform.position);
        newPos.y = mapHeight + _offsetY;
        transform.position = newPos;

        if (!IsMoving && !_targetReached)
        {
            _targetReached = true;
            _onMovementFinished?.Invoke ();
        }
    }

    public void Move (Vector2 direction)
    {
        Vector3 newPos = transform.position + new Vector3 (direction.x, 0f, direction.y) * Time.deltaTime * _walkingSpeed * _walkingSpeedMultiplier;
        float mapHeight = Utils.GetMapHeight (newPos);
        newPos.y = mapHeight + _offsetY;
        transform.position = newPos;
    }

    public void SetOffsetY (float offsetY)
    {
        _offsetY = offsetY;
    }

    public void SetWalkingSpeedMultiplier (float multiplier)
    {
        _walkingSpeedMultiplier = multiplier;
    }

    private void UpdateJump ()
    {
        if (_lerpTrajectory < 1f)
        {
            _lerpTrajectory += Time.deltaTime * _walkingSpeed;
            int index = (int) Mathf.Lerp (0, _jumpTrajectory.Count - 1, _lerpTrajectory);
            transform.position = _jumpTrajectory[index];
        }
        else
        {
            _isJumping = false;
            _lerpTrajectory = 0f;
            _onMovementFinished?.Invoke ();
        }
    }

    private void UpdateYRotationInTime ()
    {
        if (_lastPosition.x < transform.position.x)
        {
            _eulerAngleTarget = _eulerAngleRight;
        }
        else if (_lastPosition.x > transform.position.x)
        {
            _eulerAngleTarget = _eulerAngleLeft;
        }

        if (transform.rotation.eulerAngles != _eulerAngleTarget)
        {
            transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (-_eulerAngleTarget), Time.fixedDeltaTime * _ROTATION_SPEED);
        }
    }

    public void Jump (List<Vector3> trajectory, Action onMovementFinished = null)
    {
        if (trajectory == null || trajectory.Count == 0)
            throw new Exception ("Trajectory is null or empty");

        _lerpTrajectory = 0f;
        _jumpTrajectory = trajectory;
        trajectory.Insert (0, transform.position);
        _positionTarget = trajectory[trajectory.Count - 1];
        _isJumping = true;
        _onMovementFinished = onMovementFinished;
    }

    public void MoveToTarget (Vector3 target, Action onMovementFinished = null)
    {
        _positionTarget = target;
        _onMovementFinished = onMovementFinished;
        _targetReached = false;
        ActivateWalk = true;
        _initialPosition = transform.position;
        _currentLerpValue = 0;

        float dist = Vector3.Distance (_initialPosition, _positionTarget);
        _valueToAddToLerp = _walkingSpeed * Time.deltaTime / dist;
    }

    public void StopMovement ()
    {
        _currentLerpValue = 1f;
    }

    public void MoveNextToCharacter (Bounds targetCharBounds, Bounds charBounds, float margin, Action onMovementFinished = null)
    {
        Vector3 target = targetCharBounds.center;
        float xOffset = targetCharBounds.extents.x + charBounds.extents.x + margin;
        if (transform.position.x > target.x)
        {
            target.x += xOffset;

        }
        else
        {
            target.x -= xOffset;
        }

        target.z = transform.position.z;
        MoveToTarget (target, onMovementFinished);
    }
}