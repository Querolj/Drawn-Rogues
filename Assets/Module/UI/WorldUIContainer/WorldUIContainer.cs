using UnityEngine;

[RequireComponent (typeof (Canvas))]
public class WorldUIContainer : MonoBehaviour
{
    public void AddUI (Transform ui)
    {
        ui.SetParent (transform);
    }
}