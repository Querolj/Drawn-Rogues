using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSurfaceDetector : MonoBehaviour
{
    public enum SurfaceType
    {
        Ground,
        QuickSand
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private SurfaceType GetMapSurfaceType (Vector3 position)
    {
        position.y = 100f;
        if (Physics.Raycast (position, Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            return hit.point.y;
        }
        return 0f;
    }
}
