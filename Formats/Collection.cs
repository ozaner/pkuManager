using System.Collections.Generic;
using System.Drawing;

namespace pkuManager.Formats;

public abstract class Collection
{
    public abstract string FormatName { get; }

    public virtual string Name { get; protected set; }
    public abstract int BoxCount { get; }
    
    public Box CurrentBox { get; protected set; }
    public int CurrentBoxID { get; protected set; }

    public abstract string[] GetBoxNames();
    public abstract void SwitchBox(int boxID);
}

public abstract class Box
{
    public string Name { get; protected set; }
    public Image Background { get; protected set; }
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    public int Capacity => Width is 0 && Height is 0 ? int.MaxValue : Width * Height;
    public SortedDictionary<int, Slot> Data { get; protected set; }
    
    public abstract bool SwapSlots(int slotIDA, int slotIDB);
    public abstract bool ReleaseSlot(int slotID);
    public abstract bool InjectPokemon(FormatObject pkmn);

    public bool RoomForOneMore() => Data.Count < Capacity;
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
    public string[] Forms { get; }
    public string[] Appearance { get; }
    public string Location { get; } // i.e. filename, slot
    public string LocationType { get; } // i.e. "filename", "slot j"
    public string Ball { get; }
    public bool IsShadow { get; }
    public bool IsTrueOT { get; } // most-likely only used for formats supporting pku
    public bool CheckedOut { get; set; } // always false for non pku collections...

    public FormatObject pkmnObj { get; protected set; }

    public Slot(FormatObject pkmnObj, (string, string) boxSprite, (string, string) frontSprite, (string, string) backSprite,
        string nickname, string species, string game, string ot, string[] forms, string[] appearance,
        string ball, bool isShadow, bool isTrueOT, string location, string locationType, bool checkedOut)
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
        IsTrueOT = isTrueOT;
        Location = location;
        LocationType = locationType;
        CheckedOut = checkedOut;
    }
}