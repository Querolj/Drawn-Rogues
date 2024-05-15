using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackableStatesUI : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField]
    private Image _fillImage;

    [SerializeField]
    private TMP_Text _maxLifeText;

    [SerializeField]
    private TMP_Text _currentLifeText;

    [SerializeField]
    private Slider _sliderHealthBar;

    [SerializeField]
    private StatusIconsDisplayer _statusIconsDisplayer;

    [SerializeField]
    private float _updateHealthPerSecond = 10f;

    [SerializeField]
    private float _zoomedScale = 1.6f;
    #endregion

    public bool IsSliderValueUpdating { get { return _attackable.Stats.AttackableState.CurrentLife != Mathf.RoundToInt (_sliderHealthBar.value); } }

    private Attackable _attackable;

    private Dictionary < (float, float), Color > _colorPerPercentage = new Dictionary < (float, float), Color > ()
    {
        {
        (1f, 0.5f),
        Color.green
        },
        {
        (0.5f, 0.25f),
        Color.yellow
        },
        {
        (0.25f, 0.1f),
        new Color (1f, 140f / 255f, 0f)
        },
        {
        (0.1f, 0f),
        Color.red
        }
    };

    private bool _initialized = false;
    private Vector3 _initialOffsetFromAttackable;
    private void Update ()
    {
        if (!_initialized)
            return;

        if (IsSliderValueUpdating)
        {
            float updateHealthPerSecondSigned = _updateHealthPerSecond * (_attackable.Stats.AttackableState.CurrentLife >= _sliderHealthBar.value ? 1 : -1);
            float normalizedLife = _sliderHealthBar.value / _attackable.Stats.Life;
            float newNormalizedHealthBarValue = normalizedLife + updateHealthPerSecondSigned * Time.deltaTime;

            if (updateHealthPerSecondSigned < 0)
                _sliderHealthBar.value = Mathf.Clamp (newNormalizedHealthBarValue * _attackable.Stats.Life, _attackable.Stats.AttackableState.CurrentLife, _attackable.Stats.Life);
            else
                _sliderHealthBar.value = Mathf.Clamp (newNormalizedHealthBarValue * _attackable.Stats.Life, 0, _attackable.Stats.AttackableState.CurrentLife);

            if (!IsSliderValueUpdating)
                _sliderHealthBar.value = _attackable.Stats.AttackableState.CurrentLife;

            UpdateBarColor ();
        }

        UpdatePosition ();
    }

    private void UpdatePosition ()
    {
        Vector3 newPos = _attackable.transform.position + _initialOffsetFromAttackable;
        transform.position = newPos;
    }

    private void UpdateBarColor ()
    {
        foreach ((float maxPerc, float minPerc) in _colorPerPercentage.Keys)
        {
            float perc = _sliderHealthBar.value / _sliderHealthBar.maxValue;
            if (perc > minPerc && perc <= maxPerc)
            {
                _fillImage.color = _colorPerPercentage[(maxPerc, minPerc)];
            }
        }
        _currentLifeText.text = ((int) _sliderHealthBar.value).ToString ();
    }

    #region Public Methods    
    public void UpdateStatusIcons ()
    {
        _statusIconsDisplayer.DisplayStatus (_attackable.TempEffectsList);
    }

    public void Init (Attackable attackable)
    {
        _attackable = attackable ??
            throw new System.ArgumentNullException (nameof (attackable));
        _initialOffsetFromAttackable = transform.position - _attackable.transform.position;

        _sliderHealthBar.maxValue = _attackable.Stats.Life;
        _sliderHealthBar.minValue = 0;
        _sliderHealthBar.value = _attackable.Stats.AttackableState.CurrentLife;
        _maxLifeText.text = _attackable.Stats.Life.ToString ();
        _currentLifeText.text = _attackable.Stats.AttackableState.CurrentLife.ToString ();
        UpdateBarColor ();
        _initialized = true;
    }

    public void Zoom ()
    {
        transform.localScale = Vector3.one * _zoomedScale;
        transform.SetAsLastSibling ();
    }

    public void Unzoom ()
    {
        transform.localScale = Vector3.one;
    }
    #endregion
}