using System;
using Sirenix.OdinInspector;
using UnityEngine;

// Order of enum = order of operation
public enum OperationTypeEnum
{
    Add,
    AddPercentage,
    Set,
    PercentageResistance,
}

public class Passive : ScriptableObject
{
    [SerializeField, TextArea (3, 10), InfoBox ("Can contain <b>{value}</b> to display the value, and <b>{sign}</b> to display + or - depending on the value."), BoxGroup ("Description"), HideLabel]
    private string _description;
    public string Description => _description;

    [SerializeField, BoxGroup ("Description"), InfoBox ("If true, the sign displayed will be the opposite of the value's sign.")]
    private bool _inverseDisplayedSignInDescription;
    public bool InverseDisplayedSignInDescription => _inverseDisplayedSignInDescription;

    [SerializeField, BoxGroup ("Value Settings")]
    private float _maxValue = float.MaxValue;
    public float MaxValue => _maxValue;

    [SerializeField, BoxGroup ("Value Settings")]
    private float _minValue = float.MinValue;
    public float MinValue => _minValue;

    [SerializeField, BoxGroup ("Value Settings")]
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
        float mult = OperationType == OperationTypeEnum.AddPercentage || OperationType == OperationTypeEnum.PercentageResistance ?  100f : 1f;
        string descriptionWithValue = Description.Replace ("{value}", Mathf.Abs (_value * mult).ToString ());
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