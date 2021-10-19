using Newtonsoft.Json.Linq;
using pkuManager.Common;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace pkuManager.Formats.showdown
{
    /// <summary>
    /// An implementation of the .txt (Showdown!) format used by Pokémon Showdown!.
    /// </summary>
    public class ShowdownObject : FormatObject
    {
        /* ------------------------------------
         * Attributes
         * ------------------------------------
        */
        public string ShowdownName { get; set; }
        public string Nickname { get; set; }
        public string Item { get; set; }
        public string Ability { get; set; }
        public string[] Moves { get; set; }
        public byte Level { get; set; }
        public byte Friendship { get; set; }
        public byte[] IVs { get; set; }
        public byte[] EVs { get; set; }
        public Gender? Gender { get; set; }
        public Nature? Nature { get; set; }
        public bool Shiny { get; set; }
        public bool Gigantamax_Factor { get; set; }
        // PP Ups not used in Showdown, by default always max PP
        // Found one thread regarding it but nothing seems to have come of it:
        // https://www.smogon.com/forums/threads/allow-moves-to-have-non-max-pp.3653621/

        /// <summary>
        /// A list of strings that will be added to the final .txt file upon calling <see cref="ToFile"/>.
        /// </summary>
        protected List<string> Lines = new();
        
        /// <summary>
        /// Compiles each of the object's attributes to strings which are added to <see cref="Lines"/>.
        /// </summary>
        protected virtual void CompileLines()
        {
            string introLine = "";

            // Species/Nickname
            if (Nickname is null || Nickname == ShowdownName)
                introLine += ShowdownName;
            else
                introLine += $"{Nickname} ({ShowdownName})";

            // Gender
            introLine += Gender switch
            {
                Common.Gender.Male => " (M)",
                Common.Gender.Female => " (F)",
                _ => ""
            };

            // Item
            if (Item is not null)
                introLine += $" @ {Item}";

            Lines.Add(introLine);

            // Ability
            if (Ability is not null)
                Lines.Add($"Ability: {Ability}");

            // Level
            if (Level is not 100)
                Lines.Add($"Level: {Level}");

            // Shiny (no preprocessing)
            if (Shiny)
                Lines.Add("Shiny: true");

            // Friendship
            if (Friendship is not 255)
                Lines.Add($"Happiness: {Friendship}");

            // IVs
            if (!IVs.All(x => x is 31))
            {
                string ivs = $"IVs: {(IVs[0] is not 31 ? $"{IVs[0]} HP / " : "")}{(IVs[1] is not 31 ? $"{IVs[1]} Atk / " : "")}" +
                              $"{(IVs[2] is not 31 ? $"{IVs[2]} Def / " : "")}{(IVs[3] is not 31 ? $"{IVs[3]} SpA / " : "")}" +
                              $"{(IVs[4] is not 31 ? $"{IVs[4]} SpD / " : "")}{(IVs[5] is not 31 ? $"{IVs[5]} Spe / " : "")}";
                ivs = ivs[0..^3]; //remove extra " / "
                Lines.Add(ivs);
            }

            // EVs
            if (!EVs.All(x => x is 0))
            {
                string evs = $"EVs: {(EVs[0] is not 0 ? $"{EVs[0]} HP / " : "")}{(EVs[1] is not 0 ? $"{EVs[1]} Atk / " : "")}" +
                              $"{(EVs[2] is not 0 ? $"{EVs[2]} Def / " : "")}{(EVs[3] is not 0 ? $"{EVs[3]} SpA / " : "")}" +
                              $"{(EVs[4] is not 0 ? $"{EVs[4]} SpD / " : "")}{(EVs[5] is not 0 ? $"{EVs[5]} Spe / " : "")}";
                evs = evs[0..^3]; //remove extra " / "
                Lines.Add(evs);
            }

            // Nature
            if (Nature.HasValue)
                Lines.Add($"{Nature} Nature");

            // Gigantamax
            if (Gigantamax_Factor)
                Lines.Add("Gigantamax: Yes");

            // Moves
            foreach (string move in Moves)
                Lines.Add($"- {move}");
        }

        public override byte[] ToFile()
        {
            CompileLines();
            string txt = string.Join("\n", Lines);
            return Encoding.UTF8.GetBytes(txt);
        }

        public override void FromFile(byte[] file)
        {
            throw new NotImplementedException();
        }


        /* ------------------------------------
         * Read Showdown Data
         * ------------------------------------
        */
        /// <summary>
        /// A datadex of all the items, moves, abilities, and Gmax species that appear in Showdown's database.
        /// </summary>
        protected static readonly JObject SHOWDOWN_BATTLE_DATA = DataUtil.GetJson("ShowdownData");

        /// <summary>
        /// Searches the <see cref="Registry.SPECIES_DEX"/> for the
        /// Showdown name of a given pku's species/form/appearance.
        /// </summary>
        /// <param name="pku">The pku whose Showdown name is to be determined.</param>
        /// <returns><paramref name="pku"/>'s Showdown name.</returns>
        public static string GetShowdownName(pkuObject pku)
        {
            var apps = pku.GetSearchableAppearances();
            foreach (string app in apps)
            {
                List<string> searchTerms = new()
                {
                    pku.Species,
                    "Forms",
                    pku.GetSearchableForm()
                };
                if (app is not null)
                {
                    searchTerms.Add("Appearance");
                    searchTerms.Add(app);
                }
                bool? genderSplit = (bool?)Registry.SPECIES_DEX.TraverseJTokenCaseInsensitive(searchTerms.Append("Showdown Gender Split").ToArray());
                if (genderSplit is true)
                {
                    Gender? gender = pku.Gender.ToEnum<Gender>();
                    string genderStr = gender is Common.Gender.Female ? "Female" : "Male"; //Default is male
                    searchTerms.Add($"Showdown Name {genderStr}");
                }
                else
                    searchTerms.Add("Showdown Name");

                string sdName = (string)Registry.SPECIES_DEX.TraverseJTokenCaseInsensitive(searchTerms.ToArray());
                if (sdName is not null)
                    return sdName;
            }
            return null; //no match
        }

        /// <summary>
        /// Helper method to check if some <paramref name="datum"/> exists in
        /// the <paramref name="type"/> list in <see cref="SHOWDOWN_BATTLE_DATA"/>.
        /// </summary>
        /// <param name="type">The type of <paramref name="datum"/> to check
        ///                    (i.e. "items", "moves", "abilities, and "gmax").</param>
        /// <param name="datum">The datum to be checked for in the <paramref name="type"/> list.</param>
        /// <returns>Whether the given <paramref name="datum"/> exists in <see cref="SHOWDOWN_BATTLE_DATA"/>
        ///          (in the <paramref name="type"/> list) or not.</returns>
        protected static bool IsDatumValid(string type, string datum)
        {
            if (datum is null)
                return true;

            return Array.Exists(SHOWDOWN_BATTLE_DATA.TraverseJTokenCaseInsensitive(type).ToObject<string[]>(),
                x => x.ToLowerInvariant() == datum.ToLowerInvariant());
        }

        /// <summary>
        /// Whether the given item is in the Showdown! database.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>Whether the item is valid or not.</returns>
        public static bool IsItemValid(string item)
        {
            return IsDatumValid("items", item);
        }

        /// <summary>
        /// Whether the given move is in the Showdown! database.
        /// </summary>
        /// <param name="move">The move to check.</param>
        /// <returns>Whether the move is valid or not.</returns>
        public static bool IsMoveValid(string move)
        {
            return IsDatumValid("moves", move);
        }

        /// <summary>
        /// Whether the given ability is in the Showdown! database.
        /// </summary>
        /// <param name="ability">The ability to check.</param>
        /// <returns>Whether the ability is valid or not.</returns>
        public static bool IsAbilityValid(string ability)
        {
            return IsDatumValid("abilities", ability);
        }

        /// <summary>
        /// Whether the given species has a gigantamax form in the Showdown! database.
        /// </summary>
        /// <param name="species">The species to check.</param>
        /// <returns>Whether the species is Gigantamaxable or not.</returns>
        public static bool IsGMaxValid(string species)
        {
            return IsDatumValid("gmax", species);
        }


        /* ------------------------------------
         * Update Showdown Battle Data
         * ------------------------------------
        */
        private static readonly string GithubDexData = @"https://raw.githubusercontent.com/smogon/pokemon-showdown/master/data/pokedex.ts";
        private static readonly string GithubTextData = @"https://raw.githubusercontent.com/smogon/pokemon-showdown/master/data/text/";
        private static readonly string MoveFile = "moves.ts", AbilityFile = "abilities.ts", ItemFile = "items.ts";

        private static JObject ParseJSONFromShowdownRepo(string url)
        {
            // Download file to string
            string fileString;
            using WebClient client = new();
            fileString = client.DownloadString(url);

            // Convert typescript constant declaration to JSON:

            // Remove first line of text and replace it with '{'
            fileString = '{' + fileString[(fileString.IndexOf("\n") + 1)..];

            // Remove last line of text and add a '}' to the end
            fileString = fileString.Remove(fileString.TrimEnd().LastIndexOf("\n")) + '}';

            //Create final trimmed dex object
            JObject fileObject = JObject.Parse(fileString);

            return fileObject;
        }
        private static List<string> PullShowdownGMaxSpecies()
        {
            JObject fileObject = ParseJSONFromShowdownRepo(GithubDexData);
            List<string> listOfNames = new();
            foreach (var c in fileObject)
            {
                if (c.Value.TraverseJTokenCaseInsensitive("canGigantamax") is not null) //has GMax
                {
                    listOfNames.Add((string)c.Value.TraverseJTokenCaseInsensitive("name"));
                    string[] cosmeticForms = c.Value.TraverseJTokenCaseInsensitive("cosmeticFormes")?.ToObject<string[]>();
                    if (cosmeticForms?.Length > 0)
                        listOfNames.AddRange(cosmeticForms);
                }
            }
            listOfNames.RemoveAll(x => x is null); //get rid of null entries (there shouldn't be any but just in case)
            return listOfNames;
        }
        private static List<string> PullShowdownDataHelper(string url)
        {
            JObject fileObject = ParseJSONFromShowdownRepo(url);
            List<string> listOfNames = new();
            foreach (var c in fileObject)
                listOfNames.Add((string)c.Value.TraverseJTokenCaseInsensitive("name"));
            return listOfNames;
        }

        /// <summary>
        /// Pulls the current item, move, and ability data from the 
        /// <see href="https://github.com/smogon/pokemon-showdown/tree/master/data/text">Showdown Github</see>.
        /// </summary>
        /// <returns>A JObject with data on the valid moves, abilities, items, and gmax species in Showdown.</returns>
        public static JObject PullShowdownData()
        {
            List<string> moves = PullShowdownDataHelper(GithubTextData + MoveFile);
            List<string> items = PullShowdownDataHelper(GithubTextData + ItemFile);
            List<string> abilities = PullShowdownDataHelper(GithubTextData + AbilityFile);
            List<string> gmax = PullShowdownGMaxSpecies();
            JObject showdownData = new()
            {
                { "moves", JToken.FromObject(moves) },
                { "items", JToken.FromObject(items) },
                { "abilities", JToken.FromObject(abilities) },
                { "gmax", JToken.FromObject(gmax) }
            };
            return showdownData;
        }
    }
}
