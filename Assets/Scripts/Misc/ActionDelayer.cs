using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionDelayer : MonoBehaviour
{
    public static ActionDelayer Instance;

    private class DelayedAction
    {
        public Action Action;
        public float RemainingSeconds;

        public DelayedAction (Action action, float remainingSeconds)
        {
            Action = action ??
                throw new ArgumentNullException (nameof (action));
            RemainingSeconds = remainingSeconds;
        }
    }

    private List<DelayedAction> _actionsToExecute = new List<DelayedAction> ();

    private void Awake ()
    {
        if (Instance != null)
        {
            Debug.LogError ("Multiple instances of ActionDelayer found.");
            return;
        }
        Instance = this;
    }

    private void Update ()
    {
        if (_actionsToExecute.Count == 0)
            return;

        // Try launch delayed actions
        for (int i = _actionsToExecute.Count - 1; i >= 0; i--)
        {
            DelayedAction delayedAction = _actionsToExecute[i];
            delayedAction.RemainingSeconds -= Time.deltaTime;
            if (delayedAction.RemainingSeconds <= 0f)
            {
                delayedAction.Action ();
                _actionsToExecute.RemoveAt (i);
            }
        }
    }

    public void ExecuteInSeconds (float seconds, Action action)
    {
        DelayedAction delayedAction = new DelayedAction (action, seconds);
        _actionsToExecute.Add (delayedAction);
    }
}