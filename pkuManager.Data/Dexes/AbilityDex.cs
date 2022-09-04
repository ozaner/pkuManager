using System.Text.Json;
namespace pkuManager.Data.Dexes;

/// <summary>
/// Extension methods for reading data from the AbilityDex.
/// </summary>
public static class AbilityDex
{
    private static JsonElement ADR(DataDexManager ddm) => ddm.GetDexRoot("Ability");

    /// <summary>
    /// Checks if <paramref name="ability"/> exists in the given <paramref name="format"/>.
    /// </summary>
    /// <param name="format">The format to check.</param>
    /// <param name="ability">The ability to be checked.</param>
    /// <returns>Whether <paramref name="ability"/> exists in <paramref name="format"/>.</returns>
    public static bool DoesAbilityExist(this DataDexManager ddm, string format, string ability)
        => ADR(ddm).ExistsIn(ability, format);

    /// <summary>
    /// Searches the AbilityDex for the <paramref name="ID"/> of the given cannonical
    /// <paramref name="ability"/> name in the given <paramref name="format"/>.
    /// </summary>
    /// <typeparam name="T">The type of the ability <paramref name="ID"/> in <paramref name="format"/>.</typeparam>
    /// <inheritdoc cref="DoesAbilityExist(DataDexManager, string, string)" path="/param[@name='format']"/>
    /// <param name="ability">The cannonical ability name.</param>
    /// <param name="ID">The ID of <paramref name="ability"/> in <paramref name="format"/>.</param>
    /// <returns>Whether or not a valid ability <paramref name="ID"/> was found.</returns>
    public static bool TryGetAbilityID<T>(this DataDexManager ddm, string format, string ability, out T ID) where T: notnull
    {
        ID = default!; //nullability violation, but won't be null if false is returned.

        if (!DoesAbilityExist(ddm, format, ability))
            return false;

        return ADR(ddm).TryGetIndexedValue(ddm.GetIndexChain(format), out ID, ability, "Indices");
    }

    /// <summary>
    /// Searches the AbilityDex for the cannonical <paramref name="ability"/> name of the given 
    /// <paramref name="ID"/> in the given <paramref name="format"/>.
    /// </summary>
    /// <inheritdoc cref="TryGetAbilityID{T}(DataDexManager, string, string, out T)"/>
    /// <returns>Whether or not a ability was found corresponding to <paramref name="ID"/>.</returns>
    public static bool TryGetAbilityName<T>(this DataDexManager ddm, string format, out string ability, T ID) where T : notnull
    {
        if (!ADR(ddm).TryGetIndexedKey(ddm.GetIndexChain(format), out ability, ID, "$x", "Indices"))
            return false; //no matching ID found in index chain
        
        return DoesAbilityExist(ddm, format, ability); //whether the found ability exists in format.
    }
}