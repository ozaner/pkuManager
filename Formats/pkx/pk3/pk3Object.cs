using pkuManager.Common;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;
using System.IO;

namespace pkuManager.Formats.pkx.pk3
{
    /// <summary>
    /// An implementation of the .pk3 format used by the generation 3 GBA games.<br/>
    /// Implementation details mostly referenced from
    /// <see href="https://bulbapedia.bulbagarden.net/wiki/Pokémon_data_structure_(Generation_III)">Bulbapedia</see>.
    /// </summary>
    public class pk3Object : FormatObject
    {
        /* ------------------------------------
         * Data Blocks
         * ------------------------------------
        */
        /// <summary>
        /// The file size of a .pk3 file in the PC.
        /// </summary>
        internal const int FILE_SIZE_PC = 80;

        /// <summary>
        /// The file size of a .pk3 file in the party.
        /// </summary>
        internal const int FILE_SIZE_PARTY = 100;

        protected const bool BIG_ENDIANESS = false;
        protected const int NON_SUBDATA_SIZE = FILE_SIZE_PC - 4 * BLOCK_SIZE;
        protected const int BLOCK_SIZE = 12;
        protected static readonly string[] SUBSTRUCTURE_ORDER =
        {
            "GAEM", "GAME", "GEAM", "GEMA", "GMAE", "GMEA",
            "AGEM", "AGME", "AEGM", "AEMG", "AMGE", "AMEG",
            "EGAM", "EGMA", "EAGM", "EAMG", "EMGA", "EMAG",
            "MGAE", "MGEA", "MAGE", "MAEG", "MEGA", "MEAG",
        };


        public ByteArrayManipulator NonSubData { get; } = new(NON_SUBDATA_SIZE, BIG_ENDIANESS);
        public ByteArrayManipulator G { get; } = new(BLOCK_SIZE, BIG_ENDIANESS);
        public ByteArrayManipulator A { get; } = new(BLOCK_SIZE, BIG_ENDIANESS);
        public ByteArrayManipulator E { get; } = new(BLOCK_SIZE, BIG_ENDIANESS);
        public ByteArrayManipulator M { get; } = new(BLOCK_SIZE, BIG_ENDIANESS);

        // initializes properties
        public pk3Object()
        {
            // Non-Subdata
            PID = new(NonSubData, 0);
            TID = new(NonSubData, 4);
            Nickname = new(NonSubData, 8, 10);
            Language = new(NonSubData, 18);
            Egg_Name_Override = new(NonSubData, 19);
            OT = new(NonSubData, 20, 7);
            MarkingCircle = new(NonSubData, 27, 0);
            MarkingSquare = new(NonSubData, 27, 1);
            MarkingTriangle = new(NonSubData, 27, 2);
            MarkingHeart = new(NonSubData, 27, 3);
            Unused_A = new(NonSubData, 27, 4, 4); // leftover from markings byte
            Checksum = new(NonSubData, 28);
            Unused_B = new(NonSubData, 30); // probably padding

            // Block G
            Species = new(G, 0);
            Item = new(G, 2);
            Experience = new(G, 4);
            PP_Up_1 = new(G, 8, 0, 2);
            PP_Up_2 = new(G, 8, 2, 2);
            PP_Up_3 = new(G, 8, 4, 2);
            PP_Up_4 = new(G, 8, 6, 2);
            Friendship = new(G, 9);
            Unused_C = new(G, 10); // probably padding

            // Block A
            Moves = new(A, 0, 4);
            PP = new(A, 8, 4);

            // Block E
            EV_HP = new(E, 0);
            EV_Attack = new(E, 1);
            EV_Defense = new(E, 2);
            EV_Speed = new(E, 3);
            EV_Sp_Attack = new(E, 4);
            EV_Sp_Defense = new(E, 5);
            Cool = new(E, 6);
            Beauty = new(E, 7);
            Cute = new(E, 8);
            Smart = new(E, 9);
            Tough = new(E, 10);
            Sheen = new(E, 11);

            // Block M
            PKRS_Days = new(M, 0, 0, 4);
            PKRS_Strain = new(M, 0, 4, 4);
            Met_Location = new(M, 1);
            Met_Level = new(M, 2, 0, 7);
            Origin_Game = new(M, 2, 7, 4);
            Ball = new(M, 2, 11, 4);
            OT_Gender = new(M, 2, 15);
            IV_HP = new(M, 4, 0, 5);
            IV_Attack = new(M, 4, 5, 5);
            IV_Defense = new(M, 4, 10, 5);
            IV_Speed = new(M, 4, 15, 5);
            IV_Sp_Attack = new(M, 4, 20, 5);
            IV_Sp_Defense = new(M, 4, 25, 5);
            Is_Egg = new(M, 4, 30);
            Ability_Slot = new(M, 4, 31);
            Cool_Ribbon_Rank = new(M, 8, 0, 3);
            Beauty_Ribbon_Rank = new(M, 8, 3, 3);
            Cute_Ribbon_Rank = new(M, 8, 6, 3);
            Smart_Ribbon_Rank = new(M, 8, 9, 3);
            Tough_Ribbon_Rank = new(M, 8, 12, 3);
            Champion_Ribbon = new(M, 8, 15);
            Winning_Ribbon = new(M, 8, 16);
            Victory_Ribbon = new(M, 8, 17);
            Artist_Ribbon = new(M, 8, 18);
            Effort_Ribbon = new(M, 8, 19);
            Battle_Champion_Ribbon = new(M, 8, 20);
            Regional_Champion_Ribbon = new(M, 8, 21);
            National_Champion_Ribbon = new(M, 8, 22);
            Country_Ribbon = new(M, 8, 23);
            National_Ribbon = new(M, 8, 24);
            Earth_Ribbon = new(M, 8, 25);
            World_Ribbon = new(M, 8, 26);
            Unused_D = new(M, 8, 27, 4); //leftover from ribbon bytes
            Fateful_Encounter = new(M, 8, 31);
        }


