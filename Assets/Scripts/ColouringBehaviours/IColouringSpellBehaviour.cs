using System.Collections.Generic;
using UnityEngine;

public interface IColouringSpellBehaviour
{
    void Init (TurnBasedCombat bg, List<Vector2> lastStrokeDrawUVs, FrameDecor frameDecor);
}