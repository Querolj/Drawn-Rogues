using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionDelayer : MonoBehaviour
{
    private class DelayedAction
    {
        public Action Action;
        public float RemainingSecondsBeforeExecution;
        public float RemainingSecondsBeforeDeletion;

        public DelayedAction (Action action, float remainingSecondsBeforeExecution, float remainingSecondsBeforeDeletion)
        {
            Action = action ??
                throw new ArgumentNullException (nameof (action));
            RemainingSecondsBeforeExecution = remainingSecondsBeforeExecution;
            RemainingSecondsBeforeDeletion = remainingSecondsBeforeDeletion;
        }
    }

    private List<DelayedAction> _actionsToExecute = new List<DelayedAction> ();

    private void Update ()
    {
        if (_actionsToExecute.Count == 0)
            return;

        // Try launch delayed actions
        for (int i = _actionsToExecute.Count - 1; i >= 0; i--)
        {
            DelayedAction delayedAction = _actionsToExecute[i];
            delayedAction.RemainingSecondsBeforeExecution -= Time.deltaTime;
            if (delayedAction.RemainingSecondsBeforeExecution <= 0f)
            {
                delayedAction.Action ();
                if (delayedAction.RemainingSecondsBeforeDeletion <= 0f)
                    _actionsToExecute.RemoveAt (i);
                _actionsToExecute.RemoveAt (i);
            }
        }
    }

    public void ExecuteInSeconds (float delayDuration, Action action, float deletionDelay = 0f)
    {
        DelayedAction delayedAction = new DelayedAction (action, delayDuration, deletionDelay);
        _actionsToExecute.Add (delayedAction);
    }
}