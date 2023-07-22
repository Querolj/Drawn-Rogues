using UnityEngine;

public class CombatMoveIndicator : MonoBehaviour
{
    private Vector3 _baseScale;

    private void Awake ()
    {
        _baseScale = transform.localScale;
    }

    public void SetSizeFromCharacter (Bounds charBounds)
    {
        transform.localScale = Vector3.Scale (charBounds.size * 0.1f, _baseScale);
    }
}