using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

[RequireComponent (typeof (SpriteRenderer))]
public class AtkSelectCharge : AttackSelection
{
    private List<Attackable> _allTargets = new List<Attackable> ();
    private Attackable _mainTarget = null;
    private AttackInstCharge _attackCharge;
    private float _leftLimitX, _rightLimitX;
    private AttackableDetector _attackableDetector;

    public override void Activate (AttackInstance attack, Character player, CombatZone combatZone, Action onAttackEnded)
    {
        base.Activate (attack, player, combatZone, onAttackEnded);
        _attackCharge = _attack as AttackInstCharge ??
            throw new Exception (attack.Name + " is not a charge attack");

        _radiusAdded = Mathf.Max (_playerBounds.extents.x, _playerBounds.extents.y);
        foreach (Attackable attackable in _attackablesInZone)
        {
            attackable.OnMouseEntered += UpdateTarget;
            attackable.OnMouseExited += RemoveTarget;
        }

        Bounds playerBounds = player.GetSpriteBounds ();
        _attackableDetector = Instantiate (_attackCharge.AttackableDetectorTemplate, transform);
        _attackableDetector.RemoveAttackableOnExit = true;
        _attackableDetector.transform.localScale = new Vector3 (playerBounds.size.x, playerBounds.size.y, 1f);
        _attackableDetector.OnAttackableDetected += (attackable) =>
        {
            attackable.DisplayOutline (_validSelectionIcon.activeInHierarchy ? Color.green : Color.red);
        };
        _attackableDetector.OnAttackableOut += (attackable) =>
        {
            attackable.StopDisplayOutline ();
        };

        _spriteRenderer.enabled = true;
        _material.SetColor ("_Color", Color.clear);
        _moveIndicator.ActiveChargeAttackMode (playerBounds);

        // get combat zone limits 
        (_leftLimitX, _rightLimitX) = combatZone.GetMoveZoneLimitOnMap (attack.Range, player);
        _moveIndicator.gameObject.SetActive (true);
        Vector3 posMoveIndicator = playerBounds.center;
        posMoveIndicator.z = player.transform.position.z;
        _moveIndicator.SetPosition (posMoveIndicator);
        combatZone.DrawMoveLineOnMap (_leftLimitX, _rightLimitX, playerBounds);
    }

    private void IsAttackableInCharge (Attackable attackable)
    {
        if (_allTargets.Contains (attackable))
            return;

        _allTargets.Add (attackable);
    }

    private void UpdateTarget (Attackable attackable)
    {
        if (_validSelectionIcon.activeInHierarchy)
        {
            _mainTarget = attackable;
            attackable.DisplayOutline (Color.green);
        }
        else
        {
            attackable.DisplayOutline (Color.red);
            _mainTarget = null;
        }
    }

    private void RemoveTarget (Attackable attackable)
    {
        attackable.StopDisplayOutline ();
        _mainTarget = null;
    }

    public override void Deactivate ()
    {
        if (_attackablesInZone != null)
        {
            foreach (Attackable attackable in _attackablesInZone)
            {
                attackable.OnMouseEntered -= UpdateTarget;
                attackable.OnMouseExited -= RemoveTarget;
            }
        }

        _spriteRenderer.enabled = false;
        _validSelectionIcon.SetActive (false);
        _invalidSelectionIcon.SetActive (false);
        _moveIndicator.gameObject.SetActive (false);

        if (_mainTarget != null)
        {
            _mainTarget.StopDisplayOutline ();
            _mainTarget = null;
        }

        base.Deactivate ();
    }

    protected override void Update ()
    {
        base.Update ();

        if (!_spriteRenderer.enabled)
        {
            return;
        }

        TryRaycastOnAttackSelectionSprite (Mouse.current.position.ReadValue (), out Vector3 worldMousePos);
        worldMousePos.z = _attackerOriginPosition.z;
        TryTurnPlayer (worldMousePos);

        bool inRange = IsPositionInAttackRange (worldMousePos);
        UpdateIconDisplay (inRange);

        if (inRange)
        {
            _validSelectionIcon.transform.position = worldMousePos;
            if (_player.CharMovement.DirectionRight)
                _validSelectionIcon.transform.rotation = Quaternion.Euler (0, 0, 0);
            else
                _validSelectionIcon.transform.rotation = Quaternion.Euler (0, 0, 180);
        }
        else
        {
            _invalidSelectionIcon.transform.position = worldMousePos;
        }

        if (_mainTarget != null)
        {
            _mainTarget.DisplayOutline (Color.green);
            float xBoundsOffset = _mainTarget.GetSpriteBounds ().extents.x;
            xBoundsOffset += _playerBounds.extents.x;
            if (_player.CharMovement.DirectionRight)
            {
                _moveIndicator.SetPosition (_mainTarget.transform.position - new Vector3 (xBoundsOffset, 0, 0));
            }
            else
            {
                _moveIndicator.SetPosition (_mainTarget.transform.position + new Vector3 (xBoundsOffset, 0, 0));
            }
        }
        else
        {
            _moveIndicator.SetPosition (worldMousePos);
        }

        // _material.SetFloat ("_AttackRange", _attack.Range + _radiusAdded);
        // _material.SetVector ("_AttackPosition", _attackerOriginPosition);

        // if (Mouse.current.leftButton.wasPressedThisFrame && _targetAttackable != null && _validSelectionIcon.activeInHierarchy)
        // {
        //     _attackCharge.Execute (_player, _targetAttackable, _validSelectionIcon.transform.position, _onAttackEnded);
        //     Deactivate ();
        // }
    }
}