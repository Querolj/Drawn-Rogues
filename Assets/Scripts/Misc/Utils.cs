using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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

    private static ComputeShader _calculateSpriteBoundsCs;

    public static Vector4 GetTextureBorder (Texture2D texture)
    {
        _calculateSpriteBoundsCs = Resources.Load<ComputeShader> ("CalculateSpriteRect");
        if (_calculateSpriteBoundsCs == null)
        {
            Debug.LogError (nameof (_calculateSpriteBoundsCs) + "null, it was not loaded (not found?)");
            return Vector4.zero;
        }

        // Update sprite bounds
        int kernel = _calculateSpriteBoundsCs.FindKernel ("CSMain");

        _calculateSpriteBoundsCs.SetTexture (kernel, "Tex", texture);

        ComputeBuffer minXBuf = new ComputeBuffer (1, sizeof (int));
        minXBuf.SetData (new int[] { Int32.MaxValue });
        _calculateSpriteBoundsCs.SetBuffer (kernel, "MinX", minXBuf);

        ComputeBuffer minYBuf = new ComputeBuffer (1, sizeof (int));
        minYBuf.SetData (new int[] { Int32.MaxValue });
        _calculateSpriteBoundsCs.SetBuffer (kernel, "MinY", minYBuf);

        ComputeBuffer maxXBuf = new ComputeBuffer (1, sizeof (int));
        maxXBuf.SetData (new int[] {-1 });
        _calculateSpriteBoundsCs.SetBuffer (kernel, "MaxX", maxXBuf);

        ComputeBuffer maxYBuf = new ComputeBuffer (1, sizeof (int));
        maxYBuf.SetData (new int[] {-1 });
        _calculateSpriteBoundsCs.SetBuffer (kernel, "MaxY", maxYBuf);

        // Run CS
        int x = GraphicUtils.GetComputeShaderDispatchCount (texture.width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (texture.height, 32);
        int z = 1;

        _calculateSpriteBoundsCs.Dispatch (kernel, x, y, z);
        int[] minX = new int[1], minY = new int[1], maxX = new int[1], maxY = new int[1];
        minXBuf.GetData (minX);
        minYBuf.GetData (minY);
        maxXBuf.GetData (maxX);
        maxYBuf.GetData (maxY);

        // Release
        minXBuf.Release ();
        minYBuf.Release ();
        maxXBuf.Release ();
        maxYBuf.Release ();

        return new Vector4 (minX[0], minY[0], maxX[0], maxY[0]);
    }

    public static Bounds CalculateSpriteBoundsInFrame (Frame2D frame, float pixelsPerUnit)
    {
        Vector4 border = Utils.GetTextureBorder (frame.DrawTexture);
        border /= pixelsPerUnit;

        Vector3 boundsSize = new Vector3 (border.z - border.x, border.w - border.y, 0f);
        Vector3 boundsCenter = frame.transform.position - frame.Bounds.extents;
        boundsCenter += new Vector3 (border.x, border.y, 0f);
        boundsCenter += boundsSize / 2f;
        boundsCenter.z = 0f;

        return new Bounds (boundsCenter, boundsSize);
    }

    private static Dictionary<Vector2, Texture2D> _transparentTexByDimension = new Dictionary<Vector2, Texture2D> ();
    public static Texture2D GetUniqueTransparentTex (Vector2Int dimension)
    {
        Texture2D copy;
        if (_transparentTexByDimension.ContainsKey (dimension))
        {
            copy = GraphicUtils.GetTextureCopy (_transparentTexByDimension[dimension]);
            copy.filterMode = FilterMode.Point;

            return copy;
        }

        Texture2D transparentTex = new Texture2D (dimension.x, dimension.y, TextureFormat.ARGB32, false);
        Color[] pixels = new Color[dimension.x * dimension.y];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        transparentTex.SetPixels (pixels);
        transparentTex.Apply ();

        _transparentTexByDimension.Add (dimension, transparentTex);

        copy = GraphicUtils.GetTextureCopy (_transparentTexByDimension[dimension]);
        copy.filterMode = FilterMode.Point;

        return copy;
    }

    private static Camera _mainCamera = null;
    public static bool TryGetMouseToMapPosition (out Vector3 hitPos)
    {
        Vector3 screenPos = Input.mousePosition;
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
        Vector3 screenPos = Input.mousePosition;
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

    private static Dictionary<int, Attackable> _attackablesByInstanceID = new Dictionary<int, Attackable> ();
    public static bool TryGetFirstAttackableUnderMouse (Vector3 screenPos, out Attackable attackable)
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

    public static float GetMapHeight (Vector3 position)
    {
        position.y = 100f;
        if (Physics.Raycast (position, Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
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
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult> ();
        EventSystem.current.RaycastAll (eventData, raysastResults);
        return raysastResults;
    }
}