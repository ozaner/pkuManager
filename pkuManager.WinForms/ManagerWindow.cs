using pkuManager.WinForms.Formats;
using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.GUI;
using pkuManager.WinForms.Utilities;
using System;
using System.IO;
using System.Windows.Forms;
using static pkuManager.WinForms.Formats.pku.pkuBox.pkuBoxConfig;

namespace pkuManager.WinForms;

public partial class ManagerWindow : Form
{
    // Discord Rich Presence
    private readonly DiscordPresence discord;

    // References to certain UI Elements
    private readonly FolderBrowserDialog folderSelectorDialog;
    private readonly OpenFileDialog fileSelectorDialog;
    private SpriteBox SpriteBox;

    // Managers for the currently open collections
    private string currentpkuCollectionPath;
    private pkuCollectionManager pkuCollectionManager;
    private CollectionManager collectionManager;

    // UI Constants
    private const int SPRITE_BOX_Y_OFFSET = 200;

    // Export Variables
    private Slot selectedSlot; //ref to most recently selected pkuslot for exporter logic

    //Event trigger prevention
    private bool disableEventTrigger = false;

    // Constructor initializes UI elements
    public ManagerWindow()
    {
        // Add Discord RPC
        discord = new();
        FormClosed += discord.Deinitialize;

        //Initialize GUI Components
        InitializeComponent();
        InitializeSettingsUI();
        InitializeSpriteBox();
        folderSelectorDialog = new();
        fileSelectorDialog = new();

        // Open last collection (if it still exists)
        if (pkuCollection.CollectionConfigExistsIn(Properties.Settings.Default.Last_Path))
            OpenPKUCollection(Properties.Settings.Default.Last_Path);

        // Reset UI
        LHSTabs.SelectedIndex = 0; //Select Summary Tab
        UpdateSummaryTab(selectedSlot); //Reset Summary Tab
        ResetFocus(); //Reset Focus
    }


