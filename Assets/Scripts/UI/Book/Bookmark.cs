using System;
using UnityEngine;
using UnityEngine.UI;

public class Bookmark : MonoBehaviour
{
    [SerializeField]
    private Button _bookmarkButton;

    [SerializeField]
    private GameObject _bookmarkImageSelected;

    [SerializeField]
    private GameObject _bookmarkImageIdle;

    public void Init (System.Action onClick)
    {
        _bookmarkButton.onClick.AddListener (() => onClick ());
    }

    public void SetSelected (bool isSelected)
    {
        _bookmarkImageSelected.SetActive (isSelected);
        _bookmarkImageIdle.SetActive (!isSelected);
    }
}