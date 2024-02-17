using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CombatEntity : MonoBehaviour
{
    public class Factory : PlaceholderFactory<GameObject, CombatEntity> { }

    public List<CombatEntity> LinkedCombatEntities = new List<CombatEntity> ();
    public virtual int GetTurnOrder ()
    {
        throw new NotImplementedException ();
    }
}