using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorDropReward : MonoBehaviour
{
    [SerializeField]
    private Image _dropletImage;

    [SerializeField]
    private TMP_Text _dropNumText;

    public void Display (BaseColor baseColor, int dropNum)
    {
        _dropletImage.color = BaseColorUtils.GetColor (baseColor);
        _dropNumText.text = "x " + dropNum + " " + BaseColorUtils.GetColorName (baseColor) + " pixels obtained!";
        _dropNumText.text = BaseColorUtils.ColorText (_dropNumText.text, baseColor);
    }
}