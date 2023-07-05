using UnityEngine;

public class AttackableFocuser : MonoBehaviour
{
    private Attackable _lastFocusedAttackable;
    private void Update ()
    {
        if (Utils.TryGetFirstAttackableUnderMouse (Input.mousePosition, out Attackable attackable))
        {
            if (attackable != _lastFocusedAttackable)
            {
                if (_lastFocusedAttackable != null)
                {
                    _lastFocusedAttackable.OnMouseExit ();
                }
                _lastFocusedAttackable = attackable;
                _lastFocusedAttackable.OnMouseEnter ();
            }
        }
        else if (_lastFocusedAttackable != null)
        {
            _lastFocusedAttackable.OnMouseExit ();
            _lastFocusedAttackable = null;
        }
    }
}