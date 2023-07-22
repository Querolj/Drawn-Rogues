using System;
using UnityEngine;

[RequireComponent (typeof (SpriteRenderer))]
public class AtkSelectSingleTarget : AttackSelection
{
    private Attackable _focusedAttackable = null;
    private Attackable _targetAttackable = null;
    private AttackInstSingleTarget _attackSingleTarget;
    public override void Activate (AttackInstance attack, Character player, CombatZone combatZone, Action onAttackEnded)
    {
        base.Activate (attack, player, combatZone, onAttackEnded);
        _attackSingleTarget = _attack as AttackInstSingleTarget ??
            throw new Exception (attack.Name + " is not a single target attack");

        _radiusAdded = Mathf.Max (_playerBounds.extents.x, _playerBounds.extents.y);
        foreach (Attackable attackable in _attackablesInZone)
        {
            attackable.OnMouseEntered += UpdateTarget;
            attackable.OnMouseExited += RemoveTarget;
        }

        _spriteRenderer.enabled = true;
    }

    private void UpdateTarget (Attackable attackable)
    {
        if (_validSelectionIcon.activeInHierarchy)
        {
            _targetAttackable = attackable;
            attackable.DisplayOutline (Color.green);
        }
        else
        {
            attackable.DisplayOutline (Color.red);
            _targetAttackable = null;
        }
    }

    private void RemoveTarget (Attackable attackable)
    {
        attackable.StopDisplayOutline ();
        _targetAttackable = null;
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

        _focusedAttackable = null;
        _spriteRenderer.enabled = false;
        _validSelectionIcon.SetActive (false);
        _invalidSelectionIcon.SetActive (false);

        if (_targetAttackable != null)
        {
            _targetAttackable.StopDisplayOutline ();
            _targetAttackable = null;
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

        TryRaycastOnAttackSelectionSprite (Input.mousePosition, out Vector3 worldMousePos);
        worldMousePos.z = _attackerOriginPosition.z;
        TryTurnPlayer (worldMousePos);

        bool inRange = IsPositionInAttackRange (worldMousePos);
        UpdateIconDisplay (inRange);

        if (inRange)
        {
            _validSelectionIcon.transform.position = worldMousePos;
            _validSelectionIcon.transform.rotation = Quaternion.Euler (0, 0, Mathf.Atan2 (worldMousePos.y - _attackerOriginPosition.y, worldMousePos.x - _attackerOriginPosition.x) * Mathf.Rad2Deg);
        }
        else
        {
            _invalidSelectionIcon.transform.position = worldMousePos;
        }

        _material.SetFloat ("_AttackRange", _attack.Range + _radiusAdded);
        _material.SetVector ("_AttackPosition", _attackerOriginPosition);

        if (Input.GetMouseButtonDown (0) && _targetAttackable != null && _validSelectionIcon.activeInHierarchy)
        {
            _attackSingleTarget.Execute (_player, _targetAttackable, _validSelectionIcon.transform.position, _onAttackEnded);
            Deactivate ();
        }
    }
}