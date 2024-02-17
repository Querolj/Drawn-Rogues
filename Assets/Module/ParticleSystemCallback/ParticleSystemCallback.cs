using System;
using UnityEngine;

[RequireComponent (typeof (ParticleSystem))]
public class ParticleSystemCallback : MonoBehaviour
{
    public event Action OnParticleSystemDestroyed;

    private ParticleSystem _particleSystem;
    private Vector3 _targetPositionOffset;
    private Transform _target;

    void OnParticleSystemStopped ()
    {
        OnParticleSystemDestroyed?.Invoke ();
        Destroy (gameObject);
    }

    public void Play ()
    {
        if (_particleSystem == null)
            _particleSystem = GetComponent<ParticleSystem> ();

        _particleSystem.Play ();
    }

    public void SetTarget (Transform target)
    {
        _target = target;
        _targetPositionOffset = transform.position - target.position;
    }

    private void Update()
    {
        if (_target != null)
        {
            transform.position = _target.position + _targetPositionOffset;
        }
    }
}