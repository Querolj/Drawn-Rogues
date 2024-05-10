using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRuby.LightningBolt;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class StaticCloud : CombatEnvironnementHazard, IColouringSpellBehaviour
{
    #region serialized fields
    [SerializeField]
    private LightningBoltScript _lightningBoltTemplate;

    [SerializeField]
    private const float _LIGHTING_BOLT_DURATION = 0.75f;

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

    [SerializeField]
    ComputeShader _drawCloudCs;

    [SerializeField]
    ComputeShader _getVisiblePixelCountCs;

    [SerializeField]
    private float _pixelTransitionTimePerFrame = 0.3f;

    [SerializeField]
    private float _colorTransitionTime = 1.5f;
    #endregion

    #region private fields
    private float _lightingBoltTimeLeft = 0.5f;
    private LightningBoltScript _lightningBolt;
    private LineRenderer _lineRenderer;
    private SpriteRenderer _renderer;
    private List < (GameObject, Attackable) > _targetsDetected = new List < (GameObject, Attackable) > ();
    private Stack < (GameObject, Attackable) > _targetsToHit = new Stack < (GameObject, Attackable) > ();
    private Action _onTurnEnded;
    private float _damageMultiplier = 1f;
    private float _xMinBounds = float.MaxValue, _xMaxBounds = float.MinValue;
    private TurnManager _turnManager;
    private Action _onInitDone;
    private FrameDecor _frameDecor;
    private float _averageCloudTickness;
    #endregion  

    #region Injected
    private AttackInstance.Factory _attackInstanceFactory;
    private TextureTransition _textureTransition;
    #endregion

    [Inject, UsedImplicitly]
    private void Init (AttackInstance.Factory attackInstanceFactory, TextureTransition textureTransition)
    {
        _attackInstanceFactory = attackInstanceFactory;
        _textureTransition = textureTransition;
        _renderer = GetComponent<SpriteRenderer> ();

        float cloudTicknessTotal = 0;
        // TODO : we should get the number of pixels for cloud instead of their height
        foreach (Texture2D tex in _cloudTexs)
        {
            cloudTicknessTotal += tex.height * 0.45f; // 0.6f is a arbitrary number to get the cloud total pixel count
        }

        _averageCloudTickness = cloudTicknessTotal / _cloudTexs.Length;
    }

    public void Init (TurnManager turnManager, List<Vector2> lastStrokeDrawUVs, FrameDecor frameDecor, Action onInitDone = null)
    {
        _frameDecor = frameDecor;
        _onInitDone = onInitDone;
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

        Texture2D textureWithClouds = new Texture2D (_frameDecor.Width, _frameDecor.Height, TextureFormat.RGBA32, false, true);
        textureWithClouds.filterMode = FilterMode.Point;
        Color[] pixels = new Color[_frameDecor.Width * _frameDecor.Height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        textureWithClouds.SetPixels (pixels);
        textureWithClouds.Apply ();

        if (_drawCloudCs == null)
        {
            Debug.LogError (nameof (_drawCloudCs) + "null, it was not loaded (not found?)");
            return;
        }

        int kernel = _drawCloudCs.FindKernel ("Draw");

        RenderTexture rendTex = new RenderTexture (textureWithClouds.width, textureWithClouds.height, 0, RenderTextureFormat.ARGB32);
        rendTex.filterMode = FilterMode.Point;
        rendTex.enableRandomWrite = true;
        Graphics.Blit (textureWithClouds, rendTex);
        _drawCloudCs.SetTexture (kernel, "DrawnTex", rendTex);

        _drawCloudCs.SetVector ("TopColor", _topCloudColor);
        _drawCloudCs.SetVector ("MiddleColor", _middleCloudColor);
        _drawCloudCs.SetVector ("BottomColor", _bottomCloudColor);

        const float minDistanceBetweenClouds = 0.075f;

        // spawn cloud randomly in the frame
        Vector2 uvTotal = Vector2.zero;
        float lowestYuv = float.MaxValue;

        int x = GraphicUtils.GetComputeShaderDispatchCount (_frameDecor.Width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (_frameDecor.Height, 32);
        int z = 1;

        foreach (Vector2 uv in lastStrokeDrawUVs)
        {
            Vector3 localPos = uv - new Vector2 (0.5f, 0.5f);
            localPos.x *= _frameDecor.Bounds.extents.x * 2f;
            localPos.y *= _frameDecor.Bounds.extents.y * 2f;
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

            _drawCloudCs.SetInt ("TexToApplyWidth", tex.width);
            _drawCloudCs.SetInt ("TexToApplyHeight", tex.height);
            _drawCloudCs.SetTexture (kernel, "TexToApply", tex);

            _drawCloudCs.SetInt ("DrawOriginX", (int) (uv.x * _frameDecor.Width) - (tex.width / 2));
            _drawCloudCs.SetInt ("DrawOriginY", (int) (uv.y * _frameDecor.Height) - (tex.height / 2));

            _drawCloudCs.Dispatch (kernel, x, y, z);

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
        textureWithClouds.ReadPixels (new Rect (0, 0, textureWithClouds.width, textureWithClouds.height), 0, 0);
        textureWithClouds.Apply ();
        // RenderTexture.active = null;

        _textureTransition.PlayTransition (_renderer.sprite.texture, textureWithClouds, _pixelTransitionTimePerFrame, _colorTransitionTime, OnTextureTransitionEnd);
    }

    private void OnTextureTransitionEnd (Texture2D finalTexture)
    {
        _renderer.sprite = Sprite.Create (finalTexture, new Rect (0, 0, finalTexture.width, finalTexture.height), new Vector2 (0.5f, 0.5f));

        _renderer.material.mainTexture = finalTexture;

        // Calculate damage multiplier, the more thick the cloud is, the more damage it does
        int kernelGetThickness = _getVisiblePixelCountCs.FindKernel ("GetVisiblePixelCount");

        _getVisiblePixelCountCs.SetTexture (kernelGetThickness, "Tex", finalTexture);
        ComputeBuffer countBuffer = new ComputeBuffer (1, sizeof (uint));
        uint[] countBufferData = new uint[1];
        countBufferData[0] = 0;
        countBuffer.SetData (countBufferData);

        _getVisiblePixelCountCs.SetBuffer (kernelGetThickness, "Count", countBuffer);

        int x = GraphicUtils.GetComputeShaderDispatchCount (_frameDecor.Width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (_frameDecor.Height, 32);
        int z = 1;

        _getVisiblePixelCountCs.Dispatch (kernelGetThickness, x, y, z);
        uint[] totalPixelArray = new uint[1];
        countBuffer.GetData (totalPixelArray);
        uint totalPixel = totalPixelArray[0];
        countBuffer.Release ();

        Vector4 borderInPixel = GraphicUtils.GetTextureBorder (finalTexture);
        uint width = (uint) (borderInPixel.z - borderInPixel.x);
        uint drawThickness = totalPixel / width;

        _damageMultiplier = (float) drawThickness / _averageCloudTickness;
        Debug.Log ("Cloud thickness : " + drawThickness + " average cloud thickness : " + _averageCloudTickness + " damage multiplier : " + _damageMultiplier);
        _damageMultiplier = Mathf.Max (_damageMultiplier, 1f);
        _onInitDone?.Invoke ();
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

        AttackInstance attackInstance = _attackInstanceFactory.Create (_attack, _turnManager.ActivePlayerCharacter);

        attackInstance.MinDamage = (int) (_attack.MinDamage * _damageMultiplier);
        attackInstance.MaxDamage = (int) (_attack.MaxDamage * _damageMultiplier);

        attackInstance.Execute (_turnManager.ActivePlayerCharacter, target, target.GetSpriteBounds ().center, () => { });

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
            return;
        _triggeredAttackableGoIds.Add (otherGoId);
        _targetsDetected.Add ((cloud, target));
    }
}