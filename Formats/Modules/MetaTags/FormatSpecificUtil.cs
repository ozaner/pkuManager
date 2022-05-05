using Newtonsoft.Json.Linq;
using pkuManager.Formats.pku;
using System.Collections.Generic;

namespace pkuManager.Formats.Modules.MetaTags;

public static class FormatSpecificUtil
{
    public static (T value, bool invalid) GetValue<T>(pkuObject pku, string formatName, params string[] keys)
    {
        pkuObject.Format_Dict formatDict = GetFormatDict(pku, formatName);
        if (formatDict == null || keys.Length < 1)
            return (default, false); //no dict/no keys

        //traverse sub-tags
        IDictionary<string, JToken> dict = formatDict.ExtraTags;
        foreach (var key in keys[..^1])
        {
            dict.TryGetValue(key, out var temp);
            if (temp is null)
                return (default, false);
            else if (temp is IDictionary<string, JToken> newDict)
                dict = newDict;
            else
                return (default, true); //some sub-dict was not a dict (invalid).
        }

        // try get value
        if (!dict.ContainsKey(keys[^1]))
            return (default, false);

        dict.TryGetValue(keys[^1], out var value);
        try { return (value.ToObject<T>(), false); } //return whatever the value is
        catch { return (default, true); } //can't return the value, doesn't match the type (invalid)
    }

    public static pkuObject.Format_Dict GetFormatDict(pkuObject pku, string formatName)
    {
        pku.Format_Specific.TryGetValue(formatName, out var formatDict);
        return formatDict;
    }

    public static void EnsureFormatDictExists(pkuObject pku, string formatName)
    {
        if (!pku.Format_Specific.ContainsKey(formatName) || pku.Format_Specific[formatName] is null)
            pku.Format_Specific[formatName] = new();
    }
}
