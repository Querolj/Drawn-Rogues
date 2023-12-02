using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRuby.LightningBolt;
using UnityEngine;

public class StaticCloud : CombatEnvironnementHazard, IColouringSpellBehaviour
{
    private TurnManager _turnManager;

    [SerializeField]
    private LightningBoltScript _lightningBoltTemplate;

    private LightningBoltScript _lightningBolt;

    private LineRenderer _lineRenderer;

    [SerializeField]
    private const float _LIGHTING_BOLT_DURATION = 0.5f;

    private float _lightingBoltTimeLeft = 0.5f;

    [SerializeField]
    private Attack _attack;

    [SerializeField]
    private Texture2D[] _cloudTexs;

    [SerializeField]
    private Color _topCloudColor;

    [SerializeField]
    private Color _middleCloudColor;

    [SerializeField]
    private Color _bottomCloudColor;

    private SpriteRenderer _renderer;

    private List < (GameObject, Attackable) > _targetsDetected = new List < (GameObject, Attackable) > ();
    private Stack < (GameObject, Attackable) > _targetsToHit = new Stack < (GameObject, Attackable) > ();

    private Action _onTurnEnded;
    private float _damageMultiplier = 1f;

    private float _xMinBounds = float.MaxValue, _xMaxBounds = float.MinValue;

