using Newtonsoft.Json.Linq;
using pkuManager.Formats.Modules;
using pkuManager.Formats.pku;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pkuManager.Utilities;

public static class DexUtil
{
    /* ------------------------------------
     * Generic DataDex Methods
     * ------------------------------------
    */
    /// <summary>
    /// Searches the contents of a <paramref name="datadex"/> with the given <paramref name="keys"/>.<br/>
    /// If no object exists at the given location, <see langword="null"/> will be returned.<br/>
    /// Throws an exception if an object does exist but can't be casted to <typeparamref name="T"/>.<br/>
    /// Note that direct array indexing is not yet implemented.
    /// </summary>
    /// <typeparam name="T">The type of the value to be read.</typeparam>
    /// <param name="datadex">The datadex being read.</param>
    /// <param name="keys">The location of the desired value in <paramref name="datadex"/>.</param>
    /// <returns>The value pointed at by <paramref name="keys"/>, or <see langword="null"/> if it doesn't exist.</returns>
    public static T ReadDataDex<T>(this JObject datadex, params string[] keys)
    {
        JObject temp = datadex;
        for (int i = 0; i < keys.Length; i++)
        {
            //null isn't a valid key
            if (keys[i] is null || temp is null)
                return default;

            // try searching local for key
            if (temp.ContainsKey(keys[i]))
            {
                var value = temp[keys[i]];
                if (i >= keys.Length - 1) //last key
                {
                    try { return value.ToObject<T>(); } //return whatever the value is
                    catch { return default; }//can't return the value, doesn't match the type
                }
                else //not last key, should be a dict
                {
                    if (value is JObject valueJObj) //is a dict
                        temp = valueJObj; //continue loop
                    else //isn't a dict
                        return default; //failure, return null

                    //TODO: add direct array indexing, i.e. else if(value is JArray valueJArr)
                }
            }
            // no local key found, look for possible override
            else if (temp.ContainsKey("$override"))
            {
                List<string> remote_keys = new(temp["$override"].ToObject<string[]>());
                remote_keys.AddRange(keys.Skip(i));
                return datadex.ReadDataDex<T>(remote_keys.ToArray());
            }
            else //key not found, no potential override
                return default; //failure, return null
        }
        return default; //shouldn't get here
    }

    /// <summary>
    /// Returns the key that, when substituted in for "$x",
    /// has <paramref name="value"/> matching the value at <paramref name="keys"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to be read.</typeparam>
    /// <param name="datadex">The datadex being read.</param>
    /// <param name="keys">The location of the desired value in <paramref name="datadex"/>.</param>
    /// <param name="value">The value to search for.</param>
    /// <returns>The key that when substituted into the "$x" term, results in <paramref name="value"/> matching.</returns>
    public static string SearchDataDex<T>(this JObject datadex, T value, params string[] keys)
    {
        int splitIndex = Array.IndexOf(keys, "$x");
        if (splitIndex is -1)
            throw new ArgumentException("keys neeeds at least one \"$x\" key", nameof(keys));

        JObject fh = splitIndex is 0 ? datadex : datadex.ReadDataDex<JObject>(keys.Take(splitIndex).ToArray());
        foreach (var x in fh ?? new())
        {
            if (x.Key is "$override") //search override last
                continue;
            keys[splitIndex] = x.Key;
            T res = datadex.ReadDataDex<T>(keys);
            if ((value is null && res is null) || value is not null && value.Equals(res))
                return x.Key; //match found
        }
        keys[splitIndex] = "$x"; //put $x back in split index

        // no local key found, look for possible override
        if (fh?.ContainsKey("$override") is true) //search override if it exists
        {
            List<string> remote_keys = new(fh["$override"].ToObject<string[]>());
            remote_keys.AddRange(keys.Skip(splitIndex));
            return datadex.SearchDataDex(value, remote_keys.ToArray());
        }
        return null; //no match
    }

    /// <summary>
    /// Checks if <paramref name="format"/> exists  in the "Exists in" array
    /// at the location given by <paramref name="keys"/> in <paramref name="dex"/>.
    /// </summary>
    /// <param name="dex">A datadex.</param>
    /// <param name="format">A storage format.</param>
    /// <param name="keys">The path of the object in <paramref name="dex"/>.</param>
    /// <returns>Whether or not the object at the given location exists in the given format.</returns>
    public static bool ExistsIn(this JObject dex, string format, params string[] keys)
    {
        string[] arr = dex.ReadDataDex<string[]>(keys.Append("Exists in").ToArray());
        if (arr is null)
            return false;
        return Array.Exists(arr, x => x.EqualsCaseInsensitive(format));
    }

