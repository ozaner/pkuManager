using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.Modules;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace pkuManager.Formats.pkx.pk3;

public class pk3Collection : FileCollection
{
    /* ------------------------------------
     * pk3 Save fields
     * ------------------------------------
    */
    public BAMStringField<byte> OT { get; protected set; }
    public ByteArrayManipulator UnshuffledPCBAM { get; protected set; }
    public override IField<BigInteger> CurrentBoxID { get; protected set; }
    public BAMStringField<byte>[] BoxNames { get; protected set; }


    /* ------------------------------------
     * pk3 Save file constants
     * ------------------------------------
    */
    public const int MAX_BOX_CHARS = 9;

    //Save constants
    /// <summary>
    /// Size in bytes of a pk3 GBA save file, i.e. 128 KiB
    /// </summary>
    public const int SAVE_SIZE = 0x20000;
    protected const int GAME_SAVE_SIZE = SECTIONS * SECTION_SIZE;

    //Section constants
    protected const int SECTIONS = 14;
    protected const int SECTION_SIZE = 0x1000;
    protected const int DATA_ADDR = 0x0000;
    protected const int DATA_SIZE = 0xF80;
    protected const int ID_ADDR = 0xFF4;
    protected const int CHECKSUM_ADDR = 0xFF6;
    protected const int SIGNATURE_ADDR = 0xFF8;
    protected const int SIGNATURE = 0x08012025;
    protected const int SAVE_COUNT_ADDR = 0xFFC;

    //PC Buffer constants
    protected const int FIRST_PC_SECTION = 5;
    protected const int PC_SECTIONS = 9;


    /* ------------------------------------
     * Collection variables
     * ------------------------------------
    */
    public override string FormatName => "pk3";
    public override bool BigEndian => false;
    public override int BoxCount => 14;
    public override string Name => $"{OT.ValueAsString}'s PC";

    public pk3Collection(string filename) : base(filename) { }

    protected override bool DetermineValidity()
    {
        if (BAM.Length != SAVE_SIZE) //Save is right size (all GBA saves are 128 kiB)
            return false;
        
        int saveAddress = GetCurrentSaveAddress(); //only checks if current save block is valid

        //signature present in each section
        for (int i = 0; i < SECTIONS; i++)
            if (BAM.Get<uint>(saveAddress + SECTION_SIZE * i + SIGNATURE_ADDR) != SIGNATURE)
                return false;

        //checksums match in each section
        for (int i = 0; i < SECTIONS; i++)
        {
            uint twoByteSum = 0;
            for (int j = 0; j < DATA_SIZE / 4; j++)
                twoByteSum += BAM.Get<uint>(saveAddress + SECTION_SIZE * i + 4 * j);
            ushort checksum = (ushort)(twoByteSum.GetBits(0, 16) + twoByteSum.GetBits(16, 16));
            if (checksum != BAM.Get<ushort>(saveAddress + SECTION_SIZE * i + CHECKSUM_ADDR))
                return false;
        }

        //if current box is not 0-13, set it to 0 in Init()
        return true;
    }

    protected int GetCurrentSaveAddress()
    {
        uint saveAcount = BAM.Get<uint>(GAME_SAVE_SIZE - SECTION_SIZE + SAVE_COUNT_ADDR); //use last section
        uint saveBcount = BAM.Get<uint>(GAME_SAVE_SIZE * 2 - SECTION_SIZE + SAVE_COUNT_ADDR); //use last section
        return saveAcount > saveBcount ? 0 : GAME_SAVE_SIZE; //tie -> save b
    }

    protected override void Init()
    {
        int saveAddress = GetCurrentSaveAddress();

        //Get relevant section IDs
        int[] boxBufferSections = new int[PC_SECTIONS];
        int trainerInfoSection = 0;
        for (int i = 0; i < SECTIONS; i++)
        {
            int sectionID = BAM.Get<ushort>(saveAddress + SECTION_SIZE * i + ID_ADDR);
            if (sectionID == 0)
                trainerInfoSection = i;
            else if (sectionID >= FIRST_PC_SECTION)
                boxBufferSections[sectionID - FIRST_PC_SECTION] = i;
        }

        //Compute Trainer Info section address
        int trainerInfoAddress = saveAddress + SECTION_SIZE * trainerInfoSection;

        //Compute PC Buffer ranges
        (int, int)[] pcBufferRanges = new (int, int)[PC_SECTIONS];
        for (int i = 0; i < PC_SECTIONS; i++)
            pcBufferRanges[i] = (saveAddress + SECTION_SIZE * boxBufferSections[i], DATA_SIZE);

        //Determine Japanese-ness
        //  International games pad OT with 0xFF's while JPN OTs end in two 0x00's
        //  This is the best we got...
        bool isJapanese = BAM.Get<ushort>(trainerInfoAddress + 0x0000 + pk3Object.MAX_OT_CHARS - 2) is 0;

        //Init fields
        OT = new(BAM, trainerInfoAddress + 0x0000, pk3Object.MAX_OT_CHARS, FormatName);
        UnshuffledPCBAM = new(BAM, BigEndian, pcBufferRanges);
        CurrentBoxID = new BAMIntegralField(UnshuffledPCBAM, 0x0000, 4);
        if (CurrentBoxID.Value > 14)
            CurrentBoxID.Value = 0; //currentbox too large, reset

        BoxNames = new BAMStringField<byte>[BoxCount];
        for (int i = 0; i < BoxCount; i++)
            BoxNames[i] = new(UnshuffledPCBAM, 0x8344 + MAX_BOX_CHARS * i, MAX_BOX_CHARS, FormatName,
                isJapanese ? Language.Japanese : Language.English, pk3Object.IsValidLang);
    }

