using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class Drawer : MonoBehaviour
{
    private (Frame, Vector2) _coordinateByFocusedFrame;

    private Colouring _selectedColouring;
    public void SetSelectedColouring (Colouring colouring)
    {
        if (colouring == null)
            throw new ArgumentNullException (nameof (colouring));

        _selectedColouring = colouring;
        if (_selectedColouring.HasBrushSize)
            _resizableBrush.SetBrushSize (_selectedColouring.BrushSize);

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
    private bool _stopDraw = false;

    private bool _activateDrawer = false;
    #endregion

    private ResizableBrush _resizableBrush;
    private BaseColorInventory _baseColorInventory;

    [Inject, UsedImplicitly]
    private void Init (ModeSwitcher modeSwitcher, ResizableBrush resizableBrush, BaseColorInventory baseColorInventory)
    {
        _baseColorInventory = baseColorInventory;
        _resizableBrush = resizableBrush;
        modeSwitcher.OnChangeMode += (mode) =>
        {
            Activate (mode == ModeSwitcher.Mode.Draw);
        };
    }

    private void Awake ()
    {
        _mainCamera = Camera.main;
    }

    private void Start ()
    {
        OnDrawStrokeStart += ResetStrokeValidation;
        OnDrawStrokeEnd += (c, si) =>
        {
            if (_coordinateByFocusedFrame.Item1 != null)
                CommitToUndoStack ();
        };

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

    private bool _leftMouseJustPressed = false;
    void Update ()
    {
        if (_selectedColouring == null || !_activateDrawer)
            return;

        if (Input.GetKeyDown (KeyCode.Mouse0))
        {
            _leftMouseJustPressed = true;
        }
        else if (_coordinateByFocusedFrame.Item1 != null)
        {
            _leftMouseJustPressed = false;
        }

        SetFocusedFrame (Input.mousePosition);

        if (!disalowDrawUntilNewStroke && Input.GetKey (KeyCode.Mouse0) && Vector2.Distance (_lastDrawedPosition, Input.mousePosition) > _DISTANCE_TO_REACH_UNTIL_DRAW_IN_PIXEL)
        {
            if (_coordinateByFocusedFrame.Item1 != null)
            {
                DrawOnFocusedFrame (_leftMouseJustPressed);
                _lastDrawedPosition = Input.mousePosition;
            }
        }

        if (Input.GetKeyUp (KeyCode.Mouse0) || _stopDraw)
        {
            _stopDraw = false;
            _leftMouseJustPressed = false;
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
            _resizableBrush.ChangeBrushFromIndexOffset (Input.mouseScrollDelta.y < 0 ? -1 : 1);
        }

        ExecuteActionForFrame ((frame, uv) =>
        {
            frame.UpdateBrushDrawingPrediction (uv);
        });
    }

    private void SetFocusedFrame (Vector3 screenPos)
    {
        _coordinateByFocusedFrame.Item1 = null;

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
        Vector2 brushExtents = _resizableBrush.ActiveBrush.GetExtents ();
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

        if (_coordinateByFocusedFrame.Item1 != null && !_initedFrames.Contains (_coordinateByFocusedFrame.Item1))
            InitFrame (_coordinateByFocusedFrame.Item1);
    }

    private Dictionary<int, Frame> _framesTouchedCache = new Dictionary<int, Frame> ();
    private bool RayOnFrame (Vector3 position, Vector3 direction)
    {
        int layer = 1 << LayerMask.NameToLayer ("Frame3D");

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

            _coordinateByFocusedFrame.Item1 = frame;
            _coordinateByFocusedFrame.Item2 = hit.textureCoord;
        }
        else
        {
            _coordinateByFocusedFrame.Item1 = null;
            _coordinateByFocusedFrame.Item2 = Vector2.zero;
        }

        return touched;
    }

    private void ChangeBrushFromAvailableColorQuantity ()
    {
        if (_selectedColouring == null)
            return;

        int maxDrawablePix = _baseColorInventory.GetMaxDrawablePixelsFromColouring (_selectedColouring.BaseColorsUsedPerPixel);
        if (_resizableBrush.ActiveBrush.GetOpaquePixelsCount () <= maxDrawablePix)
            return;

        _resizableBrush.SetBiggestBrushPossible (maxDrawablePix);
    }

    private void DrawOnFocusedFrame (bool drawStrokeJustStarting)
    {
        if (_coordinateByFocusedFrame.Item1 == null)
            throw new ArgumentNullException (nameof (_coordinateByFocusedFrame));

        if (!AllowStartDraw ())
            return;

        ChangeBrushFromAvailableColorQuantity ();
        int maxDrawablePixCount = _baseColorInventory.GetMaxDrawablePixelsFromColouring (_selectedColouring.BaseColorsUsedPerPixel);
        // check pixel left on the char 

        if (drawStrokeJustStarting)
        {
            Debug.Log ("Draw stroke just starting");
            _lastStrokeDrawUVs.Clear ();
        }

        ExecuteActionForFrame ((frame, uv) =>
        {
            bool drawDone = frame.TryDraw (uv, _selectedColouring.Id, _selectedColouring.Texture, _selectedColouring.BaseColorsUsedPerPixel,
                _bodyPartSelection.GetPixelUsageFromSelectedBodyPart (), drawStrokeJustStarting, _resizableBrush, maxDrawablePixCount,
                out _currentStrokeInfo, out int pixelsAdded);

            if (!drawDone)
            {
                StopCurrentDrawing ();
                return;
            }

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

        bool drawedSomething = maxDrawablePixCount != _baseColorInventory.GetMaxDrawablePixelsFromColouring (_selectedColouring.BaseColorsUsedPerPixel);

        if (drawStrokeJustStarting && (_strokeValidation == null || _strokeValidation.IsValid) && drawedSomething)
        {
            PushToUndoStack ();
        }
    }

    private bool AllowStartDraw ()
    {
        if (_baseColorInventory.GetMaxDrawablePixelsFromColouring (_selectedColouring.BaseColorsUsedPerPixel) <= 0)
            return false;
        return true;
    }

    private void ExecuteActionForFrame (System.Action<Frame, Vector2> drawAction, bool drawStrokeJustStarting = false)
    {
        if (drawAction == null)
            throw new ArgumentNullException ((nameof (drawAction)));

        if (_coordinateByFocusedFrame.Item1 == null)
            return;

        // Add frame pixels to the undo history
        // List < (Frame, Color[]) > framePixels = null;
        if (drawStrokeJustStarting)
        {
            // framePixels = new List < (Frame, Color[]) > ();

            // foreach (Frame frame in _coordinateByFocusedFrame.Keys)
            // {
            //     framePixels.Add ((frame, frame.DrawTexture.GetPixels ()));
            // }

            OnDrawStrokeStart.Invoke ();
        }

        // Launch the actions
        drawAction.Invoke (_coordinateByFocusedFrame.Item1, _coordinateByFocusedFrame.Item2);
    }

    private void InitFrame (Frame frame)
    {
        frame.SetBrush (_resizableBrush.ActiveBrush);
        frame.SetOnPixelsAdded (OnPixelsAdded);
        _initedFrames.Add (frame);
    }

    private void OnPixelsAdded (List<BaseColorDrops> baseColorDrops, int pixelsAdded)
    {
        _baseColorInventory.RemoveBaseColorDrops (baseColorDrops, pixelsAdded);
    }

    public void Activate (bool activate)
    {
        _activateDrawer = activate;
        if (!_activateDrawer)
        {
            if (_coordinateByFocusedFrame.Item1 != null)
                _coordinateByFocusedFrame.Item1.StopBrushDrawingPrediction ();
        }
    }

    public void StopBrushDrawingPrediction ()
    {
        ExecuteActionForFrame ((frame, uv) =>
        {
            frame.StopBrushDrawingPrediction ();
        });
    }

    public void StopCurrentDrawing ()
    {
        if (!_drawStartedWithBrush)
            return;

        disalowDrawUntilNewStroke = true;
        _drawStartedWithBrush = false;
        OnDrawStrokeEnd?.Invoke (_selectedColouring, _currentStrokeInfo);
    }

    #region Undo & stroke reset
    private void ResetCurrentStroke ()
    {
        if (_drawerStateToPush != null)
        {
            _drawerStateToPush.Apply (ref _baseColorInventory);
            OnUndo?.Invoke ();
        }
    }

    private void CommitToUndoStack ()
    {
        // Debug.Log ("Commit to undo stack");
        _drawerStateToPush = new DrawerState (_baseColorInventory, _coordinateByFocusedFrame.Item1);
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
        drawerState.Apply (ref _baseColorInventory);
        _drawerStateToPush = null;
        OnUndo?.Invoke ();
    }
    #endregion
}