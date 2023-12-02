using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

[RequireComponent (typeof (Character))]
public abstract class AIBehaviour : MonoBehaviour
{
    protected Character _character;
    protected TrajectoryCalculator _trajectoryCalculator;
    protected CharacterAnimation _characterAnimation;

    #region  Injected
    private ActionDelayer _actionDelayer;
    protected AttackInstance.Factory _attackInstanceFactory;
    #endregion

    [Inject, UsedImplicitly]
    private void Init (ActionDelayer actionDelayer, AttackInstance.Factory attackInstanceFactory)
    {
        _actionDelayer = actionDelayer;
        _attackInstanceFactory = attackInstanceFactory;
    }

    private void Awake ()
    {
        _trajectoryCalculator = new TrajectoryCalculator ();
        _characterAnimation = GetComponentInParent<CharacterAnimation> ();
        _character = GetComponent<Character> ();
    }

    public virtual void ExecuteTurn (CombatZone combatZone, Character playerCharacter, FightRegistry fightDescription, Action onTurnEnd)
    {
        throw new NotImplementedException ();
    }

    protected void ExecuteAttack (Character playerCharacter, Attack attack, Action onTurnEnd)
    {

        Vector3 attackPos = ((Bounds) playerCharacter.GetSpriteBounds ()).center;
        attackPos.z = _character.transform.position.z;
        AttackInstance attackInstance = _attackInstanceFactory.Create (attack, _character);
        attackInstance.OnAttackStarted += _characterAnimation.PlayAttackAnimation;

        if ((_character.CharMovement.DirectionRight && playerCharacter.transform.position.x < _character.transform.position.x) ||
            (!_character.CharMovement.DirectionRight && playerCharacter.transform.position.x > _character.transform.position.x))
            _character.CharMovement.TurnTowardTarget (playerCharacter.transform.position);

        // wait for the char to turn
        _actionDelayer.ExecuteInSeconds (0.25f, () =>
        {
            switch (attackInstance)
            {
                case AttackInstTrajectoryZone attackInstTrajectoryZone:
                    ExecuteTrajectoryZoneAttack (playerCharacter, onTurnEnd, attackPos, attackInstTrajectoryZone);
                    break;
                case AttackInstTrajectory attackInstTrajectory:
                    ExecuteTrajectoryAttack (playerCharacter, onTurnEnd, attackPos, attackInstTrajectory);
                    break;
                case AttackInstProjectile attackInstProjectile:
                    ExecuteProjectileAttack (playerCharacter, onTurnEnd, attackPos, attackInstProjectile);
                    break;
                case AttackInstSingleTarget attackInstSingleTarget:
                    attackInstSingleTarget.Execute (_character, playerCharacter, attackPos, onTurnEnd);
                    break;
                case AttackInstZone attackInstArea:
                    attackInstArea.Execute (_character, null, attackPos, onTurnEnd, new List<Attackable> () { playerCharacter });
                    break;
                default:
                    throw new Exception ("Attack instance not supported");
            }
        });

    }

    private void ExecuteProjectileAttack (Character playerCharacter, Action onTurnEnd, Vector3 attackPos, AttackInstProjectile attackInstProjectile)
    {
        Bounds attackerBounds = (Bounds) _character.GetSpriteBounds ();
        Vector3 attackerOriginPosition = attackerBounds.center;
        attackerOriginPosition.z = _character.transform.position.z;
        Vector3 projectileStartPosition = attackerOriginPosition;

        if (_character.CharMovement.DirectionRight)
            projectileStartPosition += attackerBounds.extents;
        else
        {
            projectileStartPosition.x -= attackerBounds.extents.x;
            projectileStartPosition.y += attackerBounds.extents.y;
        }

        List<Vector3> trajPoints = GetTrajectoryToPlayer (projectileStartPosition, playerCharacter, attackInstProjectile.TrajectorySpeed,
            attackInstProjectile.TrajectoryRadius);
        attackInstProjectile.Execute (_character, playerCharacter, attackPos, onTurnEnd, null, trajPoints);
    }

    private void ExecuteTrajectoryAttack (Character playerCharacter, Action onTurnEnd, Vector3 attackPos, AttackInstTrajectory attackInstTrajectory)
    {
        Bounds attackerBounds = (Bounds) _character.GetSpriteBounds ();
        Vector3 attackerOriginPosition = attackerBounds.center;
        attackerOriginPosition.z = _character.transform.position.z;

        List<Vector3> trajPoints = GetTrajectoryToPlayer (attackerOriginPosition, playerCharacter, attackInstTrajectory.TrajectorySpeed,
            attackInstTrajectory.TrajectoryRadius);
        attackInstTrajectory.Execute (_character, playerCharacter, attackPos, onTurnEnd, null, trajPoints);
    }

    private void ExecuteTrajectoryZoneAttack (Character playerCharacter, Action onTurnEnd, Vector3 attackPos, AttackInstTrajectoryZone attackInstTrajectoryZone)
    {
        Bounds attackerBounds = (Bounds) _character.GetSpriteBounds ();
        Vector3 attackerOriginPosition = attackerBounds.center;
        attackerOriginPosition.z = _character.transform.position.z;

        List<Vector3> trajPoints = GetTrajectoryToPlayer (attackerOriginPosition, playerCharacter, attackInstTrajectoryZone.TrajectorySpeed,
            attackInstTrajectoryZone.TrajectoryRadius);
        attackInstTrajectoryZone.Execute (_character, playerCharacter, attackPos, onTurnEnd, new List<Attackable> () { playerCharacter }, trajPoints);
    }

    protected List<Vector3> GetTrajectoryToPlayer (Vector3 startPos, Character playerCharacter, float speed, float radius)
    {
        Bounds playerBounds = (Bounds) playerCharacter.GetSpriteBounds ();
        Vector3 targetPos = playerBounds.center;
        targetPos.z = playerCharacter.transform.position.z;

        return _trajectoryCalculator.GetCurvedTrajectory (startPos, targetPos, radius, _character.gameObject.GetInstanceID (), out Attackable attackableHit);
    }

    protected void MoveTowardCharacter (CombatZone combatZone, Character targetCharacter, Action onTurnEnd)
    {
        combatZone.TryCastCharacterToAttackable (_character.MaxDistanceToMove, _character, targetCharacter, _character.transform.position, out Attackable attackable);
        if (attackable != null)
        {
            _character.CharMovement.MoveNextToCharacter ((Bounds) attackable.GetSpriteBounds (), (Bounds) _character.GetSpriteBounds (), 0.1f, onTurnEnd);
        }
        else
        {
            bool rightSide = _character.transform.position.x < targetCharacter.transform.position.x;
            Vector3 target = _character.transform.position + (rightSide ? Vector3.right : Vector3.left) * _character.MaxDistanceToMove;
            _character.CharMovement.MoveToTarget (target, onTurnEnd);
        }

    }
}