    /* ------------------------------------
     * pkuCollection Stuff
     * ------------------------------------
    */
    private void OpenPKUCollection(string path)
    {
        discord.ClearState();

        // Initialize collectionManager and related GUI components.
        pkuCollectionManager = new pkuCollectionManager(new pkuCollection(path));
        currentpkuCollectionPath = path;

        ResetPKUBoxSelector(pkuCollectionManager.CurrentBoxID); //update box selector
        UpdateGlobalFlagUI(); //update global flag ui

        OnPKUBoxDisplayRefreshed(null, null); //initialize boxdisplaydock
        pkuCollectionManager.BoxDisplayRefreshed += OnPKUBoxDisplayRefreshed;
        pkuCollectionManager.SlotCountChanged += OnPKUSlotCountChanged;

        // Code for updating SummaryTab and ExporterButtonVisibility when a slotDisplay is selected.
        pkuCollectionManager.SlotSelected += (s, e) =>
        {
            collectionManager?.DeselectCurrentSlot();
            if (pkuCollectionManager.CurrentlySelectedSlot != selectedSlot) //if a different slot was just clicked
            {
                selectedSlot = pkuCollectionManager.CurrentlySelectedSlot;
                UpdateSummaryTab(selectedSlot);
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
    }

    // Opens a dialog to choose a new PKUCollection to open.
    private void openCollectionButton_Click(object sender, EventArgs e)
    {
        DialogResult result = folderSelectorDialog.ShowDialog(); // Show the dialog.
        if (result is DialogResult.OK) // Test result.
        {
            if (pkuCollection.CollectionConfigExistsIn(folderSelectorDialog.SelectedPath))
                OpenPKUCollection(folderSelectorDialog.SelectedPath);
            else
                MessageBox.Show("The selected folder does not have a collectionconfig.json file, and so is not a valid Collection.", "Invalid Collection");
        }
    }

    private void createNewCollectionButton_Click(object sender, EventArgs e)
    {
        string path = pkuCollectionManager.CreateACollection();
        if (path is not null)
            OpenPKUCollection(path);
    }

    private void OnPKUBoxDisplayRefreshed(object sender, EventArgs e)
    {
        DeselectSlot(); //clear summary

        pkuBoxDisplayDock.Controls.Clear(); //clear old displaybox
        pkuBoxDisplayDock.Controls.Add(pkuCollectionManager.CurrentBoxDisplay); //add new displaybox

        OnPKUSlotCountChanged(null, null); //new boxdisplay, new slot count.

        //discord RPC
        discord.Box = (string)pkuBoxSelector.SelectedItem;
        discord.UpdatePresence();
    }

    public void OnPKUSlotCountChanged(object sender, EventArgs e)
    {
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

        //import button check
        importButton.Enabled = pkuCollectionManager.HasSpaceForImport();
    }

    private void ResetPKUBoxSelector(int currentBox = 0)
    {
        pkuBoxSelector.Items.Clear();
        pkuBoxSelector.Items.AddRange(pkuCollectionManager.GetBoxNames());
        disableEventTrigger = true;
        pkuBoxSelector.SelectedIndex = currentBox;
        disableEventTrigger = false;
    }

    private void SwitchPKUBoxToSelectedIndex(object sender, EventArgs e)
        => pkuCollectionManager.SwitchBox(pkuBoxSelector.SelectedIndex);

    private void pkuBoxSelector_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (disableEventTrigger) //allows for changing boxSlot without triggering it
            return;
        SwitchPKUBoxToSelectedIndex(sender, e);
    }

    private void importButton_Click(object sender, EventArgs e)
    {
        string format = FormatChooser.ChooseImportFormat(false);
        if (format is null)
            return;
        (pkuObject pku, PortingWindow.ImportStatus status, string error)
            = PortingWindow.RunImportWindow(format, Registry.FORMATS[format], pkuCollectionManager.GetGlobalFlags(), false);
        
        if (status is PortingWindow.ImportStatus.Success)
            pkuCollectionManager.TryInjectPKMN(pku);
        else if (status is PortingWindow.ImportStatus.Invalid_File)
            MessageBox.Show(error, "Failed to Import File");
    }


    /* ------------------------------------
     * Save File Stuff
     * ------------------------------------
    */
    private void openSaveFileButton_Click(object sender, EventArgs e)
    {
        //close previous save if its still open
        if (collectionManager is not null)
        {
            DialogResult resultA = MessageBox.Show("A save file is already open, would you like to close it without saving?",
                "Save file already open", MessageBoxButtons.YesNo);
            if (resultA == DialogResult.Yes)
                closeCollectionButton_Click(null, null);
            else
                return;
        }

        string format = FormatChooser.ChooseCollectionFormat();
        if (format is null) return;
        Registry.FormatInfo fi = Registry.FORMATS[format];
        if (!fi.Collection.IsSubclassOf(typeof(FileCollection)))
        {
            MessageBox.Show("Only FileCollections are currently supported. If you are reading this, this is an error.");
            return;
        }

        FileCollection fileColl;
        fileSelectorDialog.SetExtension(fi.SaveExt);
        DialogResult result = fileSelectorDialog.ShowDialog(); // Show the dialog.
        if (result is DialogResult.OK) // Test result.
        {
            fileColl = (FileCollection)Activator.CreateInstance(fi.Collection, fileSelectorDialog.FileName);
            if (fileColl.IsCollectionValid)
                OpenCollection(fileColl);
            else
                MessageBox.Show($"The selected file is not a valid {format} save file.", "Invalid Save File");
        }
    }

    private void OpenCollection(Collection collection)
    {
        collectionControlPanel.Visible = true;
        collectionManager = new(collection);

        //Init boxDisplayDock
        boxDisplayDock.Controls.Add(collectionManager.CurrentBoxDisplay);
        collectionManager.BoxDisplayRefreshed += (s, e) =>
        {
            boxDisplayDock.Controls.Clear();
            boxDisplayDock.Controls.Add(collectionManager.CurrentBoxDisplay);
        };

        //Init SlotSelected behavior
        collectionManager.SlotSelected += (s, e) =>
        {
            pkuCollectionManager?.DeselectCurrentSlot();
            if (collectionManager.CurrentlySelectedSlot != selectedSlot) //if a different slot was just clicked
            {
                selectedSlot = collectionManager.CurrentlySelectedSlot;
                UpdateSummaryTab(selectedSlot);
            }
        };

        //Init boxSelector
        boxSelector.Items.Clear();
        boxSelector.Items.AddRange(collectionManager.GetBoxNames());
        disableEventTrigger = true;
        boxSelector.SelectedIndex = collectionManager.CurrentBoxID;
        disableEventTrigger = false;
    }

    private void saveCollectionButton_Click(object sender, EventArgs e)
        => collectionManager.Save();

    private void closeCollectionButton_Click(object sender, EventArgs e)
    {
        DeselectSlot();
        collectionControlPanel.Visible = false;
        collectionManager = null;
        boxDisplayDock.Controls.Clear();
    }

    private void boxSelector_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (disableEventTrigger)
            return;
        DeselectSlot();
        collectionManager.SwitchBox(boxSelector.SelectedIndex);
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
            otTextBox.Text = slot.OT;
            speciesTextBox.Text = slot.Species;
            formsTextBox.Text = slot.Forms.SplitLexical().ToFormattedString();
            formsTextBox.Visible = formsLabel.Visible = formsTextBox.Text?.Length > 0;
            appearanceTextBox.Text = slot.Appearance.SplitLexical().ToFormattedString();
            appearanceTextBox.Visible = appearanceLabel.Visible = appearanceTextBox.Text?.Length > 0;
            gameTextBox.Text = slot.Game;
            SpriteBox.UpdateSpriteBox(slot);

            //pku specific slot
            if (slot?.pkmnObj is pkuObject)
            {
                locationLabel.Visible = true;
                locationTextBox.Visible = true;
                locationTextBox.Text = slot.Filename;
                checkedOutLabel.Visible = slot.CheckedOut;
            }
            else
            {
                locationLabel.Visible = false;
                locationTextBox.Visible = false;
                checkedOutLabel.Visible = false;
            }

            //windows legacy design makes this unfeasible
            //Icon = Icon.FromHandle(((Bitmap)pkuSlot.BackgroundImage).GetHicon());

            // Discord RPC
            discord.SpriteURL = slot.FrontSprite.url;
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
        otTextBox.Text = "";
        speciesTextBox.Text = "";
        formsTextBox.Text = "";
        formsTextBox.Visible = formsLabel.Visible = false;
        appearanceTextBox.Text = "";
        appearanceTextBox.Visible = appearanceLabel.Visible = false;
        gameTextBox.Text = "";
        locationTextBox.Text = "";
        locationTextBox.Visible = false;
        locationLabel.Visible = false;
        checkedOutLabel.Visible = false;
        SpriteBox.UpdateSpriteBox(null);

        //change window icon
        //this.Icon = Properties.Resources.pc

        //update discord RPC
        discord.SpriteURL = "pc";
        discord.Nickname = null;
        discord.Ball = null;
        discord.UpdatePresence();
    }

