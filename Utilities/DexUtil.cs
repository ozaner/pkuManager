using Newtonsoft.Json.Linq;
using pkuManager.pku;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace pkuManager.Utilities
{
    public static class DexUtil
    {
        /// <summary>
        /// Compiles a master datadex from the given <paramref name="url"/>.
        /// </summary>
        /// <param name="url">A raw url to an uncompiled master datadex file.</param>
        /// <returns>The compiled master datadex from the <paramref name="url"/>.</returns>
        public static JObject GetMasterDatadex(string url)
        {
            JObject masterDatadex = new();
            WebClient client = new();
            try
            {
                //Download the uncompiled master-datadex.json
                JObject uncompiledJSON = JObject.Parse(client.DownloadString(url));

                foreach (var kvp in uncompiledJSON)
                {
                    try
                    {
                        //Download datadex.json
                        JObject datadexJSON = JObject.Parse(client.DownloadString((string)kvp.Value));
                        masterDatadex = DataUtil.GetCombinedJson(true, masterDatadex, datadexJSON);
                    }
                    catch
                    {
                        Debug.WriteLine($"Failed to read {kvp.Key} datadex...");
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Failed to read a master datadex...");
            }
            return masterDatadex;
        }

        /// <summary>
        /// Searches for the the default form of the given species.<br/>
        /// If the default form is unnamed (e.g. Bulbasaur) then the empty string will be returned.
        /// </summary>
        /// <param name="species">The species whose default form is to be retrieved.</param>
        /// <returns>The default form for this species or, if none found, the empty string.</returns>
        public static string GetDefaultForm(string species)
        {
            //Species must be defined
            if (species is null)
                return null;

            JToken formsCheck = Registry.MASTER_DEX.TraverseJTokenCaseInsensitive(species, "Forms");
            if (formsCheck is null)
                return ""; // No listed forms, default is just "" (i.e. base form)

            JObject forms = formsCheck is JObject ? (formsCheck as JObject) : throw new ArgumentException("Invalid Data, Forms should be a JObject.");

            foreach (var j in forms)
            {
                bool? isDefault = (bool?)j.Value.TraverseJTokenCaseInsensitive("Default");
                if (isDefault is true) //"default" exists and is true
                    return j.Key; //default form found
            }
            return ""; // There are forms, but no listed default. Default is just "" (i.e. base form)
        }

        /// <summary>
        /// Returns a concatenated string of the given form array for use in searching through datadexes.
        /// </summary>
        /// <param name="forms">An array of form names to combine.</param>
        /// <returns>The searchable form name, or <see langword="null"/> if form array is empty or null.</returns>
        public static string GetSearchableFormName(string[] forms)
        {
            if (forms?.Length is not > 0)
                return null;

            string searchableForm = "";
            string[] formsSorted = (string[])forms.Clone();
            Array.Sort(formsSorted, StringComparer.OrdinalIgnoreCase);
            foreach (string form in formsSorted)
                searchableForm += form + "|";

            return searchableForm.Remove(searchableForm.Length - 1);
        }

        /// <param name="pku">The pku whose form is to be formatted.</param>
        /// <inheritdoc cref="GetSearchableFormName(string[])"/>
        public static string GetSearchableFormName(pkuObject pku)
            => GetSearchableFormName(pku.Forms);

        /// <summary>
        /// Checks a <paramref name="datadex"/> for the given
        /// <paramref name="species"/> + <paramref name="searchableForm"/>.
        /// </summary>
        /// <param name="species">The species to be checked.</param>
        /// <param name="searchableForm">The form to be checked.</param>
        /// <param name="datadex">A datadex of relevant species and forms.</param>
        /// <returns>Whether the given <paramref name="species"/> + <paramref name="searchableForm"/>
        ///          exists in <paramref name="datadex"/>.</returns>
        public static bool FormExists(string species, string searchableForm, JObject datadex)
            => datadex.TraverseJTokenCaseInsensitive(species, "Forms", searchableForm) is not null;

        /// <summary>
        /// Gets a list of all form names the given pku can be casted to, including its current form.<br/>
        /// Null and empty form arrays are treated as default forms.
        /// </summary>
        /// <param name="pku">The pku whose castable forms are to be retrieved.</param>
        /// <returns>A list of all forms the given pku can be casted too.</returns>
        public static List<string> GetCastableForms(pkuObject pku)
        {
            string searchableFormName = GetSearchableFormName(pku) ?? GetDefaultForm(pku.Species);
            List<string> castableFormList = new() { searchableFormName };
            castableFormList.AddRange(Registry.MASTER_DEX.TraverseJTokenCaseInsensitive(
                pku.Species, "Forms", searchableFormName, "Castable to"
            )?.ToObject<List<string>>() ?? new List<string>());
            return castableFormList;
        }

        /// <summary>
        /// Checks if the given <paramref name="pku"/>'s form is the default for its species.
        /// </summary>
        /// <param name="pku">The pku to check.</param>
        /// <returns>Whether or not <paramref name="pku"/> has its default form.</returns>
        public static bool IsFormDefault(this pkuObject pku)
            => pku.Forms?.Length is not > 0 || GetSearchableFormName(pku) == GetDefaultForm(pku.Species);

        /// <summary>
        /// Checks if the given <paramref name="pku"/> can be casted to a form in the given <paramref name="datadex"/>.
        /// </summary>
        /// <param name="pku">The pku whose form is to be examined.</param>
        /// <param name="datadex">A datadex of valid form casts for each species + form combo.</param>
        /// <returns>Whether or not <paramref name="pku"/> can be casted to a form in <paramref name="datadex"/>.</returns>
        public static bool CanCastPKU(pkuObject pku, JObject datadex)
            => GetCastableForms(pku).Any(form => FormExists(pku.Species, form, datadex));
    }
}
