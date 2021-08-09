using Newtonsoft.Json.Linq;
using pkuManager.pku;
using System;
using System.Collections.Generic;

namespace pkuManager.Utilities
{
    public static class DexUtil
    {
        public static string GetDefaultForm(pkuObject pku)
        {
            return GetDefaultForm(pku.Species, Registry.MASTER_DEX);
        }

        public static string GetDefaultForm(pkuObject pku, JObject data)
        {
            return GetDefaultForm(pku.Species, data);
        }

        /// <summary>
        ///  Given a pku and some species data (in the form of a json), will return the default form of the given pku's species.
        ///  If the default form is unnamed (e.g. Rotom) then the empty string will be returned.
        /// </summary>
        /// <param name="data">The json data of all the relevant species.</param>
        /// <returns>The default form for this species or, if none found, the empty string.</returns>
        public static string GetDefaultForm(string species, JObject data)
        {
            //Species must be defined
            if (species == null)
                return null;

            JToken formsCheck = DataUtil.TraverseJTokenCaseInsensitive(data, species, "Forms");
            if (formsCheck == null)
                return ""; // No listed forms, default is just "" (i.e. base form)

            JObject forms = formsCheck is JObject ? (JObject)formsCheck : throw new ArgumentException("Invalid Data, Forms should be a JObject.");

            foreach (var j in forms)
            {
                bool? isDefault = (bool?)DataUtil.TraverseJTokenCaseInsensitive(j.Value, "Default");
                if (isDefault == true) //"default" exists and is true
                    return j.Key; //default form found
            }
            return ""; // There are forms, but no listed default. Default is just "" (i.e. base form)
        }

        /// <summary>
        /// Returns a concatenated string of the given pku's form array for use in searching through datadexes.
        /// </summary>
        /// <param name="pku">The pku whose form is to be formatted.</param>
        /// <returns>The searchable form name, or <see langword="null"/> if form array is empty or null.</returns>
        public static string GetSearchableFormName(pkuObject pku)
        {
            return GetSearchableFormName(pku.Forms);
        }

        /// <summary>
        /// Returns a concatenated string of the given form array for use in searching through datadexes.
        /// </summary>
        /// <param name="forms">An array of form names to combine.</param>
        /// <returns>The searchable form name, or <see langword="null"/> if form array is empty or null.</returns>
        public static string GetSearchableFormName(string[] forms)
        {
            if (forms == null || forms.Length == 0)
                return null;

            string searchableForm = "";
            string[] formsSorted = (string[])forms.Clone();
            Array.Sort(formsSorted, StringComparer.OrdinalIgnoreCase);
            foreach (string form in formsSorted)
                searchableForm += form + "-";

            return searchableForm.Remove(searchableForm.Length - 1);
        }

        /// <summary>
        /// Returns whether the given pku's species+form exists in the given datadex.
        /// </summary>
        /// <param name="pku">The pku whose species+form is to be checked.</param>
        /// <param name="datadex">A datadex of relevant species and forms.</param>
        /// <returns></returns>
        public static bool FormExists(pkuObject pku, JObject datadex)
        {
            return FormExists(pku.Species, GetSearchableFormName(pku.Forms), datadex);
        }

        public static bool FormExists(string species, string searchableForm, JObject datadex)
        {
            JToken searchResult = DataUtil.TraverseJTokenCaseInsensitive(datadex, species, "Forms", searchableForm);
            return searchResult != null;
        }

        /// <summary>
        /// Gets a list of all form names the given pku can be casted to (including its specified form) according to the <seealso cref="Registry.MASTER_DEX"/>.
        /// Null and empty form arrays are treated as default forms.
        /// </summary>
        /// <param name="pku">The pku whose castable forms are to be returned.</param>
        /// <returns></returns>
        public static List<string> GetCastableForms(pkuObject pku)
        {
            string searchableFormName = GetSearchableFormName(pku) ?? GetDefaultForm(pku);
            List<string> castableFormList = new List<string> { searchableFormName };
            castableFormList.AddRange(DataUtil.TraverseJTokenCaseInsensitive(
                Registry.MASTER_DEX, pku.Species, "Forms", searchableFormName, "Castable to"
            )?.ToObject<List<string>>() ?? new List<string>());
            return castableFormList;
        }

        public static bool IsFormDefault(pkuObject pku)
        {
            string searchableFormName = pku.Forms == null ? GetDefaultForm(pku) : GetSearchableFormName(pku);
            return searchableFormName == GetDefaultForm(pku);
        }

        /// <summary>
        /// Returns whether or not the given pku can be casted to a form in the given datadex.
        /// </summary>
        /// <param name="pku">The pku whose form is to be examined.</param>
        /// <param name="datadex">A datadex of valid form casts for eahc species+form combo.</param>
        /// <returns>Whether or not the given pku can be casted to a form in the given datadex.</returns>
        public static bool CanCastPKU(pkuObject pku, JObject datadex)
        {
            List<string> castableForms = GetCastableForms(pku); //all castable forms according to masterdex
            foreach (string form in castableForms)
            {
                if (FormExists(pku.Species, form, datadex)) //valid form
                    return true; //has at least one valid form, move on
            }
            return false; //went through all forms none were valid
        }
    }
}
