using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum OperationTypeEnum
{
    Add,
    AddPercentage,
    PercentageResistance,
    Set,
}

public class Passive : ScriptableObject
{
    [SerializeField]
    private float _maxValue = float.MaxValue;
    public float MaxValue => _maxValue;

    [SerializeField]
    private float _minValue = float.MinValue;
    public float MinValue => _minValue;

    [SerializeField, TextArea (3, 10), InfoBox ("Can contain {value} to display the value, and {sign} to display + or - depending on the value.")]
    private string _description;
    public string Description => _description;

    [SerializeField]
    private bool _inverseDisplayedSignInDescription;
    public bool InverseDisplayedSignInDescription => _inverseDisplayedSignInDescription;

    [SerializeField]
    private OperationTypeEnum _operationType;
    public OperationTypeEnum OperationType => _operationType;

    protected float _value;

    public void SetValue (float value)
    {
        _value = Mathf.Clamp (value, MinValue, MaxValue);
    }

    public void Add (float value)
    {
        SetValue (_value + value);
    }

    protected void AlterPropertyValue (ref float propertyValue)
    {
        switch (OperationType)
        {
            case OperationTypeEnum.Add:
                propertyValue += _value;
                break;
            case OperationTypeEnum.AddPercentage:
                propertyValue += propertyValue * _value;
                break;
            case OperationTypeEnum.Set:
                propertyValue = _value;
                break;
            case OperationTypeEnum.PercentageResistance:
                propertyValue = propertyValue * (1f - _value);
                break;
        }
    }

    public override string ToString ()
    {
        string descriptionWithValue = Description.Replace ("{value}", Mathf.Abs (_value * 100f).ToString ());
        if (InverseDisplayedSignInDescription)
            descriptionWithValue = descriptionWithValue.Replace ("{sign}", _value < 0 ? "+" : "-");
        else
            descriptionWithValue = descriptionWithValue.Replace ("{sign}", _value >= 0 ? "+" : "-");
        return descriptionWithValue;
    }

    public string ToString (float value)
    {
        string descriptionWithValue = Description.Replace ("{value}", Mathf.Abs (value * 100f).ToString ());
        if (InverseDisplayedSignInDescription)
            descriptionWithValue = descriptionWithValue.Replace ("{sign}", value < 0 ? "+" : "-");
        else
            descriptionWithValue = descriptionWithValue.Replace ("{sign}", value >= 0 ? "+" : "-");
        return descriptionWithValue;
    }
}

[Serializable]
public class PassiveSerialized<T> where T : Passive
{
    public T Passive;
    public float Value;

    public T GetInstance ()
    {
        T passive = ScriptableObject.Instantiate (Passive);
        passive.SetValue (Value);
        return passive;
    }

    public override string ToString ()
    {
        return Passive.Description + " " + Value;
    }
}