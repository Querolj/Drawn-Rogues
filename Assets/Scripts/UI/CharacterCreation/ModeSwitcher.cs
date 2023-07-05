using UnityEngine;

public class ModeSwitcher : MonoBehaviour
{
    public enum Mode
    {
        Selection,
        Draw
    }

    [SerializeField]
    private Drawer _drawer;

    [SerializeField]
    private CursorSkin _cursorSkin;

    private Mode _currentMode = Mode.Selection;
    public Mode CurrentMode { get { return _currentMode; } }

    private void Start ()
    {
        ChangeMode (Mode.Selection);
    }

    public void ChangeMode (Mode mode)
    {
        _currentMode = mode;

        switch (_currentMode)
        {
            case Mode.Selection:
                _drawer.Activate (false);
                _cursorSkin.SetCursorType (CursorSkin.CursorType.Select);
                break;
            case Mode.Draw:
                _drawer.Activate (true);
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