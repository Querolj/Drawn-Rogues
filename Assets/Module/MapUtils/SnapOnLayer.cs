using UnityEngine;

[ExecuteInEditMode]
public class SnapOnLayer : MonoBehaviour
{
    private enum SnapDirection { Down, Left, Right, Front, Back }

    [SerializeField]
    private LayerMask _layerMask;

    [SerializeField]
    private SnapDirection _snapDirection;

    private Vector3 _direction;
    private MeshRenderer _meshRenderer;
    private void Awake ()
    {
        _meshRenderer = GetComponent<MeshRenderer> ();

        switch (_snapDirection)
        {
            case SnapDirection.Down:
                _direction = Vector3.down;
                break;
            case SnapDirection.Left:
                _direction = Vector3.left;
                break;
            case SnapDirection.Right:
                _direction = Vector3.right;
                break;
            case SnapDirection.Front:
                _direction = Vector3.forward;
                break;
            case SnapDirection.Back:
                _direction = Vector3.back;
                break;
        }

    }

    private void Update ()
    {
        if (Application.isPlaying)
            return;

        RaycastHit hit;
        if (Physics.Raycast (transform.position - _direction * 100f, _direction, out hit, Mathf.Infinity, _layerMask))
        {
            Vector3 newPos = transform.position;
            switch (_snapDirection)
            {
                case SnapDirection.Down:
                    newPos.y = hit.point.y;
                    break;
                case SnapDirection.Left:
                    newPos.x = hit.point.x;
                    break;
                case SnapDirection.Right:
                    newPos.x = hit.point.x;
                    break;
                case SnapDirection.Front:
                    newPos.z = hit.point.z;
                    break;
                case SnapDirection.Back:
                    newPos.z = hit.point.z;
                    break;
            }
            transform.position = newPos;
        }
    }
}