using TMPro;
using UnityEngine;

public class DisplayModifiersAvailable : MonoBehaviour
{
    [SerializeField]
    private TMP_Text ModifiersAvailableText;

    [SerializeField]
    private CharacterCanvas _characterCanvas;

    private void Update ()
    {
        ModifiersAvailableText.text = "Modifiers : " + _characterCanvas.ModifiersCount + " / " + _characterCanvas.MaxModifieurAllowed;
    }
}