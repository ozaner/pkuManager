using pkuManager.Common;
using System.Windows.Forms;
using static pkuManager.Common.Collection;
using static pkuManager.pku.PKUCollection.PKUBoxConfig;

namespace pkuManager.pku
{
    public class PKUCollectionManager : CollectionManager
    {
        public PKUCollectionManager(PKUCollection collection, Form form) : base(collection, form) { }

        public bool CanChangeCurrentBoxType(BoxConfigType type)
        {
            return ((PKUCollection)collection).CanChangeBoxType(currentBoxID, type);
        }

        public void ChangeCurrentBoxType(BoxConfigType type)
        {
            ((PKUCollection)collection).ChangeBoxType(currentBoxID, type);
            RegenerateBoxDisplay(); //regenerates box display to be the new size.
        }

        public void SetBattleStatOverrideFlag(bool val)
        {
            ((PKUCollection)collection).SetBattleStatOverrideFlag(val);
        }

        public GlobalFlags GetGlobalFlags()
        {
            return ((PKUCollection)collection).GetGlobalFlags();
        }

        public void CheckOut(SlotInfo slotInfo)
        {
            ((PKUCollection)collection).CheckOut(currentBoxID, slotInfo);
        }
    }
}
