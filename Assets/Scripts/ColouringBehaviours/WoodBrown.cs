using UnityEngine;

public class WoodBrown : Attackable
{
    private Vector2 _initialPos;

    [SerializeField]
    private float _fallDownSpeed = 0.1f;
    private bool _fallDown = true;
    private Rigidbody _rigidbody;

    private const float KG_PER_PIXEL = 0.3f;

    protected override void Awake ()
    {
        base.Awake ();
        _rigidbody = GetComponent<Rigidbody> ();
        _initialPos = transform.position;
    }

    protected override void Start ()
    {
        base.Start ();

        ComputeShader countPixCs = Resources.Load<ComputeShader> ("CountOpaquePixels");
        if (countPixCs == null)
        {
            Debug.LogError (nameof (countPixCs) + "null, it was not loaded (not found?)");
            return;
        }

        int kernel = countPixCs.FindKernel ("CSMain");

        countPixCs.SetTexture (kernel, "Tex", _renderer.sprite.texture);

        ComputeBuffer countBuf = new ComputeBuffer (1, sizeof (int));
        countBuf.SetData (new int[] { 0 });
        countPixCs.SetBuffer (kernel, "Count", countBuf);

        int x = GraphicUtils.GetComputeShaderDispatchCount (_renderer.sprite.texture.width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (_renderer.sprite.texture.height, 32);
        int z = 1;
        countPixCs.Dispatch (kernel, x, y, z);

        int[] count = new int[1];
        countBuf.GetData (count);
        countBuf.Release ();
        int pixelCount = count[0];
        //Set life
        _maxLife = (int) ((float) pixelCount * 0.2f);
        _currentLife = _maxLife;

        // Set kilogram
        Stats.Kilogram = pixelCount * KG_PER_PIXEL;
    }

    protected override void Update ()
    {
        base.Update ();
    }

    private void FixedUpdate ()
    {
        // if (_fallDown)
        // {
        //     transform.position = transform.position + Vector3.down * _fallDownSpeed;
        // }
    }

    private void OnCollisionEnter (Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer ("Map"))
        {
            _fallDown = false;
            // _rigidbody.useGravity = true;
        }
    }
}