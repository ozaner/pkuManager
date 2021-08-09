using Newtonsoft.Json;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace pkuManager.pku
{
    public class pkuObject
    {
        [JsonProperty("Species")]
        public string Species { get; set; }

        [JsonProperty("Forms")]
        public string[] Forms { get; set; }

        [JsonProperty("Appearance")]
        public string[] Appearance { get; set; }

        [JsonProperty("Nickname")]
        public string Nickname { get; set; }

        [JsonProperty("Nickname Flag")]
        public bool? Nickname_Flag { get; set; }

        [JsonProperty("Trash Bytes")]
        public Trash_Bytes_Class Trash_Bytes { get; set; }

        [JsonProperty("True OT")]
        public string True_OT { get; set; }

        [JsonProperty("Level")]
        public int? Level { get; set; }

        [JsonProperty("Gender")]
        public string Gender { get; set; }

        [JsonProperty("EXP")]
        public int? EXP { get; set; }

        [JsonProperty("Item")]
        public string Item { get; set; }

        [JsonProperty("Moves")]
        public Move[] Moves { get; set; }

        [JsonProperty("Pokerus")] //Support accent?
        public Pokerus_Class Pokerus { get; set; }

        [JsonProperty("Shiny Leaf")]
        public string[] Shiny_Leaf { get; set; }

        [JsonProperty("PID")]
        public long? PID { get; set; }

        [JsonProperty("Shiny")]
        public bool? Shiny { get; set; } //not nullable, default false

        [JsonProperty("Nature")]
        public string Nature { get; set; }

        [JsonProperty("Stat Nature")]
        public string Stat_Nature { get; set; }

        [JsonProperty("Ability")]
        public string Ability { get; set; }

        [JsonProperty("Gigantamax Factor")]
        public bool? Gigantamax_Factor { get; set; } //not nullable, default false

        [JsonProperty("Friendship")]
        public int? Friendship { get; set; }

        [JsonProperty("Affection")]
        public int? Affection { get; set; }

        [JsonProperty("Game Info")]
        public Game_Info_Class Game_Info { get; set; }

        [JsonProperty("Catch Info")]
        public Catch_Info_Class Catch_Info { get; set; }

        [JsonProperty("Egg Info")]
        public Egg_Info_Class Egg_Info { get; set; }

        [JsonProperty("IVs")]
        public IVs_Class IVs { get; set; }

        [JsonProperty("Hyper Training")]
        public Hyper_Training_Class Hyper_Training { get; set; }

        [JsonProperty("EVs")]
        public EVs_Class EVs { get; set; }

        [JsonProperty("Contest Stats")]
        public Contest_Stats_Class Contest_Stats { get; set; }

        [JsonProperty("Ribbons")]
        public string[] Ribbons { get; set; }

        [JsonProperty("Markings")]
        public string[] Markings { get; set; }

        [JsonProperty("Shadow Info")]
        public Shadow_Info_Class Shadow_Info { get; set; }

        public partial class Trash_Bytes_Class
        {
            [JsonProperty("Gen")]
            public int Gen { get; set; }

            [JsonProperty("Nickname")]
            public byte[] Nickname { get; set; }

            [JsonProperty("OT")]
            public byte[] OT { get; set; }
        }

        public partial class Move
        {
            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("PP Ups")]
            public int? PP_Ups { get; set; }
        }

        public partial class Game_Info_Class
        {
            [JsonProperty("Origin Game")]
            public string Origin_Game { get; set; }

            // Only if exporting to official format and above fails
            [JsonProperty("Official Origin Game")]
            public string Official_Origin_Game { get; set; }

            [JsonProperty("OT")]
            public string OT { get; set; }

            [JsonProperty("Gender")]
            public string Gender { get; set; }

            // Called the Full Trainer ID (FTID)
            [JsonProperty("ID")]
            public uint? ID { get; set; }

            //[JsonProperty("TID")]
            //public int? TID { get; set; }

            //[JsonProperty("SID")]
            //public int? SID { get; set; }

            [JsonProperty("Language")]
            public string Language { get; set; }
        }

        public partial class Met_Info_Base
        {
            [JsonProperty("Met Location")]
            public string Met_Location { get; set; }

            [JsonProperty("Met Game Override")]
            public string Met_Game_Override { get; set; }

            [JsonProperty("Met Date")]
            public DateTime Met_Date { get; set; }
        }

        //child of Met_Info_Base
        public partial class Catch_Info_Class : Met_Info_Base
        {
            [JsonProperty("Pokeball")]
            public string Pokeball { get; set; }

            [JsonProperty("Met Level")]
            public int? Met_Level { get; set; }

            [JsonProperty("Fateful Encounter")]
            public bool? Fateful_Encounter { get; set; } //not nullable, default false

            [JsonProperty("Encounter Type")]
            public string Encounter_Type { get; set; }
        }

        //child of Met_Info_Base
        public partial class Egg_Info_Class : Met_Info_Base
        {
            [JsonProperty("Is Egg")]
            public bool? Is_Egg { get; set; } //not nullable, default false

            // gen 4 doesn't seem to use this, instead it's implicit from the met date/location in the egg section not being empty...
            //[JsonProperty("Hatched")] //isn't Is_Egg enough? I guess if you knew it was an egg but not when/where it was hatched you'd need this tag...
            //public bool Hatched { get; set; } //not nullable, default false

            // This is actually friendship, so is a seperate value neccesary?
            //[JsonProperty("Steps Left")]
            //public int? Steps_Left { get; set; }
        }

        public partial class Hyper_Training_Class
        {
            [JsonProperty("HP")]
            public bool? HP { get; set; }

            [JsonProperty("Attack")]
            public bool? Attack { get; set; }

            [JsonProperty("Defense")]
            public bool? Defense { get; set; }

            [JsonProperty("Sp. Attack")]
            public bool? Sp_Attack { get; set; }

            [JsonProperty("Sp. Defense")]
            public bool? Sp_Defense { get; set; }

            [JsonProperty("Speed")]
            public bool? Speed { get; set; }
        }

        public partial class IVs_Class
        {
            [JsonProperty("HP")]
            public int? HP { get; set; }

            [JsonProperty("Attack")]
            public int? Attack { get; set; }

            [JsonProperty("Defense")]
            public int? Defense { get; set; }

            [JsonProperty("Sp. Attack")]
            public int? Sp_Attack { get; set; }

            [JsonProperty("Sp. Defense")]
            public int? Sp_Defense { get; set; }

            [JsonProperty("Speed")]
            public int? Speed { get; set; }
        }

        public partial class EVs_Class
        {
            [JsonProperty("HP")]
            public int? HP { get; set; }

            [JsonProperty("Attack")]
            public int? Attack { get; set; }

            [JsonProperty("Defense")]
            public int? Defense { get; set; }

            [JsonProperty("Sp. Attack")]
            public int? Sp_Attack { get; set; }

            [JsonProperty("Sp. Defense")]
            public int? Sp_Defense { get; set; }

            [JsonProperty("Speed")]
            public int? Speed { get; set; }
        }

        public partial class Contest_Stats_Class
        {
            [JsonProperty("Cool")]
            public int? Cool { get; set; }

            [JsonProperty("Beauty")]
            public int? Beauty { get; set; }

            [JsonProperty("Cute")]
            public int? Cute { get; set; }

            [JsonProperty("Clever")]
            public int? Clever { get; set; }

            [JsonProperty("Tough")]
            public int? Tough { get; set; }

            [JsonProperty("Sheen")]
            public int? Sheen { get; set; }
        }

        public partial class Pokerus_Class
        {
            [JsonProperty("Strain")]
            public int? Strain { get; set; }

            [JsonProperty("Days")]
            public int? Days { get; set; }
        }

        public partial class Shadow_Info_Class
        {
            [JsonProperty("Shadow")]
            public bool Shadow { get; set; }

            [JsonProperty("Purified")]
            public bool Purified { get; set; }

            [JsonProperty("Heart Gauge")]
            public int? Heart_Gauge { get; set; }
        }


        /* ------------------------------------
         * Utility Methods
         * ------------------------------------
        */

        public static (pkuObject pku, string error) Deserialize(FileInfo pkuFileInfo)
        {
            string filetext;
            try
            {
                filetext = File.ReadAllText(pkuFileInfo.FullName);
            }
            catch
            {
                return (null, "Not a valid text file.");
            }

            return Deserialize(filetext);
        }

        public static (pkuObject pku, string error) Deserialize(byte[] pkuFile)
        {
            string filetext;
            try
            {
                filetext = System.Text.Encoding.UTF8.GetString(pkuFile, 0, pkuFile.Length);
            }
            catch
            {
                return (null, "Not a valid text file.");
            }

            return Deserialize(filetext);
        }

        public static (pkuObject pku, string error) Deserialize(string pkuJson)
        {
            pkuObject pku = null;
            string errormsg = null;

            try
            {
                pku = JsonConvert.DeserializeObject<pkuObject>(pkuJson);
                if (pku == null)
                    errormsg = "The pku file is empty.";
            }
            catch (Exception ex)
            {
                Match match = Regex.Match(ex.Message, "Path '(.*)'");
                if (match.Groups.Count > 1) // tag mentioned
                {
                    if (ex.Message.Contains("After parsing a value an unexpected character was encountered")) //type is fine but misformatted afterwards
                        errormsg = $"File misformatted after tag \"{match.Groups[1]}\". Perhaps you are missing a comma?";
                    else //tag type wrong
                        errormsg = $"The tag \"{match.Groups[1]}\" does not have the correct type.";
                }
                else //don't know/ no type mentioned
                    errormsg = ex.Message;
            }

            return (pku, errormsg);
        }

        public string Serialize()
        {
            string str = JsonConvert.SerializeObject(this, Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                            });

            return str;
        }

        public pkuObject DeepCopy()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<pkuObject>(serialized);
        }

        /// <returns>Whether the pku is an egg (i.e. <see cref="Egg_Info_Class.Is_Egg"/> is true).</returns>
        public bool IsAnEgg()
        {
            return Egg_Info != null && Egg_Info.Is_Egg == true;
        }

        /// <returns>Whether the pku is a shadow pokemon (i.e. <see cref="Shadow_Info_Class.Shadow"/> is true.</returns>
        public bool IsShadow()
        {
            return Shadow_Info?.Shadow == true;
        }

        /// <returns>Whether the pku is shiny (i.e. <see cref="Shiny"/> is true.</returns>
        public bool IsShiny()
        {
            return Shiny == true;
        }
    }
}