using System.Collections.Generic;
using System.Drawing;

namespace pkuManager.Common
{
    public abstract class Collection
    {
        public string collectionName { get; protected set; }

        /// <summary>
        /// Returns an array of all the box names in this collection, with the index representing their ID number in the collection.
        /// </summary>
        /// <returns></returns>
        public abstract string[] GetBoxList();

        public abstract BoxInfo getBoxInfo(int id);

        public abstract byte[] getPKMN(int boxID, int slot);

        public abstract void SwapSlots(int boxID, int slotA, int slotB);

        public abstract void Delete(int boxID, int slot);

        public abstract bool Add(byte[] file, int boxID, int slot);

        public class BoxInfo
        {
            public int width, height;
            public Image background;
            public SortedDictionary<int, SlotInfo> slots;
        }

        public class SlotInfo
        {
            public string iconURL;
            public (string url, string author) frontSprite;
            public (string url, string author) backSprite;
            public string nickname, species, game, OT, location;
            public string[] forms, appearance;
            public string locationIdentifier; //i.e. filename, box location, slot #
            public bool trueOT;
            public string ball;
            public string format;
            public bool checkedOut; //always false for non pku collections...
            public bool hasShadowHaze;
        }
    }
}
