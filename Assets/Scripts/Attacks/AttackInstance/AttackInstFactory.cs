using System;

public class AttackInstFactory
{
    private static FightDescription _fightDescription;
    public static void Init (FightDescription fightDescription)
    {
        if (_fightDescription != null)
            throw new InvalidOperationException ("AttackInstFactory already initialized");

        _fightDescription = fightDescription ??
            throw new ArgumentNullException (nameof (fightDescription));
    }

    public static AttackInstance Create (Attack attack, Character owner)
    {

        if (attack as AttackTrajectoryZone)
            return new AttackInstTrajectoryZone (attack, owner, _fightDescription);
        else if (attack as AttackZone)
            return new AttackInstZone (attack, owner, _fightDescription);
        else if (attack as AttackProjectile)
            return new AttackInstProjectile (attack, owner, _fightDescription);
        else if (attack as AttackTrajectory)
            return new AttackInstTrajectory (attack, owner, _fightDescription);
        else if (attack as AttackSingleTarget)
            return new AttackInstSingleTarget (attack, owner, _fightDescription);
        else if (attack as AttackJump)
            return new AttackInstJump (attack, owner, _fightDescription);
        else
            throw new ArgumentException ("Attack type " + attack.GetType () + "not supported");
    }
}