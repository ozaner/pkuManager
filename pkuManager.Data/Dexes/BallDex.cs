using System.Text.Json;

namespace pkuManager.Data.Dexes;

/// <summary>
/// Extension methods for reading data from the BallDex.
/// </summary>
public static class BallDex
{
    private static JsonElement BDR(DataDexManager ddm) => ddm.GetDex("Ball").RootElement;

    /// <summary>
    /// Checks if <paramref name="ball"/> exists in the given <paramref name="format"/>.
    /// </summary>
    /// <param name="format">The format to check.</param>
    /// <param name="ball">The ball to be checked.</param>
    /// <returns>Whether <paramref name="ball"/> exists in <paramref name="format"/>.</returns>
    private static bool DoesBallExist(this DataDexManager ddm, string format, string ball)
        => BDR(ddm).ExistsIn(ball, format);

    /// <summary>
    /// Searches the BallDex for the <paramref name="ID"/> of the given cannonical
    /// <paramref name="ball"/> name in the given <paramref name="format"/>.
    /// </summary>
    /// <typeparam name="T">The type of the ball <paramref name="ID"/> in <paramref name="format"/>.</typeparam>
    /// <inheritdoc cref="DoesBallExist(DataDexManager, string, string)" path="/param[@name='format']"/>
    /// <param name="ball">The cannonical ball name.</param>
    /// <param name="ID">The ID of <paramref name="ball"/> in <paramref name="format"/>.</param>
    /// <returns>Whether or not a valid ball <paramref name="ID"/> was found.</returns>
    public static bool TryGetBallID<T>(this DataDexManager ddm, string format, string ball, out T ID) where T: notnull
    {
        ID = default!; //nullability violation, but won't be null if false is returned.

        if (!DoesBallExist(ddm, format, ball))
            return false;

        return BDR(ddm).TryGetIndexedValue(ddm.GetIndexChain(format), out ID, ball, "Indices");
    }

    /// <summary>
    /// Searches the BallDex for the cannonical <paramref name="ball"/> name of the given 
    /// <paramref name="ID"/> in the given <paramref name="format"/>.
    /// </summary>
    /// <inheritdoc cref="TryGetBallID{T}(DataDexManager, string, string, out T)"/>
    /// <returns>Whether or not a ball was found corresponding to <paramref name="ID"/>.</returns>
    public static bool TryGetBallName<T>(this DataDexManager ddm, string format, out string ball, T ID) where T : notnull
    {
        if (!BDR(ddm).TryGetIndexedKey(ddm.GetIndexChain(format), out ball, ID, "$x", "Indices"))
            return false; //no matching ID found in index chain
        
        return DoesBallExist(ddm, format, ball); //whether the found ball exists in format.
    }
}