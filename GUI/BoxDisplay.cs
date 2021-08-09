using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static pkuManager.Common.Collection;

namespace pkuManager.GUI
{
    public class BoxDisplay : FlowLayoutPanel
    {
        private static readonly int SCROLLBAR_SIZE = 17;

        public event EventHandler SlotSelected;
        public event EventHandler SlotsSwapped;
        public class SlotsSwappedEventArgs : EventArgs
        {
            public int slotA, slotB;
            public SlotsSwappedEventArgs(int slotA, int slotB)
            {
                this.slotA = slotA;
                this.slotB = slotB;
            }
        }

        public SlotDisplay currentlySelectedSlotDisplay;

        public BoxDisplay(BoxInfo boxInfo)
        {
            // initial settings
            DoubleBuffered = true;
            BackgroundImageLayout = ImageLayout.Stretch;
            BackColor = Color.Transparent;
            BackgroundImage = boxInfo.background ?? Properties.Resources.grassbox;

            // box layout setup
            if (boxInfo.width == 0 || boxInfo.height == 0)
                ListSetup(boxInfo);
            else
                MatrixSetup(boxInfo);
        }

        private void ListSetup(BoxInfo boxInfo)
        {
            Width = SlotDisplay.SLOT_SIZE.Width * 6;
            Height = SlotDisplay.SLOT_SIZE.Height * 5;

            foreach (int slotID in boxInfo.slots.Keys)
                Controls.Add(new SlotDisplay(boxInfo.slots[slotID], slotID, OnClick));

            if (Controls.Count > 30)
                Width += SCROLLBAR_SIZE;

            AutoScroll = true;
        }

        private void MatrixSetup(BoxInfo boxInfo)
        {
            Width = SlotDisplay.SLOT_SIZE.Width * boxInfo.width;
            Height = SlotDisplay.SLOT_SIZE.Height * boxInfo.height;
            int max = boxInfo.width * boxInfo.height;
            for (int i = 1; i <= max; i++)
            {
                if (boxInfo.slots.ContainsKey(i))
                    Controls.Add(new SlotDisplay(boxInfo.slots[i], i, OnClick));
                else
                    Controls.Add(new SlotDisplay(i, OnClick));
            }
            if (Controls.Count > max)
                Width += SCROLLBAR_SIZE;
        }

        private void OnClick(object s, EventArgs e)
        {
            // left clicking a slot selects it
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                currentlySelectedSlotDisplay?.deselect();
                currentlySelectedSlotDisplay = ((SlotDisplay)s);
                currentlySelectedSlotDisplay.select();
                OnSlotSelected(currentlySelectedSlotDisplay, null);
            }
            //right clicking a slot while one is selected swaps it
            else if (((MouseEventArgs)e).Button == MouseButtons.Right)
            {
                if (currentlySelectedSlotDisplay != null)
                {
                    var alphaIndex = Controls.IndexOf(currentlySelectedSlotDisplay);
                    var betaIndex = Controls.IndexOf((SlotDisplay)s);
                    Controls.SetChildIndex(currentlySelectedSlotDisplay, betaIndex);
                    Controls.SetChildIndex((SlotDisplay)s, alphaIndex);
                    OnSlotsSwapped(null, new SlotsSwappedEventArgs(alphaIndex + 1, betaIndex + 1)); //slot numbers index at 1
                }
            }

        }

        protected virtual void OnSlotSelected(object s, EventArgs e)
        {
            EventHandler handler = SlotSelected;
            handler?.Invoke(s, e);
        }

        protected virtual void OnSlotsSwapped(object s, EventArgs e)
        {
            EventHandler handler = SlotsSwapped;
            handler?.Invoke(s, e);
        }

        public class SlotDisplay : Panel
        {
            /// <summary>
            /// Size of all box slots. Dimensions of the Gen 8+ box sprites, used by PokeSprite.
            /// </summary>
            public static readonly Size SLOT_SIZE = new Size(68, 56);

            public bool isEmpty;
            public bool isSelected;

            public SlotInfo slotInfo;

            public PictureBox picBox;

            public int slotID;

            public SlotDisplay(int slotID, EventHandler onClick)
            {
                DoubleBuffered = true;
                picBox = new PictureBox
                {
                    Size = SLOT_SIZE,
                    Margin = new Padding(0),
                    BorderStyle = BorderStyle.FixedSingle
                };
                Controls.Add(picBox);

                Margin = new Padding(0);
                isEmpty = true;
                this.slotID = slotID;

                AutoSize = true;

                picBox.Click += (s, e) =>
                {
                    InvokeOnClick(this, e);
                };
                Click += onClick;
            }

            public SlotDisplay(SlotInfo slotInfo, int slotID, EventHandler onClick) : this(slotID, onClick)
            {
                this.slotInfo = slotInfo;
                if (slotInfo.checkedOut)
                    picBox.BackgroundImage = Properties.Resources.checkedOut;
                picBox.ImageLocation = slotInfo.iconURL;
                if (picBox.ImageLocation == null)
                    picBox.Image = Properties.Resources.unknown_box;
                isEmpty = false;
            }

            public void select()
            {
                picBox.BackgroundImage = Properties.Resources.selection;
                picBox.Enabled = true;
            }

            public void deselect()
            {
                picBox.BackgroundImage = slotInfo?.checkedOut == true ? Properties.Resources.checkedOut : null;
                picBox.Enabled = false;

                //reset frame of box gif
                if (picBox.Image != null)
                {
                    picBox.Image.SelectActiveFrame(new FrameDimension(picBox.Image.FrameDimensionsList[0]), 0);
                    picBox.ImageLocation = picBox.ImageLocation;
                }
            }
        }
    }
}
