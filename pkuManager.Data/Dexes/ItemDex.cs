using System.Text.Json;

namespace pkuManager.Data.Dexes;

/// <summary>
/// Extension methods for reading data from the ItemDex.
/// </summary>
public static class ItemDex
{
    private static JsonElement IDR(DataDexManager ddm) => ddm.GetDexRoot("Item");

    /// <summary>
    /// Checks if <paramref name="item"/> exists in the given <paramref name="format"/>.
    /// </summary>
    /// <param name="format">The format to check.</param>
    /// <param name="item">The item to be checked.</param>
    /// <returns>Whether <paramref name="item"/> exists in <paramref name="format"/>.</returns>
    private static bool DoesItemExist(this DataDexManager ddm, string format, string item)
        => IDR(ddm).ExistsIn(item, format);

    /// <summary>
    /// Searches the ItemDex for the <paramref name="ID"/> of the given cannonical
    /// <paramref name="item"/> name in the given <paramref name="format"/>.
    /// </summary>
    /// <typeparam name="T">The type of the item <paramref name="ID"/> in <paramref name="format"/>.</typeparam>
    /// <inheritdoc cref="DoesItemExist(DataDexManager, string, string)" path="/param[@name='format']"/>
    /// <param name="item">The cannonical item name.</param>
    /// <param name="ID">The ID of <paramref name="item"/> in <paramref name="format"/>.</param>
    /// <returns>Whether or not a valid item <paramref name="ID"/> was found.</returns>
    public static bool TryGetItemID<T>(this DataDexManager ddm, string format, string item, out T ID) where T: notnull
    {
        ID = default!; //nullability violation, but won't be null if false is returned.

        if (!DoesItemExist(ddm, format, item))
            return false;

        return IDR(ddm).TryGetIndexedValue(ddm.GetIndexChain(format), out ID, item, "Indices");
    }

    /// <summary>
    /// Searches the ItemDex for the cannonical <paramref name="item"/> name of the given 
    /// <paramref name="ID"/> in the given <paramref name="format"/>.
    /// </summary>
    /// <inheritdoc cref="TryGetItemID{T}(DataDexManager, string, string, out T)"/>
    /// <returns>Whether or not a item was found corresponding to <paramref name="ID"/>.</returns>
    public static bool TryGetItemName<T>(this DataDexManager ddm, string format, out string item, T ID) where T : notnull
    {
        if (!IDR(ddm).TryGetIndexedKey(ddm.GetIndexChain(format), out item, ID, "$x", "Indices"))
            return false; //no matching ID found in index chain
        
        return DoesItemExist(ddm, format, item); //whether the found item exists in format.
    }
}