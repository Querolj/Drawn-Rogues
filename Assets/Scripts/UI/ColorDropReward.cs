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
        _dropletImage.color = BaseColorToColor.GetColor (baseColor);
        _dropNumText.text = "x " + dropNum + " " + BaseColorToColor.GetColorName (baseColor) + " droplets obtained!";
        _dropNumText.text = BaseColorToColor.ColorText (_dropNumText.text, baseColor);
    }
}