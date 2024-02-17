using TMPro;
using UnityEngine;

public class ModifierInfoDisplayer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _modifierInfoText;

    private void Awake ()
    {
        _modifierInfoText.text = string.Empty;
    }

    public void SetModifierInfo (Modifier modifier)
    {
        _modifierInfoText.gameObject.SetActive (true);
        _modifierInfoText.text = modifier.ToString ();
    }

    public void RemoveModifierInfo ()
    {
        _modifierInfoText.gameObject.SetActive (false);
    }
}