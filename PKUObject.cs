using Newtonsoft.Json;
using System;

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
    public long? PID { get; set; } //Maybe a Hex string in the future? would look better but more oppurtunity for error...

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
        public bool Fateful_Encounter { get; set; } //not nullable, default false

        [JsonProperty("Encounter Type")]
        public string Encounter_Type { get; set; }
    }

    //child of Met_Info_Base
    public partial class Egg_Info_Class : Met_Info_Base
    {
        [JsonProperty("Is Egg")]
        public bool Is_Egg { get; set; } //not nullable, default false

        //TODO
        // gen4  doesnt seem ot use this, instead its implicit from the met date/location in the egg section not being empty...
        //[JsonProperty("Hatched")] //isn't Is_Egg enough? I guess if you knew it was an egg but not when/where it was hatched you'd need this tag...
        //public bool Hatched { get; set; } //not nullable, default false

        // really neccesary?
        //[JsonProperty("Steps Left")]
        //public int? Steps_Left { get; set; }
    }

    public partial class Hyper_Training_Class
    {
        [JsonProperty("HP")]
        public bool HP { get; set; }

        [JsonProperty("Attack")]
        public bool Attack { get; set; }

        [JsonProperty("Defense")]
        public bool Defense { get; set; }

        [JsonProperty("Sp. Attack")]
        public bool Sp_Attack { get; set; }

        [JsonProperty("Sp. Defense")]
        public bool Sp_Defense { get; set; }

        [JsonProperty("Speed")]
        public bool Speed { get; set; }
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
}