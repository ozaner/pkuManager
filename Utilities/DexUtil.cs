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
            JToken formsCheck = Registry.SPECIES_DEX.TraverseJTokenCaseInsensitive(species, "Forms");
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
        /// Returns the searchable form of a pku's forms array for use in searching through datadexes.
        /// </summary>
        /// <param name="pku">The pku whose form is to be formatted.</param>
        /// <returns>The searchable form name, or the default form if form array is empty or null.</returns>
        public static string GetSearchableForm(this pkuObject pku)
            => pku.Forms?.Length is not > 0 ? GetDefaultForm(pku.Species) : DataUtil.JoinLexical(pku.Forms);

        /// <summary>
        /// Gets a list of all form names the given <paramref name="pku"/> can be casted to, including its current form.<br/>
        /// Null and empty form arrays are treated as default forms.
        /// </summary>
        /// <param name="pku">The pku whose castable forms are to be retrieved.</param>
        /// <returns>A list of all forms the given pku can be casted too.</returns>
        public static List<string> GetCastableForms(pkuObject pku)
        {
            string searchableFormName = pku.GetSearchableForm();
            List<string> castableFormList = new() { searchableFormName };
            castableFormList.AddRange(Registry.SPECIES_DEX.TraverseJTokenCaseInsensitive(
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
            => pku.GetSearchableForm() == GetDefaultForm(pku.Species);

        /// <summary>
        /// Enumerates all the different subsets of appearances
        /// of the given <paramref name="pku"/>'s appearance array.<br/>
        /// The algorithm for deciding the order of each possible subset is like counting backward in binary...
        /// </summary>
        /// <param name="pku">The pku whose appearances are to be enumerated.</param>
        /// <returns>An enumerator of <paramref name="pku"/>'s different appearance combinations.</returns>
        public static IEnumerable<string> GetSearchableAppearances(this pkuObject pku)
        {
            if (pku.Appearance?.Length is not > 0)
            {
                yield return null;
                yield break;
            }

            int effectiveSize = pku.Appearance.Length > 63 ? 64 : pku.Appearance.Length;
            ulong powesize = effectiveSize is 64 ? ulong.MaxValue : ((ulong)1 << effectiveSize)-1;
            for (ulong i = 0; i <= powesize; i++)
            {
                List<string> apps = new();
                for (int j = 0; j < effectiveSize; j++)
                {
                    if ((i & ((ulong)1 << j)) is 0) //reversed 0 and 1 so loop could go form 0 to powsize
                        apps.Add(pku.Appearance[j]);
                }
                yield return DataUtil.JoinLexical(apps.ToArray());
            }
            yield return null; //no appearance
        }

        /// <summary>
        /// Searches the <see cref="Registry.SPECIES_DEX"/> for whether the given
        /// pku's species/form/appearance exists in the given format.
        /// </summary>
        /// <param name="pku">The pku whose species/form is to be searched.</param>
        /// <param name="format">The desired format.</param>
        /// <param name="ignoreCasting">Whether or not to account for form casting.</param>
        /// <returns>Whether or not the given pku's species/form exists in the given format.</returns>
        public static bool ExistsInFormat(this pkuObject pku, string format, bool ignoreCasting = false)
        {
            bool helper(string form, string appearance)
            {
                string[] formats = appearance is null ?
                    Registry.SPECIES_DEX.TraverseJTokenCaseInsensitive(pku.Species, "Forms", form, "Exists in")?.ToObject<string[]>() :
                    Registry.SPECIES_DEX.TraverseJTokenCaseInsensitive(pku.Species, "Forms", form, "Appearance", appearance, "Exists in")?.ToObject<string[]>();
                return formats?.Contains(format) is true;
            }

            var apps = pku.GetSearchableAppearances();
            foreach(string app in apps)
            {
                if (ignoreCasting ? helper(pku.GetSearchableForm(), app) : GetCastableForms(pku).Any(form => helper(form, app)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the given species' index number in the given format,
        /// according to <see cref="Registry.SPECIES_DEX"/>.
        /// </summary>
        /// <param name="species">The name of the species.</param>
        /// <param name="format">The format the index corresponds to.</param>
        /// <returns>The index number of <paramref name="species"/> in <paramref name="format"/>.</returns>
        public static int? GetSpeciesIndex(string species, string format)
            => (int?)Registry.SPECIES_DEX.TraverseJTokenCaseInsensitive(species, "Indices", format);
    }
}
