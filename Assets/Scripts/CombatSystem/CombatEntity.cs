using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatEntity : MonoBehaviour
{
    public List<CombatEntity> LinkedCombatEntities = new List<CombatEntity> ();
    public virtual int GetTurnOrder ()
    {
        throw new NotImplementedException ();
    }
}