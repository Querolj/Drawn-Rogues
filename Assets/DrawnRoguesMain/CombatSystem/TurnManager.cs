using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class TurnManager : MonoBehaviour
{
    private Drawer _drawer;
    private PlayerController _playerController;

    public DrawedCharacter ActivePlayerCharacter { get { return _playerController.ControlledCharacter; } }

    [SerializeField]
    private EnemyTurnIndicator _enemyTurnIndicator;

    private CombatZone _currentCombatZone;
    public event Action<CombatZone> OnCombatInitiated;
    public event Action<CombatZone> OnCombatEnded;

    public Action OnPlayerTurnStart;
    public Action<Character> OnEnemyTurnStart;

    public CombatZone CurrentCombatZone
    {
        get { return _currentCombatZone; }
    }

    private int _roundCount = 0;

    // Attacks ordered by priority
    private Stack<CombatEntity> _combatEntitiesLeftToPlay;

    private ActionDelayer _actionDelayer;
    private FightRegistry _fightRegistry;
    private bool _inCombat = false;
    public bool InCombat { get { return _inCombat; } }

    private const float SECONDS_BETWEEN_APPLY_EFFECTS = 0.5f;

    [Inject, UsedImplicitly]
    private void Init (ActionDelayer actionDelayer, FightRegistry fightRegistry, Drawer drawer)
    {
        _actionDelayer = actionDelayer;
        _fightRegistry = fightRegistry;
        _drawer = drawer;
    }

    private void Awake ()
    {
        _playerController = FindAnyObjectByType<PlayerController> (); // TODO : inject
    }

    private IEnumerator Start ()
    {
        // wait for scene initialization before starting the fight
        yield return new WaitForEndOfFrame ();
        yield return new WaitForEndOfFrame ();
    }

    public void InitCombat (CombatZone combatZone)
    {
        _currentCombatZone = combatZone ??
            throw new System.ArgumentNullException (nameof (combatZone));

        if (combatZone.EnemiesInZone.Count == 0)
            throw new System.ArgumentException ("No enemies in combat zone", nameof (combatZone));

        _playerController.InitForCombat (_currentCombatZone);
        OnCombatInitiated?.Invoke (combatZone);
        _inCombat = true;
        StartCoroutine (TryStartNewRound ());
    }

    private void SetCharactersLeftToPlay ()
    {
        List<CombatEntity> sortedCombatEntitiesFromCombatZone = new List<CombatEntity> (_currentCombatZone.EnemiesInZone);
        sortedCombatEntitiesFromCombatZone.Add (_playerController.ControlledCharacter);
        sortedCombatEntitiesFromCombatZone.Sort ((a, b) => a.GetTurnOrder ().CompareTo (b.GetTurnOrder ()));

        _combatEntitiesLeftToPlay = new Stack<CombatEntity> ();
        foreach (CombatEntity combatEntity in sortedCombatEntitiesFromCombatZone)
        {
            _combatEntitiesLeftToPlay.Push (combatEntity);
            foreach (CombatEntity combatEntityLinked in combatEntity.LinkedCombatEntities)
            {
                _combatEntitiesLeftToPlay.Push (combatEntityLinked);
            }
        }
    }

    private IEnumerator WaitUntilLifeUpdated ()
    {
        List<CombatEntity> combatEntities = new List<CombatEntity> (_currentCombatZone.EnemiesInZone);
        combatEntities.Add (_playerController.ControlledCharacter);

        foreach (Character character in combatEntities)
        {
            while (character.IsLifeUpdating ())
            {
                yield return null;
            }
        }
    }

    private IEnumerator StartCharacterTurn (Character character)
    {
        if (character == null)
            throw new ArgumentNullException (nameof (character));
        Debug.Log ("Start turn : " + character.Description.DisplayName);

        yield return WaitUntilLifeUpdated ();

        if (!character.CanPlayTurn ())
        {
            _enemyTurnIndicator.Hide ();
            _fightRegistry.Report (character.Description.DisplayName + " can't play this turn");
            EndTurn (character);
            yield return null;
        }
        else
        {
            character.CallOnTurnStarted ();

            if (character == _playerController.ControlledCharacter)
            {
                _enemyTurnIndicator.Hide ();
                Debug.Log ("Player turn");
                OnPlayerTurnStart?.Invoke ();
            }
            else if (!character.WillBeDestroyed)
            {
                _enemyTurnIndicator.SetOnCharacter (character);

                OnEnemyTurnStart?.Invoke (character);
                yield return new WaitForSeconds (0.5f);
                if (character.HasAI ())
                    character.GetAI ().ExecuteTurn (_currentCombatZone, _playerController.ControlledCharacter, _fightRegistry, () => EndTurn (character));
            }
            else
            {
                _enemyTurnIndicator.Hide ();
                NextTurn ();
            }
        }
    }

    public void EndTurn (Character lastTurnCharacter)
    {
        Debug.Log ("End turn");
        ApplyAllTempEffects (new Stack<Attackable> (new List<Attackable> { lastTurnCharacter }), TempEffect.Timeline.EndTurn, () =>
        {
            NextTurn ();
        });
    }

    private void NextTurn ()
    {
        Debug.Log ("Next turn");
        if (HasCombatEnded ())
        {
            EndCombat ();
            return;
        }

        if (_combatEntitiesLeftToPlay.Count > 0)
        {
            CombatEntity combatEntity = _combatEntitiesLeftToPlay.Pop ();
            if (combatEntity is Character character)
            {
                if (character == null)
                {
                    NextTurn ();
                    return;
                }

                ApplyAllTempEffects (new Stack<Attackable> (new List<Attackable> { character }), TempEffect.Timeline.StartTurn, () =>
                {
                    StartCoroutine (StartCharacterTurn (character));
                });
            }
            else if (combatEntity is CombatEnvironnementHazard envHazard)
            {
                envHazard.ExecuteTurn (NextTurn);
            }
        }
        else
        {
            // end of the round : apply temp effects 
            List<Attackable> allAttackableInZone = new List<Attackable> (_currentCombatZone.AttackablesInZone);
            allAttackableInZone.Add (_playerController.ControlledCharacter);

            ApplyAllTempEffects (new Stack<Attackable> (allAttackableInZone), TempEffect.Timeline.EndRound, () =>
            {
                StartCoroutine (TryStartNewRound ());
            });
        }
    }

    private bool HasCombatEnded ()
    {
        return _currentCombatZone.EnemiesInZone.Count == 0;
    }

    private IEnumerator TryStartNewRound ()
    {
        SetCharactersLeftToPlay ();
        if (_combatEntitiesLeftToPlay.Count > 1)
        {
            _roundCount++;
            _fightRegistry.ReportRoundStart (_roundCount);
            NextTurn ();
        }
        else
        {
            EndCombat ();
        }

        yield return new WaitForSeconds (1f);
    }

    private void ClearFightData ()
    {
        _roundCount = 0;
        _fightRegistry.Clean ();
        _inCombat = false;
        _playerController.StopCombatMode ();
    }

    private void EndCombat ()
    {
        ClearFightData ();
        OnCombatEnded?.Invoke (_currentCombatZone);
        _currentCombatZone.EndFight ();
    }

    public void EscapeFight ()
    {
        ClearFightData ();
        OnCombatEnded?.Invoke (_currentCombatZone);
        _currentCombatZone.EscapeFight ();
    }

    private void ApplyAllTempEffects (Stack<Attackable> attackables, TempEffect.Timeline timeline, Action onAllTempEffectsApplied)
    {
        if (attackables.Count == 0)
        {
            onAllTempEffectsApplied?.Invoke ();
            return;
        }

        Attackable attackable = attackables.Pop ();
        if (attackable.WillBeDestroyed)
        {
            ApplyAllTempEffects (attackables, timeline, onAllTempEffectsApplied);
            return;
        }

        _actionDelayer.ExecuteInSeconds (SECONDS_BETWEEN_APPLY_EFFECTS, () =>
        {
            attackable.ApplyTempEffects (() => ApplyAllTempEffects (attackables, timeline, onAllTempEffectsApplied), _fightRegistry, timeline);
        });
    }
}