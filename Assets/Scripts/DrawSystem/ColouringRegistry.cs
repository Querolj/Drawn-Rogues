using System.Collections.Generic;
using UnityEngine;

public class CharColouringRegistry : MonoBehaviour
{
    public static CharColouringRegistry Instance { get; private set; }

    private List<CharacterColouring> _colourings;

    public Dictionary<int, CharacterColouring> ColouringsSourceById { get; private set; }
    public int MaxId { get; private set; }

    private void Awake ()
    {
        Instance = this;
        _colourings = new List<CharacterColouring> (Resources.LoadAll<CharacterColouring> ("Colouring/Character"));

        if (_colourings.Count == 0)
        {
            Debug.LogError ("No colouring found in Resources/Colouring folder");
            return;
        }

        _colourings.Sort ((a, b) => a.Id.CompareTo (b.Id));

        ColouringsSourceById = new Dictionary<int, CharacterColouring> ();
        foreach (CharacterColouring colouring in _colourings)
        {
            ColouringsSourceById.Add (colouring.Id, colouring);
        }

        MaxId = _colourings[_colourings.Count - 1].Id;
    }
}