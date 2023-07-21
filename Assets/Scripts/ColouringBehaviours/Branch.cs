using System;
using System.Collections.Generic;
using UnityEngine;

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

    private SpriteRenderer _renderer;
    private ComputeShader _drawBranchCs;
    private int _kernelCS;

    private const float MIN_BRANCH_ANGLE = 30f;
    private const float MAX_BRANCH_ANGLE = 65f;

    private const float MIN_BRANCH_PIX_LENGTH = 8f;
    private const float MAX_BRANCH_PIX_LENGTH = 14f;
    private Vector3Int _groupThreadCS;

    private const float FORK_RANGE_MIN = 0.4f; // In % of branch length 
    private const float FORK_RANGE_MAX = 0.6f;

    private class BranchFork
    {
        public Vector2 Origin;
        public Vector2 Direction;
        public float Length;
        public float StartLerpValue = 0f;
        public bool HasForked = false;
        public Vector2Int CurrentPositionInPixel = Vector2Int.zero;
        public bool IsOutOfTrunk = false; // It comes true when the branch reach the first alpha pixel

        public Vector2 TipPoint
        {
            get
            {
                return Origin + Direction * Length;
            }
        }
    }
    private List<BranchFork> _branchForksLevel1 = new List<BranchFork> ();
    private List<BranchFork> _branchForksLevel2 = new List<BranchFork> ();

    private Texture2D _drawedTex;
    private RenderTexture _rendTex;

    public void Init (TurnManager turnBasedCombat, List<Vector2> lastStrokeDrawUVs, FrameDecor frameDecor)
    {
        _renderer = GetComponent<SpriteRenderer> ();
        _currentDrawTime = BRANCH_DRAW_TIME;
        _lastStep = 1;
        _currentStep = 1;

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

        _rendTex = new RenderTexture (_drawedTex.width, _drawedTex.height, 0, RenderTextureFormat.ARGB32);
        _rendTex.filterMode = FilterMode.Point;
        _rendTex.enableRandomWrite = true;
        Graphics.Blit (_drawedTex, _rendTex);
        _drawBranchCs.SetTexture (_kernelCS, "DrawnTex", _rendTex);

        _drawBranchCs.SetFloats ("BrushColor", new float[4] { _branchColor.r, _branchColor.g, _branchColor.b, _branchColor.a });

        const float stepSizeInPixel = 6f;
        Vector2 lastPixelPos = new Vector2 (-1, -1);

        _groupThreadCS.x = GraphicUtils.GetComputeShaderDispatchCount (frameDecor.Width, 32);
        _groupThreadCS.y = GraphicUtils.GetComputeShaderDispatchCount (frameDecor.Height, 32);
        _groupThreadCS.z = 1;

        bool orientation = false;
        bool lastOrientation = true;

        // Fork branches level 1
        foreach (Vector2 uv in lastStrokeDrawUVs)
        {
            Vector2 pixelPos = new Vector2 ((int) (uv.x * frameDecor.DrawTexture.width), (int) (uv.y * frameDecor.DrawTexture.height));
            if (lastPixelPos.x != -1 && Vector2.Distance (lastPixelPos, pixelPos) < stepSizeInPixel)
            {
                continue;
            }

            Vector2 mainBranchDirection = (pixelPos - lastPixelPos).normalized;
            float branchAngle = UnityEngine.Random.Range (MIN_BRANCH_ANGLE, MAX_BRANCH_ANGLE);
            orientation = !orientation;

            if (!orientation)
            {
                branchAngle *= -1;
            }

            Vector2 branchDirection = Quaternion.Euler (0, 0, branchAngle) * mainBranchDirection;
            float branchLength = UnityEngine.Random.Range (MIN_BRANCH_PIX_LENGTH, MAX_BRANCH_PIX_LENGTH);
            BranchFork branchFork = new BranchFork ();
            branchFork.Origin = lastPixelPos;
            branchFork.Direction = branchDirection;
            branchFork.Length = branchLength;
            _branchForksLevel1.Add (branchFork);

            lastPixelPos = pixelPos;
            lastOrientation = orientation;
        }

        UpdateBranchesDrawing (_currentStep, ref _branchForksLevel1);

        _renderer.sprite = Sprite.Create (_drawedTex, new Rect (0, 0, _drawedTex.width, _drawedTex.height), new Vector2 (0.5f, 0.5f));
        _renderer.material.mainTexture = _drawedTex;
    }

    private void Update ()
    {
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
    }

    private void UpdateBranchesDrawing (int currentStep, ref List<BranchFork> branchForks)
    {

        float lerpValue = (float) currentStep / DRAW_STEPS_MAX;
        float oldLerpValue = (float) (currentStep - 1) / DRAW_STEPS_MAX;
        // Debug.Log ("UpdateBranchesDrawing, oldLerpValue " + oldLerpValue + ", lerpValue : " + lerpValue);

        float smoothStep = (lerpValue - oldLerpValue) / 4f;
        List<BranchFork> branchCreated = new List<BranchFork> ();
        List<BranchFork> branchToRemove = new List<BranchFork> ();

        foreach (BranchFork branchFork in branchForks)
        {
            for (float i = oldLerpValue; i <= lerpValue + 0.0001f; i += smoothStep)
            {
                float branchLerpValue = i;
                if (branchFork.StartLerpValue >= 1f)
                    branchLerpValue = 1f;
                else if (branchLerpValue < branchFork.StartLerpValue)
                    branchLerpValue = 0f;
                else
                {
                    branchLerpValue = Mathf.Abs (branchLerpValue - branchFork.StartLerpValue) / Mathf.Abs (1f - branchFork.StartLerpValue);
                    branchLerpValue = Mathf.Clamp01 (branchLerpValue);
                }

                bool branchTouched = DrawBranchPart (branchFork, _branchForkBrush, branchLerpValue);
                if (!branchTouched && !branchFork.IsOutOfTrunk && branchLerpValue > 0.6f)
                {
                    branchFork.IsOutOfTrunk = true;
                }
                else if (branchFork.IsOutOfTrunk && branchTouched)
                {
                    branchToRemove.Add (branchFork);
                    break;
                }

                if (TryForkBranch (branchFork, branchLerpValue, i, out BranchFork newFork))
                {
                    branchCreated.Add (newFork);
                }
            }
        }

        branchForks.AddRange (branchCreated);
        branchForks.RemoveAll (branchToRemove.Contains);
        ApplyDrawTex ();

        if (lerpValue >= 0.99f)
            GraphicUtils.SavePixelsAsPNG (_drawedTex.GetPixels (), "Test/Branch.png", _drawedTex.width, _drawedTex.height);
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

        newBranch = new BranchFork ();
        newBranch.Origin = branchToFork.Origin + branchToFork.Direction * forkRange * branchToFork.Length;
        newBranch.Direction = Quaternion.Euler (0, 0, UnityEngine.Random.Range (MIN_BRANCH_ANGLE, MAX_BRANCH_ANGLE)) * branchToFork.Direction;
        newBranch.Length = UnityEngine.Random.Range (MIN_BRANCH_PIX_LENGTH, MAX_BRANCH_PIX_LENGTH) / 2f;
        newBranch.StartLerpValue = branchLerpValue;
        newBranch.HasForked = true;

        return true;
    }

    // return false if the branch needs to stop growing
    private bool DrawBranchPart (BranchFork branchFork, Brush brush, float lerpValue)
    {
        Vector2 branchTipPoint = branchFork.TipPoint;
        Vector2 brushPos = Vector2.Lerp (branchFork.Origin, branchTipPoint, lerpValue);
        if (branchFork.CurrentPositionInPixel.x == (int) brushPos.x && branchFork.CurrentPositionInPixel.y == (int) brushPos.y)
            return true;

        branchFork.CurrentPositionInPixel = new Vector2Int ((int) brushPos.x, (int) brushPos.y);
        _drawBranchCs.SetInt ("BrushPosX", branchFork.CurrentPositionInPixel.x);
        _drawBranchCs.SetInt ("BrushPosY", branchFork.CurrentPositionInPixel.y);

        (int width, int height) = brush.GetDimensions ();

        _drawBranchCs.SetInt ("BrushWidth", brush.Texture.width);
        _drawBranchCs.SetInt ("BrushHeight", brush.Texture.height);

        _drawBranchCs.SetTexture (_kernelCS, "BrushTex", brush.Texture);

        int[] branchTouched = new int[1];
        branchTouched[0] = 0;
        ComputeBuffer branchTouchedBuffer = new ComputeBuffer (1, sizeof (int));
        branchTouchedBuffer.SetData (branchTouched);
        _drawBranchCs.SetBuffer (_kernelCS, "BranchTouch", branchTouchedBuffer);

        _drawBranchCs.Dispatch (_kernelCS, _groupThreadCS.x, _groupThreadCS.y, _groupThreadCS.z);

        return branchTouched[0] == 1;
    }

    private void ApplyDrawTex ()
    {
        RenderTexture.active = _rendTex;
        _drawedTex.ReadPixels (new Rect (0, 0, _drawedTex.width, _drawedTex.height), 0, 0);
        _drawedTex.Apply ();

        _renderer.sprite = Sprite.Create (_drawedTex, new Rect (0, 0, _drawedTex.width, _drawedTex.height), new Vector2 (0.5f, 0.5f));
        _renderer.material.mainTexture = _drawedTex;
    }

    public override void ExecuteTurn (Action onTurnEnded) { }

    private void OnCollisionEnter (Collision other)
    {

    }
}