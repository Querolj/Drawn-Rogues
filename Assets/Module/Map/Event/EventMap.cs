using UnityEngine;

public class EventMap : MonoBehaviour
{
    protected bool _triggered = false;
    public bool IsTriggered { get { return _triggered; } }

    public virtual void Trigger ()
    {
        _triggered = true;
    }
}