using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ColouringSlot : Slot
{
    [SerializeField]
    private TMP_Text _title;

    [SerializeField]
    private Image _colorTexImage;

    [SerializeField]
    private TMP_Text _dropNumText1;

    [SerializeField]
    private TMP_Text _dropNumText2;

    [SerializeField]
    private TMP_Text _description;

    private Colouring _colouring;
    public Colouring Colouring
    {
        get
        {
            return _colouring;
        }
    }

    public void Display (Colouring colouring)
    {
        _colouring = colouring ??
            throw new System.ArgumentNullException (nameof (colouring));

        _title.text = colouring.DisplayName;
        _colorTexImage.sprite = colouring.SpriteUI;

        _dropNumText1.text = BaseColorUtils.ColorText (colouring.BaseColorsUsedPerPixel[0].TotalDrops.ToString (), colouring.BaseColorsUsedPerPixel[0].BaseColor);

        if (colouring.BaseColorsUsedPerPixel.Count > 1)
        {
            _dropNumText2.gameObject.SetActive (true);
            _dropNumText2.text = BaseColorUtils.ColorText (colouring.BaseColorsUsedPerPixel[1].TotalDrops.ToString (), colouring.BaseColorsUsedPerPixel[1].BaseColor);
        }
        else
        {
            _dropNumText2.transform.parent.gameObject.SetActive (false);
        }

        _description.text = colouring.Description;
    }
}