using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _spellName;

    [SerializeField]
    private TMP_Text _spellColorCost;

    [SerializeField]
    private Button _button;
    public Button Button { get { return _button; } }

    public void Display (ColouringSpell spell)
    {
        _spellName.text = spell.DisplayName;
        BaseColorDrops colorDrop = spell.BaseColorsUsedPerPixel[0];
        _spellColorCost.text = BaseColorUtils.ColorText (colorDrop.TotalDrops.ToString (), colorDrop.BaseColor);
    }
}