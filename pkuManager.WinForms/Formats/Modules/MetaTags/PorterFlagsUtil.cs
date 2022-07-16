using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Utilities;

namespace pkuManager.WinForms.Formats.Modules.MetaTags;

public static class PorterFlagsUtil
{
    /// <summary>
    /// Tries to return the specified porter flag. Tries the flag in "temp"
    /// first then falls back on the root of "Porter Flags".
    /// </summary>
    /// <typeparam name="T">The data type the porter flag should be.</typeparam>
    /// <param name="pku">The pku file.</param>
    /// <param name="keys">The location of the porter flag.</param>
    /// <returns>The value of the porter flag, if existant, and whether
    /// the value was invalid or not (i.e. exists and is of right type).</returns>
    public static (T val, bool invalid) GetFlag<T>(pkuObject pku, params string[] keys)
    {
        //try temp
        (T val, bool invalid) = DataUtil.TraverseJSONDict<T>(pku.Porter_Flags.Temp_Flags, keys);
        if (invalid) //if temp didn't work, try normal
            return DataUtil.TraverseJSONDict<T>(pku.Porter_Flags.ExtraTags, keys);
        else //temp worked, return
            return (val, invalid);
    }

    /// <summary>
    /// Clears "Porter Flags"/"temp" in the pku file.
    /// </summary>
    public static void ClearTempFlags(this pkuObject pku)
        => pku.Porter_Flags.Temp_Flags.Clear();
}
