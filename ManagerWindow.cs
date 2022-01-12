using pkuManager.Formats;
using pkuManager.Formats.pku;
using pkuManager.GUI;
using pkuManager.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;
using static pkuManager.Formats.pku.pkuBox.pkuBoxConfig;

namespace pkuManager;

public partial class ManagerWindow : Form
{
    // Discord Rich Presence
    private readonly DiscordPresence discord;

    // References to certain UI Elements
    private readonly FolderBrowserDialog collectionSelectorDialog;
    private SpriteBox SpriteBox;

    // Manager for the currently open PKUCollection
    private pkuCollectionManager pkuCollectionManager;

    // UI Constants
    private const string EXPORT_BUTTON_INTRO = "Export to \n";
    private const string CHECKOUT_BUTTON_INTRO = "Check-out to \n";
    private const string IMPORT_BUTTON_INTRO = "Import a \n";
    private const string CHECKIN_BUTTON_INTRO = "Check-in via \n";
    private const int SPRITE_BOX_Y_OFFSET = 200;

    // Export Variables
    private bool checkMode = true; //false = check-out mode, true = export mode
    private Slot selectedSlot; //ref to most recently selected pkuslot for exporter logic
    private pkuObject selectedPKU => selectedSlot?.pkmnObj as pkuObject;
        
    // Constructor initializes UI elements
    public ManagerWindow()
    {
        // Add Discord RPC
        discord = new();
        FormClosed += discord.Deinitialize;

        //Initialize GUI Components
        InitializeComponent();
        InitializeSettingsUI();
        InitializePorterButtons();
        InitializeSpriteBox();
        collectionSelectorDialog = new();

        // Open last collection (if it still exists)
        if (pkuCollectionManager.IsValidPKUCollection(Properties.Settings.Default.Last_Path))
            OpenPKUCollection(Properties.Settings.Default.Last_Path);

        // Reset UI
        LHSTabs.SelectedIndex = 0; //Select Summary Tab
        UpdateSummaryTab(selectedSlot); //Reset Summary Tab
        UpdatePorterButtonVisibility(); //Reset Import/Export Button Visibility
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
        pkuCollectionManager.BoxDisplayRefreshed += OnBoxDisplayRefreshed;
        UpdateGlobalFlagUI();

        // Code for updating SummaryTab and ExporterButtonVisibility when a slotDisplay is selected.
        pkuCollectionManager.SlotSelected += (s, e) =>
        {
            if (pkuCollectionManager.CurrentlySelectedSlot != selectedSlot) //if a different slot was just clicked
            {
                selectedSlot = pkuCollectionManager.CurrentlySelectedSlot;
                UpdateSummaryTab(selectedSlot);
                UpdatePorterButtonVisibility();
            }
        };

        // save this path as last opened pkucollection
        Properties.Settings.Default.Last_Path = path;
        Properties.Settings.Default.Save();

        //Update discord collection name
        discord.Collection = pkuCollectionManager.CollectionName;
        discord.UpdatePresence();

        //Enable tool bar if collection opened successfully not already enabled
        collectionOptionsDropDown.Enabled = true;
        boxOptionsDropDown.Enabled = true;
        refreshBoxButton.Enabled = true;

        ResetBoxSelector();
    }

