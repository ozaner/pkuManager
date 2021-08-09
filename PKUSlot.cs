using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace pkuManager
{
    public class PKUSlot : PictureBox
    {
        //public PKUObject pku
        //{
        //    get { return Utilities.ImportPKU(pkuFile); }
        //}

        public static readonly Size SLOT_SIZE = new Size(68,56);

        public FileInfo pkuFile;
        public PKUObject pku; // Whether or not this pkuSlot has a pku in it, or is empty.
        public bool isSet;

        // Constructor for empty slots
        public PKUSlot()
        {
            isSet = false;
            Size = SLOT_SIZE;
            BorderStyle = BorderStyle.FixedSingle;
            Margin = new Padding(0);
        }

        // Constructor for full slots (should have some error handling for invalid pku's)
        public PKUSlot(FileInfo pkuFile, EventHandler onClick)
        {
            this.pkuFile = pkuFile;
            pku = Utilities.ImportPKU(pkuFile);

            isSet = true; // This is a holdover from having slots exist without pku.. still want to do it.

            //Initialize
            BackgroundImage = Utilities.getPKUIcon(pku);
            Width = 68;
            Height = 56;
            BorderStyle = BorderStyle.FixedSingle;
            Margin = new Padding(0);
            Click += onClick;
        }

        public void select()
        {
            this.Image = Properties.Resources.selection;
        }

        public void deselect()
        {
            this.Image = null;
        }
    }
}
