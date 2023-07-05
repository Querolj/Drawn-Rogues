using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ModifierPlacer : MonoBehaviour
{
    [SerializeField]
    private Image _modifierPlacerImage;

    [SerializeField]
    private CharacterCanvas _characterCanvas;

    private Modifier _selectedModifier;
    private Camera _mainCamera;

    private Vector3 _bottomLeftScreenLimit;
    private Vector3 _topRightScreenLimit;

    private bool _isModifierInPosition = false;
    private bool _activateDrawerAfterMouseUp = false;
    private bool _isSelectionFlipped = false;

    private void Awake ()
    {
        _mainCamera = Camera.main;
    }

    public bool TrySetModifier (Modifier modifier, Image modifierImage)
    {
        if (!_characterCanvas.CanHaveMoreModifier ())
            return false;

        _selectedModifier = modifier;
        _modifierPlacerImage.sprite = modifier.Sprite;
        _modifierPlacerImage.rectTransform.sizeDelta = modifierImage.rectTransform.sizeDelta;
        _modifierPlacerImage.gameObject.SetActive (true);

        _bottomLeftScreenLimit = _characterCanvas.ViewportBottomLeftFramePos;
        _topRightScreenLimit = _characterCanvas.ViewportTopRightFramePos;

        return true;
    }

    private void RemoveModifierSelection ()
    {
        _selectedModifier = null;
        _modifierPlacerImage.sprite = null;
        _activateDrawerAfterMouseUp = true;
        _modifierPlacerImage.gameObject.SetActive (false);
        _isSelectionFlipped = false;
    }

    private void FlipModifier ()
    {
        _modifierPlacerImage.rectTransform.rotation = Quaternion.Euler (0f, _isSelectionFlipped ? 180f : 0f, 0f);
    }

    private void Update ()
    {
        if (Input.GetMouseButtonUp (0))
        {
            if (_activateDrawerAfterMouseUp)
            {
                _activateDrawerAfterMouseUp = false;
            }
        }

        if (Input.GetKeyDown (KeyCode.F))
        {
            _isSelectionFlipped = !_isSelectionFlipped;
            FlipModifier ();
        }

        if (_selectedModifier == null)
        {
            return;
        }

        _modifierPlacerImage.transform.position = Input.mousePosition;

        Vector2 viewportMousePosition = _mainCamera.ScreenToViewportPoint (Input.mousePosition);

        Vector2 topRightModifierPlacer = Input.mousePosition;
        topRightModifierPlacer += _modifierPlacerImage.rectTransform.sizeDelta / 2f;
        topRightModifierPlacer = _mainCamera.ScreenToViewportPoint (topRightModifierPlacer);

        Vector2 bottomLeftModifierPlacer = Input.mousePosition;
        bottomLeftModifierPlacer -= _modifierPlacerImage.rectTransform.sizeDelta / 2f;
        bottomLeftModifierPlacer = _mainCamera.ScreenToViewportPoint (bottomLeftModifierPlacer);

        if (IsPlacerImageInFrameBounds (topRightModifierPlacer, bottomLeftModifierPlacer))
        {
            _modifierPlacerImage.color = Color.white;
            _isModifierInPosition = true;

        }
        else
        {
            _modifierPlacerImage.color = Color.red;
            _isModifierInPosition = false;
        }

        if (Input.GetMouseButtonDown (0))
        {
            if (_isModifierInPosition)
            {
                _characterCanvas.AddModifierFromViewport (_selectedModifier, viewportMousePosition, _isSelectionFlipped);
                RemoveModifierSelection ();
            }
        }
    }

    private bool IsPlacerImageInFrameBounds (Vector2 topRightModifierPlacer, Vector2 bottomLeftModifierPlacer)
    {
        return topRightModifierPlacer.x >= _bottomLeftScreenLimit.x && topRightModifierPlacer.x <= _topRightScreenLimit.x &&
            topRightModifierPlacer.y >= _bottomLeftScreenLimit.y && topRightModifierPlacer.y <= _topRightScreenLimit.y &&
            bottomLeftModifierPlacer.x >= _bottomLeftScreenLimit.x && bottomLeftModifierPlacer.x <= _topRightScreenLimit.x &&
            bottomLeftModifierPlacer.y >= _bottomLeftScreenLimit.y && bottomLeftModifierPlacer.y <= _topRightScreenLimit.y;
    }
}