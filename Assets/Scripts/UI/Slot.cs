using System;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [SerializeField]
    private Toggle _toggle;

    public void SetToggleGroup (ToggleGroup toggleGroup)
    {
        _toggle.group = toggleGroup;
    }

    public void SetOnClick (Action onClick)
    {
        _toggle.onValueChanged.AddListener ((bool isOn) =>
        {
            if (isOn)
            {
                onClick?.Invoke ();
            }
        });
    }
}