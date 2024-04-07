using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof (SpriteRenderer))]
public class AtkSelectProjectile : AttackSelection
{
    private Vector3 _projectileStartPosition;

    private const float _updateEverySeconds = 0.03f;
    private float _timeSinceLastUpdate = 0f;
    private Attackable _targettedAttackable = null;
    private AttackInstProjectile _attackProjectile;
    private List<Vector3> _trajectoryPoints = new List<Vector3> ();

    public override void Activate (AttackInstance attack, Character player, CombatZone combatZone, Action onAttackEnded)
    {
        base.Activate (attack, player, combatZone, onAttackEnded);

        _attackProjectile = _attack as AttackInstProjectile;
        if (_attackProjectile == null)
            throw new Exception (attack.Name + " is not a projectile attack " + _attack.GetType ().Name);

        _radiusAdded = Mathf.Max (_playerBounds.extents.x, _playerBounds.extents.y);
        _spriteRenderer.enabled = true;

        UpdateProjectileStartPos ();
    }

    private void UpdateProjectileStartPos ()
    {
        _projectileStartPosition = _attackerOriginPosition;
        if (_player.CharMovement.DirectionRight)
            _projectileStartPosition += _playerBounds.extents;
        else
        {
            _projectileStartPosition.x -= _playerBounds.extents.x;
            _projectileStartPosition.y += _playerBounds.extents.y;
        }
    }

    public override void Deactivate ()
    {
        base.Deactivate ();
        _trajectoryDrawer.ClearTrajectory ();
        if (_targettedAttackable != null)
            _targettedAttackable.StopDisplayOutline ();
    }

    private Vector3 _lastMousePos = Vector3.zero;
    protected override void Update ()
    {
        if (_attack == null || !_spriteRenderer.enabled)
        {
            if (_targettedAttackable != null)
            {
                _targettedAttackable.StopDisplayOutline ();
                _targettedAttackable = null;
            }

            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && _targettedAttackable != null && _trajectoryPoints?.Count > 1)
        {
            _attackProjectile.Execute (_player, _targettedAttackable, _validSelectionIcon.transform.position, _onAttackEnded, null, _trajectoryPoints);
            Deactivate ();
        }

        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate < _updateEverySeconds)
        {
            return;
        }
        _timeSinceLastUpdate = 0f;

        if (Vector3.Distance (_lastMousePos, Mouse.current.position.ReadValue()) < 1)
        {
            return;
        }

        TryRaycastOnAttackSelectionSprite (Mouse.current.position.ReadValue(), out Vector3 targetPos);
        targetPos.y = Utils.GetMapHeight (targetPos);
        UpdateProjectileStartPos ();
        TryTurnPlayer (targetPos);

        bool isPositionInRange = IsPositionInAttackRange (targetPos);
        UpdateIconDisplay (isPositionInRange);
        if (isPositionInRange)
        {
            _trajectoryPoints = _trajectoryCalculator.GetCurvedTrajectory (_projectileStartPosition, targetPos,
                _attackProjectile.TrajectoryRadius, _player.gameObject.GetInstanceID (), out Attackable attackable);

            if (attackable != null && _trajectoryPoints?.Count > 1)
            {
                if (_targettedAttackable != null && _targettedAttackable != attackable)
                {
                    _targettedAttackable.StopDisplayOutline ();
                }

                _targettedAttackable = attackable;
                _targettedAttackable.DisplayOutline (Color.green);

                _validSelectionIcon.transform.position = _trajectoryPoints[_trajectoryPoints.Count - 1];
            }
            else
            {
                if (_targettedAttackable != null)
                {
                    _targettedAttackable.StopDisplayOutline ();
                    _targettedAttackable = null;
                }

                _validSelectionIcon.transform.position = targetPos;
            }

            if (_trajectoryPoints?.Count > 1)
                _trajectoryDrawer.DrawTrajectory (_trajectoryPoints, _attackProjectile.TrajectoryRadius);
        }
        else
        {
            if (_targettedAttackable != null)
            {
                _targettedAttackable.StopDisplayOutline ();
                _targettedAttackable = null;
            }

            _trajectoryDrawer.ClearTrajectory ();
            _invalidSelectionIcon.transform.position = targetPos;
        }

        _material.SetFloat ("_AttackRange", _attack.Range + _radiusAdded);
        _material.SetVector ("_AttackPosition", _attackerOriginPosition);

        _lastMousePos = Mouse.current.position.ReadValue();
    }
}