    protected override void PreSave()
    {
        int saveAddress = GetCurrentSaveAddress(); //only checks if current save block is valid

        //recalculate checksums
        for (int i = 0; i < SECTIONS; i++)
        {
            uint twoByteSum = 0;
            for (int j = 0; j < DATA_SIZE / 4; j++)
                twoByteSum += BAM.Get<uint>(saveAddress + SECTION_SIZE * i + 4 * j);
            ushort checksum = (ushort)(twoByteSum.GetBits(0, 16) + twoByteSum.GetBits(16, 16));

            BAM.Set(checksum, saveAddress + SECTION_SIZE * i + CHECKSUM_ADDR);
        }
    }

    public override string[] GetBoxNames()
    {
        string[] names = new string[BoxCount];
        for (int i = 0; i < BoxCount; i++)
            names[i] = BoxNames[i].ValueAsString;
        return names;
    }

    protected override pk3Box CreateBox(int boxID)
        => new(boxID, UnshuffledPCBAM);
}

public class pk3Box : Box
{
    //PC Buffer constants
    protected const int PC_BOXES_ADDR = 0x0004;
    protected const int PC_BOX_SIZE = pk3Object.FILE_SIZE_PC * 30;

    // Box vars
    public override int Width => 6;
    public override int Height => 5;

    // pk3Box vars
    protected ByteArrayManipulator UnshuffledPCBAM;
    protected int BoxID;


    // pk3Box methods
    public pk3Box(int boxID, ByteArrayManipulator unshuffledPCBAM)
    {
        BoxID = boxID;
        UnshuffledPCBAM = unshuffledPCBAM;
    }

    //indexing: 1-30
    protected int GetSlotAddress(int slotID)
        => PC_BOXES_ADDR + PC_BOX_SIZE * BoxID + (slotID-1) * pk3Object.FILE_SIZE_PC;

    protected byte[] ReadSlotRaw(int slotID)
        => UnshuffledPCBAM.GetArray<byte>(GetSlotAddress(slotID), pk3Object.FILE_SIZE_PC);

    protected void SetSlotRaw(byte[] bytes, int slotID)
        => UnshuffledPCBAM.SetArray(GetSlotAddress(slotID), bytes[0..pk3Object.FILE_SIZE_PC]);


    // Box methods
    public override IEnumerable<(int, FormatObject)> ReadBox()
    {
        for (int i = 1; i <= Capacity; i++)
        {
            byte[] bytes = ReadSlotRaw(i);
            if (bytes.All(x => x is 0))
                continue; //empty
            pk3Object pk3 = new();
            pk3.FromEncryptedFile(bytes);
            yield return (i, pk3);
        }
    }

    public override Slot CreateSlotInfo(FormatObject pkmn)
    {
        pk3Object pk3 = pkmn as pk3Object;
        int form = pk3.Species.GetAs<int>() switch
        {
            210 => pk3Object.GetUnownFormID(pk3.PID.GetAs<uint>()),
            410 => pk3.Origin_Game.GetAs<int>() switch //deoxys
            {
                3 => 3, //E -> speed
                4 => 1, //FR -> attack
                5 => 2, //LG -> defense
                _ => 0  //RS and all else -> normal
            },
            _ => 0
        };

        //assume gender is male to get gender ratio
        DexUtil.SFA sfa = DexUtil.GetSFAFromIndices<int?>("pk3", pk3.Species.GetAs<int>(), form, false);

        if (sfa.Species is not null)
        {
            //get gender ratio
            var gr = pkxUtil.GetGenderRatio(sfa);
            bool female = gr is GenderRatio.All_Female ||
                          gr is not GenderRatio.All_Male or GenderRatio.All_Genderless
                          && (int)gr > pk3.PID.Value % 256;

            //gender determined, now get true SFA
            sfa = DexUtil.GetSFAFromIndices<int?>("pk3", pk3.Species.GetAs<int>(), form, female);
        }

        //get other info
        bool shiny = pkxUtil.IsPIDShiny(pk3.PID.GetAs<uint>(), pk3.TID.GetAs<uint>(), false);
        var sprites = ImageUtil.GetSprites(sfa, shiny, pk3.Is_Egg.ValueAsBool);
        Language lang = (Language)pk3.Language.GetAs<int>();
        lang = pk3Object.IsValidLang(lang) ? lang : Language.English;

        return new(
            pk3,
            sprites[0],
            sprites[1],
            sprites[2],
            DexUtil.CharEncoding<byte>.Decode(pk3.Nickname.GetAs<byte>(), "pk3", lang),
            sfa.Species,
            GAME_DEX.SearchIndexedValue<int?>(pk3.Origin_Game.GetAs<int>(), "pk3", "Indices", "$x"),
            DexUtil.CharEncoding<byte>.Decode(pk3.OT.GetAs<byte>(), "pk3", lang),
            sfa.Form, //forms
            null, //appearance
            BALL_DEX.SearchIndexedValue<int?>(pk3.Ball.GetAs<int>(), "pk3", "Indices", "$x"),
            false //can't be shadow
        );
    }

    public override bool ClearSlot(int slotID)
    {
        SetSlotRaw(new byte[pk3Object.FILE_SIZE_PC], slotID);
        return true;
    }

    public override bool SwapSlots(int slotIDA, int slotIDB)
    {
        //physically swap the slots in save file.
        byte[] pk3A = ReadSlotRaw(slotIDA);
        byte[] pk3B = ReadSlotRaw(slotIDB);
        SetSlotRaw(pk3B, slotIDA);
        SetSlotRaw(pk3A, slotIDB);

        return true; //mission accomplished
    }
}

