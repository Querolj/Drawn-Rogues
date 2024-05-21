using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class AtkSelectCone : AttackSelection
{
    [SerializeField]
    private GameObject _coneObject;

    [SerializeField]
    private SpriteRenderer _coneSpriteRenderer;

    [SerializeField]
    private AttackableDetector _attackableDetector;

    [SerializeField]
    private int _coneCollidePrecision = 10;

    private Vector3 _coneStartPosition;
    private AttackInstCone _attackCone;
    private Vector3 _initialConeScale = Vector3.zero;

    public override void Activate (AttackInstance attack, Character player, CombatZone combatZone, Action onAttackEnded)
    {
        base.Activate (attack, player, combatZone, onAttackEnded);

        _attackCone = _attack as AttackInstCone;
        if (_attackCone == null)
            throw new Exception (attack.Name + " is not a cone attack " + _attack.GetType ().Name);

        if (_initialConeScale == Vector3.zero)
            _initialConeScale = _coneObject.transform.localScale;

        _coneObject.transform.localScale = _initialConeScale * attack.Range;

        _radiusAdded = Mathf.Max (_playerBounds.extents.x, _playerBounds.extents.y);
        _spriteRenderer.enabled = true;
        _invalidSelectionIcon.SetActive (false);
        _validSelectionIcon.SetActive (false);
        _material.SetColor ("_Color", Color.clear);
        _coneSpriteRenderer.material.SetFloat ("_Angle", _attackCone.ConeAngle);
        UpdateConeStartPos ();

        _attackableDetector.RemoveAttackableOnExit = true;
        _attackableDetector.OnAttackableDetected += (attackable) =>
        {
            attackable.DisplayOutline (Color.red);
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

    private void UpdateConeStartPos ()
    {
        _coneStartPosition = _attackerOriginPosition;
        if (_player.CharMovement.DirectionRight)
            _coneStartPosition.x += _playerBounds.extents.x;
        else
        {
            _coneStartPosition.x -= _playerBounds.extents.x;
        }
        _coneStartPosition.y += _playerBounds.extents.y / 2f;

        _coneObject.transform.position = _coneStartPosition;
    }

    protected override void Update ()
    {
        base.Update ();

        if (!_spriteRenderer.enabled)
        {
            return;
        }

        UpdateConeStartPos ();
        TryRaycastOnAttackSelectionSprite (Mouse.current.position.ReadValue (), out Vector3 worldMousePos);
        worldMousePos.z = _attackerOriginPosition.z;
        TryTurnPlayer (worldMousePos);

        _coneObject.transform.right = worldMousePos - _coneStartPosition;

        if (_attackableDetector.AttackablesInZone?.Count == 0)
            return;

        List<Attackable> attackablesInCone = GetAttackablesInCone (_attackableDetector.AttackablesInZone);
        DisplayOutline (_attackableDetector.AttackablesInZone, attackablesInCone);

        if (attackablesInCone.Count > 0 && Mouse.current.leftButton.wasPressedThisFrame)
        {
            List<Attackable> targetsInZone = new List<Attackable> (attackablesInCone);
            _attackCone.SetConeObjectAngle (_coneObject.transform.rotation.eulerAngles.z);
            _attackCone.Execute (_player, null, _coneStartPosition, _onAttackEnded, targetsInZone);
            Deactivate ();
        }
    }

    private List<Attackable> GetAttackablesInCone (List<Attackable> attackablesInRadius)
    {
        List<Attackable> attackablesInCone = new List<Attackable> ();
        Vector3 initialDirection = Quaternion.Euler (0, 0, -_attackCone.ConeAngle / 2f) * _coneObject.transform.right;
        for (int i = 0; i < _coneCollidePrecision; i++)
        {
            float angle = i * _attackCone.ConeAngle / _coneCollidePrecision;
            Vector3 direction = Quaternion.Euler (0, 0, angle) * initialDirection;
            RaycastHit[] raycastHits = Physics.RaycastAll (_coneStartPosition, direction, _attackCone.Range);
            foreach (RaycastHit hit in raycastHits)
            {
                IEnumerable<Attackable> matches = attackablesInRadius.Where (attackable => attackable.gameObject == hit.collider.gameObject);
                foreach (Attackable attackable in matches)
                {
                    if (!attackablesInCone.Contains (attackable))
                    {
                        attackablesInCone.Add (attackable);
                    }
                }
            }

            if (attackablesInRadius.Count == attackablesInCone.Count)
                return attackablesInCone;
        }

        return attackablesInCone;
    }

    private void DisplayOutline (List<Attackable> attackablesInRadius, List<Attackable> attackablesInCone)
    {
        foreach (Attackable attackable in attackablesInRadius)
        {
            if (attackablesInCone.Contains (attackable))
            {
                attackable.DisplayOutline (Color.green);
            }
            else
            {
                attackable.DisplayOutline (Color.red);
            }
        }
    }
}