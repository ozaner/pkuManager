namespace pkuManager.Data;

internal static class MiscUtil
{
    /// <summary>
    /// Concatenates an array of strings in lexicographical order, each delimited by '|'.<br/>
    /// <i>Note: <see langword="null"/> values are skipped.</i>
    /// </summary>
    /// <param name="strs">The strings to be concatenated.</param>
    /// <returns>The values of <paramref name="strs"/> concatenated.</returns>
    internal static string JoinLexical(this string[] strs)
    {
        strs = strs.Where(x => x is not null).ToArray(); //filter out nulls
        Array.Sort(strs, StringComparer.OrdinalIgnoreCase); //sort lexographically
        return string.Join('|', strs); //join w/ '|'
    }

    /// <summary>
    /// Counts the number of '|' characters in <paramref name="str"/>.
    /// </summary>
    /// <param name="str">The string whose characters are to be counted.</param>
    /// <returns>The number of '|' characters in <paramref name="str"/>.</returns>
    internal static int PipeCount(string str) => str.Count(c => c is '|');

    /// <summary>
    /// Compares two strings based on how many pipes they contain.<br/>
    /// In the case of a tie, they are compared lexographically.
    /// </summary>
    /// <param name="a">First string to compare.</param>
    /// <param name="b">Second string to compare.</param>
    /// <returns>A positive integer if <paramref name="a"/> is 'larger',
    ///     negative if <paramref name="b"/> is, and 0 if equal.</returns>
    internal static int PipeCompareTo(string a, string b)
    {
        int aC = PipeCount(a);
        int bC = PipeCount(b);
        if (aC == bC)
            return a.CompareTo(b);
        return aC.CompareTo(bC);
    }

    /// <summary>
    /// Checks if, when split on '|', <paramref name="sub"/> is a subset or <paramref name="super"/>.<br/>
    /// If this is true we call sub a <i>pipe subset</i> of super.
    /// </summary>
    /// <param name="super">The string assumed to be the superset of <paramref name="sub"/>.</param>
    /// <param name="sub">The string assumed to be the subset of <paramref name="super"/>.</param>
    /// <returns>Whether or not <paramref name="sub"/> is a pipe subset of <paramref name="super"/>.</returns>
    internal static bool IsPipeSubset(string super, string sub)
    {
        HashSet<string> superSet = new(super.Split('|'));
        HashSet<string> subSet = new(sub.Split('|'));
        return subSet.IsSubsetOf(superSet);
    }

    /// <summary>
    /// Attempts to convert <paramref name="str"/> to an enum of type <typeparamref name="T"/>.<br/>
    /// <i>Note: spaces are replaced with underscores before conversion.</i>
    /// </summary>
    /// <typeparam name="T">An enum type.</typeparam>
    /// <param name="str">A string representing an enum of type <typeparamref name="T"/>.</param>
    /// <returns>Whether the conversion was successful.</returns>
    internal static bool TryToEnum<T>(this string str, out T e) where T : struct
        => Enum.TryParse(str.Replace(' ', '_'), false, out e);
}
