using System;
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

    public string Name;
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
        return Name + " " + Value;
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
        return Passive.Name + " " + Value;
    }
}