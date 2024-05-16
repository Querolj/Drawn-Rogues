using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu (fileName = "CharacterColouring", menuName = "Colouring/CharacterColouring", order = 1)]
public class CharacterColouring : Colouring
{
    [SerializeField, BoxGroup ("Stats")]
    private bool _baseBonusToMainStats = true; // Bonus to main stats or not
    public bool BaseBonusToMainStats => _baseBonusToMainStats;

    [SerializeField, BoxGroup ("Stats")]
    private float _kilogramPerPixel = 0.001f;
    public float KilogramPerPixel => _kilogramPerPixel;

    [SerializeField, BoxGroup ("Stats")]
    private StatsSerialized _stats;
    public StatsSerialized Stats => _stats;
}