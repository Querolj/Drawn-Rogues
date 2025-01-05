using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "ColouringSpell", menuName = "Colouring/ColouringSpell", order = 1)]
public class ColouringSpell : Colouring
{
    [SerializeField, BoxGroup ("References")]
    public GameObject BehaviourPrefab;
}