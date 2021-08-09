using pkuManager.Common;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static pkuManager.Common.Collection;
using static pkuManager.pku.PKUCollection.PKUBoxConfig;

namespace pkuManager
{
    public partial class ManagerWindow : Form
    {
        // Discord Rich Presence
        private DiscordPresence discord;

        // Refrences to certain UI Elements
        private SaveFileDialog saveFileDialog;
        private WarningWindow warningWindow;
        private FolderBrowserDialog collectionSelectorDialog;
        private PictureBox FrontSpritePictureBox, BackSpritePictureBox;

        // Manager for the currently open PKUCollection
        private PKUCollectionManager pkuCollectionManager;

        // UI String Constants
        private static readonly string EXPORT_BUTTON_INTRO = "Export to \n";
        private static readonly string CHECKOUT_BUTTON_INTRO = "Check-out to \n";

        // Export Variables
        private bool exportMode = false; //false = check-out mode, true = export mode
        private SlotInfo selectedSlotInfo; //ref to most recently selected pkuslot for exporter logic
        private PKUObject selectedPKU;

        // Constructor initializes UI elements
        public ManagerWindow()
        {
            // Add Discord RPC
            discord = new DiscordPresence();
            FormClosed += discord.Deinitialize;

            //Initialize GUI Components
            InitializeComponent();
            InitializeExportButtons();
            InitializeSpritePictureBox();
            collectionSelectorDialog = new FolderBrowserDialog();
            saveFileDialog = new SaveFileDialog();
            //saveFileDialog.RestoreDirectory = true;
            warningWindow = new WarningWindow(saveFileDialog);

            //Fast Track the PSS since default collection .txt not implemented
            //TODO implement last opened pss in a txt file next to exe...
            string path = @"C:\Users\PSS";
            OpenPKUCollection(path);
        }


        /* ------------------------------------
         * Switch PKUCollections Stuff
         * ------------------------------------
        */

        //TODO make this return a bool of whether it suceeded (i.e. valid boxconfig.json and all that. If failed, reject it/make a pss at that folder..?)
        private void OpenPKUCollection(string path)
        {
            // Initialize collectionManager and related GUI components.
            pkuCollectionManager = new PKUCollectionManager(new PKUCollection(path), this);
            ResetBoxDisplayDock(pkuCollectionManager.BoxDisplay);
            ResetBoxSelector(pkuCollectionManager.GetBoxList());
            UpdateGlobalFlagUI();

            //Update discord collection name
            discord.collection = pkuCollectionManager.GetCollectionName();
            discord.setPresence();

            //Enable tool bar if collection opened successfully not already enabled
            importAPKUToolStripMenuItem.Enabled = true;
            boxOptionsToolStripMenuItem.Enabled = true;

            // Code for updating ManagerWindow UI whenever the boxDisplay changes.
            pkuCollectionManager.BoxDisplayUpdated += (object s, EventArgs e) =>
            {
                ResetBoxDisplayDock(pkuCollectionManager.BoxDisplay);

                //box options check
                boxOptionsList.Enabled = pkuCollectionManager.CanChangeCurrentBoxType(BoxConfigType.LIST);
                boxOptions30.Enabled = pkuCollectionManager.CanChangeCurrentBoxType(BoxConfigType.THIRTY);
                boxOptions60.Enabled = pkuCollectionManager.CanChangeCurrentBoxType(BoxConfigType.SIXTY);
                boxOptions96.Enabled = pkuCollectionManager.CanChangeCurrentBoxType(BoxConfigType.NINTYSIX);

                //discord RPC
                discord.box = (string)boxSelector.SelectedItem;
                discord.setPresence();
            };

            // Code for updating SummaryTab and ExporterButtonVisibility when a slotDisplay is selected.
            pkuCollectionManager.SlotSelected += (object s, EventArgs e) =>
            {
                byte[] pkmn;
                SlotInfo newSlotInfo;
                (newSlotInfo, pkmn) = ((SlotInfo, byte[]))s;
                if (newSlotInfo != selectedSlotInfo) //if a different slot was just clicked
                {
                    selectedSlotInfo = newSlotInfo;
                    selectedPKU = pkmn == null ? null : pkuUtil.ImportPKU(pkmn);
                    UpdateSummaryTab(selectedSlotInfo);
                    UpdateExportButtonVisibility(selectedSlotInfo, selectedPKU);
                }
            };
        }

