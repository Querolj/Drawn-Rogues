using UnityEngine;

public class ColouringInstance
{
    private Colouring _colouring;
    public Colouring Colouring { get { return _colouring; } }

    public ColouringInstance (Colouring colouring)
    {
        _colouring = GameObject.Instantiate (colouring);
    }
}