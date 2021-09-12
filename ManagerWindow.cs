using pkuManager.GUI;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;
using static pkuManager.Common.Collection;
using static pkuManager.pku.pkuCollection.PKUBoxConfig;

namespace pkuManager
{
    public partial class ManagerWindow : Form
    {
        // Discord Rich Presence
        private DiscordPresence discord;

        // Refrences to certain UI Elements
        private FolderBrowserDialog collectionSelectorDialog;
        private SpriteBox SpriteBox;

        // Manager for the currently open PKUCollection
        private pkuCollectionManager pkuCollectionManager;

        // UI Constants
        private static readonly string EXPORT_BUTTON_INTRO = "Export to \n";
        private static readonly string CHECKOUT_BUTTON_INTRO = "Check-out to \n";
        private static readonly string IMPORT_BUTTON_INTRO = "Import a \n";
        private static readonly string CHECKIN_BUTTON_INTRO = "Check-in via \n";
        private static readonly int SPRITE_BOX_Y_OFFSET = 200;

        // Export Variables
        private bool checkMode = true; //false = check-out mode, true = export mode
        private SlotInfo selectedSlotInfo; //ref to most recently selected pkuslot for exporter logic
        private pkuObject selectedPKU;
        
        // Constructor initializes UI elements
        public ManagerWindow()
        {
            // Add Discord RPC
            discord = new DiscordPresence();
            FormClosed += discord.Deinitialize;

            //Initialize GUI Components
            InitializeComponent();
            InitializeSettingsUI();
            InitializeImportExportButtons();
            InitializeSpriteBox();
            collectionSelectorDialog = new FolderBrowserDialog();

            // Open last collection (if it still exists)
            if (pkuCollectionManager.IsValidPKUCollection(Properties.Settings.Default.Last_Path))
                OpenPKUCollection(Properties.Settings.Default.Last_Path);

            // Reset UI
            LHSTabs.SelectedIndex = 0; //Select Summary Tab
            UpdateSummaryTab(selectedSlotInfo); //Reset Summary Tab
            UpdateImportExportButtonVisibility(selectedSlotInfo, selectedPKU); //Reset Import/Export Button Visibility
            ResetFocus(); //Reset Focus
        }


        /* ------------------------------------
         * Switch PKUCollections Stuff
         * ------------------------------------
        */
        private void OpenPKUCollection(string path)
        {
            // Initialize collectionManager and related GUI components.
            pkuCollectionManager = new pkuCollectionManager(new pkuCollection(path));
            UpdateGlobalFlagUI();

            // save this path as last opened pkucollection
            Properties.Settings.Default.Last_Path = path;
            Properties.Settings.Default.Save();

            //Update discord collection name
            discord.collection = pkuCollectionManager.GetCollectionName();
            discord.setPresence();

            //Enable tool bar if collection opened successfully not already enabled
            collectionOptionsDropDown.Enabled = true;
            boxOptionsDropDown.Enabled = true;
            refreshBoxButton.Enabled = true;

            // Code for updating ManagerWindow UI whenever the boxDisplay changes.
            pkuCollectionManager.BoxDisplayRefreshed += OnBoxDisplayRefreshed;

            // Code for updating SummaryTab and ExporterButtonVisibility when a slotDisplay is selected.
            pkuCollectionManager.SlotSelected += (object s, EventArgs e) =>
            {
                (SlotInfo newSlotInfo, byte[] pkmn) = ((SlotInfo, byte[]))s;
                if (newSlotInfo != selectedSlotInfo) //if a different slot was just clicked
                {
                    selectedSlotInfo = newSlotInfo;
                    selectedPKU = pkmn == null ? null : pkuObject.Deserialize(pkmn).pku; //should not be null
                    UpdateSummaryTab(selectedSlotInfo);
                    UpdateImportExportButtonVisibility(selectedSlotInfo, selectedPKU);
                }
            };

            ResetBoxDisplayDock(pkuCollectionManager.BoxDisplay);
            ResetBoxSelector();
        }

        // Unlocks the current PKUCollection and opens a dialog to choose a new PKUCollection to open.
        private void openACollectionButton_Click(object sender, EventArgs e)
        {
            DialogResult result = collectionSelectorDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                if(pkuCollectionManager.IsValidPKUCollection(collectionSelectorDialog.SelectedPath))
                    OpenPKUCollection(collectionSelectorDialog.SelectedPath);
                else
                    MessageBox.Show("The selected folder does not have a collectionconfig.json file, and so is not a valid Collection.", "Invalid Collection");
            }
        }

