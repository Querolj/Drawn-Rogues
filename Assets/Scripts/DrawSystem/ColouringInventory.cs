using System;
using System.Collections.Generic;
using UnityEngine;

public class ColouringInventory : MonoBehaviour
{
    [SerializeField]
    private List<Colouring> _startingColouring = new List<Colouring> ();

    public event Action<Colouring> OnColouringAdded;
    private List<Colouring> _colouringAvailable = null;

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

    private void Awake ()
    {
        _colouringAvailable = new List<Colouring> (_startingColouring);
    }

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

}