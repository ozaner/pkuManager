using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace pkuManager
{
    public class BoxDisplay : FlowLayoutPanel
    {
        private readonly int SCROLLBAR_SIZE = 20;

        private Size baseSize;

        public enum BoxDisplayConfig
        {
            LIST,
            THIRTY,
            SIXTY,
            NINTY
        }

        public BoxDisplay(FileInfo[] pkuFiles, BoxDisplayConfig config, EventHandler onClick, Size size, Image bg = null)
        {
            baseSize = size;
            Size = baseSize;
            AutoScroll = true;

            // Set box background
            BackgroundImageLayout = ImageLayout.Stretch;
            BackgroundImage = bg ?? Properties.Resources.grassbox;
            BackColor = Color.Transparent;

            // Max slot number
            int maxSlots = ((baseSize.Width - 20) / PKUSlot.SLOT_SIZE.Width) * (baseSize.Height / PKUSlot.SLOT_SIZE.Height);

            if (config == BoxDisplayConfig.LIST)
                initializeListType(pkuFiles, onClick, maxSlots);
        }

        private void initializeListType(FileInfo[] pkuFiles, EventHandler onClick, int maxSlots)
        {
            // Populate box display
            foreach (FileInfo file in pkuFiles)
                this.Controls.Add(new PKUSlot(file, onClick));

            //for (int i = 0; i < 34; i++)
            //    Controls.Add(new PKUSlot());

            // (de)Extend boxdisplay/window if a scroll bar is (un)needed
            if (this.Controls.Count > maxSlots)
                this.Width = baseSize.Width;
            else
                this.Width = baseSize.Width - SCROLLBAR_SIZE;
        }
    }
}
