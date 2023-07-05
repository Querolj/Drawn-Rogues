using System.Collections.Generic;
using UnityEngine;

public class ForceField : Attackable, IColouringSpellBehaviour, IPlayerProjectileDefense
{

    private TurnBasedCombat _turnBasedCombat;
    private float _waveCurrentDuration = 0f;
    private const float WAVE_MAX_DURATION = 1f;

    protected override void Awake ()
    {
        base.Awake ();
        _waveCurrentDuration = WAVE_MAX_DURATION;
        _renderer.material.SetInt ("_ImpactPointReported", 0);
    }

    public void Init (TurnBasedCombat turnBasedCombat, List<Vector2> lastStrokeDrawUVs, FrameDecor frameDecor)
    {
        _turnBasedCombat = turnBasedCombat;
    }

    protected override void Start ()
    {
        base.Start ();

        //Set life
        _maxLife = (int) ((float) _turnBasedCombat.ActivePlayerCharacter.Stats.Intelligence * 0.1f);
        _currentLife = _maxLife;
    }

    protected override void Update ()
    {
        base.Update ();

        _renderer.material.SetVector ("_UserPosition", _turnBasedCombat.ActivePlayerCharacter.gameObject.transform.position);

        if (_waveCurrentDuration < WAVE_MAX_DURATION)
        {
            _waveCurrentDuration += Time.deltaTime;
            if (_waveCurrentDuration >= WAVE_MAX_DURATION)
                _renderer.material.SetInt ("_ImpactPointReported", 0);

            _renderer.material.SetFloat ("_WaveDuration", _waveCurrentDuration);
        }
    }

    private Vector2 WorldToRendererSpace (Vector3 point)
    {
        Vector3 worldOrigin = _renderer.bounds.center - _renderer.bounds.extents;
        Vector3 worldOriginTop = _renderer.bounds.center + _renderer.bounds.extents;

        float distX = worldOriginTop.x - worldOrigin.x;
        float x = point.x - worldOrigin.x;
        x /= distX;

        float distY = worldOriginTop.y - worldOrigin.y;
        float y = point.y - worldOrigin.y;
        y /= distY;

        return new Vector2 (x, y);
    }

    public void ReportImpactPoint (Vector2 impactPoint)
    {
        _waveCurrentDuration = 0f;
        _renderer.material.SetInt ("_ImpactPointReported", 1);
        _renderer.material.SetVector ("_ImpactPointWorld", impactPoint);
    }
}