using FluentDragDrop;
using pkuManager.Formats;
using pkuManager.Formats.pku;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace pkuManager.GUI;

public class BoxDisplay : FlowLayoutPanel
{
    private const int SCROLLBAR_SIZE = 17;

    private readonly bool isList;
    private SlotDisplay CurrentSlot;

    public event EventHandler SlotSelected;
    public event EventHandler ExportRequest;
    public event EventHandler CheckOutRequest; //only used by pkuCollectionManager
    public event EventHandler ReleaseRequest;
    public event EventHandler SwapRequest;


    public BoxDisplay(Box box)
    {
        // initial settings
        DoubleBuffered = true;
        BackgroundImageLayout = ImageLayout.Stretch;
        BackColor = Color.Transparent;
        BackgroundImage = box.Background ?? Properties.Resources.grassbox;
        isList = box.Width is 0 || box.Height is 0;

        // box layout setup
        if (isList)
            ListSetup(box.Data);
        else
            MatrixSetup(box.Data, box.Width, box.Height);
    }

    private void ListSetup(SortedDictionary<int, Slot> slots)
    {
        Width = SlotDisplay.SLOT_SIZE.Width * 6;
        Height = SlotDisplay.SLOT_SIZE.Height * 5;

        foreach (int slotID in slots.Keys)
            Controls.Add(new SlotDisplay(slots[slotID], slotID));

        if (Controls.Count > 30)
            Width += SCROLLBAR_SIZE;

        AutoScroll = true;
    }

    private void MatrixSetup(SortedDictionary<int, Slot> slots, int width, int height)
    {
        Width = SlotDisplay.SLOT_SIZE.Width * width;
        Height = SlotDisplay.SLOT_SIZE.Height * height;
        int max = width * height;
        for (int i = 1; i <= max; i++)
        {
            if (slots.ContainsKey(i))
                Controls.Add(new SlotDisplay(slots[i], i));
            else
                Controls.Add(new SlotDisplay(i));
        }
        if (Controls.Count > max)
            Width += SCROLLBAR_SIZE;
    }


    public void SelectSlot(SlotDisplay slotDisplay)
    {
        CurrentSlot?.Deselect();
        CurrentSlot = slotDisplay;
        CurrentSlot?.Select();
        SlotSelected.Invoke(CurrentSlot, null);
    }

    public void DeselectCurrentSlot()
        => SelectSlot(null);


    public void SendExportRequest(SlotDisplay slotDisplay)
        => ExportRequest?.Invoke(slotDisplay, null);

    public void SendCheckOutRequest(SlotDisplay slotDisplay)
        => CheckOutRequest?.Invoke(slotDisplay, null);

    public void CompleteCheckOutRequest(SlotDisplay slotDisplay)
        => slotDisplay.RemoveCheckOutButton();


    public void SendReleaseRequest(SlotDisplay slotDisplay)
        => ReleaseRequest?.Invoke(slotDisplay, null);

    public void CompleteReleaseRequest(SlotDisplay slotDisplay)
    {
        SlotDisplay empty = new(slotDisplay.SlotID);
        SelectSlot(empty);
        int index = Controls.GetChildIndex(slotDisplay);
        Controls.Remove(slotDisplay);
        if (!isList)
        {
            Controls.Add(empty);
            Controls.SetChildIndex(empty, index);
        }
    }


    public void SendSwapRequest(SlotDisplay slotDisplayA, SlotDisplay slotDisplayB)
        => SwapRequest?.Invoke((slotDisplayA, slotDisplayB), null);

    public void CompleteSwapRequest(SlotDisplay slotDisplayA, SlotDisplay slotDisplayB)
    {
        (slotDisplayA.SlotID, slotDisplayB.SlotID) = (slotDisplayB.SlotID, slotDisplayA.SlotID);
        int indexA = Controls.GetChildIndex(slotDisplayA);
        int indexB = Controls.GetChildIndex(slotDisplayB);
        Controls.SetChildIndex(slotDisplayA, indexB);
        Controls.SetChildIndex(slotDisplayB, indexA);

        SelectSlot(slotDisplayB); //select newly swapped box
    }
}

public class SlotDisplay : PictureBox
{
    /// <summary>
    /// Size of all box slots. Dimensions of the Gen 8+ box sprites, used by pkuSprite.
    /// </summary>
    public static readonly Size SLOT_SIZE = new(68, 56);

    public BoxDisplay BoxDisplay => Parent as BoxDisplay;
    public Slot Slot { get; }
    public int SlotID { get; set; }

    private bool isPKUSlot;
    private ToolStripMenuItem checkOutButton;

    //all/empty slot displays
    public SlotDisplay(int slotID)
    {
        SlotID = slotID;
        Click += (s, e) =>
        {
            // left clicking a slot selects it
            if ((e as MouseEventArgs).Button is MouseButtons.Left)
                BoxDisplay.SelectSlot(this);
        };

        MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                SlotDisplay sd = s as SlotDisplay;
                if (sd?.Slot is null)
                    return;
                sd.InitializeDragAndDrop()
                  .Move()
                  .OnMouseMove()
                  .WithData(() => sd)
                  .WithPreview()
                  .LikeWindowsExplorer()
                  .To(BoxDisplay.Controls.OfType<SlotDisplay>(),
                  (t, d) => BoxDisplay.SendSwapRequest(t, d));
            }
        };

        Size = SLOT_SIZE;
        Margin = new(0);
        DoubleBuffered = true;
        BorderStyle = BorderStyle.FixedSingle;
    }

    //filled slot displays
    public SlotDisplay(Slot slot, int slotID) : this(slotID)
    {
        Slot = slot;
        isPKUSlot = slot.pkmnObj is pkuObject;

        //init box sprite
        ImageLocation = slot.BoxSprite.url;
        if (ImageLocation is null)
            Image = Properties.Resources.unknown_box;

        //Create context menu
        ContextMenuStrip = new();
        ContextMenuStrip.Opening += (s, e) => BoxDisplay.SelectSlot(this);
        ContextMenuStrip.Items.Add("Export", null, (_, _) => BoxDisplay.SendExportRequest(this));
        ContextMenuStrip.Items.Add("Release", null, (_, _) => BoxDisplay.SendReleaseRequest(this));
        checkOutButton = new("Check-out", null, (_, _) => BoxDisplay.SendCheckOutRequest(this));

        //deal with checked out slots
        if (slot.CheckedOut)
            BackgroundImage = Properties.Resources.checkedOut;
        else if (isPKUSlot) //checked-in pkuSlot has button
            AddCheckOutButton();
    }

    public void AddCheckOutButton()
        => ContextMenuStrip.Items.Insert(0, checkOutButton);

    public void RemoveCheckOutButton()
        => ContextMenuStrip.Items.Remove(checkOutButton);

    public new void Select()
        => BackgroundImage = Properties.Resources.selection;

    public void Deselect()
    {
        BackgroundImage = Slot?.CheckedOut is true ? Properties.Resources.checkedOut : null;

        //reset frame of box gif
        if (Image is not null)
        {
            Image.SelectActiveFrame(new FrameDimension(Image.FrameDimensionsList[0]), 0);
            ImageLocation = ImageLocation; //this has a purpose... I think
        }
    }
}
