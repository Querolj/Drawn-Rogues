using System;
using System.Collections.Generic;

public class BaseColorInventory
{
    #region Player inventory

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
            OnValueChanged?.Invoke (_colorDropsAvailable);
        }
    }
    #endregion

    public event Action<Dictionary<BaseColor, int>> OnValueChanged;

    private void Awake ()
    {
        AddColorDrops (new Dictionary<BaseColor, int> () { { BaseColor.Green, 10000 }, { BaseColor.Purple, 10000 } });
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

        OnValueChanged?.Invoke (_colorDropsAvailable);
    }

    public int GetMaxDrawablePixelsFromColouring (List<BaseColorDrops> drops)
    {
        int minPixelNumber = int.MaxValue;
        foreach (BaseColorDrops baseColorQuantity in drops)
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

    public void RemoveBaseColorDrops (List<BaseColorDrops> drops, int totalPixelsDrawed)
    {
        foreach (BaseColorDrops baseColorQuantity in drops)
        {
            if (!_colorDropsAvailable.ContainsKey (baseColorQuantity.BaseColor))
                throw new Exception ("Trying to remove a color that is not in the palette");

            BaseColor bc = baseColorQuantity.BaseColor;
            if (_colorDropsAvailable[bc] < totalPixelsDrawed * baseColorQuantity.TotalDrops)
                throw new Exception ("Trying to remove more color than available");

            _colorDropsAvailable[bc] -= totalPixelsDrawed * baseColorQuantity.TotalDrops;
        }

        OnValueChanged?.Invoke (_colorDropsAvailable);
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