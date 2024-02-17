using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateIconWithNumber : MonoBehaviour
{
    [SerializeField]
    private Image _icon = null;

    [SerializeField]
    private TMP_Text _numberText = null;

    public void Display (Sprite icon, int number)
    {
        _icon.sprite = icon;
        _numberText.text = number.ToString ();
    }
}