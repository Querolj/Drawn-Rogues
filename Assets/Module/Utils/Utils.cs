using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Utils
{
    private static Dictionary<Type, int> _maxEnumValueByType = new Dictionary<Type, int> ();

    public static int GetMaxEnumValue (Type enumType)
    {
        if (_maxEnumValueByType.ContainsKey (enumType))
            return _maxEnumValueByType[enumType];

        int max = Enum.GetValues (enumType).Cast<int> ().Max ();
        _maxEnumValueByType.Add (enumType, max);

        return max;
    }

    private static Camera _mainCamera = null;
    public static bool TryGetMouseToMapPosition (out Vector3 hitPos)
    {
        Vector3 screenPos = Mouse.current.position.ReadValue ();
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        hitPos = Vector3.zero;
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

        // Output a ray from camera position, along this direction.
        if (Physics.Raycast (_mainCamera.transform.position, direction, out RaycastHit hit, Mathf.Infinity, layerMask : 1 << LayerMask.NameToLayer ("Map")))
        {
            hitPos = hit.point;
            return true;
        }
        return false;
    }

    public static bool TryGetViewportToLayerPosition (Vector3 viewportPos, out Vector3 hitPos, out GameObject goHit, int layer)
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        Vector3 screenPos = _mainCamera.ViewportToScreenPoint (viewportPos);
        return TryGetScreenToLayerPosition (screenPos, out hitPos, out goHit, layer);
    }

    public static bool TryGetMouseToLayerPosition (out Vector3 hitPos, int layer)
    {
        Vector3 screenPos = Mouse.current.position.ReadValue ();
        return TryGetScreenToLayerPosition (screenPos, out hitPos, layer);
    }

    public static bool TryGetScreenToLayerPosition (Vector3 screenPos, out Vector3 hitPos, out GameObject goHit, int layer)
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        hitPos = Vector3.zero;
        goHit = null;
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

        // Output a ray from camera position, along this direction.
        if (Physics.Raycast (_mainCamera.transform.position, direction, out RaycastHit hit, Mathf.Infinity, layerMask : layer))
        {
            hitPos = hit.point;
            goHit = hit.collider.gameObject;
            return true;
        }
        return false;
    }

    public static bool TryGetScreenToLayerPosition (Vector3 screenPos, out Vector3 hitPos, int layer)
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        hitPos = Vector3.zero;
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

        // Output a ray from camera position, along this direction.
        if (Physics.Raycast (_mainCamera.transform.position, direction, out RaycastHit hit, Mathf.Infinity, layerMask : layer))
        {
            hitPos = hit.point;
            return true;
        }
        return false;
    }

    public static float GetMapHeight (Vector3 position)
    {
        position.y = 100f;
        if (Physics.Raycast (position, Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            return hit.point.y;
        }
        return 0f;
    }

    public static float GetMapHeight (Bounds bounds)
    {
        const float RAYCAST_OFFSET = 0.1f;
        if (Physics.Raycast (bounds.center + Vector3.up * RAYCAST_OFFSET, Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            return hit.point.y;
        }
        return 0f;
    }

    public static bool TryGetMapHeight (Vector3 position, out float height)
    {
        height = 0f;
        position.y = 100f;
        if (Physics.Raycast (position, Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            height = hit.point.y;
            return true;
        }
        return false;
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement ()
    {
        return IsPointerOverUIElement (GetEventSystemRaycastResults ());
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    private static bool IsPointerOverUIElement (List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer ("UI"))
                return true;
        }
        return false;
    }

    //Gets all event system raycast results of current mouse or touch position.
    private static List<RaycastResult> GetEventSystemRaycastResults ()
    {
        PointerEventData eventData = new PointerEventData (EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue ();
        List<RaycastResult> raysastResults = new List<RaycastResult> ();
        EventSystem.current.RaycastAll (eventData, raysastResults);
        return raysastResults;
    }
}