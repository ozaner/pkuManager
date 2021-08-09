using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows.Forms;

public class PKUObject
{
    [JsonProperty("Species")]
    public string Species { get; set; }

    [JsonProperty("Form")]
    public string Form { get; set; }
    //public Type FormType { get; set; }

    [JsonProperty("Nickname")]
    public string Nickname { get; set; }

    [JsonProperty("Nicknamed")] //TODO: DEAL WITH THIS LATER
    public bool Nicknamed { get; set; } //not nullable, default false

    [JsonProperty("True OT")]
    public string True_OT { get; set; }

    [JsonProperty("Level")]
    public int? Level { get; set; }

    [JsonProperty("Gender")]
    public string Gender { get; set; }

    [JsonProperty("EXP")]
    public uint? EXP { get; set; }

    [JsonProperty("Item")]
    public string Item { get; set; }

    [JsonProperty("Pokerus")] //Support accent?
    public Pokerus_Class Pokerus { get; set; }

    [JsonProperty("Shiny Leaf")]
    public string[] Shiny_Leaf { get; set; }

    [JsonProperty("PID")]
    public uint? PID { get; set; } //Maybe a Hex string in the future? would look better but more oppurtunity for error...

    [JsonProperty("Shiny")]
    public bool Shiny { get; set; } //not nullable, default false

    [JsonProperty("Nature")]
    public string Nature { get; set; }

    [JsonProperty("Stat Nature")]
    public string Stat_Nature { get; set; }

    [JsonProperty("Ability")]
    public string Ability { get; set; }

    [JsonProperty("Gigantamax Factor")]
    public bool Gigantamax_Factor { get; set; } //not nullable, default false

    //Can do inference for this
    //[JsonProperty("Ability Slot")]
    //public string Ability_Slot { get; set; }
    //public bool Ability_SlotSpecified { get; set; }

    [JsonProperty("HT Friendship")]
    public int? HT_Friendship { get; set; }

    [JsonProperty("OT Friendship")]
    public int? OT_Friendship { get; set; }

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

    [JsonProperty("EVs")]
    public EVs_Class EVs { get; set; }

    [JsonProperty("Contest")]
    public Contest_Class Contest { get; set; }

    [JsonProperty("Moves")]
    public Moves_Class Moves { get; set; }

    [JsonProperty("PP Ups")]
    public PP_Ups_Class PP_Ups { get; set; }

    [JsonProperty("Ribbons")]
    public string[] Ribbons { get; set; }

    [JsonProperty("Markings")]
    public string[] Markings { get; set; }

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

        [JsonProperty("TID")]
        public int? TID { get; set; }

        [JsonProperty("SID")]
        public int? SID { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }
    }

    public partial class Catch_Info_Class
    {
        [JsonProperty("Pokeball")]
        public string Pokeball { get; set; }

        [JsonProperty("Met Level")]
        public int? Met_Level { get; set; }

        [JsonProperty("Met Location")]
        public string Met_Location { get; set; }

        [JsonProperty("Met Date")]
        public DateTime Met_Date { get; set; }

        [JsonProperty("Fateful Encounter")]
        public bool Fateful_Encounter { get; set; } //not nullable, default false

        [JsonProperty("Encounter Type")]
        public string Encounter_Type { get; set; }
    }

    public partial class Egg_Info_Class
    {
        [JsonProperty("Is Egg")]
        public bool Is_Egg { get; set; } //not nullable, default false

        //TODO
        //[JsonProperty("Hatched")] //isn't Is_Egg enough? I guess if you knew it was an egg but not when/where it was hatched you'd need this tag...
        //public bool Hatched { get; set; } //not nullable, default false

        [JsonProperty("Met Location")]
        public string Met_Location { get; set; }

        [JsonProperty("Met Date")]
        public DateTime Met_Date { get; set; }
    }

    public partial class Moves_Class
    {
        [JsonProperty("1")]
        public string Move1 { get; set; }

        [JsonProperty("2")]
        public string Move2 { get; set; }

        [JsonProperty("3")]
        public string Move3 { get; set; }

        [JsonProperty("4")]
        public string Move4 { get; set; }
    }

    public partial class PP_Ups_Class
    {
        [JsonProperty("1")]
        public int? PP_Up_1 { get; set; }

        [JsonProperty("2")]
        public int? PP_Up_2 { get; set; }

        [JsonProperty("3")]
        public int? PP_Up_3 { get; set; }

        [JsonProperty("4")]
        public int? PP_Up_4 { get; set; }
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

    public partial class Contest_Class
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
}