        private void createANewCollectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = pkuCollectionManager.CreateACollection();
            if(path != null)
                OpenPKUCollection(path);
        }


        /* ------------------------------------
         * Summary Tab Stuff
         * ------------------------------------
        */
        // Initializes the SpriteBox
        private void InitializeSpriteBox()
        {
            SpriteBox = new SpriteBox(summaryTab.Width, SPRITE_BOX_Y_OFFSET);
            summaryTab.Controls.Add(SpriteBox);
        }

        // Behavior for clicking on a pku slot in the box display
        // Also updates the discord presence
        private void UpdateSummaryTab(SlotInfo slotInfo)
        {
            ResetFocus();

            // summary text box fill in
            if (slotInfo != null)
            {
                nicknameTextBox.Text = slotInfo.nickname;
                otLabel.Text = slotInfo.trueOT ? "True OT:" : "OT:";
                otTextBox.Text = slotInfo.OT;
                speciesTextBox.Text = slotInfo.species;
                formsTextBox.Text = string.Join(", ", slotInfo.forms ?? Array.Empty<string>());
                formsTextBox.Visible = formsLabel.Visible = formsTextBox.Text?.Length > 0;
                appearanceTextBox.Text = string.Join(", ", slotInfo.appearance ?? Array.Empty<string>());
                appearanceTextBox.Visible = appearanceLabel.Visible = appearanceTextBox.Text?.Length > 0;
                gameTextBox.Text = slotInfo.game;
                locationLabel.Text = slotInfo.locationIdentifier;
                locationTextBox.Text = slotInfo.location;
                checkedOutLabel.Visible = slotInfo.checkedOut;
                SpriteBox.UpdateSpriteBox(slotInfo);

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
            formsTextBox.Text = "";
            formsTextBox.Visible = formsLabel.Visible = false;
            appearanceTextBox.Text = "";
            appearanceTextBox.Visible = appearanceLabel.Visible = false;
            gameTextBox.Text = "";
            locationTextBox.Text = "";
            checkedOutLabel.Visible = false;
            SpriteBox.UpdateSpriteBox(null);

            //change window icon
            //this.Icon = Properties.Resources.pc

            //update discord RPC
            discord.nickname = null;
            discord.ball = "pc";
            discord.setPresence();
        }

        private void ResetFocus()
        {
            nicknameLabel.Focus(); //remove focus from any textbox
        }


        /* ------------------------------------
         * Import/Export button Logic
         * ------------------------------------
        */
        private static string GetUIFormatName(Registry.FormatInfo fi)
        {
            return $".{ (fi.ext == "txt" ? $"txt ({fi.name})" : fi.ext)}";
        }

        // Creates a button for each import/export format registered in the Registry class.
        private void InitializeImportExportButtons()
        {
            foreach (Registry.FormatInfo fi in Registry.FORMAT_LIST)
            {
                // Importer button
                if (fi.importer != null)
                {
                    Button ib = new Button
                    {
                        Text = $"{CHECKIN_BUTTON_INTRO}.{(fi.ext == "txt" ? $"txt ({fi.name})" : fi.ext)}",
                        AutoSize = true,
                        Enabled = false,
                        FlatStyle = FlatStyle.Flat,
                        Tag = fi
                    };
                    importButtons.Controls.Add(ib);

                    ib.Click += (object sender, EventArgs e) =>
                    {
                        if(!pkuCollectionManager.RoomForOneMore())
                        {
                            MessageBox.Show("There no room in this box to import a Pokemon. Either change the box type to be bigger, or pick a different box.");
                            return;
                        }

                        (pkuObject importedpku, ImportingWindow.ImportStatus status, string reason) 
                            = ImportingWindow.RunImportWindow(fi, pkuCollectionManager.GetGlobalFlags(), checkMode);

                        if(status == ImportingWindow.ImportStatus.Success) //sucessfull import
                        {
                            if (checkMode) //this was a check-in
                            {
                                //TODO merging pkus
                                pkuCollectionManager.CheckIn(selectedSlotInfo); //adds to check-in list, and updates boxDisplay
                                UpdateSummaryTab(selectedSlotInfo);
                                UpdateImportExportButtonVisibility(selectedSlotInfo, selectedPKU);
                            }
                            else //just a normal import
                                pkuCollectionManager.AddToCurrentBox(importedpku);
                        }
                        else if (status == ImportingWindow.ImportStatus.Invalid_File)
                            MessageBox.Show($"{(checkMode ? "Check-in" : "Import")} from {GetUIFormatName(fi)} Failed! The selected file is not a valid {fi.name} file. Reason: {reason}");
                        //else if (status == ImportingWindow.ImportStatus.Canceled)
                        //    MessageBox.Show($"{(checkMode ? "Check-in" : "Import")} from {GetUIFormatName(fi)} Canceled.");
                    };
                }

                // Exporter button
                if(fi.exporter != null)
                {
                    Button eb = new Button
                    {
                        Text = CHECKOUT_BUTTON_INTRO + GetUIFormatName(fi),
                        AutoSize = true,
                        Enabled = false,
                        FlatStyle = FlatStyle.Flat,
                        Tag = fi
                    };
                    exportButtons.Controls.Add(eb);

                    eb.Click += (object sender, EventArgs e) =>
                    {
                        ExportingWindow.ExportStatus status = ExportingWindow.RunWarningWindow(fi, selectedPKU, pkuCollectionManager.GetGlobalFlags());

                        if (checkMode && status == ExportingWindow.ExportStatus.Success) //if the pokemon was just checked-out
                        {
                            pkuCollectionManager.CheckOut(selectedSlotInfo); //adds to check-out list, and updates boxDisplay
                            UpdateSummaryTab(selectedSlotInfo);
                            UpdateImportExportButtonVisibility(selectedSlotInfo, selectedPKU);
                        }
                        //else if (status != ExportingWindow.ExportStatus.Success)
                        //    MessageBox.Show($"{(checkMode ? "Check-out" : "Export")} to {GetUIFormatName(fi)} Failed!");
                    };
                }
            }
        }

        // Updates which export buttons are visible given a PKUObject.
        private void UpdateImportExportButtonVisibility(SlotInfo slotInfo, pkuObject pku)
        {
            /* ------------------------------------
             * Update export button enabledness
             * ------------------------------------
            */
            if (pku == null || (checkMode && slotInfo?.checkedOut == true)) //if pku is empty, or checking-out an already checked out pokemon
            {
                foreach (Control c in exportButtons.Controls)
                    c.Enabled = false;
            }
            else //2 exporter instances created, one for canexport, one for process+tofile...
            {
                foreach (Control button in exportButtons.Controls)
                {
                    Registry.FormatInfo fi = (Registry.FormatInfo)button.Tag;
                    if (ExportingWindow.CanExport(fi, pku, pkuCollectionManager.GetGlobalFlags())) //exportable in this format
                    {
                        if (!checkMode) //Export mode
                            button.Enabled = true;
                        else if (slotInfo?.checkedOut == false)//Check-out mode, not checked out
                        {
                            button.Enabled  = !fi.excludeCheckOut;
                        }
                        else //Check-out mode, checked out
                            button.Enabled = false;
                    }
                    else //can't export to this format
                        button.Enabled = false;
                }
            }

            // Remove buttons that can't be checked out ever (i.e. Showdown)
            foreach (Control c in exportButtons.Controls)
            {
                Registry.FormatInfo fi = (Registry.FormatInfo)c.Tag;
                c.Visible = !checkMode || !fi.excludeCheckOut;
            }


            /* ------------------------------------
             * Update import button enabledness
             * ------------------------------------
            */
            foreach (Control c in importButtons.Controls)
                c.Enabled = !checkMode || slotInfo?.checkedOut == true;
        }

        // Switches the (im/ex)port/check-(in/out) toggle variable, and text of the import/export buttons
        private void ImportExportToggleButton_Click(object sender, EventArgs e)
        {
            checkMode = !checkMode;
            foreach (Control button in importButtons.Controls)
            {
                Registry.FormatInfo fi = (Registry.FormatInfo)button.Tag;

                if (checkMode)
                    button.Text = CHECKIN_BUTTON_INTRO + GetUIFormatName(fi);
                else
                    button.Text = IMPORT_BUTTON_INTRO + GetUIFormatName(fi);

            }

            foreach (Control button in exportButtons.Controls)
            {
                Registry.FormatInfo fi = (Registry.FormatInfo)button.Tag;

                if (checkMode)
                    button.Text = CHECKOUT_BUTTON_INTRO + GetUIFormatName(fi);
                else
                    button.Text = EXPORT_BUTTON_INTRO + GetUIFormatName(fi);

            }

            if (checkMode)
            {
                exportToggleButton.Text = "Toggle Export";
                importToggleButton.Text = "Toggle Import";
            }
            else
            {
                exportToggleButton.Text = "Toggle Check-out";
                importToggleButton.Text = "Toggle Check-in";
            }


            UpdateImportExportButtonVisibility(selectedSlotInfo, selectedPKU);
        }


        /* ------------------------------------
         * Box Options Stuff
         * ------------------------------------
        */
        // Box options
        private void boxOptionsType_Click(object sender, EventArgs e)
        {
            if (sender.Equals(boxOptionsList))
                pkuCollectionManager.ChangeCurrentBoxType(BoxConfigType.LIST);
            else if (sender.Equals(boxOptions30) && !((ToolStripMenuItem)sender).Checked)
                pkuCollectionManager.ChangeCurrentBoxType(BoxConfigType.THIRTY);
            else if (sender.Equals(boxOptions60) && !((ToolStripMenuItem)sender).Checked)
                pkuCollectionManager.ChangeCurrentBoxType(BoxConfigType.SIXTY);
            else if (sender.Equals(boxOptions96) && !((ToolStripMenuItem)sender).Checked)
                pkuCollectionManager.ChangeCurrentBoxType(BoxConfigType.NINTYSIX);
        }

        private void addNewBoxButton_Click(object sender, EventArgs e)
        {
            string boxname = null;
            bool invalid = false;
            string[] boxNames = pkuCollectionManager.GetBoxList();
            DialogResult dr;
            do
            {
                dr = DataUtil.InputBox("Add New Box", invalid ? "The box name can't be empty or exist in the collection already. Choose another." : "What's the name of the new box?", ref boxname);
                invalid = boxname == null || boxname == "" || boxNames.Contains(boxname, StringComparer.OrdinalIgnoreCase);
            }
            while (invalid && dr != DialogResult.Cancel);

            if (dr != DialogResult.Cancel)
                pkuCollectionManager.AddNewBox(boxname);

            ResetBoxSelector(pkuCollectionManager.GetBoxList().Length-1);
        }

        private void removeCurrentBoxButton_Click(object sender, EventArgs e)
        {
            pkuCollectionManager.RemoveCurrentBox();
            ResetBoxSelector();
        }

        private void openBoxInFileExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pkuCollectionManager.OpenCurrentBoxInFileExplorer();
        }

        private void ResetBoxDisplayDock(BoxDisplay boxDisplay)
        {
            boxDisplayDock.Controls.Clear();
            boxDisplayDock.Controls.Add(boxDisplay);
        }

        private void ResetBoxSelector(int currentBox = 0)
        {
            boxSelector.Items.Clear();
            boxSelector.Items.AddRange(pkuCollectionManager.GetBoxList());
            boxSelector.SelectedIndex = currentBox;
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


        /* ------------------------------------
         * Settings UI Stuff
         * ------------------------------------
        */
        private void InitializeSettingsUI()
        {
            sendToRecycleButton.Checked = Properties.Settings.Default.Send_to_Recycle;
            askBeforeAutoAddButton.Checked = Properties.Settings.Default.Ask_Auto_Add;
            hideDiscordPresenceButton.Checked = Properties.Settings.Default.Hide_Discord_Presence;
        }

        private void sendToRecycleToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Send_to_Recycle = sendToRecycleButton.Checked;
            Properties.Settings.Default.Save();
        }

        private void askBeforeAutoAddToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Ask_Auto_Add = askBeforeAutoAddButton.Checked;
            Properties.Settings.Default.Save();
        }

        private void hideDiscordPresenceToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Hide_Discord_Presence = hideDiscordPresenceButton.Checked;
            Properties.Settings.Default.Save();
            discord.setPresence();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().Show();
        }


        /* ------------------------------------
         * Box Display UI Stuff
         * ------------------------------------
        */
        private void OnBoxDisplayRefreshed(object sender, EventArgs e)
        {
            ResetBoxDisplayDock(pkuCollectionManager.BoxDisplay);

            //box options check
            boxOptionsList.Enabled = pkuCollectionManager.CanChangeCurrentBoxType(BoxConfigType.LIST);
            boxOptions30.Enabled = pkuCollectionManager.CanChangeCurrentBoxType(BoxConfigType.THIRTY);
            boxOptions60.Enabled = pkuCollectionManager.CanChangeCurrentBoxType(BoxConfigType.SIXTY);
            boxOptions96.Enabled = pkuCollectionManager.CanChangeCurrentBoxType(BoxConfigType.NINTYSIX);

            BoxConfigType bcft = pkuCollectionManager.GetCurrentBoxType();
            boxOptionsList.Checked = bcft == BoxConfigType.LIST;
            boxOptions30.Checked = bcft == BoxConfigType.THIRTY;
            boxOptions60.Checked = bcft == BoxConfigType.SIXTY;
            boxOptions96.Checked = bcft == BoxConfigType.NINTYSIX;

            //discord RPC
            discord.box = (string)boxSelector.SelectedItem;
            discord.setPresence();
        }

        // Behavior when a different box is selected.
        // Also updates the discord presence
        private void boxSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetFocus();
            pkuCollectionManager.SwitchCurrentBox(boxSelector.SelectedIndex);
            ClearSummaryTab();
            UpdateImportExportButtonVisibility(null, null);
            discord.box = (string)boxSelector.SelectedItem;
            discord.setPresence();
        }

        // Behavior for the toggle checkout viewer button
        //doesn't do anything anymore, need to redo this feature
        private void viewCheckedoutButton_Click(object sender, EventArgs e)
        {
            checkoutDock.Visible = !checkoutDock.Visible;
            viewCheckedoutButton.Text = checkoutDock.Visible ? "Hide Check-Out" : "View Check-Out";
        }

        private void refreshBox_Click(object sender, EventArgs e)
        {
            boxSelector_SelectedIndexChanged(null, null);
        }
    }
}