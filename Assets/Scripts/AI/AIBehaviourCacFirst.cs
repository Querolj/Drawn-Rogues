using System;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviourCacFirst : AIBehaviour
{
    public float CacRange = 0.5f;

    public override void ExecuteTurn (CombatZone combatZone, Character playerCharacter, FightDescription fightDescription, Action onTurnEnd)
    {
        float playerToCharDist = Vector3.Distance (playerCharacter.transform.position, _character.transform.position);
        Bounds bounds = (Bounds) _character.GetSpriteBounds ();

        float radiusAdded = bounds.extents.x;
        List<Attack> reachableCacAttacks = new List<Attack> ();
        List<Attack> reachableNonCacAttacks = new List<Attack> ();

        foreach (Attack attack in _character.Attacks)
        {
            if (attack.GetRangeInMeter () + radiusAdded >= playerToCharDist)
            {
                if (attack.GetRangeInMeter () <= CacRange)
                    reachableCacAttacks.Add (attack);
                else
                    reachableNonCacAttacks.Add (attack);
            }
        }
        if (reachableCacAttacks.Count == 0 && reachableNonCacAttacks.Count == 0)
        {
            fightDescription.Report (fightDescription.GetColoredAttackableName (_character) + " moved.");
            MoveTowardCharacter (combatZone, playerCharacter, onTurnEnd);
        }
        else if (reachableCacAttacks.Count > 0)
        {
            Attack attack = reachableCacAttacks[UnityEngine.Random.Range (0, reachableCacAttacks.Count)];
            ExecuteAttack (playerCharacter, attack, onTurnEnd);
        }
        else if (combatZone.TryCastCharacterToAttackable (combatZone.SizeX, _character, playerCharacter, _character.transform.position, out Attackable attackable))
        {
            fightDescription.Report (fightDescription.GetColoredAttackableName (_character) + " moved.");
            MoveTowardCharacter (combatZone, playerCharacter, onTurnEnd);
        }
        else
        {
            Attack attack = reachableNonCacAttacks[UnityEngine.Random.Range (0, reachableNonCacAttacks.Count)];
            ExecuteAttack (playerCharacter, attack, onTurnEnd);
        }
    }

}