using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorDropQuantity
{
    public BaseColor BaseColor;
    public int Quantity;
}

[System.Serializable]
public class ColouringReward
{
    public Colouring Colouring;
    public float ChanceToDrop;
}

public class Reward : MonoBehaviour
{
    [SerializeField]
    private List<ColorDropQuantity> _colorDropsReward = new List<ColorDropQuantity> ();
    public List<ColorDropQuantity> ColorDropsReward
    {
        get { return _colorDropsReward; }
    }

    public int XpToGain;

    // [SerializeField]
    // private List<ColouringReward> _colouringsReward = new List<ColouringReward> ();
    // public List<ColouringReward> ColouringsReward
    // {
    //     get { return _colouringsReward; }
    // }
}