using System;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class MapAttackAOE : MonoBehaviour
{
    [SerializeField]
    private AttackSingleTargetNoOwner _attack;

    [SerializeField]
    private float _tick;

    [SerializeField]
    private bool _triggerOnEnter = false;

    [SerializeField]
    private Trigger[] _triggers;

    private int _triggerThatDetectedPlayer = 0;
    private DrawedCharacter _player = null;
    private float _elapsedTime = 0f;
    private AttackInstSingleTargetNoOwner _attackInstance;

    protected AttackInstance.Factory _attackInstanceFactory;

    [Inject, UsedImplicitly]
    private void Init (AttackInstance.Factory attackInstanceFactory)
    {
        _attackInstanceFactory = attackInstanceFactory;
    }

    private void Awake ()
    {
        foreach (var trigger in _triggers)
        {
            trigger.OnDetect += OnDetect;
            trigger.OnExit += OnExit;
        }

        if (_attackInstanceFactory.Create (_attack, null) is not AttackInstSingleTargetNoOwner attackInstSingleTargetNoOwner)
            throw new ArgumentException ("Attack must be of type AttackSingleTargetNoOwner");

        _attackInstance = attackInstSingleTargetNoOwner;
    }

    private void OnDetect (Collider other)
    {
        if (other.gameObject.CompareTag ("Player"))
        {
            if (_player == null)
                _player = other.gameObject.GetComponent<DrawedCharacter> ();
            if (_triggerThatDetectedPlayer == 0 && _triggerOnEnter)
                _attackInstance.Execute (null, _player, _player.transform.position, null);

            _triggerThatDetectedPlayer++;
        }
    }

    private void OnExit (Collider other)
    {
        if (other.gameObject.CompareTag ("Player"))
        {
            _triggerThatDetectedPlayer--;
            if (_triggerThatDetectedPlayer == 0)
            {
                _elapsedTime = 0f;
            }
        }
    }

    private void Update ()
    {
        if (_triggerThatDetectedPlayer == 0)
        {
            _elapsedTime = 0f;
            return;
        }

        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= _tick)
        {
            _elapsedTime = _elapsedTime - _tick;
            _attackInstance.Execute (null, _player, _player.transform.position, null);
        }
    }
}