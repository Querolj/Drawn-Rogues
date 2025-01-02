using System;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : Attackable, IColouringSpellBehaviour, IPlayerProjectileDefense
{

    private TurnManager _turnManager;
    private float _waveCurrentDuration = 0f;
    private const float WAVE_MAX_DURATION = 1f;

    public void Init (TurnManager turnManager, List<Vector2> lastStrokeDrawUVs, FrameDecor frameDecor, Action onInitDone = null)
    {
        _turnManager = turnManager;

        _waveCurrentDuration = WAVE_MAX_DURATION;

        int life = (int) ((float) _turnManager.ActivePlayerCharacter.Stats.Intelligence * 0.1f);
        life = Mathf.Max (life, 1);
        Stats.MaxLife = life;
        
        onInitDone?.Invoke ();
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

    public void ReportImpactPoint (Vector2 impactPoint)
    {
        _waveCurrentDuration = 0f;
        _renderer.material.SetInt ("_ImpactPointReported", 1);
        _renderer.material.SetVector ("_ImpactPointWorld", impactPoint);
    }
}