        /* ------------------------------------
         * File Conversion
         * ------------------------------------
        */
        public override byte[] ToFile()
        {
            UpdateChecksum(); // Calculate Checksum
            ByteArrayManipulator subData = GetEncryptedSubData(); // Encryption Step

            // PC .pk3 file is an 80 byte data structure
            ByteArrayManipulator file = new(FILE_SIZE_PC, BIG_ENDIANESS);
            file.SetArray<byte>(NonSubData, 0); // First 32 bytes
            file.SetArray<byte>(subData, NON_SUBDATA_SIZE); // Last (encrypted) 48 bytes

            return file;
        }

        public override void FromFile(byte[] file)
        {
            NonSubData.SetArray(file, 0, NON_SUBDATA_SIZE);
            UnencryptSubData(new ByteArrayManipulator(file[NON_SUBDATA_SIZE..FILE_SIZE_PC], BIG_ENDIANESS));
        }
        

        /* ------------------------------------
         * Non-Subdata
         * ------------------------------------
        */
        public BAMByteValue<uint> PID { get; }
        public BAMByteValue<uint> TID { get; }
        public BAMArray<byte> Nickname { get; }
        public BAMByteValue<byte> Language { get; }
        public BAMByteValue<byte> Egg_Name_Override { get; }
        public BAMArray<byte> OT { get; }
        public BAMBitValue<bool> MarkingCircle { get; }
        public BAMBitValue<bool> MarkingSquare { get; }
        public BAMBitValue<bool> MarkingTriangle { get; }
        public BAMBitValue<bool> MarkingHeart { get; }
        public BAMBitValue<byte> Unused_A { get; }
        public BAMByteValue<ushort> Checksum { get; }
        public BAMByteValue<ushort> Unused_B { get; }


        /* ------------------------------------
         * G: Growth Block
         * ------------------------------------
        */
        public BAMByteValue<ushort> Species { get; }
        public BAMByteValue<ushort> Item { get; }
        public BAMByteValue<uint> Experience { get; }
        public BAMBitValue<byte> PP_Up_1 { get; }
        public BAMBitValue<byte> PP_Up_2 { get; }
        public BAMBitValue<byte> PP_Up_3 { get; }
        public BAMBitValue<byte> PP_Up_4 { get; }
        public BAMByteValue<byte> Friendship { get; }
        public BAMByteValue<ushort> Unused_C { get; }


