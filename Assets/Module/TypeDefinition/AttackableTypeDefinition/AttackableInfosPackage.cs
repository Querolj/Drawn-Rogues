using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AttackableInfosPackage
{
    Transform transform;
    AttackableStats attackableStats;
    List<Effect> effects;
    List<TempEffect> tempEffects;
    AttackableDescription userDescription;
    Bounds spriteBounds;
}