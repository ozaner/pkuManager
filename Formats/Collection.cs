using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace pkuManager.Formats;

public abstract class Collection
{
    public abstract string FormatName { get; }

    public string Location { get; }
    public bool IsCollectionValid { get; }
    public abstract string Name { get; }
    public abstract int BoxCount { get; }
    public abstract IField<BigInteger> CurrentBoxID { get; protected set; }
    public Box CurrentBox { get; protected set; }

    protected virtual void PreInit() { }
    protected abstract bool DetermineValidity();
    protected abstract void Init();

    public Collection(string location)
    {
        Location = location;
        PreInit();
        IsCollectionValid = DetermineValidity();
        if (IsCollectionValid)
        {
            Init();
            CurrentBox = CreateBox(CurrentBoxID.GetAs<int>());
        }
    }

    public abstract string[] GetBoxNames();
    protected abstract Box CreateBox(int boxID);

    public void SwitchBox(int boxID)
    {
        CurrentBoxID.Value = boxID;
        CurrentBox = CreateBox(boxID);
    }
}

public abstract class FileCollection : Collection
{
    public ByteArrayManipulator BAM { get; private set; }
    public abstract bool BigEndian { get; }

    public FileCollection(string filename) : base(filename) { }

    protected override void PreInit()
        => BAM = new(File.ReadAllBytes(Location), BigEndian);

    protected virtual void PreSave() { }

    public bool Save()
    {
        PreSave();
        try
        {
            File.WriteAllBytes(Location, BAM);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public abstract class Box
{
    public Image Background { get; protected set; }
    public abstract int Width { get; }
    public abstract int Height { get; }
    public int Capacity => Width is 0 && Height is 0 ? int.MaxValue : Width * Height;

    public abstract IEnumerable<(int, FormatObject)> ReadBox();
    public abstract Slot CreateSlotInfo(FormatObject pkmn);
    public abstract int NextAvailableSlot();
    public abstract bool SetSlot(FormatObject pkmn, int slotID);
    public abstract bool SwapSlots(int slotIDA, int slotIDB);
    public abstract bool ClearSlot(int slotID);

    /// <summary>
    /// Attempts to inject a pkmn into the first available slot of the box.
    /// </summary>
    /// <param name="pkmn">A pkmn that matches the format of this Collection.</param>
    /// <returns>The slotID and Slot of the added pkmn, or (-1, null) if it failed.</returns>
    public (int, Slot) TryInjectPKMN(FormatObject pkmn)
    {
        int slotID = NextAvailableSlot();
        if (slotID != -1 && SetSlot(pkmn, slotID))
            return (slotID, CreateSlotInfo(pkmn)); //success
        return (-1, null); //failure
    }
}

public class Slot
{
    public (string url, string author) BoxSprite { get; }
    public (string url, string author) FrontSprite { get; }
    public (string url, string author) BackSprite { get; }
    public string Nickname { get; }
    public string Species { get; }
    public string Game { get; }
    public string OT { get; }
    public string Forms { get; }
    public string Appearance { get; }
    public string Ball { get; }
    public bool IsShadow { get; }

    //pku stuff
    public bool IsTrueOT { get; protected set; } // most-likely only used for formats supporting pku
    public bool CheckedOut { get; set; } // always false for non pku collections...
    public string Filename { get; protected set; }

    public FormatObject pkmnObj { get; }

    public Slot(FormatObject pkmnObj, (string, string) boxSprite, (string, string) frontSprite,
        (string, string) backSprite, string nickname, string species, string game, string ot,
        string forms, string appearance, string ball, bool isShadow)
    {
        this.pkmnObj = pkmnObj;
        BoxSprite = boxSprite;
        FrontSprite = frontSprite;
        BackSprite = backSprite;
        Nickname = nickname;
        Species = species;
        Game = game;
        OT = ot;
        Forms = forms;
        Appearance = appearance;
        Ball = ball;
        IsShadow = isShadow;
    }

    public void AddPKUData(bool checkedOut, bool isTrueOT, string filename)
    {
        CheckedOut = checkedOut;
        IsTrueOT = isTrueOT;
        Filename = filename;
    }
}