        /* ------------------------------------
         * A: Attacks Block
         * ------------------------------------
        */
        public BAMArray<ushort> Moves { get; }
        public BAMArray<byte> PP { get; }


        /* ------------------------------------
         * E: EVs & Condition Block
         * ------------------------------------
        */
        public BAMByteValue<byte> EV_HP { get; }
        public BAMByteValue<byte> EV_Attack { get; }
        public BAMByteValue<byte> EV_Defense { get; }
        public BAMByteValue<byte> EV_Speed { get; }
        public BAMByteValue<byte> EV_Sp_Attack { get; }
        public BAMByteValue<byte> EV_Sp_Defense { get; }

        public BAMByteValue<byte> Cool { get; }
        public BAMByteValue<byte> Beauty { get; }
        public BAMByteValue<byte> Cute { get; }
        public BAMByteValue<byte> Smart { get; }
        public BAMByteValue<byte> Tough { get; }
        public BAMByteValue<byte> Sheen { get; }


        /* ------------------------------------
         * M: Misc. Block
         * ------------------------------------
        */
        public BAMBitValue<byte> PKRS_Days { get; }
        public BAMBitValue<byte> PKRS_Strain { get; }

        public BAMByteValue<byte> Met_Location { get; }

        public BAMBitValue<byte> Met_Level { get; }
        public BAMBitValue<byte> Origin_Game { get; }
        public BAMBitValue<byte> Ball { get; }
        public BAMBitValue<bool> OT_Gender { get; }

        public BAMBitValue<byte> IV_HP { get; }
        public BAMBitValue<byte> IV_Attack { get; }
        public BAMBitValue<byte> IV_Defense { get; }
        public BAMBitValue<byte> IV_Speed { get; }
        public BAMBitValue<byte> IV_Sp_Attack { get; }
        public BAMBitValue<byte> IV_Sp_Defense { get; }
        public BAMBitValue<bool> Is_Egg { get; }
        public BAMBitValue<bool> Ability_Slot { get; }

        public BAMBitValue<byte> Cool_Ribbon_Rank { get; }
        public BAMBitValue<byte> Beauty_Ribbon_Rank { get; }
        public BAMBitValue<byte> Cute_Ribbon_Rank { get; }
        public BAMBitValue<byte> Smart_Ribbon_Rank { get; }
        public BAMBitValue<byte> Tough_Ribbon_Rank { get; }
        public BAMBitValue<bool> Champion_Ribbon { get; }
        public BAMBitValue<bool> Winning_Ribbon { get; }
        public BAMBitValue<bool> Victory_Ribbon { get; }
        public BAMBitValue<bool> Artist_Ribbon { get; }
        public BAMBitValue<bool> Effort_Ribbon { get; }
        public BAMBitValue<bool> Battle_Champion_Ribbon { get; }
        public BAMBitValue<bool> Regional_Champion_Ribbon { get; }
        public BAMBitValue<bool> National_Champion_Ribbon { get; }
        public BAMBitValue<bool> Country_Ribbon { get; }
        public BAMBitValue<bool> National_Ribbon { get; }
        public BAMBitValue<bool> Earth_Ribbon { get; }
        public BAMBitValue<bool> World_Ribbon { get; }
        public BAMBitValue<byte> Unused_D { get; }
        public BAMBitValue<bool> Fateful_Encounter { get; }


        /* ------------------------------------
         * Encryption
         * ------------------------------------
        */
        /// <summary>
        /// Calculates the checksum of the 4 sub-blocks, and sets it to <see cref="Checksum"/>.
        /// </summary>
        protected void UpdateChecksum()
        {
            ushort checksum = 0;
            foreach (ByteArrayManipulator bam in new[] { G, A, E, M })
            {
                for (int i = 0; i < BLOCK_SIZE / 2; i++)
                    checksum += bam.Get<ushort>(i * 2);
            }

            Checksum.Set(checksum);
        }

