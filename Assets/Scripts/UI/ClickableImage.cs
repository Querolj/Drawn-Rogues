using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent (typeof (Image))]
public class ClickableImage : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Sprite _idleSprite;

    [SerializeField]
    private Sprite _clickedSprite;

    [SerializeField]
    private Sprite _hoverSprite;

    public Action OnClick;

    private Image _image;
    private bool _clicked = false;

    private void Awake ()
    {
        _image = GetComponent<Image> ();
    }

    void IPointerClickHandler.OnPointerClick (PointerEventData eventData)
    {
        OnClick?.Invoke ();

        if (_clickedSprite != null)
            _image.sprite = _clickedSprite;

        _clicked = true;
    }

    public void ActivateIdleImage ()
    {
        _image.sprite = _idleSprite;
        _clicked = false;
    }

    public void ActivateClickedImage ()
    {
        _image.sprite = _clickedSprite;
    }

    public void OnPointerEnter (PointerEventData eventData)
    {
        if (!_clicked && _hoverSprite != null)
            _image.sprite = _hoverSprite;
    }

    public void OnPointerExit (PointerEventData eventData)
    {
        if (!_clicked && _idleSprite != null)
            _image.sprite = _idleSprite;
    }
}