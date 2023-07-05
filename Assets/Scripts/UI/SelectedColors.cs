using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectedColors : MonoBehaviour
{
    [SerializeField]
    private SelectedColor _selectedColorTemplate;

    [SerializeField]
    private TMP_Text _colorsChosenText;

    private List<Colouring> _colouring = new List<Colouring> ();
    private List<SelectedColor> _selectedColors = new List<SelectedColor> ();

    public bool TryAddColor (Colouring c, Action onColorRemoved)
    {
        if (_colouring.Count >= 3 || _colouring.Contains (c))
            return false;

        if (c == null)
            throw new ArgumentNullException (nameof (c));

        SelectedColor sc = Instantiate (_selectedColorTemplate);
        sc.transform.SetParent (transform);
        sc.transform.localPosition = new Vector3 (sc.transform.localPosition.x, sc.transform.localPosition.y, 0f);
        sc.transform.localScale = Vector3.one;
        sc.Init (c, () => UpdateSelectedColorsSlider (sc), () =>
        {
            DeleteSelectedColor (sc);
            onColorRemoved?.Invoke ();
        });

        _colouring.Add (c);
        _selectedColors.Add (sc);

        UpdateSelectedColorsSlider (sc);
        UpdateColorChosenText ();

        return true;
    }

    private void UpdateColorChosenText ()
    {
        _colorsChosenText.text = "Colors chosen : " + _colouring.Count + "/3";
    }

    private void MoreColor (SelectedColor sc)
    {
        UpdateSelectedColorsSlider (sc);
    }

    private void DeleteSelectedColor (SelectedColor sc)
    {
        _selectedColors.Remove (sc);
        _colouring.Remove (sc.Colouring);

        DestroyImmediate (sc.gameObject);
        if (_selectedColors.Count >= 1)
            UpdateSelectedColorsSlider (_selectedColors[0]);

        UpdateColorChosenText ();
    }

    private void UpdateSelectedColorsSlider (SelectedColor lastSc)
    {
        foreach (SelectedColor sc in _selectedColors)
        {
            sc.UpdateSliderMultValue (1);
            sc.UpdateAddButtonDisplay (false);
        }

        if (_selectedColors.Count == 1)
        {
            lastSc.UpdateSliderMultValue (3);
            return;
        }
        else if (_selectedColors.Count == 2)
        {
            lastSc.UpdateSliderMultValue (2);
            foreach (SelectedColor sc in _selectedColors)
            {
                if (sc.SliderMultiplicatorValue == 1)
                    sc.UpdateAddButtonDisplay (true);
                else
                    sc.UpdateAddButtonDisplay (false);
            }
        }
    }

    public List<SelectedColor> GetSelectedColors ()
    {
        return _selectedColors;
    }
}