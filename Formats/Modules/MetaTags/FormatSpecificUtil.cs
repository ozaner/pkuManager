using Newtonsoft.Json.Linq;
using pkuManager.Formats.pku;
using System.Collections.Generic;

namespace pkuManager.Formats.Modules.MetaTags;

public static class FormatSpecificUtil
{
    public static (T value, bool invalid) GetValue<T>(pkuObject pku, string formatName, params string[] keys)
    {
        pku.Format_Specific.TryGetValue(formatName, out var formatDict);
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
}
