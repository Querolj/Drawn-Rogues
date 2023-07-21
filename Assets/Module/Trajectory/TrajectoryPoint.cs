using UnityEngine;

[RequireComponent (typeof (SpriteRenderer))]
public class TrajectoryPoint : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private void Awake ()
    {
        _spriteRenderer = GetComponent<SpriteRenderer> ();
    }

    public void Init (float _radius)
    {
        transform.localScale = new Vector3 (_radius, _radius, 1);
    }
}