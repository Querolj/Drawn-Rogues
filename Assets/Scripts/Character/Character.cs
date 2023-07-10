using System;
using System.Collections.Generic;
using UnityEngine;

public class Character : Attackable
{
    [SerializeField]
    private Attack[] _attacksReferences;

    [SerializeField]
    private float _reduceEscapeChanceValue = 0.2f;
    public float ReduceEscapeChanceValue { get { return _reduceEscapeChanceValue; } }

    private List<Attack> _attacks = new List<Attack> ();

    public List<Attack> Attacks
    {
        get { return _attacks; }
    }

    private AIBehaviour _ai;
    private CharacterMovement _charMovement;
    public CharacterMovement CharMovement { get { return _charMovement; } }
    private CharacterPivot _pivot;
    public CharacterPivot Pivot { get { return _pivot; } }
    protected Collider _collider;
    public int ColliderId { get { return _collider.gameObject.GetInstanceID (); } }
    protected bool _isEnemy = true;
    public bool IsEnemy { get { return _isEnemy; } }

    public float MaxDistanceToMove
    {
        get { return Mathf.Max (0.1f, (Stats.Mobility / 150f)); }
    }

    private Action OnTurnStarted;
    public void AddOnTurnStarted (Action action)
    {
        OnTurnStarted += action;
    }

    public void RemoveOnTurnStarted (Action action)
    {
        OnTurnStarted -= action;
    }

    public void CallOnTurnStarted ()
    {
        OnTurnStarted?.Invoke ();
    }

    protected override void Awake ()
    {
        base.Awake ();

        _charMovement = GetComponentInParent<CharacterMovement> ();
        _pivot = GetComponentInParent<CharacterPivot> ();
        _collider = GetComponentInChildren<Collider> ();

        TrajectoryCalculator trajectoryCalculator = new TrajectoryCalculator ();

        foreach (Attack attack in _attacksReferences)
        {
            Attack inst = Instantiate (attack);
            _attacks.Add (inst);
        }

        if (tag != "Player")
        {
            _ai = GetComponent<AIBehaviour> ();
        }

    }

    public override Bounds GetSpriteBounds ()
    {
        Vector4 border = GraphicUtils.GetTextureBorder (_renderer.sprite.texture);
        border /= PIXEL_PER_UNIT;

        Vector3 boundsSize = new Vector3 (border.z - border.x, border.w - border.y, 0f);
        Vector3 boundsCenter = transform.position;
        boundsCenter -= _renderer.bounds.extents;
        float borderXRotated;
        if (!CharMovement.DirectionRight)
        {
            borderXRotated = _renderer.bounds.size.x - border.z;
        }
        else
            borderXRotated = border.x;
        boundsCenter += new Vector3 (borderXRotated, border.y, 0f);
        boundsCenter.y += boundsSize.y / 2f;
        boundsCenter.x += boundsSize.x / 2f;

        _bounds = new Bounds (boundsCenter, boundsSize);
        // _bounds = RotateBounds (new Bounds (boundsCenter, boundsSize), GetComponentInParent<CharacterAnimation> ().transform.eulerAngles);
        // Debug.Log ("boundsCenter center rotated : " + boundsCenter.ToString ("F3"));

        return _bounds;
    }

    public override int GetTurnOrder ()
    {
        return Stats.Mobility;
    }

    public void SetMaxAndCurrentLife (bool resetCurrentLife = true)
    {
        _maxLife = Stats.Life;
        if (resetCurrentLife)
            _currentLife = _maxLife;
    }

    public bool HasAI ()
    {
        return _ai != null;
    }

    public AIBehaviour GetAI ()
    {
        if (!HasAI ())
            Debug.LogWarning (name + " has no AI, returning null.");

        return _ai;
    }

    public bool CanPlayTurn ()
    {
        bool canPlayTurn = true;
        canPlayTurn = !HasState (State.Stunned);

        return canPlayTurn;
    }
}