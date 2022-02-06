using System;

namespace pkuManager.Formats.Fields.LambdaFields;

public class LambdaField<T> : Field<T>, ILambdaField<T>
{
    protected override T Value { get => LambdaGet(); set => LambdaSet(value); }
    public Func<T> LambdaGet { get; }
    public Action<T> LambdaSet { get; }

    public LambdaField(Func<T> get, Action<T> set, Func<T, T> getter = null, Func<T, T> setter = null)
        : base(getter, setter)
    {
        LambdaGet = get;
        LambdaSet = set;
    }
}