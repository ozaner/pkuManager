using Newtonsoft.Json;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace pkuManager.Formats.Fields;

public abstract class Field<T>
{
    protected abstract T Value { get; set; }

    protected Func<T, T> CustomGetter;

    protected Func<T, T> CustomSetter;

    protected Field(Func<T, T> getter, Func<T, T> setter)
    {
        CustomGetter = getter ?? (x => x);
        CustomSetter = setter ?? (x => x);
    }

    public static implicit operator T(Field<T> f) => f.Get();

    public T Get() => CustomGetter(Value);

    public void Set(T val) => Value = CustomSetter(val);
}

public class FieldJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
        => objectType.IsSubclassOfGeneric(typeof(Field<>));

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Get the value the Field is encapsulating
        object value;

        // null JSON token results in a Field<T> with default val.
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

        // create Field (only works for fields with matching constructor, i.e. backingfields)
        var obj = Activator.CreateInstance(objectType, new object[] { null, null });

        // invokes Field.Set(value)
        objectType.GetMethods().Where(x => x.Name is "Set" && !x.IsGenericMethod && x.GetParameters().Length is 1)
                  .First().Invoke(obj, new object[] { value });

        return obj;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Type type = value.GetType();

        // Get the value the Field encapsulates.
        var backedValue = type.GetMethods()
                              .Where(x => x.Name is "Get" && !x.IsGenericMethod && x.GetParameters().Length is 0)
                              .First().Invoke(value, Array.Empty<object>());

        // null backed values are treated as null.
        if (backedValue is null)
        {
            writer.WriteNull();
            return;
        }

        // decides which kinds of values get indented or one-lined.
        bool dontIndent = type.IsSubclassOf(typeof(Field<BigInteger?[]>)) || //all integer arrays
                          type.IsSubclassOf(typeof(Field<BigInteger[]>)) ||
                          type.IsSubclassOf(typeof(Field<string[]>)) && (backedValue as Array)?.Length < 4; //string arrays under 4
        
        // writes the value with the proper indenting.
        writer.WriteRawValue(JsonConvert.SerializeObject(backedValue, dontIndent ? Formatting.None : Formatting.Indented));
    }
}