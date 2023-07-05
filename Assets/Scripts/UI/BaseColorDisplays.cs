using System.Collections.Generic;
using UnityEngine;

public class BaseColorDisplays : MonoBehaviour
{
    [SerializeField]
    private BaseColorDisplay _baseColorDisplayTemplate;

    private Dictionary<BaseColor, BaseColorDisplay> _baseColorDisplays = new Dictionary<BaseColor, BaseColorDisplay> ();

    public void Display (Dictionary<BaseColor, int> colorDropsAvailable)
    {
        foreach (var kvp in colorDropsAvailable)
        {
            BaseColorDisplay bcDisplay;

            if (!_baseColorDisplays.ContainsKey (kvp.Key))
            {
                bcDisplay = Instantiate (_baseColorDisplayTemplate, transform);
                _baseColorDisplays.Add (kvp.Key, bcDisplay);
            }
            else
                bcDisplay = _baseColorDisplays[kvp.Key];

            bcDisplay.Display (kvp.Key, kvp.Value);
        }
    }
}