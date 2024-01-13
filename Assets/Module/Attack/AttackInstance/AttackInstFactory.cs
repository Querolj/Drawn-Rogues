using System;
using Zenject;

public class AttackInstFactory : IFactory<Attack, Character, AttackInstance>
{
    DiContainer _container;

    public AttackInstFactory (DiContainer container)
    {
        _container = container;
    }

    public AttackInstance Create (Attack attack, Character owner)
    {
        AttackInstance attackInstance;
        if (attack as AttackTrajectoryZone)
            attackInstance = _container.Instantiate<AttackInstTrajectoryZone> ();
        else if (attack as AttackZone)
            attackInstance = _container.Instantiate<AttackInstZone> ();
        else if (attack as AttackProjectile)
            attackInstance = _container.Instantiate<AttackInstProjectile> ();
        else if (attack as AttackTrajectory)
            attackInstance = _container.Instantiate<AttackInstTrajectory> ();
        else if (attack as AttackSingleTarget)
            attackInstance = _container.Instantiate<AttackInstSingleTarget> ();
        else if (attack as AttackJump)
            attackInstance = _container.Instantiate<AttackInstJump> ();
        else
            throw new ArgumentException ("Attack type " + attack.GetType () + "not supported");

        attackInstance.Init (attack, owner);
        return attackInstance;
    }
}