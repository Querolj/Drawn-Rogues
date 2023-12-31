using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField]
    private float _initialWalkingSpeed = 3f;
    private float _walkingSpeed = 3f;
    private float _walkingSpeedOffset = 0f;

    [SerializeField]
    private bool _initialDirectionIsRight = true;
    private const float _DIRECTION_CHANGE_SPEED = 1000f;

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
    private float _lerpValue = 1.1f;
    private Vector3 _lastPosition;
    public bool IsMoving
    {
        get
        {
            return _lerpValue < 1f;
        }
    }

    private bool _targetReached = false;
    private Vector3 _lastNormalHit;
    private Action _onMovementFinished;

    // Trajectory follow
    private bool _followTrajectory = false;

    private List<Vector3> _trajectory;
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

    private void FixedUpdate ()
    {
        UpdateYRotationInTime ();
        _lastPosition = transform.position;

        if (_characterAnimation.IsPlayingNonWalkingAnim)
            return;

        if (_followTrajectory)
        {
            if (_lerpTrajectory < 1f)
            {
                _lerpTrajectory += Time.fixedDeltaTime * _walkingSpeed;
                int index = (int) Mathf.Lerp (0, _trajectory.Count - 1, _lerpTrajectory);
                transform.position = _trajectory[index];
            }
            else
            {
                _followTrajectory = false;
                _lerpTrajectory = 0f;
                _onMovementFinished?.Invoke ();
            }
            return;
        }

        if (!ActivateWalk || _lerpValue >= 1f)
        {
            if (!_targetReached)
                _onMovementFinished?.Invoke ();
            _targetReached = true;
            return;
        }

        UpdateWalkingSpeedOffset ();

        // Vector3 newPos = Vector3.MoveTowards (transform.position, _positionTarget, Time.fixedDeltaTime * (_walkingSpeed + _walkingSpeedOffset));
        _lerpValue += Time.fixedDeltaTime * (_walkingSpeed);
        Vector3 newPos = Vector3.Lerp (_initialPosition, _positionTarget, _lerpValue);

        float mapHeight = Utils.GetMapHeight (transform.position);
        newPos.y = mapHeight;
        transform.position = newPos;

        if (!IsMoving && !_targetReached)
        {
            _targetReached = true;
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
            transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (-_eulerAngleTarget), Time.fixedDeltaTime * _DIRECTION_CHANGE_SPEED);
        }
    }

    private void UpdateWalkingSpeedOffset ()
    {
        float dist = Vector3.Distance (transform.position, _positionTarget);
        if (dist > 0.1f)
        {
            _walkingSpeedOffset = dist;
        }
        else
            _walkingSpeedOffset = 0f;
    }

    public void FollowTrajectory (List<Vector3> trajectory, Action onMovementFinished = null)
    {
        if (trajectory == null || trajectory.Count == 0)
            throw new Exception ("Trajectory is null or empty");

        _lerpTrajectory = 0f;
        _trajectory = trajectory;
        trajectory.Insert (0, transform.position);
        _positionTarget = trajectory[trajectory.Count - 1];
        _followTrajectory = true;
        _onMovementFinished = onMovementFinished;
    }

    public void MoveToTarget (Vector3 target, Action onMovementFinished = null)
    {
        _positionTarget = target;
        _onMovementFinished = onMovementFinished;
        _targetReached = false;
        ActivateWalk = true;
        _initialPosition = transform.position;
        _lerpValue = 0;

        float dist = Vector3.Distance (transform.position, _positionTarget);
        _walkingSpeed = _initialWalkingSpeed / dist;
    }

    public void StopMovement ()
    {
        _lerpValue = 1f;
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