    // Common GetIndex code, that allows inner looping of keys (e.g. species combos) and outer looping of formats (e.g. pk3 -> main-series)
    private static T GetIndexedValue<T>(this JObject dex, string format, IEnumerable<List<string>> other_keys)
    {
        //get chain of indices to be searched.
        List<string> indexChain = new() { format };
        string[] indexParents = FORMAT_DEX.ReadDataDex<string[]>(format, "Parent Indices");
        if (indexParents is not null)
            indexChain.AddRange(indexParents);

        foreach (string link in indexChain)
        {
            foreach (var keys in other_keys)
            {
                List<string> temp = new(keys) { link };
                T index = dex.ReadDataDex<T>(temp.ToArray());
                if (index is not null)
                    return index;
            }
        }
        return default;
    }

    /// <summary>
    /// Gets an index type value of whatever <paramref name="keys"/> points to for the <paramref name="format"/>.<br/>
    /// If format fails, tries the format's "Parent Indices".
    /// </summary>
    /// <param name="dex">A datadex.</param>
    /// <param name="format">A storage format.</param>
    /// <param name="keys">The path of the object in <paramref name="dex"/>.</param>
    /// <returns>The value at the given index name, or one of its fallback indices.</returns>
    public static T GetIndexedValue<T>(this JObject dex, string format, params string[] keys)
        => dex.GetIndexedValue<T>(format, new List<List<string>>() { keys.ToList() });


    /* ------------------------------------
     * SpeciesDex Methods
     * ------------------------------------
    */
    /// <summary>
    /// Gets a list of all form names the given <paramref name="pku"/> can be casted to, starting with its current form.<br/>
    /// Null and empty form arrays are treated as default forms.
    /// </summary>
    /// <param name="pku">The pku whose castable forms are to be retrieved.</param>
    /// <returns>A list of all forms the given <paramref name="pku"/> can be casted to.</returns>
    public static List<string> GetCastableForms(this pkuObject pku)
    {
        string searchableFormName = pku.GetSearchableForm();
        List<string> castableFormList = new() { searchableFormName };
        castableFormList.AddRange(SPECIES_DEX.ReadDataDex<List<string>>(
            pku.Species, "Forms", searchableFormName, "Castable to"
        ) ?? new List<string>());
        return castableFormList;
    }

    /// <summary>
    /// Enumerates all the different subsets of appearances
    /// of the given <paramref name="pku"/>'s appearance array.<br/>
    /// This method enumerates the appearance combos in the canonical order.
    /// </summary>
    /// <param name="pku">The pku whose appearances are to be enumerated.</param>
    /// <returns>An enumerator of <paramref name="pku"/>'s different appearance combinations.</returns>
    private static IEnumerable<string> GetSearchableAppearances(pkuObject pku)
    {
        if (pku.Appearance?.Length is not > 0)
        {
            yield return null;
            yield break;
        }

        int effectiveSize = pku.Appearance.Length > 63 ? 64 : pku.Appearance.Length;
        ulong powesize = effectiveSize is 64 ? ulong.MaxValue : ((ulong)1 << effectiveSize) - 1;
        for (ulong i = 0; i <= powesize; i++)
        {
            List<string> apps = new();
            for (int j = 0; j < effectiveSize; j++)
            {
                if ((i & ((ulong)1 << j)) is 0) //reversed 0 and 1 so loop could go form 0 to powsize
                    apps.Add(pku.Appearance[j]);
            }
            yield return apps.ToArray().JoinLexical();
        }
        yield return null; //no appearance
    }

    /// <summary>
    /// Searches for the the default form of the given species.<br/>
    /// If the default form is unnamed (e.g. Bulbasaur) then the empty string will be returned.
    /// </summary>
    /// <param name="species">The species whose default form is to be retrieved.</param>
    /// <returns>The default form for this species or, if none found, the empty string.</returns>
    public static string GetDefaultForm(string species)
    {
        JObject forms = SPECIES_DEX.ReadDataDex<JObject>(species, "Forms");
        if (forms is null) // No listed forms, default is just "" (i.e. base form)
            return "";

        foreach (var form in forms)
        {
            bool? isDefault = SPECIES_DEX.ReadDataDex<bool?>(species, "Forms", form.Key, "Default");
            if (isDefault is true)
                return form.Key;
        }
        return ""; //No form was listed as default, default is ""
    }

    /// <summary>
    /// Returns the searchable form of the <paramref name="pku"/>'s forms array for use in searching through datadexes.
    /// </summary>
    /// <param name="pku">The pku whose form is to be formatted.</param>
    /// <returns>The searchable form name, or the default form if form array is empty or null.</returns>
    public static string GetSearchableForm(this pkuObject pku)
        => !pku.Forms.IsNull && pku.Forms.Length > 0 ? pku.Forms.Get().JoinLexical() : GetDefaultForm(pku.Species);

