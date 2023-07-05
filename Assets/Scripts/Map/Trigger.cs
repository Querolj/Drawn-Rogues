using System;
using UnityEngine;

[RequireComponent (typeof (Collider))]
public class Trigger : MonoBehaviour
{
    public event Action<Collider> OnDetect;
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

    private void OnTriggerEnter (Collider other)
    {
        OnDetect?.Invoke (other);
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