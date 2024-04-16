using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MoveModifier : MonoBehaviour
{
    private Vector3 bottomLeftScreenLimit;
    private Vector3 topRightScreenLimit;
    private CursorModeSwitcher _modeSwitcher;
    private Modifier _modifier;
    private Action<Modifier> _onModifierDeleted;
    private bool _initialized = false;
    private InputActionReference _deleteModifierInput;

    public void Init (Vector3 bottomLeftLimit, Vector3 topRightLimit, CursorModeSwitcher modeSwitcher, Modifier modifier, InputActionReference deleteModifierInput, Action<Modifier> onModifierDeleted)
    {
        bottomLeftScreenLimit = bottomLeftLimit;
        topRightScreenLimit = topRightLimit;
        _modeSwitcher = modeSwitcher;
        _modifier = modifier;
        _onModifierDeleted = onModifierDeleted;
        _initialized = true;

        _deleteModifierInput = deleteModifierInput;
        _deleteModifierInput.action.performed += OnDeleteModifier;
    }

    private void OnDestroy ()
    {
        _deleteModifierInput.action.performed -= OnDeleteModifier;
    }

    private void OnDeleteModifier (InputAction.CallbackContext context)
    {
        if (!_initialized)
            return;

        if (_modeSwitcher.CurrentMode != CursorModeSwitcher.Mode.Selection)
            return;

        _onModifierDeleted?.Invoke (_modifier);
        Destroy (gameObject);
    }

    public void OnMouseDrag ()
    {
        if (!_initialized)
            return;

        if (_modeSwitcher.CurrentMode != CursorModeSwitcher.Mode.Selection)
            return;

        Vector3 mousePosClamped = Mouse.current.position.ReadValue ();
        mousePosClamped.x = Mathf.Clamp (mousePosClamped.x, bottomLeftScreenLimit.x, topRightScreenLimit.x);
        mousePosClamped.y = Mathf.Clamp (mousePosClamped.y, bottomLeftScreenLimit.y, topRightScreenLimit.y);

        if (Utils.TryGetScreenToLayerPosition (mousePosClamped, out Vector3 hitPos, 1 << LayerMask.NameToLayer ("Frame3D")))
            transform.position = hitPos;
    }
}