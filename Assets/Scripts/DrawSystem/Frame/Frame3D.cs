using UnityEngine;

[RequireComponent (typeof (MeshFilter), typeof (MeshRenderer))]
public class Frame3D : Frame
{
    [SerializeField]
    private bool _generateDrawingTextureFromDimension = false;

    [SerializeField]
    private Vector2Int _dimension = new Vector2Int (512, 512);

    protected MeshRenderer _renderer;

    protected override void Awake ()
    {
        base.Awake ();

        _renderer = GetComponent<MeshRenderer> ();
        _mat = GetComponent<MeshRenderer> ().material;

        if (_generateDrawingTextureFromDimension)
            _drawTexture = Utils.GetUniqueTransparentTex (_dimension);
        else
            _drawTexture = Utils.GetUniqueTransparentTex (new Vector2Int (_mat.mainTexture.width, _mat.mainTexture.height));

        _width = _mat.mainTexture.width;
        _height = _mat.mainTexture.height;
        _pixelIds = new int[_width * _height];
        _pixelUsages = new int[_width * _height];
        _pixelTimestamps = new int[Width * Height];

        _initialPixels = new Color[_width * _height];

        _mat.SetTexture ("_DrawTex", _drawTexture);
        _mat.SetFloat ("Width", _width);
        _mat.SetFloat ("Height", _height);
    }

    public override void UpdateBrushDrawingPrediction (Vector2 uv)
    {
        _mat.SetInt ("MousePosX", (int) (uv.x * _width));
        _mat.SetInt ("MousePosY", (int) (uv.y * _height));
    }

    protected override Vector2Int GetMousePosFrameSpace (Vector2 uv)
    {
        return new Vector2Int ((int) (uv.x * _width), (int) (uv.y * _height));
    }

    public override Vector3 FrameSpaceToWorldSpace (Vector2 uv)
    {
        Bounds b = _renderer.bounds;
        Vector3 worldPosition = b.center;
        worldPosition += b.extents.x * -transform.right;
        worldPosition += b.extents.y * -transform.up;

        worldPosition += transform.right * uv.x * b.size.x / _width;
        worldPosition += transform.up * uv.y * b.size.y / _height;

        return worldPosition;
    }
}