using UnityEngine;

public class FreeDrawUI : MonoBehaviour
{
    [SerializeField]
    private SpellButtonGenerator _spellButtonGen;

    public void Init (DrawedCharacter character)
    {
        _spellButtonGen.GenerateButtons (character);
    }
}