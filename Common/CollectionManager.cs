using pkuManager.GUI;
using System;
using System.Windows.Forms;
using static pkuManager.Common.Collection;
using static pkuManager.GUI.BoxDisplay;

namespace pkuManager.Common
{
    public class CollectionManager
    {
        protected Collection collection;
        protected int currentBoxID;
        protected int currentSlotID;

        public event EventHandler BoxDisplayRefreshed; //passed to manager window so it knows to redraw the display.
        public event EventHandler SlotSelected; //passed to manager window so it knows what the currently selected slot is.

        public BoxDisplay BoxDisplay { get; protected set; }

        public CollectionManager(Collection collection)
        {
            this.collection = collection;
            currentBoxID = 0;
            //MUST CALL SWITCH CURRENT BOX ONCE AFTER CREATION
        }

        public string GetCollectionName()
        {
            return collection.collectionName;
        }

        public string[] GetBoxList()
        {
            return collection.GetBoxList();
        }

        public bool AddToCurrentBox(byte[] file, int slotID)
        {
            bool success = collection.Add(file, currentBoxID, slotID);
            if(success)
                RegenerateBoxDisplay();
            return success;
        }

        public void SwitchCurrentBox(int boxID)
        {
            currentBoxID = boxID;
            RegenerateBoxDisplay();
        }

        protected void RegenerateBoxDisplay()
        {
            BoxDisplay = new BoxDisplay(collection.getBoxInfo(currentBoxID));
            BoxDisplay.NotifyColectionManager += OnBoxDisplayCommand;
            OnBoxDisplayRefreshed(null); //BoxDisplay updated, invoke event
        }

        //runs commands given to it by the boxdisplay (i.e. select, delete, swap)
        protected virtual void OnBoxDisplayCommand(object sender, EventArgs e)
        {
            NotifyCollectionEventArgs args = (NotifyCollectionEventArgs)e;
            SlotDisplay slotDisplay = (SlotDisplay)sender;

            //Calls a swap on the given slots on the collection.
            if (args.command == "swap")
                collection.SwapSlots(currentBoxID, args.intA, args.intB);

            //Invoked whenever the a slot is selected in the BoxDisplay is updated (i.e. is chained to the SlotSelected event of the current BoxDisplay)
            //Just passes the event call down to the SlotSelected event
            else if (args.command == "select")
            {
                (SlotInfo, byte[]) obj = slotDisplay == null ? (null, null) : (slotDisplay.slotInfo, collection.getPKMN(currentBoxID, slotDisplay.slotID));
                SlotSelected?.Invoke(obj, e);
            }

            //When the "Delete" option on a SlotDisplay context menu is chosen.
            else if (args.command == "delete")
            {
                DialogResult dialogResult = MessageBox.Show($"This will {(Properties.Settings.Default.Send_to_Recycle ? $"send {slotDisplay.slotInfo.nickname} to the Recycle Bin" : $"permanently delete {slotDisplay.slotInfo.nickname}")}, are you sure?", "Confirm Release", MessageBoxButtons.YesNo);
                if(dialogResult == DialogResult.Yes)
                {
                    collection.Delete(currentBoxID, slotDisplay.slotID);
                    RegenerateBoxDisplay();
                    MessageBox.Show($"{slotDisplay.slotInfo.nickname} was released.\nBye-Bye, {slotDisplay.slotInfo.nickname}!", "Goodbye");
                }
            }
        }

        //Invoked whenever the BoxDisplay is updated (i.e. when RegenerateBoxDisplay is called)
        //Just passes the event call down to the BoxDisplay event
        protected virtual void OnBoxDisplayRefreshed(EventArgs e)
        {
            BoxDisplayRefreshed?.Invoke(this, e);
        }
    }
}