        protected void ApplyXOR(ByteArrayManipulator subData)
        {
            uint encryptionKey = TID ^ PID;
            for (int i = 0; i < subData.Length / 4; i++) //xor subData with key in 4 byte chunks
            {
                uint chunk = subData.Get<uint>(4 * i);
                chunk ^= encryptionKey;
                subData.Set(chunk, 4 * i);
            }
        }

        /// <summary>
        /// Compiles and encrypts the current <see cref="G"/>, <see cref="A"/>, <see cref="E"/>,
        /// and <see cref="M"/> blocks with the current <see cref="PID"/> and <see cref="TID"/>.
        /// </summary>
        /// <returns>A 48 byte encrypted sub-data array.</returns>
        protected ByteArrayManipulator GetEncryptedSubData()
        {
            ByteArrayManipulator subData = new(4 * BLOCK_SIZE, BIG_ENDIANESS);
            string order = SUBSTRUCTURE_ORDER[PID % SUBSTRUCTURE_ORDER.Length];
            subData.SetArray<byte>(G, BLOCK_SIZE * order.IndexOf('G'));
            subData.SetArray<byte>(A, BLOCK_SIZE * order.IndexOf('A'));
            subData.SetArray<byte>(E, BLOCK_SIZE * order.IndexOf('E'));
            subData.SetArray<byte>(M, BLOCK_SIZE * order.IndexOf('M'));

            ApplyXOR(subData);
            return subData;
        }

        protected void UnencryptSubData(ByteArrayManipulator subData)
        {
            ApplyXOR(subData);

            string order = SUBSTRUCTURE_ORDER[PID % SUBSTRUCTURE_ORDER.Length];
            G.SetArray(subData.GetArray<byte>(BLOCK_SIZE * order.IndexOf('G'), BLOCK_SIZE), 0);
            A.SetArray(subData.GetArray<byte>(BLOCK_SIZE * order.IndexOf('A'), BLOCK_SIZE), 0);
            E.SetArray(subData.GetArray<byte>(BLOCK_SIZE * order.IndexOf('E'), BLOCK_SIZE), 0);
            M.SetArray(subData.GetArray<byte>(BLOCK_SIZE * order.IndexOf('M'), BLOCK_SIZE), 0);
        }


        /* ------------------------------------
         * Character Encoding
         * ------------------------------------
        */
        /// <summary>
        /// A list of all languages supported in Gen 3.
        /// </summary>
        public static readonly List<Language> VALID_LANGUAGES = new()
        {
            Common.Language.Japanese,
            Common.Language.English,
            Common.Language.French,
            Common.Language.Italian,
            Common.Language.German,
            Common.Language.Spanish
        };

        protected static readonly JObject PK3_CHARACTER_ENCODING_DATA = DataUtil.GetJson("pk3CharEncoding");
        protected static readonly Dictionary<byte, char> INTERNATIONAL_CHARSET = PK3_CHARACTER_ENCODING_DATA["International"].ToObject<Dictionary<byte, char>>();
        protected static readonly Dictionary<byte, char> GERMAN_CHARSET = DataUtil.GetCombinedJson(new JObject[]
        {
            (JObject)PK3_CHARACTER_ENCODING_DATA["International"],
            (JObject)PK3_CHARACTER_ENCODING_DATA["German"]
        }).ToObject<Dictionary<byte, char>>();
        protected static readonly Dictionary<byte, char> FRENCH_CHARSET = DataUtil.GetCombinedJson(new JObject[]
        {
            (JObject)PK3_CHARACTER_ENCODING_DATA["International"],
            (JObject)PK3_CHARACTER_ENCODING_DATA["French"]
        }).ToObject<Dictionary<byte, char>>();
        protected static readonly Dictionary<byte, char> JAPANESE_CHARSET = PK3_CHARACTER_ENCODING_DATA["Japanese"].ToObject<Dictionary<byte, char>>();

