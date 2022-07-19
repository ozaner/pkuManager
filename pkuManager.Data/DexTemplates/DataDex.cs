using System.Text.Json;

namespace pkuManager.Data.DexTemplates;

/// <summary>
/// Represents an abstract DataDex.
/// </summary>
public abstract class DataDex
{
    /// <summary>
    /// The root of the DataDex, i.e. the entire JSON object.
    /// </summary>
    protected JsonElement Root { get; }

    /// <summary>
    /// Constructs a <see cref="DataDex"/> from the given <paramref name="jsonDoc"/>.
    /// </summary>
    /// <param name="jsonDoc">The DataDex as a JSON document.</param>
    public DataDex(JsonDocument jsonDoc) => Root = jsonDoc.RootElement;

    ///<inheritdoc cref="TryGetValueBase{T}(out T, string[])"/>
    protected bool TryGetValue<T>(out T? value, params string[] keys) where T: struct
        => TryGetValueBase(out value, keys);

    ///<inheritdoc cref="TryGetValueBase{T}(out T, string[])"/>
    protected bool TryGetValue<T>(out T? value, params string[] keys) where T : class
        => TryGetValueBase(out value, keys);

    /// <summary>
    /// Reads a value by starting at <see cref="Root"/> and traversing the JSON tree using the
    /// <paramref name="keys"/> in sequential order.<br/> Fails if any of the <paramref name="keys"/>
    /// are invalid, or the value cannot be casted to <typeparamref name="T"/>?
    /// </summary>
    /// <typeparam name="T">The A non-nullable form of the type value will be casted to.</typeparam>
    /// <param name="value">If the search was a success, the value being searched for.
    /// <br/>Otherwise, <see langword="default"/>.</param>
    /// <param name="keys">The keys of the parent objects of the desired element.
    ///     <br/>Note that null keys always fail.</param>
    /// <returns>Whether the <paramref name="value"/> was found.</returns>
    private bool TryGetValueBase<T>(out T? value, params string[] keys)
    {
        value = default;

        var currentNode = Root;
        foreach (string key in keys)
            if (!currentNode.TryGetProperty(key, out currentNode))
                return false; //invalid keys

        try {
            value = currentNode.Deserialize<T>();
            return true;
        }
        catch { return false; } //type mismatch, return false
    }

    /// <summary>
    /// Searches for the key that, when substituted in for the "$<paramref name="x"/>"
    /// element in <paramref name="keys"/>, points to <paramref name="value"/> in the DataDex.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
    /// <param name="x"></param>
    /// <param name="value">The value to search for.</param>
    /// <param name="keys">The list of keys to traverse starting at the <see cref="Root"/>.<br/>
    ///     Must contain <b>exactly one</b> "$x" element.</param>
    /// <returns>Whether the key, i.e. $<paramref name="x"/>, was successfully found.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="keys"/>
    ///     does not contain exactly one "$x" element.</exception>
    protected bool TryGetKey<T>(out string x, T value, params string[] keys)
    {
        x = null!; //forcing a null here, but you shouldn't use x if false was returned...
        
        //Check that there is exactly one $x
        if (keys.Count(y => y == "$x") is not 1)
            throw new ArgumentException($"{nameof(keys)} must contain exactly one \"$x\" element.");

        //Partition keys on $x
        int splitIndex = Array.IndexOf(keys, "$x");
        string[] firstHalf = keys.Take(splitIndex).ToArray();
        string[] secondHalf = keys.Skip(splitIndex+1).ToArray();

        //traverse first half
        JsonElement fhNode = Root;
        foreach (string key in firstHalf)
            if (!fhNode.TryGetProperty(key, out fhNode))
                return false; //invalids keys

        //Make sure first half valid
        if (fhNode.ValueKind is not JsonValueKind.Object)
            return false; //must be an object

        foreach (var property in fhNode.EnumerateObject())
        {
            //traverse second half w/ $x = property.Name
            JsonElement shNode = fhNode.GetProperty(property.Name); //no exceptions
            foreach (string key in secondHalf)
                if (!shNode.TryGetProperty(key, out shNode))
                    continue; //invalid branch

            //check for match
            bool match = false;
            if (value is null && shNode.ValueKind is JsonValueKind.Null) //null case
                match = true;
            else
            {
                try { match = value!.Equals(shNode.Deserialize<T>()); }
                catch { } //shNode/value type mismatch
            }

            //$x found
            if (match)
            {
                x = property.Name;
                return true;
            }
        }
        return false; //no match found
    }
}