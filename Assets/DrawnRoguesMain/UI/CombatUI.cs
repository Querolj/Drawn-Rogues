using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CombatUI : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField]
    private AttackButtonGenerator _attackButtonGen;

    [SerializeField]
    private SpellButtonGenerator _spellButtonGen;

    [SerializeField]
    private Toggle _moveToggle;

    [SerializeField]
    private Toggle _attackToggle;

    [SerializeField]
    private Toggle _artefactToggle;

    [SerializeField]
    private Toggle _drawToggle;

    [SerializeField]
    private Button _endPlayerTurnButton;

    [SerializeField]
    private TurnAnnouncer _turnAnnouncer;

    [SerializeField]
    private Vacuumer _vacuumer;
    #endregion

    #region Private Fields
    private TurnManager _turnManager;
    private PlayerController _playerController;
    private Drawer _drawer;
    private AttackSelectionManager _attackSelectionManager;
    private CursorModeSwitcher _modeSwitcher;
    #endregion

    [Inject, UsedImplicitly]
    private void Init (CursorModeSwitcher modeSwitcher, AttackSelectionManager attackSelectionManager, Drawer drawer)
    {
        _modeSwitcher = modeSwitcher;
        _attackSelectionManager = attackSelectionManager;
        _drawer = drawer;
    }

    private void Awake ()
    {
        _turnManager = FindAnyObjectByType<TurnManager> (); // TODO : inject
        _playerController = FindAnyObjectByType<PlayerController> (); // TODO : inject

        gameObject.SetActive (false);
        ActivateActionsToggleGOs (false);
        // _vacuumer.OnVacuumDone += () =>
        // {
        //     _turnManager.EndTurn (_playerController.ControlledCharacter);
        // };

        _turnManager.OnCombatInitiated += (combatZone) =>
        {
            gameObject.SetActive (combatZone != null);
            ActivateActionsToggleGOs (false);
        };

        _turnManager.OnCombatEnded += (combatZone) =>
        {
            gameObject.SetActive (false);
            ActivateActionsToggleGOs (false);
        };

        _turnManager.OnPlayerTurnStart += () =>
        {
            ActivateActionsToggleGOs (true);
            _turnAnnouncer.SetText (_turnManager.ActivePlayerCharacter, true);
        };

        _turnManager.OnEnemyTurnStart += (character) =>
        {
            _attackSelectionManager.DeactivateCurrentAttackSelection ();
            ActivateActionsToggleGOs (false);
            _turnAnnouncer.SetText (character, false);
        };

        _endPlayerTurnButton.onClick.AddListener (() =>
        {
            OffAllToggles ();
            _playerController.PauseCombatMove ();
            _attackSelectionManager.DeactivateCurrentAttackSelection ();
            _turnManager.EndTurn (_playerController.ControlledCharacter);
        });

        _moveToggle.onValueChanged.AddListener ((value) =>
        {
            if (_moveToggle.isOn)
            {
                _playerController.StartCombatMove ();
            }
            else
                _playerController.PauseCombatMove ();
        });

        _attackToggle.onValueChanged.AddListener ((value) =>
        {
            if (_attackToggle.isOn)
            {
                _spellButtonGen.gameObject.SetActive (false);
                _attackButtonGen.gameObject.SetActive (true);
                _attackButtonGen.GenerateButtons (_turnManager.ActivePlayerCharacter, _turnManager.CurrentCombatZone, () => _turnManager.EndTurn (_playerController.ControlledCharacter));
            }
            else
            {
                _attackSelectionManager.DeactivateCurrentAttackSelection ();
                _attackButtonGen.gameObject.SetActive (false);
            }
        });

        _drawToggle.onValueChanged.AddListener ((value) =>
        {
            if (_drawToggle.isOn)
            {
                _attackButtonGen.gameObject.SetActive (false);
                _spellButtonGen.gameObject.SetActive (true);
                _spellButtonGen.GenerateButtons (_turnManager.ActivePlayerCharacter);
            }
            else
            {
                _spellButtonGen.gameObject.SetActive (false);
            }
        });

        _drawer.OnDrawStrokeEnd += (c, si) =>
        {
            if (_drawToggle.isOn)
            {
                _modeSwitcher.ChangeMode (CursorModeSwitcher.Mode.Selection);
            }
        };

        _artefactToggle.onValueChanged.AddListener ((value) =>
        {
            // OffAllToggles ();
            // if (_artefactToggle.isOn)
            // {
            //     _vacuumer.ActivateVacuumer (_turnManager.CurrentCombatZone);
            // }
            // else
            // {
            //     _vacuumer.DeactivateVacuumer ();
            // }
        });
    }

    private void OffAllToggles ()
    {
        _moveToggle.isOn = false;
        _attackToggle.isOn = false;
        _drawToggle.isOn = false;
        _artefactToggle.isOn = false;
    }

    private void ActivateActionsToggleGOs (bool activate)
    {
        _moveToggle.enabled = activate;
        _attackToggle.enabled = activate;
        _drawToggle.enabled = activate;
        _artefactToggle.enabled = activate;

        if (!activate)
        {
            _moveToggle.isOn = false;
            _attackToggle.isOn = false;
            _drawToggle.isOn = false;
            _artefactToggle.isOn = false;
        }
        else
        {
            _moveToggle.isOn = true;
        }
    }

}