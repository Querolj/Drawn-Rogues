using TMPro;
using UnityEngine;

public class EscapeZone : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _chanceOfEscapeText;

    [SerializeField]
    private GameObject _escapeIcon;

    public void FlipIcon ()
    {
        _escapeIcon.transform.rotation = Quaternion.Euler (0, 180, 0);
    }

    public void SetChanceOfEscape (float chance)
    {
        string chanceStr = (chance * 100f).ToString ("F0") + "%";
        if (chance < 0.1f)
        {
            chanceStr = "<color=red>" + chanceStr.ToString () + "</color>";
        }
        else if (chance < 0.25f)
        {
            chanceStr = "<color=orange>" + chanceStr.ToString () + "</color>";
        }
        else if (chance < 0.5f)
        {
            chanceStr = "<color=yellow>" + chanceStr.ToString () + "</color>";
        }
        else
        {
            chanceStr = "<color=green>" + chanceStr.ToString () + "</color>";
        }

        _chanceOfEscapeText.text = "Escape sucess : " + chanceStr;
    }
}