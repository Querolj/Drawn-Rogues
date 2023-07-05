using System.Collections.Generic;
using UnityEngine;

public class StateIconsDisplayer : MonoBehaviour
{
    [SerializeField]
    private StateIconWithNumber _templateStateIcon;

    private Dictionary<TempEffect, StateIconWithNumber> _stateIcons = new Dictionary<TempEffect, StateIconWithNumber> ();

    public void DisplayState (List<TempEffect> tempEffects)
    {
        foreach (StateIconWithNumber stateIcon in _stateIcons.Values)
        {
            stateIcon.gameObject.SetActive (false);
        }

        foreach (TempEffect tempEffect in tempEffects)
        {
            if (!_stateIcons.ContainsKey (tempEffect))
            {
                StateIconWithNumber stateIcon = Instantiate (_templateStateIcon, transform);
                stateIcon.Display (tempEffect.Icon, tempEffect.TurnDuration);
                _stateIcons.Add (tempEffect, stateIcon);
            }
            else
            {
                _stateIcons[tempEffect].gameObject.SetActive (true);
                _stateIcons[tempEffect].Display (tempEffect.Icon, tempEffect.TurnDuration);
            }
        }
    }
}