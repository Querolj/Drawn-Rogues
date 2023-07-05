using UnityEngine;

public class EventLowerInTime : EventMap
{
    [SerializeField]
    private float _speed = 1f;

    private void FixedUpdate ()
    {
        if (!_triggered)
            return;
        transform.position += Vector3.down * _speed * Time.fixedDeltaTime;
    }
}