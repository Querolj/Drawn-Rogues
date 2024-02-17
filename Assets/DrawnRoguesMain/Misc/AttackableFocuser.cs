using System.Collections.Generic;
using UnityEngine;

public class AttackableFocuser : MonoBehaviour
{
    private Attackable _lastFocusedAttackable;
    private Camera _mainCamera;
    private void Update ()
    {
        if (TryGetFirstAttackableUnderMouse (Input.mousePosition, out Attackable attackable))
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

    private Dictionary<int, Attackable> _attackablesByInstanceID = new Dictionary<int, Attackable> ();
    private bool TryGetFirstAttackableUnderMouse (Vector3 screenPos, out Attackable attackable)
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        attackable = null;

        // Remap so (0, 0) is the center of the window,
        // and the edges are at -0.5 and +0.5.
        Vector2 relative = new Vector2 (
            screenPos.x / Screen.width - 0.5f,
            screenPos.y / Screen.height - 0.5f
        );

        // Angle in radians from the view axis
        // to the top plane of the view pyramid.
        float verticalAngle = 0.5f * Mathf.Deg2Rad * _mainCamera.fieldOfView;

        // World space height of the view pyramid
        // measured at 1 m depth from the camera.
        float worldHeight = 2f * Mathf.Tan (verticalAngle);

        // Convert relative position to world units.
        Vector3 worldUnits = relative * worldHeight;
        worldUnits.x *= _mainCamera.aspect;
        worldUnits.z = 1;

        // Rotate to match camera orientation.
        Vector3 direction = _mainCamera.transform.rotation * worldUnits;

        RaycastHit[] results = new RaycastHit[1];
        int hits = Physics.RaycastNonAlloc (_mainCamera.transform.position, direction, results, Mathf.Infinity, layerMask : 1 << LayerMask.NameToLayer ("Attackable"));

        if (hits > 0)
        {
            int id = results[0].collider.GetInstanceID ();
            if (!_attackablesByInstanceID.ContainsKey (id))
                _attackablesByInstanceID.Add (id, results[0].collider.GetComponentInParent<Attackable> ());
            attackable = _attackablesByInstanceID[id];
            return true;
        }

        return false;
    }
}