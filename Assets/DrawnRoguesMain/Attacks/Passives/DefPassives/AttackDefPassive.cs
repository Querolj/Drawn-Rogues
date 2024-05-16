using System;
using UnityEngine;

// Attack passive that is usually used to make the character defend from an attack (ex : resistance to fire)
[CreateAssetMenu (fileName = "AttackDefPassive", menuName = "Passive/AttackDefPassive", order = 1)]
public class AttackDefPassive : AttackPassive
{
}

[Serializable]
public class AttackDefPassiveSerialized : PassiveSerialized<AttackDefPassive> { }