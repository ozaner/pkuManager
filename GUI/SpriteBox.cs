﻿using pkuManager.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using static pkuManager.Common.Collection;

namespace pkuManager.GUI;

public class SpriteBox : PictureBox
{
    private bool isBack;
    private SlotInfo currentSlotInfo;
    private int spriteBoxYOffset;
    private int containerWidth;

    private readonly ToolTip tooltip = new();

    public SpriteBox(int containerWidth, int spriteBoxYOffset) : base()
    {
        ErrorImage = Properties.Resources.unknown;
        this.containerWidth = containerWidth;
        this.spriteBoxYOffset = spriteBoxYOffset;

        Location = new(containerWidth / 2, spriteBoxYOffset);

        //Initialize sprite box
        Width = containerWidth;
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

        if(slotInfo is not null)
            ImageLocation = slotInfo.FrontSprite.url;
    }

    private void OnSpriteboxLoaded(object s, AsyncCompletedEventArgs e)
    {
        int xOffset = containerWidth / 2;
        int yOffset = spriteBoxYOffset;
        Location = new Point(xOffset - Size.Width / 2, yOffset - Size.Height / 2);

        if (currentSlotInfo?.HasShadowHaze is true)
            BackgroundImage = UseLargeShadowBG(Image.Size) ? Properties.Resources.shadowbgx2 : Properties.Resources.shadowbg;
    }

    private void OnSpriteboxClick(object s, EventArgs e)
    {
        // shift click opens author link if possible
        if (ModifierKeys.HasFlag(Keys.Shift))
        {
            string url = isBack ? currentSlotInfo.BackSprite.author : currentSlotInfo.FrontSprite.author;
            if (url.IsValidURL())
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
            string url = isBack ? currentSlotInfo?.BackSprite.url : currentSlotInfo?.FrontSprite.url;
            ImageLocation = url;
        }
    }

    private void OnSpriteboxHover(object s, EventArgs e)
    {
        if (ModifierKeys.HasFlag(Keys.Shift))
        {
            string author = isBack ? currentSlotInfo.BackSprite.author : currentSlotInfo.FrontSprite.author;
            tooltip.SetToolTip(this, author);
        }
        else
            tooltip.RemoveAll();
    }

    private static bool UseLargeShadowBG(Size size)
        => Properties.Resources.shadowbg.Width * 1.4 < size.Width ||
           Properties.Resources.shadowbg.Height * 1.4 < size.Height;
}