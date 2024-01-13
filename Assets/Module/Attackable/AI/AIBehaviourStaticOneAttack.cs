using System;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviourStaticOneAttack : AIBehaviour
{
    public override void ExecuteTurn (CombatZone combatZone, Character playerCharacter, FightRegistry fightDescription, Action onTurnEnd)
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
            onTurnEnd ();
        }
        else
        {
            Attack attack = reachableAttacks[0];
            ExecuteAttack (playerCharacter, attack, onTurnEnd);
        }
    }
}