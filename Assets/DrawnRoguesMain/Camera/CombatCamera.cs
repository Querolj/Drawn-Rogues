using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class CombatCamera : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _cameraMoveInput;

    [SerializeField]
    private InputActionReference _cameraDragInput;

    [SerializeField]
    private CombatZone _combatZone;

    [SerializeField]
    private float _cameraArrowSpeed = 3f;

    [SerializeField]
    private float _cameraMouseSpeed = 0.3f;

    private Bounds _bounds;
    private Vector3 _oldMousePosition;

    private CameraService _cameraService;

    [Inject, UsedImplicitly]
    private void Init (CameraService cameraService)
    {
        _cameraService = cameraService;
    }

    private void Start ()
    {
        _bounds = _combatZone.Bounds;
    }

    private void FixedUpdate ()
    {
        if (!_combatZone.FightStarted || _cameraService.IsBlending)
            return;

        Vector3 direction = Vector3.zero;
        float cameraSpeed = _cameraArrowSpeed;
        if (_cameraMoveInput.action.IsPressed ())
        {
            direction = _cameraMoveInput.action.ReadValue<Vector2> ();
        }

        if (_cameraDragInput.action.IsPressed ())
        {
            direction = -((Vector3) Mouse.current.position.ReadValue () - _oldMousePosition);
            if (direction.magnitude > 250f) // avoid out of screen fast movement, when the mouse comes back from the other side of the screen. Value 250f is arbitrary.
                direction = Vector3.zero;
            cameraSpeed = _cameraMouseSpeed;
        }

        if (direction != Vector3.zero)
        {
            Vector3 newPosition = transform.position + direction.normalized * cameraSpeed * Time.fixedDeltaTime;
            newPosition.x = Mathf.Clamp (newPosition.x, _bounds.min.x, _bounds.max.x);
            newPosition.y = Mathf.Clamp (newPosition.y, _bounds.center.y, _bounds.max.y);
            transform.position = newPosition;
        }

        _oldMousePosition = Mouse.current.position.ReadValue ();
    }
}