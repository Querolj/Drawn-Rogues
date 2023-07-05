using System;
using System.Collections.Generic;
using UnityEngine;

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
    private const string KERNEL_DRAW_COLOR = "DrawColor";
    private const string KERNEL_DRAW_TEX = "DrawTex";
    private const string KERNEL_DRAW_BRUSH_TEX = "DrawBrushTex";

    private ComputeShader _drawPathOnFrameCs;
    private ComputeShader _countAvailablePixelsCs;
    private ComputeShader _readTextureInfos = null;

    protected Action<Colouring, int> _onPixelsAdded;
    public void SetOnPixelsAdded (Action<Colouring, int> onPixelsAdded)
    {
        _onPixelsAdded += onPixelsAdded;
    }

    private int _activeBrushId = -1;
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

    protected Color[] _initialPixels;

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

        _drawPathOnFrameCs = Resources.Load<ComputeShader> ("DrawPathOnMap");
        if (_drawPathOnFrameCs == null)
        {
            Debug.LogError (nameof (_drawPathOnFrameCs) + "null, it was not loaded (not found?)");
            return;
        }

        _countAvailablePixelsCs = Resources.Load<ComputeShader> ("CountAvailablePixels");
        if (_countAvailablePixelsCs == null)
        {
            Debug.LogError (nameof (_countAvailablePixelsCs) + "null, it was not loaded (not found?)");
            return;
        }

        _readTextureInfos = Resources.Load<ComputeShader> ("ReadTextureInfos");
        if (_readTextureInfos == null)
        {
            Debug.LogError (nameof (_readTextureInfos) + "null, it was not loaded (not found?)");
            return;
        }

        if (_maxPixelsAllowed != -1)
        {
            ResetPixelAllowed (_maxPixelsAllowed);
        }
    }

    public FrameInfos GetTextureInfos ()
    {
        return new FrameInfos (_pixelIds, _pixelUsages, _pixelTimestamps, _currentPixelsAllowed, _maxPixelsAllowed);
    }

    public void SetBrushDrawingPrediction (Brush brush, Texture2D texture)
    {
        // Update frame material
        _mat.SetInt ("BrushWidth", brush.Texture.width);
        _mat.SetInt ("BrushHeight", brush.Texture.height);

        _mat.SetTexture ("BrushTex", brush.Texture);
        _mat.SetTexture ("Tex", texture);
        _mat.SetInt ("TexWidth", texture.width);
        _mat.SetInt ("TexHeight", texture.height);
    }

    public virtual void UpdateBrushDrawingPrediction (Vector2 coordinate)
    {
        throw new NotImplementedException ();
    }

    public virtual Vector3 FrameSpaceToWorldSpace (Vector2 uv)
    {
        throw new NotImplementedException ();
    }

    public void StopBrushDrawingPrediction ()
    {
        _mat.SetInt ("BrushWidth", -1);
        _mat.SetInt ("BrushHeight", -1);
    }

    public void SetBrush (Brush brush)
    {
        if (brush == null)
            throw new ArgumentNullException (nameof (brush));

        (int width, int height) = brush.GetDimensions ();

        _drawOnFrameCs.SetInt ("BrushWidth", width);
        _drawOnFrameCs.SetInt ("BrushHeight", height);

        int kernel = _drawOnFrameCs.FindKernel (KERNEL_DRAW_COLOR);
        _drawOnFrameCs.SetTexture (kernel, "BrushTex", brush.Texture);
        kernel = _drawOnFrameCs.FindKernel (KERNEL_DRAW_TEX);
        _drawOnFrameCs.SetTexture (kernel, "BrushTex", brush.Texture);

        _activeBrushId = brush.GetInstanceID ();
    }

    public void SetBrush (Texture2D brushTex)
    {
        if (brushTex == null)
            throw new ArgumentNullException (nameof (brushTex));

        int kernel = _drawOnFrameCs.FindKernel (KERNEL_DRAW_BRUSH_TEX);

        _drawOnFrameCs.SetInt ("BrushWidth", brushTex.width);
        _drawOnFrameCs.SetInt ("BrushHeight", brushTex.height);
        RenderTexture rendTex = new RenderTexture (brushTex.width, brushTex.height, 0, RenderTextureFormat.ARGB32);
        rendTex.enableRandomWrite = true;
        Graphics.Blit (brushTex, rendTex);
        _drawOnFrameCs.SetTexture (kernel, "BrushTex", rendTex);
    }

    public void SetBrushForPath (Texture2D borderBrushTex, Texture2D centerBrushTex)
    {
        if (borderBrushTex == null)
            throw new ArgumentNullException (nameof (borderBrushTex));

        if (centerBrushTex == null)
            throw new ArgumentNullException (nameof (centerBrushTex));

        if (borderBrushTex.width != centerBrushTex.width || borderBrushTex.height != centerBrushTex.height)
            throw new ArgumentException ("Border and center brush textures must have the same dimensions");

        int kernel = _drawPathOnFrameCs.FindKernel ("DrawPath");

        _drawPathOnFrameCs.SetInt ("BrushWidth", borderBrushTex.width);
        _drawPathOnFrameCs.SetInt ("BrushHeight", borderBrushTex.height);
        RenderTexture borderRendTex = new RenderTexture (borderBrushTex.width, borderBrushTex.height, 0, RenderTextureFormat.ARGB32);
        borderRendTex.enableRandomWrite = true;
        Graphics.Blit (borderBrushTex, borderRendTex);
        _drawPathOnFrameCs.SetTexture (kernel, "BorderBrushTex", borderRendTex);

        RenderTexture centerRendTex = new RenderTexture (centerBrushTex.width, centerBrushTex.height, 0, RenderTextureFormat.ARGB32);
        centerRendTex.enableRandomWrite = true;
        Graphics.Blit (centerBrushTex, centerRendTex);
        _drawPathOnFrameCs.SetTexture (kernel, "CenterBrushTex", centerRendTex);
    }

    public void Clear ()
    {
        _pixelIds = new int[Width * Height];
        _pixelUsages = new int[Width * Height];
        _pixelTimestamps = new int[Width * Height];

        _drawTexture.SetPixels (0, 0, Width, Height, _initialPixels, 0);
        _drawTexture.Apply ();
    }

    protected virtual Vector2Int GetMousePosFrameSpace (Vector2 coordinate)
    {
        throw new NotImplementedException ();
    }

    private StrokeInfo _currentStrokeInfo = null;
    public StrokeInfo LastStrokeInfo
    {
        get { return _currentStrokeInfo; }
    }

    private Vector2Int _previousMousePosFrameSpace = Vector2Int.zero;

    // return number of pixels draw
    public int Draw (Vector2 coordinate, Colouring colouring, PixelUsage pixelUsage, bool isNewStroke,
        Brush[] brushes, int brushIndex, int availablePixels, out StrokeInfo strokeInfo)
    {
        if (brushes == null || brushes.Length == 0)
            throw new ArgumentNullException (nameof (brushes));

        if (brushIndex < 0 || brushIndex >= brushes.Length)
            throw new ArgumentOutOfRangeException (nameof (brushIndex) + " must be between 0 and " + (brushes.Length - 1));

        if (isNewStroke || _currentStrokeInfo == null)
        {
            _previousMousePosFrameSpace = Vector2Int.zero;
            _currentPixelTimestamp++;
            _currentStrokeInfo = new StrokeInfo (pixelUsage, _currentPixelTimestamp, this);
        }

        strokeInfo = _currentStrokeInfo;

        availablePixels = Mathf.Min (_currentPixelsAllowed, availablePixels);
        if (availablePixels == 0)
            return 0;
        // Debug.Log ("Available pixels: " + availablePixels);
        int initialAvailablePixels = availablePixels;

        // Set brush
        Brush brush = brushes[brushIndex];
        SetBrush (brush);
        _activeBrushId = brush.GetInstanceID ();

        ComputeShader cs = _drawOnFrameCs;
        int kernel = cs.FindKernel (KERNEL_DRAW_TEX);

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
        cs.SetInt ("PixelId", colouring.Id);

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
        cs.SetTexture (kernel, "ColorTex", colouring.Texture);
        cs.SetInt ("ColorTexWidth", colouring.Texture.width);
        cs.SetInt ("ColorTexHeight", colouring.Texture.height);

        // Launch CS
        int x = GraphicUtils.GetComputeShaderDispatchCount (_width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (_height, 32);
        int z = 1;

        int[] pixelsAdded = { 0 };
        int colorUsageMaxValue = Utils.GetMaxEnumValue (typeof (PixelUsage));
        int[] colorUsageTouchedInt = new int[colorUsageMaxValue + 1];

        Vector2Int mousePosFrameSpace = GetMousePosFrameSpace (coordinate);
        float leapSize = 0.34f;
        int pixelAddedSum = 0;

        if (_previousMousePosFrameSpace == Vector2Int.zero)
        {
            _previousMousePosFrameSpace = mousePosFrameSpace;
            leapSize = 2f;
        }

        for (float i = 0; i < 1.1f; i += leapSize) // smooth done on the CPU because we would need to count some pixels
        {
            int lerpedMousePosX = (int) Mathf.Lerp ((float) _previousMousePosFrameSpace.x, (float) mousePosFrameSpace.x, i);
            int lerpedMousePosY = (int) Mathf.Lerp ((float) _previousMousePosFrameSpace.y, (float) mousePosFrameSpace.y, i);

            int freePixelCount = CountAvailablePixels (new Vector2Int (lerpedMousePosX, lerpedMousePosY), brush);
            // Need to set a smaller brush if not enough pixels available
            if (freePixelCount > availablePixels)
            {
                if (brushIndex == 0)
                {
                    ReleaseBuffers (pixelIdsBuffer, pixelUsagesBuffer, pixelTimestampsBuffer, downBorderTouchedBuffer);
                    _currentPixelsAllowed -= Mathf.Clamp (initialAvailablePixels - availablePixels, 0, _maxPixelsAllowed);
                    return pixelAddedSum;
                }

                for (int j = brushIndex - 1; j >= 0; j--)
                {
                    brush = brushes[j];
                    freePixelCount = CountAvailablePixels (new Vector2Int (lerpedMousePosX, lerpedMousePosY), brush);
                    if (freePixelCount <= availablePixels)
                    {
                        brushIndex = j;
                        SetBrush (brush);
                        _activeBrushId = brush.GetInstanceID ();
                        break;
                    }
                }

                if (freePixelCount > availablePixels)
                {
                    ReleaseBuffers (pixelIdsBuffer, pixelUsagesBuffer, pixelTimestampsBuffer, downBorderTouchedBuffer);
                    _currentPixelsAllowed -= Mathf.Clamp (initialAvailablePixels - availablePixels, 0, _maxPixelsAllowed);
                    return pixelAddedSum;
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
            pixelAddedSum += pixelsAdded[0];
            availablePixels -= pixelsAdded[0];
            pixelsAdded[0] = 0;

            colorsUsagesTouchedBuffer.GetData (colorUsageTouchedInt);

            // Release
            colorsUsagesTouchedBuffer.Release ();
            pixelsAddedBuffer.Release ();

            if (availablePixels <= 0)
                break;
        }

        _currentPixelsAllowed -= Mathf.Clamp (initialAvailablePixels - availablePixels, 0, _maxPixelsAllowed);

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

        _currentStrokeInfo.AddPixels (pixelAddedSum);
        _onPixelsAdded?.Invoke (colouring, pixelAddedSum);

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

        return pixelAddedSum;
    }

    private int CountAvailablePixels (Vector2Int mouseCoordinate, Brush brush)
    {
        ComputeShader cs = _countAvailablePixelsCs;

        int kernel = cs.FindKernel ("CountAvailablePixels");
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

    public Dictionary < (int, PixelUsage), int > GetPixelIdsAndUsagesCount ()
    {
        if (_readTextureInfos == null)
        {
            Debug.LogError (nameof (_readTextureInfos) + "null, it was not loaded (not found?). Returning null.");
            return null;
        }

        // Read pixels count from frame texture
        int kernel = _readTextureInfos.FindKernel ("CSMain");

        // Set color ids infos
        // Set color ids buffer
        ComputeBuffer colorsIdsBuffer = new ComputeBuffer (_pixelIds.Length, sizeof (int));
        colorsIdsBuffer.SetData (_pixelIds);
        _readTextureInfos.SetBuffer (kernel, "ColorIds", colorsIdsBuffer);

        // Set color usages buffer
        ComputeBuffer colorsUsagesBuffer = new ComputeBuffer (_pixelUsages.Length, sizeof (int));
        colorsUsagesBuffer.SetData (_pixelUsages);
        _readTextureInfos.SetBuffer (kernel, "ColorUsages", colorsUsagesBuffer);

        // Set color counts
        int colorUsageMaxValue = Utils.GetMaxEnumValue (typeof (PixelUsage));
        int[] pixCounts = new int[(CharColouringRegistry.Instance.MaxId + 1) * (colorUsageMaxValue + 1)];

        ComputeBuffer pixCountBuffer = new ComputeBuffer (pixCounts.Length, sizeof (uint));
        pixCountBuffer.SetData (pixCounts);
        _readTextureInfos.SetBuffer (kernel, "PixelCountsByColorIdAndUsage", pixCountBuffer);

        // Set MaxId
        _readTextureInfos.SetInt ("IdLenght", CharColouringRegistry.Instance.MaxId + 1);
        _readTextureInfos.SetInt ("Width", Width);

        // Exec CS
        int x = GraphicUtils.GetComputeShaderDispatchCount (Width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (Height, 32);
        int z = 1;
        _readTextureInfos.Dispatch (kernel, x, y, z);

        // retrieve limited colors & read
        pixCountBuffer.GetData (pixCounts);

        Dictionary < (int, PixelUsage), int > pixelCountByColorIdAndUsage = new Dictionary < (int, PixelUsage), int > ();
        for (int id = 1; id <= CharColouringRegistry.Instance.MaxId; id++)
        {
            foreach (PixelUsage colorUsage in System.Enum.GetValues (typeof (PixelUsage)))
            {
                int index = ((int) colorUsage * (CharColouringRegistry.Instance.MaxId + 1)) + id;
                if (pixCounts[index] > 0)
                {
                    pixelCountByColorIdAndUsage.Add ((id, colorUsage), pixCounts[index]);
                }
            }
        }

        // Release
        colorsIdsBuffer.Release ();
        colorsUsagesBuffer.Release ();
        pixCountBuffer.Release ();

        return pixelCountByColorIdAndUsage;
    }

    public void DrawPath ()
    {
        return;
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

    public void InitByFrameInfos (FrameInfos frameInfos)
    {
        if (frameInfos.PixelIds.Length != Width * Height)
            throw new Exception ("texInfos dimensions does not match with frame, len is : " + frameInfos.PixelIds.Length);

        _pixelIds = frameInfos.PixelIds;
        _pixelUsages = frameInfos.PixelUsages;
        _pixelTimestamps = frameInfos.PixelTimestamps;
        _currentPixelsAllowed = frameInfos.CurrentPixelsAllowed;
        _maxPixelsAllowed = frameInfos.MaxPixelsAllowed;

        Color[] pixels = new Color[Width * Height];
        Dictionary<Colouring, Color[]> coloringsPixels = new Dictionary<Colouring, Color[]> ();
        for (int i = 0; i < pixels.Length; i++)
        {
            int id = _pixelIds[i];
            if (CharColouringRegistry.Instance.ColouringsSourceById.ContainsKey (id))
            {
                if (!coloringsPixels.ContainsKey (CharColouringRegistry.Instance.ColouringsSourceById[id]))
                    coloringsPixels.Add (CharColouringRegistry.Instance.ColouringsSourceById[id], CharColouringRegistry.Instance.ColouringsSourceById[id].Texture.GetPixels ());

                int w = CharColouringRegistry.Instance.ColouringsSourceById[id].Texture.width;
                int x = i % w;
                int y = (i / w) % w;
                int index = (y * w) + x;

                pixels[i] = coloringsPixels[CharColouringRegistry.Instance.ColouringsSourceById[id]][index];
            }
        }

        DrawTexture.SetPixels (pixels);
        DrawTexture.Apply ();
    }
}