using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public enum WidthAdjective
{
    PaperThin,
    Thin,
    Medium,
    Large,
    Huge,
}

public enum HeightAdjective
{
    Minuscule,
    Short,
    Medium,
    Tall,
    Giant,
}

public class DrawedCharacterFormDescription
{
    [JsonProperty ("numberOfArms")]
    private int _numberOfArms = 0;
    public int NumberOfArms
    {
        get
        {
            return _numberOfArms;
        }
    }

    [JsonProperty ("numberOfLegs")]
    private int _numberOfLegs = 0;
    public int NumberOfLegs
    {
        get
        {
            return _numberOfLegs;
        }
    }

    [JsonProperty ("tallPercentage")]
    private float _tallPercentage = -1;

    [JsonProperty ("widePercentage")]
    private float _widePercentage = -1;

    private Dictionary<float, HeightAdjective> _heightPercentageToHeightAdjective = new Dictionary<float, HeightAdjective> ()
    { { 0f, HeightAdjective.Minuscule }, { 0.15f, HeightAdjective.Short }, { 0.3f, HeightAdjective.Medium }, { 0.6f, HeightAdjective.Tall }, { 0.85f, HeightAdjective.Giant },
    };

    public HeightAdjective HeightAdjective
    {
        get
        {
            return GetAdjectiveForHeightPercentage ();
        }
    }

    private Dictionary<float, WidthAdjective> _widthPercentageToWidthAdjective = new Dictionary<float, WidthAdjective> ()
    { { 0f, WidthAdjective.PaperThin }, { 0.15f, WidthAdjective.Thin }, { 0.3f, WidthAdjective.Medium }, { 0.6f, WidthAdjective.Large }, { 0.85f, WidthAdjective.Huge },
    };
    public WidthAdjective WidthAdjective
    {
        get
        {
            return GetAdjectiveForWidthPercentage ();
        }
    }

    private ComputeShader _getCharProportionCs = null;

    public event Action OnUpdated;

    public DrawedCharacterFormDescription ()
    {

    }

    public DrawedCharacterFormDescription (DrawedCharacterFormDescription dcfd)
    {
        _numberOfArms = dcfd._numberOfArms;
        _numberOfLegs = dcfd._numberOfLegs;
        _tallPercentage = dcfd._tallPercentage;
        _widePercentage = dcfd._widePercentage;
        _heightPercentageToHeightAdjective = dcfd._heightPercentageToHeightAdjective;
        _widthPercentageToWidthAdjective = dcfd._widthPercentageToWidthAdjective;

    }

