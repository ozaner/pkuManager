using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static pkuManager.Common.Collection;

namespace pkuManager.GUI;

public class BoxDisplay : FlowLayoutPanel
{
    private const int SCROLLBAR_SIZE = 17;

    public event EventHandler NotifyColectionManager;

    public class NotifyCollectionEventArgs : EventArgs
    {
        public string Command;
        public int IntA, IntB;
        public NotifyCollectionEventArgs(string command, int intA = -1, int intB = -1)
        {
            Command = command;
            IntA = intA;
            IntB = intB;
        }
    }

    public SlotDisplay currentlySelectedSlotDisplay;

    public BoxDisplay(BoxInfo boxInfo)
    {
        // initial settings
        DoubleBuffered = true;
        BackgroundImageLayout = ImageLayout.Stretch;
        BackColor = Color.Transparent;
        BackgroundImage = boxInfo.Background ?? Properties.Resources.grassbox;

        // box layout setup
        if (boxInfo.Width is 0 || boxInfo.Height is 0)
            ListSetup(boxInfo);
        else
            MatrixSetup(boxInfo);
    }

    private void ListSetup(BoxInfo boxInfo)
    {
        Width = SlotDisplay.SLOT_SIZE.Width * 6;
        Height = SlotDisplay.SLOT_SIZE.Height * 5;

        foreach (int slotID in boxInfo.Slots.Keys)
            Controls.Add(new SlotDisplay(boxInfo.Slots[slotID], slotID, OnClick, this));

        if (Controls.Count > 30)
            Width += SCROLLBAR_SIZE;

        AutoScroll = true;
    }

    private void MatrixSetup(BoxInfo boxInfo)
    {
        Width = SlotDisplay.SLOT_SIZE.Width * boxInfo.Width;
        Height = SlotDisplay.SLOT_SIZE.Height * boxInfo.Height;
        int max = boxInfo.Width * boxInfo.Height;
        for (int i = 1; i <= max; i++)
        {
            if (boxInfo.Slots.ContainsKey(i))
                Controls.Add(new SlotDisplay(boxInfo.Slots[i], i, OnClick, this));
            else
                Controls.Add(new SlotDisplay(i, OnClick));
        }
        if (Controls.Count > max)
            Width += SCROLLBAR_SIZE;
    }

    private void OnClick(object s, EventArgs e)
    {
        // left clicking a slot selects it
        if ((e as MouseEventArgs).Button is MouseButtons.Left)
        {
            currentlySelectedSlotDisplay?.Deselect();
            currentlySelectedSlotDisplay = (SlotDisplay)s;
            currentlySelectedSlotDisplay.Select();
            OnNotifyCollection(currentlySelectedSlotDisplay, new NotifyCollectionEventArgs("select"));
        }

        //middle clicking swaps slots
        if ((e as MouseEventArgs).Button is MouseButtons.Middle)
        {
            if (currentlySelectedSlotDisplay is not null)
            {
                SlotDisplay sd = (SlotDisplay)s;
                var alphaIndex = Controls.IndexOf(currentlySelectedSlotDisplay);
                var betaIndex = Controls.IndexOf(sd);
                Controls.SetChildIndex(currentlySelectedSlotDisplay, betaIndex);
                Controls.SetChildIndex(sd, alphaIndex);
                OnNotifyCollection(null, new NotifyCollectionEventArgs("swap", alphaIndex + 1, betaIndex + 1)); //slot numbers index at 1
            }
        }
    }

    private void OnDragDrop(object s, EventArgs e)
    {
        DragEventArgs dea = (DragEventArgs)e;
        Debug.WriteLine(((SlotDisplay)dea.Data.GetData(typeof(SlotDisplay))).slotID);
    }

    protected virtual void OnNotifyCollection(object s, EventArgs e)
    {
        EventHandler handler = NotifyColectionManager;
        handler?.Invoke(s, e);
    }

    public class SlotDisplay : Panel
    {
        /// <summary>
        /// Size of all box slots. Dimensions of the Gen 8+ box sprites, used by PokeSprite.
        /// </summary>
        public static readonly Size SLOT_SIZE = new(68, 56);

        public bool isEmpty;
        public bool isSelected;

        public BoxDisplay bd;

        public SlotInfo slotInfo;

        public PictureBox picBox;

        public int slotID;

        public SlotDisplay(int slotID, EventHandler onClick)
        {
            DoubleBuffered = true;
            picBox = new PictureBox
            {
                Size = SLOT_SIZE,
                Margin = new(0),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(picBox);

            Margin = new(0);
            isEmpty = true;
            this.slotID = slotID;

            AutoSize = true;

            picBox.Click += (_, e) => InvokeOnClick(this, e);
            Click += onClick;
        }

        public SlotDisplay(SlotInfo slotInfo, int slotID, EventHandler onClick, BoxDisplay bd) : this(slotID, onClick)
        {
            this.bd = bd;
            this.slotInfo = slotInfo;
            if (slotInfo.CheckedOut)
                picBox.BackgroundImage = Properties.Resources.checkedOut;
            picBox.ImageLocation = slotInfo.IconURL;
            if (picBox.ImageLocation is null)
                picBox.Image = Properties.Resources.unknown_box;
            isEmpty = false;

            //Add context menu to non empty slots
            ContextMenuStrip = new ContextMenuStrip();
            ContextMenuStrip.Items.Add("Release", null, (s, e) => {
                bd.OnNotifyCollection(this, new NotifyCollectionEventArgs("delete"));
                bd.OnNotifyCollection(null, new NotifyCollectionEventArgs("select"));
            });
        }

        public new void Select()
        {
            picBox.BackgroundImage = Properties.Resources.selection;
            picBox.Enabled = true;
        }

        public void Deselect()
        {
            picBox.BackgroundImage = slotInfo?.CheckedOut is true ? Properties.Resources.checkedOut : null;
            picBox.Enabled = false;

            //reset frame of box gif
            if (picBox.Image is not null)
            {
                picBox.Image.SelectActiveFrame(new FrameDimension(picBox.Image.FrameDimensionsList[0]), 0);
                picBox.ImageLocation = picBox.ImageLocation;
            }
        }
    }
}
