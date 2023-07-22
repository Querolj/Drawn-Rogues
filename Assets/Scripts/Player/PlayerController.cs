using System.Collections.Generic;
using Cinemachine;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour
{
    public enum ControlMode
    {
        None,
        Combat,
        Map,
        FreeDecorDraw
    }

    #region Debug
    [SerializeField]
    private string _playerNameToLoad;

    [SerializeField]
    private GameObject _drawedCharacterTestPrefab;
    #endregion

    [SerializeField]
    private AttackRegistry _attackRegistry;

    [SerializeField]
    private CinemachineVirtualCamera _playerVirtualCamera;

    [SerializeField]
    private CinemachineVirtualCamera _freeDrawVirtualCamera;

    private ControlMode _controlMode = ControlMode.Map;

    [SerializeField]
    private CharacterCanvas _characterCanvas;

    [SerializeField]
    private Transform _startPosition;

    [SerializeField]
    private GameObject _charCreationUI;

    [SerializeField]
    private AttackChoser _attackChoserUI;

    [SerializeField]
    private GameObject _mapUI;

    [SerializeField]
    private FreeDrawUI _freeDrawUI;

    private TurnManager _turnBasedCombat;

    // [SerializeField]
    private DrawedCharacter _controlledCharacter;
    public DrawedCharacter ControlledCharacter
    {
        get { return _controlledCharacter; }
    }

    private const int STARTING_DROP_TOTAL = 300;

    #region combat mode
    private float _leftLimitX;
    private float _rightLimitX;
    private const float ALLOW_CLICK_DISTANCE_Z = 0.3f;
    private CombatZone _currentCombatZone = null;
    #endregion

    #region free draw mode
    private float _lockedZ;
    #endregion

    private List<DrawedCharacter> _ownedCharacters = new List<DrawedCharacter> ();

    private ModeSwitcher _modeSwitcher;
    private MoveIndicator _moveIndicator;
    private BaseColorInventory _baseColorInventory;
    private Attackable.Factory _attackableFactory;

    [Inject, UsedImplicitly]
    private void Init (ModeSwitcher modeSwitcher, MoveIndicator moveIndicator, BaseColorInventory baseColorInventory, Attackable.Factory attackableFactory)
    {
        _modeSwitcher = modeSwitcher;
        _moveIndicator = moveIndicator;
        _baseColorInventory = baseColorInventory;
        _attackableFactory = attackableFactory;
    }

    private void Awake ()
    {
        _turnBasedCombat = FindObjectOfType<TurnManager> (); // TODO : inject

        if (!string.IsNullOrEmpty (_playerNameToLoad)) // for debug
        {
            SetAllColorDrop (100000);
            LoadDrawCharacterTest ();
        }
        else if (_ownedCharacters.Count == 0)
        {
            SetAllColorDrop (STARTING_DROP_TOTAL);
            StartDrawCharacter ();
        }

        _characterCanvas.OnCharacterCreated += OnCharacterCreated;
        _characterCanvas.OnCharacterModified += StopCharacterDraw;
    }

    private void SetAllColorDrop (int total)
    {
        Dictionary<BaseColor, int> colorDropsAvailable = new Dictionary<BaseColor, int> ();

        foreach (BaseColor bc in System.Enum.GetValues (typeof (BaseColor)))
        {
            colorDropsAvailable.Add (bc, total);
        }

        _baseColorInventory.ColorDropsAvailable = colorDropsAvailable;
    }

    private void LoadDrawCharacterTest ()
    {
        DrawedCharacterInfos infos = Loader.LoadDrawedCharacterInfos (_playerNameToLoad);
        DrawedCharacter drawedCharacter = (DrawedCharacter) _attackableFactory.Create (_drawedCharacterTestPrefab);
        GameObject drawedCharacterGo = drawedCharacter.gameObject;

        _controlledCharacter = drawedCharacter;
        _moveIndicator.SetPosition (_startPosition.position);
        _controlledCharacter.Init (infos);

        CharacterPivot pivot = drawedCharacterGo.GetComponentInParent<CharacterPivot> ();
        pivot.InitForMap ();
        pivot.transform.position = _startPosition.position;

        _playerVirtualCamera.Follow = _controlledCharacter.transform;
        _playerVirtualCamera.LookAt = _controlledCharacter.transform;
        _freeDrawVirtualCamera.Follow = _controlledCharacter.transform;
        _freeDrawVirtualCamera.LookAt = _controlledCharacter.transform;
    }

    public void StartDrawCharacter ()
    {
        if (_charCreationUI.gameObject.activeSelf)
            return;

        _characterCanvas.Activate (_controlledCharacter == null ? _startPosition.position : _controlledCharacter.Pivot.transform.position, _controlledCharacter);
        _charCreationUI.SetActive (true);
        _mapUI.SetActive (false);
    }

    private void OnCharacterCreated (GameObject characterGo)
    {
        SetAllColorDrop (0);
        StopCharacterDraw ();

        _controlledCharacter = characterGo.GetComponentInChildren<DrawedCharacter> ();
        characterGo.transform.position = _startPosition.position;
        _playerVirtualCamera.Follow = _controlledCharacter.transform;
        _playerVirtualCamera.LookAt = _controlledCharacter.transform;
        _freeDrawVirtualCamera.Follow = _controlledCharacter.transform;
        _freeDrawVirtualCamera.LookAt = _controlledCharacter.transform;

        ChooseNewAttack (0);
    }

    private void StopCharacterDraw ()
    {
        _charCreationUI.SetActive (false);
        _mapUI.SetActive (true);
        _characterCanvas.Deactivate ();
    }

    private void Update ()
    {
        if (_controlledCharacter == null || _charCreationUI.gameObject.activeSelf)
            return;

        if (Input.GetKeyDown (KeyCode.Space))
        {
            if (_controlMode == ControlMode.Map)
            {
                _controlledCharacter.CharMovement.StopMovement ();
                InitForFreeDraw ();
            }
            else if (_controlMode == ControlMode.FreeDecorDraw)
            {
                StopFreeDrawMode ();
            }
        }

        if (_modeSwitcher.CurrentMode == ModeSwitcher.Mode.Selection)
        {
            if (_controlMode == ControlMode.Map)
                UpdateMoveOnMap ();
            else if (_controlMode == ControlMode.Combat)
                UpdateMoveOnCombat ();
        }
        else if (_modeSwitcher.CurrentMode == ModeSwitcher.Mode.Draw)
        {
            if (_controlMode == ControlMode.FreeDecorDraw)
            {
                UpdateMoveOnFreeDraw ();
            }
        }

    }

    private void UpdateMoveOnMap ()
    {
        if (_characterCanvas.gameObject.activeSelf)
            return;

        if (Input.GetMouseButton (0) && !Utils.IsPointerOverUIElement ())
        {
            if (Utils.TryGetMouseToMapPosition (out Vector3 mouseToMapPosition))
            {
                _controlledCharacter.CharMovement.MoveToTarget (mouseToMapPosition);
                ActivateArrowAtPosition (mouseToMapPosition);
            }
        }
    }

    private void UpdateMoveOnCombat ()
    {
        if (_characterCanvas.gameObject.activeSelf)
            return;

        if (Utils.TryGetMouseToMapPosition (out Vector3 mouseToMapPosition) && _moveIndicator.gameObject.activeSelf && !Utils.IsPointerOverUIElement ())
        {
            if (Mathf.Abs (mouseToMapPosition.z - _moveIndicator.GetPosition ().z) <= ALLOW_CLICK_DISTANCE_Z)
            {
                mouseToMapPosition.z = _moveIndicator.GetPosition ().z;
                mouseToMapPosition.x = Mathf.Clamp (mouseToMapPosition.x, _leftLimitX, _rightLimitX);
                ActivateArrowAtPosition (mouseToMapPosition);

                if (Input.GetMouseButtonDown (0))
                {
                    _controlledCharacter.CharMovement.MoveToTarget (mouseToMapPosition, TryEscape);
                }
            }
        }
    }

    private void UpdateMoveOnFreeDraw ()
    {
        if (Utils.TryGetMouseToMapPosition (out Vector3 mouseToMapPosition) && _moveIndicator.gameObject.activeSelf && !Utils.IsPointerOverUIElement ())
        {
            if (Mathf.Abs (mouseToMapPosition.z - _lockedZ) <= ALLOW_CLICK_DISTANCE_Z)
            {
                mouseToMapPosition.z = _lockedZ;
                ActivateArrowAtPosition (mouseToMapPosition);

                if (Input.GetMouseButtonDown (0))
                {
                    _controlledCharacter.CharMovement.MoveToTarget (mouseToMapPosition);
                }
            }
        }
    }

    private void TryEscape ()
    {
        if (_currentCombatZone.IsInEscapeZone (_controlledCharacter.transform.position))
        {
            float chance = _currentCombatZone.GetChanceOfEscape ();
            if (Random.Range (0f, 1f) <= chance)
            {
                Debug.Log ("Escape success");
                StopCombatMode ();
                _turnBasedCombat.EscapeFight ();
            }
            else
            {
                Debug.Log ("Escape failed");
                _turnBasedCombat.EndTurn (_controlledCharacter);
            }
        }
    }

    private void ActivateArrowAtPosition (Vector3 position)
    {
        _moveIndicator.gameObject.SetActive (true);
        _moveIndicator.SetPosition (position);
    }

    public void InitForCombat (CombatZone combatZone)
    {
        if (_controlledCharacter == null)
            throw new System.Exception (nameof (_controlledCharacter) + " not set, can't fight. (it should be set)");

        _currentCombatZone = combatZone ??
            throw new System.Exception (nameof (combatZone) + " not set, can't fight. (it should be set)");

        _controlMode = ControlMode.Combat;
        _moveIndicator.ActiveCombatMode (_controlledCharacter.GetSpriteBounds ());
    }

    public void InitForFreeDraw ()
    {
        if (_controlledCharacter == null)
            throw new System.Exception (nameof (_controlledCharacter) + " not set, can't fight. (it should be set)");

        _freeDrawVirtualCamera.Priority = 20;
        _modeSwitcher.ChangeMode (ModeSwitcher.Mode.Draw);
        _lockedZ = _controlledCharacter.transform.position.z;
        _controlMode = ControlMode.FreeDecorDraw;
        _freeDrawUI.gameObject.SetActive (true);
        _freeDrawUI.Init (_controlledCharacter);
        _moveIndicator.ActiveCombatMode (_controlledCharacter.GetSpriteBounds ());
    }

    private void StopFreeDrawMode ()
    {
        _freeDrawUI.gameObject.SetActive (false);
        _controlledCharacter.CharMovement.StopMovement ();
        _controlMode = ControlMode.Map;
        _freeDrawVirtualCamera.Priority = 0;
        _modeSwitcher.ChangeMode (ModeSwitcher.Mode.Selection);
        _moveIndicator.DeactivateCombatMode ();
    }

    private bool _combatMoveStarted = false;
    public void StartCombatMove ()
    {
        if (_combatMoveStarted)
            return;

        (_leftLimitX, _rightLimitX) = _currentCombatZone.GetMoveZoneLimitOnMap (_controlledCharacter.MaxDistanceToMove, _controlledCharacter);
        _moveIndicator.gameObject.SetActive (true);
        _combatMoveStarted = true;
        Vector3 posMoveIndicator = _controlledCharacter.GetSpriteBounds ().center;
        posMoveIndicator.z = _currentCombatZone.transform.position.z;
        _moveIndicator.SetPosition (posMoveIndicator);
        _currentCombatZone.DrawMoveLineOnMap (_leftLimitX, _rightLimitX, (Bounds) _controlledCharacter.GetSpriteBounds ());
    }

    public void PauseCombatMove ()
    {
        _moveIndicator.gameObject.SetActive (false);
        _currentCombatZone.StopDrawMoveZoneLineOnMap ();
        _combatMoveStarted = false;
    }

    public void StopCombatMode ()
    {
        _moveIndicator.gameObject.SetActive (false);
        _currentCombatZone.StopDrawMoveZoneLineOnMap ();
        _controlMode = ControlMode.Map;
        _moveIndicator.DeactivateCombatMode ();
    }

    public void AddXp (int xp)
    {
        int charOldLevel = _controlledCharacter.Level;
        _controlledCharacter.GainXp (xp);

        int levelDiff = _controlledCharacter.Level - charOldLevel;
        if (levelDiff > 0)
            ChooseNewAttack (levelDiff - 1);
    }

    private void ChooseNewAttack (int additionalAttackToChoose)
    {
        _controlMode = ControlMode.None;
        if (!_attackRegistry.TryGetAttacksToChooseFrom (_controlledCharacter, out List<Attack> attacks))
        {
            Debug.LogWarning ("No attack found for " + _controlledCharacter.Name);
            return;
        }

        _attackChoserUI.DisplayAttackToChose (attacks, (attack) =>
        {
            _controlledCharacter.Attacks.Add (attack);
            additionalAttackToChoose--;
            if (additionalAttackToChoose > 0)
                ChooseNewAttack (additionalAttackToChoose - 1);
            else
                _controlMode = ControlMode.Map;
        });
    }

    public void SetMoveMode (ControlMode moveMode)
    {
        _controlMode = moveMode;
    }

    public void RemoveAllTempEffectOnChar ()
    {
        _controlledCharacter.RemoveAllTempEffect ();
    }
}