    private void ResetFocus()
        => nicknameLabel.Focus(); //removes focus from any textbox

    public void DeselectSlot()
    {
        ResetFocus();
        collectionManager?.DeselectCurrentSlot();
        pkuCollectionManager?.DeselectCurrentSlot();
        selectedSlot = null;
        ClearSummaryTab();
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
        folderSelectorDialog.SelectedPath = currentpkuCollectionPath;
        folderSelectorDialog.InitialDirectory = currentpkuCollectionPath;
        folderSelectorDialog.ShowNewFolderButton = true;
        DialogResult drA = folderSelectorDialog.ShowDialog();
        if (drA == DialogResult.OK)
        {
            if (folderSelectorDialog.SelectedPath.IsSubPathOf(currentpkuCollectionPath))
            {
                var relPath = Path.GetRelativePath(currentpkuCollectionPath, folderSelectorDialog.SelectedPath);
                if (pkuCollectionManager.AddBox(relPath))
                {
                    ResetPKUBoxSelector(pkuCollectionManager.BoxCount - 1);
                    OnPKUBoxDisplayRefreshed(null, null);
                }
                else
                    MessageBox.Show("That folder is invalid, or is already a box in the pkuCollection", "Invalid folder");
            }
            else
                MessageBox.Show("Only folders inside your pkuCollection can be added as new boxes.", "Invalid folder");
        }
    }

    private void removeCurrentBoxButton_Click(object sender, EventArgs e)
    {
        if (pkuCollectionManager.RemoveCurrentBox())
        {
            ResetPKUBoxSelector();
            OnPKUBoxDisplayRefreshed(null, null);
        }
    }

    private void openBoxInFileExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        => pkuCollectionManager.OpenBoxInFileExplorer();


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
}