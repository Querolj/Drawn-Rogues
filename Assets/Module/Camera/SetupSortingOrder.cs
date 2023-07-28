using UnityEngine;

public class SetupSortingOrder : MonoBehaviour
{
    private void Awake()
    {
        Camera camera = Camera.main;
        camera.transparencySortMode = TransparencySortMode.CustomAxis;
        camera.transparencySortAxis = new Vector3(0, 0, 1);
    }
}