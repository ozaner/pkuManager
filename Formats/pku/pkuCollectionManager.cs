using pkuManager.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using static pkuManager.Formats.pku.pkuBox.pkuBoxConfig;

namespace pkuManager.Formats.pku;

public class pkuCollectionManager : CollectionManager
{
    protected pkuCollection pkuCollection => Collection as pkuCollection;
    protected pkuBox CurrentBox => pkuCollection.CurrentBox as pkuBox;


    public pkuCollectionManager(pkuCollection collection) : base(collection) { }


    /* ------------------------------------
     * pku Collection Methods
     * ------------------------------------
    */
    public static string CreateACollection()
    {
        FolderBrowserDialog fd = new()
        {
            ShowNewFolderButton = true, //doesn't show up, probabaly oversight in net 5 update of winforms...
        };

        if (fd.ShowDialog() is DialogResult.OK)
        {
            if (IsValidPKUCollection(fd.SelectedPath))
                MessageBox.Show("This folder is already has a collectionConfig.json and thus is a valid collection.");
            else
                new pkuCollection.PKUCollectionConfig().ToString().WriteToFile(@$"{fd.SelectedPath}\collectionConfig.json");
            return fd.SelectedPath; //success
        }
        return null; //failure
    }

    public static bool IsValidPKUCollection(string path)
        => File.Exists(@$"{path}\collectionConfig.json");


    /* ------------------------------------
     * Global Flag Methods
     * ------------------------------------
    */
    public GlobalFlags GetGlobalFlags()
        => pkuCollection.GetGlobalFlags();

    public void SetBattleStatOverrideFlag(bool val)
        => pkuCollection.SetBattleStatOverrideFlag(val);

    public void SetDefaultFormOverrideFlag(bool val)
        => pkuCollection.SetDefaultFormOverrideFlag(val);


    /* ------------------------------------
     * Box Methods
     * ------------------------------------
    */
    public void CheckOut(Slot slot)
        => CurrentBox.CheckOut(slot);

    public void CheckIn(Slot slot)
        => CurrentBox.CheckIn(slot);

    public void AddBox(string boxName)
    {
        if (pkuCollection.AddBox(boxName))
            SwitchBox(Collection.BoxCount - 1);
        else
            MessageBox.Show("That box name is invalid or alredy exists.", "Invalid box name");
    }

    public bool RemoveCurrentBox()
    {
        if (pkuCollection.BoxCount is 1)
            MessageBox.Show("Can't remove the last box. Create another box first.", "Last box");
        else
        {
            DialogResult dr = MessageBox.Show("Note that this will only remove the box from the Collection config, and will NOT delete the underlying folder (i.e. the pku files won't be deleted). Continue?", "Remove Current Box", MessageBoxButtons.YesNo);
            if (dr is DialogResult.Yes)
            {
                pkuCollection.RemoveCurrentBox(); //switches to 0th box
                RefreshBoxDisplay();
                return true;
            }
        }
        return false;
    }


    /* ------------------------------------
     * pku Box Methods
     * ------------------------------------
    */
    public void OpenBoxInFileExplorer()
        => Process.Start("explorer.exe", @$"{pkuCollection.path}\{pkuCollection.CurrentBox.Name}");

    public BoxConfigType GetBoxType()
        => CurrentBox.BoxType;

    public bool CanChangeBoxType(BoxConfigType type)
        => CurrentBox.CanChangeBoxType(type);

    public void ChangeBoxType(BoxConfigType type)
    {
        if (CurrentBox.ChangeBoxType(type))
            SwitchBox(pkuCollection.CurrentBoxID);
    }
}