using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public enum AttackSelectionType
{
    SingleTarget,
    Projectile,
    Jump,
    TrajectoryZone,
    Zone,
    Self,
    Trajectory,
    Cone
}

public class AttackSelection : MonoBehaviour
{
    public class Factory : PlaceholderFactory<AttackSelection, AttackSelection> { }

    [SerializeField]
    private AttackSelectionType _attackSelectionType;
    public AttackSelectionType AttackSelectionType
    {
        get
        {
            return _attackSelectionType;
        }
    }

    [SerializeField]
    protected GameObject _validSelectionIcon;

    [SerializeField]
    protected GameObject _invalidSelectionIcon;

    protected Material _material;
    protected SpriteRenderer _spriteRenderer;
    protected Camera _mainCamera;
    protected float _radiusAdded;

    protected AttackInstance _attack;
    protected Action _onAttackEnded;
    protected Vector3 _attackerOriginPosition;
    protected Character _player;
    protected Bounds _playerBounds;
    protected List<Attackable> _attackablesInZone = new List<Attackable> ();
    protected TrajectoryCalculator _trajectoryCalculator;
    protected TrajectoryDrawer _trajectoryDrawer;
    protected CombatZone _combatZone;
    protected Vector2 _lastMousePos = Vector3.zero;

    private void Awake ()
    {
        _spriteRenderer = GetComponent<SpriteRenderer> ();
        _mainCamera = Camera.main;
        _material = _spriteRenderer.material;
    }

    [Inject, UsedImplicitly]
    public void Init (TrajectoryCalculator trajectoryCalculator, TrajectoryDrawer trajectoryDrawer)
    {
        _trajectoryCalculator = trajectoryCalculator;
        _trajectoryDrawer = trajectoryDrawer;
    }

    public virtual void Activate (AttackInstance attack, Character player, CombatZone combatZone, Action onAttackEnded)
    {
        if (combatZone == null)
            throw new ArgumentNullException (nameof (combatZone));
        _attack = attack ??
            throw new ArgumentNullException (nameof (attack));
        _onAttackEnded = onAttackEnded ??
            throw new ArgumentNullException (nameof (onAttackEnded));
        _player = player ??
            throw new ArgumentNullException (nameof (player));
        _combatZone = combatZone ??
            throw new ArgumentNullException (nameof (combatZone));

        _playerBounds = (Bounds) player.GetSpriteBounds ();
        _attackerOriginPosition = _playerBounds.center;
        _attackerOriginPosition.z = player.transform.position.z;
        _attackablesInZone = combatZone.AttackablesInZone;

        transform.position = new Vector3 (transform.position.x, transform.position.y, _attackerOriginPosition.z);

        _material.SetFloat ("_BoundXMin", combatZone.LineXMin);
        _material.SetFloat ("_BoundXMax", combatZone.LineXMax);
    }

    protected void TryTurnPlayer (Vector3 targetPosition)
    {
        if ((_player.CharMovement.DirectionRight && targetPosition.x < _player.transform.position.x) ||
            (!_player.CharMovement.DirectionRight && targetPosition.x > _player.transform.position.x))
            _player.CharMovement.TurnTowardTarget (targetPosition);
    }

    public virtual void Deactivate ()
    {
        _attack = null;
        gameObject.SetActive (false);
        _attackablesInZone = null;
    }

    protected virtual void Update ()
    {
        if (_lastMousePos == Mouse.current.position.ReadValue ())
        {
            return;
        }

        if (_attack == null)
        {
            return;
        }

        _lastMousePos = Mouse.current.position.ReadValue ();
    }

    private Dictionary<int, Attackable> _attackableCached = new Dictionary<int, Attackable> ();

    protected bool TryRaycastOnAttackSelectionSprite (Vector3 screenPos, out Vector3 hitPos)
    {
        hitPos = Vector3.zero;
        // Remap so (0, 0) is the center of the window,
        // and the edges are at -0.5 and +0.5.
        Vector2 relative = new Vector2 (
            screenPos.x / Screen.width - 0.5f,
            screenPos.y / Screen.height - 0.5f
        );

        // Angle in radians from the view axis
        // to the top plane of the view pyramid.
        float verticalAngle = 0.5f * Mathf.Deg2Rad * _mainCamera.fieldOfView;

        // World space height of the view pyramid
        // measured at 1 m depth from the camera.
        float worldHeight = 2f * Mathf.Tan (verticalAngle);

        // Convert relative position to world units.
        Vector3 worldUnits = relative * worldHeight;
        worldUnits.x *= _mainCamera.aspect;
        worldUnits.z = 1;

        // Rotate to match camera orientation.
        Vector3 direction = _mainCamera.transform.rotation * worldUnits;

        // Output a ray from camera position, along this direction.
        if (Physics.Raycast (_mainCamera.transform.position, direction, out RaycastHit hit, Mathf.Infinity,
                layerMask : 1 << LayerMask.NameToLayer ("AttackSelectionSprite")))
        {
            hitPos = hit.point;
            return true;
        }
        return false;
    }

    protected virtual bool IsPositionInAttackRange (Vector3 position)
    {
        return Vector3.Distance (position, _attackerOriginPosition) <= _radiusAdded + _attack.Range &&
            position.x >= _combatZone.LineXMin && position.x <= _combatZone.LineXMax;
    }

    protected void UpdateIconDisplay (bool inRange)
    {
        _invalidSelectionIcon.SetActive (!inRange);
        _validSelectionIcon.SetActive (inRange);
    }
}