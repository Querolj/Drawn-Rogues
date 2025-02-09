using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (MeshFilter), typeof (MeshRenderer))]
public class Frame : MonoBehaviour
{
    protected int[] _pixelIds;
    public int[] PixelIds
    {
        get
        {
            if (_pixelIds == null)
            {
                Debug.LogWarning (nameof (_pixelIds) + " not yet set, returning null");
                return null;
            }
            return _pixelIds;
        }
        set { _pixelIds = value; } // Used to apply undo history
    }

    protected int[] _pixelUsages;
    public int[] PixelUsages
    {
        get
        {
            if (_pixelUsages == null)
            {
                Debug.LogWarning (nameof (_pixelUsages) + " not yet set, returning null");
                return null;
            }
            return _pixelUsages;
        }
        set { _pixelUsages = value; } // Used to apply undo history
    }

    protected int[] _pixelTimestamps;
    public int[] PixelTimestamps
    {
        get
        {
            if (_pixelTimestamps == null)
            {
                Debug.LogWarning (nameof (_pixelTimestamps) + " not yet set, returning null");
                return null;
            }
            return _pixelTimestamps;
        }
        set { _pixelTimestamps = value; } // Used to apply undo history
    }

    private int _currentPixelTimestamp = 1;

    protected Material _mat;
    private ComputeShader _drawOnFrameCs;
    private const string KERNEL_DRAW_TEX_CHARACTER = "DrawTexCharacter";
    private const string KERNEL_DRAW_TEX_SPELL = "DrawTexSpell";

    private ComputeShader _countAvailablePixelsCs;

    protected Action<List<BaseColorDrops>, int> _onPixelsAdded;
    public void SetOnPixelsAdded (Action<List<BaseColorDrops>, int> onPixelsAdded)
    {
        _onPixelsAdded += onPixelsAdded;
    }

    protected int _width;
    protected int _height;

    public int Width
    {
        get { return _width; }
    }

    public int Height
    {
        get { return _height; }
    }

    protected Texture2D _drawTexture;
    public Texture2D DrawTexture
    {
        get
        {
            return _drawTexture;
        }
    }
    public Texture2D MainTexture
    {
        get
        {
            return (Texture2D) _renderer.material.mainTexture;
        }
    }

    protected Color[] _clearPixels;
    protected Color[] _mainTexPixels;

    protected int _currentPixelsAllowed = -1;
    public int CurrentPixelsAllowed
    {
        get { return _currentPixelsAllowed; }
        set { _currentPixelsAllowed = value; } // Used to apply undo history
    }

    [SerializeField]
    protected int _maxPixelsAllowed = -1;

    [SerializeField]
    private bool _disallowDrawOnTransparency = true;

    [SerializeField]
    private Vector2Int _dimension = new Vector2Int (64, 64);

    protected MeshRenderer _renderer;

    public int MaxPixelsAllowed
    {
        get { return _maxPixelsAllowed; }
        set { _maxPixelsAllowed = value; }
    }

    public void ResetPixelAllowed (int value)
    {
        _currentPixelsAllowed = value;
        _maxPixelsAllowed = value;
    }

    protected virtual void Awake ()
    {
        _drawOnFrameCs = Resources.Load<ComputeShader> ("DrawOnFrame");
        if (_drawOnFrameCs == null)
        {
            Debug.LogError (nameof (_drawOnFrameCs) + "null, it was not loaded (not found?)");
            return;
        }

        _countAvailablePixelsCs = Resources.Load<ComputeShader> ("CountDrawablePixelsUnderBrush");
        if (_countAvailablePixelsCs == null)
        {
            Debug.LogError (nameof (_countAvailablePixelsCs) + "null, it was not loaded (not found?)");
            return;
        }

        if (_maxPixelsAllowed != -1)
        {
            ResetPixelAllowed (_maxPixelsAllowed);
        }

        _renderer = GetComponent<MeshRenderer> ();
        _mat = GetComponent<MeshRenderer> ().material;
        _mat.SetFloat ("_Width", _dimension.x);
        _mat.SetFloat ("_Height", _dimension.y);

        _drawTexture = GraphicUtils.GetUniqueTransparentTex (_dimension);

        _width = _dimension.x;
        _height = _dimension.y;
        _pixelIds = new int[_width * _height];
        _pixelUsages = new int[_width * _height];
        _pixelTimestamps = new int[Width * Height];

        _clearPixels = new Color[_width * _height];
        if (_disallowDrawOnTransparency && _renderer.material.mainTexture != null)
            _mainTexPixels = ((Texture2D) _renderer.material.mainTexture).GetPixels ();

        _mat.SetTexture ("_DrawTex", _drawTexture);
    }

