using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawedCharacter : Character
{
    [SerializeField]
    private GameObject _modifierLayer;

    [SerializeField]
    private List<ColouringSpell> _colouringSpell = new List<ColouringSpell> ();
    public List<ColouringSpell> ColouringSpells
    {
        get { return _colouringSpell; }
    }

    private FrameReader _frameReader;

    private int _level = 1;
    public int Level
    {
        get { return _level; }
    }
    private int _totalXp = 0;
    private const int ADD_PIXELS_PER_LEVEL = 100;

    private ModifierGoInstanceFactory _modifierGoInstanceFactory;

    private ComputeShader _calculateSpriteBoundsCs;
    private DrawedCharacterFormDescription _drawedCharacterFormDescription;
    public DrawedCharacterFormDescription DrawedCharacterFormDescription
    {
        get { return _drawedCharacterFormDescription; }
    }

    private List<ModifierInfos> _modifierAdded = new List<ModifierInfos> ();
    public List<ModifierInfos> ModifiersAdded
    {
        get { return _modifierAdded; }
    }

    public event Action OnStatsChanged;
    public Action OnDrawedCharacterUpdate;
    private Material _mat;

    private int _maxPixelsAllowed;
    public int MaxPixelsAllowed
    {
        get { return _maxPixelsAllowed; }
    }

    protected override void Awake ()
    {
        base.Awake ();
        _isEnemy = false;
        _modifierGoInstanceFactory = GameObject.FindFirstObjectByType<ModifierGoInstanceFactory> (); // TODO : Inject
        _frameReader = GameObject.FindFirstObjectByType<FrameReader> (); // TODO : Inject

        _collider = GetComponent<Collider> ();

        _calculateSpriteBoundsCs = Resources.Load<ComputeShader> ("CalculateSpriteRect");
        if (_calculateSpriteBoundsCs == null)
        {
            Debug.LogError (nameof (_calculateSpriteBoundsCs) + "null, it was not loaded (not found?)");
            return;
        }

        ComputeShader getCharProportionCs = Resources.Load<ComputeShader> ("GetCharacterProportion");
        if (getCharProportionCs == null)
        {
            Debug.LogError (nameof (getCharProportionCs) + "null, it was not loaded (not found?)");
            return;
        }

        _drawedCharacterFormDescription = new DrawedCharacterFormDescription ();
        _mat = GetComponent<SpriteRenderer> ().material;
    }

    protected override void Start ()
    {
        base.Start ();

        if (_collider as BoxCollider != null)
        {
            BoxCollider boxCollider = _collider as BoxCollider;
            Vector3 boxSize = _bounds.size;
            boxSize.z = 0.1f;
            boxCollider.size = boxSize;

            boxCollider.center = transform.InverseTransformPoint (_bounds.center);
            Vector3 boxCenter = boxCollider.center;
            boxCenter.z = 0f;
            boxCollider.center = boxCenter;
        }
    }

    private void UpdateStats (Dictionary < (int, PixelUsage), int > pixelUsageByIds, bool resetCurrentLife = true)
    {
        Stats = new Stats (pixelUsageByIds);

        foreach (ModifierInfos modifierInfos in _modifierAdded)
        {
            Modifier modifier = Resources.Load<Modifier> ("Modifier/" + modifierInfos.SOFileName);
            if (modifier == null)
                throw new Exception ("Modifier " + modifierInfos.SOFileName + " not found");
            Stats.AddStats (modifier.Stats);
        }

        OnStatsChanged?.Invoke ();
        // Debug.Log (Stats.ToString ());
        SetMaxAndCurrentLife (resetCurrentLife);
    }

    public void GenerateColliders ()
    {
        // AdvancedPolygonCollider advancedPolyCol;
        // if (_collider2dGo == null)
        // {
        //     _collider2dGo = new GameObject ("Collider2D");
        //     _collider2dGo.transform.SetParent (transform);
        //     _collider2dGo.transform.localPosition = Vector3.zero;
        //     Bounds bounds = (Bounds) GetSpriteBounds ();
        //     SpriteRenderer rendCol2d = _collider2dGo.AddComponent<SpriteRenderer> ();
        //     rendCol2d.sprite = _renderer.sprite;
        //     advancedPolyCol = _collider2dGo.AddComponent<AdvancedPolygonCollider> ();
        //     advancedPolyCol.RunInPlayMode = true;
        //     advancedPolyCol.DistanceThreshold = 0;
        //     advancedPolyCol.AlphaTolerance = 1;
        //     rendCol2d.color = Color.clear;
        // }
        // else
        // {
        //     advancedPolyCol = _collider2dGo.GetComponent<AdvancedPolygonCollider> ();
        //     advancedPolyCol.RecalculatePolygon ();
        // }

    }

    public Sprite GetShadowSprite ()
    {
        Bounds bounds = (Bounds) GetSpriteBounds ();
        Texture2D shadowTexture = new Texture2D ((int) (bounds.size.x * PIXEL_PER_UNIT), (int) (bounds.size.y * PIXEL_PER_UNIT));
        Color[] shadowPixels = new Color[shadowTexture.width * shadowTexture.height];
        for (int i = 0; i < shadowPixels.Length; i++)
        {
            shadowPixels[i] = Color.black;
        }
        Sprite shadowSprite = Sprite.Create (shadowTexture, new Rect (0, 0, shadowTexture.width, shadowTexture.height), new Vector2 (0.5f, 0.5f), PIXEL_PER_UNIT);

        return shadowSprite;
    }

    private void AddModifier (Modifier modifier, ModifierInfos modifierInfos)
    {
        _modifierAdded.Add (modifierInfos);

        GameObject modifierGoInstance = _modifierGoInstanceFactory.Create (Renderer.bounds, _modifierLayer.transform, modifier,
            modifierInfos.GetLocalPosition (Renderer.bounds, new Vector3 (0.5f, 0.5f, 0f)), modifierInfos.IsFlipped);

        if (modifier.Stats?.HasAnyStats () == true)
        {
            Stats.AddStats (modifier.Stats);
            OnStatsChanged?.Invoke ();
        }
    }

    private void RemoveAllModifiersGo ()
    {
        for (int i = 0; i < _modifierLayer.transform.childCount; i++)
        {
            Destroy (_modifierLayer.transform.GetChild (i).gameObject);
        }
    }

    public void Init (DrawedCharacterFormDescription drawedCharacterFormDescription, Frame frame, List<ModifierInfos> modifiersAdded, bool resetCurrentLife = true)
    {
        _drawedCharacterFormDescription = drawedCharacterFormDescription;

        RemoveAllModifiersGo ();
        _modifierAdded = new List<ModifierInfos> ();
        foreach (ModifierInfos modifierInfos in modifiersAdded)
        {
            Modifier modifier = Resources.Load<Modifier> ("Modifier/" + modifierInfos.SOFileName);
            if (modifier == null)
                throw new Exception ("Modifier " + modifierInfos.SOFileName + " not found");
            AddModifier (modifier, modifierInfos);
        }

        GenerateColliders ();
        UpdateStats (_frameReader.GetPixelIdsAndUsagesCount (frame), resetCurrentLife);
        Sprite sprite = Sprite.Create (frame.DrawTexture, new Rect (0, 0, frame.DrawTexture.width, frame.DrawTexture.height), new Vector2 (0.5f, 0.5f), PIXEL_PER_UNIT);
        _renderer.sprite = sprite;
        OnDrawedCharacterUpdate?.Invoke ();

        // OnDrawedCharacterUpdate?.Invoke ();
        // GraphicUtils.SavePixelsAsPNG (_frame.DrawTexture.GetPixels (), "Test/playerInit.png", _frame.DrawTexture.width, _frame.DrawTexture.height);
    }

    public void Init (DrawedCharacterInfos drawedCharacterInfos)
    {
        _name = drawedCharacterInfos.Name;
        _drawedCharacterFormDescription = drawedCharacterInfos.DrawedCharacterFormDescription;
        _modifierAdded = new List<ModifierInfos> ();
        foreach (ModifierInfos modifierInfos in drawedCharacterInfos.ModifierInfos)
        {
            Modifier modifier = Resources.Load<Modifier> ("Modifier/" + modifierInfos.SOFileName);
            if (modifier == null)
                throw new Exception ("Modifier " + modifierInfos.SOFileName + " not found");
            AddModifier (modifier, modifierInfos);
        }

        LoadFrameInfos (drawedCharacterInfos.FrameInfos);
        GenerateColliders ();
        OnDrawedCharacterUpdate?.Invoke ();
        // UpdateStats ();
    }

    private void LoadFrameInfos (FrameInfos frameInfos)
    {
        const int DIMENSION = 64;
        if (frameInfos.PixelIds.Length != DIMENSION * DIMENSION)
            throw new Exception ("frameInfos width and height does not match " + DIMENSION);

        Texture2D drawTex = new Texture2D (DIMENSION, DIMENSION);
        drawTex.filterMode = FilterMode.Point;
        drawTex.wrapMode = TextureWrapMode.Clamp;
        _maxPixelsAllowed = frameInfos.MaxPixelsAllowed;

        Color[] pixels = new Color[DIMENSION * DIMENSION];
        Dictionary<Colouring, Color[]> coloringsPixels = new Dictionary<Colouring, Color[]> ();
        for (int i = 0; i < pixels.Length; i++)
        {
            int id = frameInfos.PixelIds[i];
            if (CharColouringRegistry.Instance.ColouringsSourceById.ContainsKey (id))
            {
                if (!coloringsPixels.ContainsKey (CharColouringRegistry.Instance.ColouringsSourceById[id]))
                    coloringsPixels.Add (CharColouringRegistry.Instance.ColouringsSourceById[id], CharColouringRegistry.Instance.ColouringsSourceById[id].Texture.GetPixels ());

                int w = CharColouringRegistry.Instance.ColouringsSourceById[id].Texture.width;
                int x = i % w;
                int y = (i / w) % w;
                int index = (y * w) + x;

                pixels[i] = coloringsPixels[CharColouringRegistry.Instance.ColouringsSourceById[id]][index];
            }
        }

        drawTex.SetPixels (pixels);
        drawTex.Apply ();

        Sprite sprite = Sprite.Create (drawTex, new Rect (0, 0, drawTex.width, drawTex.height), new Vector2 (0.5f, 0.5f), PIXEL_PER_UNIT);
        _renderer.sprite = sprite;
        GraphicUtils.SavePixelsAsPNG (drawTex.GetPixels (), "Test/playerInit.png", drawTex.width, drawTex.height);

    }

    public Action OnLevelUp;
    // Formulat : n^3
    public void GainXp (int xp)
    {
        int totalXpToNextLevel = (int) Mathf.Pow (_level, 3f);
        while (xp > 0)
        {
            if (_totalXp + xp >= totalXpToNextLevel)
            {
                xp -= (int) (totalXpToNextLevel - _totalXp);
                _totalXp = totalXpToNextLevel;
                _level++;
                totalXpToNextLevel = (int) Mathf.Pow (_level, 3f);
                OnLevelUp?.Invoke ();
                _maxPixelsAllowed += ADD_PIXELS_PER_LEVEL;
            }
            else
            {
                _totalXp += xp;
                xp = 0;
            }
        }
    }

    public int GetMaxModifierAllowed ()
    {
        return _level + 1;
    }
}