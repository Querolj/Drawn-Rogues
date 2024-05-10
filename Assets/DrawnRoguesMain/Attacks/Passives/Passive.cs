using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum OperationTypeEnum
{
    Add,
    Substract,
    AddPercentage,
    PercentageResistance,
    Set,
}

public class Passive : ScriptableObject
{
    public float MaxValue = float.MaxValue;
    public float MinValue = float.MinValue;

    [TextArea (3, 10), InfoBox ("Can contain {value} to display the value, and {sign} to display + or - depending on the value.")]
    public string Description;
    public bool InverseDisplayedSignInDescription;
    public OperationTypeEnum OperationType;
    protected float Value;

    public void SetValue (float value)
    {
        Value = Mathf.Clamp (value, MinValue, MaxValue);
    }

    public void Add (float value)
    {
        SetValue (Value + value);
    }

    public override string ToString ()
    {
        string descriptionWithValue = Description.Replace ("{value}", Mathf.Abs (Value).ToString ());
        if (InverseDisplayedSignInDescription)
            descriptionWithValue = descriptionWithValue.Replace ("{sign}", Value < 0 ? "+" : "-");
        else
            descriptionWithValue = descriptionWithValue.Replace ("{sign}", Value >= 0 ? "+" : "-");
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