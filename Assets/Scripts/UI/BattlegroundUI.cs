using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BattlegroundUI : MonoBehaviour
{
    [SerializeField]
    private TurnBasedCombat _battleground;

    [SerializeField]
    private PlayerController _playerController;

    [SerializeField]
    private GameObject _showAttackSelectionButton;

    [SerializeField]
    private AttackButtonGenerator _attackButtonGen;

    [SerializeField]
    private SpellButtonGenerator _spellButtonGen;

    [SerializeField]
    private AttackSelectionManager _attackSelectionManager;

    [SerializeField]
    private Toggle _moveToggle;

    [SerializeField]
    private Toggle _attackToggle;

    [SerializeField]
    private Toggle _artefactToggle;

    [SerializeField]
    private GameObject _actionPanel;

    [SerializeField]
    private Toggle _drawToggle;

    [SerializeField]
    private Button _endPlayerTurnButton;

    [SerializeField]
    private TurnAnnouncer _turnAnnouncer;

    [SerializeField]
    private Vacuumer _vacuumer;

    [SerializeField]
    private Drawer _drawer;

    private ModeSwitcher _modeSwitcher;

    [Inject, UsedImplicitly]
    private void Init (ModeSwitcher modeSwitcher)
    {
        _modeSwitcher = modeSwitcher;
    }

    private void Awake ()
    {
        _actionPanel.SetActive (false);
        ActivateActionsToggle (false);
        _vacuumer.OnVacuumDone += () =>
        {
            _battleground.EndTurn (_playerController.ControlledCharacter);
        };

        _battleground.OnCombatInitiated += (combatZone) =>
        {
            _actionPanel.SetActive (combatZone != null);
            _playerController.InitForCombat (_battleground.CurrentCombatZone);
            ActivateActionsToggle (false);
        };

        _battleground.OnCombatEnded += (combatZone) =>
        {
            _actionPanel.SetActive (false);
            ActivateActionsToggle (false);
            _playerController.StopCombatMode ();
        };

        _battleground.OnPlayerTurnStart += () =>
        {
            ActivateActionsToggle (true);
            _turnAnnouncer.SetText (_battleground.ActivePlayerCharacter, true);
        };

        _battleground.OneEnemyTurnStart += (character) =>
        {
            _attackSelectionManager.DeactivateCurrentAttackSelection ();
            ActivateActionsToggle (false);
            _turnAnnouncer.SetText (character, false);
        };

        _endPlayerTurnButton.onClick.AddListener (() =>
        {
            _playerController.PauseCombatMove ();
            _attackSelectionManager.DeactivateCurrentAttackSelection ();
            _battleground.EndTurn (_playerController.ControlledCharacter);
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
                if (_battleground.ActivePlayerCharacter == null)
                    Debug.Log ("Active player character is null");
                _attackButtonGen.GenerateButtons (_battleground.ActivePlayerCharacter, _battleground.CurrentCombatZone, () => _battleground.EndTurn (_playerController.ControlledCharacter));
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
                if (_battleground.ActivePlayerCharacter == null)
                    Debug.Log ("Active player character is null");
                _spellButtonGen.GenerateButtons (_battleground.ActivePlayerCharacter);
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
                _battleground.EndTurn (_playerController.ControlledCharacter);
            }
        };

        _artefactToggle.onValueChanged.AddListener ((value) =>
        {
            if (_artefactToggle.isOn)
            {
                _vacuumer.ActivateVacuumer (_battleground.CurrentCombatZone);
            }
            else
            {
                _vacuumer.DeactivateVacuumer ();
            }
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