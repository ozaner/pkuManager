using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields;

public interface IBoundable
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }
}

public interface IIntField : IField<BigInteger>, IBoundable { }
public interface IIntArrayField : IField<BigInteger[]>, IBoundable { }

public static class IntFieldExtensions
{
    public static void SetAs(this IIntArrayField field, BigInteger val, int i)
    {
        BigInteger[] vals = field.Value;
        vals[i] = val;
        field.Value = vals;
    }

    // Integral Cast Accessors
    public static T GetAs<T>(this IIntField field) where T : struct
        => field.Value.BigIntegerTo<T>();

    public static void SetAs<T>(this IIntField field, T val) where T : struct
        => field.Value = val.ToBigInteger();

    // IntegralArray Cast Accessors
    public static T[] GetAs<T>(this IIntArrayField field) where T : struct
        => Array.ConvertAll(field.Value, x => x.BigIntegerTo<T>());

    public static void SetAs<T>(this IIntArrayField field, T[] vals) where T : struct
        => field.Value = Array.ConvertAll(vals, x => x.ToBigInteger());

    // Integral Cast Accessors
    public static T GetAs<T>(this IIntArrayField field, int i) where T : struct
        => field.Value[i].BigIntegerTo<T>();

    public static void SetAs<T>(this IIntArrayField field, T val, int i) where T : struct
        => field.SetAs(val.ToBigInteger(), i);
}