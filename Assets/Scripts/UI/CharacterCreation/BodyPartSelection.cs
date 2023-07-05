using UnityEngine;
using UnityEngine.UI;

public class BodyPartSelection : MonoBehaviour
{
    [SerializeField]
    private Toggle _headToggle;

    [SerializeField]
    private Toggle _bodyToggle;

    [SerializeField]
    private Toggle _armToggle;

    [SerializeField]
    private Toggle _legToggle;

    public PixelUsage GetPixelUsageFromSelectedBodyPart ()
    {
        if (_headToggle.isOn)
        {
            return PixelUsage.Head;
        }
        else if (_bodyToggle.isOn)
        {
            return PixelUsage.Body;
        }
        else if (_armToggle.isOn)
        {
            return PixelUsage.Arm;
        }
        else if (_legToggle.isOn)
        {
            return PixelUsage.Leg;
        }
        else
            throw new System.Exception ("No body part selected");

    }
}