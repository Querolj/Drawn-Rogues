using System;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviourAgressive : AIBehaviour
{
    public override void ExecuteTurn (CombatZone combatZone, Character playerCharacter, FightDescription fightDescription, Action onTurnEnd)
    {
        float playerToCharDist = Vector3.Distance (playerCharacter.transform.position, _character.transform.position);
        Bounds bounds = (Bounds) _character.GetSpriteBounds ();
        float radiusAdded = bounds.extents.x;

        List<Attack> reachableAttacks = new List<Attack> ();
        foreach (Attack attack in _character.Attacks)
        {
            if (attack.GetRangeInMeter () + radiusAdded >= playerToCharDist)
            {
                reachableAttacks.Add (attack);
            }
        }

        if (reachableAttacks.Count == 0)
        {
            fightDescription.Report (fightDescription.GetColoredAttackableName (_character.Description, _character.tag) + " moved.");
            MoveTowardCharacter (combatZone, playerCharacter, onTurnEnd);
        }
        else
        {
            Attack attack = reachableAttacks[UnityEngine.Random.Range (0, reachableAttacks.Count)];
            ExecuteAttack (playerCharacter, attack, onTurnEnd);
        }
    }
}