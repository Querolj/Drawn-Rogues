using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Collider))]
public class AttackableDetector : MonoBehaviour
{
    public bool RemoveAttackableOnExit { set; get; } = false;

    private Dictionary<int, Attackable> _instanceFound = new Dictionary<int, Attackable> ();
    private List<Attackable> _attackablesInZone = new List<Attackable> ();
    public List<Attackable> AttackablesInZone
    {
        get
        {
            _attackablesInZone.RemoveAll (attackable => attackable == null || attackable.WillBeDestroyed);
            return _attackablesInZone;
        }
    }

    private List<Character> _enemiesCharactersInZone = new List<Character> ();
    public List<Character> EnemiesCharactersInZone
    {
        get
        {
            // remove null characters (killed) in the list before returning it
            _enemiesCharactersInZone.RemoveAll (character => character == null || character.WillBeDestroyed);
            return _enemiesCharactersInZone;
        }
    }

    private Collider _collider;
    public Bounds Bounds { get { return _collider.bounds; } }
    public Action<Attackable> OnAttackableDetected;
    public Action<Attackable> OnAttackableOut;

    private const float Z_RANGE_DETECTION = 0.1f;

    private void Awake ()
    {
        _collider = GetComponent<Collider> ();
        Vector3 newPos = transform.position;
        newPos.z = transform.position.z + 0.05f; // small offset to be behind attackable, so we can trigger OnMouseEnter/Exit events on attackables
        transform.position = newPos;
    }

    private void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Player")
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer ("Attackable") && Mathf.Abs (other.transform.position.z - transform.position.z) < Z_RANGE_DETECTION)
        {
            Attackable attackable;
            if (!_instanceFound.ContainsKey (other.gameObject.GetInstanceID ()))
            {
                attackable = other.gameObject.GetComponentInParent<Attackable> ();
                if (attackable == null)
                    Debug.LogError ("AttackableDetector: " + other.gameObject.name + " has no Attackable component");
                _instanceFound.Add (other.gameObject.GetInstanceID (), attackable);
            }
            else
                attackable = _instanceFound[other.gameObject.GetInstanceID ()];

            if (!_attackablesInZone.Contains (attackable))
            {
                _attackablesInZone.Add (attackable);
                if (other.tag == "Enemy")
                {
                    _enemiesCharactersInZone.Add ((Character) attackable);
                }
            }

            OnAttackableDetected?.Invoke (attackable);
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (!RemoveAttackableOnExit || (other.tag == "Player"))
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer ("Attackable") && Mathf.Abs (other.transform.position.z - transform.position.z) < Z_RANGE_DETECTION)
        {
            Attackable attackable;
            if (!_instanceFound.ContainsKey (other.gameObject.GetInstanceID ()))
            {
                attackable = other.gameObject.GetComponentInParent<Attackable> ();
                _instanceFound.Add (other.gameObject.GetInstanceID (), attackable);
            }
            else
                attackable = _instanceFound[other.gameObject.GetInstanceID ()];

            if (_attackablesInZone.Contains (attackable))
            {
                _attackablesInZone.Remove (attackable);
                if (other.tag == "Enemy")
                    _enemiesCharactersInZone.Remove ((Character) attackable);
            }

            OnAttackableOut?.Invoke (attackable);
        }
    }
}