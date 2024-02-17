using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class QuickSand : MonoBehaviour
{
    [SerializeField]
    private Trigger _trigger;

    [SerializeField]
    private float _kilogramThreshold = 40f;

    [SerializeField]
    private float _playerSpeedMultiplierWhenInQuicksand = 0.2f;
    
    private Attackable _player = null;
    private Bounds _playerBounds;
    private Material _material;
    // [Inject, UsedImplicitly]
    // private void Init ()
    // {
    // }
    
    void Start()
    {
        _trigger.OnDetect += OnDetect;
        _trigger.OnExit += OnExit;
        _material = GetComponent<MeshRenderer>().material;
    }

    private void OnDetect (Collider other)
    {
        if (_player == null && other.gameObject.CompareTag ("Player"))
        {
            _player = other.gameObject.GetComponent<Attackable>();
            _playerBounds = _player.GetSpriteBounds();
            _material.SetVector ("_BoundCenter", _playerBounds.center);
            _material.SetFloat ("_BoundExtendX", _playerBounds.extents.x);
            Debug.Log("Player detected");
        }
    }

    private void OnExit (Collider other)
    {
        if (other.gameObject.CompareTag ("Player"))
        {
            _player = null;
            Debug.Log("Player exited");
        }
    }
}
