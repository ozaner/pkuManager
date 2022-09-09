using System.Text.Json;

namespace pkuManager.Data.Dexes;

/// <summary>
/// Extension methods for reading data from the SpeciesDex.
/// </summary>
public static class SpeciesDex
{
    private static JsonElement SDR(DataDexManager ddm) => ddm.GetDexRoot("Species");

    /// <summary>
    /// Searches for the the default form of the given species.<br/>
    /// If the default form is unnamed (e.g. Bulbasaur) then the empty string will be returned.
    /// </summary>
    /// <param name="species">The species whose default form is to be retrieved.</param>
    /// <returns>The default form for this species or, if none is found, the empty string.</returns>
    public static string GetDefaultForm(this DataDexManager ddm, string species)
        => SDR(ddm).TryGetValue(out string defaultForm, species, "Default Form") ? defaultForm : "";

    /// <summary>
    /// Gets a list of all form names the given <paramref name="sfam"/> can be casted to.
    /// </summary>
    /// <param name="sfam">The SFAM to search.</param>
    /// <returns>An array of all forms the given <paramref name="sfam"/> can be casted to.</returns>
    public static string[] GetCastableForms(this DataDexManager ddm, SFAM sfam)
    {
        if (SDR(ddm).TryGetValue(out string[] readForms, sfam.Species, "Forms", sfam.Form, "Castable to"))
            return readForms;
        return Array.Empty<string>();
    }

    /// <summary>
    /// Searches the SpeciesDex for the <paramref name="growthRate"/> of the given <paramref name="sfam"/>.
    /// </summary>
    /// <inheritdoc cref="GetCastableForms(DataDexManager, SFAM)" path="/param[@name='sfam']"/>
    /// <param name="growthRate">The <see cref="GrowthRate"/> of <paramref name="sfam"/>, if found.</param>
    /// <returns>Whether or not a <paramref name="growthRate"/> was found.</returns>
    public static bool TryGetGrowthRate(this DataDexManager ddm, SFAM sfam, out GrowthRate growthRate)
    {
        growthRate = default;
        if (SDR(ddm).TryGetSFAMValue(sfam, out string grStr, "Growth Rate"))
            return grStr.TryToEnum(out growthRate); //fails if growth rate is invalid
        return false; //no growth rate found
    }

    /// <summary>
    /// Searches the SpeciesDex for the <paramref name="genderRatio"/> of the given <paramref name="sfam"/>.
    /// </summary>
    /// <inheritdoc cref="GetCastableForms(DataDexManager, SFAM)" path="/param[@name='sfam']"/>
    /// <param name="genderRatio">The <see cref="GenderRatio"/> of <paramref name="sfam"/>, if found.</param>
    /// <returns>Whether or not a <paramref name="genderRatio"/> was found.</returns>
    public static bool TryGetGenderRatio(this DataDexManager ddm, SFAM sfam, out GenderRatio genderRatio)
    {
        genderRatio = default;
        if (SDR(ddm).TryGetSFAMValue(sfam, out string grStr, "Gender Ratio"))
            return grStr.TryToEnum(out genderRatio); //fails if gender ratio is invalid
        return false; //no gender ratio found
    }

    /// <summary>
    /// Searches the SpeciesDex for the species <paramref name="name"/> of the
    /// given <paramref name="sfam"/> in the given <paramref name="lang"/>.
    /// </summary>
    /// <inheritdoc cref="GetCastableForms(DataDexManager, SFAM)" path="/param[@name='sfam']"/>
    /// <param name="lang">The desired language of the species name.</param>
    /// <param name="name">The species name of <paramref name="sfam"/> in <paramref name="lang"/>, if found.</param>
    /// <returns>Whether or not a <paramref name="name"/> was found for the given <paramref name="lang"/>.</returns>
    public static bool TryGetSpeciesLangName(this DataDexManager ddm, SFAM sfam, string lang, out string name)
        => SDR(ddm).TryGetSFAMValue(sfam, out name, "Names", lang);

