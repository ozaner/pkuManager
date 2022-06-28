using Newtonsoft.Json.Linq;
using pkuManager.WinForms.Formats.Modules;
using pkuManager.WinForms.Formats.pku;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace pkuManager.WinForms.Utilities;

public static class DexUtil
{
    /* ------------------------------------
     * Generic DataDex Methods
     * ------------------------------------
    */
    /// <summary>
    /// Searches the contents of a <paramref name="dex"/> with the given <paramref name="keys"/>.<br/>
    /// If no object exists at the given location, <see langword="null"/> will be returned.<br/>
    /// Throws an exception if an object does exist but can't be casted to <typeparamref name="T"/>.<br/>
    /// Note that direct array indexing is not yet implemented.
    /// </summary>
    /// <typeparam name="T">The nullable type of the value to be read.</typeparam>
    /// <param name="dex">The datadex being read.</param>
    /// <param name="keys">The location of the desired value in <paramref name="dex"/>.</param>
    /// <returns>The value pointed at by <paramref name="keys"/>, or <see langword="null"/> if it doesn't exist.</returns>
    public static T ReadDataDex<T>(this JObject dex, params string[] keys)
    {
        JObject temp = dex;
        if (keys?.Length is 0)
            return dex.ToObject<T>();
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
                return dex.ReadDataDex<T>(remote_keys.ToArray());
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
    /// <param name="value">The value to search for.</param>
    /// <returns>The key that when substituted into the "$x" term, results in <paramref name="value"/> matching.</returns>
    /// <inheritdoc cref="ReadDataDex{T}(JObject, string[])"/>
    public static string SearchDataDex<T>(this JObject dex, T value, params string[] keys)
    {
        int splitIndex = Array.IndexOf(keys, "$x");
        if (splitIndex is -1)
            throw new ArgumentException("keys neeeds at least one \"$x\" key", nameof(keys));

        JObject fh = splitIndex is 0 ? dex : dex.ReadDataDex<JObject>(keys.Take(splitIndex).ToArray());
        foreach (var x in fh ?? new())
        {
            if (x.Key is "$override") //search override last
                continue;
            keys[splitIndex] = x.Key;
            T res = dex.ReadDataDex<T>(keys);
            if ((value is null && res is null) || value is not null && value.Equals(res))
                return x.Key; //match found
        }
        keys[splitIndex] = "$x"; //put $x back in split index

        // no local key found, look for possible override
        if (fh?.ContainsKey("$override") is true) //search override if it exists
        {
            List<string> remote_keys = new(fh["$override"].ToObject<string[]>());
            remote_keys.AddRange(keys.Skip(splitIndex));
            return dex.SearchDataDex(value, remote_keys.ToArray());
        }
        return null; //no match
    }

    /// <summary>
    /// Checks if <paramref name="format"/> exists in the "Exists in" array
    /// at the location given by <paramref name="keys"/> in <paramref name="dex"/>.
    /// </summary>
    /// <param name="format">The format whose indices are to be checked.</param>
    /// <returns>Whether or not the object at the given location exists in the given format.</returns>
    /// <inheritdoc cref="ReadDataDex{T}(JObject, string[])"/>
    public static bool ExistsIn(this JObject dex, string format, params string[] keys)
    {
        string[] arr = dex.ReadDataDex<string[]>(keys.Append("Exists in").ToArray());
        if (arr is null)
            return false;
        return Array.Exists(arr, x => x.EqualsCaseInsensitive(format));
    }

    public static string[] AllExistsIn(this JObject dex, string format, params string[] keys)
    {
        JObject jobj = ReadDataDex<JObject>(dex, keys);
        List<string> finalList = new();
        foreach (var x in jobj)
        {
            if (dex.ExistsIn(format, keys.Append(x.Key).ToArray()))
                finalList.Add(x.Key);
        }
        return finalList.ToArray();
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
    /// <param name="format">The format whose indices are to be searched for.</param>
    /// <param name="keys">The path of the object in <paramref name="dex"/>.</param>
    /// <returns>The value at the given index name, or one of its fallback indices.</returns>
    /// <inheritdoc cref="ReadDataDex{T}(JObject, string[])"/>
    public static T GetIndexedValue<T>(this JObject dex, string format, params string[] keys)
        => dex.GetIndexedValue<T>(format, new List<List<string>>() { keys.ToList() });

    public static string SearchIndexedValue<T>(this JObject dex, T value, string format, string indexName, params string[] keys)
    {
        //get chain of indices to be searched.
        List<string> indexChain = new() { format };
        string[] indexParents = FORMAT_DEX.ReadDataDex<string[]>(format, "Parent Indices");
        if (indexParents is not null)
            indexChain.AddRange(indexParents);

        int xLoc = Array.IndexOf(keys, "$x");
        foreach (string link in indexChain)
        {
            string res = dex.SearchDataDex(value, keys.Append(indexName).Append(link).ToArray());
            keys[xLoc] = res;
            if (res is not null && dex.ExistsIn(format, keys))
                return res;
            keys[xLoc] = "$x";
        }
        return null;
    }


    /* ------------------------------------
     * SpeciesDex Methods
     * ------------------------------------
    */
    /// <summary>
    /// A data structure representing a particular type of Pokémon under the
    /// pku indexing system (i.e. a species, form, and appearance).
    /// </summary>
    public readonly struct SFA
    {
        /// <summary>
        /// A pku species (e.g. "Pikachu")
        /// </summary>
        public readonly string Species;

        /// <summary>
        /// A searchable pku form (e.g. "", or "Origin", or "Galarian|Zen Mode")
        /// </summary>
        public readonly string Form;

        /// <summary>
        /// A searchable pku appearance (e.g. null or "Sinnoh Cap")
        /// </summary>
        public readonly string Appearance;

        /// <summary>
        /// Whether or not to use the male or female value in the case of a gender-split.
        /// </summary>
        public readonly bool IsFemale;

        public SFA(string species, string form, string appearance, bool isFemale)
        {
            Species = species;
            Form = form ?? GetDefaultForm(species); //null form is default form
            Appearance = appearance;
            IsFemale = isFemale;
        }

        public SFA(string species, string[] forms, string[] appearances, bool isFemale)
        {
            Species = species;
            Form = forms.JoinLexical() ?? GetDefaultForm(species); //null form is default form
            Appearance = appearances.JoinLexical();
            IsFemale = isFemale;
        }

        public static implicit operator SFA(pkuObject pku)
            => new(pku.Species.Value, pku.Forms.Value, pku.Appearance.Value, pku.Gender.ToEnum<Gender>() is Gender.Female);
    }

    /// <summary>
    /// Gets a list of all form names the given <paramref name="sfa"/> can be casted to, starting with its current form.<br/>
    /// Null and empty form arrays are treated as default forms.
    /// </summary>
    /// <param name="sfa">An SFA.</param>
    /// <returns>A list of all forms the given <paramref name="sfa"/> can be casted to.</returns>
    public static List<string> GetCastableForms(this SFA sfa)
    {
        List<string> castableFormList = new() { sfa.Form };
        castableFormList.AddRange(SPECIES_DEX.ReadDataDex<List<string>>(
            sfa.Species, "Forms", sfa.Form, "Castable to"
        ) ?? new List<string>());
        return castableFormList;
    }

    /// <summary>
    /// Enumerates all the different subsets of appearances
    /// of the given <paramref name="sfa"/>'s appearance.<br/>
    /// This method enumerates the appearance combos in the canonical order.
    /// </summary>
    /// <param name="sfa">An SFA.</param>
    /// <returns>An enumerator of <paramref name="sfa"/>'s different appearance combinations.</returns>
    private static IEnumerable<string> GetSearchableAppearances(this SFA sfa)
    {
        string[] appList = sfa.Appearance.SplitLexical();
        if (appList is not null)
        {
            int effectiveSize = appList.Length > 10 ? 10 : appList.Length;
            int powesize = (1 << effectiveSize) - 1;
            for (int i = 0; i <= powesize; i++)
            {
                List<string> apps = new();
                for (int j = 0; j < effectiveSize; j++)
                {
                    if ((i & (1 << j)) is 0) //reversed 0 and 1 so loop could go form 0 to powsize
                        apps.Add(appList[j]);
                }
                yield return apps.ToArray().JoinLexical();
            }
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
        => SPECIES_DEX.ReadDataDex<string>(species, "Default Form") ?? "";

    // Iterates through each possible appearance, returning every combo of key prefixes for species/form/appearances.
    // Used to search through the SpeciesDex.
    private static IEnumerable<List<string>> GetSearchablePKUCombos(this SFA sfa)
    {
        var apps = sfa.GetSearchableAppearances();
        List<string> keys = new() { sfa.Species, "Forms", sfa.Form, "Appearance", null };
        foreach (string app in apps)
        {
            if (app is null)
                yield return keys.Take(3).ToList();
            else
                keys[4] = app;
            yield return keys;
        }
        yield return new() { sfa.Species }; //all forms/appearnces failed, try base species.
    }

    /// <summary>
    /// Like <see cref="ReadDataDex{T}(JObject, string[])"/> but specifically for the <see cref="SPECIES_DEX"/>.<br/>
    /// Searches through the appearances, form(s), then species of a species entry with the given keys.
    /// </summary>
    /// <param name="sfa">The pku whose species/form/appearance is to be used.</param>
    /// <inheritdoc cref="ReadDataDex{T}(JObject, string[])"/>
    public static T ReadSpeciesDex<T>(this JObject dex, SFA sfa, params string[] keys)
    {
        var combos = sfa.GetSearchablePKUCombos();
        foreach (var combo in combos)
        {
            combo.AddRange(keys);
            T obj = dex.ReadDataDex<T>(combo.ToArray());
            if (obj is not null)
                return obj;
            else //try gender split
            {
                T[] objArr = dex.ReadDataDex<T[]>(combo.ToArray());
                if (objArr?.Length == 2) //two values, one for each gender
                    return objArr[Convert.ToInt32(sfa.IsFemale)];
            }
        }
        return default;
    }

    /// <inheritdoc cref="ReadSpeciesDex{T}(JObject, SFA, string[])"/>
    public static T ReadSpeciesDex<T>(SFA sfa, params string[] keys)
        => ReadSpeciesDex<T>(SPECIES_DEX, sfa, keys);

    /// <summary>
    /// Searches the <see cref="SPECIES_DEX"/> for the first castable form of the pku that exists in the given format.<br/>
    /// The priority order being : original -> casted -> default (if allowed).
    /// </summary>
    /// <param name="sfa">The pku whose species/form is to be searched.</param>
    /// <param name="format">The desired format.</param>
    /// <param name="allowCasting">Whether or not to include castable forms.</param>
    /// <param name="allowDefault">Whether or not to include the default form (essentially casting to it).</param>
    /// <returns>The first form found to exist in the format, or <see langword="null"/> if no form exists.</returns>
    public static string FirstFormInFormat(this SFA sfa, string format, bool allowCasting, bool allowDefault)
    {
        string defaultForm = GetDefaultForm(sfa.Species);

        List<string> forms = new() { sfa.Form };
        if (allowCasting)
            forms.AddRange(GetCastableForms(sfa));
        if (allowDefault && !forms.Contains(defaultForm))
            forms.Add(defaultForm);

        var combos = sfa.GetSearchablePKUCombos();
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
    /// Like <see cref="GetIndexedValue(JObject, string, string[])"/> but specifically for the <see cref="SPECIES_DEX"/>.<br/>
    /// Searches through the appearances, form(s), then species of a species entry with the given keys.
    /// </summary>
    /// <param name="sfa">The SFA to search through.</param>
    /// <inheritdoc cref="GetIndexedValue(JObject, string, string[])"/>
    public static T GetSpeciesIndexedValue<T>(SFA sfa, string format, params string[] keys)
    {
        var combos = sfa.GetSearchablePKUCombos();
        IEnumerable<List<string>> temp()
        {
            foreach(var combo in combos)
            {
                combo.AddRange(keys);
                yield return combo;
            }
        }
        
        T val = SPECIES_DEX.GetIndexedValue<T>(format, temp());
        if (val is not null)
            return val;
        else //try gender split
        {
            T[] valArr = SPECIES_DEX.GetIndexedValue<T[]>(format, temp());
            if (valArr?.Length == 2) //two values, one for each gender
                return valArr[Convert.ToInt32(sfa.IsFemale)];
        }
        return default;
    }

    /// <summary>
    /// Gets the SFA that corresponds to the given species, form, and appearance indices for the given
    /// <paramref name="format"/>. If any index is null, it will be ignored in the search.<br/>
    /// Basically maps from a format's indexing system to project pku's.
    /// </summary>
    /// <typeparam name="T">The nullable type of the index (e.g. int?, string).</typeparam>
    /// <param name="format">The format to map the indices from.</param>
    /// <param name="speciesID">The index of the species.</param>
    /// <param name="formID">The index of the form.</param>
    /// <param name="isFemale">Whether to use the male or female value in the case of a gender-split.</param>
    /// <param name="appearanceID">The index of the appearance.</param>
    /// <returns>The SFA corresponding to the given indices. The SFA will have all null entries if no match is found.</returns>
    public static SFA GetSFAFromIndices<T>(string format, T speciesID, T formID, bool isFemale, T appearanceID = default)
    {
        if (speciesID is null)
            return new();

        bool matchFound(SFA sfa)
        {
            List<string> keys = new(){ sfa.Species, "Forms", sfa.Form };
            if (sfa.Appearance is not null)
            {
                keys.Add("Appearances");
                keys.Add(sfa.Appearance);
            }
            return SPECIES_DEX.ExistsIn(format, keys.ToArray()) &&
                GetSpeciesIndexedValue<T>(sfa, format, "Indices").Equals(speciesID) &&
                (formID is null || GetSpeciesIndexedValue<T>(sfa, format, "Form Indices").Equals(formID)) &&
                (appearanceID is null || GetSpeciesIndexedValue<T>(sfa, format, "Appearance Indices").Equals(appearanceID));
        }

        foreach (var species in SPECIES_DEX)
        {
            if ((species.Value as JObject).ContainsKey("Forms"))
            {
                JObject formsObj = species.Value["Forms"] as JObject;
                foreach (var form in formsObj)
                {
                    if ((form.Value as JObject).ContainsKey("Appearances"))
                    {
                        JObject appsObj = form.Value["Appearances"] as JObject;
                        foreach (var app in appsObj)
                        {
                            SFA sfa1 = new(species.Key, form.Key, app.Key, isFemale);
                            if (matchFound(sfa1))
                                return sfa1;
                        }
                    }
                    SFA sfa2 = new(species.Key, form.Key, null, isFemale);
                    if (matchFound(sfa2))
                        return sfa2;
                }
            }
        }
        return new();
    }


    /* ------------------------------------
     * Character Encoding Methods
     * ------------------------------------
    */
    public static class CharEncoding
    {
        public static bool IsLangDependent(string format)
            => FORMAT_DEX.ReadDataDex<bool?>(format, "Character Encoding", "Language Dependent") is true;

        public static BigInteger GetTerminator(string format)
            //what lang dependent format doesn't have english?
            => GetCodepoint('\u0000', format, TagUtil.DEFAULT_SEMANTIC_LANGUAGE).Value;

        /// <summary>
        /// Searches for the codepoint associated with the given char.
        /// </summary>
        /// <param name="c">The character to search.</param>
        /// <param name="format">The format being encoded to.</param>
        /// <param name="language">The language to search. Assumed to exist in this <paramref name="format"/>.</param>
        /// <returns>The codepoint that <paramref name="c"/> maps to, or null if none is found.</returns>
        private static BigInteger? GetCodepoint(char c, string format, string language = null)
        {
            string langKey = IsLangDependent(format) ? language : "All";

            //should be byte/ushort before conversion
            var temp = FORMAT_DEX.SearchDataDex(c, format, "Character Encoding", langKey, "$x");
            if (temp is null)
                return null;
            return BigInteger.Parse(temp);
        }

        /// <summary>
        /// Searches for the <see langword="char"/> associated with the given <paramref name="codepoint"/>.
        /// </summary>
        /// <param name="codepoint">The codepoint to search.</param>
        /// <param name="format">The format being encoded to.</param>
        /// <param name="language">The language to search. Assumed to exist in this <paramref name="format"/>.</param>
        /// <returns>The <see langword="char"/> that <paramref name="codepoint"/> maps to,
        ///          or null if none is found.</returns>
        private static char? GetChar(BigInteger codepoint, string format, string language = null)
        {
            string langKey = IsLangDependent(format) ? language : "All";
            return FORMAT_DEX.ReadDataDex<char?>(format, "Character Encoding", langKey, codepoint.ToString());
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
        public static (BigInteger[] encodedStr, bool truncated, bool hasInvalidChars)
            Encode(string str, int maxLength, string format, string language = null)
        {
            bool truncated = false, hasInvalidChars = false;
            BigInteger[] encodedStr = new BigInteger[maxLength];

            //Encode string
            int successfulChars = 0;
            while (str?.Length > 0 && successfulChars < maxLength)
            {
                BigInteger? encodedChar = GetCodepoint(str[0], format, language); //get next character
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
                encodedStr[successfulChars] = GetTerminator(format); //terminator
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
        /// <returns>A tuple of 1) the string decoded from <paramref name="encodedStr"/> and<br/>
        ///                     2) whether any characters were skipped due to being invalid.</returns>
        public static (string decodedStr, bool hasInvalidChars)
            Decode(BigInteger[] encodedStr, string format, string language = null)
        {
            StringBuilder sb = new();
            bool hasInvalidChars = false;
            foreach (BigInteger e in encodedStr)
            {
                if (e.Equals(GetTerminator(format)))
                    break;
                char? c = GetChar(e, format, language);
                if (c is not null)
                    sb.Append(c.Value);
                else
                    hasInvalidChars = true;
            }
            return (sb.ToString(), hasInvalidChars);
        }
    }
}