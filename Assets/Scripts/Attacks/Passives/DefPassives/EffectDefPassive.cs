using System;
using UnityEngine;

[CreateAssetMenu (fileName = "EffectDefPassive", menuName = "Passive/EffectDefPassive", order = 1)]
public class EffectDefPassive : EffectPassive { }

[Serializable]
public class EffectDefPassiveSerialized : PassiveSerialized<EffectDefPassive>
{ }