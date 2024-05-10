using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoDebug : MonoBehaviour
{
    private PlayerController _playerController;

    void Start()
    {
        _playerController = FindAnyObjectByType<PlayerController> (); // TODO : inject
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
        }
    }

    private void ReloadSceneWithArachu()
    {
        // Load scene with Arachu

    }
}
