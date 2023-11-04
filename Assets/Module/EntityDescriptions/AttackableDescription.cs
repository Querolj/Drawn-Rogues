using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackableDescription : MonoBehaviour
{
    [SerializeField]
    private string _displayName;
    public string DisplayName
    {
        get { return _displayName; }
        set { _displayName = value; }
    }

}