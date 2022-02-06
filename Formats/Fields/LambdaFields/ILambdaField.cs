using System;

namespace pkuManager.Formats.Fields.LambdaFields;

public interface ILambdaField<T>
{
    public Func<T> LambdaGet { get; }
    public Action<T> LambdaSet { get; }
}