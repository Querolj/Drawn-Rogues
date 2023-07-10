using UnityEngine;

[CreateAssetMenu (fileName = "CharacterColouring", menuName = "Colouring/CharacterColouring", order = 1)]
public class CharacterColouring : Colouring
{
    public bool BaseBonusToMainStats = true; // Bonus to main stats or not
    public float KilogramPerPixel = 0.1f;
    public StatsSerialized Stats;

}