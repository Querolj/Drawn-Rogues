using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Page : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _title;

    [SerializeField]
    private ColouringSlot[] _colouringSlots;

    [SerializeField]
    private GameObject[] _separator;

    public int AvailableColouringSlot
    {
        get { return _colouringSlots.Length; }
    }

    public void Display (List<Colouring> colourings)
    {
        if (colourings == null)
            throw new System.ArgumentNullException (nameof (colourings));

        if (colourings.Count > _colouringSlots.Length)
            throw new System.ArgumentException ("Too many colourings to display", nameof (colourings));

        if (_separator.Length != _colouringSlots.Length - 1)
            throw new System.ArgumentException ("Separator count must be equal to colouring slot count - 1", nameof (_separator));

        for (int i = 0; i < _colouringSlots.Length; i++)
        {
            if (i < colourings.Count)
            {
                _colouringSlots[i].Display (colourings[i]);
            }
            else
            {
                _colouringSlots[i].gameObject.SetActive (false);
            }

            if (i != _colouringSlots.Length - 1)
            {
                _separator[i].SetActive (i < colourings.Count - 1);
            }
        }

        if (_title != null)
        {
            BaseColor baseColor = colourings[0].BaseColorsUsedPerPixel[0].BaseColor;
            _title.text = BaseColorToColor.ColorText (BaseColorToColor.GetColorName (baseColor).ToUpper (), baseColor);
        }
    }

}