using System;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    Poisonned,
    Stunned,
    Burn,
    Paralyzed,
    Bleed
}

public class AttackableState
{
    private int _maxLife;
    public int MaxLife
    {
        get { return _maxLife; }
    }

    private int _currentLife;
    public int CurrentLife
    {
        get { return _currentLife; }
    }

    public event Action OnLifeReachZero;
    public event Action OnStateListChanged;

    private HashSet<State> _states = new HashSet<State> ();

    public AttackableState (int maxLife)
    {
        _maxLife = maxLife;
        _currentLife = maxLife;
    }

    public void ReceiveDamage (int damageAmount)
    {
        _currentLife -= damageAmount;
        _currentLife = Mathf.Clamp (_currentLife, 0, MaxLife);
        if (_currentLife == 0)
        {
            OnLifeReachZero?.Invoke ();
        }
    }

    public void Heal (int healAmount)
    {
        _currentLife += healAmount;
        _currentLife = Mathf.Clamp (_currentLife, 0, MaxLife);
    }

    public bool HasState (State state)
    {
        return _states.Contains (state);
    }

    public void AddState (State state)
    {
        _states.Add (state);
        OnStateListChanged?.Invoke ();
    }

    public void RemoveState (State state)
    {
        _states.Remove (state);
        OnStateListChanged?.Invoke ();
    }
}