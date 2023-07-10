using System;
using UnityEngine;

[RequireComponent (typeof (ParticleSystem))]
public class ParticleSystemCallback : MonoBehaviour
{
    public Action OnParticleSystemDestroyed;

    void OnParticleSystemStopped ()
    {
        OnParticleSystemDestroyed?.Invoke ();
        Destroy (gameObject);
    }

    public void Play ()
    {
        GetComponent<ParticleSystem> ().Play ();
    }
}