using System;
using System.Collections.Generic;
using UnityEngine;

public class GraphicUtils
{

    private static ComputeShader _calculateSpriteBoundsCs;

    public static Vector4 GetTextureBorder (Texture2D texture)
    {
        _calculateSpriteBoundsCs = Resources.Load<ComputeShader> ("CalculateSpriteRect");
        if (_calculateSpriteBoundsCs == null)
        {
            Debug.LogError (nameof (_calculateSpriteBoundsCs) + "null, it was not loaded (not found?)");
            return Vector4.zero;
        }

        // Update sprite bounds
        int kernel = _calculateSpriteBoundsCs.FindKernel ("CSMain");

        _calculateSpriteBoundsCs.SetTexture (kernel, "Tex", texture);

        ComputeBuffer minXBuf = new ComputeBuffer (1, sizeof (int));
        minXBuf.SetData (new int[] { Int32.MaxValue });
        _calculateSpriteBoundsCs.SetBuffer (kernel, "MinX", minXBuf);

        ComputeBuffer minYBuf = new ComputeBuffer (1, sizeof (int));
        minYBuf.SetData (new int[] { Int32.MaxValue });
        _calculateSpriteBoundsCs.SetBuffer (kernel, "MinY", minYBuf);

        ComputeBuffer maxXBuf = new ComputeBuffer (1, sizeof (int));
        maxXBuf.SetData (new int[] {-1 });
        _calculateSpriteBoundsCs.SetBuffer (kernel, "MaxX", maxXBuf);

        ComputeBuffer maxYBuf = new ComputeBuffer (1, sizeof (int));
        maxYBuf.SetData (new int[] {-1 });
        _calculateSpriteBoundsCs.SetBuffer (kernel, "MaxY", maxYBuf);

        // Run CS
        int x = GraphicUtils.GetComputeShaderDispatchCount (texture.width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (texture.height, 32);
        int z = 1;

        _calculateSpriteBoundsCs.Dispatch (kernel, x, y, z);
        int[] minX = new int[1], minY = new int[1], maxX = new int[1], maxY = new int[1];
        minXBuf.GetData (minX);
        minYBuf.GetData (minY);
        maxXBuf.GetData (maxX);
        maxYBuf.GetData (maxY);

        // Release
        minXBuf.Release ();
        minYBuf.Release ();
        maxXBuf.Release ();
        maxYBuf.Release ();

        return new Vector4 (minX[0], minY[0], maxX[0], maxY[0]);
    }

    private static Dictionary<Vector2, Texture2D> _transparentTexByDimension = new Dictionary<Vector2, Texture2D> ();
    public static Texture2D GetUniqueTransparentTex (Vector2Int dimension)
    {
        Texture2D copy;
        if (_transparentTexByDimension.ContainsKey (dimension))
        {
            copy = GraphicUtils.GetTextureCopy (_transparentTexByDimension[dimension]);
            copy.filterMode = FilterMode.Point;

            return copy;
        }

        Texture2D transparentTex = new Texture2D (dimension.x, dimension.y, TextureFormat.ARGB32, false);
        Color[] pixels = new Color[dimension.x * dimension.y];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        transparentTex.SetPixels (pixels);
        transparentTex.Apply ();

        _transparentTexByDimension.Add (dimension, transparentTex);

        copy = GraphicUtils.GetTextureCopy (_transparentTexByDimension[dimension]);
        copy.filterMode = FilterMode.Point;

        return copy;
    }

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