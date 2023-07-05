using UnityEngine;

[RequireComponent (typeof (MeshRenderer))]
[ExecuteInEditMode]
public class SnapOnMap : MonoBehaviour
{
    [SerializeField]
    private float _yOffset = 0f;

    [SerializeField]
    private bool _rotate = true;

    private MeshRenderer _meshRenderer;

    private void Awake ()
    {
        _meshRenderer = GetComponent<MeshRenderer> ();
    }

    private void Update ()
    {
        if (Application.isPlaying)
            return;

        RaycastHit hit;
        if (Physics.Raycast (transform.position - Vector3.down * 100f, Vector3.down, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            Vector3 newPos = transform.position;
            newPos.y = hit.point.y + _meshRenderer.bounds.extents.y + _yOffset;

            transform.position = newPos;
            if (_rotate)
            {
                Quaternion q = Quaternion.FromToRotation (Vector3.up, hit.normal);
                Vector3 euler = q.eulerAngles;
                euler.x = 0f;
                euler.y = 0f;
                transform.rotation = Quaternion.Euler (euler);
            }
        }
    }
}