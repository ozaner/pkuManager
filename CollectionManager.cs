using pkuManager.Formats;
using pkuManager.Formats.pku;
using pkuManager.GUI;
using System;
using System.Windows.Forms;

namespace pkuManager;

public class CollectionManager
{
    protected Collection Collection { get; }
    public BoxDisplay CurrentBoxDisplay { get; protected set; }
    public Slot CurrentlySelectedSlot { get; protected set; }

    public event EventHandler BoxDisplayRefreshed;
    public event EventHandler SlotSelected;

    public CollectionManager(Collection collection)
        => Collection = collection;

    public string CollectionName => Collection.Name;

    public string[] GetBoxNames() => Collection.GetBoxNames();
    public int BoxCount => Collection.BoxCount;

    protected void RefreshBoxDisplay()
    {
        CurrentBoxDisplay = new(Collection.CurrentBox);
        CurrentBoxDisplay.ReleaseRequest += CompleteReleaseRequest;
        CurrentBoxDisplay.SlotSelected += (s, _) =>
        {
            CurrentlySelectedSlot = (s as SlotDisplay)?.Slot;
            SlotSelected?.Invoke(null, null);
        };
        
        BoxDisplayRefreshed?.Invoke(null, null);
    }

    protected void CompleteReleaseRequest(object s, EventArgs e)
    {
        // Ask if they're sure
        SlotDisplay slotDisplay = s as SlotDisplay;
        string msg = $"Are you sure you want to " + (this is pkuCollectionManager && Properties.Settings.Default.Send_to_Recycle ?
            $"send {slotDisplay.Slot.Location} to the recycle bin?" :
            $"permanently delete {slotDisplay.Slot.Location}?");
        if (MessageBox.Show(msg, "Release Pokémon?", MessageBoxButtons.YesNo) is not DialogResult.Yes)
            return;

        // Try to release
        if (Collection.CurrentBox.ReleaseSlot(slotDisplay.SlotID))
        {
            CurrentBoxDisplay.CompleteReleaseRequest(slotDisplay);
            BoxDisplayRefreshed?.Invoke(null, null);
            MessageBox.Show($"{slotDisplay.Slot.Nickname} was released.\nBye-Bye, {slotDisplay.Slot.Nickname}!", "Goodbye");
        }
        else
            MessageBox.Show($"Failed to release {slotDisplay.Slot.Nickname}...", "Error");
    }

    public void SwitchBox(int boxID)
    {
        Collection.SwitchBox(boxID);
        RefreshBoxDisplay();
    }

    public bool RoomForOneMore()
        => Collection.CurrentBox.RoomForOneMore();

    public bool InjectPokemon(FormatObject fo)
        => Collection.CurrentBox.InjectPokemon(fo);

    public void DeselectCurrentSlot()
        => CurrentBoxDisplay.DeselectCurrentSlot();
}