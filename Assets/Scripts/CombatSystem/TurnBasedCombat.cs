using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBasedCombat : MonoBehaviour
{
    [SerializeField]
    private Drawer _drawer;

    [SerializeField]
    private PlayerController _playerController;
    public DrawedCharacter ActivePlayerCharacter { get { return _playerController.ControlledCharacter; } }

    [SerializeField]
    private FightDescription _fightDescription;

    [SerializeField]
    private EnemyTurnIndicator _enemyTurnIndicator;

    private CombatZone _currentCombatZone;
    public event Action<CombatZone> OnCombatInitiated;
    public event Action<CombatZone> OnCombatEnded;

    public Action OnPlayerTurnStart;
    public Action<Character> OneEnemyTurnStart;

    public CombatZone CurrentCombatZone
    {
        get { return _currentCombatZone; }
    }

    private int _roundCount = 0;

    // Attacks ordered by priority
    private Stack<CombatEntity> _combatEntitiesLeftToPlay;
    private void Awake () { }

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

        OnCombatInitiated?.Invoke (combatZone);
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

    private IEnumerator StartCharacterTurn (Character character)
    {
        if (character == null)
            throw new ArgumentNullException (nameof (character));

        if (!character.CanPlayTurn ())
        {
            _enemyTurnIndicator.Hide ();
            _fightDescription.Report (character.Name + " can't play this turn");
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
                Debug.Log ("Enemy turn : " + character.Name);
                _enemyTurnIndicator.SetOnCharacter (character);

                OneEnemyTurnStart?.Invoke (character);
                yield return new WaitForSeconds (0.5f);
                if (character.HasAI ())
                    character.GetAI ().ExecuteTurn (_currentCombatZone, _playerController.ControlledCharacter, _fightDescription, () => EndTurn (character));
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
        ApplyAllTempEffects (new Stack<Attackable> (new List<Attackable> { lastTurnCharacter }), TempEffect.Timeline.EndTurn, () =>
        {
            NextTurn ();
        });
    }

    private void NextTurn ()
    {
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
            _fightDescription.ReportRoundStart (_roundCount);
            NextTurn ();
        }
        else
        {
            EndCombat ();
        }

        yield return new WaitForSeconds (1f);
    }

    private void EndCombat ()
    {
        _fightDescription.Report ("Combat ended!");
        OnCombatEnded?.Invoke (_currentCombatZone);
        _currentCombatZone.EndFight ();
    }

    public void EscapeFight ()
    {
        _fightDescription.Report ("Escaped!");
        OnCombatEnded?.Invoke (_currentCombatZone);
        _currentCombatZone.EscapeFight ();
    }

    private const float SECONDS_BETWEEN_APPLY_ALL_EFFECTS = 0.5f;
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

        ActionDelayer.Instance.ExecuteInSeconds (SECONDS_BETWEEN_APPLY_ALL_EFFECTS, () =>
        {
            attackable.ApplyTempEffects (() => ApplyAllTempEffects (attackables, timeline, onAllTempEffectsApplied), _fightDescription, timeline);
        });
    }
}