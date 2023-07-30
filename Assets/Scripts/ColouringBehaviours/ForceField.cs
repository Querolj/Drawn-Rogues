using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class ForceField : Attackable, IColouringSpellBehaviour, IPlayerProjectileDefense
{

    private TurnManager _turnManager;
    private float _waveCurrentDuration = 0f;
    private const float WAVE_MAX_DURATION = 1f;

    protected override void Awake ()
    {
        base.Awake ();
        _waveCurrentDuration = WAVE_MAX_DURATION;
        _renderer.material.SetInt ("_ImpactPointReported", 0);
    }

    public void Init (TurnManager turnManager, List<Vector2> lastStrokeDrawUVs, FrameDecor frameDecor)
    {
        _turnManager = turnManager;
    }

    protected override void Start ()
    {
        base.Start ();

        //Set life
        if(_turnManager== null)
            Debug.LogError("_turnManager null");
        _maxLife = (int) ((float) _turnManager.ActivePlayerCharacter.Stats.Intelligence * 0.1f);
        _currentLife = _maxLife;
    }

    protected override void Update ()
    {
        base.Update ();

        _renderer.material.SetVector ("_UserPosition", _turnManager.ActivePlayerCharacter.gameObject.transform.position);

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