        /// <summary>
        /// The language dependent, 1-byte character encoding used by .pk3.
        /// </summary>
        internal static readonly CharacterEncoding<byte> CHARACTER_ENCODING = new(0xFF,
            (Common.Language.Japanese, JAPANESE_CHARSET),
            (Common.Language.German, GERMAN_CHARSET),
            (Common.Language.French, FRENCH_CHARSET),
            (Common.Language.English, INTERNATIONAL_CHARSET),
            (Common.Language.Italian, INTERNATIONAL_CHARSET),
            (Common.Language.Spanish, INTERNATIONAL_CHARSET)
        );

        /// <summary>
        /// The maximum number of characters in a .pk3 nickname.<br/>
        /// Note that while the JPN games only display the first 5 of these, they are all stored under the hood.
        /// </summary>
        public const int MAX_NICKNAME_CHARS = 10;

        /// <summary>
        /// The maximum number of characters in a .pk3 OT name.
        /// </summary>
        public const int MAX_OT_CHARS = 7;


        /* ------------------------------------
         * Met Location Encoding
         * ------------------------------------
        */
        protected static readonly JObject PK3_LOCATION_DATA = DataUtil.GetJson("gen3Locations");
        protected static readonly Dictionary<byte?, string> RS_LOCATION_TABLE = DataUtil.GetCombinedJson(new[]
        {
            (JObject)PK3_LOCATION_DATA["Base"],
            (JObject)PK3_LOCATION_DATA["RS"]
        }).ToObject<Dictionary<byte?, string>>();
        protected static readonly Dictionary<byte?, string> FRLG_LOCATION_TABLE = DataUtil.GetCombinedJson(new []
        {
            (JObject)PK3_LOCATION_DATA["Base"],
            (JObject)PK3_LOCATION_DATA["FRLG"]
        }).ToObject<Dictionary<byte?, string>>();
        protected static readonly Dictionary<byte?, string> E_LOCATION_TABLE = DataUtil.GetCombinedJson(new []
        {
            (JObject)PK3_LOCATION_DATA["Base"],
            (JObject)PK3_LOCATION_DATA["RS"],
            (JObject)PK3_LOCATION_DATA["E"]
        }).ToObject<Dictionary<byte?, string>>();
        protected static readonly Dictionary<int?, string> COLO_LOCATION_TABLE = DataUtil.GetCombinedJson(new []
        {
            (JObject)PK3_LOCATION_DATA["Base"],
            (JObject)PK3_LOCATION_DATA["Colo"]
        }).ToObject<Dictionary<int?, string>>();
        protected static readonly Dictionary<int?, string> XD_LOCATION_TABLE = DataUtil.GetCombinedJson(new []
{
            (JObject)PK3_LOCATION_DATA["Base"],
            (JObject)PK3_LOCATION_DATA["XD"]
        }).ToObject<Dictionary<int?, string>>();

        /// <summary>
        /// Gets the default location of the given gen 3 <paramref name="game"/>.
        /// </summary>
        /// <param name="game">A gen 3 game.</param>
        /// <returns>The default location of <paramref name="game"/>.</returns>
        public static string GetDefaultLocationName(string game) => game?.ToLowerInvariant() switch
        {
            "ruby" or "sapphire" => RS_LOCATION_TABLE[0],
            "emerald" => E_LOCATION_TABLE[0],
            "xd" => XD_LOCATION_TABLE[0],

            "firered" or "leafgreen" or "colosseum" or _ => null
        };

        /// <summary>
        /// Gets the gen 3 ID of the given <paramref name="location"/> for the given <paramref name="game"/>.
        /// </summary>
        /// <param name="game">A gen 3 game.</param>
        /// <param name="location">A location in <paramref name="game"/>.</param>
        /// <returns>The location ID, or null if the game/location combo is invalid.</returns>
        public static byte? EncodeMetLocation(string game, string location)
        {
            if (location is null)
                return null;

            // Game must be specified to find a location (i.e. full path of a location is "Game:Location")
            if (game is null)
                return null;

            // Get .pk3 Game Location
            var locationTableGBA = game.ToLowerInvariant() switch
            {
                "ruby" or "sapphire" => RS_LOCATION_TABLE,
                "firered" or "leafgreen" => FRLG_LOCATION_TABLE,
                "emerald" => E_LOCATION_TABLE,
                _ => null,//invalid game
            };
            if(locationTableGBA is not null)
                return locationTableGBA?.FirstOrDefault(x => x.Value.ToLowerInvariant() == location.ToLowerInvariant()).Key;

            var locationTableGCN = game.ToLowerInvariant() switch
            {
                "colosseum" => COLO_LOCATION_TABLE,
                "xd" => XD_LOCATION_TABLE,
                _ => null,//invalid game
            };

            //GCN locations need to be truncated.
            return (byte?)locationTableGCN?.FirstOrDefault(x => x.Value.ToLowerInvariant() == location.ToLowerInvariant()).Key;
        }


