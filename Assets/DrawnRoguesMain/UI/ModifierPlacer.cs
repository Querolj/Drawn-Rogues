using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ModifierPlacer : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _placeModifierInput;

    [SerializeField]
    private InputActionReference _flipModifierInput;

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
        _flipModifierInput.action.performed += FlipModifier;
        _placeModifierInput.action.performed += PlaceModifier;
    }

    private void OnDestroy ()
    {
        _flipModifierInput.action.performed -= FlipModifier;
        _placeModifierInput.action.performed -= PlaceModifier;
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

    private void FlipModifier (InputAction.CallbackContext context)
    {
        _isSelectionFlipped = !_isSelectionFlipped;
        _modifierPlacerImage.rectTransform.rotation = Quaternion.Euler (0f, _isSelectionFlipped ? 180f : 0f, 0f);
    }

    private void PlaceModifier (InputAction.CallbackContext context)
    {
        if (_activateDrawerAfterMouseUp)
        {
            _activateDrawerAfterMouseUp = false;
        }

        if (_selectedModifier == null)
        {
            return;
        }

        if (_isModifierInPosition)
        {
            Vector2 viewportMousePosition = _mainCamera.ScreenToViewportPoint (Mouse.current.position.ReadValue ());
            _characterCanvas.AddModifierFromViewport (_selectedModifier, viewportMousePosition, _isSelectionFlipped);
            RemoveModifierSelection ();
        }
    }

    private void Update ()
    {
        if (_selectedModifier == null)
        {
            return;
        }

        _modifierPlacerImage.transform.position = Mouse.current.position.ReadValue ();

        Vector2 topRightModifierPlacer = Mouse.current.position.ReadValue ();
        topRightModifierPlacer += _modifierPlacerImage.rectTransform.sizeDelta / 2f;
        topRightModifierPlacer = _mainCamera.ScreenToViewportPoint (topRightModifierPlacer);

        Vector2 bottomLeftModifierPlacer = Mouse.current.position.ReadValue ();
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
    }

    private bool IsPlacerImageInFrameBounds (Vector2 topRightModifierPlacer, Vector2 bottomLeftModifierPlacer)
    {
        return topRightModifierPlacer.x >= _bottomLeftScreenLimit.x && topRightModifierPlacer.x <= _topRightScreenLimit.x &&
            topRightModifierPlacer.y >= _bottomLeftScreenLimit.y && topRightModifierPlacer.y <= _topRightScreenLimit.y &&
            bottomLeftModifierPlacer.x >= _bottomLeftScreenLimit.x && bottomLeftModifierPlacer.x <= _topRightScreenLimit.x &&
            bottomLeftModifierPlacer.y >= _bottomLeftScreenLimit.y && bottomLeftModifierPlacer.y <= _topRightScreenLimit.y;
    }
}