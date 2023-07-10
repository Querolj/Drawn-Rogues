using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class BaseColorDisplays : MonoBehaviour
{
    [SerializeField]
    private BaseColorDisplay _baseColorDisplayTemplate;

    private Dictionary<BaseColor, BaseColorDisplay> _baseColorDisplays = new Dictionary<BaseColor, BaseColorDisplay> ();

    [Inject, UsedImplicitly]
    private void Init (BaseColorInventory baseColorInventory)
    {
        baseColorInventory.OnValueChanged += Display;
    }

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