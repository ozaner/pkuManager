using pkuManager.Common;
using pkuManager.Utilities;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static pkuManager.Common.Collection;
using static pkuManager.pku.pkuCollection.PKUBoxConfig;

namespace pkuManager.pku
{
    public class pkuCollectionManager : CollectionManager
    {
        public pkuCollectionManager(pkuCollection collection) : base(collection) { }

        public bool CanChangeCurrentBoxType(BoxConfigType type)
        {
            return ((pkuCollection)collection).CanChangeBoxType(currentBoxID, type);
        }

        public void ChangeCurrentBoxType(BoxConfigType type)
        {
            ((pkuCollection)collection).ChangeBoxType(currentBoxID, type);
            RegenerateBoxDisplay(); //regenerates box display to be the new size.
        }

        public void SetBattleStatOverrideFlag(bool val)
        {
            ((pkuCollection)collection).SetBattleStatOverrideFlag(val);
        }

        public GlobalFlags GetGlobalFlags()
        {
            return ((pkuCollection)collection).GetGlobalFlags();
        }

        public void CheckOut(SlotInfo slotInfo)
        {
            ((pkuCollection)collection).CheckOut(currentBoxID, slotInfo);
        }

        public void CheckIn(SlotInfo slotInfo)
        {
            ((pkuCollection)collection).CheckIn(currentBoxID, slotInfo);
        }

        public void AddNewBox(string boxName)
        {
            ((pkuCollection)collection).AddNewBox(boxName);
            currentBoxID = GetBoxList().Length - 1;
            ((pkuCollection)collection).SwitchCurrrentBox(currentBoxID);
            RegenerateBoxDisplay();
        }

        public void RemoveCurrentBox()
        {
            DialogResult dr = MessageBox.Show("Note that this will only remove the box from the collection config, and will NOT delete the underlying folder (i.e. the pku files won't be deleted). Continue?", "Remove Current Box", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                ((pkuCollection)collection).RemoveBox(currentBoxID);
                currentBoxID = 0;
                RegenerateBoxDisplay();
            }
        }

        public void OpenCurrentBoxInFileExplorer()
        {
            ((pkuCollection)collection).OpenBoxInFileExplorer(currentBoxID);
        }

        public bool RoomForOneMore()
        {
            return ((pkuCollection)collection).RoomForOneMore(currentBoxID);
        }

        public bool AddToCurrentBox(pkuObject pku)
        {
            return AddToCurrentBox(Encoding.UTF8.GetBytes(pku.Serialize()), -1);
        }

        public BoxConfigType GetCurrentBoxType()
        {
            return ((pkuCollection)collection).GetCurrentBoxType(currentBoxID);
        }

        public new void SwitchCurrentBox(int boxID)
        {
            currentBoxID = boxID;
            ((pkuCollection)collection).SwitchCurrrentBox(boxID);
            RegenerateBoxDisplay();
        }


        public static string CreateACollection()
        {
            FolderBrowserDialog fd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true //doesn't show up, probabaly oversight in net 5 update of winforms...
            };

            if(fd.ShowDialog() == DialogResult.OK)
            {
                if(IsValidPKUCollection(fd.SelectedPath))
                    MessageBox.Show("This folder is already has a collectionConfig.json and thus is a valid Collection.");
                else
                    new pkuCollection.PKUCollectionConfig().ToString().WriteToFile(@$"{fd.SelectedPath}\collectionConfig.json");
                return fd.SelectedPath; //success
            }
            return null; //failure
        }

        public static bool IsValidPKUCollection(string path)
        {
            return File.Exists(@$"{path}\collectionConfig.json");
        }
    }
}
