using System;

namespace pkuManager.WinForms.Formats.Fields.LambdaFields;

public class LambdaField<T> : IField<T>
{
    protected bool IsWrapped { get; }

    //No wrap parameters
    protected Func<T> LambdaGet { get; }
    protected Action<T> LambdaSet { get; }
    
    //wrap parameters
    protected IField<T> WrappedField { get; }
    protected Func<T, T> LambdaGetModifier { get; }
    protected Func<T, T> LambdaSetModifier { get; }

    public T Value
    {
        get => IsWrapped ? LambdaGetModifier(WrappedField.Value) : LambdaGet();
        set
        {
            if (IsWrapped)
                WrappedField.Value = LambdaSetModifier(value);
            else
                LambdaSet(value);
        }
    }

    public LambdaField(Func<T> get, Action<T> set)
    {
        IsWrapped = false;
        LambdaGet = get;
        LambdaSet = set;
    }

    public LambdaField(IField<T> wrappedField, Func<T, T> getModifier, Func<T, T> setModifier)
    {
        IsWrapped = true;
        WrappedField = wrappedField;
        LambdaGetModifier = getModifier;
        LambdaSetModifier = setModifier;
    }
}