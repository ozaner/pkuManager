using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using pkuManager.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace pkuManager.pku
{
    public class pkuObject : pkuDictionaryTag
    {
        [JsonProperty("Species")]
        public string Species { get; set; }

        [JsonProperty("Nickname")]
        public string Nickname { get; set; }

        [JsonProperty("Nickname Flag")]
        public bool? Nickname_Flag { get; set; }

        [JsonProperty("True OT")]
        public string True_OT { get; set; }

        [JsonProperty("Forms")]
        [JsonConverter(typeof(OneLineArrayConverter))]
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
        [JsonConverter(typeof(OneLineArrayConverter))]
        public string[] Markings { get; set; }

        [JsonProperty("Pokérus")]
        public Pokerus_Class Pokerus { get; set; }

        [JsonProperty("Shadow Info")]
        public Shadow_Info_Class Shadow_Info { get; set; }

        [JsonProperty("Shiny Leaf")]
        [JsonConverter(typeof(OneLineArrayConverter))]
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
            [JsonConverter(typeof(OneLineArrayConverter))]
            public ushort[] Nickname { get; set; }

            [JsonProperty("OT")]
            [JsonConverter(typeof(OneLineArrayConverter))]
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

            // The full trainer ID, or FTID, if you will.
            [JsonProperty("ID")]
            public uint? ID { get; set; }

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

            // This is actually friendship, so is a seperate value isn't really necessary
            //[JsonProperty("Steps Left")]
            //public int? Steps_Left { get; set; }
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
            public Dictionary<int, byte> Main_Data { get; set; }

            [JsonProperty("A")]
            public Dictionary<int, byte> A { get; set; }

            [JsonProperty("B")]
            public Dictionary<int, byte> B { get; set; }

            [JsonProperty("C")]
            public Dictionary<int, byte> C { get; set; }

            [JsonProperty("D")]
            public Dictionary<int, byte> D { get; set; }
        }


        /* ------------------------------------
         * Utility Methods
         * ------------------------------------
        */

        /// <summary>
        /// Merges two pkuObjects, overriding the non-null entries
        /// of <paramref name="pkuA"/> with <paramref name="pkuB"/>.
        /// </summary>
        /// <param name="pkuA">The base of the merge.</param>
        /// <param name="pkuB">The pku that will be layered on top of <paramref name="pkuA"/>.</param>
        /// <returns>A pkuObject representing the merging of
        ///          <paramref name="pkuA"/> and <paramref name="pkuB"/>.</returns>
        public static pkuObject Merge(pkuObject pkuA, pkuObject pkuB)
        {
            JObject a = JObject.FromObject(pkuA);
            JObject b = JObject.FromObject(pkuB);
            var (p, e) = Deserialize(DataUtil.GetCombinedJson(a, b).ToString());
            if (e is not null)
                throw new ArgumentException($"The merged pku isn't valid: {e}");
            return p;
        }

        /// <summary>
        /// Merges a pkuObject with one of its format overrides, if it exists.<br/>
        /// See <see cref="Merge(pkuObject, pkuObject)"/>.
        /// </summary>
        /// <param name="pku">The pku to be merged.</param>
        /// <param name="format">The name of the format override to merge with <paramref name="pku"/>.</param>
        /// <returns>A pkuObject representing the merging of <paramref name="pku"/>
        ///          and and its <paramref name="format"/> override. Or just
        ///          <paramref name="pku"/> if that doesn't exist.</returns>
        public static pkuObject MergeFormatOverride(pkuObject pku, string format)
        {
            pkuObject pkuOverride = null;
            if (pku.Format_Overrides?.TryGetValue(format, out pkuOverride) is true)
                return Merge(pku, pkuOverride);
            else
                return pku;
        }

        /// <inheritdoc cref="Deserialize(string)"/>
        /// <param name="pkuFileInfo">A reference to the pku file.</param>
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

        /// <inheritdoc cref="Deserialize(string)"/>
        /// <param name="pkuFile">An array of bytes representing the pku file in UTF-8.</param>
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

            try
            {
                pku = JsonConvert.DeserializeObject<pkuObject>(pkuJson, jsonSettings);
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
                else //don't know/no type mentioned
                    errormsg = ex.Message;
            }

            return (pku, errormsg);
        }

        /// <summary>
        /// Serializes this pkuObject as a JSON string. Null entries are pruned.
        /// </summary>
        /// <returns>A JSON string of this pkuObject.</returns>
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, jsonSettings);
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="pkuObject"/>.
        /// </summary>
        /// <returns>A deep copy of this pkuObject.</returns>
        public pkuObject DeepCopy()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<pkuObject>(serialized);
        }

        /// <summary>
        /// Whether this pku has been explictly marked as an egg.
        /// </summary>
        /// <returns>Whether <see cref="Egg_Info_Class.Egg"/> is true.</returns>
        public bool IsEgg()
        {
            return Egg_Info?.Egg == true;
        }

        /// <summary>
        /// Whether this pku has been explictly marked as shadow.
        /// </summary>
        /// <returns>Whether <see cref="Shadow_Info_Class.Shadow"/> is true.</returns>
        public bool IsShadow()
        {
            return Shadow_Info?.Shadow == true;
        }

        /// <summary>
        /// Whether this pku has been explictly marked as shiny.
        /// </summary>
        /// <returns>Whether <see cref="Shiny"/> is true.</returns>
        public bool IsShiny()
        {
            return Shiny == true;
        }


        /* ------------------------------------
         * Serialization Mechanics
         * ------------------------------------
        */

        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = EmptyToNullContractResolver.Instance
        };

        private class EmptyToNullContractResolver : DefaultContractResolver
        {
            public static readonly EmptyToNullContractResolver Instance = new EmptyToNullContractResolver();

            private static bool IsEmpty(object value)
            {
                //all null values are empty
                if (value == null)
                    return true;

                Type valueType = value.GetType();

                //array and dictionary tags won't be serialized if they are null or empty (but if they have null entries they still will...)
                if (typeof(ICollection).IsAssignableFrom(valueType))
                    return ((ICollection)value).Count < 1;

                //pkuDictionaryTags tags won't be serialized if they are null or all their entries IsEmpty()
                else if (valueType.IsSubclassOf(typeof(pkuDictionaryTag)))
                    return value == null || valueType.GetProperties().All(propertyInfo => IsEmpty(propertyInfo.GetValue(value)));

                //if it's not null, and not an empty collection/pkuDict it must have some value
                else
                    return false;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
                property.ValueProvider = new EmptyToNullValueProvider(member as PropertyInfo);
                return property;
            }

            private class EmptyToNullValueProvider : IValueProvider
            {
                private readonly PropertyInfo _targetProperty;

                public EmptyToNullValueProvider(PropertyInfo targetProperty)
                {
                    _targetProperty = targetProperty;
                }

                // Called during deserialization.
                // Value parameter is the original value read from the JSON.
                // Target is the object on which to set the value.
                public void SetValue(object target, object value)
                {
                    _targetProperty.SetValue(target, IsEmpty(value) ? null : value);
                }

                // Called during serialization.
                // Target parameter has the object from which to read the value.
                // Return value is what gets written to the JSON.
                public object GetValue(object target)
                {
                    object value = _targetProperty.GetValue(target);
                    return IsEmpty(value) ? null : value;
                }
            }
        }

        private class OneLineArrayConverter : JsonConverter
        {
            public override bool CanWrite => true;

            public override bool CanRead => false;

            public override bool CanConvert(Type objectType) => true;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                => throw new NotImplementedException();

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteRawValue(JsonConvert.SerializeObject(value, Formatting.None));
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
}