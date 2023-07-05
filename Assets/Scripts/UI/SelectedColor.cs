using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedColor : MonoBehaviour
{

    [SerializeField]
    private TMP_Text _colorName;

    [SerializeField]
    private Image _colorTexture;

    [SerializeField]
    private Button _addButton;

    [SerializeField]
    private Button _removeButton;

    [SerializeField]
    private Slider _slider;

    public int SliderMultiplicatorValue
    {
        get { return (int) _slider.value; }
    }

    private Colouring _colouring;
    public Colouring Colouring
    {
        get { return _colouring; }
    }

    public void Init (Colouring c, Action onAdd, Action onRemove)
    {
        if (c == null)
            throw new System.ArgumentNullException (nameof (c));

        if (onAdd == null)
            throw new System.ArgumentNullException (nameof (onAdd));

        if (onRemove == null)
            throw new System.ArgumentNullException (nameof (onRemove));

        _colouring = c;
        _colorName.text = c.Name;
        _slider.value = 3;
        _colorTexture.sprite = Sprite.Create (c.TextureUI, _colorTexture.sprite.rect, Vector2.one / 2f, _colorTexture.sprite.pixelsPerUnit);
        _addButton.onClick.AddListener (() => onAdd.Invoke ());
        _removeButton.onClick.AddListener (() => onRemove.Invoke ());
    }

    public void UpdateSliderMultValue (int mult)
    {
        _slider.value = mult;
    }

    public void UpdateAddButtonDisplay (bool display)
    {
        _addButton.gameObject.SetActive (display);
    }
}