using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Attackable))]
public class Vacuumable : MonoBehaviour
{
    [SerializeField]
    private List<ColorDropQuantity> _colorDropsReward = new List<ColorDropQuantity> ();
    public List<ColorDropQuantity> ColorDropsReward
    {
        get { return _colorDropsReward; }
    }

    [SerializeField]
    private List<ColouringReward> _colouringsReward = new List<ColouringReward> ();
    public List<ColouringReward> ColouringsReward
    {
        get { return _colouringsReward; }
    }

    [SerializeField]
    private float _vacuumingBaseChance = 0.5f;
    public float VacuumingBaseChance
    {
        get { return _vacuumingBaseChance; }
    }
}