    public void CalculateCharProportion (Texture2D charTex, int[] pixelUsages)
    {
        if (_getCharProportionCs == null)
        {
            _getCharProportionCs = Resources.Load<ComputeShader> ("GetCharacterProportion");
            if (_getCharProportionCs == null)
            {
                Debug.LogError (nameof (_getCharProportionCs) + "null, it was not loaded (not found?)");
                return;
            }
        }

        int kernel = _getCharProportionCs.FindKernel ("CSMain");

        _getCharProportionCs.SetTexture (kernel, "Tex", charTex);

        ComputeBuffer minXBuf = new ComputeBuffer (1, sizeof (int));
        minXBuf.SetData (new int[] { Int32.MaxValue });
        _getCharProportionCs.SetBuffer (kernel, "MinX", minXBuf);

        ComputeBuffer minYBuf = new ComputeBuffer (1, sizeof (int));
        minYBuf.SetData (new int[] { Int32.MaxValue });
        _getCharProportionCs.SetBuffer (kernel, "MinY", minYBuf);

        ComputeBuffer maxXBuf = new ComputeBuffer (1, sizeof (int));
        maxXBuf.SetData (new int[] {-1 });
        _getCharProportionCs.SetBuffer (kernel, "MaxX", maxXBuf);

        ComputeBuffer maxYBuf = new ComputeBuffer (1, sizeof (int));
        maxYBuf.SetData (new int[] {-1 });
        _getCharProportionCs.SetBuffer (kernel, "MaxY", maxYBuf);

        _getCharProportionCs.SetInt ("FrameWidth", charTex.width);

        ComputeBuffer pixelUsageBuffer = new ComputeBuffer (pixelUsages.Length, sizeof (int));
        pixelUsageBuffer.SetData (pixelUsages);
        _getCharProportionCs.SetBuffer (kernel, "PixelUsages", pixelUsageBuffer);

        // Run CS
        int x = GraphicUtils.GetComputeShaderDispatchCount (charTex.width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (charTex.height, 32);
        int z = 1;

        _getCharProportionCs.Dispatch (kernel, x, y, z);
        int[] minX = new int[1], minY = new int[1], maxX = new int[1], maxY = new int[1];
        minXBuf.GetData (minX);
        minYBuf.GetData (minY);
        maxXBuf.GetData (maxX);
        maxYBuf.GetData (maxY);

        _widePercentage = (float) (maxX[0] - minX[0]) / charTex.width;
        _tallPercentage = (float) (maxY[0] - minY[0]) / charTex.height;

        // Release
        minXBuf.Release ();
        minYBuf.Release ();
        maxXBuf.Release ();
        maxYBuf.Release ();
        pixelUsageBuffer.Release ();

        OnUpdated?.Invoke ();
    }

    public void AddStrokeInfo (StrokeInfo strokeInfo)
    {
        if (strokeInfo.PixelUsage == PixelUsage.Arm)
        {
            if (!strokeInfo.PixelUsagesTouched.Contains (PixelUsage.Arm) && strokeInfo.PixelUsagesTouched.Contains (PixelUsage.Body))
            {
                _numberOfArms++;
                OnUpdated?.Invoke ();
            }
        }
        else if (strokeInfo.PixelUsage == PixelUsage.Leg)
        {
            if (!strokeInfo.PixelUsagesTouched.Contains (PixelUsage.Leg) && strokeInfo.PixelUsagesTouched.Contains (PixelUsage.Body) && strokeInfo.DownBorderTouched)
            {
                _numberOfLegs++;
                OnUpdated?.Invoke ();
            }
        }
    }

    public bool HasAnyForm ()
    {
        return _tallPercentage > 0 || _widePercentage > 0;
    }

    private string GetAdjectiveStrForHeightPercentage ()
    {
        float tallPercentage = _tallPercentage;
        float lastKey = 0;
        foreach (var key in _heightPercentageToHeightAdjective.Keys)
        {
            if (tallPercentage < key)
            {
                return _heightPercentageToHeightAdjective[lastKey].ToString ();
            }

            lastKey = key;
        }

        return _heightPercentageToHeightAdjective[lastKey].ToString ();
    }

    private HeightAdjective GetAdjectiveForHeightPercentage ()
    {
        float tallPercentage = _tallPercentage;
        float lastKey = 0;
        foreach (var key in _heightPercentageToHeightAdjective.Keys)
        {
            if (tallPercentage < key)
            {
                return _heightPercentageToHeightAdjective[lastKey];
            }

            lastKey = key;
        }

        return _heightPercentageToHeightAdjective[lastKey];
    }

    private string GetAdjectiveStrForWidthPercentage ()
    {
        float widePercentage = _widePercentage;
        float lastKey = 0;
        foreach (var key in _widthPercentageToWidthAdjective.Keys)
        {
            if (widePercentage < key)
            {
                return _widthPercentageToWidthAdjective[lastKey].ToString ();
            }

            lastKey = key;
        }

        return _widthPercentageToWidthAdjective[lastKey].ToString ();
    }

    private WidthAdjective GetAdjectiveForWidthPercentage ()
    {
        float widePercentage = _widePercentage;
        float lastKey = 0;
        foreach (var key in _widthPercentageToWidthAdjective.Keys)
        {
            if (widePercentage < key)
            {
                return _widthPercentageToWidthAdjective[lastKey];
            }

            lastKey = key;
        }

        return _widthPercentageToWidthAdjective[lastKey];

    }

    public override string ToString ()
    {
        string desc = "";
        desc += "Arms: " + _numberOfArms + "\n";
        desc += "Legs: " + _numberOfLegs + "\n";

        if (_tallPercentage != -1)
        {
            desc += "Height : " + GetAdjectiveForHeightPercentage () + "\n";
        }

        if (_widePercentage != -1)
        {
            desc += "Wideness : " + GetAdjectiveForWidthPercentage () + "\n";
        }

        return desc;
    }
}