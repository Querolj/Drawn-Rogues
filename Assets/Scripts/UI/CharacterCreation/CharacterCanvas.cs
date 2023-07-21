using System;
using System.Collections.Generic;
using Cinemachine;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CharacterCanvas : MonoBehaviour
{
    [SerializeField]
    private Frame _frame;

    [SerializeField]
    private Transform _modifierLayer;

    [SerializeField]
    private ModifierGoInstanceFactory _modifierGoInstanceFactory;

    [SerializeField]
    private TMP_InputField _nameField;

    [SerializeField]
    private GameObject _drawedCharacterTemplate;

    [SerializeField]
    private Button _validateButton;

    [SerializeField]
    private CinemachineVirtualCamera _camera;

    private FrameReader _frameReader;

    private Stats _stats;
    public Stats Stats
    {
        get { return _stats; }
    }
    private DrawedCharacterFormDescription _drawedCharacterFormDescription;
    public DrawedCharacterFormDescription DrawedCharacterFormDescription
    {
        get { return _drawedCharacterFormDescription; }
    }

    private Drawer _drawer;
    private List<ModifierInfos> _modifiersAdded = new List<ModifierInfos> ();
    public event Action OnStatsChanged;

    private Vector2 _viewportBottomLeftFramePos;
    public Vector2 ViewportBottomLeftFramePos
    {
        get
        {
            UpdateViewportCornersPos ();
            return _viewportBottomLeftFramePos;
        }
    }
    private Vector2 _viewportTopRightFramePos;
    public Vector2 ViewportTopRightFramePos
    {
        get
        {
            UpdateViewportCornersPos ();
            return _viewportTopRightFramePos;
        }
    }

    private MeshRenderer _frameMeshRenderer;
    private Camera _mainCamera;
    private MeshRenderer _modifierLayerRenderer;
    private DrawedCharacter _drawedCharacterToModify;
    public event Action<GameObject> OnCharacterCreated;
    public event Action OnCharacterModified;
    private int _maxModifieurAllowed = 3;
    public int MaxModifieurAllowed
    {
        get { return _maxModifieurAllowed; }
    }

    public int ModifiersCount
    {
        get { return _modifiersAdded.Count; }
    }

    public bool CanHaveMoreModifier ()
    {
        return _modifiersAdded.Count < _maxModifieurAllowed;
    }

    public event Action OnCharFormUpdated;

    private const int BASE_PIXELS_ALLOWED = 300;

    private ModeSwitcher _modeSwitcher;

    private Attackable.Factory _attackableFactory;

    [Inject, UsedImplicitly]
    private void Init (ModeSwitcher modeSwitcher, Attackable.Factory attackableFactory)
    {
        _modeSwitcher = modeSwitcher;
        _attackableFactory = attackableFactory;
    }

    private void Awake ()
    {
        _mainCamera = Camera.main;
        _drawer = GameObject.FindFirstObjectByType<Drawer> (); // TODO : Inject
        _frameReader = GameObject.FindFirstObjectByType<FrameReader> (); // TODO : Inject
        _stats = new Stats ();
        _modifierLayerRenderer = _modifierLayer.GetComponent<MeshRenderer> ();
        _validateButton.gameObject.SetActive (false);

        ComputeShader getCharProportionCs = Resources.Load<ComputeShader> ("GetCharacterProportion");
        if (getCharProportionCs == null)
        {
            Debug.LogError (nameof (getCharProportionCs) + "null, it was not loaded (not found?)");
            return;
        }

        _drawedCharacterFormDescription = new DrawedCharacterFormDescription ();
        _drawedCharacterFormDescription.OnUpdated += () =>
        {
            OnCharFormUpdated?.Invoke ();
        };

        _frameMeshRenderer = _frame.GetComponent<MeshRenderer> ();
        if (_frameMeshRenderer == null)
            throw new Exception ("No mesh renderer found on " + _frame.name);
        UpdateViewportCornersPos ();
    }

    private void UpdateViewportCornersPos ()
    {
        _viewportTopRightFramePos = _mainCamera.WorldToViewportPoint (_frameMeshRenderer.bounds.max);
        _viewportBottomLeftFramePos = _mainCamera.WorldToViewportPoint (_frameMeshRenderer.bounds.min);
    }

    private void Start ()
    {
        _frame.SetOnPixelsAdded ((c, pc) => UpdateStats ());

        _validateButton.onClick.AddListener (() =>
        {
            _validateButton.gameObject.SetActive (false);
            _drawedCharacterFormDescription.CalculateCharProportion (_frame.DrawTexture, _frame.PixelUsages);
            if (_drawedCharacterToModify == null)
                CreateDrawedCharacter ();
            else
                ModifyDrawedCharacter ();
        });

        _drawer.OnDrawStrokeEnd += (lc, si) =>
        {
            _drawedCharacterFormDescription.AddStrokeInfo (si);
            _drawedCharacterFormDescription.CalculateCharProportion (_frame.DrawTexture, _frame.PixelUsages);
            GraphicUtils.SavePixelsAsPNG (_frame.DrawTexture.GetPixels (), "Test/player.png", _frame.DrawTexture.width, _frame.DrawTexture.height);

            if (_drawedCharacterFormDescription.HasAnyForm ())
            {
                _validateButton.gameObject.SetActive (true);
            }
        };

        _drawer.OnUndo += () =>
        {
            UpdateStats ();
        };
    }

    public void Activate (Vector3 position, DrawedCharacter dc = null)
    {
        _modeSwitcher.ChangeMode (ModeSwitcher.Mode.Draw);
        transform.position = position;
        gameObject.SetActive (true);
        _drawedCharacterToModify = dc;
        _modifiersAdded.Clear ();
        _camera.Priority = 20;
        RemoveAllModifiersGo ();

        if (dc != null)
        {
            _validateButton.gameObject.SetActive (true);
            _drawedCharacterFormDescription = new DrawedCharacterFormDescription (dc.DrawedCharacterFormDescription);
            _drawedCharacterFormDescription.OnUpdated += () =>
            {
                OnCharFormUpdated?.Invoke ();
            };

            _maxModifieurAllowed = dc.GetMaxModifierAllowed ();
            foreach (ModifierInfos modifierInfos in dc.ModifiersAdded)
            {
                Modifier modifier = Resources.Load<Modifier> ("Modifier/" + modifierInfos.SOFileName);
                if (modifier == null)
                    throw new Exception ("Modifier " + modifierInfos.SOFileName + " not found");
                AddModifier (modifier, modifierInfos);
                _stats.AddStats (modifier.Stats);
            }
            UpdateStats ();
        }
        else
        {
            _validateButton.gameObject.SetActive (false);
            _frame.ResetPixelAllowed (BASE_PIXELS_ALLOWED);
        }
    }

    public void Deactivate ()
    {
        _modeSwitcher.ChangeMode (ModeSwitcher.Mode.Selection);
        gameObject.SetActive (false);
        _camera.Priority = 0;
    }

    private void UpdateStats (bool resetCurrentLife = true)
    {
        _stats = new Stats (_frameReader.GetPixelIdsAndUsagesCount (_frame));

        foreach (ModifierInfos modifierInfos in _modifiersAdded)
        {
            Modifier modifier = Resources.Load<Modifier> ("Modifier/" + modifierInfos.SOFileName);
            if (modifier == null)
                throw new Exception ("Modifier " + modifierInfos.SOFileName + " not found");
            _stats.AddStats (modifier.Stats);
        }

        OnStatsChanged?.Invoke ();
        // Debug.Log (_stats.ToString ());
    }

    public void AddModifierFromViewport (Modifier selectedModifier, Vector2 viewportMousePosition, bool isFlipped)
    {
        if (!Utils.TryGetViewportToLayerPosition (viewportMousePosition, out Vector3 hitPos, out GameObject goHit, 1 << LayerMask.NameToLayer ("Frame3D")))
        {
            throw new Exception ("No hit on frame");
        }

        Vector3 localHitPos = _modifierLayer.InverseTransformPoint (hitPos);
        GameObject modifierGoInstance = _modifierGoInstanceFactory.Create (_modifierLayerRenderer.bounds, _modifierLayer, selectedModifier, localHitPos, isFlipped, 0f,
            (mod) =>
            {
                _modifiersAdded.Remove (_modifiersAdded.Find (m => m.Modifier == mod));
            });
        modifierGoInstance.transform.localRotation = Quaternion.Euler (90, 0, 0);

        Vector3 layerWorldOrigin = _modifierLayerRenderer.bounds.center - _modifierLayerRenderer.bounds.extents;
        Vector2 percentagePos = new Vector2 ((hitPos.x - layerWorldOrigin.x) / _modifierLayerRenderer.bounds.size.x, (hitPos.y - layerWorldOrigin.y) / _modifierLayerRenderer.bounds.size.y);

        ModifierInfos modifierInfos = new ModifierInfos (percentagePos, selectedModifier, isFlipped);
        _modifiersAdded.Add (modifierInfos);

        UpdateStats ();
    }

    public void AddModifier (Modifier selectedModifier, ModifierInfos modifierInfos)
    {
        _modifiersAdded.Add (modifierInfos);

        Vector3 bottomLeftWorld = _modifierLayerRenderer.bounds.center - _modifierLayerRenderer.bounds.extents;
        Vector3 topRightWorld = _modifierLayerRenderer.bounds.center + _modifierLayerRenderer.bounds.extents;
        Vector3 bottomLeftLocal = _modifierLayer.InverseTransformPoint (bottomLeftWorld);
        Vector3 topRightLocal = _modifierLayer.InverseTransformPoint (topRightWorld);
        Vector3 localPos;
        localPos.x = Mathf.Lerp (bottomLeftLocal.x, topRightLocal.x, modifierInfos.PercentagePosition.x);
        localPos.y = 0f;
        localPos.z = Mathf.Lerp (bottomLeftLocal.z, topRightLocal.z, modifierInfos.PercentagePosition.y);

        GameObject modifierGoInstance = _modifierGoInstanceFactory.Create (_modifierLayerRenderer.bounds, _modifierLayer, selectedModifier,
            localPos, modifierInfos.IsFlipped, 1.5f,
            (mod) =>
            {
                _modifiersAdded.Remove (_modifiersAdded.Find (m => m.Modifier == mod));
            });
        modifierGoInstance.transform.localRotation = Quaternion.Euler (90, 0, 0);

        UpdateStats ();
    }

    private bool ValidateDrawing ()
    {
        if (_nameField.text == null || _nameField.text.Length == 0)
        {
            Debug.LogError ("_nameText is null");
            return false;
        }
        if (!_drawedCharacterFormDescription.HasAnyForm ())
        {
            Debug.LogError ("_drawedCharacterFormDescription has no form");
            return false;
        }

        return true;
    }

    private void Update ()
    {
        if (Input.GetKey (KeyCode.LeftControl) && Input.GetKeyDown (KeyCode.S))
        {
            SaveCharacter ();
        }
    }

    private void SaveCharacter ()
    {
        if (!ValidateDrawing ())
        {
            Debug.LogError ("Can't save character, drawing is not valid");
            return;
        }

        Debug.Log ("Save character " + _nameField.text);
        DrawedCharacterInfos infos = new DrawedCharacterInfos (_frame.GetTextureInfos (), _nameField.text, _modifiersAdded, _drawedCharacterFormDescription);
        Saver.SaveDrawedCharacterInfos (infos);
    }

    private void CreateDrawedCharacter ()
    {
        DrawedCharacter drawedCharacter = (DrawedCharacter) _attackableFactory.Create (_drawedCharacterTemplate);
        GameObject drawedCharacterGo = drawedCharacter.gameObject;
        drawedCharacter.Init (_drawedCharacterFormDescription, _frame, _modifiersAdded);
        drawedCharacter.Name = _nameField.text;

        CharacterPivot pivot = drawedCharacterGo.GetComponentInParent<CharacterPivot> ();
        pivot.InitForMap ();

        OnCharacterCreated?.Invoke (drawedCharacterGo);
        _modeSwitcher.ChangeMode (ModeSwitcher.Mode.Selection);
    }

    private void ModifyDrawedCharacter ()
    {
        _drawedCharacterToModify.Init (_drawedCharacterFormDescription, _frame, _modifiersAdded, false);
        _modeSwitcher.ChangeMode (ModeSwitcher.Mode.Selection);
        OnCharacterModified?.Invoke ();
    }

    private void RemoveAllModifiersGo ()
    {
        for (int i = 0; i < _modifierLayer.transform.childCount; i++)
        {
            Destroy (_modifierLayer.transform.GetChild (i).gameObject);
        }
    }
}