        // Unlocks the current PKUCollection and opens a dialog to choose a new PKUCollection to open.
        private void setCollectionDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = collectionSelectorDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                pkuCollectionManager?.Unlock(); //unlock previous PKUCollection
                OpenPKUCollection(collectionSelectorDialog.SelectedPath);
                discord.collection = Path.GetFileName(Path.GetFileName(collectionSelectorDialog.SelectedPath));
                discord.setPresence();
            }
        }


        /* ------------------------------------
         * Sprite Box Logic
         * ------------------------------------
        */

        private partial class SpriteBoxArgs
        {
            public bool isShadow;
            public string AuthorFront, AuthorBack;
        }

        private void OnSpriteboxLoaded(object s, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            PictureBox spriteBox = (PictureBox)s;
            if (e.Error != null) //if there was an error loading sprite
                spriteBox.Image = Properties.Resources.unknown;

            int xOffset = Summary.Width / 2;
            int yOffset = 175;
            spriteBox.Location = new Point(xOffset - spriteBox.Size.Width / 2, yOffset - spriteBox.Size.Height / 2);

            //deal with shadow haze
            SpriteBoxArgs spa = (SpriteBoxArgs)spriteBox.Tag;
            if (spa.isShadow)
                spriteBox.BackgroundImage = useLargeShadowBG(spriteBox.Image.Size) ? Properties.Resources.shadowbgx2 : Properties.Resources.shadowbg;
            else
                spriteBox.BackgroundImage = null;
        }

        private void OnSpriteboxClick(object s, EventArgs e)
        {
            // shift click opens author link if possible
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                SpriteBoxArgs spa = ((SpriteBoxArgs)((Control)s).Tag);
                string url = FrontSpritePictureBox.Visible ? spa.AuthorFront : spa.AuthorBack;
                if (DataUtil.isValidURL(url))
                    System.Diagnostics.Process.Start(url);
            }
            //TODO: ctrl click mega evolves sprite if keystone is held.
            else // normal left click switches front & back
            {
                FrontSpritePictureBox.Visible = s != FrontSpritePictureBox;
                BackSpritePictureBox.Visible = s != BackSpritePictureBox;
            }
        }

        private void OnSpriteboxHover(object s, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                SpriteBoxArgs spa = ((SpriteBoxArgs)((Control)s).Tag);
                string author = FrontSpritePictureBox.Visible ? spa.AuthorFront : spa.AuthorBack;
                toolTip1.SetToolTip((Control)s, author);
            }
            else
                toolTip1.RemoveAll();
        }

        private void InitializeSpritePictureBox()
        {
            //Initialize sprite boxes
            FrontSpritePictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackgroundImageLayout = ImageLayout.Center,
                MinimumSize = Properties.Resources.shadowbgx2.Size
            };
            BackSpritePictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackgroundImageLayout = ImageLayout.Center,
                MinimumSize = Properties.Resources.shadowbgx2.Size
            };
            FrontSpritePictureBox.LoadCompleted += OnSpriteboxLoaded;
            BackSpritePictureBox.LoadCompleted += OnSpriteboxLoaded;
            FrontSpritePictureBox.Click += OnSpriteboxClick;
            BackSpritePictureBox.Click += OnSpriteboxClick;
            FrontSpritePictureBox.MouseHover += OnSpriteboxHover;
            BackSpritePictureBox.MouseHover += OnSpriteboxHover;

            //Add them to the summary tab
            Summary.Controls.Add(FrontSpritePictureBox);
            Summary.Controls.Add(BackSpritePictureBox);
        }

        private void UpdateSpriteBox(SlotInfo slotInfo)
        {
            //reset to front sprite
            FrontSpritePictureBox.Visible = true;
            BackSpritePictureBox.Visible = false;

            //always reset picture box to prevent ghost images
            FrontSpritePictureBox.Image = BackSpritePictureBox.Image = null;
            FrontSpritePictureBox.Tag = BackSpritePictureBox.Tag = null;
            FrontSpritePictureBox.BackgroundImage = BackSpritePictureBox.BackgroundImage = null;
            toolTip1.RemoveAll();

            if (slotInfo != null) //add sprites to sprite boxes
            {
                FrontSpritePictureBox.Tag = BackSpritePictureBox.Tag = new SpriteBoxArgs
                {
                    isShadow = slotInfo.hasShadowHaze,
                    AuthorFront = slotInfo.frontSprite.author,
                    AuthorBack = slotInfo.backSprite.author
                };
                FrontSpritePictureBox.ImageLocation = slotInfo.frontSprite.url;
                BackSpritePictureBox.ImageLocation = slotInfo.backSprite.url;
            }
        }

        private bool useLargeShadowBG(Size size)
        {
            if (Properties.Resources.shadowbg.Width * 1.4 < size.Width || Properties.Resources.shadowbg.Height * 1.4 < size.Height)
                return true;
            else
                return false;
        }


        /* ------------------------------------
         * Summary Tab Stuff
         * ------------------------------------
        */

        // Behavior for clicking on a pku slot in the box display
        // Also updates the discord presence
        private void UpdateSummaryTab(SlotInfo slotInfo)
        {
            // summary text box fill in
            if (slotInfo != null)
            {
                nicknameTextBox.Text = slotInfo.nickname;
                otLabel.Text = slotInfo.trueOT ? "True OT:" : "OT:";
                otTextBox.Text = slotInfo.OT;
                speciesTextBox.Text = slotInfo.species;
                gameTextBox.Text = slotInfo.game;
                locationLabel.Text = slotInfo.locationIdentifier;
                locationTextBox.Text = slotInfo.location;
                checkedOutLabel.Visible = slotInfo.checkedOut;
                UpdateSpriteBox(slotInfo);

                //windows legacy design makes this unfeasible
                //Icon = Icon.FromHandle(((Bitmap)pkuSlot.BackgroundImage).GetHicon());

                // Discord RPC
                discord.nickname = nicknameTextBox.Text;
                discord.ball = slotInfo.ball;
                discord.setPresence();
            }
            else
                ClearSummaryTab();
        }

        // Clears the summary and deselects the currently selected pku slot.
        // Also clears the discord presence
        private void ClearSummaryTab()
        {
            // summary text box display
            //nicknameLabel.Text = "Nickname";
            nicknameTextBox.Text = "";
            otLabel.Text = "True OT:";
            otTextBox.Text = "";
            speciesTextBox.Text = "";
            gameTextBox.Text = "";
            locationTextBox.Text = "";
            checkedOutLabel.Visible = false;
            UpdateSpriteBox(null);

            //change window icon
            //this.Icon = Properties.Resources.pc

            //update discord RPC
            discord.nickname = null;
            discord.ball = "pc";
            discord.setPresence();
        }


        /* ------------------------------------
         * Export button Logic
         * ------------------------------------
        */

        // Creates a button for each export format registered in the Registry class.
        private void InitializeExportButtons()
        {
            foreach (string name in Registry.EXPORTER_DICT.Keys)
            {
                Button eb = new Button
                {
                    Text = CHECKOUT_BUTTON_INTRO + name,
                    AutoSize = true,
                    Enabled = false
                };
                exportButtons.Controls.Add(eb);
                toolTip1.SetToolTip(eb, "TEST");

                eb.Click += (object sender, EventArgs e) =>
                {
                    Exporter exporter = (Exporter)Activator.CreateInstance(Registry.EXPORTER_DICT[name], new object[] { selectedPKU, pkuCollectionManager.GetGlobalFlags() });
                    warningWindow.runWarningWindow(exporter);

                    //if this is a checkout
                    if (!exportMode && warningWindow.successfulExport) //if the pokemon was just checked-out
                        pkuCollectionManager.CheckOut(selectedSlotInfo); //adds to check out list, and updates boxDisplay.

                    //These are for displaying the checked-out text
                    UpdateSummaryTab(selectedSlotInfo);
                    LHSTabs.SelectedIndex = 0;
                    Summary.Focus();
                };
            }
        }

        // Updates which export buttons are visible given a PKUObject.
        private void UpdateExportButtonVisibility(SlotInfo slotInfo, PKUObject pku)
        {
            if (pku == null || (!exportMode && slotInfo?.checkedOut == true)) //if pku is empty, or checking-out an already checked out pokemon
            {
                foreach (Control c in exportButtons.Controls)
                    c.Enabled = false;
            }
            else //2 exporter instances created, one for canexport, one for process+tofile...
            {
                foreach (Control button in exportButtons.Controls)
                {
                    string name;
                    if (exportMode)
                        name = button.Text.Substring(EXPORT_BUTTON_INTRO.Length);
                    else
                        name = button.Text.Substring(CHECKOUT_BUTTON_INTRO.Length);
                    Exporter exporter = (Exporter)Activator.CreateInstance(Registry.EXPORTER_DICT[name], new object[] { pku, pkuCollectionManager.GetGlobalFlags() });
                    if (exporter.canExport()) //exportable in this format
                    {
                        if (exportMode) //Export mode
                            button.Enabled = true;
                        else if (slotInfo?.checkedOut == false)//Check-out mode, not checked out
                            button.Enabled = true;
                        else //Check-out mode, checked out
                            button.Enabled = false;
                    }
                    else //can't export to this format
                        button.Enabled = false;
                }
            }
        }

        // Switches the export/check-out toggle variable, and text of the export buttons
        private void exportToggleButton_Click(object sender, EventArgs e)
        {
            exportMode = !exportMode;
            foreach (Control button in exportButtons.Controls)
            {
                if (exportMode)
                    button.Text = EXPORT_BUTTON_INTRO + button.Text.Substring(CHECKOUT_BUTTON_INTRO.Length);
                else
                    button.Text = CHECKOUT_BUTTON_INTRO + button.Text.Substring(EXPORT_BUTTON_INTRO.Length);
            }

            if (exportMode)
                exportToggleButton.Text = "Toggle Check-out";
            else
                exportToggleButton.Text = "Toggle Export";

            UpdateExportButtonVisibility(selectedSlotInfo, selectedPKU);
        }


        /* ------------------------------------
         * Box Display Stuff
         * ------------------------------------
        */

        // Behavior when a different box is selected.
        // Also updates the discord presence
        private void boxSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            pkuCollectionManager.SwitchCurrentBox(boxSelector.SelectedIndex);
            ClearSummaryTab();
            UpdateExportButtonVisibility(null, null);
            discord.box = (string)boxSelector.SelectedItem;
            discord.setPresence();
        }

        // Box options
        private void boxOptionsType_Click(object sender, EventArgs e)
        {
            if (sender.Equals(boxOptionsList))
                pkuCollectionManager.ChangeCurrentBoxType(BoxConfigType.LIST);
            else if (sender.Equals(boxOptions30))
                pkuCollectionManager.ChangeCurrentBoxType(BoxConfigType.THIRTY);
            else if (sender.Equals(boxOptions60))
                pkuCollectionManager.ChangeCurrentBoxType(BoxConfigType.SIXTY);
            else if (sender.Equals(boxOptions96))
                pkuCollectionManager.ChangeCurrentBoxType(BoxConfigType.NINTYSIX);
        }


        /* ------------------------------------
         * Global Flag UI Stuff
         * ------------------------------------
        */

        // Updates UI elements dependent on the PKUCollection config.
        // Right now it's just the global flag "Stat Nature Override".
        private void UpdateGlobalFlagUI()
        {
            enableBattleStatOverrideButton.Checked = pkuCollectionManager.GetGlobalFlags().Battle_Stat_Override;
        }

        // Behavior for when the Stat Nature Override button is checked/unchecked
        private void enableBattleStatOverrideButton_CheckedChanged(object sender, EventArgs e)
        {
            pkuCollectionManager.SetBattleStatOverrideFlag(((ToolStripMenuItem)sender).Checked);
        }

        private void ResetBoxDisplayDock(GUI.BoxDisplay boxDisplay)
        {
            boxDisplayDock.Controls.Clear();
            boxDisplayDock.Controls.Add(boxDisplay);
        }

        private void ResetBoxSelector(string[] boxList)
        {
            boxSelector.Items.Clear();
            boxSelector.Items.AddRange(pkuCollectionManager.GetBoxList());
            boxSelector.SelectedIndex = 0;
        }


        /* ------------------------------------
         * OLD CODE
         * ------------------------------------
        */

        // Behavior for the toggle checkout viewer button
        //doesn't do anything anymore, need to redo this feature
        private void viewCheckedoutButton_Click(object sender, EventArgs e)
        {
            checkoutDock.Visible = !checkoutDock.Visible;
            viewCheckedoutButton.Text = checkoutDock.Visible ? "Hide Check-Out" : "View Check-Out";
        }

        //doesn't do anything... yet
        private void importPkuButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Importing .pku...");
            Console.WriteLine(".pku imported! (NOT REALLY!)");
        }
    }
}