        /* ------------------------------------
         * Ability Encoding 
         * ------------------------------------
        */
        public static readonly JObject PK3_ABILITY_DATA = DataUtil.GetJson("pk3Abilities");

        /// <summary>
        /// Returns whether the given ability is possible in each of the given species' ability slots.
        /// </summary>
        /// <param name="dex">The national dex # of the species to be checked.</param>
        /// <param name="abilityID">The ability ID of the ability to be checked.</param>
        /// <returns>A 2-tuple of bools. The first/second bool is true if the ability
        ///          is consistent with slot1/slot2 respectively.</returns>
        public static (bool slot1, bool slot2) IsAbilityValid(int dex, int abilityID)
        {
            if (dex is < 1 or > 386) //must be a valid gen 3 dex
                throw new ArgumentException("pk3Util.GetAbilityID must be given a valid gen 3 dex number.");

            int slot1 = (int)PK3_ABILITY_DATA["" + dex]["1"];
            int? slot2 = (int?)PK3_ABILITY_DATA["" + dex]?["2"];

            return (slot1 == abilityID, slot2 == abilityID);
        }


        /* ------------------------------------
         * Form Encoding 
         * ------------------------------------
        */
        public static readonly JObject VALID_FORMS = DataUtil.GetJson("gen3Forms");

        /// <summary>
        /// Gets the form ID of an Unown with the given PID in Gen 3.
        /// </summary>
        /// <param name="pid">The Unown's PID.</param>
        /// <returns>The Unown form ID determined by the PID.</returns>
        public static int GetUnownFormID(uint pid)
        {
            uint formID = 0;
            formID.SetBits(pid.GetBits(0, 2), 0, 2); //first two bits of byte 0
            formID.SetBits(pid.GetBits(8, 2), 2, 2); //first two bits of byte 1
            formID.SetBits(pid.GetBits(16, 2), 4, 2); //first two bits of byte 2
            formID.SetBits(pid.GetBits(24, 2), 6, 2); //first two bits of byte 3

            return (int)formID % 28;
        }

        /// <summary>
        /// Gets the Unown form name the given ID corresponds to.
        /// </summary>
        /// <param name="id">An Unown form ID.</param>
        /// <returns>The name of the Unown form with <paramref name="id"/>. Null if ID is invalid.</returns>
        public static string GetUnownFormName(int id) => id switch
        {
            < 0 or > 27 => null, //invalid id
            26 => "!",
            27 => "?",
            _ => "" + (char)(0x41 + id) //A-Z
        };


        /* ------------------------------------
         * Misc. Utility
         * ------------------------------------
        */
        /// <summary>
        /// The last valid move ID used in Gen 3. Corresponds to Psycho Boost.
        /// </summary>
        internal const ushort LAST_MOVE_ID = 354;

        /// <summary>
        /// The national dex # of the last Gen 3 species (i.e. Deoxys).
        /// </summary>
        public const int LAST_DEX_NUM = 386;

        /// <summary>
        /// When <see cref="Egg_Name_Override"/> is set to this value, the pk3 nickname will<br/>
        /// be overriden by the <see cref="pkxUtil.EGG_NICKNAME"/> of the game's language.
        /// </summary>
        public const byte EGG_NAME_OVERRIDE_CONST = 0x06;

