using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (SpriteRenderer))]
public class AtkSelectJump : AttackSelection
{
    [SerializeField]
    private AttackableDetector _attackableDetectorTemplate;

    private const float _updateEverySeconds = 0.03f;
    private float _timeSinceLastUpdate = 0f;
    private AttackInstJump _attackJump;
    private List<Vector3> _trajectoryPoints = new List<Vector3> ();
    private Vector3 _trajectoryStart;
    private GameObject _shadowGo;
    private float _radius;
    private Attackable _attackableFocused;
    private AttackableDetector _attackableDetector = null;

    public override void Activate (AttackInstance attack, Character player, CombatZone combatZone, Action onAttackEnded)
    {
        base.Activate (attack, player, combatZone, onAttackEnded);
        _attackJump = attack as AttackInstJump ??
            throw new Exception (attack.Name + " is not a projectile attack");

        _trajectoryStart = _playerBounds.center;
        _trajectoryStart.z = transform.position.z;

        _radiusAdded = Mathf.Max (_playerBounds.extents.x, _playerBounds.extents.y);
        _spriteRenderer.enabled = true;

        _radius = _playerBounds.size.x > _playerBounds.size.y ? _playerBounds.size.x : _playerBounds.size.y;

        // Create shadow
        _shadowGo = new GameObject ("Shadow");
        SpriteRenderer shadowSpriteRenderer = _shadowGo.AddComponent<SpriteRenderer> ();
        shadowSpriteRenderer.sprite = (player as DrawedCharacter).GetShadowSprite ();

        if (_attackableDetector == null)
        {
            _attackableDetector = Instantiate (_attackableDetectorTemplate, transform);
            _attackableDetector.RemoveAttackableOnExit = true;
            _attackableDetector.GetComponent<SpriteRenderer> ().enabled = false;
        }

        _attackableDetector.transform.localScale = new Vector3 (_playerBounds.size.x / 10f, _playerBounds.size.y / 10f, 1f);
    }

    public override void Deactivate ()
    {
        base.Deactivate ();
        GameObject.DestroyImmediate (_shadowGo);
        _trajectoryDrawer.ClearTrajectory ();
    }

    private Vector3 _lastMousePos = Vector3.zero;
    protected override void Update ()
    {
        if (_attack == null || !_spriteRenderer.enabled)
        {
            return;
        }

        if (Input.GetMouseButtonDown (0) && _attackableFocused == null && _trajectoryPoints?.Count > 1 && _attackableDetector.AttackablesInZone?.Count == 0 && !Utils.IsPointerOverUIElement ())
        {
            _attackJump.Execute (_player, null, Vector3.zero, _onAttackEnded, null, _trajectoryPoints);
            Deactivate ();
            return;
        }

        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate < _updateEverySeconds)
        {
            return;
        }
        _timeSinceLastUpdate = 0f;

        if (Vector3.Distance (_lastMousePos, Input.mousePosition) < 2)
        {
            return;
        }

        TryRaycastOnAttackSelectionSprite (Input.mousePosition, out Vector3 targetPos);
        targetPos.y = Utils.GetMapHeight (targetPos);
        Debug.Log ("targetPos 2 " + targetPos.ToString ("F3"));

        TryTurnPlayer (targetPos);

        Vector3 attackableDetectorPos = targetPos + Vector3.up * _playerBounds.extents.y;
        _attackableDetector.transform.position = attackableDetectorPos;

        bool isPositionInRange = IsPositionInAttackRange (targetPos);

        if (isPositionInRange && _attackableDetector.AttackablesInZone?.Count == 0)
        {
            _shadowGo.transform.position = _trajectoryPoints?.Count > 0 ? _trajectoryPoints[_trajectoryPoints.Count - 1] : targetPos;

            _trajectoryPoints = _trajectoryCalculator.GetCurvedTrajectory (_trajectoryStart, targetPos,
                _radius, _player.gameObject.GetInstanceID ());

            UpdateIconDisplay (true);

            if (_trajectoryPoints?.Count > 1)
                _trajectoryDrawer.DrawTrajectory (_trajectoryPoints, _radius);
        }
        else
        {
            UpdateIconDisplay (false);
            _trajectoryDrawer.ClearTrajectory ();
            _invalidSelectionIcon.transform.position = attackableDetectorPos;
        }

        _material.SetFloat ("_AttackRange", _attack.Range + _radiusAdded);
        _material.SetVector ("_AttackPosition", _attackerOriginPosition);

        _lastMousePos = Input.mousePosition;
    }
}