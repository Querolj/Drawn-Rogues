using UnityEngine;

[RequireComponent (typeof (SpriteRenderer))]
public class Outline : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Material _material;
    public int OutlineThickness
    {
        get;
        set;
    } = 1;

    private void Awake ()
    {
        _renderer = GetComponent<SpriteRenderer> ();
        _material = _renderer.material;
        DeactivateOutline ();
    }

    public void SetRenderer (SpriteRenderer rend)
    {
        _material.SetTexture ("_MainTex", rend.sprite.texture);
        _renderer.sprite = rend.sprite;
    }

    public void ActivateOutline (Color color)
    {
        _renderer.enabled = true;
        _material.SetColor ("_OutlineColor", color);
        _material.SetFloat ("_OutlineThickness", OutlineThickness);
    }

    public void DeactivateOutline ()
    {
        _renderer.enabled = false;
    }
}