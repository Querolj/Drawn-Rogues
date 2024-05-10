using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MoveIndicator : MonoBehaviour
{
    private float _boundExtendY;

    [SerializeField]
    private float _speed = 1f;

    [SerializeField]
    private DecalProjector _decalProjector = null;

    [SerializeField]
    private GameObject _arrowIndicator;

    [SerializeField]
    private CombatMoveIndicator _combatMoveIndicator;

    [SerializeField]
    private GameObject _moveDecal;

    [SerializeField]
    private float _circleSize = 10f;

    private float _heightMaxOffset = 0.1f;
    private float _heightMinOffset = 0f;
    private float _heightCurrentOffset = 0f;
    private bool _arrowMoveUp = true;
    public event Action<Vector3> OnPositionSet;

    private void Awake ()
    {
        _heightCurrentOffset = _heightMinOffset;
        DeactivateCombatMode ();
    }

    private void FixedUpdate ()
    {
        if (_arrowIndicator.activeSelf)
            UpdateArrowAndDecalHeight ();
    }

    private void UpdateArrowAndDecalHeight ()
    {
        UpdateHeightOffset ();
        _decalProjector.size = new Vector3 (_heightCurrentOffset * _circleSize, _heightCurrentOffset * _circleSize, _decalProjector.size.z);
        Vector3 targetPosition = _arrowIndicator.transform.position;
        targetPosition.y = Utils.GetMapHeight (_arrowIndicator.transform.position) + _boundExtendY + _heightCurrentOffset;

        _arrowIndicator.transform.position = targetPosition;
    }

    private void UpdateHeightOffset ()
    {
        if (_arrowMoveUp)
        {
            _heightCurrentOffset += +_speed * Time.fixedDeltaTime;
            if (_heightCurrentOffset >= _heightMaxOffset)
            {
                _heightCurrentOffset = _heightMaxOffset;
                _arrowMoveUp = false;
            }
        }
        else
        {
            _heightCurrentOffset -= _speed * Time.fixedDeltaTime;
            if (_heightCurrentOffset <= _heightMinOffset)
            {
                _heightCurrentOffset = _heightMinOffset;
                _arrowMoveUp = true;
            }
        }
    }

    public void SetPosition (Vector3 position)
    {
        position.y = Utils.GetMapHeight (_arrowIndicator.transform.position) + _boundExtendY;
        _combatMoveIndicator.transform.position = position;
        if (_arrowIndicator.activeSelf)
            position.y += _heightCurrentOffset;

        _arrowIndicator.transform.position = position;
        OnPositionSet?.Invoke (position);
    }

    public void ActiveCombatMode (Bounds charBounds)
    {
        _boundExtendY = charBounds.extents.y;
        _moveDecal.SetActive (false);
        _combatMoveIndicator.gameObject.SetActive (true);
        _combatMoveIndicator.SetSizeFromCharacter (charBounds);
        _arrowIndicator.SetActive (true);
    }

    public void DeactivateCombatMode ()
    {
        _boundExtendY = _arrowIndicator.GetComponent<SpriteRenderer> ().bounds.extents.y;
        _moveDecal.SetActive (true);
        _combatMoveIndicator.gameObject.SetActive (false);
        _arrowIndicator.SetActive (true);
    }

    public void Hide()
    {
        _arrowIndicator.SetActive (false);
        _moveDecal.SetActive (false);
        _combatMoveIndicator.gameObject.SetActive (false);
    }

    public Vector3 GetPosition ()
    {
        return _arrowIndicator.transform.position;
    }
}