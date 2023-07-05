using System.Collections.Generic;
using UnityEngine;

public class CharacterColorsSelections : MonoBehaviour
{
    [SerializeField]
    private List<CharacterColouring> _charColorings; // Debug

    [SerializeField]
    private CharacterColorSelection _colorSelectionTemplate;

    [SerializeField]
    private Transform _contentTransform;

    [SerializeField]
    private SelectedColors _selectedColors;

    private void Start ()
    {
        foreach (CharacterColouring charColoring in _charColorings)
        {
            CharacterColorSelection colorSelect = Instantiate (_colorSelectionTemplate);
            colorSelect.Init (charColoring.Name, charColoring.TextureUI, () =>
            {
                if (_selectedColors.TryAddColor (charColoring, () => colorSelect.gameObject.SetActive (true)))
                    colorSelect.gameObject.SetActive (false);
            });
            colorSelect.transform.SetParent (_contentTransform);
            colorSelect.transform.localScale = Vector3.one;
            colorSelect.transform.localPosition =
                new Vector3 (colorSelect.transform.localPosition.x, colorSelect.transform.localPosition.y, 0f);
        }
    }
}