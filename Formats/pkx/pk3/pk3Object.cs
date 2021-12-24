using pkuManager.Common;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Linq;
using System;
using pkuManager.Formats.Fields.BAMFields;
using System.Numerics;
using pkuManager.Formats.Modules;
using pkuManager.Formats.Fields;
using OneOf;

namespace pkuManager.Formats.pkx.pk3;

/// <summary>
/// An implementation of the .pk3 format used by the generation 3 GBA games.<br/>
/// Implementation details mostly referenced from
/// <see href="https://bulbapedia.bulbagarden.net/wiki/Pokémon_data_structure_(Generation_III)">Bulbapedia</see>.
/// </summary>
public class pk3Object : FormatObject, Species_O, Item_O, TID_O, Friendship_O,
                         IVs_O, EVs_O, Contest_Stats_O, Ball_O, Met_Level_O,
                         OT_Gender_O, Language_O
{
    /* ------------------------------------
     * Initialization
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

    // initializes fields
    public pk3Object()
    {
        //Rearranges pk3 battle stats to match modern indices (i.e. H/A/SA/S/D/SD <-> H/A/SA/D/SD/S)
        static BigInteger[] getStats(BigInteger[] x) { DataUtil.Permutate(x, (3, 5), (3, 4)); return x; };
        static BigInteger[] setStats(BigInteger[] x) { DataUtil.Permutate(x, (3, 5), (4, 5)); return x; };

        // Non-Subdata
        PID = new(NonSubData, 0, 4);
        TID = new(NonSubData, 4, 4);
        Nickname = new(NonSubData, 8, 1, 10);
        Language = new(NonSubData, 18, 1);
        Egg_Name_Override = new(NonSubData, 19, 1);
        OT = new(NonSubData, 20, 1, 7);
        MarkingCircle = new(NonSubData, 27, 0);
        MarkingSquare = new(NonSubData, 27, 1);
        MarkingTriangle = new(NonSubData, 27, 2);
        MarkingHeart = new(NonSubData, 27, 3);
        Unused_A = new(NonSubData, 27, 4, 4); // leftover from markings byte
        Checksum = new(NonSubData, 28, 2);
        Unused_B = new(NonSubData, 30, 2); // probably padding

        // Block G
        Species = new(G, 0, 2);
        Item = new(G, 2, 2);
        Experience = new(G, 4, 4);
        PP_Ups = new(G, 8, 0, 2, 4);
        Friendship = new(G, 9, 1);
        Unused_C = new(G, 10, 2); // probably padding

        // Block A
        Moves = new(A, 0, 2, 4);
        PP = new(A, 8, 1, 4);

        // Block E
        EVs = new(E, 0, 1, 6, getStats, setStats);
        Contest_Stats = new(E, 6, 1, 6);

        // Block M
        PKRS_Days = new(M, 0, 0, 4);
        PKRS_Strain = new(M, 0, 4, 4);
        Met_Location = new(M, 1, 1);
        Met_Level = new(M, 2, 0, 7);
        Origin_Game = new(M, 2, 7, 4);
        Ball = new(M, 2, 11, 4);
        OT_Gender = new(M, 2, 15);
        IVs = new(M, 4, 0, 5, 6, getStats, setStats);
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
        file.SetArray<byte>(0, NonSubData); // First 32 bytes
        file.SetArray<byte>(NON_SUBDATA_SIZE, subData); // Last (encrypted) 48 bytes

        return file;
    }

    public override void FromFile(byte[] file)
    {
        NonSubData.SetArray(0, file, NON_SUBDATA_SIZE);
        UnencryptSubData(new ByteArrayManipulator(file[NON_SUBDATA_SIZE..FILE_SIZE_PC], BIG_ENDIANESS));
    }
        

    /* ------------------------------------
     * Non-Subdata
     * ------------------------------------
    */
    public BAMIntegralField PID { get; }
    public BAMIntegralField TID { get; }
    public BAMArrayField Nickname { get; }
    public BAMIntegralField Language { get; }
    public BAMIntegralField Egg_Name_Override { get; }
    public BAMArrayField OT { get; }
    public BAMBoolField MarkingCircle { get; }
    public BAMBoolField MarkingSquare { get; }
    public BAMBoolField MarkingTriangle { get; }
    public BAMBoolField MarkingHeart { get; }
    public BAMIntegralField Unused_A { get; }
    public BAMIntegralField Checksum { get; }
    public BAMIntegralField Unused_B { get; }


    /* ------------------------------------
     * G: Growth Block
     * ------------------------------------
    */
    public BAMIntegralField Species { get; }
    public BAMIntegralField Item { get; }
    public BAMIntegralField Experience { get; }
    public BAMArrayField PP_Ups { get; }
    public BAMIntegralField Friendship { get; }
    public BAMIntegralField Unused_C { get; }


    /* ------------------------------------
     * A: Attacks Block
     * ------------------------------------
    */
    public BAMArrayField Moves { get; }
    public BAMArrayField PP { get; }


    /* ------------------------------------
     * E: EVs & Condition Block
     * ------------------------------------
    */
    public BAMArrayField EVs { get; }
    public BAMArrayField Contest_Stats { get; }


    /* ------------------------------------
     * M: Misc. Block
     * ------------------------------------
    */
    public BAMIntegralField PKRS_Days { get; }
    public BAMIntegralField PKRS_Strain { get; }

    public BAMIntegralField Met_Location { get; }
    public BAMIntegralField Met_Level { get; }
    public BAMIntegralField Origin_Game { get; }
    public BAMIntegralField Ball { get; }
    public BAMBoolField OT_Gender { get; }

    public BAMArrayField IVs { get; }
    public BAMBoolField Is_Egg { get; }
    public BAMBoolField Ability_Slot { get; }

    public BAMIntegralField Cool_Ribbon_Rank { get; }
    public BAMIntegralField Beauty_Ribbon_Rank { get; }
    public BAMIntegralField Cute_Ribbon_Rank { get; }
    public BAMIntegralField Smart_Ribbon_Rank { get; }
    public BAMIntegralField Tough_Ribbon_Rank { get; }
    public BAMBoolField Champion_Ribbon { get; }
    public BAMBoolField Winning_Ribbon { get; }
    public BAMBoolField Victory_Ribbon { get; }
    public BAMBoolField Artist_Ribbon { get; }
    public BAMBoolField Effort_Ribbon { get; }
    public BAMBoolField Battle_Champion_Ribbon { get; }
    public BAMBoolField Regional_Champion_Ribbon { get; }
    public BAMBoolField National_Champion_Ribbon { get; }
    public BAMBoolField Country_Ribbon { get; }
    public BAMBoolField National_Ribbon { get; }
    public BAMBoolField Earth_Ribbon { get; }
    public BAMBoolField World_Ribbon { get; }
    public BAMIntegralField Unused_D { get; }
    public BAMBoolField Fateful_Encounter { get; }


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
        Checksum.SetAs(checksum);
    }

    protected void ApplyXOR(ByteArrayManipulator subData)
    {
        uint encryptionKey = TID.GetAs<uint>() ^ PID.GetAs<uint>();
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
        string order = SUBSTRUCTURE_ORDER[PID.GetAs<uint>() % SUBSTRUCTURE_ORDER.Length];
        subData.SetArray<byte>(BLOCK_SIZE * order.IndexOf('G'), G);
        subData.SetArray<byte>(BLOCK_SIZE * order.IndexOf('A'), A);
        subData.SetArray<byte>(BLOCK_SIZE * order.IndexOf('E'), E);
        subData.SetArray<byte>(BLOCK_SIZE * order.IndexOf('M'), M);

        ApplyXOR(subData);
        return subData;
    }

    protected void UnencryptSubData(ByteArrayManipulator subData)
    {
        ApplyXOR(subData);

        string order = SUBSTRUCTURE_ORDER[PID.GetAs<uint>() % SUBSTRUCTURE_ORDER.Length];
        G.SetArray(0, subData.GetArray<byte>(BLOCK_SIZE * order.IndexOf('G'), BLOCK_SIZE));
        A.SetArray(0, subData.GetArray<byte>(BLOCK_SIZE * order.IndexOf('A'), BLOCK_SIZE));
        E.SetArray(0, subData.GetArray<byte>(BLOCK_SIZE * order.IndexOf('E'), BLOCK_SIZE));
        M.SetArray(0, subData.GetArray<byte>(BLOCK_SIZE * order.IndexOf('M'), BLOCK_SIZE));
    }


    /* ------------------------------------
     * Character Encoding
     * ------------------------------------
    */
    /// <summary>
    /// Determines whether the given langauge exists in pk3.
    /// </summary>
    public static bool IsValidLang(Language lang) => lang is
        Common.Language.Japanese or
        Common.Language.English or
        Common.Language.French or
        Common.Language.Italian or
        Common.Language.German or
        Common.Language.Spanish;

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
     * Form Encoding 
     * ------------------------------------
    */
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
    /// When <see cref="Egg_Name_Override"/> is set to this value, the pk3 nickname will<br/>
    /// be overriden by the <see cref="pkxUtil.EGG_NICKNAME"/> of the game's language.
    /// </summary>
    public const byte EGG_NAME_OVERRIDE_CONST = 0x06;

    /// <summary>
    /// Returns the rank of the given Gen 3 contest category.
    /// </summary>
    /// <param name="firstRibbon">The first ribbon in the contest category to check.</param>
    /// <param name="ribbons">A list of ribbons to check for the highest ranking ribbon.</param>
    /// <returns>The numeric rank of the contest category ribbon
    ///          (given by <paramref name="firstRibbon"/>) in <paramref name="firstRibbon"/>.</returns>
    public static int GetRibbonRank(Ribbon firstRibbon, HashSet<Ribbon> ribbons)
    {
        if (firstRibbon is not (Ribbon.Cool_G3 or Ribbon.Beauty_G3 or Ribbon.Cute_G3 or Ribbon.Smart_G3 or Ribbon.Tough_G3))
            throw new ArgumentException($"{nameof(firstRibbon)} must be one of the normal rank contest ribbons.", nameof(firstRibbon));

        return new[]
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
    public static bool IsValidRibbon(Ribbon ribbon) => ribbon is
        (>= Ribbon.Cool_G3 and <= Ribbon.Tough_Master_G3) or Ribbon.Champion or
        Ribbon.Winning or Ribbon.Victory or Ribbon.Artist or Ribbon.Effort or 
        Ribbon.Battle_Champion or Ribbon.Regional_Champion or Ribbon.National_Champion or 
        Ribbon.Country or Ribbon.National or Ribbon.Earth or Ribbon.World;


    /* ------------------------------------
     * Duct Tape
     * ------------------------------------
    */
    IntegralField Species_O.Species => Species;
    OneOf<IntegralField, Field<string>> Item_O.Item => Item;
    IntegralField Friendship_O.Friendship => Friendship;
    IntegralField TID_O.TID => TID;
    IntegralArrayField IVs_O.IVs => IVs;
    IntegralArrayField EVs_O.EVs => EVs;
    IntegralArrayField Contest_Stats_O.Contest_Stats => Contest_Stats;
    OneOf<IntegralField, Field<string>> Ball_O.Ball => Ball;
    IntegralField Met_Level_O.Met_Level => Met_Level;
    OneOf<IntegralField, Field<Gender>, Field<Gender?>, Field<bool>> OT_Gender_O.OT_Gender => OT_Gender;
    IntegralField Language_O.Language => Language;
}