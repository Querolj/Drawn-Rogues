using System;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkColouring : MonoBehaviour
{
    [SerializeField]
    private Image _unselectedBookmarkColorRend;

    [SerializeField]
    private Image _selectedBookmarkColorRend;

    [SerializeField]
    private Button _button;

    public void Init (BaseColor bc, Action onClick)
    {
        _button.onClick.AddListener (() => onClick ());
        _unselectedBookmarkColorRend.color = BaseColorToColor.GetColor (bc);
        _selectedBookmarkColorRend.color = BaseColorToColor.GetColor (bc);
    }

    public void SetSelected (bool isSelected)
    {
        _selectedBookmarkColorRend.gameObject.SetActive (isSelected);
        _unselectedBookmarkColorRend.gameObject.SetActive (!isSelected);
    }
}