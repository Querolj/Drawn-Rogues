using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputActionMapEnum
{
    Map,
    Combat,
    None
}

public class InputMapSwitcher : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset _mapActionMap;

    [SerializeField]
    private InputActionAsset _combatActionMap;

    private void Awake ()
    {
        _mapActionMap.Enable ();
        _combatActionMap.Disable ();
    }

    public void SwitchActionMap (InputActionMapEnum actionMap)
    {
        switch (actionMap)
        {
            case InputActionMapEnum.Map:
                _mapActionMap.Enable ();
                _combatActionMap.Disable ();
                break;
            case InputActionMapEnum.Combat:
                _mapActionMap.Disable ();
                _combatActionMap.Enable ();
                break;
            case InputActionMapEnum.None:
                _mapActionMap.Disable ();
                _combatActionMap.Disable ();
                break;
        }
    }
}