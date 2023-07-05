using System;
using UnityEngine;

[CreateAssetMenu (fileName = "EffectOffPassive", menuName = "Passive/EffectOffPassive", order = 1)]
public class EffectOffPassive : EffectPassive { }

[Serializable]
public class EffectOffPassiveSerialized : PassiveSerialized<EffectOffPassive>
{ }