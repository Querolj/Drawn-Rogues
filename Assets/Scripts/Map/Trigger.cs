using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Collider))]
public class Trigger : MonoBehaviour
{
    public event Action<Collider> OnDetect;
    public event Action<Collider> OnExit;

    private MeshRenderer _renderer;
    private Collider _collider;
    public Bounds Bounds
    {
        get
        {
            if (_collider == null)
                _collider = GetComponent<Collider> ();
            return _collider.bounds;
        }
    }
    private void Awake ()
    {
        _renderer = GetComponent<MeshRenderer> ();
        if (_collider == null)
            _collider = GetComponent<Collider> ();
    }

    private HashSet<int> _idsDetected = new HashSet<int> ();
    private void OnTriggerEnter (Collider other)
    {
        int id = other.gameObject.GetInstanceID ();
        if (_idsDetected.Contains (id))
            return;

        _idsDetected.Add (id);
        OnDetect?.Invoke (other);
    }

    private void OnTriggerExit (Collider other)
    {
        _idsDetected.Remove (other.gameObject.GetInstanceID ());
        OnExit?.Invoke (other);
    }

    public void HideRenderer ()
    {
        _renderer.enabled = false;
    }

    public void ShowRenderer ()
    {
        _renderer.enabled = true;
    }

}