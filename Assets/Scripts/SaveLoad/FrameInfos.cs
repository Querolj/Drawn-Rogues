public class FrameInfos
{
    public int[] PixelIds { get; private set; }
    public int[] PixelUsages { get; private set; }
    public int[] PixelTimestamps { get; private set; }
    public int CurrentPixelsAllowed;
    public int MaxPixelsAllowed;

    public FrameInfos (int[] pixelIds, int[] pixelUsages, int[] pixelTimestamps, int currentPixelsAllowed, int maxPixelsAllowed)
    {
        PixelIds = pixelIds;
        PixelUsages = pixelUsages;
        PixelTimestamps = pixelTimestamps;
        CurrentPixelsAllowed = currentPixelsAllowed;
        MaxPixelsAllowed = maxPixelsAllowed;
    }

    // public FrameInfos (Frame frame)
    // {
    //     PixelIds = frame.PixelIds;
    //     PixelUsages = frame.PixelUsages;
    //     PixelTimestamps = frame.PixelTimestamps;
    //     CurrentPixelsAllowed = frame.CurrentPixelsAllowed;
    //     MaxPixelsAllowed = frame.MaxPixelsAllowed;
    // }
}