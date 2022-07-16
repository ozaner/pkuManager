using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Utilities;

namespace pkuManager.WinForms.Formats.Modules.MetaTags;

public static class FormatSpecificUtil
{
    public static (T value, bool invalid) GetValue<T>(pkuObject pku, string formatName, params string[] keys)
    {
        pkuObject.Format_Dict formatDict = GetFormatDict(pku, formatName);
        if (formatDict == null || keys.Length < 1)
            return (default, false); //no dict/no keys

        return DataUtil.TraverseJSONDict<T>(formatDict.ExtraTags, keys);
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