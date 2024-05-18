using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

[RequireComponent (typeof (AttackableDescription))]
public class Attackable : CombatEntity
{
    #region Serialized Fields
    [SerializeField]
    private bool _generateStateUI = true;

    [SerializeField]
    private AttackableStatesUI _stateUITemplate;
    protected AttackableStatesUI _stateUI;

    [SerializeField]
    private Squasher _squasher;
    public Squasher Squasher
    {
        get { return _squasher; }
    }

    [SerializeField]
    protected StatsSerialized _statsSerialized = new StatsSerialized ();

    [SerializeField]
    protected List<EffectSerialized> _effectsSerialized = new List<EffectSerialized> ();

    [SerializeField]
    protected Outline _outline;

    [SerializeField]
    private List<EventMap> _eventsOnDeath = new List<EventMap> ();
    #endregion Serialized Fields

    protected const float PIXEL_PER_UNIT = 100f;

    protected SpriteRenderer _renderer;
    public AttackableStats Stats = new AttackableStats ();
    public SpriteRenderer Renderer
    {
        get { return _renderer; }
    }

    protected Dictionary<TempEffect.Timeline, List<TempEffect>> _tempEffects = new Dictionary<TempEffect.Timeline, List<TempEffect>> ();
    public Dictionary<TempEffect.Timeline, List<TempEffect>> TempEffects
    {
        get { return _tempEffects; }
    }

    public List<TempEffect> TempEffectsList
    {
        get { return _tempEffects.Values.SelectMany (x => x).ToList (); }
    }

    private bool _hightLightSprite = false;
    private Color _hightLightColorLow = new Color (1f, 0.5f, 0.5f, 1f);
    private Color _hightLightColorHigh = Color.white;
    private float _hightLightSpeed = 4f;
    private int _hightLightDir = 1;
    private float _currentLerpColorHightLight = 0f;
    protected Bounds _bounds;
    protected AttackableDescription _description;
    public AttackableDescription Description
    {
        get { return _description; }
    }

    public Sprite Sprite
    {
        get
        {
            return _renderer.sprite;
        }
    }

    protected bool _willBeDestroyed = false;
    private bool _destroyed = false;
    private const float DEATH_DELAY = 0.5f;
    public bool WillBeDestroyed
    {
        get { return _willBeDestroyed; }
    }

    public event Action<Attackable> OnMouseEntered;
    public event Action<Attackable> OnMouseExited;
    public event Action<Attackable> OnDestroyed;

    #region Injection
    private ActionDelayer _actionDelayer;
    private WorldUIContainer _worldUIContainer;

    [Inject, UsedImplicitly]
    private void Init (ActionDelayer actionDelayer, WorldUIContainer worldUIContainer)
    {
        _actionDelayer = actionDelayer;
        _worldUIContainer = worldUIContainer;
    }
    #endregion Injection

    protected virtual void Awake ()
    {
        _description = GetComponent<AttackableDescription> ();
        _renderer = GetComponent<SpriteRenderer> ();
        if (_statsSerialized.HasAnyStats ())
        {
            Stats = new AttackableStats (_statsSerialized);
        }

        Stats.AttackableState.OnLifeReachZero += () =>
        {
            _willBeDestroyed = true;
            StartCoroutine (DestroyInSeconds (DEATH_DELAY));
        };
    }

    protected virtual void Start ()
    {
        if (_generateStateUI)
            StartCoroutine (CreateStateUI ());

        if (_outline != null)
        {
            _outline.SetRenderer (_renderer);
        }

        if (_renderer.sprite != null)
        {
            _bounds = GetSpriteBounds ();
        }
    }

    protected IEnumerator CreateStateUI ()
    {
        // Wait for initialization of some Attackable
        yield return new WaitForEndOfFrame ();
        yield return new WaitForEndOfFrame ();

        Bounds spriteBounds = GetSpriteBounds ();
        Vector3 stateUIPos = spriteBounds.center;
        stateUIPos.z = transform.position.z - 0.001f; // - 0.001f to avoid z-fighting 
        float margin = 0.1f;
        stateUIPos.y += spriteBounds.extents.y + margin;
        _stateUI = Instantiate (_stateUITemplate, stateUIPos, Quaternion.identity);
        _stateUI.Init (this);
        _worldUIContainer.AddUI (_stateUI.transform);
        _stateUI.transform.SetParent (_worldUIContainer.transform);
        _stateUI.transform.localScale = Vector3.one;
    }

    protected virtual void Update ()
    {
        if (_destroyed)
            return;

        if (_hightLightSprite)
        {
            HightLightSprite ();
        }
    }

    private System.Collections.IEnumerator DestroyInSeconds (float s)
    {
        if (_destroyed)
            yield return null;

        yield return new WaitForSeconds (s);
        _destroyed = true;
        OnDestroyed?.Invoke (this);
        foreach (EventMap e in _eventsOnDeath)
        {
            e.Trigger ();
        }
        DestroyImmediate (gameObject);
        DestroyImmediate (_stateUI?.gameObject);
    }

    private void HightLightSprite ()
    {
        _renderer.color = Color.Lerp (_hightLightColorLow, _hightLightColorHigh, _currentLerpColorHightLight);

        _currentLerpColorHightLight = Mathf.Clamp (_currentLerpColorHightLight + Time.deltaTime * _hightLightSpeed * _hightLightDir, -0.0001f, 1.0001f);
        if (_currentLerpColorHightLight >= 1f)
        {
            _hightLightDir = -1;
            _currentLerpColorHightLight = 1f;
        }
        else if (_currentLerpColorHightLight <= 0f)
        {
            _hightLightDir = 1;
            _currentLerpColorHightLight = 0f;
        }
    }

