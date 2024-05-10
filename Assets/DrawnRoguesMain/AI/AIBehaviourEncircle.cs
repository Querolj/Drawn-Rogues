using System;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviourEncircle : AIBehaviour
{
    [SerializeField]
    private Attack _circleAttack;

    public override void ExecuteTurn (CombatZone combatZone, Character playerCharacter, FightRegistry fightDescription, Action onTurnEnd)
    {
        if (_circleAttack == null)
            throw new ArgumentNullException (nameof (_circleAttack));

        bool encircleAttackFound = false;
        foreach (Attack attack in _character.Attacks)
        {
            if (attack.Name == _circleAttack.Name)
            {
                encircleAttackFound = true;
                break;
            }
        }
        if (!encircleAttackFound)
            throw new ArgumentException ("Character does not contain encircle attack " + _circleAttack.Name);

        Bounds playerBounds = playerCharacter.GetSpriteBounds ();
        Vector3 sideOfPlayerToFocus = playerBounds.center;
        sideOfPlayerToFocus.x += playerBounds.extents.x * (_character.transform.position.x > playerCharacter.transform.position.x ? 1 : -1);
        sideOfPlayerToFocus.z = _character.transform.position.z;
        float playerToCharDist = Vector3.Distance (sideOfPlayerToFocus, _character.transform.position);

        Bounds bounds = (Bounds) _character.GetSpriteBounds ();
        if (TryAllowCircle (combatZone, playerCharacter, bounds, out Vector3 targetPos))
        {
            // Debug.Log ("Encircle " + _character.Description.DisplayName + " to " + targetPos);
            _character.CharMovement.TurnTowardTarget (targetPos);
            AttackInstJump jump = _attackInstanceFactory.Create (_circleAttack, _character) as AttackInstJump;
            float radius = bounds.extents.x > bounds.extents.y ? bounds.extents.x : bounds.extents.y;

            List<Vector3> trajPoints = _trajectoryCalculator.GetCurvedTrajectory (_character.transform.position, targetPos, radius, _character.gameObject.GetInstanceID ());
            jump.Execute (_character, null, Vector3.zero, onTurnEnd, null, trajPoints);

            return;
        }

        float radiusAdded = bounds.extents.x;
        List<Attack> reachableAttacks = new List<Attack> ();
        foreach (Attack attack in _character.Attacks)
        {
            if (attack.Name == _circleAttack.Name)
                continue;

            if (attack.GetRangeInMeter () + radiusAdded >= playerToCharDist)
            {
                reachableAttacks.Add (attack);
            }
        }

        if (reachableAttacks.Count == 0)
        {
            fightDescription.Report (fightDescription.GetColoredAttackableName (_character.Description.DisplayName, _character.tag) + " moved.");
            MoveTowardCharacter (combatZone, playerCharacter, onTurnEnd);
        }
        else
        {
            Attack attack = reachableAttacks[UnityEngine.Random.Range (0, reachableAttacks.Count)];
            ExecuteAttack (playerCharacter, attack, onTurnEnd);
        }
    }

    private bool TryAllowCircle (CombatZone combatZone, Character player, Bounds charBounds, out Vector3 position)
    {
        position = Vector3.zero;

        if (combatZone.EnemiesInZone.Count < 2)
            return false;

        bool rightSideOfPlayer = _character.transform.position.x > player.transform.position.x;

        // check if another enemy is on the other side of the player
        foreach (Character character in combatZone.EnemiesInZone)
        {
            if (character == _character)
                continue;

            bool rightSide = character.transform.position.x > player.transform.position.x;
            if (rightSideOfPlayer != rightSide)
                return false;
        }

        // Check a free zone next to the player, on the side where there are no enemies
        Bounds playerBounds = player.GetSpriteBounds ();

        Vector3 startingPos = playerBounds.center;
        startingPos.x += playerBounds.extents.x * (!rightSideOfPlayer ? 1 : -1) + charBounds.extents.x * (!rightSideOfPlayer ? 1 : -1);
        startingPos.z = _character.transform.position.z;
        if (!Utils.TryGetMapHeight (startingPos, out float height))
            return false;
        startingPos.y = height + charBounds.extents.y;

        // if the potential free zone is too far from the character to use the encircleAttack, don't allow to encircle
        float zoneSize = _circleAttack.GetRangeInMeter () - Mathf.Abs (startingPos.x - _character.transform.position.x);
        // Debug.Log ("zoneSize : " + zoneSize + " " + _encircleAttack.Range + " " + Mathf.Abs (startingPos.x - _character.transform.position.x));
        if (zoneSize < charBounds.size.x)
            return false;

        float castXLimit = combatZone.CastCharacterInZone (zoneSize, _character, !rightSideOfPlayer, startingPos,
            new HashSet<int> () { player.ColliderId, player.gameObject.GetInstanceID () });

        // Does the char fit in the focused zone?
        if (Mathf.Abs (castXLimit - startingPos.x) < charBounds.size.x)
            return false;

        float leftLimitX, rightLimitX;
        if (rightSideOfPlayer)
        {
            rightLimitX = startingPos.x - charBounds.extents.x;
            leftLimitX = castXLimit + charBounds.extents.x;
        }
        else
        {
            leftLimitX = startingPos.x + charBounds.extents.x;
            rightLimitX = castXLimit - charBounds.extents.x;
        }

        // Debug.Log ("Zone : " + leftLimitX + " " + rightLimitX + " " + startingPos.ToString ("F3") + " " + charBounds.extents.x + " " + castXLimit + " " + charBounds.size.x);

        position.x = Mathf.Lerp (leftLimitX, rightLimitX, UnityEngine.Random.Range (0.0f, 1.0f));
        position.z = _character.transform.position.z;
        if (!Utils.TryGetMapHeight (position, out height))
            return false;
        position.y = height;

        return true;
    }
}