    /// <summary>
    /// Searches the SpeciesDex for the species <paramref name="ID"/> of the
    /// given <paramref name="sfam"/> in the given <paramref name="format"/>.
    /// </summary>
    /// <typeparam name="T">The type that <paramref name="format"/> uses for its species IDs.</typeparam>
    /// <inheritdoc cref="GetCastableForms(DataDexManager, SFAM)" path="/param[@name='sfam']"/>
    /// <param name="format">The format to search in.</param>
    /// <param name="ID">The species ID of <paramref name="sfam"/> in <paramref name="format"/>, if found.</param>
    /// <returns>Whether or not an <paramref name="ID"/> was found in the given <paramref name="format"/>.</returns>
    public static bool TryGetSpeciesID<T>(this DataDexManager ddm, SFAM sfam, string format, out T ID) where T : notnull
        => SDR(ddm).TryGetSFAMIndexedValue(sfam, ddm.GetIndexChain(format), out ID, "Indices");

    /// <summary>
    /// Searches the SpeciesDex for the form <paramref name="ID"/> of the
    /// given <paramref name="sfam"/> in the given <paramref name="format"/>.
    /// </summary>
    /// <typeparam name="T">The type that <paramref name="format"/> uses for its form IDs.</typeparam>
    /// <inheritdoc cref="GetCastableForms(DataDexManager, SFAM)" path="/param[@name='sfam']"/>
    /// <inheritdoc cref="TryGetSpeciesID{T}(DataDexManager, SFAM, string, out T)" path="/param[@name='format']"/>
    /// <param name="ID">The form ID of <paramref name="sfam"/> in <paramref name="format"/>, if found.</param>
    /// <inheritdoc cref="TryGetSpeciesID{T}(DataDexManager, SFAM, string, out T)" path="/returns"/>
    public static bool TryGetFormID<T>(this DataDexManager ddm, SFAM sfam, string format, out T ID) where T : notnull
        => SDR(ddm).TryGetSFAMIndexedValue(sfam, ddm.GetIndexChain(format), out ID, "Form Indices");

    /// <summary>
    /// Searches the SpeciesDex for the appearance <paramref name="ID"/> of the
    /// given <paramref name="sfam"/> in the given <paramref name="format"/>.
    /// </summary>
    /// <typeparam name="T">The type that <paramref name="format"/> uses for its appearance IDs.</typeparam>
    /// <inheritdoc cref="GetCastableForms(DataDexManager, SFAM)" path="/param[@name='sfam']"/>
    /// <inheritdoc cref="TryGetSpeciesID{T}(DataDexManager, SFAM, string, out T)" path="/param[@name='format']"/>
    /// <param name="ID">The appearance ID of <paramref name="sfam"/> in <paramref name="format"/>, if found.</param>
    /// <inheritdoc cref="TryGetSpeciesID{T}(DataDexManager, SFAM, string, out T)" path="/returns"/>
    public static bool TryGetAppearanceID<T>(this DataDexManager ddm, SFAM sfam, string format, out T ID) where T : notnull
        => SDR(ddm).TryGetSFAMIndexedValue(sfam, ddm.GetIndexChain(format), out ID, "Appearance Indices");

    /// <summary>
    /// Searches the SpeciesDex for the ability <paramref name="slots"/>
    /// of the given <paramref name="sfam"/> in the given <paramref name="format"/>.
    /// </summary>
    /// <typeparam name="T">The type that <paramref name="format"/> uses for its ability IDs.</typeparam>
    /// <inheritdoc cref="GetCastableForms(DataDexManager, SFAM)" path="/param[@name='sfam']"/>
    /// <inheritdoc cref="TryGetSpeciesID{T}(DataDexManager, SFAM, string, out T)" path="/param[@name='format']"/>
    /// <param name="slots">An the abilities, in slot order, of <paramref name="sfam"/> in <paramref name="format"/>, if found.</param>
    /// <returns>Whether or not an ability slot array was found.</returns>
    public static bool TryGetAbilitySlots<T>(this DataDexManager ddm, SFAM sfam, string format, out T[] slots) where T : notnull
        => SDR(ddm).TryGetSFAMIndexedValue(sfam, ddm.GetIndexChain(format), out slots, "Ability Slots");

