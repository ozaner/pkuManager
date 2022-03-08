using pkuManager.Formats;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.GUI;
using pkuManager.Utilities;
using System;
using System.IO;
using System.Windows.Forms;

namespace pkuManager;

public class CollectionManager
{
    protected static readonly SaveFileDialog SAVE_FILE_DIALOG = new();
    
    protected Collection Collection { get; }
    protected Box CurrentBox => Collection.CurrentBox;
    public BoxDisplay CurrentBoxDisplay { get; protected set; }
    public int CurrentBoxID => Collection.CurrentBoxID.GetAs<int>();
    public Slot CurrentlySelectedSlot { get; protected set; }

    public event EventHandler BoxDisplayRefreshed; //entirely new boxdisplay
    public event EventHandler SlotCountChanged; //same box display, just added/removed some slots
    public event EventHandler SlotSelected;

    public CollectionManager(Collection collection)
    {
        Collection = collection;
        if (!collection.IsCollectionValid)
            throw new ArgumentException("CollectionManager only accepts valid collections.", nameof(collection));
        RefreshBoxDisplay();
    }

    public string CollectionName => Collection.Name;

    public string[] GetBoxNames() => Collection.GetBoxNames();
    public int BoxCount => Collection.BoxCount;

    protected virtual void RefreshBoxDisplay()
    {
        CurrentBoxDisplay = new(CurrentBox);
        CurrentBoxDisplay.ExportRequest += CompleteExportRequest;
        CurrentBoxDisplay.ReleaseRequest += CompleteReleaseRequest;
        CurrentBoxDisplay.SwapRequest += CompleteSwapRequest;
        CurrentBoxDisplay.SlotSelected += (s, _) =>
        {
            CurrentlySelectedSlot = (s as SlotDisplay)?.Slot;
            SlotSelected?.Invoke(null, null);
        };
        
        BoxDisplayRefreshed?.Invoke(null, null);
    }

    protected virtual void CompleteExportRequest(object s, EventArgs e)
    {
        FormatObject fo = (s as SlotDisplay).Slot.pkmnObj;
        SAVE_FILE_DIALOG.SetExtension(Registry.FORMATS[fo.FormatName].Ext);
        DialogResult result = SAVE_FILE_DIALOG.ShowDialog(); // Show the dialog box.

        if (result is DialogResult.OK) //Successful choice of file name + location
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(SAVE_FILE_DIALOG.FileName), SAVE_FILE_DIALOG.FileName), fo.ToFile());
    }

    protected void CompleteReleaseRequest(object s, EventArgs e)
    {
        // Ask if they're sure
        SlotDisplay slotDisplay = s as SlotDisplay;
        string msg = $"Are you sure you want to " + (this is pkuCollectionManager && Properties.Settings.Default.Send_to_Recycle ?
            $"send {slotDisplay.Slot.Filename} to the recycle bin?" :
            $"permanently delete {slotDisplay.Slot.Filename}?");
        if (MessageBox.Show(msg, "Release Pokémon?", MessageBoxButtons.YesNo) is not DialogResult.Yes)
            return;

        // Try to release
        if (CurrentBox.ClearSlot(slotDisplay.SlotID))
        {
            CurrentBoxDisplay.CompleteReleaseRequest(slotDisplay);
            SlotCountChanged?.Invoke(null, null);
            MessageBox.Show($"{slotDisplay.Slot.Nickname} was released.\nBye-Bye, {slotDisplay.Slot.Nickname}!", "Goodbye");
        }
        else
            MessageBox.Show($"Failed to release {slotDisplay.Slot.Nickname}...", "Error");
    }

    protected void CompleteSwapRequest(object s, EventArgs e)
    {
        (SlotDisplay a, SlotDisplay b) = ((SlotDisplay, SlotDisplay))s;
        if (CurrentBox.SwapSlots(a.SlotID, b.SlotID))
            CurrentBoxDisplay.CompleteSwapRequest(a, b);
    }

    public bool TryInjectPKMN(FormatObject pkmn)
    {
        (int newSlotID, Slot newSlot) = CurrentBox.TryInjectPKMN(pkmn);
        if (newSlotID != -1) //success
        {
            CurrentBoxDisplay.AddSlotAt(newSlot, newSlotID);
            SlotCountChanged?.Invoke(null, null);
            return true;
        }
        return false;
    }

    public void SwitchBox(int boxID)
    {
        Collection.SwitchBox(boxID);
        RefreshBoxDisplay();
    }

    public void DeselectCurrentSlot()
        => CurrentBoxDisplay.DeselectCurrentSlot();

    public void Save()
    {
        if (Collection is FileCollection fc)
        {
            fc.Save();
            MessageBox.Show($"Succesfully saved to {Collection.Location}.", "Save Complete");
        }
        else
            MessageBox.Show("This collection doesn't support saving...");
    }
}