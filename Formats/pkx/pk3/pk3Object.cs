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

        public ByteArrayManipulator NonSubData { get; protected set; } = new(NON_SUBDATA_SIZE, BIG_ENDIANESS);
        public ByteArrayManipulator G { get; protected set; } = new(BLOCK_SIZE, BIG_ENDIANESS);
        public ByteArrayManipulator A { get; protected set; } = new(BLOCK_SIZE, BIG_ENDIANESS);
        public ByteArrayManipulator E { get; protected set; } = new(BLOCK_SIZE, BIG_ENDIANESS);
        public ByteArrayManipulator M { get; protected set; } = new(BLOCK_SIZE, BIG_ENDIANESS);


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

        public override (bool, string) IsFile(byte[] file)
        {
            if (file.Length is not (FILE_SIZE_PC or FILE_SIZE_PARTY))
                return (false, $"A .pk3 file must be {FILE_SIZE_PC} or {FILE_SIZE_PARTY} bytes long.");

            return (true, null);
        }

        public override void FromFile(byte[] file)
        {
            NonSubData = new(file[0..NON_SUBDATA_SIZE], BIG_ENDIANESS);
            (G, A, E, M) = UnencryptSubData(new ByteArrayManipulator(file[NON_SUBDATA_SIZE..FILE_SIZE_PC], BIG_ENDIANESS));
        }


        /* ------------------------------------
         * Non-Subdata
         * ------------------------------------
        */
        public uint PID { get => NonSubData.GetUInt(0); set => NonSubData.SetUInt(value, 0); }
        public uint ID { get => NonSubData.GetUInt(4); set => NonSubData.SetUInt(value, 4); }
        public byte[] Nickname { get => NonSubData.GetBytes(8, 10); set => NonSubData.SetBytes(value, 8, 10); }
        public byte Language { get => NonSubData.GetByte(18); set => NonSubData.SetByte(value, 18); }
        public byte Egg_Name_Override { get => NonSubData.GetByte(19); set => NonSubData.SetByte(value, 19); }
        public byte[] OT { get => NonSubData.GetBytes(20, 7); set => NonSubData.SetBytes(value, 20, 7); }

        public bool MarkingCircle { get => NonSubData.GetBool(27, 0); set => NonSubData.SetBool(value, 27, 0); }
        public bool MarkingSquare { get => NonSubData.GetBool(27, 1); set => NonSubData.SetBool(value, 27, 1); }
        public bool MarkingTriangle { get => NonSubData.GetBool(27, 2); set => NonSubData.SetBool(value, 27, 2); }
        public bool MarkingHeart { get => NonSubData.GetBool(27, 3); set => NonSubData.SetBool(value, 27, 3); }
        // Byte 27, bits 4-7 (unused, leftover from markings byte)

        protected ushort Checksum { get => NonSubData.GetUShort(28); set => NonSubData.SetUShort(value, 28); }
        // Bytes 28-29 (unused, probably padding)


        /* ------------------------------------
         * G: Growth Block
         * ------------------------------------
        */
        public ushort Species { get => G.GetUShort(0); set => G.SetUShort(value, 0); }
        public ushort Item { get => G.GetUShort(2); set => G.SetUShort(value, 2); }
        public uint Experience { get => G.GetUInt(4); set => G.SetUInt(value, 4); }

        public byte PP_Up_1 { get => G.GetByteBits(8, 0, 2); set => G.SetByteBits(value, 8, 0, 2); }
        public byte PP_Up_2 { get => G.GetByteBits(8, 2, 2); set => G.SetByteBits(value, 8, 2, 2); }
        public byte PP_Up_3 { get => G.GetByteBits(8, 4, 2); set => G.SetByteBits(value, 8, 4, 2); }
        public byte PP_Up_4 { get => G.GetByteBits(8, 6, 2); set => G.SetByteBits(value, 8, 6, 2); }

        public byte Friendship { get => G.GetByte(9); set => G.SetByte(value, 9); }
        // Bytes 10-11 (unused, probably padding)


        /* ------------------------------------
         * A: Attacks Block
         * ------------------------------------
        */
        public ushort Move_1 { get => A.GetUShort(0); set => A.SetUShort(value, 0); }
        public ushort Move_2 { get => A.GetUShort(2); set => A.SetUShort(value, 2); }
        public ushort Move_3 { get => A.GetUShort(4); set => A.SetUShort(value, 4); }
        public ushort Move_4 { get => A.GetUShort(6); set => A.SetUShort(value, 6); }

        public byte PP_1 { get => A.GetByte(8); set => A.SetByte(value, 8); }
        public byte PP_2 { get => A.GetByte(9); set => A.SetByte(value, 9); }
        public byte PP_3 { get => A.GetByte(10); set => A.SetByte(value, 10); }
        public byte PP_4 { get => A.GetByte(11); set => A.SetByte(value, 11); }


        /* ------------------------------------
         * E: EVs & Condition Block
         * ------------------------------------
        */
        public byte EV_HP { get => E.GetByte(0); set => E.SetByte(value, 0); }
        public byte EV_Attack { get => E.GetByte(1); set => E.SetByte(value, 1); }
        public byte EV_Defense { get => E.GetByte(2); set => E.SetByte(value, 2); }
        public byte EV_Speed { get => E.GetByte(3); set => E.SetByte(value, 3); }
        public byte EV_Sp_Attack { get => E.GetByte(4); set => E.SetByte(value, 4); }
        public byte EV_Sp_Defense { get => E.GetByte(5); set => E.SetByte(value, 5); }

        public byte Cool { get => E.GetByte(6); set => E.SetByte(value, 6); }
        public byte Beauty { get => E.GetByte(7); set => E.SetByte(value, 7); }
        public byte Cute { get => E.GetByte(8); set => E.SetByte(value, 8); }
        public byte Smart { get => E.GetByte(9); set => E.SetByte(value, 9); }
        public byte Tough { get => E.GetByte(10); set => E.SetByte(value, 10); }
        public byte Sheen { get => E.GetByte(11); set => E.SetByte(value, 11); }


        /* ------------------------------------
         * M: Misc. Block
         * ------------------------------------
        */
        public byte PKRS_Days { get => M.GetByteBits(0, 0, 4); set => M.SetByteBits(value, 0, 0, 4); }
        public byte PKRS_Strain { get => M.GetByteBits(0, 4, 4); set => M.SetByteBits(value, 0, 4, 4); }

        public byte Met_Location { get => M.GetByte(1); set => M.SetByte(value, 1); }
        public byte Met_Level { get => (byte)M.GetUShortBits(2, 0, 7); set => M.SetUShortBits(value, 2, 0, 7); }
        public byte Origin_Game { get => (byte)M.GetUShortBits(2, 7, 4); set => M.SetUShortBits(value, 2, 7, 4); }
        public byte Ball { get => (byte)M.GetUShortBits(2, 11, 4); set => M.SetUShortBits(value, 2, 11, 4); }
        public bool OT_Gender { get => M.GetBool(3, 7); set => M.SetBool(value, 3, 7); }

        public byte IV_HP { get => (byte)M.GetUIntBits(4, 0, 5); set => M.SetUIntBits(value, 4, 0, 5); }
        public byte IV_Attack { get => (byte)M.GetUIntBits(4, 5, 5); set => M.SetUIntBits(value, 4, 5, 5); }
        public byte IV_Defense { get => (byte)M.GetUIntBits(4, 10, 5); set => M.SetUIntBits(value, 4, 10, 5); }
        public byte IV_Speed { get => (byte)M.GetUIntBits(4, 15, 5); set => M.SetUIntBits(value, 4, 15, 5); }
        public byte IV_Sp_Attack { get => (byte)M.GetUIntBits(4, 20, 5); set => M.SetUIntBits(value, 4, 20, 5); }
        public byte IV_Sp_Defense { get => (byte)M.GetUIntBits(4, 25, 5); set => M.SetUIntBits(value, 4, 25, 5); }

        public bool Is_Egg { get => M.GetBool(7, 6); set => M.SetBool(value, 7, 6); }
        public bool Ability_Slot { get => M.GetBool(7, 7); set => M.SetBool(value, 7, 7); }

        public byte Cool_Ribbon_Rank { get => (byte)M.GetUIntBits(8, 0, 3); set => M.SetUIntBits(value, 8, 0, 3); }
        public byte Beauty_Ribbon_Rank { get => (byte)M.GetUIntBits(8, 3, 3); set => M.SetUIntBits(value, 8, 3, 3); }
        public byte Cute_Ribbon_Rank { get => (byte)M.GetUIntBits(8, 6, 3); set => M.SetUIntBits(value, 8, 6, 3); }
        public byte Smart_Ribbon_Rank { get => (byte)M.GetUIntBits(8, 9, 3); set => M.SetUIntBits(value, 8, 9, 3); }
        public byte Tough_Ribbon_Rank { get => (byte)M.GetUIntBits(8, 12, 3); set => M.SetUIntBits(value, 8, 12, 3); }

        public bool Champion_Ribbon { get => M.GetBool(9, 7); set => M.SetBool(value, 9, 7); }
        public bool Winning_Ribbon { get => M.GetBool(10, 0); set => M.SetBool(value, 10, 0); }
        public bool Victory_Ribbon { get => M.GetBool(10, 1); set => M.SetBool(value, 10, 1); }
        public bool Artist_Ribbon { get => M.GetBool(10, 2); set => M.SetBool(value, 10, 2); }
        public bool Effort_Ribbon { get => M.GetBool(10, 3); set => M.SetBool(value, 10, 3); }
        public bool Battle_Champion_Ribbon { get => M.GetBool(10, 4); set => M.SetBool(value, 10, 4); }
        public bool Regional_Champion_Ribbon { get => M.GetBool(10, 5); set => M.SetBool(value, 10, 5); }
        public bool National_Champion_Ribbon { get => M.GetBool(10, 6); set => M.SetBool(value, 10, 6); }
        public bool Country_Ribbon { get => M.GetBool(10, 7); set => M.SetBool(value, 10, 7); }
        public bool National_Ribbon { get => M.GetBool(11, 0); set => M.SetBool(value, 11, 0); }
        public bool Earth_Ribbon { get => M.GetBool(11, 1); set => M.SetBool(value, 11, 1); }
        public bool World_Ribbon { get => M.GetBool(11, 2); set => M.SetBool(value, 11, 2); }
        // byte 11, bits 3-6 (unused, leftover from ribbon bytes)
        public bool Fateful_Encounter { get => M.GetBool(11, 7); set => M.SetBool(value, 11, 7); }


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
                    checksum += bam.GetUShort(i * 2);
            }

            Checksum = checksum;
        }

        protected void ApplyXOR(ByteArrayManipulator subData)
        {
            uint encryptionKey = ID ^ PID;
            for (int i = 0; i < subData.Length / 4; i++) //xor subData with key in 4 byte chunks
            {
                uint chunk = subData.GetUInt(4 * i);
                chunk ^= encryptionKey;
                subData.SetUInt(chunk, 4 * i);
            }
        }

        /// <summary>
        /// Compiles and encrypts the current <see cref="G"/>, <see cref="A"/>, <see cref="E"/>,
        /// and <see cref="M"/> blocks with the current <see cref="PID"/> and <see cref="ID"/>.
        /// </summary>
        /// <returns>A 48 byte encrypted sub-data array.</returns>
        protected ByteArrayManipulator GetEncryptedSubData()
        {
            ByteArrayManipulator subData = new(4 * BLOCK_SIZE, BIG_ENDIANESS);
            string order = SUBSTRUCTURE_ORDER[PID % SUBSTRUCTURE_ORDER.Length];
            subData.SetBytes(G, BLOCK_SIZE * order.IndexOf('G'));
            subData.SetBytes(A, BLOCK_SIZE * order.IndexOf('A'));
            subData.SetBytes(E, BLOCK_SIZE * order.IndexOf('E'));
            subData.SetBytes(M, BLOCK_SIZE * order.IndexOf('M'));

            ApplyXOR(subData);
            return subData;
        }

        protected (ByteArrayManipulator G, ByteArrayManipulator A, ByteArrayManipulator E, ByteArrayManipulator M)
            UnencryptSubData(ByteArrayManipulator subData)
        {
            ApplyXOR(subData);

            string order = SUBSTRUCTURE_ORDER[PID % SUBSTRUCTURE_ORDER.Length];
            ByteArrayManipulator G = new(subData.GetBytes(BLOCK_SIZE * order.IndexOf('G'), BLOCK_SIZE), BIG_ENDIANESS);
            ByteArrayManipulator A = new(subData.GetBytes(BLOCK_SIZE * order.IndexOf('A'), BLOCK_SIZE), BIG_ENDIANESS);
            ByteArrayManipulator E = new(subData.GetBytes(BLOCK_SIZE * order.IndexOf('E'), BLOCK_SIZE), BIG_ENDIANESS);
            ByteArrayManipulator M = new(subData.GetBytes(BLOCK_SIZE * order.IndexOf('M'), BLOCK_SIZE), BIG_ENDIANESS);

            return (G, A, E, M);
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
        /// be overriden by the <see cref="pkxUtil.EGG_STRING"/> of the game's language.
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
