using UnityEngine;

public class CombatMoveIndicator : MonoBehaviour
{
    [SerializeField]
    private Transform _planeTransform;

    private Vector3 BaseScale;

    private void Awake ()
    {
        BaseScale = transform.localScale;
    }

    public void SetSizeFromCharacter (DrawedCharacter dc)
    {
        Bounds bounds = dc.GetSpriteBounds ();
        transform.localScale = Vector3.Scale (bounds.size, BaseScale);
    }
}