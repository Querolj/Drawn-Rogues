using UnityEngine;

public class ModifierLayerDepthAjuster : MonoBehaviour
{
    [SerializeField]
    private CharacterMovement _characterMove;

    [SerializeField]
    private bool _behindCharacter = false;

    private void Awake ()
    {
        if (_characterMove == null)
            throw new System.Exception (nameof (_characterMove) + " not set, can't adjust layer depth. (it should be set)");
    }

    private void Update ()
    {
        int factor = _behindCharacter ? -1 : 1;
        float newDepth = 0.0001f * (_characterMove.DirectionRight ? -factor : factor);
        if (transform.localPosition.z != newDepth)
            transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, newDepth);
    }
}