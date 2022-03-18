using Json.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace pkuManager.Formats.pku;

[JsonObject(MemberSerialization.OptIn)]
public class pkuObject : FormatObject
{
    public override string FormatName => "pku";

    [JsonProperty("Species")]
    public string Species { get; set; }

    [JsonProperty("Nickname")]
    public string Nickname { get; set; }

    [JsonProperty("Nickname Flag")]
    public bool? Nickname_Flag { get; set; }

    [JsonProperty("True OT")]
    public BackedField<string> True_OT { get; set; } = new();

    [JsonProperty("Forms")]
    public BackedField<string[]> Forms { get; set; } = new();

    [JsonProperty("Appearance")]
    public string[] Appearance { get; set; }

    [JsonProperty("Gender")]
    public string Gender { get; set; }

    [JsonProperty("Level")]
    public int? Level { get; set; }

    [JsonProperty("EXP")]
    public int? EXP { get; set; }

    [JsonProperty("Item")]
    public BackedField<string> Item { get; set; } = new();

    [JsonProperty("Moves")]
    public Move[] Moves { get; set; }

    [JsonProperty("PID")]
    public uint? PID { get; set; }

    [JsonProperty("Shiny")]
    public bool? Shiny { get; set; }

    [JsonProperty("Nature")]
    public BackedField<string> Nature { get; set; } = new();

    [JsonProperty("Stat Nature")]
    public BackedField<string> Stat_Nature { get; set; } = new();

    [JsonProperty("Ability")]
    public string Ability { get; set; }

    [JsonProperty("Gigantamax Factor")]
    public bool? Gigantamax_Factor { get; set; }

    [JsonProperty("Friendship")]
    public BackedField<BigInteger?> Friendship { get; set; } = new();

    [JsonProperty("Affection")]
    public int? Affection { get; set; }

    [JsonProperty("Game Info")]
    public Game_Info_Class Game_Info { get; set; } = new();

    [JsonProperty("Catch Info")]
    public Catch_Info_Class Catch_Info { get; set; } = new();

    [JsonProperty("Egg Info")]
    public Egg_Info_Class Egg_Info { get; set; }

    [JsonProperty("IVs")]
    public IVs_Class IVs { get; set; } = new();

    [JsonProperty("Hyper Training")]
    public Hyper_Training_Class Hyper_Training { get; set; } = new();

    [JsonProperty("EVs")]
    public EVs_Class EVs { get; set; } = new();

    [JsonProperty("Contest Stats")]
    public Contest_Stats_Class Contest_Stats { get; set; } = new();

    [JsonProperty("Ribbons")]
    public string[] Ribbons { get; set; }

    [JsonProperty("Markings")]
    public string[] Markings { get; set; }

    [JsonProperty("Pokérus")]
    public Pokerus_Class Pokerus { get; set; }

    [JsonProperty("Shadow Info")]
    public Shadow_Info_Class Shadow_Info { get; set; }

    [JsonProperty("Shiny Leaf")]
    public string[] Shiny_Leaf { get; set; }

    [JsonProperty("Trash Bytes")]
    public Trash_Bytes_Class Trash_Bytes { get; set; }

    [JsonProperty("Byte Override")]
    public Dictionary<string, JToken> Byte_Override { get; set; } = new();

    [JsonProperty("Format Overrides")]
    public Dictionary<string, pkuObject> Format_Overrides { get; set; }

    public class Trash_Bytes_Class : pkuDictionaryTag
    {
        [JsonProperty("Nickname")]
        public BigInteger[] Nickname { get; set; }

        [JsonProperty("OT")]
        public BigInteger[] OT { get; set; }
    }

