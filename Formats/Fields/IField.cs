using Newtonsoft.Json;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace pkuManager.Formats.Fields;

public interface IField<T>
{
    public T Value { get; set; }
}

public static class FieldExtensions
{
    public static bool IsNull<T>(this IField<T> field)
        => field.Value is null;

    // Integral Cast Accessors
    public static T GetAs<T>(this IIntegralField field) where T : struct
        => field.Value.BigIntegerTo<T>();

    public static void SetAs<T>(this IIntegralField field, T val) where T : struct
        => field.Value = val.ToBigInteger();

    // IntegralArray Cast Accessors
    public static T[] GetAs<T>(this IIntegralArrayField field) where T : struct
        => Array.ConvertAll(field.Value, x => x.BigIntegerTo<T>());

    public static void SetAs<T>(this IIntegralArrayField field, T[] vals) where T : struct
        => field.Value = Array.ConvertAll(vals, x => x.ToBigInteger());

    // Integral Cast Accessors
    public static T GetAs<T>(this IIntegralArrayField field, int i) where T : struct
        => field.Value[i].BigIntegerTo<T>();

    public static void SetAs<T>(this IIntegralArrayField field, T val, int i) where T : struct
        => field.SetAs(val.ToBigInteger(), i);


    public static void SetAs(this IIntegralArrayField field, BigInteger val, int i)
    {
        BigInteger[] vals = field.Value;
        vals[i] = val;
        field.Value = vals;
    }
}

public class IFieldJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
        => objectType.ImplementsGenericInterface(typeof(IField<>));

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        //Get the value the field is encapsulating
        object value;

        //null JSON token results in a IField<T> with default val.
        if (reader.TokenType is not JsonToken.StartArray && reader.Value is null)
            value = null;
        //handle arrays
        else if (reader.TokenType is JsonToken.StartArray)
        {
            List<object> temp = new();
            reader.Read();
            while (reader.TokenType is not JsonToken.EndArray)
            {
                temp.Add(reader.Value);
                reader.Read();
            }
            object tempArray = temp.ToArray();
            Type arrType = objectType.GetGenericArguments()[0];

            if (arrType == typeof(BigInteger?)) // integer? arrays need to be converted
                tempArray = Array.ConvertAll(tempArray as object[], x => x is null ? (BigInteger?)null : (x as ValueType).ToBigInteger());
            else if (arrType == typeof(BigInteger)) // integer arrays need to be converted
                tempArray = Array.ConvertAll(tempArray as object[], x => (x as ValueType).ToBigInteger());

            Array destinationArray = Array.CreateInstance(arrType, temp.Count);
            Array.Copy(tempArray as Array, destinationArray, temp.Count);
            value = destinationArray.Length is 0 ? null : destinationArray;
        }
        //handle (non bigint) integers
        else if (reader.TokenType is JsonToken.Integer && (reader.Value.GetType() != typeof(BigInteger?) ||
                                                           reader.Value.GetType() != typeof(BigInteger)))
            value = (reader.Value as ValueType).ToBigInteger(); //wont be null, already checked
        //everything else
        else
            value = reader.Value;

        //create field (only works for fields with empty constructor, i.e. backingfields)
        var obj = Activator.CreateInstance(objectType, Array.Empty<object>());

        //invokes IField.Value setter
        objectType.GetProperties().Where(x => x.Name is "Value").First()
            .GetSetMethod().Invoke(obj, new object[] { value });

        return obj;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Type type = value.GetType();

        // Get the value the field encapsulates.
        var backedValue = type.GetProperties().Where(x => x.Name is "Value").First()
            .GetGetMethod().Invoke(value, Array.Empty<object>());

        // null backed values are treated as null.
        if (backedValue is null)
        {
            writer.WriteNull();
            return;
        }

        // decides which kinds of values get indented or one-lined.
        bool dontIndent = typeof(IField<BigInteger?[]>).IsAssignableFrom(type) || //all integer arrays
                          typeof(IField<BigInteger[]>).IsAssignableFrom(type) ||
                          typeof(IField<string[]>).IsAssignableFrom(type) && (backedValue as Array)?.Length < 4; //string arrays under 4

        // writes the value with the proper indenting.
        writer.WriteRawValue(JsonConvert.SerializeObject(backedValue, dontIndent ? Formatting.None : Formatting.Indented));
    }
}