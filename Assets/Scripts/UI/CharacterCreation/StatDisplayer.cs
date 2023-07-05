using TMPro;
using UnityEngine;

public enum Stat
{
    Intelligence,
    Strenght,
    Life,
    Mobility
}

public class StatDisplayer : MonoBehaviour
{
    [SerializeField]
    private Stat _statToDisplay;

    [SerializeField]
    private DrawedCharacter _character;

    [SerializeField]
    private TMP_Text _text;

    void Update ()
    {
        UpdateStatDisplay ();
    }

    private void UpdateStatDisplay ()
    {
        switch (_statToDisplay)
        {
            case Stat.Intelligence:
                _text.text = _character.Stats.Intelligence.ToString ();
                break;
            case Stat.Strenght:
                _text.text = _character.Stats.Strenght.ToString ();
                break;
            case Stat.Life:
                _text.text = _character.Stats.Life.ToString ();
                break;
            case Stat.Mobility:
                _text.text = _character.Stats.Mobility.ToString ();
                break;
            default:
                Debug.LogError ("unknown stat " + _statToDisplay);
                break;
        }
    }
}