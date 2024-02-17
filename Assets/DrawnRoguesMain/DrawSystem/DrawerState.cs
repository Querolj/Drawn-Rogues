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
    private Dictionary<BaseColor, int> _colorDropsAvailable = new Dictionary<BaseColor, int> ();

    public BaseColorPaletteState (BaseColorInventory baseColorInventory)
    {
        if (baseColorInventory == null)
            throw new ArgumentNullException (nameof (baseColorInventory));
        _colorDropsAvailable = new Dictionary<BaseColor, int> (baseColorInventory.ColorDropsAvailable);

    }

    public void Apply (ref BaseColorInventory palette)
    {
        if (palette == null)
            throw new ArgumentNullException (nameof (palette));
        palette.ColorDropsAvailable = new Dictionary<BaseColor, int> (_colorDropsAvailable);
    }
}

public class DrawerState
{
    private FrameState _frameState;
    private BaseColorPaletteState _paletteState;

    public DrawerState (BaseColorInventory palette, Frame frame)
    {
        if (palette == null)
            throw new ArgumentNullException (nameof (palette));
        if (frame == null)
            throw new ArgumentNullException (nameof (frame));

        _frameState = new FrameState (ref frame);
        _paletteState = new BaseColorPaletteState (palette);

    }

    public void Apply (ref BaseColorInventory palette)
    {
        if (palette == null)
            throw new ArgumentNullException (nameof (palette));

        _paletteState.Apply (ref palette);
        _frameState.Apply ();
    }
}