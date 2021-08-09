using pkuManager.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using static pkuManager.Common.Collection;

namespace pkuManager.GUI
{
    public class SpriteBox : PictureBox
    {
        private bool isBack;
        private SlotInfo currentSlotInfo;
        private int SpriteBoxYOffset;
        private int ContainerWidth;

        private ToolTip tooltip = new ToolTip();

        public SpriteBox(int ContainerWidth, int SpriteBoxYOffset) : base()
        {
            ErrorImage = Properties.Resources.unknown;
            this.ContainerWidth = ContainerWidth;
            this.SpriteBoxYOffset = SpriteBoxYOffset;

            Location = new Point(ContainerWidth / 2, SpriteBoxYOffset);

            //Initialize sprite box
            Width = ContainerWidth;
            SizeMode = PictureBoxSizeMode.CenterImage;
            BackgroundImageLayout = ImageLayout.Center;
            MinimumSize = Properties.Resources.shadowbgx2.Size; //maybe change that...?
            LoadCompleted += OnSpriteboxLoaded;
            Click += OnSpriteboxClick;
            MouseHover += OnSpriteboxHover;
        }

        public void UpdateSpriteBox(SlotInfo slotInfo)
        {
            currentSlotInfo = slotInfo;

            Image = null; //reset to prevent ghost images
            BackgroundImage = null;
            isBack = false; //reset back to front
            tooltip.RemoveAll(); //reset tooltip

            if(slotInfo != null)
                ImageLocation = slotInfo.frontSprite.url;
        }

        private void OnSpriteboxLoaded(object s, AsyncCompletedEventArgs e)
        {
            int xOffset = ContainerWidth / 2;
            int yOffset = SpriteBoxYOffset;
            Location = new Point(xOffset - Size.Width / 2, yOffset - Size.Height / 2);

            if (currentSlotInfo?.hasShadowHaze == true)
                BackgroundImage = UseLargeShadowBG(Image.Size) ? Properties.Resources.shadowbgx2 : Properties.Resources.shadowbg;
        }

        private void OnSpriteboxClick(object s, EventArgs e)
        {
            // shift click opens author link if possible
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                string url = isBack ? currentSlotInfo.backSprite.author : currentSlotInfo.frontSprite.author;
                if (DataUtil.isValidURL(url))
                {
                    try
                    {
                        var ps = new ProcessStartInfo(url)
                        {
                            UseShellExecute = true,
                            Verb = "open"
                        };
                        Process.Start(ps);
                    }
                    catch
                    {
                        Debug.WriteLine("Couldn't open link for some reason...");
                    }
                }
            }
            //TODO: ctrl click mega evolves sprite if keystone is held.
            else // normal left click switches front & back
            {
                isBack = !isBack; //switch back and front
                string url = isBack ? currentSlotInfo?.backSprite.url : currentSlotInfo?.frontSprite.url;
                ImageLocation = url;
            }
        }

        private void OnSpriteboxHover(object s, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                string author = isBack ? currentSlotInfo.backSprite.author : currentSlotInfo.frontSprite.author;
                tooltip.SetToolTip(this, author);
            }
            else
                tooltip.RemoveAll();
        }

        private bool UseLargeShadowBG(Size size)
        {
            if (Properties.Resources.shadowbg.Width * 1.4 < size.Width || Properties.Resources.shadowbg.Height * 1.4 < size.Height)
                return true;
            else
                return false;
        }
    }
}
