using System.Collections.Generic;
using UnityEngine;

public class FrameReader : MonoBehaviour
{
    private ComputeShader _readTextureInfos = null;

    private void Awake ()
    {
        _readTextureInfos = Resources.Load<ComputeShader> ("ReadTextureInfos");
        if (_readTextureInfos == null)
        {
            Debug.LogError (nameof (_readTextureInfos) + "null, it was not loaded (not found?)");
        }
    }

    public Dictionary < (int, PixelUsage), int > GetPixelIdsAndUsagesCount (Frame frame)
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
        ComputeBuffer colorsIdsBuffer = new ComputeBuffer (frame.PixelIds.Length, sizeof (int));
        colorsIdsBuffer.SetData (frame.PixelIds);
        _readTextureInfos.SetBuffer (kernel, "ColorIds", colorsIdsBuffer);

        // Set color usages buffer
        ComputeBuffer colorsUsagesBuffer = new ComputeBuffer (frame.PixelUsages.Length, sizeof (int));
        colorsUsagesBuffer.SetData (frame.PixelUsages);
        _readTextureInfos.SetBuffer (kernel, "ColorUsages", colorsUsagesBuffer);

        // Set color counts
        int colorUsageMaxValue = Utils.GetMaxEnumValue (typeof (PixelUsage));
        int[] pixCounts = new int[(CharColouringRegistry.Instance.MaxId + 1) * (colorUsageMaxValue + 1)];

        ComputeBuffer pixCountBuffer = new ComputeBuffer (pixCounts.Length, sizeof (uint));
        pixCountBuffer.SetData (pixCounts);
        _readTextureInfos.SetBuffer (kernel, "PixelCountsByColorIdAndUsage", pixCountBuffer);

        // Set MaxId
        _readTextureInfos.SetInt ("IdLenght", CharColouringRegistry.Instance.MaxId + 1);
        _readTextureInfos.SetInt ("Width", frame.Width);

        // Exec CS
        int x = GraphicUtils.GetComputeShaderDispatchCount (frame.Width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (frame.Height, 32);
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
}