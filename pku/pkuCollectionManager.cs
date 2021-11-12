using pkuManager.Common;
using pkuManager.Utilities;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static pkuManager.Common.Collection;
using static pkuManager.pku.pkuCollection.PKUBoxConfig;

namespace pkuManager.pku;

public class pkuCollectionManager : CollectionManager
{
    public pkuCollectionManager(pkuCollection collection) : base(collection) { }

    public bool CanChangeCurrentBoxType(BoxConfigType type)
        => (collection as pkuCollection).CanChangeBoxType(currentBoxID, type);

    public void ChangeCurrentBoxType(BoxConfigType type)
    {
        (collection as pkuCollection).ChangeBoxType(currentBoxID, type);
        RegenerateBoxDisplay(); //regenerates box display to be the new size.
    }

    public void SetBattleStatOverrideFlag(bool val)
        => (collection as pkuCollection).SetBattleStatOverrideFlag(val);

    public GlobalFlags GetGlobalFlags()
        => (collection as pkuCollection).GetGlobalFlags();

    public void CheckOut(SlotInfo slotInfo)
        => (collection as pkuCollection).CheckOut(currentBoxID, slotInfo);

    public void CheckIn(SlotInfo slotInfo)
        => (collection as pkuCollection).CheckIn(currentBoxID, slotInfo);

    public void AddNewBox(string boxName)
    {
        (collection as pkuCollection).AddNewBox(boxName);
        currentBoxID = GetBoxList().Length - 1;
        (collection as pkuCollection).SwitchCurrrentBox(currentBoxID);
        RegenerateBoxDisplay();
    }

    public void RemoveCurrentBox()
    {
        DialogResult dr = MessageBox.Show("Note that this will only remove the box from the collection config, and will NOT delete the underlying folder (i.e. the pku files won't be deleted). Continue?", "Remove Current Box", MessageBoxButtons.YesNo);
        if (dr is DialogResult.Yes)
        {
            (collection as pkuCollection).RemoveBox(currentBoxID);
            currentBoxID = 0;
            RegenerateBoxDisplay();
        }
    }

    public void OpenCurrentBoxInFileExplorer()
        => (collection as pkuCollection).OpenBoxInFileExplorer(currentBoxID);

    public bool RoomForOneMore()
        => (collection as pkuCollection).RoomForOneMore(currentBoxID);

    public bool AddToCurrentBox(pkuObject pku)
        => AddToCurrentBox(Encoding.UTF8.GetBytes(pku.Serialize()), -1);

    public BoxConfigType GetCurrentBoxType()
        => (collection as pkuCollection).GetCurrentBoxType(currentBoxID);

    public new void SwitchCurrentBox(int boxID)
    {
        currentBoxID = boxID;
        (collection as pkuCollection).SwitchCurrrentBox(boxID);
        RegenerateBoxDisplay();
    }

    public static string CreateACollection()
    {
        FolderBrowserDialog fd = new()
        {
            ShowNewFolderButton = true //doesn't show up, probabaly oversight in net 5 update of winforms...
        };

        if(fd.ShowDialog() is DialogResult.OK)
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
        => File.Exists(@$"{path}\collectionConfig.json");
}