    // Iterates through each possible appearance, returning every combo of key prefixes for species/form/appearances.
    // Used to search through the SpeciesDex.
    private static IEnumerable<List<string>> GetSearchablePKUCombos(this pkuObject pku)
    {
        string form = pku.GetSearchableForm();

        var apps = GetSearchableAppearances(pku);
        List<string> keys = new() { pku.Species, "Forms", form, "Appearance", null };
        foreach (string app in apps)
        {
            if (app is null)
                yield return keys.Take(3).ToList();
            else
                keys[4] = app;
            yield return keys;
        }
        yield return new() { pku.Species }; //all forms/appearnces failed, try base species.
    }

    /// <summary>
    /// Like <see cref="ReadDataDex{T}(JObject, string[])"/> but searches through the<br/>
    /// appearances, form(s), then species level of a species entry with the given keys.
    /// </summary>
    /// <param name="dex">The species dex to read.</param>
    /// <param name="pku">The pku whose species/form/appearance is to be used.</param>
    /// <inheritdoc cref="ReadDataDex{T}(JObject, string[])"/>
    public static T ReadSpeciesDex<T>(this JObject dex, pkuObject pku, params string[] keys)
    {
        var combos = pku.GetSearchablePKUCombos();
        foreach (var combo in combos)
        {
            combo.AddRange(keys);
            T obj = dex.ReadDataDex<T>(combo.ToArray());
            if (obj is not null)
                return obj;
        }
        return default;
    }

    /// <inheritdoc cref="ReadSpeciesDex{T}(JObject, pkuObject, string[])"/>
    public static T ReadSpeciesDex<T>(pkuObject pku, params string[] keys)
        => ReadSpeciesDex<T>(SPECIES_DEX, pku, keys);

    /// <summary>
    /// Searches the <see cref="SPECIES_DEX"/> for the first castable form of the pku that exists in the given format.<br/>
    /// The priority order being : original -> casted -> default (if allowed).
    /// </summary>
    /// <param name="pku">The pku whose species/form is to be searched.</param>
    /// <param name="format">The desired format.</param>
    /// <param name="allowCasting">Whether or not to include castable forms.</param>
    /// <param name="allowDefault">Whether or not to include the default form (essentially casting to it).</param>
    /// <returns>The first form found to exist in the format, or <see langword="null"/> if no form exists.</returns>
    public static string FirstFormInFormat(this pkuObject pku, string format, bool allowCasting, bool allowDefault)
    {
        string defaultForm = GetDefaultForm(pku.Species);

        List<string> forms = new() { GetSearchableForm(pku) };
        if (allowCasting)
            forms.AddRange(GetCastableForms(pku));
        if (allowDefault && !forms.Contains(defaultForm))
            forms.Add(defaultForm);

        var combos = pku.GetSearchablePKUCombos();
        foreach(string form in forms)
        {
            foreach (var combo in combos)
            {
                if(combo.Count > 2)
                    combo[2] = form; // 0: species, 1: "Form", 2: form
                if (SPECIES_DEX.ExistsIn(format, combo.ToArray()))
                    return form;
            }
        }
        return null; //pku combo not found
    }

    /// <summary>
    /// Like <see cref="GetIndexedValue(JObject, string, string[])"/> but searches through the<br/>
    /// appearances, form(s), then species level of a species entry with the given keys.
    /// </summary>
    /// <param name="pku">The pku.</param>
    /// <inheritdoc cref="GetIndexedValue(JObject, string, string[])"/>
    public static T GetSpeciesIndexedValue<T>(pkuObject pku, string format, params string[] keys)
    {
        var combos = pku.GetSearchablePKUCombos();
        IEnumerable<List<string>> temp()
        {
            foreach(var combo in combos)
            {
                combo.AddRange(keys);
                yield return combo;
            }
        }
        return SPECIES_DEX.GetIndexedValue<T>(format, temp());
    }


    /* ------------------------------------
     * Character Encoding Methods
     * ------------------------------------
    */
    public static class CharEncoding<T> where T : struct
    {
        private static bool IsLangDependent(string format)
            => FORMAT_DEX.ReadDataDex<bool?>(format, "Character Encoding", "Language Dependent") is true;

        private static T GetTerminator(string format, Language? language = null)
            => GetCodepoint('\u0000', format, language).Value;

