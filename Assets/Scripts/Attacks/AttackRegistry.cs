using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackRegistry : MonoBehaviour
{
    private List<Attack> _attacks = new List<Attack> ();

    private void Awake ()
    {
        _attacks = Resources.LoadAll<Attack> ("Attacks").ToList ();
    }

    public List<Attack> GetAttacksToChooseFrom (DrawedCharacter dc)
    {
        const int NUM_OF_ATTACKS_TO_CHOOSE = 2;

        List<Attack> attacksToChooseFrom = new List<Attack> ();
        foreach (Attack attack in _attacks)
        {
            if (AttackCanBeChosen (dc, attack))
                attacksToChooseFrom.Add (attack);
        }

        if (attacksToChooseFrom.Count < NUM_OF_ATTACKS_TO_CHOOSE)
            throw new System.Exception ($"Not enough attacks found for " + dc.Name);

        // Get random attacks
        List<Attack> randomAttacks = new List<Attack> ();
        for (int i = 0; i < NUM_OF_ATTACKS_TO_CHOOSE; i++)
        {
            int randomIndex = Random.Range (0, attacksToChooseFrom.Count);
            randomAttacks.Add (attacksToChooseFrom[randomIndex]);
            attacksToChooseFrom.RemoveAt (randomIndex);
        }

        return randomAttacks;
    }

    private bool AttackCanBeChosen (DrawedCharacter dc, Attack attack)
    {
        if (!attack.AvailableForPlayer)
            return false;

        if (dc.Attacks.Any (a => a.Name == attack.Name))
            return false;

        if (attack.RequiredLevel > dc.Level)
            return false;

        if (attack.RequiredHeightAdjectives != null && attack.RequiredHeightAdjectives.Length > 0 && !attack.RequiredHeightAdjectives.Contains (dc.DrawedCharacterFormDescription.HeightAdjective))
            return false;

        if (attack.RequiredWidthAdjectives != null && attack.RequiredWidthAdjectives.Length > 0 && !attack.RequiredWidthAdjectives.Contains (dc.DrawedCharacterFormDescription.WidthAdjective))
            return false;

        if (attack.RequiredNumberOfArmsMin != -1 && dc.DrawedCharacterFormDescription.NumberOfArms < attack.RequiredNumberOfArmsMin)
            return false;

        if (attack.RequiredNumberOfArmsMax != -1 && dc.DrawedCharacterFormDescription.NumberOfArms > attack.RequiredNumberOfArmsMax)
            return false;

        if (attack.RequiredNumberOfLegsMin != -1 && dc.DrawedCharacterFormDescription.NumberOfLegs < attack.RequiredNumberOfLegsMin)
            return false;

        if (attack.RequiredNumberOfLegsMax != -1 && dc.DrawedCharacterFormDescription.NumberOfLegs > attack.RequiredNumberOfLegsMax)
            return false;

        return true;
    }
}