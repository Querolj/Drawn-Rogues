using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent (typeof (DecalProjector))]
public class EnemyTurnIndicator : MonoBehaviour
{
    private DecalProjector _decalProjector = null;

    private Transform _target;

    private void Awake ()
    {
        _decalProjector = GetComponent<DecalProjector> ();
    }

    public void SetOnCharacter (Character character)
    {
        _decalProjector.enabled = true;
        _target = character.transform;
    }

    public void Hide ()
    {
        _target = null;
        _decalProjector.enabled = false;
    }

    private void Update ()
    {
        if (_target != null)
        {
            Vector3 newPos = _target.position;
            newPos.y = Utils.GetMapHeight (_target.position) + 0.5f;
            transform.position = newPos;
        }
    }
}