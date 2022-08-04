using System.Text.Json;
using pkuManager.Data.DexTemplates;

namespace pkuManager.Data.Dexes;

/// <summary>
/// Represents a FormatDex.<br/>
/// Includes misc. information on particular formats.
/// </summary>
public class FormatDex : DataDex
{
    /// <summary>
    /// Constructs a <see cref="FormatDex"/> from the given <paramref name="jsonDoc"/>.
    /// </summary>
    /// <inheritdoc cref="DataDex(JsonDocument)"/>
    public FormatDex(JsonDocument jsonDoc) : base(jsonDoc) { }

    /// <summary>
    /// Gets an array of index names (e.g. "pk3", "main-series"), called the 'index chain',<br/>
    /// in order of precedence that they should be checked when searching for an indexed value.<br/>
    /// Note that <paramref name="format"/> is always the first element.
    /// </summary>
    /// <param name="format">The format in question.</param>
    /// <returns>The index chain of the given <paramref name="format"/>.</returns>
    public List<string> GetIndexChain(string format)
    {
        List<string> indexChain = new() { format };
        if (TryGetValue(out string[] indexParents, format, "Parent Indices"))
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
    public bool IsLangDependent(string format)
        => TryGetValue(out bool langDep, format, "Character Encoding", "Language Dependent") && langDep;

    /// <summary>
    /// Gets the null terminator character in the given <paramref name="format"/>'s encoding.
    /// </summary>
    /// <inheritdoc cref="IsLangDependent(string)" path="/param[@name='format']"/>
    /// <returns>The codepoint of the terminator character.</returns>
    /// <exception cref="ArgumentException">Thrown if the given <paramref name="format"/>
    ///     is one without a defined character encoding or, if format <see cref="IsLangDependent(string)"/>,
    ///     thrown if "English" is not an encodable language.</exception>
    public int GetTerminator(string format)
    {
        //Assume all lang dep formats have English...
        if (TryGetCodepoint(out int codepoint, '\u0000', format, "English"))
            return codepoint;
        else
            throw new ArgumentException($"{nameof(format)} should be a format with custom char " +
                $"encodings and, if lang dependent, one of them should be English.");
    }

    /// <summary>
    /// Attemps to find the char <paramref name="c"/> that cooresponds to
    /// the <paramref name="codepoint"/> in the given <paramref name="format"/>'s encoding.
    /// </summary>
    /// <param name="c">The character <paramref name="codepoint"/> represents, if one is found.</param>
    /// <param name="codepoint">The codepoint to check.</param>
    /// <param name="language">The language the character should be interpreted under.</param>
    /// <inheritdoc cref="IsLangDependent(string)" path="/param[@name='format']"/>
    /// <returns>Whether or not a valid <paramref name="c"/>har was found.</returns>
    public bool TryGetChar(out char c, int codepoint, string format, string? language = null)
    {
        string langKey = IsLangDependent(format) ? language! : "All";
        return TryGetValue(out c, format, "Character Encoding", langKey, codepoint.ToString());
    }

    /// <summary>
    /// Attempts to find the <paramref name="codepoint"/> the character
    /// <paramref name="c"/> corresponds to in the given <paramref name="format"/>'s encoding.
    /// </summary>
    /// <param name="codepoint">The codepoint of <paramref name="c"/>, if one is found.</param>
    /// <param name="c">The character whose <paramref name="codepoint"/> is to be found.</param>
    /// <inheritdoc cref="IsLangDependent(string)" path="/param[@name='format']"/>
    /// <inheritdoc cref="TryGetChar(out char, int, string, string?)" path="/param[@name='language']"/>
    /// <returns>Whether or not a valid <paramref name="codepoint"/> was found.</returns>
    public bool TryGetCodepoint(out int codepoint, char c, string format, string? language = null)
    {
        string langKey = IsLangDependent(format) ? language! : "All";

        if (TryGetKey(out string codepointStr, c, format, "Character Encoding", langKey, "$x"))
            return int.TryParse(codepointStr, out codepoint); //if dex has a non-int codepoint for some reason, return false
        else
        {
            codepoint = default;
            return false;
        }
    }
}