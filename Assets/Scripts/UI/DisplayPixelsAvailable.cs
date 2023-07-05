using TMPro;
using UnityEngine;

public class DisplayPixelsAvailable : MonoBehaviour
{
    [SerializeField]
    private TMP_Text PixelsAvailableText;

    [SerializeField]
    private Frame _frame;

    private void Update ()
    {
        PixelsAvailableText.text = "Pixels : " + (_frame.MaxPixelsAllowed - _frame.CurrentPixelsAllowed) + " / " + _frame.MaxPixelsAllowed;
    }
}