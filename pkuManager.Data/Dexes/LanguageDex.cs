using System.Text.Json;

namespace pkuManager.Data.Dexes;

/// <summary>
/// Extension methods for reading data from the LanguageDex.
/// </summary>
public static class LanguageDex
{
    private static JsonElement LDR(DataDexManager ddm) => ddm.GetDexRoot("Language");

    /// <summary>
    /// Checks if <paramref name="langauge"/> exists in the given <paramref name="format"/>.
    /// </summary>
    /// <param name="format">The format to check.</param>
    /// <param name="langauge">The langauge to be checked.</param>
    /// <returns>Whether <paramref name="langauge"/> exists in <paramref name="format"/>.</returns>
    private static bool DoesLanguageExist(this DataDexManager ddm, string format, string langauge)
        => LDR(ddm).ExistsIn(langauge, format);

    /// <summary>
    /// Searches the LanguageDex for the <paramref name="ID"/> of the given cannonical
    /// <paramref name="langauge"/> name in the given <paramref name="format"/>.
    /// </summary>
    /// <typeparam name="T">The type of the langauge <paramref name="ID"/> in <paramref name="format"/>.</typeparam>
    /// <inheritdoc cref="DoesLanguageExist(DataDexManager, string, string)" path="/param[@name='format']"/>
    /// <param name="langauge">The cannonical langauge name.</param>
    /// <param name="ID">The ID of <paramref name="langauge"/> in <paramref name="format"/>.</param>
    /// <returns>Whether or not a valid langauge <paramref name="ID"/> was found.</returns>
    public static bool TryGetLanguageID<T>(this DataDexManager ddm, string format, string langauge, out T ID) where T: notnull
    {
        ID = default!; //nullability violation, but won't be null if false is returned.

        if (!DoesLanguageExist(ddm, format, langauge))
            return false;

        return LDR(ddm).TryGetIndexedValue(ddm.GetIndexChain(format), out ID, langauge, "Indices");
    }

    /// <summary>
    /// Searches the LanguageDex for the cannonical <paramref name="langauge"/> name of the given 
    /// <paramref name="ID"/> in the given <paramref name="format"/>.
    /// </summary>
    /// <inheritdoc cref="TryGetLanguageID{T}(DataDexManager, string, string, out T)"/>
    /// <returns>Whether or not a langauge was found corresponding to <paramref name="ID"/>.</returns>
    public static bool TryGetLanguageName<T>(this DataDexManager ddm, string format, out string langauge, T ID) where T : notnull
    {
        if (!LDR(ddm).TryGetIndexedKey(ddm.GetIndexChain(format), out langauge, ID, "$x", "Indices"))
            return false; //no matching ID found in index chain
        
        return DoesLanguageExist(ddm, format, langauge); //whether the found langauge exists in format.
    }

    /// <summary>
    /// Searches the LanguageDex for all languages that exist in the given <paramref name="format"/>.
    /// </summary>
    /// <inheritdoc cref="DoesLanguageExist(DataDexManager, string, string)" path="/param[@name='format']"/>
    /// <returns>A list of all languages in <paramref name="format"/>.</returns>
    public static List<string> GetAllLanguages(this DataDexManager ddm, string format)
    {
        List<string> finalList = new();
        foreach (var prop in LDR(ddm).EnumerateObject())
        {
            if (ddm.DoesLanguageExist(format, prop.Name))
                finalList.Add(prop.Name);
        }
        return finalList;
    }
}