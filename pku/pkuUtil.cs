using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pkuManager.Utilities;
using System;
using System.IO;

namespace pkuManager.pku
{
    public static class pkuUtil
    {
        // Imports a pku from a fileinfo object.
        // Includes all tags used in the supported formats.
        public static PKUObject ImportPKU(FileInfo pkuFile)
        {
            string pkuString = File.ReadAllText(pkuFile.FullName);
            PKUObject pku = JsonConvert.DeserializeObject<PKUObject>(pkuString);
            return pku;
        }

        // Imports a pku from a byte[].
        // Includes all tags used in the supported formats.
        public static PKUObject ImportPKU(byte[] pkuFile)
        {
            string pkuString = System.Text.Encoding.UTF8.GetString(pkuFile, 0, pkuFile.Length);
            PKUObject pku = JsonConvert.DeserializeObject<PKUObject>(pkuString);
            return pku;
        }

        /// <summary>
        ///  Given a pku and some species data (in the form of a json), will return the default form of the given pku's species.
        ///  If the default form is unnamed (e.g. Rotom) then the empty string will be returned.
        /// </summary>
        /// <param name="pku">The pku whose species will be checked.</param>
        /// <param name="data">The json data of all the relevant species.</param>
        /// <returns>The default form for this species or, if none found, the empty string.</returns>
        public static string getDefaultForm(string species, JObject data)
        {
            //Species must be defined
            if (species == null)
                throw new ArgumentException("Expected a pku with a species.");

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
        /// Returns whether the given pku is currently an egg.
        /// </summary>
        /// <param name="pku">The pku to check.</param>
        /// <returns></returns>
        public static bool IsAnEgg(PKUObject pku)
        {
            return pku.Egg_Info != null && pku.Egg_Info.Is_Egg;
        }

        /// <summary>
        /// Returns whether the given pku is currently a shadow pokemon.
        /// </summary>
        /// <param name="pku">The pku to check.</param>
        /// <returns></returns>
        public static bool IsShadow(PKUObject pku)
        {
            return pku.Shadow_Info?.Shadow == true;
        }
    }
}
