using UnityEngine;

public class QuickBookManager : MonoBehaviour
{
    [SerializeField]
    private QuickBookColouring _quickBookCharColor;

    [SerializeField]
    private QuickBookModifier _quickBookModifier;

    [SerializeField]
    private Bookmark _bookmarkCharColor;

    [SerializeField]
    private Bookmark _bookmarkModifier;

    [SerializeField]
    private ModeSwitcher _modeSwitcher;

    private void Awake ()
    {
        _bookmarkCharColor.Init (() =>
        {
            OpenQuickBook (BookType.CharColouring);
        });
        _bookmarkModifier.Init (() =>
        {
            OpenQuickBook (BookType.Modifier);
        });
    }

    private void Start ()
    {
        OpenQuickBook (BookType.CharColouring);
    }

    public void OpenQuickBook (BookType bookType)
    {
        _modeSwitcher.ChangeMode (bookType == BookType.Modifier ? ModeSwitcher.Mode.Selection : ModeSwitcher.Mode.Draw);
        _quickBookCharColor.gameObject.SetActive (bookType == BookType.CharColouring);
        _quickBookModifier.gameObject.SetActive (bookType == BookType.Modifier);
        _bookmarkCharColor.SetSelected (bookType == BookType.CharColouring);
        _bookmarkModifier.SetSelected (bookType == BookType.Modifier);
    }
}