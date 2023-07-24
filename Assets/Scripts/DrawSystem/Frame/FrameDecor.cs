using System;
using DigitalRuby.AdvancedPolygonCollider;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class FrameDecor : Frame
{
    [SerializeField]
    private AdvancedPolygonCollider _advancedPolygonColliderTemplate;

    private Drawer _drawer;
    private TurnManager _turnBasedCombat;
    private const float _MESH_COLLIDER_THICKNESS = 0.03f;
    public Bounds Bounds
    {
        get
        {
            return _renderer.bounds;
        }
    }

    private Attackable.Factory _attackableFactory;

    [Inject, UsedImplicitly]
    private void Init (Attackable.Factory attackableFactory)
    {
        _attackableFactory = attackableFactory;
    }

    protected override void Awake ()
    {
        base.Awake ();
        _drawer = FindAnyObjectByType<Drawer> (); // TODO : inject
        _turnBasedCombat = FindAnyObjectByType<TurnManager> (); // TODO : inject

        _drawer.OnDrawStrokeEnd += (c, si) =>
        {
            if (si.FrameTouched != this)
            {
                return;
            }
            GenerateColliderForColoring (c as ColouringSpell);
            Clear ();
        };
    }

    public void GenerateColliderForColoring (ColouringSpell colouringSpell)
    {
        int id = colouringSpell.Id;
        if (!TryGenerateSpriteFromFrame (colouringSpell, out Sprite sprite))
        {
            Debug.LogError ("Failed to generate sprite from frame, returning null");
            return;
        }

        AdvancedPolygonCollider advancedPolyCol = GameObject.Instantiate (_advancedPolygonColliderTemplate);

        SpriteRenderer spriteRend = advancedPolyCol.GetComponent<SpriteRenderer> ();

        advancedPolyCol.transform.SetParent (gameObject.transform);

        // Create a gameobject with a polycollider from Sprite phys shape (name it by color usage)
        spriteRend.sprite = sprite;
        spriteRend.material.mainTexture = sprite.texture;

        advancedPolyCol.RecalculatePolygon ();

        PolygonCollider2D polyCol = advancedPolyCol.GetComponent<PolygonCollider2D> ();

        if (colouringSpell.BehaviourPrefab == null)
            throw new Exception ("could not load prefab for colouring spell " + colouringSpell.Name + ", can't generate collider for lc " + colouringSpell.Name);

        // goBehaviour = GameObject.Instantiate (colouringSpell.BehaviourPrefab);
        GameObject goBehaviour = _attackableFactory.Create (colouringSpell.BehaviourPrefab).gameObject;

        goBehaviour.transform.SetParent (gameObject.transform);
        Vector3 newLocalPos = Vector3.zero;
        newLocalPos.z = -0.01f;
        goBehaviour.transform.localPosition = newLocalPos;
        goBehaviour.transform.localRotation = Quaternion.identity;

        Mesh mesh = ExtrudeSprite.CreateMesh (polyCol.points, frontDistance: -_MESH_COLLIDER_THICKNESS, backDistance : _MESH_COLLIDER_THICKNESS);

        MeshCollider meshCol = goBehaviour.GetComponent<MeshCollider> ();
        meshCol.sharedMesh = mesh;

        SpriteRenderer spriteRendColouring = goBehaviour.GetComponent<SpriteRenderer> ();
        if (spriteRendColouring.sprite == null)
            spriteRendColouring.sprite = sprite;
        else
        {
            MeshFilter meshFilter = goBehaviour.GetComponent<MeshFilter> ();
            if (meshFilter != null)
            {
                meshFilter.mesh = mesh;
            }
        }

        ParticleSystem particleSystem = goBehaviour.GetComponent<ParticleSystem> ();
        if (particleSystem != null)
        {
            ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
            shapeModule.mesh = mesh;
        }

        Attackable attackable = goBehaviour.GetComponentInParent<Attackable> ();
        if (attackable != null)
        {
            if (attackable is IColouringSpellBehaviour spellBehaviour)
                spellBehaviour.Init (_turnBasedCombat, _drawer.LastStrokeDrawUVs, this);
        }
        else
        {
            CombatEnvironnementHazard combatEnvironnementHazard = goBehaviour.GetComponentInParent<CombatEnvironnementHazard> ();
            if (combatEnvironnementHazard != null)
            {
                if (combatEnvironnementHazard is IColouringSpellBehaviour spellBehaviour)
                    spellBehaviour.Init (_turnBasedCombat, _drawer.LastStrokeDrawUVs, this);
            }

            _turnBasedCombat.ActivePlayerCharacter.LinkedCombatEntities.Add (combatEnvironnementHazard);
        }

        Destroy (advancedPolyCol.gameObject);
    }

    private ComputeShader _retrievePixelsByColorIdCs;
    private bool TryGenerateSpriteFromFrame (ColouringSpell colouring, out Sprite sprite)
    {
        sprite = null;
        int id = colouring.Id;

        if (_retrievePixelsByColorIdCs == null)
        {
            _retrievePixelsByColorIdCs = Resources.Load<ComputeShader> ("RetrievePixelsByColorId");
            if (_retrievePixelsByColorIdCs == null)
            {
                Debug.LogError (nameof (_retrievePixelsByColorIdCs) + "null, it was not loaded (not found?)");
                return false;
            }
        }

        int kernel;

        kernel = _retrievePixelsByColorIdCs.FindKernel ("RetrieveTexPixels");

        // Input Color usages on frame
        ComputeBuffer colorIdsBuffer = new ComputeBuffer (PixelIds.Length, sizeof (int));
        colorIdsBuffer.SetData (PixelIds);
        _retrievePixelsByColorIdCs.SetBuffer (kernel, "ColorIds", colorIdsBuffer);

        // Input Width
        int width = Width;
        int height = Height;

        _retrievePixelsByColorIdCs.SetInt ("Width", width);

        // Input ColorId
        _retrievePixelsByColorIdCs.SetInt ("ColorId", id);

        if (colouring.Texture != null)
        {
            _retrievePixelsByColorIdCs.SetTexture (kernel, "ColorTexture", colouring.Texture);
            _retrievePixelsByColorIdCs.SetInt ("UseColorTex", 1);
            _retrievePixelsByColorIdCs.SetInt ("ColorTexWidth", colouring.Texture.width);
            _retrievePixelsByColorIdCs.SetInt ("ColorTexHeight", colouring.Texture.height);
        }
        else
        {
            _retrievePixelsByColorIdCs.SetInt ("UseColorTex", 0);
        }

        // set pixels buffer
        ComputeBuffer pixelMatchesBuffer = new ComputeBuffer (width * height, sizeof (float) * 4);
        _retrievePixelsByColorIdCs.SetBuffer (kernel, "PixelMatches", pixelMatchesBuffer);

        ComputeBuffer pixelFoundBuffer = new ComputeBuffer (1, sizeof (int));
        pixelFoundBuffer.SetData (new int[] { 0 });
        _retrievePixelsByColorIdCs.SetBuffer (kernel, "PixelFound", pixelFoundBuffer);

        // Run CS
        int x = GraphicUtils.GetComputeShaderDispatchCount (width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (height, 32);
        int z = 1;

        _retrievePixelsByColorIdCs.Dispatch (kernel, x, y, z);

        // release buffers
        colorIdsBuffer.Release ();

        // Get pixels by index, create texture and Sprite
        int[] pixelFound = new int[1];
        pixelFoundBuffer.GetData (pixelFound);
        pixelFoundBuffer.Release ();

        Color[] pixelMatches = new Color[width * height];
        pixelMatchesBuffer.GetData (pixelMatches);
        pixelMatchesBuffer.Release ();

        // GraphicUtils.SavePixelsAsPNG (pixelMatches, "Test/test.png", width, height);
        // GraphicUtils.SavePixelsAsPNG (DrawTexture.GetPixels (), "Test/frame.png", width, height);

        if (pixelFound[0] == 0)
        {
            // Debug.LogError ("No pixel found for color id " + id + " on frame " + frame.name);
            return false;
        }

        Texture2D tex = new Texture2D (width, height, TextureFormat.RGBA32, 0, false);
        tex.SetPixels (pixelMatches);
        tex.Apply ();

        // Create sprite
        Rect rect = new Rect (0, 0, width, height);
        sprite = Sprite.Create (tex, rect, new Vector2 (0.5f, 0.5f), 100, 0, SpriteMeshType.Tight);
        sprite.texture.filterMode = FilterMode.Point;

        return true;
    }
}