using System.Text.Json;

namespace pkuManager.Data.Dexes;

/// <summary>
/// Extension methods for reading data from the FormatDex.
/// </summary>
public static class FormatDex
{
    private static JsonElement FDR(DataDexManager ddm) => ddm.GetDexRoot("Format");

    //TODO: make this internal
    /// <summary>
    /// Gets an array of index names (e.g. "pk3", "main-series"), called the 'index chain',<br/>
    /// in order of precedence that they should be checked when searching for an indexed value.<br/>
    /// Note that <paramref name="format"/> is always the first element.
    /// </summary>
    /// <param name="format">The format in question.</param>
    /// <returns>The index chain of the given <paramref name="format"/>.</returns>
    public static List<string> GetIndexChain(this DataDexManager ddm, string format)
    {
        List<string> indexChain = new() { format };
        if (FDR(ddm).TryGetValue(out string[] indexParents, format, "Parent Indices"))
            indexChain.AddRange(indexParents);
        return indexChain;
    }


    /* ------------------------------------
     * Character Encoding Stuff
     * ------------------------------------
    */
    /// <summary>
    /// Determines whether the given <paramref name="format"/>
    /// has a language dependent character encoding or not.
    /// </summary>
    /// <param name="format">The format of the encoding.</param>
    /// <returns>Whether or not the given format is language dependent.</returns>
    public static bool IsLangDependent(this DataDexManager ddm, string format)
        => FDR(ddm).TryGetValue(out bool langDep, format, "Character Encoding", "Language Dependent") && langDep;

    /// <summary>
    /// Attemps to find the char <paramref name="c"/> that cooresponds to
    /// the <paramref name="codepoint"/> in the given <paramref name="format"/>'s encoding.
    /// </summary>
    /// <param name="c">The character that <paramref name="codepoint"/> represents, if one is found.</param>
    /// <param name="codepoint">The codepoint to check.</param>
    /// <inheritdoc cref="IsLangDependent(DataDexManager, string)" path="/param[@name='format']"/>
    /// <param name="language">The language the character should be interpreted under.<br/>
    ///     <see langword="null"/> if <paramref name="format"/> is <see cref="IsLangDependent(DataDexManager, string)"/>.</param>
    /// <returns>Whether or not a valid <paramref name="c"/>har was found.</returns>
    public static bool TryGetChar(this DataDexManager ddm, out char c, int codepoint, string format, string? language = null)
    {
        string langKey = ddm.IsLangDependent(format) ? language! : "All";
        return FDR(ddm).TryGetValue(out c, format, "Character Encoding", langKey, codepoint.ToString());
    }

    /// <summary>
    /// Attempts to find the <paramref name="codepoint"/> the character
    /// <paramref name="c"/> corresponds to in the given <paramref name="format"/>'s encoding.
    /// </summary>
    /// <param name="codepoint">The codepoint of <paramref name="c"/>, if one is found.</param>
    /// <param name="c">The character whose <paramref name="codepoint"/> is to be found.</param>
    /// <inheritdoc cref="IsLangDependent(DataDexManager, string)(string)" path="/param[@name='format']"/>
    /// <inheritdoc cref="TryGetChar(DataDexManager, out char, int, string, string?)" path="/param[@name='language']"/>
    /// <returns>Whether or not a valid <paramref name="codepoint"/> was found.</returns>
    public static bool TryGetCodepoint(this DataDexManager ddm, out int codepoint, char c, string format, string? language = null)
    {
        string langKey = ddm.IsLangDependent(format) ? language! : "All";

        if (FDR(ddm).TryGetKey(out string codepointStr, c, format, "Character Encoding", langKey, "$x"))
            return int.TryParse(codepointStr, out codepoint); //if dex has a non-int codepoint for some reason, return false
        else
        {
            codepoint = default;
            return false;
        }
    }

    /// <summary>
    /// Gets the null terminator character in the given <paramref name="format"/>'s encoding.
    /// </summary>
    /// <inheritdoc cref="TryGetCodepoint(DataDexManager, out int, char, string, string?)" path="/param[@name='format']"/>
    /// <returns>The codepoint of the terminator character.</returns>
    /// <exception cref="ArgumentException">Thrown if 1) the given <paramref name="format"/>
    ///     is one without a defined character encoding or 2) <paramref name="format"/>
    ///     <see cref="IsLangDependent(DataDexManager, string)"/>, and "English" is not an encodable language.</exception>
    public static int GetTerminator(this DataDexManager ddm, string format)
    {
        //Assume all lang dep formats have English...
        if (ddm.TryGetCodepoint(out int codepoint, '\u0000', format, "English"))
            return codepoint;
        else
            throw new ArgumentException($"{nameof(format)} should be a format with custom char " +
                $"encodings and, if lang dependent, one of them should be English.");
    }
}