    public void StartHightLightSprite ()
    {
        _hightLightSprite = true;
        _currentLerpColorHightLight = 0f;
        _hightLightDir = 1;
    }

    public void StopHightLightSprite ()
    {
        _hightLightSprite = false;
        _renderer.color = Color.white;
    }

    public bool IsLifeUpdating ()
    {
        return _stateUI.IsSliderValueUpdating;
    }

    public virtual Bounds GetSpriteBounds ()
    {
        Vector4 border = GraphicUtils.GetTextureBorder (_renderer.sprite.texture);
        border /= 100f;

        Vector3 boundsSize = new Vector3 (border.z - border.x, border.w - border.y, 0f);
        Vector3 boundsCenter = transform.position;
        boundsCenter -= _renderer.sprite.bounds.extents;
        boundsCenter += new Vector3 (border.x, border.y, 0f);
        boundsCenter += boundsSize / 2f;
        _bounds = new Bounds (boundsCenter, boundsSize);
        // _bounds = RotateBounds (new Bounds (boundsCenter, boundsSize), transform.eulerAngles);
        return _bounds;
    }

    public float GetXOffSetFromRenderer ()
    {
        Bounds bounds = GetSpriteBounds ();
        return _renderer.bounds.center.x - bounds.center.x;
    }

    protected Bounds RotateBounds (Bounds bounds, Vector3 eulerAngle)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
        Quaternion rotation = Quaternion.Euler (eulerAngle);
        Vector3 min = center - extents;
        Vector3 max = center + extents;
        Vector3 newMin = center;
        Vector3 newMax = center;
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3 (min.x, min.y, min.z);
        corners[1] = new Vector3 (min.x, min.y, max.z);
        corners[2] = new Vector3 (min.x, max.y, min.z);
        corners[3] = new Vector3 (min.x, max.y, max.z);
        corners[4] = new Vector3 (max.x, min.y, min.z);
        corners[5] = new Vector3 (max.x, min.y, max.z);
        corners[6] = new Vector3 (max.x, max.y, min.z);
        corners[7] = new Vector3 (max.x, max.y, max.z);
        for (int i = 0; i < 8; i++)
        {
            corners[i] = rotation * (corners[i] - center) + center;
            newMin = Vector3.Min (newMin, corners[i]);
            newMax = Vector3.Max (newMax, corners[i]);
        }
        return new Bounds ((newMin + newMax) / 2f, newMax - newMin);

    }

    public void FadeSprite ()
    {
        StartCoroutine (FadeSpriteCoroutine ());
    }

    private IEnumerator FadeSpriteCoroutine ()
    {
        const float fadedTime = 0.06f;
        const int fadeIteration = 3;
        Color color = _renderer.color;
        Color fadedColor = color;
        fadedColor.a = 0.3f;

        for (int i = 0; i < fadeIteration; i++)
        {
            _renderer.color = fadedColor;
            yield return new WaitForSeconds (fadedTime);
            _renderer.color = color;
            if (i != fadeIteration - 1)
                yield return new WaitForSeconds (fadedTime);
        }

    }

    public void DisplayOutline (Color color)
    {
        _outline.ActivateOutline (color);
    }

    public void StopDisplayOutline ()
    {
        _outline.DeactivateOutline ();
    }

    public void OnMouseEnter ()
    {
        OnMouseEntered?.Invoke (this);
        if (_stateUI != null)
            _stateUI.Zoom ();
    }

    public void OnMouseExit ()
    {
        OnMouseExited?.Invoke (this);
        if (_stateUI != null)
            _stateUI.Unzoom ();
    }

    #region Effects
    public void AddTempEffect (TempEffect tempEffect)
    {
        if (!_tempEffects.ContainsKey (tempEffect.EffectApplicationTimeline))
            _tempEffects.Add (tempEffect.EffectApplicationTimeline, new List<TempEffect> ());

        _tempEffects[tempEffect.EffectApplicationTimeline].Add (tempEffect);
        _stateUI.UpdateStatusIcons ();
    }

    public void RemoveAllTempEffect ()
    {
        _tempEffects.Clear ();
        _stateUI.UpdateStatusIcons ();
    }

    public bool HasTempEffect (TempEffect tmpEffect)
    {
        foreach (List<TempEffect> tempEffects in TempEffects.Values)
        {
            foreach (TempEffect effect in tempEffects)
            {
                if (effect.DisplayName == tmpEffect.DisplayName)
                    return true;
            }
        }

        return false;
    }

    private const float SECONDS_BETWEEN_EFFECTS = 0.5f;
    public void ApplyTempEffects (Action onAllEffectApplied, FightRegistry fightDescription, TempEffect.Timeline timeline, int index = 0)
    {
        if (!_tempEffects.ContainsKey (timeline) || _tempEffects[timeline].Count == 0 || index >= _tempEffects[timeline].Count)
        {
            onAllEffectApplied?.Invoke ();
            return;
        }

        _tempEffects[timeline][index].Apply (this, fightDescription, () =>
        {
            int nextIndex = index + 1;

            if (_tempEffects[timeline][index].EffectWoreOff)
            {
                _tempEffects[timeline].RemoveAt (index);
                nextIndex = index;
            }
            _stateUI.UpdateStatusIcons ();

            if (nextIndex < _tempEffects[timeline].Count)
            {
                _actionDelayer.ExecuteInSeconds (SECONDS_BETWEEN_EFFECTS, () =>
                {
                    ApplyTempEffects (onAllEffectApplied, fightDescription, timeline, nextIndex);
                });
            }
            else
                onAllEffectApplied?.Invoke ();

        });
    }

    #endregion Effects
}