        /// <summary>
        /// Calculates the PP of the given move with the given number of PP Ups.
        /// </summary>
        /// <param name="moveID">The ID of the move.</param>
        /// <param name="ppups">The number of PP Ups the move has.</param>
        /// <returns>The PP the move with <paramref name="moveID"/> would have with <paramref name="ppups"/> PP Ups.</returns>
        public static byte CalculatePP(ushort moveID, byte ppups)
        {
            // empty move check
            if (moveID is 0)
                return 0;

            //pp formula
            return (byte)((5 + ppups) * PokeAPIUtil.GetMoveBasePP(moveID).Value / 5);
        }

        /// <summary>
        /// Returns the rank of the given Gen 3 contest category.
        /// </summary>
        /// <param name="firstRibbon">The first ribbon in the contest category to check.</param>
        /// <param name="ribbons">A list of ribbons to check for the highest ranking ribbon.</param>
        /// <returns>The numeric rank of the contest category ribbon
        ///          (given by <paramref name="firstRibbon"/>) in <paramref name="firstRibbon"/>.</returns>
        public static byte GetRibbonRank(Ribbon firstRibbon, HashSet<Ribbon> ribbons)
        {
            if (firstRibbon is not (Ribbon.Cool_G3 or Ribbon.Beauty_G3 or Ribbon.Cute_G3 or Ribbon.Smart_G3 or Ribbon.Tough_G3))
                throw new ArgumentException($"{nameof(firstRibbon)} must be one of the normal rank contest ribbons.", nameof(firstRibbon));

            return (byte)new[]
            {
                1 * Convert.ToInt32(ribbons.Contains(firstRibbon)),
                2 * Convert.ToInt32(ribbons.Contains(firstRibbon + 1)),
                3 * Convert.ToInt32(ribbons.Contains(firstRibbon + 2)),
                4 * Convert.ToInt32(ribbons.Contains(firstRibbon + 2)),
            }.Max();
        }

        /// <summary>
        /// Whether the given <paramref name="ribbon"/> exists in Gen 3.
        /// </summary>
        /// <param name="ribbon">The ribbon to check.</param>
        /// <returns>Whether <paramref name="ribbon"/> exists in Gen 3.</returns>
        public static bool IsValidRibbon(Ribbon ribbon)
        {
            return ribbon is (>= Ribbon.Cool_G3 and <= Ribbon.Tough_Master_G3) or Ribbon.Champion or
                             Ribbon.Winning or Ribbon.Victory or Ribbon.Artist or Ribbon.Effort or 
                             Ribbon.Battle_Champion or Ribbon.Regional_Champion or Ribbon.National_Champion or 
                             Ribbon.Country or Ribbon.National or Ribbon.Earth or Ribbon.World;
        }

        /// <summary>
        /// Given a GBA ROM, creates a JObject of the different possible ability IDs a pokemon species can have.
        /// </summary>
        /// <param name="path">Path to a main series Pokemon GBA ROM.</param>
        /// <param name="offset">The starting index of the species table in this ROM.</param>
        /// <returns>A JObject with the ability table given by the ROM.
        ///          Note that in it, species and abilities are referenced by index #.</returns>
        public static JObject ProduceGBAAbilityDex(string path, int offset)
        {
            // offset = 0x3203E8 for Emerald[U]
            byte[] ROM = File.ReadAllBytes(path);
            JObject abiltyDex = new();

            for (int i = 1; i <= 385; i++) //For each Gen 3 Pokemon (except deoxys)
            {
                int index = PokeAPIUtil.GetSpeciesIndex(i, 3).Value; //These are all valid pokedex #s
                byte slot1 = ROM[offset + 28 * (index - 1) + 22];
                byte slot2 = ROM[offset + 28 * (index - 1) + 23];

                // Adjust for Air Lock (77 in Gen 3 -> 76 in Gen 4+)
                slot1 = slot1 == 77 ? (byte)76 : slot1;
                slot2 = slot2 == 77 ? (byte)76 : slot2;

                // Add Entry to AbilityDex
                JObject jo = new();
                jo.Add("1", slot1);
                if (slot2 != 0) //No entry for blank Slot 2's
                    jo.Add("2", slot2);
                abiltyDex.Add("" + i, jo);
            }

            return abiltyDex;
        }
    }
}
