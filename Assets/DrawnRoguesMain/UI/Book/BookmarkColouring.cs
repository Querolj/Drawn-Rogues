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

    private BaseColor _baseColor;
    public BaseColor BaseColor => _baseColor;

    public void Init (BaseColor bc, Action onClick)
    {
        _button.onClick.AddListener (() => onClick ());
        _baseColor = bc;
        _unselectedBookmarkColorRend.color = BaseColorUtils.GetColor (bc);
        _selectedBookmarkColorRend.color = BaseColorUtils.GetColor (bc);
    }

    public void SetSelected (bool isSelected)
    {
        _selectedBookmarkColorRend.gameObject.SetActive (isSelected);
        _unselectedBookmarkColorRend.gameObject.SetActive (!isSelected);
    }
}