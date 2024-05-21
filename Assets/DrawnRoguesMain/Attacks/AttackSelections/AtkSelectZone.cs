using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AtkSelectZone : AttackSelection
{
    private AttackInstZone _attackZone;
    private AttackableDetector _attackableDetector;

    public override void Activate (AttackInstance attack, Character player, CombatZone combatZone, Action onAttackEnded)
    {
        base.Activate (attack, player, combatZone, onAttackEnded);
        _attackZone = attack as AttackInstZone ??
            throw new Exception (attack.Name + " is not a projectile attack");

        _radiusAdded = Mathf.Max (_playerBounds.extents.x, _playerBounds.extents.y);
        _spriteRenderer.enabled = true;
        _attackableDetector = Instantiate (_attackZone.AttackableDetectorTemplate, transform);
        _attackableDetector.RemoveAttackableOnExit = true;
        _attackableDetector.transform.localScale = new Vector3 (_attackZone.ZoneSize.x, _attackZone.ZoneSize.y, 1f);
        _attackableDetector.OnAttackableDetected += (attackable) =>
        {
            attackable.DisplayOutline (_validSelectionIcon.activeInHierarchy ? Color.green : Color.red);
        };
        _attackableDetector.OnAttackableOut += (attackable) =>
        {
            attackable.StopDisplayOutline ();
        };
    }

    public override void Deactivate ()
    {
        base.Deactivate ();
        StopDisplayAttackablesOutlines ();
        _trajectoryDrawer.ClearTrajectory ();
        if (_attackableDetector != null && _attackableDetector.gameObject != null)
            DestroyImmediate (_attackableDetector.gameObject);
    }

    protected override void Update ()
    {
        if (_attack == null || !_spriteRenderer.enabled)
        {
            StopDisplayAttackablesOutlines ();
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && _attackableDetector.AttackablesInZone?.Count > 0 && _validSelectionIcon.activeInHierarchy)
        {
            List<Attackable> targetsInZone = null;
            if (_attackableDetector.AttackablesInZone?.Count > 0)
                targetsInZone = new List<Attackable> (_attackableDetector.AttackablesInZone);
            _attackZone.Execute (_player, null, _validSelectionIcon.transform.position, _onAttackEnded, targetsInZone);
            Deactivate ();
        }

        if (Vector3.Distance (_lastMousePos, Mouse.current.position.ReadValue()) < 2)
        {
            return;
        }

        TryRaycastOnAttackSelectionSprite (Mouse.current.position.ReadValue(), out Vector3 targetPos);
        TryTurnPlayer (targetPos);

        bool isPositionInRange = IsPositionInAttackRange (targetPos);
        UpdateIconDisplay (isPositionInRange);

        if (isPositionInRange)
        {
            if (!_attackableDetector.gameObject.activeInHierarchy)
                _attackableDetector.gameObject.SetActive (true);

            _validSelectionIcon.transform.position = targetPos;
            Vector3 attackableDetectorPos = _validSelectionIcon.transform.position + _attackableDetector.Bounds.extents.y * 0.9f * Vector3.up;
            _attackableDetector.transform.position = attackableDetectorPos;
        }
        else
        {
            if (_attackableDetector.gameObject.activeInHierarchy)
            {
                _attackableDetector.gameObject.SetActive (false);
                StopDisplayAttackablesOutlines ();
                _trajectoryDrawer.ClearTrajectory ();
            }

            _invalidSelectionIcon.transform.position = targetPos;

        }

        _material.SetFloat ("_AttackRange", _attack.Range + _radiusAdded);
        _material.SetVector ("_AttackPosition", _attackerOriginPosition);

        _lastMousePos = Mouse.current.position.ReadValue();
    }

    private void StopDisplayAttackablesOutlines ()
    {
        if (_attackableDetector.AttackablesInZone?.Count > 0)
        {
            foreach (Attackable attackable in _attackableDetector.AttackablesInZone)
            {
                attackable.StopDisplayOutline ();
            }
        }
    }
}