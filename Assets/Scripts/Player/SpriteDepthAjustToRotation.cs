using UnityEngine;

public class SpriteDepthAjustToRotation : MonoBehaviour
{
    private Transform _transformToAjustTo;
    private float _initialDepth;
    public void Init (Transform transformToAjustTo)
    {
        _transformToAjustTo = transformToAjustTo;
        _initialDepth = transform.localPosition.z;
    }

    private float _lastZEulerAngle;
    private void Update ()
    {
        if (_lastZEulerAngle == _transformToAjustTo.rotation.eulerAngles.z)
            return;

        // ajust own transform depth to transformToAjustTo rotation
        if (_transformToAjustTo.rotation.eulerAngles.y > 90)
        {
            transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, -_initialDepth);
        }
        else
        {
            transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, _initialDepth);
        }

        _lastZEulerAngle = _transformToAjustTo.rotation.eulerAngles.z;
    }
}