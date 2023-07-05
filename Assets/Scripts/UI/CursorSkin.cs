using UnityEngine;

public class CursorSkin : MonoBehaviour
{
    public enum CursorType
    {
        Select,
        Draw
    }

    [SerializeField]
    private Texture2D _selectCursorTex;

    [SerializeField]
    private Texture2D _drawCursorTex;

    private CursorType _currentCursorType = CursorType.Select;

    public void SetCursorType (CursorType cursorType)
    {
        _currentCursorType = cursorType;

        switch (_currentCursorType)
        {
            case CursorType.Select:
                Cursor.SetCursor (_selectCursorTex, Vector2.zero, CursorMode.ForceSoftware);
                break;
            case CursorType.Draw:
                Cursor.SetCursor (_drawCursorTex, Vector2.zero, CursorMode.ForceSoftware);
                break;
            default:
                Debug.LogError ("Unknown cursor type: " + _currentCursorType);
                break;
        }
    }
}