using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseColorDisplay : MonoBehaviour
{
    [SerializeField]
    private Image _baseColorImage;

    [SerializeField]
    private TMP_Text _dropNumber;

    public void Display (BaseColor bc, int dropNumber)
    {
        _baseColorImage.color = BaseColorToColor.GetColor (bc);
        _dropNumber.text = dropNumber.ToString ();
    }
}