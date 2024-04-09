using System;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviourNoAction : AIBehaviour
{
    public override void ExecuteTurn (CombatZone combatZone, Character playerCharacter, FightRegistry fightDescription, Action onTurnEnd)
    {
        onTurnEnd ();
    }
}