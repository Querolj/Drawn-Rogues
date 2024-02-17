using System;
using UnityEngine;

public class Squasher : MonoBehaviour
{
    private float _yScaleTarget = 0f;
    private float _initialYScale = 1f;

    private float _durationSquash = 0f;
    private float _durationUnsquash = 0f;

    private float _currentLerp = 0f;
    private Action _onSquashFinished;

    private void Update ()
    {
        if (_currentLerp < _durationSquash)
        {
            _currentLerp += Time.deltaTime;
            float yScale = Mathf.Lerp (_initialYScale, _yScaleTarget, _currentLerp / _durationSquash);
            transform.localScale = new Vector3 (1f, yScale, 1f);

            if (_currentLerp >= _durationSquash && transform.localScale.y < 0.999f) // reverse squash
            {
                _durationSquash = _durationUnsquash;
                _currentLerp = 0f;
                _yScaleTarget = 1f;
                _initialYScale = transform.localScale.y;
            }

        }
        else if (_onSquashFinished != null)
        {
            _onSquashFinished ();
            _onSquashFinished = null;
        }
    }

    public void SquashHorizontally (float yScaleTarget, float durationSquash, float durationUnsquash, Action onSquashFinished = null)
    {
        _yScaleTarget = yScaleTarget;
        _durationSquash = durationSquash;
        _durationUnsquash = durationUnsquash;
        _currentLerp = 0f;
        _initialYScale = 1f;
        _onSquashFinished = onSquashFinished;
    }

}