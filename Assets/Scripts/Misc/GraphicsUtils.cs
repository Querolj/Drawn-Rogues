using System.Collections.Generic;
using UnityEngine;

public class GraphicUtils
{
    public static Texture2D Resize (Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture (targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit (texture2D, rt);
        Texture2D result = new Texture2D (targetX, targetY);
        result.ReadPixels (new Rect (0, 0, targetX, targetY), 0, 0);
        result.Apply ();
        return result;
    }

    public static (Vector2Int, Vector2Int) GetLeftBottomRightTopCornersOfRectPoints (List<Vector2Int> points)
    {
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (Vector2Int point in points)
        {
            if (point.x < minX)
                minX = point.x;
            if (point.x > maxX)
                maxX = point.x;
            if (point.y < minY)
                minY = point.y;
            if (point.y > maxY)
                maxY = point.y;
        }

        return (new Vector2Int (minX, minY), new Vector2Int (maxX, maxY));
    }

    public static int GetComputeShaderDispatchCount (int pixelCount, int groupThreadSize)
    {
        return pixelCount / groupThreadSize + (pixelCount % groupThreadSize > 0 ? 1 : 0);
    }

    public static Texture2D GetTextureCopy (Texture2D source)
    {
        return Resize (source, source.width, source.height);
    }

    public static void SaveTextureAsPNG (Texture2D texture, string fullPath)
    {
        byte[] _bytes = texture.EncodeToPNG ();
        System.IO.File.WriteAllBytes (fullPath, _bytes);
        // Debug.Log (_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
    }

    public static void SavePixelsAsPNG (Color[] pixels, string _fullPath, int width, int height)
    {
        Texture2D texture = new Texture2D (width, height, TextureFormat.RGBA32, false);
        texture.SetPixels (pixels);
        texture.Apply ();
        SaveTextureAsPNG (texture, _fullPath);
    }

    public static Texture2D DuplicateTextureRG16 (Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary (
            source.width,
            source.height,
            0,
            RenderTextureFormat.RG16);

        Graphics.Blit (source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D (source.width, source.height, TextureFormat.RG16, 0, true);
        readableText.ReadPixels (new Rect (0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply ();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary (renderTex);
        return readableText;
    }
}