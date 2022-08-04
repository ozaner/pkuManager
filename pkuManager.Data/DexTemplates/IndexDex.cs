using pkuManager.Data.Dexes;
using System.Text.Json;

namespace pkuManager.Data.DexTemplates;

public class IndexDex : DataDex
{
    public FormatDex FormatDex { get; }

    public IndexDex(JsonDocument jsonDoc, FormatDex formatDex) : base(jsonDoc)
        => FormatDex = formatDex;

    public bool ExistsIn(string value, string format)
        => TryGetValue(Root, out string[] formats, value, "Exists in") //has exists in array
            && formats.Contains(format); //format is in that array

    //assumes indexed property is last key
    private bool TryGetIndexedValueBase<T>(string format, out T index, string value, params string[] keys) where T: notnull
    {
        index = default!;

        //Check if value even exists in format
        if (!ExistsIn(value, format))
            return false;

        var indexChain = FormatDex.GetIndexChain(format);
        string[] keysWithLink = keys.Append("").ToArray();
        foreach (var link in indexChain)
        {
            keysWithLink[^1] = link;
            if (TryGetValue(Root, out index, keysWithLink))
                return true; //index found
        }
        return false; //no index found
    }

    //assumes indexed property is last key
    protected bool TryGetIndexedKey<T>(string format, out string x, T value, params string[] keys) where T: notnull
    {
        x = null!; //forcing a null here, but you shouldn't use x if false was returned...

        var indexChain = FormatDex.GetIndexChain(format);
        string[] keysWithLink = keys.Append("").ToArray();
        foreach (var link in indexChain)
        {
            keysWithLink[^1] = link;
            if (TryGetKey(Root, out x, value, keysWithLink))
                return true; //index found
        }
        return false; //no index found
    }
}