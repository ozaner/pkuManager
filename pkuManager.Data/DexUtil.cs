using System.Text.Json;

namespace pkuManager.Data;

internal static class DexUtil
{
    /// <summary>
    /// Reads a value by starting at <paramref name="root"/> and traversing the JSON tree using the
    /// <paramref name="keys"/> in sequential order.<br/> Fails if any of the <paramref name="keys"/>
    /// are invalid, the value is <see langword="null"/>, or the value cannot be casted to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The A non-nullable type that value will be casted to.</typeparam>
    /// <param name="root">The node to start traversing <paramref name="keys"/> from.</param>
    /// <param name="value">If the search was a success, the value being searched for.
    ///     <br/>Otherwise, <see langword="default"/>. Cannot be <see langword="null"/>
    ///     if <see langword="true"/> is returned.</param>
    /// <param name="keys">The keys of the parent objects of the desired element.
    ///     <br/>Note that null keys always fail.</param>
    /// <returns>Whether the <paramref name="value"/> was found.</returns>
    internal static bool TryGetValue<T>(this JsonElement root, out T value, params string[] keys) where T : notnull
    {
        value = default!; //shouldn't use this value if false returned anyway...

        var currentNode = root;
        foreach (string key in keys)
            if (key is null || !currentNode.TryGetProperty(key, out currentNode))
                return false; //invalid keys

        if (currentNode.ValueKind is JsonValueKind.Null)
            return false;

        try
        {
            value = currentNode.Deserialize<T>()!; //cannot be null because of previous check
            return true;
        }
        catch { return false; } //type mismatch, return false
    }

    /// <summary>
    /// Searches for the key that, when substituted in for the "$<paramref name="x"/>"
    /// element in <paramref name="keys"/>, points to <paramref name="value"/> in <paramref name="root"/>.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
    /// <param name="root">The node to start traversing <paramref name="keys"/> from.</param>
    /// <param name="x">A string that will be set to the key being searched for, if successful.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="keys">The list of keys to traverse starting at the <paramref name="root"/>.
    ///     <br/>Must contain <b>exactly one</b> "$x" element.</param>
    /// <returns>Whether the key, i.e. $<paramref name="x"/>, was successfully found.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="keys"/>
    ///     does not contain exactly one "$x" element.</exception>
    internal static bool TryGetKey<T>(this JsonElement root, out string x, T value, params string[] keys) where T : notnull
    {
        bool match(JsonElement fhNode, T value, string[] secondHalf)
            => TryGetValue(fhNode, out T valueToMatch, secondHalf) //value found
                && valueToMatch.Equals(value); //values match
        return TryGetKeyBase(root, out x, value, keys, match);
    }

    internal static bool ExistsIn(this JsonElement root, string value, string format)
        => root.TryGetValue(out string[] formats, value, "Exists in") //has exists in array
            && formats.Contains(format); //format is in that array

    //assumes indexed property is last key
    internal static bool TryGetIndexedValue<T>(this JsonElement root, List<string> indexNames, out T value, params string[] keys) where T : notnull
    {
        value = default!;

        if (!root.TryGetValue(out JsonElement indexedRoot, keys))
            return false; //keys don't even point anywhere

        //Try each index, in order
        foreach (string index in indexNames)
            if (indexedRoot.TryGetValue(out value, index))
                return true; //index found
        
        return false; //no index found
    }

    //assumes indexed property is last key
    internal static bool TryGetIndexedKey<T>(this JsonElement root, List<string> indexNames, out string x,
        T value, params string[] keys) where T : notnull
    {
        bool match(JsonElement fhNode, T value, string[] secondHalf)
            => TryGetIndexedValue(fhNode, indexNames, out T valueToMatch, secondHalf) //value found
                && valueToMatch.Equals(value); //values match

        return TryGetKeyBase(root, out x, value, keys, match);
    }


    /* ------------------------------------
     * Base Case Methods
     * ------------------------------------
    */
    private static bool TryGetKeyBase<T>(this JsonElement root, out string x, T value, string[] keys,
        Func<JsonElement, T, string[], bool> matchFunc) where T : notnull
    {
        x = null!; //forcing a null here, but you shouldn't use x if false was returned...

        //Check that there is exactly one $x
        if (keys.Count(key => key is "$x") is not 1)
            throw new ArgumentException($"{nameof(keys)} must contain exactly one \"$x\" element.");

        //Partition keys on $x
        int splitIndex = Array.IndexOf(keys, "$x");
        string[] firstHalf = keys.Take(splitIndex).ToArray();
        string[] secondHalf = keys.Skip(splitIndex + 1).Prepend("").ToArray();

        //traverse first half, and make sure it is an object
        if (!TryGetValue(root, out JsonElement fhNode, firstHalf)
            || fhNode.ValueKind is not JsonValueKind.Object)
            return false; //invalid keys/not an object

        //traverse second half for each possible $x until a match is found
        foreach (var property in fhNode.EnumerateObject())
        {
            secondHalf[0] = property.Name;
            if (matchFunc(fhNode, value, secondHalf))
            {
                x = property.Name;
                return true;
            }
        }
        return false; //no match found
    }
}
