using System;

public class CombatEnvironnementHazard : CombatEntity
{
    public virtual void ExecuteTurn (Action onTurnEnded)
    {
        throw new NotImplementedException ();
    }
}