    // Unlocks the current PKUCollection and opens a dialog to choose a new PKUCollection to open.
    private void openACollectionButton_Click(object sender, EventArgs e)
    {
        DialogResult result = collectionSelectorDialog.ShowDialog(); // Show the dialog.
        if (result is DialogResult.OK) // Test result.
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
        if(path is not null)
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
    private void UpdateSummaryTab(Slot slot)
    {
        ResetFocus();

        // summary text box fill in
        if (slot is not null)
        {
            nicknameTextBox.Text = slot.Nickname;
            otLabel.Text = slot.IsTrueOT ? "True OT:" : "OT:";
            otTextBox.Text = slot.OT;
            speciesTextBox.Text = slot.Species;
            formsTextBox.Text = string.Join(", ", slot.Forms ?? Array.Empty<string>());
            formsTextBox.Visible = formsLabel.Visible = formsTextBox.Text?.Length > 0;
            appearanceTextBox.Text = string.Join(", ", slot.Appearance ?? Array.Empty<string>());
            appearanceTextBox.Visible = appearanceLabel.Visible = appearanceTextBox.Text?.Length > 0;
            gameTextBox.Text = slot.Game;
            locationLabel.Text = slot.LocationType;
            locationTextBox.Text = slot.Location;
            checkedOutLabel.Visible = slot.CheckedOut;
            SpriteBox.UpdateSpriteBox(slot);

            //windows legacy design makes this unfeasible
            //Icon = Icon.FromHandle(((Bitmap)pkuSlot.BackgroundImage).GetHicon());

            // Discord RPC
            discord.Nickname = nicknameTextBox.Text;
            discord.Ball = slot.Ball;
            discord.UpdatePresence();
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
        discord.Nickname = null;
        discord.Ball = "pc";
        discord.UpdatePresence();
    }

    private void ResetFocus()
        => nicknameLabel.Focus(); //removes focus from any textbox


    /* ------------------------------------
     * Import/Export button Logic
     * ------------------------------------
    */
    // Creates a button for each import/export format registered in the Registry class.
    private void InitializePorterButtons()
    {
        foreach (var kvp in Registry.FORMATS)
        {
            // Importer button
            if (kvp.Value.Importer is not null)
            {
                Button ib = new()
                {
                    Text = $"{CHECKIN_BUTTON_INTRO}.{(kvp.Value.Ext is "txt" ? $"txt ({kvp.Key})" : kvp.Value.Ext)}",
                    AutoSize = true,
                    Enabled = false,
                    FlatStyle = FlatStyle.Flat,
                    Tag = kvp.Value
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
                        = ImportingWindow.RunImportWindow(kvp.Key, kvp.Value, pkuCollectionManager.GetGlobalFlags(), checkMode);

                    if(status is ImportingWindow.ImportStatus.Success) //sucessfull import
                    {
                        if (checkMode) //this was a check-in
                        {
                            //TODO merging pkus
                            pkuCollectionManager.CheckIn(selectedSlot); //adds to check-in list, and updates boxDisplay
                            UpdateSummaryTab(selectedSlot);
                            UpdatePorterButtonVisibility();
                        }
                        else //just a normal import
                            pkuCollectionManager.InjectPokemon(importedpku);
                    }
                    else if (status is ImportingWindow.ImportStatus.Invalid_File)
                        MessageBox.Show($"{(checkMode ? "Check-in" : "Import")} from {kvp.Key} Failed! The selected file is not a valid {kvp.Key} file. Reason: {reason}");
                    //else if (status is ImportingWindow.ImportStatus.Canceled)
                    //    MessageBox.Show($"{(checkMode ? "Check-in" : "Import")} from {GetUIFormatName(fi)} Canceled.");
                };
            }

            // Exporter button
            if(kvp.Value.Exporter is not null)
            {
                Button eb = new()
                {
                    Text = CHECKOUT_BUTTON_INTRO + kvp.Key,
                    AutoSize = true,
                    Enabled = false,
                    FlatStyle = FlatStyle.Flat,
                    Tag = kvp.Value
                };
                exportButtons.Controls.Add(eb);

                eb.Click += (object sender, EventArgs e) =>
                {
                    ExportingWindow.ExportStatus status = ExportingWindow.RunWarningWindow(kvp.Key, kvp.Value, selectedPKU, pkuCollectionManager.GetGlobalFlags());

                    if (checkMode && status is ExportingWindow.ExportStatus.Success) //if the pokemon was just checked-out
                    {
                        pkuCollectionManager.CheckOut(selectedSlot); //adds to check-out list, and updates boxDisplay
                        UpdateSummaryTab(selectedSlot);
                        UpdatePorterButtonVisibility();
                    }
                    //else if (status != ExportingWindow.ExportStatus.Success)
                    //    MessageBox.Show($"{(checkMode ? "Check-out" : "Export")} to {GetUIFormatName(fi)} Failed!");
                };
            }
        }
    }

    // Updates which export buttons are visible given a PKUObject.
    private void UpdatePorterButtonVisibility()
    {
        /* ------------------------------------
         * Update export button enabledness
         * ------------------------------------
        */
        if (selectedPKU is null || (checkMode && selectedSlot?.CheckedOut is true)) //if pku is empty, or checking-out an already checked out pokemon
        {
            foreach (Control c in exportButtons.Controls)
                c.Enabled = false;
        }
        else //2 exporter instances created, one for canexport, one for process+tofile...
        {
            foreach (Control button in exportButtons.Controls)
            {
                Registry.FormatInfo fi = (Registry.FormatInfo)button.Tag;
                if (ExportingWindow.CanExport(fi, selectedPKU, pkuCollectionManager.GetGlobalFlags())) //exportable in this format
                {
                    if (!checkMode) //Export mode
                        button.Enabled = true;
                    else if (selectedSlot?.CheckedOut is false)//Check-out mode, not checked out
                        button.Enabled  = !fi.ExcludeCheckOut;
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
            c.Visible = !checkMode || !fi.ExcludeCheckOut;
        }


        /* ------------------------------------
         * Update import button enabledness
         * ------------------------------------
        */
        foreach (Control c in importButtons.Controls)
            c.Enabled = !checkMode || selectedSlot?.CheckedOut is true;
    }

    // Switches the (im/ex)port/check-(in/out) toggle variable, and text of the import/export buttons
    private void ImportExportToggleButton_Click(object sender, EventArgs e)
    {
        checkMode = !checkMode;
        foreach (Control button in importButtons.Controls)
        {
            Registry.FormatInfo fi = (Registry.FormatInfo)button.Tag;
            button.Text = (checkMode ? CHECKIN_BUTTON_INTRO : IMPORT_BUTTON_INTRO) + fi.Ext;
        }

        foreach (Control button in exportButtons.Controls)
        {
            Registry.FormatInfo fi = (Registry.FormatInfo)button.Tag;
            button.Text = (checkMode ? CHECKOUT_BUTTON_INTRO : EXPORT_BUTTON_INTRO) + fi.Ext;
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

        UpdatePorterButtonVisibility();
    }


    /* ------------------------------------
     * Box Options Stuff
     * ------------------------------------
    */
    // Box options
    private void boxOptionsType_Click(object sender, EventArgs e)
    {
        if (sender.Equals(boxOptionsList))
            pkuCollectionManager.ChangeBoxType(BoxConfigType.LIST);
        else if (sender.Equals(boxOptions30) && !(sender as ToolStripMenuItem).Checked)
            pkuCollectionManager.ChangeBoxType(BoxConfigType.THIRTY);
        else if (sender.Equals(boxOptions60) && !(sender as ToolStripMenuItem).Checked)
            pkuCollectionManager.ChangeBoxType(BoxConfigType.SIXTY);
        else if (sender.Equals(boxOptions96) && !(sender as ToolStripMenuItem).Checked)
            pkuCollectionManager.ChangeBoxType(BoxConfigType.NINTYSIX);
    }

    private void addNewBoxButton_Click(object sender, EventArgs e)
    {
        string boxname = null;
        bool invalid = false;
        string[] boxNames = pkuCollectionManager.GetBoxNames();
        DialogResult dr;
        do
        {
            dr = DataUtil.InputBox("Add New Box", invalid ? "The box name can't be empty or exist in the collection already. Choose another." : "What's the name of the new box?", ref boxname);
            invalid = boxname is null or "" || boxNames.Contains(boxname, StringComparer.OrdinalIgnoreCase);
        }
        while (invalid && dr is not DialogResult.Cancel);

        if (dr is not DialogResult.Cancel)
            pkuCollectionManager.AddBox(boxname);

        ResetBoxSelector(pkuCollectionManager.BoxCount-1);
    }

    private void removeCurrentBoxButton_Click(object sender, EventArgs e)
    {
        if (pkuCollectionManager.RemoveCurrentBox())
            ResetBoxSelector();
    }

    private void openBoxInFileExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        => pkuCollectionManager.OpenBoxInFileExplorer();

    private void ResetBoxSelector(int currentBox = 0)
    {
        boxSelector.Items.Clear();
        boxSelector.Items.AddRange(pkuCollectionManager.GetBoxNames());
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
        enableDefaultFormOverrideButton.Checked = pkuCollectionManager.GetGlobalFlags().Default_Form_Override;
    }

    // Behavior for when the Stat Nature Override button is checked/unchecked
    private void enableBattleStatOverrideButton_CheckedChanged(object sender, EventArgs e)
        => pkuCollectionManager.SetBattleStatOverrideFlag((sender as ToolStripMenuItem).Checked);

    // Behavior for when the Default Form Override button is checked/unchecked
    private void enableDefaultFormOverrideButton_Click(object sender, EventArgs e)
    {
        pkuCollectionManager.SetDefaultFormOverrideFlag((sender as ToolStripMenuItem).Checked);
        pkuCollectionManager.DeselectCurrentSlot();
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
        discord.UpdatePresence();
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        => new AboutBox().Show();


    /* ------------------------------------
     * Box Display UI Stuff
     * ------------------------------------
    */
    private void OnBoxDisplayRefreshed(object sender, EventArgs e)
    {
        boxDisplayDock.Controls.Clear(); //clear old displaybox
        boxDisplayDock.Controls.Add(pkuCollectionManager.CurrentBoxDisplay); //add new displaybox

        //box options check
        boxOptionsList.Enabled = pkuCollectionManager.CanChangeBoxType(BoxConfigType.LIST);
        boxOptions30.Enabled = pkuCollectionManager.CanChangeBoxType(BoxConfigType.THIRTY);
        boxOptions60.Enabled = pkuCollectionManager.CanChangeBoxType(BoxConfigType.SIXTY);
        boxOptions96.Enabled = pkuCollectionManager.CanChangeBoxType(BoxConfigType.NINTYSIX);

        BoxConfigType bcft = pkuCollectionManager.GetBoxType();
        boxOptionsList.Checked = bcft is BoxConfigType.LIST;
        boxOptions30.Checked = bcft is BoxConfigType.THIRTY;
        boxOptions60.Checked = bcft is BoxConfigType.SIXTY;
        boxOptions96.Checked = bcft is BoxConfigType.NINTYSIX;

        //discord RPC
        discord.Box = (string)boxSelector.SelectedItem;
        discord.UpdatePresence();
    }

    // Behavior when a different box is selected.
    // Also updates the discord presence
    private void boxSelector_SelectedIndexChanged(object sender, EventArgs e)
    {
        ResetFocus();
        pkuCollectionManager.SwitchBox(boxSelector.SelectedIndex);
        ClearSummaryTab();
        selectedSlot = null;
        UpdatePorterButtonVisibility();
        discord.Box = (string)boxSelector.SelectedItem;
        discord.UpdatePresence();
    }

    // Behavior for the toggle checkout viewer button
    //doesn't do anything anymore, need to redo this feature
    private void viewCheckedoutButton_Click(object sender, EventArgs e)
    {
        checkoutDock.Visible = !checkoutDock.Visible;
        viewCheckedoutButton.Text = checkoutDock.Visible ? "Hide Check-Out" : "View Check-Out";
    }

    private void refreshBox_Click(object sender, EventArgs e)
        => boxSelector_SelectedIndexChanged(null, null);
}