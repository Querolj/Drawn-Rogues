using System.Collections.Generic;
using UnityEngine;

public class StatusIconsDisplayer : MonoBehaviour
{
    [SerializeField]
    private StatusIconWithNumber _templateStatusIcon;

    private Dictionary<TempEffect, StatusIconWithNumber> _statusIcons = new Dictionary<TempEffect, StatusIconWithNumber> ();

    public void DisplayStatus (List<TempEffect> tempEffects)
    {
        foreach (StatusIconWithNumber statusIcon in _statusIcons.Values)
        {
            statusIcon.gameObject.SetActive (false);
        }

        foreach (TempEffect tempEffect in tempEffects)
        {
            if (!_statusIcons.ContainsKey (tempEffect))
            {
                StatusIconWithNumber statusIcon = Instantiate (_templateStatusIcon, transform);
                statusIcon.Display (tempEffect.Icon, tempEffect.TurnDuration);
                _statusIcons.Add (tempEffect, statusIcon);
            }
            else
            {
                _statusIcons[tempEffect].gameObject.SetActive (true);
                _statusIcons[tempEffect].Display (tempEffect.Icon, tempEffect.TurnDuration);
            }
        }
    }
}