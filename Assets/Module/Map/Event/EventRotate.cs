using UnityEngine;

public class EventRotate : EventMap
{
    [SerializeField]
    private float _speed = 1f;

    [SerializeField]
    private Vector3 _targetEulerAngles;

    private Collider _collider;
    private float _currentLerp = 0f;
    private Vector3 _initialAngles;

    private void Awake ()
    {
        _collider = GetComponent<Collider> ();
        _collider.enabled = false;
        _initialAngles = transform.eulerAngles;
    }

    public override void Trigger ()
    {
        base.Trigger ();
        _triggered = true;
        _currentLerp = 0f;
    }

    private void FixedUpdate ()
    {
        if (!_triggered || _collider.enabled)
            return;

        _currentLerp += _speed * Time.fixedDeltaTime;
        transform.rotation = Quaternion.Slerp (Quaternion.Euler (_initialAngles), Quaternion.Euler (_targetEulerAngles), _currentLerp);

        if (_currentLerp >= 1f)
        {
            _collider.enabled = true;
        }
    }
}