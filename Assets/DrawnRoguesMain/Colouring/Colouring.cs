using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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

    [SerializeField, BoxGroup ("Display")]
    private string _displayName;
    public string DisplayName => _displayName;

    [SerializeField, TextArea (3, 10), BoxGroup ("Display")]
    private string _description;
    public string Description => _description;

    [SerializeField, BoxGroup ("Display")]
    private Texture2D _textureUI;
    public Texture2D TextureUI => _textureUI;

    [SerializeField, BoxGroup ("Display")]
    private Texture2D _texture;
    public Texture2D Texture => _texture;

    [SerializeField, BoxGroup ("Use condition")]
    private List<BaseColorDrops> _baseColorsUsedPerPixel;
    public List<BaseColorDrops> BaseColorsUsedPerPixel => _baseColorsUsedPerPixel;

    [SerializeField, BoxGroup ("Use condition")]
    private List<PixelUsage> _pixelUsages;
    public List<PixelUsage> PixelUsages => _pixelUsages;

    [Button ("Set pixel usage for character"), BoxGroup ("Use condition")]
    private void SetPixelUsageForCharacter ()
    {
        _pixelUsages = new List<PixelUsage> ();
        _pixelUsages.Add (PixelUsage.Head);
        _pixelUsages.Add (PixelUsage.Body);
        _pixelUsages.Add (PixelUsage.Leg);
        _pixelUsages.Add (PixelUsage.Arm);
    }
    [Button ("Set pixel usage for spell"), BoxGroup ("Use condition")]
    private void SetPixelUsageForSpell ()
    {
        _pixelUsages = new List<PixelUsage> ();
        _pixelUsages.Add (PixelUsage.Combat);
        _pixelUsages.Add (PixelUsage.Map);
    }

    [SerializeField, BoxGroup ("Brush")]
    private bool _hasBrushSize;
    public bool HasBrushSize => _hasBrushSize;

    [SerializeField, ShowIf (nameof (HasBrushSize)), BoxGroup ("Brush")]
    private int _brushSize;
    public int BrushSize => _brushSize;

    [SerializeField, BoxGroup ("Brush")]
    private bool _useTextureAsBrush;
    public bool UseTextureAsBrush => _useTextureAsBrush;

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