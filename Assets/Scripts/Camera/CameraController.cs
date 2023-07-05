using UnityEngine;

[RequireComponent (typeof (Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float _panSpeed = 2f;
    private Camera _camera;
    private Vector3 _lastMousePos;
    private const float MAX_DIST_BETWEEN_MOUSE_POS = 200f;

    #region transition 
    [SerializeField]
    private float _transitionSpeed = 1f;
    private Vector3 _drawAngle = new Vector3 (18f, 0f, 0f);
    private float _drawHeight = 1f;
    private Vector3 _mapAngle = new Vector3 (41f, 0f, 0f);
    private float _mapHeight = 2f;
    private bool _isTransitioning = false;
    private bool _isFollowingTransform = false;
    private Transform _followedTransform;

    public enum CameraState
    {
        Map,
        Draw,
        Combat
    }

    private CameraState _cameraStateTarget = CameraState.Map;
    private Vector3 _startPos;
    private Vector3 _endPos;

    private Vector3 _startAngle;
    private Vector3 _endAngle;
    private float _currentLerp = 0f;
    #endregion

    private float _heightOffset = 0f;

    private void Awake ()
    {
        _camera = GetComponent<Camera> ();
    }

    void Update ()
    {
        if (_isFollowingTransform && Input.GetKeyDown (KeyCode.C)) // just for debug, it's supposed to be after transition
            UpdateFollowTransform ();

        if (_isTransitioning)
            return;

        if (_cameraStateTarget != CameraState.Combat && Input.GetKeyDown (KeyCode.Space))
        {
            if (_cameraStateTarget == CameraState.Map)
                _cameraStateTarget = CameraState.Draw;
            else
                _cameraStateTarget = CameraState.Map;

            SetCameraState (_cameraStateTarget);
        }

        if ((Input.GetMouseButton (2) || Input.GetMouseButton (1)))
        {
            // avoid mouse outside of the screen that leads to big leap for the cam position
            if (Vector3.Distance (_lastMousePos, Input.mousePosition) > MAX_DIST_BETWEEN_MOUSE_POS)
                return;

            Vector3 mouseDir = _lastMousePos - Input.mousePosition;
            Pan (mouseDir);
        }

        _lastMousePos = Input.mousePosition;
    }

    private void FixedUpdate ()
    {
        UpdateHeightOffset ();

        if (_isTransitioning)
        {
            UpdateTransition ();
            return;
        }
    }

    public void SetCameraState (CameraState state)
    {
        _cameraStateTarget = state;
        StartInputTransition ();
    }

    private void UpdateHeightOffset ()
    {
        RaycastHit[] hits = Physics.RaycastAll (transform.position, Vector3.down, 100f);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer ("Map"))
            {
                _heightOffset = hit.distance;
                if (_cameraStateTarget == CameraState.Map)
                    _heightOffset = _mapHeight - hit.distance;
                else
                    _heightOffset = _drawHeight - hit.distance;

                if (!_isTransitioning)
                    transform.position = new Vector3 (transform.position.x, transform.position.y + _heightOffset, transform.position.z);
                break;
            }
        }
    }

    private void Pan (Vector3 mouseDirection)
    {
        Vector3 direction = Vector3.zero;
        if (_cameraStateTarget == CameraState.Combat)
        {
            direction = new Vector3 (mouseDirection.x, mouseDirection.y, 0f);

        }
        else
            direction = new Vector3 (mouseDirection.x, 0f, mouseDirection.y);
        transform.position += direction * Time.deltaTime * _panSpeed;
    }

    private void StartInputTransition ()
    {
        _currentLerp = 0f;
        _isTransitioning = true;
        _startPos = transform.position;
        _endPos = new Vector3 (transform.position.x, (_cameraStateTarget == CameraState.Map ? _mapHeight : _drawHeight) + _heightOffset, transform.position.z);
        _startAngle = transform.rotation.eulerAngles;
        _endAngle = (_cameraStateTarget == CameraState.Map ? _mapAngle : _drawAngle);
    }

    public void FollowTransform (Transform target)
    {
        _isFollowingTransform = true;
        _followedTransform = target;
    }

    private void UpdateFollowTransform ()
    {
        if (_followedTransform == null)
            return;

        float hfov = Camera.VerticalToHorizontalFieldOfView (_camera.fieldOfView, _camera.aspect);
        float distance = ((_cameraStateTarget == CameraState.Map ? _mapHeight : _drawHeight) + _heightOffset) / Mathf.Tan (hfov * 0.5f * Mathf.Deg2Rad);
        transform.position = _followedTransform.position - transform.forward * distance;
    }

    private void UpdateTransition ()
    {
        _currentLerp += Time.fixedDeltaTime * _transitionSpeed;
        transform.rotation = Quaternion.Lerp (Quaternion.Euler (_startAngle), Quaternion.Euler (_endAngle), _currentLerp);
        transform.position = Vector3.Lerp (_startPos, _endPos, _currentLerp);

        if (_currentLerp >= 1f)
        {
            _isTransitioning = false;
        }
    }

    public void ActiveCombatView (BoxCollider boxCollider)
    {
        _cameraStateTarget = CameraState.Combat;
        _startAngle = transform.rotation.eulerAngles;
        _endAngle = _drawAngle;

        _startPos = transform.position;

        Bounds bounds = boxCollider.bounds;
        Vector3[] boxVertices = new Vector3[3];
        boxVertices[0] = bounds.extents.x * Vector3.right - bounds.extents.y * Vector3.up - bounds.extents.z * Vector3.forward;
        boxVertices[1] = -bounds.extents.x * Vector3.right - bounds.extents.y * Vector3.up - bounds.extents.z * Vector3.forward;

        boxVertices[2] = bounds.extents.y * Vector3.up;

        // boxVertices[0] = bounds.extents.x * Vector3.right + bounds.extents.y * Vector3.up + bounds.extents.z * Vector3.forward;
        // boxVertices[1] = bounds.extents.x * Vector3.right + bounds.extents.y * Vector3.up - bounds.extents.z * Vector3.forward;
        // boxVertices[2] = bounds.extents.x * Vector3.right - bounds.extents.y * Vector3.up + bounds.extents.z * Vector3.forward;
        // boxVertices[3] = bounds.extents.x * Vector3.right - bounds.extents.y * Vector3.up - bounds.extents.z * Vector3.forward;
        // boxVertices[4] = -bounds.extents.x * Vector3.right + bounds.extents.y * Vector3.up + bounds.extents.z * Vector3.forward;
        // boxVertices[5] = -bounds.extents.x * Vector3.right + bounds.extents.y * Vector3.up - bounds.extents.z * Vector3.forward;
        // boxVertices[6] = -bounds.extents.x * Vector3.right - bounds.extents.y * Vector3.up + bounds.extents.z * Vector3.forward;
        // boxVertices[7] = -bounds.extents.x * Vector3.right - bounds.extents.y * Vector3.up - bounds.extents.z * Vector3.forward;

        float degreeCorrection = 0f;

        Quaternion targetedRotation = Quaternion.Euler (_endAngle);
        Vector3 cameraRight = Quaternion.Euler (-Vector3.up * degreeCorrection) * targetedRotation * Vector3.right;
        Vector3 cameraUp = Quaternion.Euler (-Vector3.up * degreeCorrection) * targetedRotation * Vector3.up;
        Vector3 cameraForward = Quaternion.Euler (-Vector3.up * degreeCorrection) * targetedRotation * Vector3.forward;

        float verticalFovTan = Mathf.Tan (0.5f * Mathf.Deg2Rad * _camera.fieldOfView);
        float horizontalFovTan = _camera.aspect * verticalFovTan;

        float maxCameraDist = 0f;

        for (int i = 0; i < boxVertices.Length; ++i)
        {
            float cameraDist = Mathf.Abs (Vector3.Dot (boxVertices[i], cameraUp)) / verticalFovTan;
            cameraDist -= Vector3.Dot (boxVertices[i], cameraForward);

            if (cameraDist > maxCameraDist)
            {
                maxCameraDist = cameraDist;
            }

            cameraDist = Mathf.Abs (Vector3.Dot (boxVertices[i], cameraRight)) / horizontalFovTan;
            cameraDist -= Vector3.Dot (boxVertices[i], cameraForward);

            if (cameraDist > maxCameraDist)
            {
                maxCameraDist = cameraDist;
            }
        }

        _endPos = bounds.center - cameraForward * maxCameraDist;
        _isTransitioning = true;
        _currentLerp = 0f;
    }
}