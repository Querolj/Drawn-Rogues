using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Drawer : MonoBehaviour
{

    [SerializeField]
    private Brush[] _brushes;
    private Brush _activeBrush;
    private int _indexActiveBrush = 0;
    private GameObject[] _brushRenderer;
    private Dictionary<Frame, Vector2> _coordinateByFocusedFrame = new Dictionary<Frame, Vector2> ();

    [SerializeField]
    private BaseColorPalette _palette;

    // [SerializeField]
    // private QuickBookColouring _quickBook;
    private Colouring _selectedColouring;
    public void SetSelectedColouring (Colouring colouring)
    {
        _selectedColouring = colouring;
        if (colouring.HasBrushSize)
            SetBrush (colouring.BrushSize - 1); // -1 because BrushSize is in pixel size, not an index
        Activate (true);
    }

    [SerializeField]
    private BodyPartSelection _bodyPartSelection;

    private Camera _mainCamera;

    private const float _DISTANCE_TO_REACH_UNTIL_DRAW_IN_PIXEL = 1;
    private Vector2 _lastDrawedPosition;
    private HashSet<Frame> _initedFrames = new HashSet<Frame> ();

    public event Action<Colouring, StrokeInfo> OnDrawStrokeEnd;
    private List<Vector2> _lastStrokeDrawUVs = new List<Vector2> ();
    public List<Vector2> LastStrokeDrawUVs
    {
        get
        {
            return _lastStrokeDrawUVs;
        }
    }

    public event Action OnDrawStrokeStart;
    public event Action OnDraw;
    private StrokeInfo _currentStrokeInfo = null;

    private bool _drawStartedWithBrush = false;

    #region undo

    public event Action OnUndo;
    private DrawerState _drawerStateToPush = null;
    private Stack<DrawerState> _drawerStateHistory = new Stack<DrawerState> ();
    #endregion

    #region Current stroke info
    private class StrokeValidation
    {
        private bool _invalidated = false;
        private bool _ruleValidated = false;

        private PixelUsage _pixelUsageToValidate;
        public bool IsValid
        {
            get
            {
                return !_invalidated && _ruleValidated;
            }
        }

        public StrokeValidation (PixelUsage pixelUsage)
        {
            _pixelUsageToValidate = pixelUsage;
        }

        // return false if can't update
        public bool TryUpdate (StrokeInfo strokeInfo)
        {
            if (_invalidated)
                return false;

            if (_pixelUsageToValidate == PixelUsage.Arm)
            {
                foreach (PixelUsage cu in strokeInfo.PixelUsagesTouched)
                {
                    if (cu == PixelUsage.Head || cu == PixelUsage.Leg)
                    {
                        _invalidated = true;
                        return false;
                    }

                    if (cu == PixelUsage.Arm || cu == PixelUsage.Body)
                        _ruleValidated = true;
                }
            }
            else if (_pixelUsageToValidate == PixelUsage.Leg)
            {
                foreach (PixelUsage cu in strokeInfo.PixelUsagesTouched)
                {
                    if (cu == PixelUsage.Head || cu == PixelUsage.Arm)
                    {
                        _invalidated = true;
                        return false;
                    }

                    if (cu == PixelUsage.Leg || (cu == PixelUsage.Body && strokeInfo.DownBorderTouched))
                        _ruleValidated = true;
                }
            }
            return true;
        }
    }
    private StrokeValidation _strokeValidation;
    private bool disalowDrawUntilNewStroke = false;
    private bool _activateDrawer = false;
    #endregion

    private void Awake ()
    {
        Array.Sort (_brushes, (b1, b2) => b1.GetOpaquePixelsCount ().CompareTo (b2.GetOpaquePixelsCount ()));
    }

    private void Start ()
    {
        _mainCamera = Camera.main;
        if (_brushes.Length == 0)
        {
            Debug.LogError ("Need at least on brush");
            return;
        }

        _activeBrush = _brushes[0];
        _brushRenderer = new GameObject[_brushes.Length];

        for (int i = 0; i < _brushes.Length; i++)
        {
            GameObject brushGo = new GameObject (_brushes[i].name);
            brushGo.transform.SetParent (transform);
            SpriteRenderer rend = brushGo.AddComponent<SpriteRenderer> ();
            rend.sprite = Sprite.Create (_brushes[i].Texture, new Rect (0, 0, _brushes[i].Texture.width, _brushes[i].Texture.height), Vector2.one / 2);
            rend.color = new Color (1f, 1f, 1f, 0.3f);
            brushGo.SetActive (false);
            _brushRenderer[i] = brushGo;
        }

        OnDrawStrokeStart += ResetStrokeValidation;
        OnDrawStrokeEnd += CommitToUndoStack;

        ResetStrokeValidation ();

    }

    private void ResetStrokeValidation ()
    {
        PixelUsage currentPixUsage = _bodyPartSelection.GetPixelUsageFromSelectedBodyPart ();

        if (currentPixUsage == PixelUsage.Arm || currentPixUsage == PixelUsage.Leg)
        {
            _strokeValidation = new StrokeValidation (currentPixUsage);
        }
        else
        {
            _strokeValidation = null;
        }
    }

    void Update ()
    {
        if (_selectedColouring == null || !_activateDrawer)
            return;

        bool leftMouseDown = Input.GetKeyDown (KeyCode.Mouse0);

        SetFocusedFrame (Input.mousePosition);
        if (!disalowDrawUntilNewStroke && Input.GetKey (KeyCode.Mouse0) && Vector2.Distance (_lastDrawedPosition, Input.mousePosition) > _DISTANCE_TO_REACH_UNTIL_DRAW_IN_PIXEL)
        {
            if (_coordinateByFocusedFrame.Count > 0)
            {
                DrawOnFocusedFrames (leftMouseDown);
                _lastDrawedPosition = Input.mousePosition;
            }
        }

        if (Input.GetKeyUp (KeyCode.Mouse0))
        {
            if (_drawStartedWithBrush)
            {
                if (_strokeValidation != null && !_strokeValidation.IsValid)
                {
                    ResetCurrentStroke ();
                }
                else
                {
                    OnDrawStrokeEnd?.Invoke (_selectedColouring, _currentStrokeInfo);
                }
                _drawStartedWithBrush = false;
            }

            if (disalowDrawUntilNewStroke)
                disalowDrawUntilNewStroke = false;
        }

        if (!_selectedColouring.HasBrushSize && Input.mouseScrollDelta.y != 0)
        {
            ScrollBrush (Input.mouseScrollDelta.y < 0 ? -1 : 1);
        }

        DoForAllFrames ((frame, uv) =>
        {
            frame.UpdateBrushDrawingPrediction (uv);
        });
    }

    private Dictionary<int, Frame> _frameByInstancesFound = new Dictionary<int, Frame> ();
    private void SetFocusedFrame (Vector3 screenPos)
    {
        _coordinateByFocusedFrame.Clear ();

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
        RayOnFrame (_mainCamera.transform.position, direction);
        // Debug.DrawRay (_mainCamera.transform.position, direction * 100, Color.red, 10f, true);

        // ray on brush corners
        Vector2 brushExtents = _activeBrush.GetExtents ();
        Vector3 bottomLeft = _mainCamera.transform.position;
        bottomLeft += -_mainCamera.transform.right * brushExtents.x;
        bottomLeft += -_mainCamera.transform.up * brushExtents.y;
        RayOnFrame (_mainCamera.transform.position, direction);
        // Debug.DrawRay (bottomLeft, direction * 100, Color.red, 10f, true);

        Vector3 bottomRight = _mainCamera.transform.position;
        bottomRight += _mainCamera.transform.right * brushExtents.x;
        bottomRight += -_mainCamera.transform.up * brushExtents.y;
        RayOnFrame (_mainCamera.transform.position, direction);
        // Debug.DrawRay (bottomRight, direction * 100, Color.red, 10f, true);

        Vector3 topLeft = _mainCamera.transform.position;
        topLeft += -_mainCamera.transform.right * brushExtents.x;
        topLeft += _mainCamera.transform.up * brushExtents.y;
        RayOnFrame (_mainCamera.transform.position, direction);
        // Debug.DrawRay (topLeft, direction * 100, Color.red, 10f, true);

        Vector3 topRight = _mainCamera.transform.position;
        topRight += _mainCamera.transform.right * brushExtents.x;
        topRight += _mainCamera.transform.up * brushExtents.y;
        RayOnFrame (_mainCamera.transform.position, direction);
        // Debug.DrawRay (topRight, direction * 100, Color.red, 10f);

        InitFocusedFrames ();
    }

    private void InitFocusedFrames ()
    {
        foreach (Frame frame in _coordinateByFocusedFrame.Keys)
        {
            if (!_initedFrames.Contains (frame))
            {
                InitFrame (frame);
            }
        }
    }

    private Dictionary<int, Frame> _framesTouchedCache = new Dictionary<int, Frame> ();
    private bool RayOnFrame (Vector3 position, Vector3 direction)
    {
        int layer;
        // if (_drawContext == DrawContext.Map)
        //     layer = 1 << LayerMask.NameToLayer ("Map");
        // else
        layer = 1 << LayerMask.NameToLayer ("Frame3D");

        bool touched = Physics.Raycast (position, direction, out RaycastHit hit, 100f, layer);
        if (touched)
        {
            Frame frame;

            int id = hit.transform.GetInstanceID ();
            if (!_framesTouchedCache.ContainsKey (id))
            {
                frame = hit.transform.GetComponent<Frame> ();
                if (frame == null)
                    throw new Exception ("Frame not found on " + hit.transform.name);
                _framesTouchedCache.Add (hit.transform.GetInstanceID (), hit.transform.GetComponent<Frame> ());
            }
            else
                frame = _framesTouchedCache[id];

            if (!_coordinateByFocusedFrame.ContainsKey (frame))
                _coordinateByFocusedFrame.Add (frame, hit.textureCoord);
        }

        return touched;
    }

    private void UpdateBrushFromAvailableColorQuantity ()
    {
        if (_selectedColouring == null)
            return;

        int maxDrawablePix = _palette.GetMaxDrawablePixelsFromColouring (_selectedColouring);
        if (_activeBrush.GetOpaquePixelsCount () <= maxDrawablePix)
            return;

        for (int i = _brushes.Length - 1; i >= 0; i--)
        {
            if (_brushes[i].GetOpaquePixelsCount () <= maxDrawablePix)
            {
                SetBrush (i);
                return;
            }
        }
    }

    private void ScrollBrush (int indexOffset)
    {
        _indexActiveBrush = Mathf.Clamp ((_indexActiveBrush + indexOffset) % _brushes.Length, 0, _brushes.Length - 1);
        _activeBrush = _brushes[_indexActiveBrush];
        foreach (Frame frame in _coordinateByFocusedFrame.Keys)
        {
            frame.SetBrushDrawingPrediction (_activeBrush, _selectedColouring.Texture);
        }
    }

    private void SetBrush (int index)
    {
        if (index < 0 || index >= _brushes.Length)
            throw new Exception ("Brush index out of bound : " + index);

        _indexActiveBrush = index;
        _activeBrush = _brushes[_indexActiveBrush];

        if (_selectedColouring != null)
        {
            foreach (Frame frame in _coordinateByFocusedFrame.Keys)
            {
                frame.SetBrushDrawingPrediction (_activeBrush, _selectedColouring.Texture);
            }
        }

    }

    private void DrawOnFocusedFrames (bool drawStrokeJustStarting)
    {
        if (_coordinateByFocusedFrame == null)
            throw new ArgumentNullException (nameof (_coordinateByFocusedFrame));

        if (!AllowStartDraw ())
            return;

        UpdateBrushFromAvailableColorQuantity ();
        int maxDrawablePixCount = _palette.GetMaxDrawablePixelsFromColouring (_selectedColouring);
        // check pixel left on the char 

        if (drawStrokeJustStarting)
            _lastStrokeDrawUVs.Clear ();

        DoForAllFrames ((frame, uv) =>
        {
            int pixelsAdded = frame.Draw (uv, _selectedColouring, _bodyPartSelection.GetPixelUsageFromSelectedBodyPart (), drawStrokeJustStarting,
                _brushes, _indexActiveBrush, maxDrawablePixCount, out _currentStrokeInfo);

            if (pixelsAdded <= 0)
                return;

            if (_strokeValidation != null && !_strokeValidation.TryUpdate (_currentStrokeInfo))
            {
                disalowDrawUntilNewStroke = true;
                ResetCurrentStroke ();
                ResetStrokeValidation ();
            }
            else
            {
                _lastStrokeDrawUVs.Add (uv);
                _drawStartedWithBrush = true;
                OnDraw?.Invoke ();
            }

        }, drawStrokeJustStarting);

        bool drawedSomething = maxDrawablePixCount != _palette.GetMaxDrawablePixelsFromColouring (_selectedColouring);

        if (drawStrokeJustStarting && (_strokeValidation == null || _strokeValidation.IsValid) && drawedSomething)
        {
            PushToUndoStack ();
        }
    }

    private bool AllowStartDraw ()
    {
        if (_palette.GetMaxDrawablePixelsFromColouring (_selectedColouring) <= 0)
            return false;
        return true;
    }

    private void DoForAllFrames (System.Action<Frame, Vector2> drawAction, bool drawStrokeJustStarting = false)
    {
        if (drawAction == null)
            throw new ArgumentNullException ((nameof (drawAction)));

        if (_coordinateByFocusedFrame == null || _coordinateByFocusedFrame.Count == 0)
            return;

        // Add frame pixels to the undo history
        List < (Frame, Color[]) > framePixels = null;
        if (drawStrokeJustStarting)
        {
            framePixels = new List < (Frame, Color[]) > ();
            foreach (Frame frame in _coordinateByFocusedFrame.Keys)
            {
                framePixels.Add ((frame, frame.DrawTexture.GetPixels ()));
            }

            OnDrawStrokeStart.Invoke ();
        }

        // Launch the actions
        foreach ((Frame frame, Vector2 uv) in _coordinateByFocusedFrame)
        {
            drawAction.Invoke (frame, uv);
        }

    }

    private void InitFrame (Frame frame)
    {
        frame.SetBrush (_activeBrush);
        frame.SetOnPixelsAdded (OnPixelsAdded);
        _initedFrames.Add (frame);
    }

    private void OnPixelsAdded (Colouring colouring, int pixelsAdded)
    {
        _palette.RemoveBaseColorDrops (colouring, pixelsAdded);
    }

    public void Activate (bool activate)
    {
        _activateDrawer = activate;
        if (!_activateDrawer)
        {
            foreach (Frame frame in _coordinateByFocusedFrame.Keys)
            {
                frame.StopBrushDrawingPrediction ();
            }
        }
        else
        {
            foreach (Frame frame in _coordinateByFocusedFrame.Keys)
            {
                frame.SetBrushDrawingPrediction (_activeBrush, _selectedColouring.Texture);
            }
        }
    }

    public void StopBrushDrawingPrediction ()
    {
        DoForAllFrames ((frame, uv) =>
        {
            frame.StopBrushDrawingPrediction ();
        });
    }

    public bool IsFrameFocused (Frame frame)
    {
        return _coordinateByFocusedFrame.ContainsKey (frame);
    }

    public void CancelCurrentDraw ()
    {
        disalowDrawUntilNewStroke = true;
    }

    #region Undo & stroke reset
    private void ResetCurrentStroke ()
    {
        if (_drawerStateToPush != null)
        {
            _drawerStateToPush.Apply (ref _palette);
            OnUndo?.Invoke ();
        }
    }

    private void CommitToUndoStack (Colouring ci, StrokeInfo currentStrokeInfo)
    {
        // Debug.Log ("Commit to undo stack");
        _drawerStateToPush = new DrawerState (_palette, _coordinateByFocusedFrame.Keys.ToList ());
    }

    private void PushToUndoStack ()
    {
        if (_drawerStateToPush != null)
        {
            _drawerStateHistory.Push (_drawerStateToPush);
        }
        else
        {
            Debug.LogError ("No frame to push to undo stack");
        }
    }

    public void Undo ()
    {
        try
        {
            if (_drawerStateHistory.Peek () == null)
                return;
        }
        catch (InvalidOperationException) { return; }

        DrawerState drawerState = _drawerStateHistory.Pop ();
        List<Frame> frames = _coordinateByFocusedFrame.Keys.ToList ();
        drawerState.Apply (ref _palette);
        _drawerStateToPush = null;
        OnUndo?.Invoke ();
    }
    #endregion
}