using pkuManager.Common;
using pkuManager.GUI;
using System;
using System.Windows.Forms;
using static pkuManager.Common.Collection;
using static pkuManager.GUI.BoxDisplay;

namespace pkuManager
{
    public class CollectionManager
    {
        protected Collection collection;
        protected int currentBoxID;
        protected int currentSlotID;

        public event EventHandler BoxDisplayUpdated;
        public event EventHandler SlotSelected;

        public BoxDisplay BoxDisplay { get; private set; }

        public CollectionManager(Collection collection, Form mw)
        {
            collection.Lock(); //lock the Collection before reading/writing to it

            //Failsafe to make sure collection unlocks when ManagerWindow closes.
            mw.FormClosing += (object s, FormClosingEventArgs e) => { Unlock(); };

            this.collection = collection;
            currentBoxID = 0;
            RegenerateBoxDisplay();
        }

        public string GetCollectionName()
        {
            return collection.collectionName;
        }

        public string[] GetBoxList()
        {
            return collection.GetBoxList();
        }

        public void SwitchCurrentBox(int boxID)
        {
            currentBoxID = boxID;
            RegenerateBoxDisplay();
        }

        protected void RegenerateBoxDisplay()
        {
            BoxDisplay = new BoxDisplay(collection.getBoxInfo(currentBoxID));
            BoxDisplay.SlotSelected += OnSlotSelected; //Get new SlotSelected event handler
            BoxDisplay.SlotsSwapped += OnSlotsSwapped;
            OnBoxDisplayUpdated(null); //BoxDisplay updated, invoke event
        }

        //Invoked whenever the BoxDisplay is updated (i.e. when RegenerateBoxDisplay is called)
        //Calls a swap on the given slots on the collection.
        protected virtual void OnSlotsSwapped(object sender, EventArgs e)
        {
            SlotsSwappedEventArgs args = (SlotsSwappedEventArgs)e;
            collection.SwapSlots(currentBoxID, args.slotA, args.slotB);
        }

        //Invoked whenever the BoxDisplay is updated (i.e. when RegenerateBoxDisplay is called)
        //Just passes the event call down to the BoxDisplay event
        protected virtual void OnBoxDisplayUpdated(EventArgs e)
        {
            EventHandler handler = BoxDisplayUpdated;
            handler?.Invoke(this, e);
        }

        //Invoked whenever the a slot is selected in the BoxDisplay is updated (i.e. is chained to the SlotSelected event of the current BoxDisplay)
        //Just passes the event call down to the SlotSelected event
        protected virtual void OnSlotSelected(object s, EventArgs e)
        {
            EventHandler handler = SlotSelected;
            (SlotInfo, byte[]) obj = (((SlotDisplay)s).slotInfo, collection.getPKMN(currentBoxID, ((SlotDisplay)s).slotID));
            handler?.Invoke(obj, e);
        }

        public void Unlock()
        {
            collection.Unlock();
        }
    }
}