    public FrameInfos GetTextureInfos ()
    {
        return new FrameInfos (_pixelIds, _pixelUsages, _pixelTimestamps, _currentPixelsAllowed, _maxPixelsAllowed);
    }

    public virtual void UpdateBrushDrawingPrediction (Vector2 uv)
    {
        _mat.SetInt ("_MousePosX", (int) (uv.x * _width));
        _mat.SetInt ("_MousePosY", (int) (uv.y * _height));
    }

    public void StopBrushDrawingPrediction ()
    {
        _mat.SetInt ("_BrushWidth", -1);
        _mat.SetInt ("_BrushHeight", -1);
    }

    public void SetBrush (Brush brush, Texture2D colorTex = null)
    {
        if (brush == null)
            throw new ArgumentNullException (nameof (brush));

        (int width, int height) = brush.GetDimensions ();

        if (colorTex != null)
        {
            _mat.SetTexture ("_ColorTex", colorTex);
            _mat.SetInt ("_ColorTexWidth", colorTex.width);
            _mat.SetInt ("_ColorTexHeight", colorTex.height);
            _mat.SetInt ("_BrushWidth", width);
            _mat.SetInt ("_BrushHeight", height);
            _mat.SetTexture ("_BrushTex", brush.Texture);
        }

        _drawOnFrameCs.SetInt ("BrushWidth", width);
        _drawOnFrameCs.SetInt ("BrushHeight", height);

        int kernel = _drawOnFrameCs.FindKernel (KERNEL_DRAW_TEX_CHARACTER);
        _drawOnFrameCs.SetTexture (kernel, "BrushTex", brush.Texture);
        kernel = _drawOnFrameCs.FindKernel (KERNEL_DRAW_TEX_SPELL);
        _drawOnFrameCs.SetTexture (kernel, "BrushTex", brush.Texture);
    }

    public void ClearMetadata ()
    {
        _pixelIds = new int[Width * Height];
        _pixelUsages = new int[Width * Height];
        _pixelTimestamps = new int[Width * Height];
    }

    public void ClearDrawTexture ()
    {
        _drawTexture.SetPixels (0, 0, Width, Height, _clearPixels, 0);
        _drawTexture.Apply ();
    }

    protected virtual Vector2Int GetMousePosFrameSpace (Vector2 uv)
    {
        return new Vector2Int ((int) (uv.x * _width), (int) (uv.y * _height));
    }

    private StrokeInfo _currentStrokeInfo = null;
    public StrokeInfo LastStrokeInfo
    {
        get { return _currentStrokeInfo; }
    }

    private Vector2Int _previousMousePosFrameSpace = Vector2Int.zero;

