using UnityEngine;

[CreateAssetMenu (fileName = "ColouringSpell", menuName = "Colouring/ColouringSpell", order = 1)]
public class ColouringSpell : Colouring
{
    public GameObject BehaviourPrefab;
    public bool ClearMetadataOnFrame = false;
}