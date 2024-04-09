using System;
using UnityEngine;

public class TextureTransition : MonoBehaviour
{
    [SerializeField]
    ComputeShader _textureTransitionCs;

    private float _pixelTransitionDurationPerFrame;
    private float _colorTransitionDuration;
    private float _currentFrameDuration;
    private float _totalTransitionDuration;
    private Texture2D _textureToTransition;
    private Texture2D _targetTexture;
    private bool _transitionRunning = false;
    private Action<Texture2D> _onTransitionEnded;

    public void PlayTransition (Texture2D from, Texture2D to, float pixelTransitionDurationPerFrame, float colorTransitionDuration, Action<Texture2D> onTransitionEnd)
    {
        _currentFrameDuration = 0f;
        _totalTransitionDuration = 0f;
        _pixelTransitionDurationPerFrame = pixelTransitionDurationPerFrame;
        _colorTransitionDuration = colorTransitionDuration;
        _textureToTransition = from;
        _targetTexture = to;
        _transitionRunning = true;

        _onTransitionEnded = onTransitionEnd;
    }

    private void Update ()
    {
        if (!_transitionRunning)
            return;

        _currentFrameDuration += Time.deltaTime;
        _totalTransitionDuration += Time.deltaTime;

        if (_currentFrameDuration >= _pixelTransitionDurationPerFrame)
        {
            Transition ();
            _currentFrameDuration = 0f;
        }
    }

    private void Transition ()
    {
        int kernel = _textureTransitionCs.FindKernel ("Transition");

        RenderTexture rendTex = new RenderTexture (_textureToTransition.width, _textureToTransition.height, 0, RenderTextureFormat.ARGB32);
        rendTex.filterMode = FilterMode.Point;
        rendTex.enableRandomWrite = true;
        Graphics.Blit (_textureToTransition, rendTex);
        _textureTransitionCs.SetTexture (kernel, "TexToTransition", rendTex);
        _textureTransitionCs.SetTexture (kernel, "TargetTex", _targetTexture);
        _textureTransitionCs.SetTexture (kernel, "Tex", _textureToTransition);

        ComputeBuffer pixelRemovedOrAddedBuffer = new ComputeBuffer (1, sizeof (uint));
        uint[] pixelRemovedOrAddedData = new uint[1];
        pixelRemovedOrAddedData[0] = 0;
        pixelRemovedOrAddedBuffer.SetData (pixelRemovedOrAddedData);
        _textureTransitionCs.SetBuffer (kernel, "PixelRemovedOrAdded", pixelRemovedOrAddedBuffer);

        float colorLerp = Mathf.Min (1f, _totalTransitionDuration / _colorTransitionDuration);
        _textureTransitionCs.SetFloat ("ColorLerp", colorLerp);

        int x = GraphicUtils.GetComputeShaderDispatchCount (_textureToTransition.width, 32);
        int y = GraphicUtils.GetComputeShaderDispatchCount (_textureToTransition.height, 32);
        int z = 1;

        _textureTransitionCs.Dispatch (kernel, x, y, z);
        pixelRemovedOrAddedBuffer.GetData (pixelRemovedOrAddedData);
        bool hasPixelRemovedOrAddedData = pixelRemovedOrAddedData[0] == 1;

        pixelRemovedOrAddedBuffer.Release ();

        if (hasPixelRemovedOrAddedData || colorLerp <= 1f)
        {
            RenderTexture.active = rendTex;
            _textureToTransition.ReadPixels (new Rect (0, 0, rendTex.width, rendTex.height), 0, 0);
            _textureToTransition.Apply ();
            RenderTexture.active = null;
        }

        if (colorLerp >= 1f && !hasPixelRemovedOrAddedData)
        {
            StopTransition ();
        }
    }

    private void StopTransition ()
    {
        _transitionRunning = false;
        _onTransitionEnded?.Invoke (_textureToTransition);
    }
}