using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Colouring", menuName = "Colouring/Colouring", order = 1)]
public class Colouring : ScriptableObject
{
    [SerializeField]
    private int _id = -1;
    public int Id
    {
        get { return _id; }
    }

    public string Name;
    public List<BaseColorDrops> BaseColorsUsedPerPixel;
    public List<PixelUsage> PixelUsages;
    public bool HasBrushSize;
    public bool UseTextureAsBrush;
    public int BrushSize;
    public Texture2D Texture;
    public Texture2D TextureUI;
    public string Description;

    private Sprite _spriteUI = null;
    public Sprite SpriteUI
    {
        get
        {
            if (_spriteUI == null)
            {
                _spriteUI = Sprite.Create (TextureUI, new Rect (0, 0, TextureUI.width, TextureUI.height), Vector2.zero);
            }
            return _spriteUI;
        }
    }

    private void OnValidate ()
    {
        if (BaseColorsUsedPerPixel == null || BaseColorsUsedPerPixel.Count == 0)
        {
            Debug.LogError ("No base color used per pixel for " + name);
        }

        if (PixelUsages == null || PixelUsages.Count == 0)
        {
            Debug.LogError ("No pixel usage for " + name);
        }
    }
}

[Serializable]
public class BaseColorDrops
{
    public BaseColor BaseColor;
    public int TotalDrops;
}