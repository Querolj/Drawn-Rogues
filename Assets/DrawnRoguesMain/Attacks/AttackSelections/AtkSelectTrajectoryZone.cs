using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof (SpriteRenderer))]
public class AtkSelectTrajectoryZone : AttackSelection
{
    private const float _updateEverySeconds = 0.03f;
    private float _timeSinceLastUpdate = 0f;
    private AttackInstTrajectoryZone _attackTrajectoryZone;
    private List<Vector3> _trajectoryPoints = new List<Vector3> ();
    private AttackableDetector _attackableDetector;
    private Vector3 _trajectoryStart;
    private Attackable _mainTarget;

    public override void Activate (AttackInstance attack, Character player, CombatZone combatZone, Action onAttackEnded)
    {
        base.Activate (attack, player, combatZone, onAttackEnded);
        _attackTrajectoryZone = attack as AttackInstTrajectoryZone ??
            throw new Exception (attack.Name + " is not a trajectory zone attack");

        _trajectoryStart = _playerBounds.center;
        _trajectoryStart.z = transform.position.z;

        _radiusAdded = Mathf.Max (_playerBounds.extents.x, _playerBounds.extents.y);
        _spriteRenderer.enabled = true;
        _attackableDetector = Instantiate (_attackTrajectoryZone.AttackableDetectorTemplate, transform);
        _attackableDetector.RemoveAttackableOnExit = true;
        _attackableDetector.transform.localScale = new Vector3 (_attackTrajectoryZone.ZoneSize.x, _attackTrajectoryZone.ZoneSize.y, 1f);
        _attackableDetector.OnAttackableDetected += (attackable) =>
        {
            attackable.DisplayOutline (_validSelectionIcon.activeInHierarchy ? Color.green : Color.red);
        };
        _attackableDetector.OnAttackableOut += (attackable) =>
        {
            if (attackable != _mainTarget)
                attackable.StopDisplayOutline ();
        };
    }

    public override void Deactivate ()
    {
        base.Deactivate ();
        StopDisplayAttackablesContours ();
        _trajectoryDrawer.ClearTrajectory ();
        _mainTarget = null;
        if (_attackableDetector != null && _attackableDetector.gameObject != null)
            DestroyImmediate (_attackableDetector.gameObject);
    }

    private Vector3 _lastMousePos = Vector3.zero;
    protected override void Update ()
    {
        if (_attack == null || !_spriteRenderer.enabled)
        {
            StopDisplayAttackablesContours ();
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && (_attackableDetector.AttackablesInZone?.Count > 0 || _mainTarget != null) &&
            _trajectoryPoints?.Count > 1 && _validSelectionIcon.activeInHierarchy)
        {
            List<Attackable> targetsInZone = null;
            if (_attackableDetector.AttackablesInZone?.Count > 0)
                targetsInZone = new List<Attackable> (_attackableDetector.AttackablesInZone);
            _attackTrajectoryZone.Execute (_player, _mainTarget, _validSelectionIcon.transform.position, _onAttackEnded, targetsInZone, _trajectoryPoints);
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
        targetPos.y = Utils.GetMapHeight (_validSelectionIcon.transform.position);
        TryTurnPlayer (targetPos);

        bool isPositionInRange = IsPositionInAttackRange (targetPos);
        UpdateIconDisplay (isPositionInRange);

        if (isPositionInRange)
        {
            if (!_attackableDetector.gameObject.activeInHierarchy)
                _attackableDetector.gameObject.SetActive (true);

            if (_trajectoryCalculator.TryGetSmashTrajectory (_trajectoryStart, targetPos,
                    _attackTrajectoryZone.TrajectoryRadius, _player.GetInstanceID (), out Attackable attackable, out _trajectoryPoints))
            {
                if (TryGetAttackableUnderSword (out Attackable attackableUnderSword))
                {

                    if (_mainTarget != attackableUnderSword)
                    {
                        _mainTarget?.StopDisplayOutline ();
                        _mainTarget = attackableUnderSword;
                        _mainTarget.DisplayOutline (Color.green);
                    }
                }
                else
                {
                    if (_attackableDetector.AttackablesInZone?.Contains (_mainTarget) == false)
                        _mainTarget?.StopDisplayOutline ();
                    _mainTarget = null;
                }

            }

            _validSelectionIcon.transform.position = _trajectoryPoints?.Count > 0 ? _trajectoryPoints[_trajectoryPoints.Count - 1] : targetPos;
            Vector3 attackableDetectorPos = _validSelectionIcon.transform.position + _attackableDetector.Bounds.extents.y * 0.9f * Vector3.up;
            _attackableDetector.transform.position = attackableDetectorPos;

            if (_trajectoryPoints?.Count > 1)
                _trajectoryDrawer.DrawTrajectory (_trajectoryPoints, _attackTrajectoryZone.TrajectoryRadius);
        }
        else
        {
            if (_attackableDetector.gameObject.activeInHierarchy)
            {
                _mainTarget = null;
                _attackableDetector.gameObject.SetActive (false);
                StopDisplayAttackablesContours ();
                _trajectoryDrawer.ClearTrajectory ();
            }

            _invalidSelectionIcon.transform.position = targetPos;

        }

        _material.SetFloat ("_AttackRange", _attack.Range + _radiusAdded);
        _material.SetVector ("_AttackPosition", _attackerOriginPosition);

        _lastMousePos = Mouse.current.position.ReadValue();
    }

    private void StopDisplayAttackablesContours ()
    {
        if (_attackableDetector.AttackablesInZone?.Count > 0)
        {
            foreach (Attackable attackable in _attackableDetector.AttackablesInZone)
            {
                attackable.StopDisplayOutline ();
            }
        }

        _mainTarget?.StopDisplayOutline ();
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