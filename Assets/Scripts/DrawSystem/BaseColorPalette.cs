using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseColorPalette : MonoBehaviour
{
    [SerializeField]
    private List<Colouring> _startingColouring = new List<Colouring> ();

    [SerializeField]
    private BaseColorDisplays _baseColorDisplays;

    #region Player inventory
    public event Action<Colouring> OnColouringAdded;
    private List<Colouring> _colouringAvailable = null;
    public void AddColouring (Colouring colouring)
    {
        if (_colouringAvailable.Contains (colouring))
            throw new Exception ("Trying to add a colouring that is already in the palette");

        _colouringAvailable.Add (colouring);
        OnColouringAdded?.Invoke (colouring);
    }

    public bool HasColouring (Colouring colouring)
    {
        return _colouringAvailable.Contains (colouring);
    }

    public Dictionary<BaseColor, List<Colouring>> ColouringAvailableByBaseColor
    {
        get
        {
            Dictionary<BaseColor, List<Colouring>> colouringsByBaseColor = new Dictionary<BaseColor, List<Colouring>> ();

            foreach (Colouring colouring in _colouringAvailable)
            {
                BaseColorDrops baseColorDrops = colouring.BaseColorsUsedPerPixel[0];
                if (!colouringsByBaseColor.ContainsKey (baseColorDrops.BaseColor))
                {
                    colouringsByBaseColor.Add (baseColorDrops.BaseColor, new List<Colouring> ());
                }

                colouringsByBaseColor[baseColorDrops.BaseColor].Add (colouring);
            }

            return colouringsByBaseColor;
        }
    }

    private Dictionary<BaseColor, int> _colorDropsAvailable = new Dictionary<BaseColor, int> ();
    public Dictionary<BaseColor, int> ColorDropsAvailable
    {
        get
        {
            return _colorDropsAvailable;
        }
        set
        {
            _colorDropsAvailable = value;
            _baseColorDisplays.Display (_colorDropsAvailable);
        }
    }
    #endregion

    private void Awake ()
    {
        _colouringAvailable = new List<Colouring> (_startingColouring);
    }

    public void AddColorDrops (Dictionary<BaseColor, int> colorDrops)
    {
        foreach (var kvp in colorDrops)
        {
            if (!_colorDropsAvailable.ContainsKey (kvp.Key))
                _colorDropsAvailable.Add (kvp.Key, kvp.Value);
            else
                _colorDropsAvailable[kvp.Key] += kvp.Value;
        }

        _baseColorDisplays.Display (_colorDropsAvailable);
    }

    public int GetMaxDrawablePixelsFromColouring (Colouring colouring)
    {
        int minPixelNumber = int.MaxValue;
        foreach (BaseColorDrops baseColorQuantity in colouring.BaseColorsUsedPerPixel)
        {
            if (!_colorDropsAvailable.ContainsKey (baseColorQuantity.BaseColor))
                return 0;

            int colorDropAvailable = _colorDropsAvailable[baseColorQuantity.BaseColor];
            int pixelNumber = colorDropAvailable / baseColorQuantity.TotalDrops;
            if (pixelNumber < minPixelNumber)
            {
                minPixelNumber = pixelNumber;
            }
        }
        return minPixelNumber;
    }

    public void RemoveBaseColorDrops (Colouring colouring, int totalPixelsDrawed)
    {
        foreach (BaseColorDrops baseColorQuantity in colouring.BaseColorsUsedPerPixel)
        {
            if (!_colorDropsAvailable.ContainsKey (baseColorQuantity.BaseColor))
                throw new Exception ("Trying to remove a color that is not in the palette");

            BaseColor bc = baseColorQuantity.BaseColor;
            if (_colorDropsAvailable[bc] < totalPixelsDrawed * baseColorQuantity.TotalDrops)
                throw new Exception ("Trying to remove more color than available");

            _colorDropsAvailable[bc] -= totalPixelsDrawed * baseColorQuantity.TotalDrops;
        }

        _baseColorDisplays.Display (_colorDropsAvailable);
    }

    public int TotalColorQuantityLeft ()
    {
        int total = 0;
        foreach (int colorQuantity in _colorDropsAvailable.Values)
        {
            total += colorQuantity;
        }
        return total;
    }
}