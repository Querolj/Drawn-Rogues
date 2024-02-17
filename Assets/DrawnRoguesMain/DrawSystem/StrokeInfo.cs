using System.Collections.Generic;

public class StrokeInfo
{
    public PixelUsage PixelUsage;
    public List<PixelUsage> PixelUsagesTouched = new List<PixelUsage> ();
    public bool DownBorderTouched = false;
    public int Timestamps;
    public Frame FrameTouched;
    private int _numOfPixels;
    public int NumOfPixels
    {
        get
        {
            return _numOfPixels;
        }
    }

    public StrokeInfo (PixelUsage pixelUsage, int timestamps, Frame frameTouched)
    {
        PixelUsage = pixelUsage;
        Timestamps = timestamps;
        FrameTouched = frameTouched;
        _numOfPixels = 0;
    }

    public void AddPixels (int pixelsNumber)
    {
        _numOfPixels += pixelsNumber;
    }
}