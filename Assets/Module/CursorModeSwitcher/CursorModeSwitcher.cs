using System;
using UnityEngine;

public class CursorModeSwitcher : MonoBehaviour
{
    public enum Mode
    {
        Selection,
        Draw
    }

    [SerializeField]
    private CursorSkin _cursorSkin;
    public Action<Mode> OnChangeMode;

    private Mode _currentMode = Mode.Selection;
    public Mode CurrentMode { get { return _currentMode; } }
    
    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Start ()
    {
        ChangeMode (Mode.Selection);
    }

    public void ChangeMode (Mode mode)
    {
        _currentMode = mode;
        OnChangeMode?.Invoke (_currentMode);

        return;
        switch (_currentMode)
        {
            case Mode.Selection:
                _cursorSkin.SetCursorType (CursorSkin.CursorType.Select);
                break;
            case Mode.Draw:
                _cursorSkin.SetCursorType (CursorSkin.CursorType.Draw);
                break;
            default:
                Debug.LogError ("Unknown mode: " + _currentMode);
                break;
        }

    }

    public void SetSelectionMode ()
    {
        ChangeMode (Mode.Selection);
    }

    public void SetDrawMode ()
    {
        ChangeMode (Mode.Draw);
    }
}