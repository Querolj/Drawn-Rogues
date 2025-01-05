using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraService : MonoBehaviour
{
    [SerializeField]
    private CinemachineBrain _cinemachineBrain;

    public bool IsBlending
    {
        get
        {
            return _cinemachineBrain.IsBlending;
        }
    }
}
