using System.Text.Json;

namespace pkuManager.Data.Dexes;

/// <summary>
/// Extension methods for reading data from the MoveDex.
/// </summary>
public static class MoveDex
{
    private static JsonElement MDR(DataDexManager ddm) => ddm.GetDexRoot("Move");

    /// <summary>
    /// Checks if <paramref name="move"/> exists in the given <paramref name="format"/>.
    /// </summary>
    /// <param name="format">The format to check.</param>
    /// <param name="move">The move to be checked.</param>
    /// <returns>Whether <paramref name="move"/> exists in <paramref name="format"/>.</returns>
    private static bool DoesMoveExist(this DataDexManager ddm, string format, string move)
        => MDR(ddm).ExistsIn(move, format);

    /// <summary>
    /// Searches the MoveDex for the <paramref name="ID"/> of the given cannonical
    /// <paramref name="move"/> name in the given <paramref name="format"/>.
    /// </summary>
    /// <typeparam name="T">The type of the move <paramref name="ID"/> in <paramref name="format"/>.</typeparam>
    /// <inheritdoc cref="DoesMoveExist(DataDexManager, string, string)" path="/param[@name='format']"/>
    /// <param name="move">The cannonical move name.</param>
    /// <param name="ID">The ID of <paramref name="move"/> in <paramref name="format"/>.</param>
    /// <returns>Whether or not a valid move <paramref name="ID"/> was found.</returns>
    public static bool TryGetMoveID<T>(this DataDexManager ddm, string format, string move, out T ID) where T: notnull
    {
        ID = default!; //nullability violation, but won't be null if false is returned.

        if (!DoesMoveExist(ddm, format, move))
            return false;

        return MDR(ddm).TryGetIndexedValue(ddm.GetIndexChain(format), out ID, move, "Indices");
    }

    /// <summary>
    /// Searches the MoveDex for the cannonical <paramref name="move"/> name of the given 
    /// <paramref name="ID"/> in the given <paramref name="format"/>.
    /// </summary>
    /// <inheritdoc cref="TryGetMoveID{T}(DataDexManager, string, string, out T)"/>
    /// <returns>Whether or not a move was found corresponding to <paramref name="ID"/>.</returns>
    public static bool TryGetMoveName<T>(this DataDexManager ddm, string format, out string move, T ID) where T : notnull
    {
        if (!MDR(ddm).TryGetIndexedKey(ddm.GetIndexChain(format), out move, ID, "$x", "Indices"))
            return false; //no matching ID found in index chain
        
        return DoesMoveExist(ddm, format, move); //whether the found move exists in format.
    }

    /// <summary>
    /// Searches for the <paramref name="basePP"/> of <paramref name="move"/> in <paramref name="format"/>.
    /// </summary>
    /// <inheritdoc cref="DoesMoveExist(DataDexManager, string, string)" path="/param[@name='format']"/>
    /// <inheritdoc cref="TryGetMoveID{T}(DataDexManager, string, string, out T)" path="/param[@name='move']"/>
    /// <param name="basePP">The base pp of <paramref name="move"/>, or 0 if it wasn't found.</param>
    /// <returns>Whether or not the base PP was found.</returns>
    public static bool TryGetMovePP(this DataDexManager ddm, string format, string move, out int basePP)
    {
        basePP = default;

        if (!DoesMoveExist(ddm, format, move))
            return false;

        return MDR(ddm).TryGetIndexedValue(ddm.GetIndexChain(format), out basePP, move, "Base PP");
    }
}