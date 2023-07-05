using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CharacterColorSelection : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _colorName;

    [SerializeField]
    private Image _colorTexture;

    [SerializeField]
    private Button _addButton;

    public void Init (string colorName, Texture2D colorTex, Action onColorSelected)
    {
        if (string.IsNullOrEmpty (colorName))
            throw new ArgumentNullException (nameof (colorName));
        if (colorTex == null)
            throw new ArgumentNullException (nameof (colorTex));
        if (onColorSelected == null)
            throw new ArgumentNullException (nameof (onColorSelected));

        _colorName.text = colorName;
        _colorTexture.sprite = Sprite.Create (colorTex, _colorTexture.sprite.rect, Vector2.one / 2f, _colorTexture.sprite.pixelsPerUnit);
        _addButton.onClick.AddListener (() => onColorSelected.Invoke ());
    }
}