    // return false if can't draw on the given position on the frame
    public bool TryDraw (Vector2 coordinate, int colouringId, Texture2D texture, List<BaseColorDrops> baseColorDrops,
        PixelUsage pixelUsage, bool isNewStroke, ResizableBrush resizableBrush, int maxDrawablePixCount, Drawer.DrawMode drawMode,
        out StrokeInfo strokeInfo, out int totalPixelDraw)
    {
        totalPixelDraw = 0;
        if (resizableBrush == null)
            throw new ArgumentNullException (nameof (resizableBrush));

        if (isNewStroke || _currentStrokeInfo == null)
        {
            _previousMousePosFrameSpace = Vector2Int.zero;
            _currentPixelTimestamp++;
            _currentStrokeInfo = new StrokeInfo (pixelUsage, _currentPixelTimestamp, this);
        }

        strokeInfo = _currentStrokeInfo;

        // check if we draw on transparency
        if (_disallowDrawOnTransparency)
        {
            int x1 = (int) (coordinate.x * _width);
            int y1 = (int) (coordinate.y * _height);
            int index = y1 * _width + x1;

            if (_pixelTimestamps[index] != 0 && _pixelTimestamps[index] != _currentPixelTimestamp)
            {
                return false;
            }

            if (_mainTexPixels[index].a == 0)
            {
                return false;
            }
        }

        maxDrawablePixCount = Mathf.Min (_currentPixelsAllowed, maxDrawablePixCount);
        if (maxDrawablePixCount == 0)
            return false;

        int initialAvailablePixels = maxDrawablePixCount;

        // Set brush
        SetBrush (resizableBrush.ActiveBrush);

        ComputeShader cs = _drawOnFrameCs;
        int kernel;
        if (drawMode == Drawer.DrawMode.Character)
            kernel = cs.FindKernel (KERNEL_DRAW_TEX_CHARACTER);
        else
            kernel = cs.FindKernel (KERNEL_DRAW_TEX_SPELL);

        // Set Frame tex
        RenderTexture rendTex = new RenderTexture (_width, _height, 0, RenderTextureFormat.ARGB32);
        rendTex.enableRandomWrite = true;
        Graphics.Blit (_drawTexture, rendTex);

        cs.SetTexture (kernel, "FrameTex", rendTex);

        // Set Tex Dim
        cs.SetInt ("FrameWidth", _width);
        cs.SetInt ("FrameHeight", _height);

        // Set pixels ids buffer
        ComputeBuffer pixelIdsBuffer = new ComputeBuffer (_pixelIds.Length, sizeof (int));
        pixelIdsBuffer.SetData (_pixelIds);
        cs.SetBuffer (kernel, "PixelIds", pixelIdsBuffer);

        // Set pixels usages buffer
        ComputeBuffer pixelUsagesBuffer = new ComputeBuffer (_pixelUsages.Length, sizeof (int));
        pixelUsagesBuffer.SetData (_pixelUsages);
        cs.SetBuffer (kernel, "PixelUsages", pixelUsagesBuffer);
        cs.SetInt ("PixelUsage", (int) pixelUsage);

        // Set pixel id
        cs.SetInt ("PixelId", colouringId);

        // Set pixel timestamps
        ComputeBuffer pixelTimestampsBuffer = new ComputeBuffer (_pixelTimestamps.Length, sizeof (int));
        pixelTimestampsBuffer.SetData (_pixelTimestamps);
        cs.SetBuffer (kernel, "PixelTimestamps", pixelTimestampsBuffer);

        cs.SetInt ("PixelTimestamp", _currentPixelTimestamp);

        // Set down border touched buffer
        ComputeBuffer downBorderTouchedBuffer = new ComputeBuffer (1, sizeof (int));
        downBorderTouchedBuffer.SetData (new int[] { strokeInfo.DownBorderTouched ? 1 : 0 });
        cs.SetBuffer (kernel, "DownBorderTouched", downBorderTouchedBuffer);

        // Set Texture to draw
        cs.SetTexture (kernel, "ColorTex", texture);
        cs.SetInt ("ColorTexWidth", texture.width);
        cs.SetInt ("ColorTexHeight", texture.height);

        // Launch CS
        int x = GraphicUtils.GetComputeShaderDispatchCount (_width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (_height, 32);
        int z = 1;

        int[] pixelsAdded = { 0 };
        int colorUsageMaxValue = Utils.GetMaxEnumValue (typeof (PixelUsage));
        int[] colorUsageTouchedInt = new int[colorUsageMaxValue + 1];

        Vector2Int mousePosFrameSpace = GetMousePosFrameSpace (coordinate);
        float leapSize = 0.34f;

        if (_previousMousePosFrameSpace == Vector2Int.zero)
        {
            _previousMousePosFrameSpace = mousePosFrameSpace;
            leapSize = 2f;
        }

        for (float i = 0; i < 1.1f; i += leapSize) // smooth done on the CPU because we would need to count some pixels
        {
            int lerpedMousePosX = (int) Mathf.Lerp ((float) _previousMousePosFrameSpace.x, (float) mousePosFrameSpace.x, i);
            int lerpedMousePosY = (int) Mathf.Lerp ((float) _previousMousePosFrameSpace.y, (float) mousePosFrameSpace.y, i);

            int freePixelCount = CountDrawablePixelsUnderBrush (new Vector2Int (lerpedMousePosX, lerpedMousePosY), resizableBrush.ActiveBrush);

            // Set a smaller brush if not enough pixels available
            if (freePixelCount > maxDrawablePixCount)
            {
                for (int j = resizableBrush.Brushes.Length - 1; j >= 0; j--)
                {
                    if (resizableBrush.HasSmallestBrushActive ()) // no smaller brush available
                    {
                        ReleaseBuffers (pixelIdsBuffer, pixelUsagesBuffer, pixelTimestampsBuffer, downBorderTouchedBuffer);
                        _currentPixelsAllowed -= Mathf.Clamp (initialAvailablePixels - maxDrawablePixCount, 0, _maxPixelsAllowed);
                        return true;
                    }
                    
                    resizableBrush.SetBiggestBrushPossible (maxDrawablePixCount);

                    freePixelCount = CountDrawablePixelsUnderBrush (new Vector2Int (lerpedMousePosX, lerpedMousePosY), resizableBrush.ActiveBrush);
                    if (freePixelCount <= maxDrawablePixCount)
                    {
                        SetBrush (resizableBrush.ActiveBrush);
                        break;
                    }
                }

                if (freePixelCount > maxDrawablePixCount)
                {
                    ReleaseBuffers (pixelIdsBuffer, pixelUsagesBuffer, pixelTimestampsBuffer, downBorderTouchedBuffer);
                    _currentPixelsAllowed -= Mathf.Clamp (initialAvailablePixels - maxDrawablePixCount, 0, _maxPixelsAllowed);
                    return true;
                }
            }

            cs.SetInt ("MousePosX", lerpedMousePosX);
            cs.SetInt ("MousePosY", lerpedMousePosY);

            // Set color usage touched
            ComputeBuffer colorsUsagesTouchedBuffer = new ComputeBuffer (colorUsageMaxValue + 1, sizeof (int));
            colorsUsagesTouchedBuffer.SetData (colorUsageTouchedInt);
            cs.SetBuffer (kernel, "PixelUsagesTouched", colorsUsagesTouchedBuffer);

            // pixels added (new pixels count)
            ComputeBuffer pixelsAddedBuffer = new ComputeBuffer (1, sizeof (int));
            pixelsAddedBuffer.SetData (pixelsAdded);
            cs.SetBuffer (kernel, "PixelsAdded", pixelsAddedBuffer);
            cs.Dispatch (kernel, x, y, z);

            // get pixels added
            pixelsAddedBuffer.GetData (pixelsAdded);
            totalPixelDraw += pixelsAdded[0];
            maxDrawablePixCount -= pixelsAdded[0];
            pixelsAdded[0] = 0;

            colorsUsagesTouchedBuffer.GetData (colorUsageTouchedInt);

            // Release
            colorsUsagesTouchedBuffer.Release ();
            pixelsAddedBuffer.Release ();

            if (maxDrawablePixCount <= 0)
                break;
        }

        _currentPixelsAllowed -= Mathf.Clamp (initialAvailablePixels - maxDrawablePixCount, 0, _maxPixelsAllowed);

        // Get datas
        pixelIdsBuffer.GetData (_pixelIds);
        pixelUsagesBuffer.GetData (_pixelUsages);
        pixelTimestampsBuffer.GetData (_pixelTimestamps);
        int[] downBorderTouched = new int[1];
        if (!strokeInfo.DownBorderTouched)
        {
            downBorderTouchedBuffer.GetData (downBorderTouched);
            strokeInfo.DownBorderTouched = downBorderTouched[0] > 0;
        }

        _currentStrokeInfo.AddPixels (totalPixelDraw);
        _onPixelsAdded?.Invoke (baseColorDrops, totalPixelDraw);

        for (int i = 1; i < colorUsageTouchedInt.Length; i++)
        {
            if (colorUsageTouchedInt[i] > 0 && !strokeInfo.PixelUsagesTouched.Contains ((PixelUsage) i))
            {
                strokeInfo.PixelUsagesTouched.Add ((PixelUsage) i);
            }
        }

        // Releases
        ReleaseBuffers (pixelIdsBuffer, pixelUsagesBuffer, pixelTimestampsBuffer, downBorderTouchedBuffer);

        RenderTexture.active = rendTex;
        _drawTexture.ReadPixels (new Rect (0, 0, _width, _height), 0, 0);
        _drawTexture.Apply ();

        _previousMousePosFrameSpace = mousePosFrameSpace;

        void ReleaseBuffers (ComputeBuffer pixelIdsBuffer, ComputeBuffer pixelUsagesBuffer, ComputeBuffer pixelTimestampsBuffer, ComputeBuffer downBorderTouchedBuffer)
        {
            pixelIdsBuffer.Release ();
            pixelUsagesBuffer.Release ();
            pixelTimestampsBuffer.Release ();
            downBorderTouchedBuffer.Release ();
        }

        return true;
    }

    private int CountDrawablePixelsUnderBrush (Vector2Int mouseCoordinate, Brush brush)
    {
        ComputeShader cs = _countAvailablePixelsCs;

        int kernel = cs.FindKernel ("CountDrawablePixelsUnderBrush");
        cs.SetInt ("MousePosX", mouseCoordinate.x);
        cs.SetInt ("MousePosY", mouseCoordinate.y);

        cs.SetInt ("FrameWidth", _width);
        cs.SetInt ("FrameHeight", _height);

        cs.SetInt ("BrushWidth", brush.Texture.width);
        cs.SetInt ("BrushHeight", brush.Texture.height);

        ComputeBuffer pixelIdsBuffer = new ComputeBuffer (_pixelIds.Length, sizeof (int));
        pixelIdsBuffer.SetData (_pixelIds);
        cs.SetBuffer (kernel, "PixelIds", pixelIdsBuffer);

        ComputeBuffer pixelCountBuffer = new ComputeBuffer (1, sizeof (int));
        pixelCountBuffer.SetData (new int[] { 0 });
        cs.SetBuffer (kernel, "Count", pixelCountBuffer);

        cs.SetTexture (kernel, "BrushTex", brush.Texture);

        int x = GraphicUtils.GetComputeShaderDispatchCount (_width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (_height, 32);
        int z = 1;

        cs.Dispatch (kernel, x, y, z);

        int[] pixelCount = new int[1];
        pixelCountBuffer.GetData (pixelCount);

        pixelIdsBuffer.Release ();
        pixelCountBuffer.Release ();

        return pixelCount[0];
    }

    public void Copy (Frame frame)
    {
        if (frame == null)
            throw new Exception ("frame null, returning");

        if (frame.Width != Width || frame.Height != Height)
            throw new Exception ("frame size different, returning");

        RenderTexture rt = new RenderTexture (frame.Width, frame.Height, 24);
        RenderTexture.active = rt;
        Graphics.Blit (frame.DrawTexture, rt);
        _drawTexture.ReadPixels (new Rect (0, 0, frame.Width, frame.Height), 0, 0);
        _drawTexture.Apply ();

        _pixelIds = frame.PixelIds;
        _pixelUsages = frame.PixelUsages;
        _pixelTimestamps = frame.PixelTimestamps;
        _currentPixelsAllowed = frame.CurrentPixelsAllowed;
        _maxPixelsAllowed = frame.MaxPixelsAllowed;
    }
}