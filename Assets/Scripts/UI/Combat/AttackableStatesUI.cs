using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackableStatesUI : MonoBehaviour
{
    private Attackable _attackable;

    [SerializeField]
    private Image _fillImage;

    [SerializeField]
    private TMP_Text _maxLifeText;

    [SerializeField]
    private TMP_Text _currentLifeText;

    [SerializeField]
    private Slider _sliderHealthBar;

    [SerializeField]
    private StateIconsDisplayer _stateIconsDisplayer;

    public float CurrentValue
    {
        get { return _sliderHealthBar.value; }
    }

    private float _lerpTotalTime = 0.5f;
    private float _lerp = 0f;
    private float _lastTargetValue;
    private bool _changeSliderValue = false;

    private Dictionary < (float, float), Color > _colorPerPercentage = new Dictionary < (float, float), Color > ()
    {
        {
        (1f, 0.5f), Color.green
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

    private System.Collections.IEnumerator Start ()
    {
        //  wait for Characters to be initialized
        yield return new WaitForEndOfFrame ();
        yield return new WaitForEndOfFrame ();

        _sliderHealthBar.maxValue = _attackable.Stats.Life;
        _sliderHealthBar.minValue = 0;
        _sliderHealthBar.value = _attackable.Stats.AttackableState.CurrentLife;
        _maxLifeText.text = _attackable.Stats.Life.ToString ();
        _currentLifeText.text = _attackable.Stats.AttackableState.CurrentLife.ToString ();
        UpdateBarColor ();
        _initialized = true;
    }

    public void Init (Attackable attackable)
    {
        _attackable = attackable ??
            throw new System.ArgumentNullException (nameof (attackable));
        _initialOffsetFromAttackable = transform.position - _attackable.transform.position;
    }

    private void Update ()
    {
        if (!_initialized)
            return;

        if (!_changeSliderValue && _attackable.Stats.AttackableState.CurrentLife != _sliderHealthBar.value)
        {
            _changeSliderValue = true;
            _lastTargetValue = _sliderHealthBar.value;
            _lerp = 0f;
        }

        if (_changeSliderValue)
        {
            _lerp += Time.deltaTime / _lerpTotalTime;
            _sliderHealthBar.value = Mathf.Lerp (_lastTargetValue, _attackable.Stats.AttackableState.CurrentLife, _lerp);
            if (_lerp >= 1f)
            {
                _changeSliderValue = false;
            }

            UpdateBarColor ();
        }

        UpdatePosition ();
    }

    private void UpdatePosition ()
    {
        transform.position = _attackable.transform.position + _initialOffsetFromAttackable;
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

    public void UpdateStateIcons ()
    {
        _stateIconsDisplayer.DisplayState (_attackable.TempEffectsList);
    }
}