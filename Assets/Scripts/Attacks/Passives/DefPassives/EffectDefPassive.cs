using System;
using UnityEngine;

// Effect passive that is usually used to make the character boost his effect coming from his attack (ex : +10% poison efficiency)
[CreateAssetMenu (fileName = "EffectDefPassive", menuName = "Passive/EffectDefPassive", order = 1)]
public class EffectDefPassive : EffectPassive { }

[Serializable]
public class EffectDefPassiveSerialized : PassiveSerialized<EffectDefPassive>
{ }