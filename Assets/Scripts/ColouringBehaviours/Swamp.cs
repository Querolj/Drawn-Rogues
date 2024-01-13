using System;
using System.Collections.Generic;
using DigitalRuby.AdvancedPolygonCollider;
using UnityEngine;

public class Swamp : MonoBehaviour, ICombatEnvironnementHazard, IColouringSpellBehaviour
{
    [SerializeField]
    private AdvancedPolygonCollider _advancedPolygonColliderTemplate;

    private const float _MESH_COLLIDER_THICKNESS = 0.03f;

    private SpriteRenderer _renderer;
    private ComputeShader _drawCollapseCs;
    private Texture2D _drawCollapseTexture;

    private const float _updateTime = 0.01f;
    private float _updateTimer = 0f;

    private FrameDecor _frameDecor;
    private Action _onInitDone;

    public void Init (TurnManager bg, List<Vector2> lastStrokeDrawUVs, FrameDecor frameDecor, Action onInitDone = null)
    {
        _renderer = GetComponent<SpriteRenderer> ();
        _frameDecor = frameDecor ??
            throw new ArgumentNullException (nameof (frameDecor));
        _onInitDone = onInitDone;

        // Init CS
        _drawCollapseCs = Resources.Load<ComputeShader> ("DrawCollapse");
        if (_drawCollapseCs == null)
        {
            Debug.LogError (nameof (_drawCollapseCs) + "null, it was not loaded (not found?)");
            return;
        }
        _drawCollapseCs.SetInt ("Width", frameDecor.Width);
        _drawCollapseCs.SetInt ("Height", frameDecor.Height);

        // get left & right limits
        Vector4 border = GraphicUtils.GetTextureBorder (frameDecor.DrawTexture);
        Vector3 bottomLeft = new Vector2 (border.x, border.y);
        bottomLeft /= 100f;
        bottomLeft += frameDecor.transform.position;
        bottomLeft -= frameDecor.Bounds.extents;

        Vector3 topRight = new Vector2 (border.z, border.w);
        topRight /= 100f;
        topRight += frameDecor.transform.position;
        topRight -= frameDecor.Bounds.extents;

        // Debug.Log (bottomLeft.ToString ("F3") + " : " + topRight.ToString ("F3"));

        // cast raycast from left to right
        int drawWidth = Mathf.RoundToInt (border.z - border.x + 1);
        int drawHeight = Mathf.RoundToInt (border.w - border.y + 1);

        float[] heights = new float[drawWidth];
        float lowestHeightHit = float.MaxValue;
        for (int i = 0; i < drawWidth; i++)
        {
            Vector3 start = new Vector3 (bottomLeft.x + i * 0.01f, bottomLeft.y, frameDecor.transform.position.z);

            if (Physics.Raycast (start, Vector3.down, out RaycastHit hit, 100f, LayerMask.GetMask ("Map")))
            {
                Debug.DrawLine (start, hit.point, Color.red, 100f);
                heights[i] = hit.point.y;

                if (heights[i] < lowestHeightHit)
                {
                    lowestHeightHit = hit.point.y;
                }
            }
        }

        int[] heightsInPixel = new int[drawWidth];
        for (int i = 0; i < heights.Length; i++)
        {
            heightsInPixel[i] = (int) ((heights[i] - lowestHeightHit) * 100f);
        }

        ComputeBuffer heightLimitsBuffer = new ComputeBuffer (drawWidth, sizeof (int));
        heightLimitsBuffer.SetData (heightsInPixel);
        _drawCollapseCs.SetBuffer (0, "HeightLimits", heightLimitsBuffer);

        // generate the texture needed
        float worldHeight = topRight.y - lowestHeightHit;
        int height = (int) (worldHeight * 100f);

        _drawCollapseTexture = new Texture2D (drawWidth, height);
        Color[] clear = new Color[drawWidth * height];
        _drawCollapseTexture.SetPixels (clear);
        _renderer.sprite = Sprite.Create (_drawCollapseTexture, new Rect (0, 0, drawWidth, height), new Vector2 (0.5f, 0.5f));

        Debug.Log (frameDecor.DrawTexture.width + " : " + frameDecor.DrawTexture.height);
        Debug.Log (drawWidth + " : " + drawHeight);
        Debug.Log (border.x + " : " + border.y);

        Color[] drawPixels = frameDecor.DrawTexture.GetPixels ((int) border.x, (int) border.y, drawWidth, drawHeight);
        _drawCollapseTexture.SetPixels (0, _drawCollapseTexture.height - drawHeight, drawWidth, drawHeight, drawPixels);
        _drawCollapseTexture.filterMode = FilterMode.Point;
        _drawCollapseTexture.Apply ();

        Vector3 newPos = Vector3.Lerp (bottomLeft, topRight, 0.5f);
        newPos.y -= worldHeight * 0.5f;
        newPos.y += drawHeight * 0.5f * 0.01f;
        transform.position = newPos;
    }

