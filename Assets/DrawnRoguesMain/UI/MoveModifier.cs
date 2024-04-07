using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveModifier : MonoBehaviour
{
    private Vector3 bottomLeftScreenLimit;
    private Vector3 topRightScreenLimit;
    private CursorModeSwitcher _modeSwitcher;
    private Modifier _modifier;
    private Action<Modifier> _onModifierDeleted;
    private bool _initialized = false;

    public void Init (Vector3 bottomLeftLimit, Vector3 topRightLimit, CursorModeSwitcher modeSwitcher, Modifier modifier, Action<Modifier> onModifierDeleted)
    {
        bottomLeftScreenLimit = bottomLeftLimit;
        topRightScreenLimit = topRightLimit;
        _modeSwitcher = modeSwitcher;
        _modifier = modifier;
        _onModifierDeleted = onModifierDeleted;
        _initialized = true;
    }

    private void OnMouseDrag ()
    {
        if (!_initialized)
            return;

        if (_modeSwitcher.CurrentMode != CursorModeSwitcher.Mode.Selection)
            return;

        Vector3 mousePosClamped = Mouse.current.position.ReadValue();
        mousePosClamped.x = Mathf.Clamp (mousePosClamped.x, bottomLeftScreenLimit.x, topRightScreenLimit.x);
        mousePosClamped.y = Mathf.Clamp (mousePosClamped.y, bottomLeftScreenLimit.y, topRightScreenLimit.y);

        if (Utils.TryGetScreenToLayerPosition (mousePosClamped, out Vector3 hitPos, 1 << LayerMask.NameToLayer ("Frame3D")))
            transform.position = hitPos;
    }

    private void OnMouseOver ()
    {
        if (!_initialized)
            return;

        if (_modeSwitcher.CurrentMode != CursorModeSwitcher.Mode.Selection)
            return;

        if (Input.GetKeyDown (KeyCode.Delete))
        {
            _onModifierDeleted?.Invoke (_modifier);
            Destroy (gameObject);
        }
    }
}