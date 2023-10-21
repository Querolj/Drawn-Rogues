using System.Collections.Generic;
using UnityEngine;

public class Balance : MonoBehaviour
{
    [SerializeField]
    private float _lerpSpeed = 0.1f;

    [SerializeField]
    private TransformToLerp[] _transformToLerps;

    [SerializeField]
    private float _kilogramMax = 100;

    [SerializeField]
    private GameObject _bell;

    [SerializeField]
    private EventMap _eventBridge;

    private Dictionary<Attackable, Vector3> _attackablesDetectedWithLastPosition = new Dictionary<Attackable, Vector3> ();
    private float _currentKilogram = 0f;
    private float _targetLerp = 0f;
    private float _currentLerp = 0f;

    private float _initialButtonYPosition;
    #region frame optimisation
    private const int UPDATE_PER_SECONDS = 24;
    private float _timeSinceLastUpdate = 0f;
    #endregion

    private void Awake ()
    {
        foreach (TransformToLerp transformToLerp in _transformToLerps)
        {
            transformToLerp.Init ();
        }

        _initialButtonYPosition = _bell.transform.position.y;
    }

    private float GetButtonOffset ()
    {
        return Mathf.Abs (_bell.transform.position.y - _initialButtonYPosition);
    }

    private void LateUpdate ()
    {
        if (_attackablesDetectedWithLastPosition.Count == 0)
        {
            _currentKilogram = 0f;
            _targetLerp = 0f;
            return;
        }

        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate < 1f / UPDATE_PER_SECONDS)
        {
            return;
        }
        _timeSinceLastUpdate = 0f;

        _currentKilogram = 0f;
        List<Attackable> attackablesToUpdate = new List<Attackable> ();
        foreach (Attackable attackable in _attackablesDetectedWithLastPosition.Keys)
        {
            Vector3 lastPosition = _attackablesDetectedWithLastPosition[attackable];
            if (attackable != null)
            {
                // Is the attackable not falling down ?
                if (Mathf.Abs (attackable.gameObject.transform.position.y - lastPosition.y) < 0.001f + GetButtonOffset ())
                {
                    _currentKilogram += attackable.Stats.Kilogram;
                }
                attackablesToUpdate.Add (attackable);
            }
        }

        foreach (Attackable attackable in attackablesToUpdate)
        {
            _attackablesDetectedWithLastPosition[attackable] = attackable.gameObject.transform.position + new Vector3 (0, GetButtonOffset (), 0);
        }

        _targetLerp = _currentKilogram / _kilogramMax;

    }

    private void Update ()
    {
        if (_eventBridge.IsTriggered)
            return;

        if (_currentLerp >= 1f)
        {
            _eventBridge.Trigger ();
            return;
        }

        if (Mathf.Abs (_currentLerp - _targetLerp) < 0.001)
        {
            return;
        }

        if (_currentLerp < _targetLerp)
        {
            _currentLerp += _lerpSpeed * Time.deltaTime;
        }
        else
        {
            _currentLerp -= _lerpSpeed * Time.deltaTime;
        }

        foreach (TransformToLerp transformToLerp in _transformToLerps)
        {
            transformToLerp.LerpToTarget (_currentLerp);
        }
    }

    private void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer ("Attackable"))
        {
            Attackable attackable = other.GetComponent<Attackable> ();
            if (!_attackablesDetectedWithLastPosition.ContainsKey (attackable))
            {
                Vector3 pos = attackable.gameObject.transform.position;
                pos.y += GetButtonOffset ();
                _attackablesDetectedWithLastPosition.Add (attackable, pos);
            }
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer ("Attackable"))
        {
            Attackable attackable = other.GetComponent<Attackable> ();
            if (_attackablesDetectedWithLastPosition.ContainsKey (attackable))
            {
                _attackablesDetectedWithLastPosition.Remove (attackable);
            }
        }
    }
}

[System.Serializable]
public class TransformToLerp
{
    [SerializeField]
    private Transform _transformToLerp;

    [SerializeField]
    private Vector3 _targetLocation;

    [SerializeField]
    private bool _lerpLocation = true;

    [SerializeField]
    private Vector3 _targetEulerAngles;

    [SerializeField]
    private bool _lerpRotation = false;

    [SerializeField]
    private bool _localSpace = true;

    private Vector3 _initialLocation;
    private Vector3 _initialAngles;

    public void Init ()
    {
        if (!_lerpLocation && !_lerpRotation)
        {
            Debug.LogWarning ("TransformToLerp " + _transformToLerp.name + " has no lerp enabled");
            return;
        }

        if (_localSpace)
        {
            _initialLocation = _transformToLerp.localPosition;
            _initialAngles = _transformToLerp.localEulerAngles;
        }
        else
        {
            _initialLocation = _transformToLerp.position;
            _initialAngles = _transformToLerp.eulerAngles;
        }
    }

    public void LerpToTarget (float lerpSpeed)
    {
        if (_localSpace)
        {
            if (_lerpLocation)
                _transformToLerp.localPosition = Vector3.Lerp (_initialLocation, _targetLocation, lerpSpeed);

            if (_lerpRotation)
                _transformToLerp.localEulerAngles = Vector3.Lerp (_initialAngles, _targetEulerAngles, lerpSpeed);
        }
        else
        {
            if (_lerpLocation)
                _transformToLerp.position = Vector3.Lerp (_initialLocation, _targetLocation, lerpSpeed);

            if (_lerpRotation)
                _transformToLerp.eulerAngles = Vector3.Lerp (_initialAngles, _targetEulerAngles, lerpSpeed);
        }
    }
}