        /// <summary>
        /// Searches for the codepoint associated with the given char.
        /// </summary>
        /// <param name="c">The character to search.</param>
        /// <param name="format">The format being encoded to.</param>
        /// <param name="language">The language to search. Assumed to exist in this <paramref name="format"/>.</param>
        /// <returns>The codepoint that <paramref name="c"/> maps to, or null if none is found.</returns>
        private static T? GetCodepoint(char c, string format, Language? language = null)
        {
            string langStr = IsLangDependent(format) ? language.ToFormattedString() : "All";
            return FORMAT_DEX.SearchDataDex(c, format, "Character Encoding", langStr, "$x").CastTo<T?>(); //should be byte/ushort
        }

        /// <summary>
        /// Searches for the <see langword="char"/> associated with the given <paramref name="codepoint"/>.
        /// </summary>
        /// <param name="codepoint">The codepoint to search.</param>
        /// <param name="format">The format being encoded to.</param>
        /// <param name="language">The language to search. Assumed to exist in this <paramref name="format"/>.</param>
        /// <returns>The <see langword="char"/> that <paramref name="codepoint"/> maps to,
        ///          or null if none is found.</returns>
        private static char? GetChar(T codepoint, string format, Language? language = null)
        {
            string langStr = IsLangDependent(format) ? language.ToFormattedString() : "All";
            return FORMAT_DEX.ReadDataDex<char?>(format, "Character Encoding", langStr, codepoint.ToString());
        }

        /// <summary>
        /// Encodes a given string, ending with the terminator
        /// if the maximum length is not reached. Padded with 0s.<br/>
        /// </summary>
        /// <param name="str">The string to be encoded.</param>
        /// <param name="maxLength">The desired length of the encoded string.</param>
        /// <param name="format">The format being encoded to.</param>
        /// <param name="language">The language to encode <paramref name="str"/>, if <paramref name="format"/>
        ///                        is language dependent. Null otherwise.</param>
        /// <returns>The encoded form of <paramref name="str"/>.</returns>
        public static (T[] encodedStr, bool truncated, bool hasInvalidChars)
            Encode(string str, int maxLength, string format, Language? language = null)
        {
            bool truncated = false, hasInvalidChars = false;
            T[] encodedStr = new T[maxLength];

            //Encode string
            int successfulChars = 0;
            while (str?.Length > 0 && successfulChars < maxLength)
            {
                T? encodedChar = GetCodepoint(str[0], format, language); //get next character
                str = str[1..]; //chop off current character

                //if character invalid
                if (encodedChar is null)
                {
                    hasInvalidChars = true;
                    continue;
                }

                //else character not invalid
                encodedStr[successfulChars] = encodedChar.Value;
                successfulChars++;

                //stop encoding when limit reached
                if (successfulChars >= maxLength)
                    break;
            }

            //Deal with terminator
            if (successfulChars < maxLength)
                encodedStr[successfulChars] = GetTerminator(format, language); //terminator
            return (encodedStr, truncated, hasInvalidChars);
        }

        /// <summary>
        /// Decodes a given encoded string, stopping at the first instance of the terminator.<br/>
        /// If an invalid language is passed, an exception will be thrown.
        /// </summary>
        /// <param name="encodedStr">A string encoded with this character encoding.</param>
        /// <param name="format">The format being decoded from.</param>
        /// <param name="language">The language <paramref name="encodedStr"/> was encoded with, if <paramref name="format"/>
        ///                        is language dependent. Null otherwise.</param>
        /// <returns>The string decoded from <paramref name="encodedStr"/>.</returns>
        public static string Decode(T[] encodedStr, string format, Language? language = null)
        {
            StringBuilder sb = new();
            foreach (T e in encodedStr)
            {
                if (e.Equals(GetTerminator(format, language)))
                    break;
                char? c = GetChar(e, format, language);
                if (c is not null)
                    sb.Append(c.Value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Overlays the given <paramref name="encodedStr"/> over the given <paramref name="trash"/> array.
        /// </summary>
        /// <param name="encodedStr">An encoded string.</param>
        /// <param name="trash">The trash bytes to be applied to <paramref name="encodedStr"/>.</param>
        /// <param name="format">The format being decoded from.</param>
        /// <param name="language">The language <paramref name="encodedStr"/> was encoded with, if <paramref name="format"/>
        ///                        is language dependent. Null otherwise.</param>
        /// <returns>The encoded string 'trashed' with the given trash bytes.</returns>
        public static T[] Trash(T[] encodedStr, ushort[] trash, string format, Language? language = null)
        {
            //cast ushort trash to generic T
            T[] trashedStr = Array.ConvertAll(trash.Clone() as ushort[],
                x => (T)Convert.ChangeType(x, typeof(T)));

            trashedStr = trashedStr[0..encodedStr.Length];
            for (int i = 0; i < encodedStr.Length; i++)
            {
                trashedStr[i] = encodedStr[i];
                if (encodedStr[i].Equals(GetTerminator(format, language)))
                    break;
            }
            return trashedStr;
        }
    }
}