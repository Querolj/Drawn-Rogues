using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface ICombatEntity
{
    public class Factory : PlaceholderFactory<GameObject, ICombatEntity> { }
    public List<ICombatEntity> GetLinkedCombatEntities ();
    public int GetTurnOrder ();
}