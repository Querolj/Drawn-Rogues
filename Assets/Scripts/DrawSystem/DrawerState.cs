using System;
using System.Collections.Generic;
using UnityEngine;

public class FrameState
{
    private Color[] _pixels;
    private int[] _pixelUsages;
    private int[] _pixelIds;
    private int[] _pixelTimestamps;
    private int _currentPixelsAllowed;
    private Frame _frame;

    public FrameState (ref Frame frame)
    {
        _frame = frame??
        throw new ArgumentNullException (nameof (frame));

        _pixels = frame.DrawTexture.GetPixels ();

        _pixelUsages = new int[frame.PixelUsages.Length];
        frame.PixelUsages.CopyTo (_pixelUsages, 0);

        _pixelIds = new int[frame.PixelIds.Length];
        frame.PixelIds.CopyTo (_pixelIds, 0);

        _pixelTimestamps = new int[frame.PixelTimestamps.Length];
        frame.PixelTimestamps.CopyTo (_pixelTimestamps, 0);

        _currentPixelsAllowed = frame.CurrentPixelsAllowed;
    }

    public void Apply ()
    {
        Debug.Log ("Apply frame state " + _frame.name);
        _frame.DrawTexture.SetPixels (_pixels);
        _frame.DrawTexture.Apply ();

        _pixelUsages.CopyTo (_frame.PixelUsages, 0);
        _pixelIds.CopyTo (_frame.PixelIds, 0);
        _pixelTimestamps.CopyTo (_frame.PixelTimestamps, 0);
        _frame.CurrentPixelsAllowed = _currentPixelsAllowed;
    }

}

public class BaseColorPaletteState
{
    // private List < (ColouringInstance, int) > _pixQuantityByColouringInst;
    // public List < (ColouringInstance, int) > PixQuantityByColouringInst { get { return _pixQuantityByColouringInst; } }
    private Dictionary<BaseColor, int> _colorDropsAvailable = new Dictionary<BaseColor, int> ();

    public BaseColorPaletteState (BaseColorPalette palette)
    {
        if (palette == null)
            throw new ArgumentNullException (nameof (palette));
        _colorDropsAvailable = new Dictionary<BaseColor, int> (palette.ColorDropsAvailable);

    }

    public void Apply (ref BaseColorPalette palette)
    {
        if (palette == null)
            throw new ArgumentNullException (nameof (palette));
        palette.ColorDropsAvailable = new Dictionary<BaseColor, int> (_colorDropsAvailable);
    }
}

public class DrawerState
{
    private List<FrameState> _frameStates;
    private BaseColorPaletteState _paletteState;

    public DrawerState (BaseColorPalette palette, List<Frame> frames)
    {
        if (palette == null)
            throw new ArgumentNullException (nameof (palette));
        if (frames == null)
            throw new ArgumentNullException (nameof (frames));
        _frameStates = new List<FrameState> ();

        for (int i = 0; i < frames.Count; i++)
        {
            Frame frame = frames[i];
            _frameStates.Add (new FrameState (ref frame));
        }
        _paletteState = new BaseColorPaletteState (palette);

    }

    public void Apply (ref BaseColorPalette palette)
    {
        if (palette == null)
            throw new ArgumentNullException (nameof (palette));

        _paletteState.Apply (ref palette);
        for (int i = 0; i < _frameStates.Count; i++)
        {
            _frameStates[i].Apply ();
        }
    }

}