using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class Branch : CombatEnvironnementHazard, IColouringSpellBehaviour
{
    private const float KG_PER_PIXEL = 0.3f;
    private const int DRAW_STEPS_MAX = 16;
    private int _currentStep = 0;
    private int _lastStep = 0;

    private const float BRANCH_DRAW_TIME = 0.75f;
    private float _currentDrawTime = 0f;
    private int _currentBranchLevel = 0;

    [SerializeField]
    private Brush _branchForkBrush;

    [SerializeField]
    private Color _branchColor;

    [SerializeField]
    private SpriteAnimation _flowerGrowTemplate;

    [SerializeField]
    private Attack _healingAttack;

    [SerializeField]
    private AreaOfEffect2D _healingAreaOfEffectFxRemplate;

    private TurnManager _turnManager;
    private SpriteRenderer _renderer;
    private ComputeShader _drawBranchCs;
    private int _kernelCS;

    private const float MIN_BRANCH_ANGLE = 30f;
    private const float MAX_BRANCH_ANGLE = 65f;

    private const float MIN_BRANCH_PIX_LENGTH = 14f;
    private const float MAX_BRANCH_PIX_LENGTH = 20f;
    private Vector3Int _groupThreadCS;

    private const float FORK_RANGE_MIN = 0.4f; // In % of branch length 
    private const float FORK_RANGE_MAX = 0.6f;

    private Action _onInitDone;
    private List<Attackable> _attackableInRange = new List<Attackable> ();
    private bool _branchTouchingGround = false;
    private FrameDecor _frameDecor;

    private class BranchFork
    {
        private static int _idCount = 0;

        public int Id { get; }

        public Vector2 Origin { get; }
        public Vector2 Direction { get; }
        public float Length { get; }
        public float StartLerpValue = 0f;
        public bool HasForked = false;
        public Vector2Int CurrentPositionInPixel = Vector2Int.zero;
        public Vector2 TipPoint { get; }

        public BranchFork (Vector2 origin, Vector2 direction, float length)
        {
            _idCount++;
            Id = _idCount;
            Origin = origin;
            Direction = direction;
            Length = length;
            TipPoint = Origin + Direction * Length;
        }
    }

    private List<BranchFork> _branchForksLevel1 = new List<BranchFork> ();
    private List<BranchFork> _branchForksLevel2 = new List<BranchFork> ();

    private Texture2D _drawedTex;
    private RenderTexture _rendTex;
    private Color[] _drawedTexInitialPixels;
    private Color[] _frameTexPixels;

    private int[] _branchIds;

    // Flower grow 
    private int _totalFlowerGrown = 0;
    private int _totalFlowerExpected = 0;
    private bool _growFinished = false;
    private Vector2 _bottonLeftFlowerPos;
    private Vector2 _topRightFlowerPos;
    private List<Vector3> _flowerPositions = new List<Vector3> ();

    private AttackInstance.Factory _attackInstanceFactory;

    [Inject, UsedImplicitly]
    private void Init (AttackInstance.Factory attackInstanceFactory)
    {
        _attackInstanceFactory = attackInstanceFactory;
    }

    private void Awake ()
    {
        _turnManager = FindObjectOfType<TurnManager> (); // TODO : inject
    }

    public override void ExecuteTurn (Action onTurnEnded)
    {
        if (!_branchTouchingGround)
        {
            onTurnEnded?.Invoke ();
            return;
        }

        if (_totalFlowerGrown == 0)
        {
            onTurnEnded?.Invoke ();
            return;
        }

        if (_attackableInRange.Count == 0)
        {
            onTurnEnded?.Invoke ();
            return;
        }

        string healingEffectName = _healingAttack.EffectsSerialized[0].Effect.EffectName;
        float mult = 1 + (_totalFlowerGrown * 0.075f);

        int attackableLeft = _attackableInRange.Count;
        foreach (Attackable attackable in _attackableInRange)
        {
            if (attackable.WillBeDestroyed)
                continue;
            Character character = attackable as Character;
            if (character == null)
                continue;

            AttackInstance attackInstance = _attackInstanceFactory.Create (_healingAttack, character);
            attackInstance.ApplyMultiplierToEffect (healingEffectName, mult);

            attackInstance.Execute (character,
                character,
                character.GetSpriteBounds ().center,
                () =>
                {
                    attackableLeft--;
                    if (attackableLeft <= 0)
                        onTurnEnded?.Invoke ();
                });
        }

    }

    private bool IsMainBranchTouchingGround (List<Vector2> strokeDrawUVs, FrameDecor frameDecor)
    {
        //Convert firstPointWorld to world position
        Vector3 firstPointWorld = frameDecor.Bounds.center;
        firstPointWorld += new Vector3 (strokeDrawUVs[0].x * frameDecor.Bounds.size.x, strokeDrawUVs[0].y * frameDecor.Bounds.size.y, 0f);
        firstPointWorld -= new Vector3 (frameDecor.Bounds.extents.x, frameDecor.Bounds.extents.y, 0f);
        firstPointWorld.z = frameDecor.transform.position.z;
        firstPointWorld.y += 0.1f;
        if (Physics.Raycast (firstPointWorld, Vector3.down, out RaycastHit hitFirstPoint, 0.15f, LayerMask.GetMask ("Map")))
        {
            return true;
        }

        // Convert lastPointWorld to world position
        Vector3 lastPointWorld = frameDecor.Bounds.center;
        lastPointWorld += new Vector3 (strokeDrawUVs[strokeDrawUVs.Count - 1].x * frameDecor.Bounds.size.x, strokeDrawUVs[strokeDrawUVs.Count - 1].y * frameDecor.Bounds.size.y, 0f);
        lastPointWorld -= new Vector3 (frameDecor.Bounds.extents.x, frameDecor.Bounds.extents.y, 0f);
        lastPointWorld.z = frameDecor.transform.position.z;
        lastPointWorld.y += 0.1f;
        if (Physics.Raycast (lastPointWorld, Vector3.down, out RaycastHit hitLastPoint, 0.15f, LayerMask.GetMask ("Map")))
        {
            return true;
        }

        return false;
    }

    public void Init (TurnManager turnBasedCombat, List<Vector2> strokeDrawUVs, FrameDecor frameDecor, Action onInitDone = null)
    {
        _frameDecor = frameDecor;
        if (!IsMainBranchTouchingGround (strokeDrawUVs, frameDecor))
        {
            Debug.Log ("Branch not touching ground, aborting");
            Rigidbody rigidbody = GetComponent<Rigidbody> ();
            rigidbody.isKinematic = false;

            onInitDone?.Invoke ();
            return;
        }

        GetComponent<Collider> ().enabled = false;

        _branchTouchingGround = true;
        _onInitDone = onInitDone;
        _renderer = GetComponent<SpriteRenderer> ();
        _currentDrawTime = BRANCH_DRAW_TIME;
        _lastStep = 1;
        _currentStep = 1;
        _drawedTexInitialPixels = frameDecor.DrawTexture.GetPixels ();
        _frameTexPixels = frameDecor.MainTexture.GetPixels ();
        _branchIds = new int[_drawedTexInitialPixels.Length];

        // set draw texture
        _drawedTex = new Texture2D (frameDecor.Width, frameDecor.Height, TextureFormat.RGBA32, false);
        _drawedTex.filterMode = FilterMode.Point;
        _drawedTex.SetPixels (_renderer.sprite.texture.GetPixels ());
        _drawedTex.Apply ();

        _drawBranchCs = Resources.Load<ComputeShader> ("DrawBranch");
        if (_drawBranchCs == null)
        {
            Debug.LogError (nameof (_drawBranchCs) + "null, it was not loaded (not found?)");
            return;
        }

        _kernelCS = _drawBranchCs.FindKernel ("DrawBranch");

        _rendTex = new RenderTexture (_drawedTex.width, _drawedTex.height, 0, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Point,
            enableRandomWrite = true
        };
        Graphics.Blit (_drawedTex, _rendTex);
        _drawBranchCs.SetTexture (_kernelCS, "DrawnTex", _rendTex);

        _drawBranchCs.SetFloats ("BrushColor", new float[4] { _branchColor.r, _branchColor.g, _branchColor.b, _branchColor.a });

        const float stepSizeInPixel = 6f;
        Vector2 lastPixelPos = strokeDrawUVs[0];

        _groupThreadCS.x = GraphicUtils.GetComputeShaderDispatchCount (frameDecor.Width, 32);
        _groupThreadCS.y = GraphicUtils.GetComputeShaderDispatchCount (frameDecor.Height, 32);
        _groupThreadCS.z = 1;

        bool orientation = false;
        bool lastOrientation = true;

        // Fork branches level 1
        foreach (Vector2 uv in strokeDrawUVs)
        {
            Vector2 pixelPos = new Vector2 ((int) (uv.x * frameDecor.DrawTexture.width), (int) (uv.y * frameDecor.DrawTexture.height));
            if (Vector2.Distance (lastPixelPos, pixelPos) < stepSizeInPixel)
            {
                continue;
            }

            Vector2 mainBranchDirection = (pixelPos - lastPixelPos).normalized;
            orientation = !orientation;
            float branchAngle = UnityEngine.Random.Range (MIN_BRANCH_ANGLE, MAX_BRANCH_ANGLE) * (orientation ? 1 : -1);
            Vector2 branchDirection = Quaternion.Euler (0, 0, branchAngle) * mainBranchDirection;
            float branchLength = UnityEngine.Random.Range (MIN_BRANCH_PIX_LENGTH, MAX_BRANCH_PIX_LENGTH);

            if (!TryGetBranchInitialPos (branchDirection, pixelPos, out Vector2 branchPos))
            {
                continue;
            }

            BranchFork branchFork = new BranchFork (branchPos, branchDirection, branchLength);
            _branchForksLevel1.Add (branchFork);

            lastPixelPos = branchPos;
            lastOrientation = orientation;
        }

        UpdateBranchesDrawing (_currentStep, ref _branchForksLevel1);

        _renderer.sprite = Sprite.Create (_drawedTex, new Rect (0, 0, _drawedTex.width, _drawedTex.height), new Vector2 (0.5f, 0.5f));
        _renderer.material.mainTexture = _drawedTex;
    }

    private bool TryGetBranchInitialPos (Vector2 branchDirection, Vector2 branchOrigin, out Vector2 branchPos)
    {
        branchPos = branchOrigin;
        const int MAX_PIXEL_DIST_IN_TRONK = 4;

        for (int i = 0; i < MAX_PIXEL_DIST_IN_TRONK; i++)
        {
            branchPos = branchOrigin + branchDirection * i;
            int index = (int) branchPos.x + (int) branchPos.y * _drawedTex.width;
            if (index < 0 || index >= _drawedTexInitialPixels.Length)
                return false;

            Color pixelColor = _drawedTexInitialPixels[index];
            if (pixelColor.a < 0.99f)
            {
                return true;
            }
        }

        return false;
    }

    private void Update ()
    {
        if (!_branchTouchingGround)
            return;

        _currentDrawTime -= Time.deltaTime;
        if (_currentStep < DRAW_STEPS_MAX && _currentDrawTime <= BRANCH_DRAW_TIME - ((float) BRANCH_DRAW_TIME * (float) _currentStep / (float) DRAW_STEPS_MAX))
        {
            _currentStep++;
            if (_currentStep != _lastStep)
            {
                UpdateBranchesDrawing (_currentStep, ref _branchForksLevel1);
            }
        }

        _lastStep = _currentStep;

        if (!_growFinished && _currentStep == DRAW_STEPS_MAX && _totalFlowerExpected == _totalFlowerGrown)
            OnGrowFinished ();
    }

    private void UpdateBranchesDrawing (int currentStep, ref List<BranchFork> branchForks)
    {
        float nextLerp = (float) currentStep / DRAW_STEPS_MAX;
        float oldLerp = (float) (currentStep - 1) / DRAW_STEPS_MAX;
        // Debug.Log ("UpdateBranchesDrawing, oldLerpValue " + oldLerpValue + ", lerpValue : " + lerpValue);

        float step = (nextLerp - oldLerp) / 4f;
        List<BranchFork> branchCreated = new List<BranchFork> ();
        List<BranchFork> branchToRemove = new List<BranchFork> ();

        foreach (BranchFork branchFork in branchForks)
        {
            bool stopGrow = false;

            for (float currentLerp = oldLerp; currentLerp <= nextLerp + 0.0001f; currentLerp += step)
            {
                float branchLerpValue = currentLerp;
                if (branchFork.StartLerpValue >= 1f)
                    branchLerpValue = 1f;
                else if (branchLerpValue < branchFork.StartLerpValue)
                    branchLerpValue = 0f;
                else
                {
                    branchLerpValue = Mathf.Abs (branchLerpValue - branchFork.StartLerpValue) / Mathf.Abs (1f - branchFork.StartLerpValue);
                    branchLerpValue = Mathf.Clamp01 (branchLerpValue);
                }

                Vector2 brushPosF = Vector2.Lerp (branchFork.Origin, branchFork.TipPoint, branchLerpValue);
                Vector2Int brushPos = new Vector2Int ((int) brushPosF.x, (int) brushPosF.y);

                // Check if current brush position is available
                if (branchFork.CurrentPositionInPixel.x == brushPos.x && branchFork.CurrentPositionInPixel.y == brushPos.y)
                    continue;

                branchFork.CurrentPositionInPixel = new Vector2Int (brushPos.x, brushPos.y);
                int index = branchFork.CurrentPositionInPixel.x + branchFork.CurrentPositionInPixel.y * _drawedTex.width;

                if (currentLerp == oldLerp && index >= 0 && index < _drawedTexInitialPixels.Length &&
                    _drawedTexInitialPixels[index].a > 0.99f && _branchIds[index] != branchFork.Id)
                    stopGrow = true;

                if (!stopGrow)
                {
                    // Check if next brush position is available
                    Vector2 nextBrushPosF = Vector2.Lerp (branchFork.Origin, branchFork.TipPoint, branchLerpValue + step);
                    Vector2Int nextBrushPos = new Vector2Int ((int) nextBrushPosF.x, (int) nextBrushPosF.y);
                    int nextIndex = nextBrushPos.x + nextBrushPos.y * _drawedTex.width;
                    if (nextIndex >= 0 && nextIndex < _drawedTexInitialPixels.Length &&
                        _drawedTexInitialPixels[nextIndex].a > 0.99f && _branchIds[nextIndex] != branchFork.Id)
                        stopGrow = true;

                    if (nextIndex >= 0 && nextIndex < _frameTexPixels.Length &&
                        _frameTexPixels[nextIndex].a < 0.01f)
                        stopGrow = true;
                }

                if (stopGrow)
                {
                    branchToRemove.Add (branchFork);
                    break;
                }

                DrawBranchPart (branchFork, _branchForkBrush);

                if (TryForkBranch (branchFork, branchLerpValue, currentLerp, out BranchFork newFork))
                {
                    branchCreated.Add (newFork);
                }
            }

            if (!stopGrow && currentStep == DRAW_STEPS_MAX)
            {
                GrowFlower (branchFork);
                _totalFlowerExpected++;
            }
        }

        branchForks.AddRange (branchCreated);
        branchForks.RemoveAll (branchToRemove.Contains);
        ApplyDrawTex ();

        if (nextLerp >= 0.99f)
            GraphicUtils.SavePixelsAsPNG (_drawedTex.GetPixels (), "Test/Branch.png", _drawedTex.width, _drawedTex.height);
    }

    private void GrowFlower (BranchFork branchFork)
    {
        // get the world position of the flower
        // Get the flower position in % relative to the drawed texture
        Vector3 flowerWorldLocalPos = new Vector3 (branchFork.CurrentPositionInPixel.x / (float) _drawedTex.width, branchFork.CurrentPositionInPixel.y / (float) _drawedTex.height, 0f);

        // scale the flower position to the drawed texture size in world space
        flowerWorldLocalPos.Scale (new Vector3 ((float) _drawedTex.width / 100f, (float) _drawedTex.height / 100f, 0f));

        // move the flower position to the center of the drawed texture (assume the pivot of the drawed texture is 0.5, 0.5).
        // So, substract by half the drawed texture size divided by 100 (assume a pixel is 0.01 world unit)
        flowerWorldLocalPos -= new Vector3 ((float) _drawedTex.width / 200f, (float) _drawedTex.height / 200f, 0f);

        // put the flower in front of the drawed texture
        flowerWorldLocalPos.z = -0.0001f;

        SpriteAnimation flowerAnimation = Instantiate (_flowerGrowTemplate);
        flowerAnimation.transform.SetParent (transform);
        flowerAnimation.transform.localPosition = flowerWorldLocalPos;
        flowerAnimation.OnAnimationEnded += () => _totalFlowerGrown++;

        Vector3 flowerWorldPos = transform.TransformPoint (flowerWorldLocalPos);
        _flowerPositions.Add (flowerWorldPos);

        if (_totalFlowerExpected == 0)
        {
            _bottonLeftFlowerPos = flowerWorldPos;
            _topRightFlowerPos = flowerWorldPos;
        }
        else
        {
            // recheck if flower position is the new bottom left or top right
            if (flowerWorldPos.x < _bottonLeftFlowerPos.x)
                _bottonLeftFlowerPos.x = flowerWorldPos.x;
            if (flowerWorldPos.y < _bottonLeftFlowerPos.y)
                _bottonLeftFlowerPos.y = flowerWorldPos.y;

            if (flowerWorldPos.x > _topRightFlowerPos.x)
                _topRightFlowerPos.x = flowerWorldPos.x;
            if (flowerWorldPos.y > _topRightFlowerPos.y)
                _topRightFlowerPos.y = flowerWorldPos.y;
        }
    }

    private void OnGrowFinished ()
    {
        _growFinished = true;

        AreaOfEffect2D healingAreaOfEffectFx = Instantiate (_healingAreaOfEffectFxRemplate, transform);
        // get the dimension in world, and convert it to pixel by * 100
        Vector2Int dimension = new Vector2Int ((int) Mathf.Abs ((_topRightFlowerPos.x - _bottonLeftFlowerPos.x) * 100), (int) Mathf.Abs ((_topRightFlowerPos.y - _bottonLeftFlowerPos.y) * 100));
        Vector3 areaPos = (_topRightFlowerPos + _bottonLeftFlowerPos) / 2f;
        areaPos.z = transform.position.z - 0.0002f; // Display just in front of flowers
        healingAreaOfEffectFx.transform.position = areaPos;

        float radius = -1;
        foreach (Vector3 flowerPos in _flowerPositions)
        {
            radius = Mathf.Max (radius, Vector3.Distance (flowerPos, areaPos));
        }

        healingAreaOfEffectFx.Init (dimension, radius);
        healingAreaOfEffectFx.OnTransitionFinished += () => _onInitDone?.Invoke ();
        healingAreaOfEffectFx.OnTriggerEnterCircle += OnTriggerEnter;
        healingAreaOfEffectFx.OnTriggerExitCircle += OnTriggerExit;
    }

    private void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer ("Attackable"))
        {
            Attackable attackable = other.gameObject.GetComponent<Attackable> ();
            if (_attackableInRange.Contains (attackable))
                return;

            _attackableInRange.Add (attackable);
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer ("Attackable"))
        {
            Attackable attackable = other.gameObject.GetComponent<Attackable> ();
            if (!_attackableInRange.Contains (attackable))
                return;

            _attackableInRange.Remove (attackable);
        }
    }

    private bool ShouldStopGrow (BranchFork branchFork)
    {
        int i = branchFork.CurrentPositionInPixel.x + branchFork.CurrentPositionInPixel.y * _drawedTex.width;

        if (_drawedTexInitialPixels[i].a > 0.99f && _branchIds[i] != branchFork.Id)
            return true;

        return false;
    }

    private bool TryForkBranch (BranchFork branchToFork, float branchLerpValue, float mainLerpValue, out BranchFork newBranch)
    {
        newBranch = null;
        if (branchToFork.HasForked)
            return false;

        if (branchLerpValue < FORK_RANGE_MAX)
            return false;

        branchToFork.HasForked = true;
        float forkRange = UnityEngine.Random.Range (FORK_RANGE_MIN, FORK_RANGE_MAX);

        newBranch = new BranchFork (branchToFork.Origin + branchToFork.Direction * forkRange * branchToFork.Length,
            Quaternion.Euler (0, 0, UnityEngine.Random.Range (MIN_BRANCH_ANGLE, MAX_BRANCH_ANGLE)) * branchToFork.Direction,
            UnityEngine.Random.Range (MIN_BRANCH_PIX_LENGTH, MAX_BRANCH_PIX_LENGTH) / 2f);

        newBranch.StartLerpValue = branchLerpValue;
        newBranch.HasForked = true;

        return true;
    }

    private void DrawBranchPart (BranchFork branchFork, Brush brush)
    {
        // Debug.Log ("DrawBranchPart, branchFork.CurrentPositionInPixel : " + branchFork.CurrentPositionInPixel + ", brushPos : " + brushPos);
        int i = branchFork.CurrentPositionInPixel.x + branchFork.CurrentPositionInPixel.y * _drawedTex.width;
        if (i < 0 || i >= _drawedTexInitialPixels.Length)
            return;

        _branchIds[i] = branchFork.Id;

        _drawBranchCs.SetInt ("BrushPosX", branchFork.CurrentPositionInPixel.x);
        _drawBranchCs.SetInt ("BrushPosY", branchFork.CurrentPositionInPixel.y);

        (int width, int height) = brush.GetDimensions ();

        _drawBranchCs.SetInt ("BrushWidth", brush.Texture.width);
        _drawBranchCs.SetInt ("BrushHeight", brush.Texture.height);

        _drawBranchCs.SetTexture (_kernelCS, "BrushTex", brush.Texture);

        _drawBranchCs.Dispatch (_kernelCS, _groupThreadCS.x, _groupThreadCS.y, _groupThreadCS.z);
    }

    private void ApplyDrawTex ()
    {
        RenderTexture.active = _rendTex;
        _drawedTex.ReadPixels (new Rect (0, 0, _drawedTex.width, _drawedTex.height), 0, 0);
        _drawedTex.Apply ();

        _renderer.sprite = Sprite.Create (_drawedTex, new Rect (0, 0, _drawedTex.width, _drawedTex.height), new Vector2 (0.5f, 0.5f));
        _renderer.material.mainTexture = _drawedTex;
    }

}