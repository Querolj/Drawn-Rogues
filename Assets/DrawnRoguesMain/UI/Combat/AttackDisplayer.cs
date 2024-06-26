using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackDisplayer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _name;

    [SerializeField]
    private TMP_Text _damage;

    [SerializeField]
    private TMP_Text _targettingType;

    [SerializeField]
    private TMP_Text _range;

    [SerializeField]
    private TMP_Text _effects;

    [SerializeField]
    private Toggle _toggle;
    public Toggle Toggle
    {
        get
        {
            return _toggle;
        }
    }

    public void Display (Attack attack)
    {
        _name.text = attack.Name;
        _damage.text = attack.MinDamage + "-" + attack.MaxDamage + " " + attack.AttackElement.ToString ();
        _targettingType.text = GetTargettingType (attack.AttackSelectionType);
        _range.text = attack.GetRangeInMeter ().ToString () + " meters";
        _effects.text = attack.GetEffectsString ();
    }

    private string GetTargettingType (AttackSelectionType targettingType)
    {
        switch (targettingType)
        {
            case AttackSelectionType.SingleTarget:
                return "Point in range";
            case AttackSelectionType.Projectile:
                return "Projectile";
            case AttackSelectionType.TrajectoryZone:
                return "Zone from distance";
            case AttackSelectionType.Zone:
                return "Zone";
            default:
                return "Unknown";
        }
    }
}