    public void Init (TurnManager turnManager, List<Vector2> lastStrokeDrawUVs, FrameDecor frameDecor, Action onInitDone = null)
    {
        _renderer = GetComponent<SpriteRenderer> ();
        _lightningBolt = Instantiate (_lightningBoltTemplate);
        _lightningBolt.transform.SetParent (transform);
        Vector3 locPos = Vector3.zero;
        locPos.z = 0.001f;
        _lightningBolt.transform.localPosition = locPos;
        _lineRenderer = _lightningBolt.GetComponent<LineRenderer> ();
        _lineRenderer.enabled = false;
        _turnManager = turnManager;

        List<int> initialIndexList = new List<int> ();
        for (int i = 0; i < _cloudTexs.Length; i++)
        {
            initialIndexList.Add (i);
        }

        int lastIndexSelected = -1;
        Vector3 lastLocalPos = Vector3.zero;

        Texture2D drawedTex = new Texture2D (frameDecor.Width, frameDecor.Height, TextureFormat.RGBA32, false);
        drawedTex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[frameDecor.Width * frameDecor.Height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        drawedTex.SetPixels (pixels);
        drawedTex.Apply ();

        ComputeShader drawCloudCs = Resources.Load<ComputeShader> ("DrawCloud");
        if (drawCloudCs == null)
        {
            Debug.LogError (nameof (drawCloudCs) + "null, it was not loaded (not found?)");
            return;
        }

        int kernel = drawCloudCs.FindKernel ("Draw");

        RenderTexture rendTex = new RenderTexture (drawedTex.width, drawedTex.height, 0, RenderTextureFormat.ARGB32);
        rendTex.filterMode = FilterMode.Point;
        rendTex.enableRandomWrite = true;
        Graphics.Blit (drawedTex, rendTex);
        drawCloudCs.SetTexture (kernel, "DrawnTex", rendTex);

        drawCloudCs.SetVector ("TopColor", _topCloudColor);
        drawCloudCs.SetVector ("MiddleColor", _middleCloudColor);
        drawCloudCs.SetVector ("BottomColor", _bottomCloudColor);

        const float minDistanceBetweenClouds = 0.075f;

        // spawn cloud randomly in the frame
        Vector2 uvTotal = Vector2.zero;
        float lowestYuv = float.MaxValue;

        int x = GraphicUtils.GetComputeShaderDispatchCount (frameDecor.Width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (frameDecor.Height, 32);
        int z = 1;

        foreach (Vector2 uv in lastStrokeDrawUVs)
        {
            Vector3 localPos = uv - new Vector2 (0.5f, 0.5f);
            localPos.x *= frameDecor.Bounds.extents.x * 2f;
            localPos.y *= frameDecor.Bounds.extents.y * 2f;
            localPos.z = -0.001f;

            lowestYuv = Mathf.Min (lowestYuv, uv.y);

            if (lastLocalPos != Vector3.zero)
            {
                float distance = Vector3.Distance (lastLocalPos, localPos);
                if (distance < minDistanceBetweenClouds)
                {
                    continue;
                }
            }

            uvTotal += uv;
            lastLocalPos = localPos;

            List<int> currentIndexList = new List<int> (initialIndexList);
            int index = 0;
            if (lastIndexSelected != -1)
            {
                currentIndexList.Remove (lastIndexSelected);
            }
            index = currentIndexList[UnityEngine.Random.Range (0, currentIndexList.Count)];
            lastIndexSelected = index;

            Texture2D tex = _cloudTexs[index];

            drawCloudCs.SetInt ("TexToApplyWidth", tex.width);
            drawCloudCs.SetInt ("TexToApplyHeight", tex.height);
            drawCloudCs.SetTexture (kernel, "TexToApply", tex);

            drawCloudCs.SetInt ("DrawOriginX", (int) (uv.x * frameDecor.Width) - (tex.width / 2));
            drawCloudCs.SetInt ("DrawOriginY", (int) (uv.y * frameDecor.Height) - (tex.height / 2));

            drawCloudCs.Dispatch (kernel, x, y, z);

            // Create Trigger with a collider
            GameObject triggerGo = new GameObject ("Trigger");

            // Add trigger collider
            BoxCollider boxCollider = triggerGo.AddComponent<BoxCollider> ();
            Vector3 boxSize = new Vector3 (tex.width / 100f, 5f, 0.25f);
            boxCollider.size = boxSize;
            Vector3 boxCenter = Vector3.zero;
            boxCenter.y = -(boxSize.y / 2f);
            boxCollider.center = boxCenter;
            boxCollider.isTrigger = true;

            float minBounds = transform.TransformPoint (localPos - boxCollider.bounds.extents).x;
            if (_xMinBounds > minBounds)
                _xMinBounds = minBounds;

            float maxBounds = transform.TransformPoint (localPos + boxCollider.bounds.extents).x;
            if (_xMaxBounds < maxBounds)
                _xMaxBounds = maxBounds;

            Trigger trigger = triggerGo.AddComponent<Trigger> ();
            if (trigger == null)
                throw new System.ArgumentNullException (nameof (trigger));

            triggerGo.transform.SetParent (transform);
            triggerGo.transform.localPosition = localPos;
            trigger.OnDetect += (collider) =>
            {
                OnTriggerDetect (trigger.gameObject, collider);
            };

            Vector3 worldPosOffsetted = trigger.transform.position;
            worldPosOffsetted.z -= uv.y * 0.01f;
            triggerGo.transform.position = worldPosOffsetted;

            Vector3 triggerLocalPos = trigger.transform.localPosition;
        }

        RenderTexture.active = rendTex;
        drawedTex.ReadPixels (new Rect (0, 0, drawedTex.width, drawedTex.height), 0, 0);
        drawedTex.Apply ();

        // GraphicUtils.SavePixelsAsPNG (drawedTex.GetPixels (), "Test/statiCloud.png", drawedTex.width, drawedTex.height);

        Vector4 borderInPixel = GraphicUtils.GetTextureBorder (drawedTex);
        Vector2 texSize = new Vector2 (borderInPixel.z - borderInPixel.x, borderInPixel.w - borderInPixel.y);
        _renderer.sprite = Sprite.Create (drawedTex, new Rect (0, 0, drawedTex.width, drawedTex.height), new Vector2 (0.5f, 0.5f));
        _renderer.material.mainTexture = drawedTex;

        // Calculate damage multiplier, the more thick the cloud is, the more damage it does
        ComputeShader getThicknessCs = Resources.Load<ComputeShader> ("GetThickness");

        if (getThicknessCs == null)
        {
            Debug.LogError (nameof (getThicknessCs) + "null, it was not loaded (not found?)");
            return;
        }

        int kernelGetThickness = getThicknessCs.FindKernel ("GetThickness");

        getThicknessCs.SetTexture (kernelGetThickness, "Tex", frameDecor.DrawTexture);
        ComputeBuffer thicknessBuffer = new ComputeBuffer (frameDecor.DrawTexture.width, sizeof (uint));
        uint[] thickness = new uint[frameDecor.DrawTexture.width];
        thicknessBuffer.SetData (thickness);

        getThicknessCs.SetBuffer (kernelGetThickness, "ThicknessBuffer", thicknessBuffer);
        getThicknessCs.SetInt ("MinX", (int) borderInPixel.x);
        getThicknessCs.SetInt ("MaxX", (int) borderInPixel.z);

        getThicknessCs.Dispatch (kernelGetThickness, x, y, z);
        thicknessBuffer.GetData (thickness);

        uint averageThickness = 0;
        for (int i = 0; i < thickness.Length; i++)
        {
            averageThickness += thickness[i];
        }

        averageThickness /= (uint) texSize.x;

        _damageMultiplier = (float) averageThickness / 14f; // 14f is the average thickness of the brush used
        _damageMultiplier = Mathf.Clamp (_damageMultiplier, 1f, _damageMultiplier);

        onInitDone?.Invoke ();
    }

    private void Update ()
    {
        if (_lightningBolt.enabled)
        {
            _lightingBoltTimeLeft -= Time.deltaTime;
            _renderer.material.SetFloat ("_LightingDuration", _lightingBoltTimeLeft);
        }
    }

    private void FilterTargetsOutsideOfBounds ()
    {
        _targetsDetected = _targetsDetected.Where (t => t.Item2.transform.position.x > _xMinBounds && t.Item2.transform.position.x < _xMaxBounds).ToList ();
    }

    public override void ExecuteTurn (Action onTurnEnded)
    {
        FilterTargetsOutsideOfBounds ();

        if (_targetsDetected.Count > 0)
        {
            _onTurnEnded = onTurnEnded ??
                throw new ArgumentNullException (nameof (onTurnEnded));

            _lightingBoltTimeLeft = _LIGHTING_BOLT_DURATION;
            _targetsToHit = new Stack < (GameObject, Attackable) > (_targetsDetected);
            StartCoroutine (ChainLightingBolt ());
        }
        else
        {
            onTurnEnded ();
        }
    }

    private IEnumerator ChainLightingBolt ()
    {
        if (_targetsToHit.Count > 0)
        {
            CastLightingBolt ();
            yield return new WaitForSeconds (_LIGHTING_BOLT_DURATION);
            StartCoroutine (ChainLightingBolt ());
        }
        else
        {
            _lightningBolt.enabled = false;
            _lineRenderer.enabled = false;
            _onTurnEnded ();
        }
    }

    private void CastLightingBolt ()
    {
        (GameObject go, Attackable target) = _targetsToHit.Pop ();
        if (target.WillBeDestroyed)
            return;

        _lightningBolt.enabled = true;
        _lineRenderer.enabled = true;

        _lightningBolt.StartObject = go;
        _lightningBolt.EndObject = target.gameObject;
        
        // TODO : Create a factory for spells, to be able to use injected stuff
        // AttackInstance attackInstance = AttackInstFactory.Create (_attack, _turnManager.ActivePlayerCharacter);

        // attackInstance.MinDamage = (int) (_attack.MinDamage * _damageMultiplier);
        // attackInstance.MaxDamage = (int) (_attack.MaxDamage * _damageMultiplier);

        // attackInstance.Execute (_turnManager.ActivePlayerCharacter, target, target.GetSpriteBounds ().center, () => { });

        _renderer.material.SetVector ("_LightStartPos", go.transform.position + Vector3.down * 0.3f);
    }

    private HashSet<int> _triggeredAttackableGoIds = new HashSet<int> ();
    private void OnTriggerDetect (GameObject cloud, Collider other)
    {
        int otherGoId = other.gameObject.GetInstanceID ();
        if (other.tag == "Player" || _triggeredAttackableGoIds.Contains (otherGoId))
            return;

        Attackable target = other.GetComponent<Attackable> ();
        if (target == null)
            throw new System.ArgumentNullException (nameof (target));
        _triggeredAttackableGoIds.Add (otherGoId);
        _targetsDetected.Add ((cloud, target));
    }
}