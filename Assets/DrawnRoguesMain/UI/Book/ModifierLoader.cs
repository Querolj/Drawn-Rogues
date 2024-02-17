using System.Collections.Generic;
using UnityEngine;

public class ModifierLoader : MonoBehaviour
{
    private Dictionary<ModifierType, List<Modifier>> _modifiersByType = null;

    public Dictionary<ModifierType, List<Modifier>> GetModifiers ()
    {
        if (_modifiersByType?.Count > 0)
            return _modifiersByType;

        _modifiersByType = new Dictionary<ModifierType, List<Modifier>> ();
        Modifier[] modifiers = Resources.LoadAll<Modifier> ("Modifier");

        if (modifiers == null)
            throw new System.Exception ("No modifiers found");

        foreach (Modifier modifier in modifiers)
        {
            if (!_modifiersByType.ContainsKey (modifier.Type))
            {
                _modifiersByType.Add (modifier.Type, new List<Modifier> ());
            }

            _modifiersByType[modifier.Type].Add (modifier);
        }

        return _modifiersByType;

    }

    public int GetMaxModifierList ()
    {
        if (_modifiersByType == null)
            GetModifiers ();

        int max = 0;
        foreach (KeyValuePair<ModifierType, List<Modifier>> modifierType in _modifiersByType)
        {
            if (modifierType.Value.Count > max)
                max = modifierType.Value.Count;
        }
        return max;
    }

}