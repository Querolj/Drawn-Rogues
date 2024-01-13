using System;
using UnityEngine;

[RequireComponent (typeof (ParticleSystem))]
public class ParticleSystemCallback : MonoBehaviour
{
    public event Action OnParticleSystemDestroyed;

    private ParticleSystem _particleSystem;

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
}