    /// <summary>
    /// Checks whether the given <paramref name="sfam"/> has a match in the given <paramref name="format"/>.
    /// </summary>
    /// <inheritdoc cref="GetCastableForms(DataDexManager, SFAM)" path="/param[@name='sfam']"/>
    /// <inheritdoc cref="TryGetSpeciesID{T}(DataDexManager, SFAM, string, out T)" path="/param[@name='format']"/>
    /// <returns>Whether or not <paramref name="sfam"/> has a match.</returns>
    public static bool SFAMExists(this DataDexManager ddm, SFAM sfam, string format)
    {
        foreach (var sfamRoot in SDR(ddm).GetSFAMRoots(sfam))
            if (sfamRoot.TryGetValue(out string[] formats, "Exists in") && Array.Exists(formats, x => x == format))
                return true;
        return false; //no match
    }

    /// <summary>
    /// Searches for an <paramref name="sfam"/> that matches the given species, form,
    /// and appearance indices in the given <paramref name="format"/>.<br/>
    /// Basically a map from a <paramref name="format"/>'s indexing system to the canonical pku one.
    /// </summary>
    /// <typeparam name="T">The type that <paramref name="format"/> uses for its SFA IDs.</typeparam>
    /// <inheritdoc cref="TryGetSpeciesID{T}(DataDexManager, SFAM, string, out T)" path="/param[@name='format']"/>
    /// <param name="speciesID">The index of the species.</param>
    /// <param name="formID">The index of the form, if null it won't be searched for.</param>
    /// <param name="appID">The index of the appearance, if null it won't be searched for.</param>
    /// <param name="isFemale">Whether to use the female value in the case of a gender-split.</param>
    /// <param name="sfam">The <see cref="SFAM"/> corresponding to the given indices in <paramref name="format"/>, if found.</param>
    /// <returns>Whther or not an <paramref name="sfam"/> was found.</returns>
    public static bool TryGetSFAMFromIDs<T>(this DataDexManager ddm, out SFAM sfam, string format,
        T speciesID, T? formID, T? appID, bool isFemale) where T : notnull
    {
        bool doesSFAMMatch(SFAM sfam)
        {
            if (ddm.TryGetSpeciesID(sfam, format, out T searchSpeciesID) && searchSpeciesID.Equals(speciesID)) //species match
            {
                bool formChecks = formID is null || ddm.TryGetFormID(sfam, format, out T searchFormID) && searchFormID.Equals(formID);
                bool appChecks = appID is null || ddm.TryGetAppearanceID(sfam, format, out T searchAppID) && searchAppID.Equals(appID);
                return formChecks && appChecks; //if species, form, and app IDs all checkout then, it must be the right SFAM...?
            }
            return false; //species doesn't even exist
        }

        sfam = new(null!, null!, "", isFemale);
        foreach (var speciesProp in SDR(ddm).EnumerateObject()) //for all species
        {
            sfam.Species = speciesProp.Name;
            if (speciesProp.Value.TryGetValue(out JsonElement formsObj, "Forms")) //for all forms
            {
                foreach (var formProp in formsObj.EnumerateObject())
                {
                    sfam.Form = formProp.Name;
                    if (formProp.Value.TryGetValue(out JsonElement appsObj, "Appearance")) //for all apps
                    {
                        foreach (var appProp in appsObj.EnumerateObject())
                        {
                            sfam.Appearance = appProp.Name;
                            if (doesSFAMMatch(sfam))
                                return true;
                        }
                        sfam.Appearance = ""; //reset app
                    }
                    else if (doesSFAMMatch(sfam)) //no apps, just try form
                        return true;
                }
            }
        }
        return false; //no form matched
    }


