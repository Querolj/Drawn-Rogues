using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent (typeof (DecalProjector))]
public class CombatLine : MonoBehaviour
{
    private DecalProjector _decalProjector;

    private void Awake ()
    {
        _decalProjector = GetComponent<DecalProjector> ();
    }

    public void SetWidth (float width)
    {
        _decalProjector.size = new Vector3 (width, _decalProjector.size.y, _decalProjector.size.z);
    }
}