    private bool _collapseEnded = false;
    private void Update ()
    {
        if (_drawCollapseCs == null || _collapseEnded)
            return;

        _updateTimer += Time.deltaTime;
        if (_updateTimer >= _updateTime)
        {
            _updateTimer = 0f;
            _collapseEnded = CollapseDrawUpdate ();
            if (_collapseEnded)
            {
                GenerateCollider ();
                _onInitDone?.Invoke ();
            }
        }
    }

    private void GenerateCollider ()
    {
        AdvancedPolygonCollider advancedPolyCol = GameObject.Instantiate (_advancedPolygonColliderTemplate);
        advancedPolyCol.transform.SetParent (transform);
        SpriteRenderer spriteRend = advancedPolyCol.GetComponent<SpriteRenderer> ();
        spriteRend.sprite = _renderer.sprite;
        spriteRend.material.mainTexture = _renderer.sprite.texture;

        advancedPolyCol.RecalculatePolygon ();
        // PolygonCollider2D polyCol = advancedPolyCol.GetComponent<PolygonCollider2D> ();

        // Mesh mesh = ExtrudeSprite.CreateMesh (polyCol.points, frontDistance: -_MESH_COLLIDER_THICKNESS, backDistance : _MESH_COLLIDER_THICKNESS);

        // MeshCollider meshCol = GetComponent<MeshCollider> ();
        // meshCol.sharedMesh = mesh;
    }

    private bool CollapseDrawUpdate ()
    {
        int kernel = _drawCollapseCs.FindKernel ("Collapse");

        //Set texture
        RenderTexture rendTex = new RenderTexture (_drawCollapseTexture.width, _drawCollapseTexture.height, 0, RenderTextureFormat.ARGB32);
        rendTex.filterMode = FilterMode.Point;
        rendTex.enableRandomWrite = true;
        Graphics.Blit (_drawCollapseTexture, rendTex);
        _drawCollapseCs.SetTexture (kernel, "DrawnTex", rendTex);

        // Check if at least one pixel moved
        ComputeBuffer hasMovedBuffer = new ComputeBuffer (1, sizeof (int));
        hasMovedBuffer.SetData (new int[] { 0 });
        _drawCollapseCs.SetBuffer (kernel, "HasPixelMoved", hasMovedBuffer);

        int x = GraphicUtils.GetComputeShaderDispatchCount (_frameDecor.Width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (_frameDecor.Height, 32);
        int z = 1;

        _drawCollapseCs.Dispatch (kernel, x, y, z);

        int[] hasMoved = new int[1];
        hasMovedBuffer.GetData (hasMoved);
        if (hasMoved[0] == 0)
        {
            hasMovedBuffer.Release ();
            return true;
        }

        hasMovedBuffer.Release ();
        RenderTexture.active = rendTex;
        _drawCollapseTexture.ReadPixels (new Rect (0, 0, _drawCollapseTexture.width, _drawCollapseTexture.height), 0, 0);
        _drawCollapseTexture.Apply ();

        return false;
    }

    #region ICombatEnvironnementHazard
    public void ExecuteTurn (Action onTurnEnded)
    {
        onTurnEnded?.Invoke ();
    }

    public List<ICombatEntity> GetLinkedCombatEntities()
    {
        throw new NotImplementedException();
    }

    public int GetTurnOrder()
    {
        throw new NotImplementedException();
    }
    #endregion ICombatEnvironnementHazard
}