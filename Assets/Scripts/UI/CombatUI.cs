using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CombatUI : MonoBehaviour
{
    private TurnManager _turnManager;
    private PlayerController _playerController;
    private Drawer _drawer;
    private AttackSelectionManager _attackSelectionManager;

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

    private ModeSwitcher _modeSwitcher;

    [Inject, UsedImplicitly]
    private void Init (ModeSwitcher modeSwitcher, AttackSelectionManager attackSelectionManager)
    {
        _modeSwitcher = modeSwitcher;
        _attackSelectionManager = attackSelectionManager;
    }

    private void Awake ()
    {
        _turnManager = FindAnyObjectByType<TurnManager> (); // TODO : inject
        _playerController = FindAnyObjectByType<PlayerController> (); // TODO : inject
        _drawer = FindAnyObjectByType<Drawer> (); // TODO : inject

        gameObject.SetActive (false);
        ActivateActionsToggle (false);
        // _vacuumer.OnVacuumDone += () =>
        // {
        //     _turnManager.EndTurn (_playerController.ControlledCharacter);
        // };

        _turnManager.OnCombatInitiated += (combatZone) =>
        {
            gameObject.SetActive (combatZone != null);
            _playerController.InitForCombat (_turnManager.CurrentCombatZone);
            ActivateActionsToggle (false);
        };

        _turnManager.OnCombatEnded += (combatZone) =>
        {
            gameObject.SetActive (false);
            ActivateActionsToggle (false);
            _playerController.StopCombatMode ();
        };

        _turnManager.OnPlayerTurnStart += () =>
        {
            ActivateActionsToggle (true);
            _turnAnnouncer.SetText (_turnManager.ActivePlayerCharacter, true);
        };

        _turnManager.OneEnemyTurnStart += (character) =>
        {
            _attackSelectionManager.DeactivateCurrentAttackSelection ();
            ActivateActionsToggle (false);
            _turnAnnouncer.SetText (character, false);
        };

        _endPlayerTurnButton.onClick.AddListener (() =>
        {
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
                if (_turnManager.ActivePlayerCharacter == null)
                    Debug.Log ("Active player character is null");
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
                if (_turnManager.ActivePlayerCharacter == null)
                    Debug.Log ("Active player character is null");
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
                _modeSwitcher.ChangeMode (ModeSwitcher.Mode.Selection);
                _turnManager.EndTurn (_playerController.ControlledCharacter);
            }
        };

        _artefactToggle.onValueChanged.AddListener ((value) =>
        {
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

    private void ActivateActionsToggle (bool activate)
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