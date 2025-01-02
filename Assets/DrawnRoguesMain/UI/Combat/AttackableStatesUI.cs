using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackableStatesUI : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField, BoxGroup ("UI Settings"), Range (0f, 1f)]
    private float _updatePercentagePerSecond = 0.05f;

    [SerializeField, BoxGroup ("UI Settings")]
    private float _zoomedScale = 1.6f;

    [SerializeField, FoldoutGroup ("References")]
    private Image _fillImage;

    [SerializeField, FoldoutGroup ("References")]
    private TMP_Text _maxLifeText;

    [SerializeField, FoldoutGroup ("References")]
    private TMP_Text _currentLifeText;

    [SerializeField, FoldoutGroup ("References")]
    private Slider _sliderHealthBar;

    [SerializeField, FoldoutGroup ("References")]
    private StatusIconsDisplayer _statusIconsDisplayer;
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
            float currentLife = _attackable.Stats.AttackableState.CurrentLife;
            float maxLife = _attackable.Stats.MaxLife;

            float deltaPercentageSigned = _updatePercentagePerSecond * Time.deltaTime * (currentLife >= _sliderHealthBar.value ? 1 : -1);

            float normalizedSliderValue = _sliderHealthBar.value / _sliderHealthBar.maxValue;
            float newNormalizedHealthBarValue = normalizedSliderValue + deltaPercentageSigned;

            if (deltaPercentageSigned > 0)
                _sliderHealthBar.value = Mathf.Clamp (newNormalizedHealthBarValue * maxLife, 0, currentLife);
            else
                _sliderHealthBar.value = Mathf.Clamp (newNormalizedHealthBarValue * maxLife, currentLife, maxLife);

            if (!IsSliderValueUpdating)
                _sliderHealthBar.value = currentLife;

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

        _sliderHealthBar.maxValue = _attackable.Stats.MaxLife;
        _sliderHealthBar.minValue = 0;
        _sliderHealthBar.value = _attackable.Stats.AttackableState.CurrentLife;
        _maxLifeText.text = _attackable.Stats.MaxLife.ToString ();
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