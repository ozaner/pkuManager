using Newtonsoft.Json.Linq;
using System;

namespace pkuManager.Exporters
{
    public static class pkCommons
    {
        public static JObject POKESTAR_DATA = Utilities.getJson("pokestarData");
        public static JObject NATIONALDEX_DATA = Utilities.getJson("nationaldexData");
        public static readonly string FILENAME_ABILITIES = "Abilities";
        public static readonly string FILENAME_NATURES = "Natures";
        public static readonly string FILENAME_MOVES = "Moves";

        // Returns the national dex # of the given pokemon species
        // Returns null if species is not in the nat dex
        public static int? GetNationalDex(PKUObject pku)
        {
            return (int?)NATIONALDEX_DATA?[pku.Species]?["National Dex"];
        }

        // Returns ID of the given nature, null if nature is unofficial
        public static int? GetNatureID(string nature)
        {
            int val = ReadingUtil.getIDFromFile(FILENAME_NATURES, nature);
            if (val == -1)
                return null;
            return val;
        }

        // Returns ID of the given move, null if move is unofficial
        public static int? GetMoveID(string move)
        {
            int val = ReadingUtil.getIDFromFile(FILENAME_MOVES, move);
            if (val == -1)
                return null;
            return val;
        }

        // Returns ID of the given ability, null if ability is unofficial, 0 if ability is None
        public static int? GetAbilityID(string ability)
        {
            int val = ReadingUtil.getIDFromFile(FILENAME_ABILITIES, ability);
            if (val == -1)
                return null;
            return val;
        }

        // Returns ID of given gender
        // Returns -1 if gender is invalid. (genderless counts as invalid if isTrainer)
        public static Gender? GetGenderID(string gender, bool isTrainer)
        {
            if (gender == null)
                return null;
            switch (gender.ToLower())
            {
                case "male":
                case "m":
                    return Gender.Male;
                case "female":
                case "f":
                    return Gender.Female;
                case "genderless":
                    return isTrainer ? (Gender?)null : Gender.Genderless; // only pokemon can be genderless
                default:
                    return null; //invalid gender
            }
        }

        // Returns ID of the given game version, using Origin Game and then Official Origin Game if that fails.
        // Returns null if no ID is found
        public static int? GetGameID(PKUObject pku)
        {
            int? id = ReadingUtil.getIDFromFile("Games", pku.Game_Info?.Origin_Game); //try getting id from origin game
            id ??= ReadingUtil.getIDFromFile("Games", pku.Game_Info?.Official_Origin_Game); // if id is null try official origin game
            return id;

            //if (gen == 3 && (id > 5 || id != 15)) //Game ID doesn't exist in gen 3
            //    return 0;
            //if (gen == 4 && id > 15) //Game ID doesn't exist in gen 4
            //    return 0;
            //if (gen == 5 && id > 23) //Game ID doesn't exist in gen 5
            //    return 0;
            //if (gen == 6 && id > 27) //Game ID doesn't exist in gen 6
            //    return 0;
            //if (gen == 7 && id > 41) //Game ID doesn't exist in gen 7
            //    return 0;
            //if (gen == 8 && id > 45) //Game ID doesn't exist in gen 8
            //    return 0;
        }

        // Returns: (int?, bool)
        // int?: The gen 5 id of a pokestar species, null if it's not a pokestar species
        // bool: Whether the form was an invalid pokestar form (will give the default species id in this case)
        //       Will be false if species isn't even valid.
        public static (int?, bool) GetPokestarID(PKUObject pku)
        {
            string species = pku.Species;
            string form = pku.Form;
            if (species == null)
                return (null, false);

            bool forminvalid = false;
            int? gen5ID = null;

            // Try getting the spcies-form ID
            if(form != null)
                gen5ID = (int?)POKESTAR_DATA[species]?["Forms"]?[form]?["Gen 5 Index"];

            // If form is invalid (i.e. above didn't work) then just get default form id
            if (!gen5ID.HasValue)
            {
                string defaultForm = Utilities.getDefaultForm(pku, POKESTAR_DATA);
                gen5ID = (int?)POKESTAR_DATA[species]?["Forms"]?[defaultForm]?["Gen 5 Index"];
            }
            return (gen5ID, forminvalid);
        }

        // Returns whether or not this pku has visual gender diffrences in the given gen
        public static bool hasGenderDifferences(PKUObject pku, int gen)
        {
            if (gen > 4 || gen < 1)
                throw new Exception("This method is only implemented for gens 1-4");

            if (gen < 4 || pku.Species == null)
                return false; //Gender differences were not implemented until gen 4

            bool? gdSearch = (bool?)NATIONALDEX_DATA[pku.Species]?["Gender Differences"];
            bool gd = gdSearch.HasValue && gdSearch.Value;
            if (gen == 4)
            {
                if (GetNationalDex(pku) == 133) 
                    return false; // Eevee is a special case, only got gender differences in gen 8+ (also starter in LGPE)
                return gd; //No other special cases
            }

            throw new Exception("This line should never have been reached"); //Should never reach this point
        }

        // Returns whether or not the given pku is an egg
        public static bool isAnEgg(PKUObject pku)
        {
            return pku.Egg_Info != null && pku.Egg_Info.Is_Egg;
        }
    }
}
