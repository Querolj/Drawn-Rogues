using System;
using UnityEngine;

// Effect passive that is usually used to make the character lower the efficiency of an ennemy effect that would be applied to the character 
// (ex : -10% poison efficiency)
[CreateAssetMenu (fileName = "EffectOffPassive", menuName = "Passive/EffectOffPassive", order = 1)]
public class EffectOffPassive : EffectPassive { }

[Serializable]
public class EffectOffPassiveSerialized : PassiveSerialized<EffectOffPassive>
{ }