using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum State
{
    Poisonned,
    Stunned,
    Burn,
    Paralyzed,
    Bleed
}

public class Attackable : CombatEntity
{
    protected const float PIXEL_PER_UNIT = 100f;

    [SerializeField]
    protected string _name;
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    [SerializeField]
    private bool _generateStateUI = true;

    [SerializeField]
    private AttackableStatesUI _stateUITemplate;
    protected AttackableStatesUI _stateUI;

    [SerializeField]
    private int _outlineThickness;

    private GameObject _canvasWorldSpace;

    [SerializeField]
    private Squasher _squasher;
    public Squasher Squasher
    {
        get { return _squasher; }
    }

    [SerializeField]
    protected StatsSerialized _statsSerialized = new StatsSerialized ();

    [SerializeField]
    protected Outline _outline;

    [SerializeField]
    private List<EventMap> _eventsOnDeath = new List<EventMap> ();

    protected SpriteRenderer _renderer;
    public SpriteRenderer Renderer
    {
        get { return _renderer; }
    }

    public Stats Stats = new Stats ();
    public Dictionary<string, Effect> EffectInstancesByName
    {
        get
        {

            Dictionary<string, Effect> effectsInstByNames = new Dictionary<string, Effect> ();
            foreach (Effect effect in Stats.EffectByNames.Values)
            {
                if (!effectsInstByNames.ContainsKey (effect.Name))
                    effectsInstByNames.Add (effect.Name, effect.GetCopy ());
                else
                    effectsInstByNames[effect.Name].AddToInitialValue (effect.InitialValue);
            }

            return effectsInstByNames;
        }
    }

    protected Dictionary<TempEffect.Timeline, List<TempEffect>> _tempEffects = new Dictionary<TempEffect.Timeline, List<TempEffect>> ();
    public Dictionary<TempEffect.Timeline, List<TempEffect>> TempEffects
    {
        get { return _tempEffects; }
    }

    private HashSet<string> _uniqueEffectNames = new HashSet<string> ();
    public HashSet<string> UniqueEffectNames
    {
        get { return _uniqueEffectNames; }
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

    protected int _maxLife;
    public int MaxLife
    {
        get { return _maxLife; }
    }

    protected int _currentLife;
    public int CurrentLife
    {
        get { return _currentLife; }
        set { _currentLife = value; }
    }

    public Sprite Sprite
    {
        get
        {
            return _renderer.sprite;
        }
    }

    private bool _willBeDestroyed = false;
    private bool _destroyed = false;

    public bool WillBeDestroyed
    {
        get { return _willBeDestroyed; }
    }

    private HashSet<State> _states = new HashSet<State> ();

    public bool HasState (State state)
    {
        return _states.Contains (state);
    }

    public void AddState (State state)
    {
        _states.Add (state);
        _stateUI.UpdateStateIcons ();
    }

    public void RemoveState (State state)
    {
        _states.Remove (state);
        _stateUI.UpdateStateIcons ();
    }

    public event Action<Attackable> OnMouseEntered;

    public event Action<Attackable> OnMouseExited;
    public event Action<Attackable> OnDestroyed;

    protected virtual void Awake ()
    {
        _renderer = GetComponent<SpriteRenderer> ();
        _canvasWorldSpace = GameObject.Find ("CanvasWorldSpace");
        if (_statsSerialized.HasAnyStats ())
            Stats.AddStats (_statsSerialized);
    }

    protected virtual void Start ()
    {
        if (_generateStateUI)
            StartCoroutine (CreateStateUI ());

        if (_outline != null)
        {
            _outline.SetRenderer (_renderer);
            _outline.OutlineThickness = _outlineThickness;
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
        _stateUI.transform.SetParent (_canvasWorldSpace.transform);
        _stateUI.transform.localScale = Vector3.one;
    }

    protected virtual void Update ()
    {
        if (_hightLightSprite)
        {
            HightLightSprite ();
        }

        if (!_destroyed && _stateUI?.CurrentValue <= 0.0001f)
        {
            StartCoroutine (DestroyInSeconds (0.5f));
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

    public virtual Bounds GetSpriteBounds ()
    {
        Vector4 border = Utils.GetTextureBorder (_renderer.sprite.texture);
        border /= 100f;

        Vector3 boundsSize = new Vector3 (border.z - border.x, border.w - border.y, 0f);
        Vector3 boundsCenter = transform.position;
        //  Debug.Log (Name + ", pos  " + boundsCenter.ToString ("F4"));
        boundsCenter -= _renderer.sprite.bounds.extents;
        boundsCenter += new Vector3 (border.x, border.y, 0f);
        // Debug.Log (Name + ", pos 2  " + boundsCenter.ToString ("F4"));
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

    public virtual void ReceiveDamage (int damageAmount)
    {
        _currentLife -= damageAmount;
        _currentLife = Mathf.Clamp (_currentLife, 0, MaxLife);
        if (_currentLife == 0)
        {
            _willBeDestroyed = true;
        }
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

    public void AddTempEffect (TempEffect tempEffect)
    {
        if (!_tempEffects.ContainsKey (tempEffect.EffectApplicationTimeline))
            _tempEffects.Add (tempEffect.EffectApplicationTimeline, new List<TempEffect> ());

        _tempEffects[tempEffect.EffectApplicationTimeline].Add (tempEffect);
        _stateUI.UpdateStateIcons ();
    }

    public void RemoveAllTempEffect ()
    {
        _tempEffects.Clear ();
        _stateUI.UpdateStateIcons ();
    }

    private const float SECONDS_BETWEEN_EFFECTS = 0.5f;
    public void ApplyTempEffects (Action onAllEffectApplied, FightDescription fightDescription, TempEffect.Timeline timeline, int index = 0)
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
            _stateUI.UpdateStateIcons ();

            if (nextIndex < _tempEffects[timeline].Count)
            {
                ActionDelayer.Instance.ExecuteInSeconds (SECONDS_BETWEEN_EFFECTS, () =>
                {
                    ApplyTempEffects (onAllEffectApplied, fightDescription, timeline, nextIndex);
                });
            }
            else
                onAllEffectApplied?.Invoke ();

        });
    }

    public void DisplayOutline (Color color)
    {
        _outline.ActivateOutline (color);
    }

    public void StopDisplayOutline ()
    {
        _outline.DeactivateOutline ();
    }

    private const float _HEATLTHBAR_SCALE = 1.6f;
    public void OnMouseEnter ()
    {
        OnMouseEntered?.Invoke (this);
        if (_stateUI != null)
            _stateUI.transform.localScale = Vector3.one * _HEATLTHBAR_SCALE;
    }

    public void OnMouseExit ()
    {
        OnMouseExited?.Invoke (this);
        if (_stateUI != null)
            _stateUI.transform.localScale = Vector3.one;
    }
}