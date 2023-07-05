using System.Collections.Generic;
using UnityEngine;

public enum ColouringType
{
    Character,
    Spell,
    Map
}

public class ColouringLoader : MonoBehaviour
{
    private Dictionary<BaseColor, List<Colouring>> _charColouringsByBaseColor = null;
    private Dictionary<BaseColor, List<Colouring>> _spellColouringsByBaseColor = null;

    public Dictionary<BaseColor, List<Colouring>> GetColourings (ColouringType colouringType)
    {
        Colouring[] colourings = null;
        switch (colouringType)
        {
            case ColouringType.Character:
                if (_charColouringsByBaseColor != null)
                    return _charColouringsByBaseColor;
                colourings = Resources.LoadAll<Colouring> ("Colouring/Character");
                break;
            case ColouringType.Spell:
                if (_spellColouringsByBaseColor != null)
                    return _spellColouringsByBaseColor;
                colourings = Resources.LoadAll<Colouring> ("Colouring/Spell");
                break;
            case ColouringType.Map:
                throw new System.NotImplementedException ();
        }

        if (colourings == null)
            throw new System.Exception ("No colourings found");

        Dictionary<BaseColor, List<Colouring>> colouringsByBaseColor = new Dictionary<BaseColor, List<Colouring>> ();

        foreach (Colouring colouring in colourings)
        {
            BaseColorDrops baseColorDrops = colouring.BaseColorsUsedPerPixel[0];
            if (!colouringsByBaseColor.ContainsKey (baseColorDrops.BaseColor))
            {
                colouringsByBaseColor.Add (baseColorDrops.BaseColor, new List<Colouring> ());
            }

            colouringsByBaseColor[baseColorDrops.BaseColor].Add (colouring);
        }

        switch (colouringType)
        {
            case ColouringType.Character:
                _charColouringsByBaseColor = colouringsByBaseColor;
                break;
            case ColouringType.Spell:
                _spellColouringsByBaseColor = colouringsByBaseColor;
                break;
            case ColouringType.Map:
                throw new System.NotImplementedException ();
        }

        return colouringsByBaseColor;
    }

    public int GetMaxColouringList (ColouringType colouringType)
    {
        Dictionary<BaseColor, List<Colouring>> colouringsByBaseColor = GetColourings (colouringType);
        int max = 0;
        foreach (KeyValuePair<BaseColor, List<Colouring>> pair in colouringsByBaseColor)
        {
            if (pair.Value.Count > max)
                max = pair.Value.Count;
        }
        return max;
    }
}