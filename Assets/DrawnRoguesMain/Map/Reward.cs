using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PixelDropQuantity
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
    private List<PixelDropQuantity> _pixelDropsReward = new List<PixelDropQuantity> ();
    public List<PixelDropQuantity> PixelDropsReward
    {
        get { return _pixelDropsReward; }
    }

    public int XpToGain;

    // [SerializeField]
    // private List<ColouringReward> _colouringsReward = new List<ColouringReward> ();
    // public List<ColouringReward> ColouringsReward
    // {
    //     get { return _colouringsReward; }
    // }
}