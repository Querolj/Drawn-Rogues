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
    private float _heavyDepressionHeight = 0.1f;

    [SerializeField]
    private float _lightDepressionHeight = 0.02f;

    [SerializeField]
    private float _kilogramThreshold = 40f;

    [SerializeField]
    private float _playerSpeedMultiplierWhenInQuicksand = 0.2f;

    private Character _player = null;
    private Bounds _playerBounds;
    private Material _material;

    void Awake ()
    {
        _trigger.OnDetect += OnDetect;
        _trigger.OnExit += OnExit;
        _material = GetComponent<MeshRenderer> ().material;
    }

    private void OnDetect (Collider other)
    {
        if (_player == null && other.gameObject.CompareTag ("Player"))
        {
            _player = other.gameObject.GetComponent<Character> ();
            _playerBounds = _player.GetSpriteBounds ();
            _material.SetVector ("_BoundCenter", _playerBounds.center);
            _material.SetFloat ("_BoundExtendX", _playerBounds.extents.x);
            _material.SetInt ("_IsPlayerOnTopOfQuickSand", 1);
            if (_player.Stats.Kilogram > _kilogramThreshold)
            {
                _material.SetFloat ("_MaxDepressionHeight", _heavyDepressionHeight);
                _player.CharMovement.SetOffsetY (_heavyDepressionHeight);
                _player.CharMovement.SetWalkingSpeedMultiplier (_playerSpeedMultiplierWhenInQuicksand);
            }
            else
            {
                _material.SetFloat ("_MaxDepressionHeight", _lightDepressionHeight);
                _player.CharMovement.SetOffsetY (_lightDepressionHeight);
            }
            Debug.Log ("Player detected");
        }
    }

    private void OnExit (Collider other)
    {
        if (other.gameObject.CompareTag ("Player") && _player != null)
        {
            _player.CharMovement.SetWalkingSpeedMultiplier (1f);
            _player.CharMovement.SetOffsetY (0f);
            _player = null;
            _material.SetInt ("_IsPlayerOnTopOfQuickSand", 0);
            Debug.Log ("Player exited");
        }
    }

    private void Update()
    {
        if (_player != null)
        {
            _playerBounds = _player.GetSpriteBounds ();
            _material.SetVector ("_BoundCenter", _playerBounds.center);
            _material.SetFloat ("_BoundExtendX", _playerBounds.extents.x);
        }
    }
}