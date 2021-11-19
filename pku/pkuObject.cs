using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace pkuManager.pku;

public class pkuObject : pkuDictionaryTag
{
    [JsonProperty("Species")]
    public string Species { get; set; }

    [JsonProperty("Nickname")]
    public string Nickname { get; set; }

    [JsonProperty("Nickname Flag")]
    public bool? Nickname_Flag { get; set; }

    [JsonProperty("True OT")]
    public BackedField<string> True_OT { get; set; } = new();

    [JsonProperty("Forms")]
    public string[] Forms { get; set; }

    [JsonProperty("Appearance")]
    public string[] Appearance { get; set; }

    [JsonProperty("Gender")]
    public string Gender { get; set; }

    [JsonProperty("Level")]
    public int? Level { get; set; }

    [JsonProperty("EXP")]
    public int? EXP { get; set; }

    [JsonProperty("Item")]
    public string Item { get; set; }

    [JsonProperty("Moves")]
    public Move[] Moves { get; set; }

    [JsonProperty("PID")]
    public uint? PID { get; set; }

    [JsonProperty("Shiny")]
    public bool? Shiny { get; set; }

    [JsonProperty("Nature")]
    public string Nature { get; set; }

    [JsonProperty("Stat Nature")]
    public string Stat_Nature { get; set; }

    [JsonProperty("Ability")]
    public string Ability { get; set; }

    [JsonProperty("Gigantamax Factor")]
    public bool? Gigantamax_Factor { get; set; }

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

    [JsonProperty("Pokérus")]
    public Pokerus_Class Pokerus { get; set; }

    [JsonProperty("Shadow Info")]
    public Shadow_Info_Class Shadow_Info { get; set; }

    [JsonProperty("Shiny Leaf")]
    public string[] Shiny_Leaf { get; set; }

    [JsonProperty("Trash Bytes")]
    public Trash_Bytes_Class Trash_Bytes { get; set; }

    [JsonProperty("Byte Override")]
    public Byte_Override_Class Byte_Override { get; set; }

    [JsonProperty("Format Overrides")]
    public Dictionary<string, pkuObject> Format_Overrides { get; set; }

    public class Trash_Bytes_Class : pkuDictionaryTag
    {
        [JsonProperty("Nickname")]
        public ushort[] Nickname { get; set; }

        [JsonProperty("OT")]
        public ushort[] OT { get; set; }
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
        public string Origin_Game { get; set; }

        // Only if exporting to official format and above fails
        [JsonProperty("Official Origin Game")]
        public string Official_Origin_Game { get; set; }

        [JsonProperty("OT")]
        public string OT { get; set; }

        [JsonProperty("Gender")]
        public string Gender { get; set; }

        [JsonProperty("TID")]
        public uint? TID { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }
    }

    //parent of Catch_Info and Egg_Info
    public class Met_Info_Base : pkuDictionaryTag
    {
        [JsonProperty("Met Location")]
        public string Met_Location { get; set; }

        [JsonProperty("Met Game Override")]
        public string Met_Game_Override { get; set; }

        [JsonProperty("Met Date")]
        public DateTime? Met_Date { get; set; }
    }

    public class Catch_Info_Class : Met_Info_Base
    {
        [JsonProperty("Ball")]
        public string Ball { get; set; }

        [JsonProperty("Met Level")]
        public int? Met_Level { get; set; }

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

    public class IVs_Class : pkuDictionaryTag
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

    public class EVs_Class : pkuDictionaryTag
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

    public class Contest_Stats_Class : pkuDictionaryTag
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

    public class Byte_Override_Class : pkuDictionaryTag
    {
        [JsonProperty("Main Data")]
        [JsonConverter(typeof(ByteOverrideConverter))]
        public Dictionary<string, JToken> Main_Data { get; set; }

        [JsonProperty("A")]
        [JsonConverter(typeof(ByteOverrideConverter))]
        public Dictionary<string, JToken> A { get; set; }

        [JsonProperty("B")]
        [JsonConverter(typeof(ByteOverrideConverter))]
        public Dictionary<string, JToken> B { get; set; }

        [JsonProperty("C")]
        [JsonConverter(typeof(ByteOverrideConverter))]
        public Dictionary<string, JToken> C { get; set; }

        [JsonProperty("D")]
        [JsonConverter(typeof(ByteOverrideConverter))]
        public Dictionary<string, JToken> D { get; set; }
    }


    /* ------------------------------------
     * Utility Methods
     * ------------------------------------
    */
    /// <summary>
    /// The current pkuSchema from the pkuData repo.
    /// </summary>
    private static readonly JsonSchema pkuSchema = JsonSchema.FromJsonAsync(DataUtil
        .DownloadJson("https://raw.githubusercontent.com/project-pku/pkuData/main/pkuSchema.json", "pkuSchema", true)
        .ToString()).Result;

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

    /// <inheritdoc cref="Deserialize(string)"/>
    /// <param name="pkuFileInfo">A reference to the pku file.</param>
    public static (pkuObject pku, string error) Deserialize(FileInfo pkuFileInfo)
    {
        try
        {
            string filetext = File.ReadAllText(pkuFileInfo.FullName);
            return Deserialize(filetext);
        }
        catch
        {
            return (null, "Not a valid text file.");
        }
    }

    /// <inheritdoc cref="Deserialize(string)"/>
    /// <param name="pkuFile">An array of bytes representing the pku file in UTF-8.</param>
    public static (pkuObject pku, string error) Deserialize(byte[] pkuFile)
    {
        try
        {
            string filetext = System.Text.Encoding.UTF8.GetString(pkuFile, 0, pkuFile.Length);
            return Deserialize(filetext);
        }
        catch
        {
            return (null, "Not a valid text file.");
        }
    }

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

        var errors = pkuSchema.Validate(pkuJson);
        if (!errors.Any())
        {
            try
            {
                pku = JsonConvert.DeserializeObject<pkuObject>(pkuJson, jsonSettings);
            }
            catch
            {
                errormsg = "Could not read .pku file...";
            }
        }
        else
        {
            var error = errors.FirstOrDefault();
            errormsg = error is null ? ".pku file is misformatted."
                                     : $"Something is wrong with '{error.Path}' or one of it's sub-tags.";
        }

        return (pku, errormsg);
    }

    /// <summary>
    /// Serializes this pkuObject as a JSON string. Null entries are pruned.
    /// </summary>
    /// <returns>A JSON string of this pkuObject.</returns>
    public string Serialize(bool formatted = false)
        => JsonConvert.SerializeObject(this, formatted ? Formatting.Indented : Formatting.None, jsonSettings);

    /// <summary>
    /// Creates a deep copy of this <see cref="pkuObject"/>.
    /// </summary>
    /// <returns>A deep copy of this pkuObject.</returns>
    public pkuObject DeepCopy()
        => Deserialize(Serialize()).pku;

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


    /* ------------------------------------
     * Serialization Mechanics
     * ------------------------------------
    */
    private static readonly JsonSerializerSettings jsonSettings = new()
    {
        Converters = new List<JsonConverter> { new FieldJsonConverter() },
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
            if (valueType.IsSubclassOfGeneric(typeof(Field<>)))
            {
                value = valueType.GetMethods()
                                 .Where(x => x.Name is "Get" && !x.IsGenericMethod && x.GetParameters().Length is 0)
                                 .First().Invoke(value, Array.Empty<object>());

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