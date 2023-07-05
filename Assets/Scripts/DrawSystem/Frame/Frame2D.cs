using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public struct VerticalArea
{
    public int UpperLimit;
    public int LowerLimit;
}

[RequireComponent (typeof (SpriteRenderer))]
public class Frame2D : Frame
{
    public struct Corners
    {
        public Vector2 TopRight;
        public Vector2 BottomLeft;
    }

    private SpriteRenderer _renderer;
    public Bounds Bounds
    {
        get { return _renderer.bounds; }
    }

    private Camera _mainCamera;
    private Corners _screenSpaceFrameCorners;

    protected override void Awake ()
    {
        base.Awake ();

        _renderer = GetComponent<SpriteRenderer> ();
        _width = _renderer.sprite.texture.width;
        _height = _renderer.sprite.texture.height;

        Texture2D copy = GraphicUtils.GetTextureCopy (_renderer.sprite.texture);
        copy.filterMode = FilterMode.Point;
        _renderer.sprite = Sprite.Create (copy, _renderer.sprite.rect, Vector2.one * 0.5f, _renderer.sprite.pixelsPerUnit, 1, SpriteMeshType.Tight, _renderer.sprite.border, true);
        _drawTexture = copy;

        _initialPixels = _renderer.sprite.texture.GetPixels ();

        _mat = _renderer.material;
        _mat.SetInt ("Width", Width);
        _mat.SetInt ("Height", Height);
        _mat.SetTexture ("_MainTex", _renderer.sprite.texture);

        _mainCamera = Camera.main;
        _pixelIds = new int[Width * Height];
        _pixelUsages = new int[Width * Height];
        _pixelTimestamps = new int[Width * Height];

        CalculateScreenSpaceCorner ();
    }

    public override void UpdateBrushDrawingPrediction (Vector2 mouseScreenPos)
    {
        Vector2Int mousePosFrameSpace = WorldSpaceToFrameSpace (_mainCamera.ScreenToWorldPoint (mouseScreenPos));

        _mat.SetInt ("MousePosX", mousePosFrameSpace.x);
        _mat.SetInt ("MousePosY", mousePosFrameSpace.y);
    }

    protected override Vector2Int GetMousePosFrameSpace (Vector2 coordinate)
    {
        return WorldSpaceToFrameSpace (_mainCamera.ScreenToWorldPoint (coordinate));
    }

    public Vector2Int WorldSpaceToFrameSpace (Vector3 position)
    {
        position.z = 0;

        Bounds b = _renderer.sprite.bounds;
        Vector2 scaledExtents = new Vector2 (b.extents.x, b.extents.y) * transform.localScale;

        Vector3 bottomLeftW = transform.position + new Vector3 (-scaledExtents.x, -scaledExtents.y, 0);
        Vector3 TopRightW = transform.position + new Vector3 (scaledExtents.x, scaledExtents.y, 0);
        float distX = Mathf.Abs (TopRightW.x - bottomLeftW.x);
        float propX = (position.x - bottomLeftW.x) / distX;

        float distY = Mathf.Abs (TopRightW.y - bottomLeftW.y);
        float propY = (position.y - bottomLeftW.y) / distY;
        Vector2 frameSpacePos = new Vector2 (propX * Width, propY * Height);

        return new Vector2Int ((int) frameSpacePos.x, (int) frameSpacePos.y);
    }

    public override Vector3 FrameSpaceToWorldSpace (Vector2 uv)
    {
        Vector2 framePos = new Vector2 (uv.x * Width, uv.y * Height);
        Bounds b = _renderer.sprite.bounds;
        Vector2 scaledExtents = new Vector2 (b.extents.x, b.extents.y) * transform.localScale;

        Vector3 bottomLeftW = transform.position + new Vector3 (-scaledExtents.x, -scaledExtents.y, 0);
        Vector3 TopRightW = transform.position + new Vector3 (-scaledExtents.x, scaledExtents.y, 0);

        float propX = (float) framePos.x / (float) Width;
        float propY = (float) framePos.y / (float) Height;

        Vector3 worldPosition = bottomLeftW;
        worldPosition.x += Mathf.Abs (TopRightW.x - bottomLeftW.x) * propX;
        worldPosition.y += Mathf.Abs (TopRightW.y - bottomLeftW.y) * propY;

        return worldPosition;
    }

    private void CalculateScreenSpaceCorner ()
    {
        Bounds b = _renderer.sprite.bounds;
        Vector2 scaledExtends = new Vector2 (b.extents.x, b.extents.y) * transform.localScale;
        _screenSpaceFrameCorners.TopRight = _mainCamera.WorldToScreenPoint (transform.position + new Vector3 (scaledExtends.x, scaledExtends.y, 0));
        _screenSpaceFrameCorners.BottomLeft = _mainCamera.WorldToScreenPoint (transform.position - new Vector3 (scaledExtends.x, scaledExtends.y, 0));
    }

    public bool CheckPixeUsageInBrush (Vector3 screenPosition, HashSet<PixelUsage> disallowedColorUsages, HashSet<PixelUsage> neededColorUsages, Brush brush)
    {
        Vector2Int mousePosFrameSpace = WorldSpaceToFrameSpace (_mainCamera.ScreenToWorldPoint (screenPosition));

        const float epsilon = 0.0001f;
        bool neededColorUsageFound = false;
        for (int i = 0; i < brush.Texture.width; i++)
        {
            for (int j = 0; j < brush.Texture.height; j++)
            {
                int brushIndex = (j * brush.Texture.width) + i;
                if (brush.Pixels[brushIndex].a <= epsilon)
                    continue;

                int x = i + mousePosFrameSpace.x - (brush.Texture.width / 2);
                int y = j + mousePosFrameSpace.y - (brush.Texture.height / 2);

                int frameIndex = (y * Width) + x;
                if (frameIndex >= _pixelUsages.Length)
                    continue;

                PixelUsage cu = (PixelUsage) _pixelUsages[frameIndex];

                if (disallowedColorUsages.Contains (cu))
                    return false;

                if (!neededColorUsageFound)
                    neededColorUsageFound = neededColorUsages.Contains (cu);
            }
        }

        return neededColorUsageFound;
    }
}