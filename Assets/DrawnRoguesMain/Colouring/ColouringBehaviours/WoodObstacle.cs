using System;
using System.Collections.Generic;
using UnityEngine;

public class WoodObstacle : Attackable, IColouringSpellBehaviour
{
    private const float KG_PER_PIXEL = 0.3f;

    public void Init (TurnManager bg, List<Vector2> lastStrokeDrawUVs, FrameDecor frameDecor, Action onInitDone = null)
    {
        ComputeShader countPixCs = Resources.Load<ComputeShader> ("CountOpaquePixels");
        if (countPixCs == null)
            throw new Exception (nameof (countPixCs) + "null, it was not loaded (not found?)");

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
        Stats.MaxLife = (int) ((float) pixelCount * 0.2f);

        // Set kilogram
        Stats.Kilogram = pixelCount * KG_PER_PIXEL;

        // TODO : to be replaced by a proper way of placing obstacle
        transform.localPosition = Vector3.back * 0.7f;

        onInitDone?.Invoke ();
    }
}