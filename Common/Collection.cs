using System.Collections.Generic;
using System.Drawing;

namespace pkuManager.Common;

public abstract class Collection
{
    public string CollectionName { get; protected set; }

    /// <summary>
    /// Returns all the box names in this collection, sorted by their ID number in the collection.
    /// </summary>
    /// <returns>An array of all the box names in this collection.</returns>
    public abstract string[] GetBoxList();

    public abstract BoxInfo GetBoxInfo(int id);

    public abstract byte[] GetPKMN(int boxID, int slot);

    public abstract void SwapSlots(int boxID, int slotA, int slotB);

    public abstract void Delete(int boxID, int slot);

    public abstract bool Add(byte[] file, int boxID, int slot);

    public class BoxInfo
    {
        public int Width, Height;
        public Image Background;
        public SortedDictionary<int, SlotInfo> Slots;
    }

    public class SlotInfo
    {
        public string IconURL;
        public (string url, string author) FrontSprite;
        public (string url, string author) BackSprite;
        public string Nickname, Species, Game, OT, Location;
        public string[] Forms, Appearance;
        public string LocationIdentifier; //i.e. filename, box location, slot #
        public bool TrueOT;
        public string Ball;
        public string Format;
        public bool CheckedOut; //always false for non pku collections...
        public bool HasShadowHaze;
    }
}