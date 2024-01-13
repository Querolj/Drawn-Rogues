using System;

public interface ICombatEnvironnementHazard : ICombatEntity
{
    public void ExecuteTurn (Action onTurnEnded);
}