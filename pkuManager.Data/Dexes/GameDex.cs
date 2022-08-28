using System.Text.Json;

namespace pkuManager.Data.Dexes;

/// <summary>
/// Extension methods for reading data from the GameDex.
/// </summary>
public static class GameDex
{
    private static JsonElement GDR(DataDexManager ddm) => ddm.GetDex("Game").RootElement;

    /// <summary>
    /// Checks if <paramref name="game"/> exists in the given <paramref name="format"/>.
    /// </summary>
    /// <param name="format">The format to check.</param>
    /// <param name="game">The game to be checked.</param>
    /// <returns>Whether <paramref name="game"/> exists in <paramref name="format"/>.</returns>
    private static bool DoesGameExist(this DataDexManager ddm, string format, string game)
        => GDR(ddm).ExistsIn(game, format);

    /// <summary>
    /// Searches the GameDex for the <paramref name="ID"/> of the given cannonical
    /// <paramref name="game"/> name in the given <paramref name="format"/>.
    /// </summary>
    /// <typeparam name="T">The type of the game <paramref name="ID"/> in <paramref name="format"/>.</typeparam>
    /// <inheritdoc cref="DoesGameExist(DataDexManager, string, string)" path="/param[@name='format']"/>
    /// <param name="game">The cannonical game name.</param>
    /// <param name="ID">The ID of <paramref name="game"/> in <paramref name="format"/>.</param>
    /// <returns>Whether or not a valid game <paramref name="ID"/> was found.</returns>
    public static bool TryGetGameID<T>(this DataDexManager ddm, string format, string game, out T ID) where T: notnull
    {
        ID = default!; //nullability violation, but won't be null if false is returned.

        if (!DoesGameExist(ddm, format, game))
            return false;

        return GDR(ddm).TryGetIndexedValue(ddm.GetIndexChain(format), out ID, game, "Indices");
    }

    /// <summary>
    /// Searches the GameDex for the cannonical <paramref name="game"/> name of the given 
    /// <paramref name="ID"/> in the given <paramref name="format"/>.
    /// </summary>
    /// <inheritdoc cref="TryGetGameID{T}(DataDexManager, string, string, out T)"/>
    /// <returns>Whether or not a game was found corresponding to <paramref name="ID"/>.</returns>
    public static bool TryGetGameName<T>(this DataDexManager ddm, string format, out string game, T ID) where T : notnull
    {
        if (!GDR(ddm).TryGetIndexedKey(ddm.GetIndexChain(format), out game, ID, "$x", "Indices"))
            return false; //no matching ID found in index chain
        
        return DoesGameExist(ddm, format, game); //whether the found game exists in format.
    }

    /// <summary>
    /// Searches the GameDex for the <paramref name="ID"/> of the given
    /// <paramref name="location"/> in the given <paramref name="game"/>.
    /// </summary>
    /// <param name="game">The cannonical game name.</param>
    /// <param name="game">The cannonical location name.</param>
    /// <param name="ID">The ID of <paramref name="location"/> in <paramref name="game"/>.</param>
    /// <returns>Whether or not a valid location <paramref name="ID"/> was found.</returns>
    public static bool TryGetLocationID(this DataDexManager ddm, string game, string location, out int ID)
    {
        ID = default;

        if (GDR(ddm).TryGetKey(out string IDStr, location, game, "Locations", "$x") && int.TryParse(IDStr, out ID))
            return true; //location found, and int type index

        return false; //location not found, or non-parsable int index...
    }

    /// <summary>
    /// Searches the GameDex for the cannonical <paramref name="location"/> name of the given 
    /// <paramref name="ID"/> in the given <paramref name="game"/>.
    /// </summary>
    /// <inheritdoc cref="TryGetLocationID{T}(DataDexManager, string, string, out T)"/>
    /// <returns>Whether or not a <paramref name="location"/> was found in
    ///     <paramref name="game"/> with the given <paramref name="ID"/>.</returns>
    public static bool TryGetLocationName(this DataDexManager ddm, string game, int ID, out string location)
        => GDR(ddm).TryGetValue(out location, game, "Locations", $"{ID}");
}