    /* ------------------------------------
     * Species Related Enums
     * ------------------------------------
    */
    /// <summary>
    /// An EXP growth type a Pokémon species can have.
    /// Index numbers correspond to those used in the official games.
    /// </summary>
    public enum GrowthRate
    {
        Erratic = 0,
        Fluctuating,
        Medium_Fast,
        Medium_Slow,
        Fast,
        Slow
    }

    /// <summary>
    /// A gender ratio a Pokémon species can have.<br/>
    /// Index numbers correspond to the gender threshold use to determine a Pokémon's gender.
    /// </summary>
    public enum GenderRatio
    {
        All_Male = 0,
        Male_7_Female_1 = 31,
        Male_3_Female_1 = 63,
        Male_1_Female_1 = 127,
        Male_1_Female_3 = 191,
        Male_1_Female_7 = 225,
        All_Female = 254,
        All_Genderless = 255
    }


    /* ------------------------------------
     * SpeciesDex Base Methods
     * ------------------------------------
    */
    private static bool TryGetSFAMValueBase<T>(Func<JsonElement, string[], (bool, JsonElement)> getValFunc,
        JsonElement root, SFAM sfam, out T value, params string[] keys) where T : notnull
    {
        value = default!; //shouldn't use this value if false returned anyway...

        foreach (JsonElement sfamRoot in root.GetSFAMRoots(sfam))
        {
            (bool success, JsonElement valueAsElement) = getValFunc(sfamRoot, keys);
            if (success)
            {
                //check for modified value
                if (valueAsElement.ValueKind is JsonValueKind.Array)
                {
                    var en = valueAsElement.EnumerateArray().ToArray();
                    if (en.Length > 0 && en[0].ValueKind is JsonValueKind.String && en[0].GetString()!.StartsWith("$")) //modifier
                        if (sfam.Modifiers.TryGetValue(en[0].GetString()!, out int index)) //found modifier match
                            valueAsElement = en[index + 1];
                        else //no modifier match, default to index 0
                            valueAsElement = en[0 + 1];
                }

                //try casting value
                try
                {
                    value = valueAsElement.Deserialize<T>()!; //cannot be null because of previous check
                    return true;
                }
                catch { return false; } //type mismatch, return false
            }
        }
        return false; //no matches
    }

    private static bool TryGetSFAMValue<T>(this JsonElement root, SFAM sfam, out T value, params string[] keys) where T : notnull
        => TryGetSFAMValueBase((r, k) => (DexUtil.TryGetValue(r, out JsonElement je, k), je), root, sfam, out value, keys);

    private static bool TryGetSFAMIndexedValue<T>(this JsonElement root, SFAM sfam, List<string> indexNames, out T value, params string[] keys) where T : notnull
        => TryGetSFAMValueBase((r, k) => (DexUtil.TryGetIndexedValue(r, indexNames, out JsonElement je, k), je), root, sfam, out value, keys);

    private static IEnumerable<JsonElement> GetSFAMRoots(this JsonElement root, SFAM sfam)
    {
        if (!root.TryGetValue(out JsonElement speciesRoot, sfam.Species))
            yield break; //species doesn't even exist (in root)

        //if species has any forms
        if (speciesRoot.TryGetValue(out JsonElement formRoot, "Forms", sfam.Form))
        {
            //try appearances
            if (formRoot.TryGetValue(out JsonElement appsRoot, "Appearance"))
            {
                List<string> matchingApps = new();
                foreach (JsonProperty appProp in appsRoot.EnumerateObject())
                {
                    if (MiscUtil.IsPipeSubset(sfam.Appearance, appProp.Name)) //match found
                        matchingApps.Add(appProp.Name);
                }
                matchingApps.Sort(MiscUtil.PipeCompareTo); //stable sort (pipes -> lex)
                foreach (string app in matchingApps)
                    if(appsRoot.TryGetValue(out JsonElement appRoot, app))
                        yield return appRoot;
            }

            //apps failed, try form
            yield return formRoot;
        }

        //form failed, try species
        yield return speciesRoot;
    }
}
