using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof (SpriteRenderer))]
public class AtkSelectTrajectory : AttackSelection
{
    private const float _updateEverySeconds = 0.03f;
    private float _timeSinceLastUpdate = 0f;
    private AttackInstTrajectory _attackTrajectory;
    private List<Vector3> _trajectoryPoints = new List<Vector3> ();
    private Vector3 _trajectoryStart;
    private Attackable _target;

    public override void Activate (AttackInstance attack, Character player, CombatZone combatZone, Action onAttackEnded)
    {
        base.Activate (attack, player, combatZone, onAttackEnded);
        _attackTrajectory = attack as AttackInstTrajectory ??
            throw new Exception (attack.Name + " is not a trajectory attack");

        _trajectoryStart = _playerBounds.center;
        _trajectoryStart.z = transform.position.z;

        _radiusAdded = Mathf.Max (_playerBounds.extents.x, _playerBounds.extents.y);
        _spriteRenderer.enabled = true;
    }

    public override void Deactivate ()
    {
        base.Deactivate ();
        _target?.StopDisplayOutline ();
        _trajectoryDrawer.ClearTrajectory ();
        _target = null;
    }

    private Vector3 _lastMousePos = Vector3.zero;
    protected override void Update ()
    {
        if (_attack == null || !_spriteRenderer.enabled)
        {
            _target?.StopDisplayOutline ();
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && _target != null &&
            _trajectoryPoints?.Count > 1 && _validSelectionIcon.activeInHierarchy)
        {
            _attackTrajectory.Execute (_player, _target, _validSelectionIcon.transform.position, _onAttackEnded, null, _trajectoryPoints);
            Deactivate ();
        }

        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate < _updateEverySeconds)
        {
            return;
        }
        _timeSinceLastUpdate = 0f;

        if (Vector3.Distance (_lastMousePos, Mouse.current.position.ReadValue()) < 2)
        {
            return;
        }

        TryRaycastOnAttackSelectionSprite (Mouse.current.position.ReadValue(), out Vector3 targetPos);
        targetPos.y = Utils.GetMapHeight (targetPos);

        TryTurnPlayer (targetPos);

        bool isPositionInRange = IsPositionInAttackRange (targetPos);
        UpdateIconDisplay (isPositionInRange);

        if (isPositionInRange)
        {
            _validSelectionIcon.transform.position = targetPos;

            if (_trajectoryCalculator.TryGetSmashTrajectory (_trajectoryStart, targetPos,
                    _attackTrajectory.TrajectoryRadius, _player.GetInstanceID (), out Attackable attackable, out _trajectoryPoints))
            {
                if (TryGetAttackableUnderSword (out Attackable attackableUnderSword))
                {
                    if (_target != attackableUnderSword)
                    {
                        _target?.StopDisplayOutline ();
                        _target = attackableUnderSword;
                        _target.DisplayOutline (Color.green);
                    }
                }
                else
                {
                    _target = null;
                }

            }

            _validSelectionIcon.transform.position = _trajectoryPoints?.Count > 0 ? _trajectoryPoints[_trajectoryPoints.Count - 1] : targetPos;
            if (_trajectoryPoints?.Count > 1)
                _trajectoryDrawer.DrawTrajectory (_trajectoryPoints, _attackTrajectory.TrajectoryRadius);
        }
        else
        {
            _target?.StopDisplayOutline ();
            _target = null;
            _trajectoryDrawer.ClearTrajectory ();
            _invalidSelectionIcon.transform.position = targetPos;
        }

        _material.SetFloat ("_AttackRange", _attack.Range + _radiusAdded);
        _material.SetVector ("_AttackPosition", _attackerOriginPosition);

        _lastMousePos = Mouse.current.position.ReadValue();
    }

    private Dictionary<int, Attackable> _attackablesUnderSwordCache = new Dictionary<int, Attackable> ();
    private bool TryGetAttackableUnderSword (out Attackable attackable)
    {
        attackable = null;

        if (Physics.Raycast (_validSelectionIcon.transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Attackable")))
        {
            int id = hit.collider.gameObject.GetInstanceID ();
            if (!_attackablesUnderSwordCache.TryGetValue (id, out attackable))
            {
                attackable = hit.collider.GetComponentInParent<Attackable> ();
                if (attackable != null)
                {
                    _attackablesUnderSwordCache.Add (id, attackable);
                }
            }

            if (attackable.gameObject.tag == "Player") // also cache the player even if we don't want to return it
                return false;

            return true;
        }
        return false;
    }
}