using UnityEngine;

[RequireComponent (typeof (SpriteRenderer))]
public class AreaOfEffect2D : MonoBehaviour
{
    [SerializeField]
    private Color _color = Color.white;

    [SerializeField]
    private float _appearingTransitionInSec = 0.5f;

    private float _currentAppearingTransitionValue = 0f;

    private SpriteRenderer _spriteRenderer;

    public void Init (Vector2Int dimension, float radius)
    {
        int maxDimension = Mathf.Max (dimension.x, dimension.y);
        _spriteRenderer = GetComponent<SpriteRenderer> ();

        _spriteRenderer.material.SetFloat ("_Radius", radius);
        _spriteRenderer.material.SetColor ("_Color", _color);
        float circleThickness = _spriteRenderer.material.GetFloat ("_CircleThickness");

        int texDimension = (int) (radius * 2f * 100f);
        texDimension += (int) (circleThickness * 600f); // add six times circle thickness to avoid having the circle cropped on the texture borders
        Texture2D tex = new Texture2D (texDimension, texDimension);
        Sprite sprite = Sprite.Create (tex, new Rect (0, 0, texDimension, texDimension), new Vector2 (0.5f, 0.5f));
        _spriteRenderer.sprite = sprite;
    }

    private void Update ()
    {
        if (_currentAppearingTransitionValue > _appearingTransitionInSec)
            return;
            
        _currentAppearingTransitionValue += Time.deltaTime;

        if (_appearingTransitionInSec > 0f)
        {
            _spriteRenderer.material.SetFloat ("_TransitionLerp", _currentAppearingTransitionValue / _appearingTransitionInSec);
        }
    }
}