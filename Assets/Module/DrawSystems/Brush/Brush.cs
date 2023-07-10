using System;
using UnityEngine;

[Serializable]
public class Brush
{
    [SerializeField]
    private Texture2D _texture;
    public Texture2D Texture
    {
        get
        {
            return _texture;
        }
    }

    private Color[] _pixels;
    public Color[] Pixels
    {
        get
        {
            if (_pixels == null || _pixels.Length == 0)
            {
                _pixels = _texture.GetPixels ();
            }

            return _pixels;
        }
    }

    private const float PIXEL_PER_UNIT = 100f;

    private int _opaquePixelsCount = -1;
    public int GetOpaquePixelsCount ()
    {
        if (_opaquePixelsCount == -1)
        {
            _opaquePixelsCount = 0;
            foreach (Color pixel in Pixels)
            {
                if (pixel.a > 0)
                    _opaquePixelsCount++;
            }
        }

        return _opaquePixelsCount;
    }

    public (int, int) GetDimensions ()
    {
        return (_texture.width, _texture.height);
    }

    public Vector2 GetExtents ()
    {
        return new Vector2 (_texture.width / PIXEL_PER_UNIT, _texture.height / PIXEL_PER_UNIT);
    }
}