using UnityEngine;

public class CombatCamera : MonoBehaviour
{
    [SerializeField]
    private CombatZone _combatZone;

    [SerializeField]
    private float _cameraArrowSpeed = 3f;

    [SerializeField]
    private float _cameraMouseSpeed = 0.3f;

    private Bounds _bounds;
    private Vector3 _oldMousePosition;

    private void Start ()
    {
        _bounds = _combatZone.Bounds;
    }

    private void FixedUpdate ()
    {
        if (!_combatZone.FightStarted)
            return;

        Vector3 direction = Vector3.zero;
        float cameraSpeed = _cameraArrowSpeed;
        if (Input.GetKey (KeyCode.UpArrow))
            direction += Vector3.up;
        if (Input.GetKey (KeyCode.DownArrow))
            direction += Vector3.down;
        if (Input.GetKey (KeyCode.LeftArrow))
            direction += Vector3.left;
        if (Input.GetKey (KeyCode.RightArrow))
            direction += Vector3.right;
        if (Input.GetMouseButton (1) || Input.GetMouseButton (2))
        {
            direction = -(Input.mousePosition - _oldMousePosition);
            if (direction.magnitude > 250f) // avoid out of screen fast movement, when the mouse comes back from the other side of the screen. Value 250f is arbitrary.
                direction = Vector3.zero;
            cameraSpeed = _cameraMouseSpeed;
        }

        if (direction != Vector3.zero)
        {
            Vector3 newPosition = transform.position + direction * cameraSpeed * Time.fixedDeltaTime;
            newPosition.x = Mathf.Clamp (newPosition.x, _bounds.min.x, _bounds.max.x);
            newPosition.y = Mathf.Clamp (newPosition.y, _bounds.min.y, _bounds.max.y);
            transform.position = newPosition;
        }

        _oldMousePosition = Input.mousePosition;
    }
}