    public class Move : pkuDictionaryTag
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("PP Ups")]
        public int? PP_Ups { get; set; }
    }

    public class Game_Info_Class : pkuDictionaryTag
    {
        [JsonProperty("Origin Game")]
        public BackedField<string> Origin_Game { get; set; } = new();

        // Only if exporting to official format and above fails
        [JsonProperty("Official Origin Game")]
        public BackedField<string> Official_Origin_Game { get; set; } = new();

        [JsonProperty("OT")]
        public string OT { get; set; }

        [JsonProperty("Gender")]
        public BackedField<string> Gender { get; set; } = new();

        [JsonProperty("TID")]
        public BackedField<BigInteger?> TID { get; set; } = new();

        [JsonProperty("Language")]
        public BackedField<string> Language { get; set; } = new();
    }

    //parent of Catch_Info and Egg_Info
    public class Met_Info_Base : pkuDictionaryTag
    {
        [JsonProperty("Met Location")]
        public BackedField<string> Met_Location { get; set; } = new();

        [JsonProperty("Met Game Override")]
        public BackedField<string> Met_Game_Override { get; set; } = new();

        [JsonProperty("Met Date")]
        public DateTime? Met_Date { get; set; }
    }

    public class Catch_Info_Class : Met_Info_Base
    {
        [JsonProperty("Ball")]
        public BackedField<string> Ball { get; set; } = new();

        [JsonProperty("Met Level")]
        public BackedField<BigInteger?> Met_Level { get; set; } = new();

        [JsonProperty("Fateful Encounter")]
        public bool? Fateful_Encounter { get; set; }

        [JsonProperty("Encounter Type")]
        public string Encounter_Type { get; set; }
    }

    public class Egg_Info_Class : Met_Info_Base
    {
        [JsonProperty("Egg")]
        public bool? Egg { get; set; }

        // Games dont use this, instead it's implicit from the met location in the egg section not being empty...
        // If you knew it was an egg but not when/where it was hatched, you couldn't represent that in-game...
        //[JsonProperty("Hatched")]
        //public bool? Hatched { get; set; }
    }

    public class Hyper_Training_Class : pkuDictionaryTag
    {
        [JsonProperty("HP")]
        public BackedField<bool?> HP { get; set; } = new();

        [JsonProperty("Attack")]
        public BackedField<bool?> Attack { get; set; } = new();

        [JsonProperty("Defense")]
        public BackedField<bool?> Defense { get; set; } = new();

        [JsonProperty("Sp. Attack")]
        public BackedField<bool?> Sp_Attack { get; set; } = new();

        [JsonProperty("Sp. Defense")]
        public BackedField<bool?> Sp_Defense { get; set; } = new();

        [JsonProperty("Speed")]
        public BackedField<bool?> Speed { get; set; } = new();
    }

    public class IVs_Class : pkuDictionaryTag
    {
        [JsonProperty("HP")]
        public BackedField<BigInteger?> HP { get; set; } = new();

        [JsonProperty("Attack")]
        public BackedField<BigInteger?> Attack { get; set; } = new();

        [JsonProperty("Defense")]
        public BackedField<BigInteger?> Defense { get; set; } = new();

        [JsonProperty("Sp. Attack")]
        public BackedField<BigInteger?> Sp_Attack { get; set; } = new();

        [JsonProperty("Sp. Defense")]
        public BackedField<BigInteger?> Sp_Defense { get; set; } = new();

        [JsonProperty("Speed")]
        public BackedField<BigInteger?> Speed { get; set; } = new();
    }

    public class EVs_Class : pkuDictionaryTag
    {
        [JsonProperty("HP")]
        public BackedField<BigInteger?> HP { get; set; } = new();

        [JsonProperty("Attack")]
        public BackedField<BigInteger?> Attack { get; set; } = new();

        [JsonProperty("Defense")]
        public BackedField<BigInteger?> Defense { get; set; } = new();

        [JsonProperty("Sp. Attack")]
        public BackedField<BigInteger?> Sp_Attack { get; set; } = new();

        [JsonProperty("Sp. Defense")]
        public BackedField<BigInteger?> Sp_Defense { get; set; } = new();

        [JsonProperty("Speed")]
        public BackedField<BigInteger?> Speed { get; set; } = new();
    }

    public class Contest_Stats_Class : pkuDictionaryTag
    {
        [JsonProperty("Cool")]
        public BackedField<BigInteger?> Cool { get; set; } = new();

        [JsonProperty("Beauty")]
        public BackedField<BigInteger?> Beauty { get; set; } = new();

        [JsonProperty("Cute")]
        public BackedField<BigInteger?> Cute { get; set; } = new();

        [JsonProperty("Clever")]
        public BackedField<BigInteger?> Clever { get; set; } = new();

        [JsonProperty("Tough")]
        public BackedField<BigInteger?> Tough { get; set; } = new();

        [JsonProperty("Sheen")]
        public BackedField<BigInteger?> Sheen { get; set; } = new();
    }

    public class Pokerus_Class : pkuDictionaryTag
    {
        [JsonProperty("Strain")]
        public int? Strain { get; set; }

        [JsonProperty("Days")]
        public int? Days { get; set; }
    }

    public class Shadow_Info_Class : pkuDictionaryTag
    {
        [JsonProperty("Shadow")]
        public bool? Shadow { get; set; }

        [JsonProperty("Purified")]
        public bool? Purified { get; set; }

        [JsonProperty("Heart Gauge")]
        public int? Heart_Gauge { get; set; }
    }


    /* ------------------------------------
     * FormatObject Methods
     * ------------------------------------
    */
    public override byte[] ToFile()
        => Encoding.UTF8.GetBytes(Serialize(true));

    public override void FromFile(byte[] file)
    {
        throw new NotImplementedException();
    }

    public string SourceFilename { get; set; }


    /* ------------------------------------
     * Copy/Merge Methods
     * ------------------------------------
    */
    /// <summary>
    /// Merges two pkuObjects, overriding the non-null entries
    /// of <paramref name="pkuA"/> with <paramref name="pkuB"/>.<br/>
    /// Note that this does not merge arrays.
    /// </summary>
    /// <param name="pkuA">The base of the merge.</param>
    /// <param name="pkuB">The pku that will be layered on top of <paramref name="pkuA"/>.</param>
    /// <returns>A pkuObject representing the merging of
    ///          <paramref name="pkuA"/> and <paramref name="pkuB"/>.</returns>
    public static pkuObject Merge(pkuObject pkuA, pkuObject pkuB)
    {
        JObject a = JObject.Parse(pkuA.Serialize());
        JObject b = JObject.Parse(pkuB.Serialize());
        var (p, e) = Deserialize(DataUtil.CombineJson(false, a, b).ToString());
        if (e is not null)
            throw new ArgumentException($"The merged pku isn't valid: {e}");
        return p;
    }

    /// <summary>
    /// Merges a pkuObject with one of its format overrides, if it exists.<br/>
    /// See <see cref="Merge(pkuObject, pkuObject)"/>.<br/>
    /// Note that this does not merge arrays.
    /// </summary>
    /// <param name="pku">The pku to be merged.</param>
    /// <param name="format">The name of the format override to merge with <paramref name="pku"/>.</param>
    /// <returns>A pkuObject representing the merging of <paramref name="pku"/>
    ///          and and its <paramref name="format"/> override. Or just
    ///          <paramref name="pku"/> if that doesn't exist.</returns>
    public static pkuObject MergeFormatOverride(pkuObject pku, string format)
        => pku.Format_Overrides?.TryGetValue(format, out pkuObject pkuOverride) is true ? Merge(pku, pkuOverride) : pku;

    /// <summary>
    /// Creates a deep copy of this <see cref="pkuObject"/>.
    /// </summary>
    /// <returns>A deep copy of this pkuObject.</returns>
    public pkuObject DeepCopy()
        => Deserialize(Serialize()).pku;


    /* ------------------------------------
     * Field Utility Methods
     * ------------------------------------
    */
    /// <summary>
    /// Whether this pku has been explictly marked as an egg.
    /// </summary>
    /// <returns>Whether <see cref="Egg_Info_Class.Egg"/> is true.</returns>
    public bool IsEgg()
        => Egg_Info?.Egg is true;

    /// <summary>
    /// Whether this pku has been explictly marked as shadow.
    /// </summary>
    /// <returns>Whether <see cref="Shadow_Info_Class.Shadow"/> is true.</returns>
    public bool IsShadow()
        => Shadow_Info?.Shadow is true;

    /// <summary>
    /// Whether this pku has been explictly marked as shiny.
    /// </summary>
    /// <returns>Whether <see cref="Shiny"/> is true.</returns>
    public bool IsShiny()
        => Shiny is true;

    public BackedField<BigInteger?>[] IVs_Array => new[] {
        IVs.HP, IVs.Attack, IVs.Defense, IVs.Sp_Attack, IVs.Sp_Defense, IVs.Speed
    };

    public BackedField<bool?>[] Hyper_Training_Array => new[] {
        Hyper_Training.HP, Hyper_Training.Attack, Hyper_Training.Defense,
        Hyper_Training.Sp_Attack, Hyper_Training.Sp_Defense, Hyper_Training.Speed
    };

    public BackedField<BigInteger?>[] EVs_Array => new[] {
        EVs.HP, EVs.Attack, EVs.Defense, EVs.Sp_Attack, EVs.Sp_Defense, EVs.Speed
    };

    public BackedField<BigInteger?>[] Contest_Stats_Array => new[] {
        Contest_Stats.Cool, Contest_Stats.Beauty, Contest_Stats.Cute,
        Contest_Stats.Clever, Contest_Stats.Tough, Contest_Stats.Sheen
    };


    /* ------------------------------------
     * Serialization Mechanics
     * ------------------------------------
    */
    /// <summary>
    /// The current pkuSchema from the pkuData repo.
    /// </summary>
    private static readonly JsonSchema pkuSchema = JsonSchema.FromText(DataUtil
        .DownloadJson("https://raw.githubusercontent.com/project-pku/pkuData/main/pkuSchema.json", "pkuSchema", true)
        .ToString());

    private static readonly System.Text.Json.JsonDocumentOptions JSON_DOC_OPTIONS = new()
    {
        CommentHandling = System.Text.Json.JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly ValidationOptions JSON_VALIDATION_OPTIONS = new()
    {
        OutputFormat = OutputFormat.Detailed
    };

    /// <summary>
    /// Attempts to deserialize a .pku file. If this fails, an error string is returned.
    /// </summary>
    /// <param name="pkuJson">The pku JSON string.</param>
    /// <returns>A tuple of the deserialized pkuObject and the error string, if any.<br/>
    ///          The pkuObject will be null if there was an error.</returns>
    public static (pkuObject pku, string error) Deserialize(string pkuJson)
    {
        pkuObject pku = null;
        string errormsg = null;
        System.Text.Json.JsonDocument pkuJsonDoc;

        //try reading json
        try
        {
            pkuJsonDoc = System.Text.Json.JsonDocument.Parse(pkuJson, JSON_DOC_OPTIONS);
        }
        catch //invalid json
        {
            errormsg = ".pku file is misformatted (invalid JSON)...";
            return (pku, errormsg);
        }

        // json valid, verify against pkuSchema
        ValidationResults results = pkuSchema.Validate(pkuJsonDoc.RootElement, JSON_VALIDATION_OPTIONS);
        if (results.IsValid) //pku is valid
        {
            try //try deserializing pku
            {
                pku = JsonConvert.DeserializeObject<pkuObject>(pkuJson, jsonSettings);
            }
            catch //failed to deserialize (why?)
            {
                errormsg = "Could not read .pku file...";
            }
        }
        else //invalid pku
        {
            while (results.HasNestedResults)
                results = results.NestedResults[0];
            errormsg = results.Message is null ? ".pku file is misformatted."
                                               : $"Something is wrong with {results.InstanceLocation}: {results.Message}";
        }
        return (pku, errormsg);
    }

    /// <summary>
    /// Serializes this pkuObject as a JSON string. Null entries are pruned.
    /// </summary>
    /// <returns>A JSON string of this pkuObject.</returns>
    public string Serialize(bool formatted = false)
        => JsonConvert.SerializeObject(this, formatted ? Formatting.Indented : Formatting.None, jsonSettings);

    private static readonly JsonSerializerSettings jsonSettings = new()
    {
        Converters = new List<JsonConverter> { new IFieldJsonConverter() },
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = ShouldSerializeContractResolver.Instance
    };

    private class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public static readonly ShouldSerializeContractResolver Instance = new();

        private static bool IsEmpty(object value)
        {
            //all null values are empty
            if (value is null)
                return true;

            Type valueType = value.GetType();

            // check backing of fields
            if (valueType.ImplementsGenericInterface(typeof(IField<>)))
            {
                value = valueType.GetProperties().Where(x => x.Name is "Value").First()
                    .GetGetMethod().Invoke(value, Array.Empty<object>());

                if (value is null) //backing value was null
                    return true;
            }

            //array and dictionary tags won't be serialized if they are null or empty (but if they have null entries they still will...)
            if (typeof(ICollection).IsAssignableFrom(valueType))
                return (value as ICollection).Count < 1;

            //pkuDictionaryTags tags won't be serialized if they are null or all their properties IsEmpty()
            else if (valueType.IsSubclassOf(typeof(pkuDictionaryTag)))
                return value is null || valueType.GetProperties().All(propertyInfo => IsEmpty(propertyInfo.GetValue(value)));

            //if it's not null, and not an empty collection/pkuDict it must have some value
            else
                return false;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            property.ShouldSerialize = x => !IsEmpty((member as PropertyInfo).GetValue(x));
            return property;
        }
    }

    private class ByteOverrideConverter : JsonConverter
    {
        public override bool CanWrite => true;

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType) => objectType.IsSubclassOf(typeof(IDictionary<string, JToken>));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => throw new NotImplementedException();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IDictionary<string, JToken> values = value as IDictionary<string, JToken>;
            writer.WriteStartObject();
            foreach (var x in values)
            {
                writer.WritePropertyName(x.Key);
                object val = x.Value.Type switch
                {
                    JTokenType.Integer => x.Value.ToObject<BigInteger>(),
                    JTokenType.Array => x.Value.ToObject<BigInteger[]>(),
                    _ => throw new NotImplementedException()
                };
                writer.WriteRawValue(JsonConvert.SerializeObject(val, Formatting.None));
            }
            writer.WriteEndObject();
        }
    }
}

public class pkuDictionaryTag
{
    /// <summary>
    /// Where tags not used by pkuManager are stored.
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken> ExtraTags { get; set; }
}