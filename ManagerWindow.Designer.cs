namespace pkuManager
{
    partial class ManagerWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManagerWindow));
            this.boxSelector = new System.Windows.Forms.ComboBox();
            this.changeBoxNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBoxSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.customToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBoxImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openBoxInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LHSTabs = new System.Windows.Forms.TabControl();
            this.summaryTab = new System.Windows.Forms.TabPage();
            this.appearanceTextBox = new System.Windows.Forms.TextBox();
            this.formsTextBox = new System.Windows.Forms.TextBox();
            this.speciesTextBox = new System.Windows.Forms.TextBox();
            this.gameTextBox = new System.Windows.Forms.TextBox();
            this.otTextBox = new System.Windows.Forms.TextBox();
            this.nicknameTextBox = new System.Windows.Forms.TextBox();
            this.appearanceLabel = new System.Windows.Forms.Label();
            this.formsLabel = new System.Windows.Forms.Label();
            this.locationLabel = new System.Windows.Forms.Label();
            this.gameLabel = new System.Windows.Forms.Label();
            this.locationTextBox = new System.Windows.Forms.TextBox();
            this.otLabel = new System.Windows.Forms.Label();
            this.speciesLabel = new System.Windows.Forms.Label();
            this.nicknameLabel = new System.Windows.Forms.Label();
            this.exportTab = new System.Windows.Forms.TabPage();
            this.exportToggleButton = new System.Windows.Forms.Button();
            this.exportButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.importTab = new System.Windows.Forms.TabPage();
            this.importToggleButton = new System.Windows.Forms.Button();
            this.importButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.checkedOutLabel = new System.Windows.Forms.Label();
            this.viewCheckedoutButton = new System.Windows.Forms.Button();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutDock = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.boxDisplayDock = new System.Windows.Forms.Panel();
            this.openDropDown = new System.Windows.Forms.ToolStripMenuItem();
            this.openACollectionButton = new System.Windows.Forms.ToolStripMenuItem();
            this.createANewCollectionButton = new System.Windows.Forms.ToolStripMenuItem();
            this.collectionOptionsDropDown = new System.Windows.Forms.ToolStripMenuItem();
            this.enableBattleStatOverrideButton = new System.Windows.Forms.ToolStripMenuItem();
            this.enableDefaultFormOverrideButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBar = new System.Windows.Forms.MenuStrip();
            this.boxOptionsDropDown = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBoxTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boxOptionsList = new System.Windows.Forms.ToolStripMenuItem();
            this.boxOptions30 = new System.Windows.Forms.ToolStripMenuItem();
            this.boxOptions60 = new System.Windows.Forms.ToolStripMenuItem();
            this.boxOptions96 = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewBoxButton = new System.Windows.Forms.ToolStripMenuItem();
            this.removeCurrentBoxButton = new System.Windows.Forms.ToolStripMenuItem();
            this.openBoxInFileExplorerButton = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsDropDown = new System.Windows.Forms.ToolStripMenuItem();
            this.sendToRecycleButton = new System.Windows.Forms.ToolStripMenuItem();
            this.askBeforeAutoAddButton = new System.Windows.Forms.ToolStripMenuItem();
            this.hideDiscordPresenceButton = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutButton = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshBoxButton = new System.Windows.Forms.Button();
            this.LHSTabs.SuspendLayout();
            this.summaryTab.SuspendLayout();
            this.exportTab.SuspendLayout();
            this.importTab.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.toolBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // boxSelector
            // 
            this.boxSelector.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.boxSelector.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.boxSelector.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.boxSelector.FormattingEnabled = true;
            this.boxSelector.Location = new System.Drawing.Point(332, 31);
            this.boxSelector.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.boxSelector.Name = "boxSelector";
            this.boxSelector.Size = new System.Drawing.Size(140, 23);
            this.boxSelector.TabIndex = 4;
            this.boxSelector.SelectedIndexChanged += new System.EventHandler(this.boxSelector_SelectedIndexChanged);
            // 
            // changeBoxNameToolStripMenuItem
            // 
            this.changeBoxNameToolStripMenuItem.Name = "changeBoxNameToolStripMenuItem";
            this.changeBoxNameToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.changeBoxNameToolStripMenuItem.Text = "Change box name";
            // 
            // changeBoxSizeToolStripMenuItem
            // 
            this.changeBoxSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.customToolStripMenuItem});
            this.changeBoxSizeToolStripMenuItem.Name = "changeBoxSizeToolStripMenuItem";
            this.changeBoxSizeToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.changeBoxSizeToolStripMenuItem.Text = "Change box size";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(67, 22);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(67, 22);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(67, 22);
            // 
            // customToolStripMenuItem
            // 
            this.customToolStripMenuItem.Name = "customToolStripMenuItem";
            this.customToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            // 
            // changeBoxImageToolStripMenuItem
            // 
            this.changeBoxImageToolStripMenuItem.Name = "changeBoxImageToolStripMenuItem";
            this.changeBoxImageToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            // 
            // openBoxInExplorerToolStripMenuItem
            // 
            this.openBoxInExplorerToolStripMenuItem.Name = "openBoxInExplorerToolStripMenuItem";
            this.openBoxInExplorerToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            // 
            // LHSTabs
            // 
            this.LHSTabs.Controls.Add(this.summaryTab);
            this.LHSTabs.Controls.Add(this.exportTab);
            this.LHSTabs.Controls.Add(this.importTab);
            this.LHSTabs.Location = new System.Drawing.Point(0, 31);
            this.LHSTabs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LHSTabs.Name = "LHSTabs";
            this.LHSTabs.SelectedIndex = 0;
            this.LHSTabs.Size = new System.Drawing.Size(328, 324);
            this.LHSTabs.TabIndex = 7;
            // 
            // summaryTab
            // 
            this.summaryTab.Controls.Add(this.appearanceTextBox);
            this.summaryTab.Controls.Add(this.formsTextBox);
            this.summaryTab.Controls.Add(this.speciesTextBox);
            this.summaryTab.Controls.Add(this.gameTextBox);
            this.summaryTab.Controls.Add(this.otTextBox);
            this.summaryTab.Controls.Add(this.nicknameTextBox);
            this.summaryTab.Controls.Add(this.appearanceLabel);
            this.summaryTab.Controls.Add(this.formsLabel);
            this.summaryTab.Controls.Add(this.locationLabel);
            this.summaryTab.Controls.Add(this.gameLabel);
            this.summaryTab.Controls.Add(this.locationTextBox);
            this.summaryTab.Controls.Add(this.otLabel);
            this.summaryTab.Controls.Add(this.speciesLabel);
            this.summaryTab.Controls.Add(this.nicknameLabel);
            this.summaryTab.Location = new System.Drawing.Point(4, 24);
            this.summaryTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.summaryTab.Name = "summaryTab";
            this.summaryTab.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.summaryTab.Size = new System.Drawing.Size(320, 296);
            this.summaryTab.TabIndex = 0;
            this.summaryTab.Text = "Summary";
            this.summaryTab.UseVisualStyleBackColor = true;
            // 
            // appearanceTextBox
            // 
            this.appearanceTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.appearanceTextBox.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.appearanceTextBox.Location = new System.Drawing.Point(98, 122);
            this.appearanceTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.appearanceTextBox.Name = "appearanceTextBox";
            this.appearanceTextBox.ReadOnly = true;
            this.appearanceTextBox.Size = new System.Drawing.Size(116, 22);
            this.appearanceTextBox.TabIndex = 13;
            // 
            // formsTextBox
            // 
            this.formsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.formsTextBox.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.formsTextBox.Location = new System.Drawing.Point(98, 100);
            this.formsTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.formsTextBox.Name = "formsTextBox";
            this.formsTextBox.ReadOnly = true;
            this.formsTextBox.Size = new System.Drawing.Size(116, 22);
            this.formsTextBox.TabIndex = 10;
            // 
            // speciesTextBox
            // 
            this.speciesTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.speciesTextBox.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.speciesTextBox.Location = new System.Drawing.Point(98, 78);
            this.speciesTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.speciesTextBox.Name = "speciesTextBox";
            this.speciesTextBox.ReadOnly = true;
            this.speciesTextBox.Size = new System.Drawing.Size(116, 22);
            this.speciesTextBox.TabIndex = 5;
            // 
            // gameTextBox
            // 
            this.gameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gameTextBox.Location = new System.Drawing.Point(98, 55);
            this.gameTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gameTextBox.Name = "gameTextBox";
            this.gameTextBox.ReadOnly = true;
            this.gameTextBox.Size = new System.Drawing.Size(116, 23);
            this.gameTextBox.TabIndex = 6;
            // 
            // otTextBox
            // 
            this.otTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.otTextBox.Location = new System.Drawing.Point(98, 32);
            this.otTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.otTextBox.Name = "otTextBox";
            this.otTextBox.ReadOnly = true;
            this.otTextBox.Size = new System.Drawing.Size(116, 23);
            this.otTextBox.TabIndex = 4;
            // 
            // nicknameTextBox
            // 
            this.nicknameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nicknameTextBox.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nicknameTextBox.Location = new System.Drawing.Point(98, 10);
            this.nicknameTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.nicknameTextBox.Name = "nicknameTextBox";
            this.nicknameTextBox.ReadOnly = true;
            this.nicknameTextBox.Size = new System.Drawing.Size(116, 22);
            this.nicknameTextBox.TabIndex = 3;
            // 
            // appearanceLabel
            // 
            this.appearanceLabel.AutoSize = true;
            this.appearanceLabel.Location = new System.Drawing.Point(12, 124);
            this.appearanceLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.appearanceLabel.Name = "appearanceLabel";
            this.appearanceLabel.Size = new System.Drawing.Size(73, 15);
            this.appearanceLabel.TabIndex = 12;
            this.appearanceLabel.Text = "Appearance:";
            // 
            // formsLabel
            // 
            this.formsLabel.AutoSize = true;
            this.formsLabel.Location = new System.Drawing.Point(12, 102);
            this.formsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.formsLabel.Name = "formsLabel";
            this.formsLabel.Size = new System.Drawing.Size(43, 15);
            this.formsLabel.TabIndex = 11;
            this.formsLabel.Text = "Forms:";
            // 
            // locationLabel
            // 
            this.locationLabel.AutoSize = true;
            this.locationLabel.Location = new System.Drawing.Point(15, 264);
            this.locationLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.locationLabel.Name = "locationLabel";
            this.locationLabel.Size = new System.Drawing.Size(61, 15);
            this.locationLabel.TabIndex = 9;
            this.locationLabel.Text = "Filename: ";
            // 
            // gameLabel
            // 
            this.gameLabel.AutoSize = true;
            this.gameLabel.Location = new System.Drawing.Point(12, 58);
            this.gameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.gameLabel.Name = "gameLabel";
            this.gameLabel.Size = new System.Drawing.Size(77, 15);
            this.gameLabel.TabIndex = 7;
            this.gameLabel.Text = "Origin Game:";
            // 
            // locationTextBox
            // 
            this.locationTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.locationTextBox.Location = new System.Drawing.Point(98, 261);
            this.locationTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.locationTextBox.Name = "locationTextBox";
            this.locationTextBox.ReadOnly = true;
            this.locationTextBox.Size = new System.Drawing.Size(116, 23);
            this.locationTextBox.TabIndex = 8;
            // 
            // otLabel
            // 
            this.otLabel.AutoSize = true;
            this.otLabel.Location = new System.Drawing.Point(12, 35);
            this.otLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.otLabel.Name = "otLabel";
            this.otLabel.Size = new System.Drawing.Size(49, 15);
            this.otLabel.TabIndex = 1;
            this.otLabel.Text = "True OT:";
            // 
            // speciesLabel
            // 
            this.speciesLabel.AutoSize = true;
            this.speciesLabel.Location = new System.Drawing.Point(12, 80);
            this.speciesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.speciesLabel.Name = "speciesLabel";
            this.speciesLabel.Size = new System.Drawing.Size(49, 15);
            this.speciesLabel.TabIndex = 2;
            this.speciesLabel.Text = "Species:";
            // 
            // nicknameLabel
            // 
            this.nicknameLabel.AutoSize = true;
            this.nicknameLabel.Location = new System.Drawing.Point(12, 12);
            this.nicknameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.nicknameLabel.Name = "nicknameLabel";
            this.nicknameLabel.Size = new System.Drawing.Size(64, 15);
            this.nicknameLabel.TabIndex = 0;
            this.nicknameLabel.Text = "Nickname:";
            // 
            // exportTab
            // 
            this.exportTab.Controls.Add(this.exportToggleButton);
            this.exportTab.Controls.Add(this.exportButtons);
            this.exportTab.Location = new System.Drawing.Point(4, 24);
            this.exportTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.exportTab.Name = "exportTab";
            this.exportTab.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.exportTab.Size = new System.Drawing.Size(320, 296);
            this.exportTab.TabIndex = 1;
            this.exportTab.Text = "Export";
            this.exportTab.UseVisualStyleBackColor = true;
            // 
            // exportToggleButton
            // 
            this.exportToggleButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exportToggleButton.Location = new System.Drawing.Point(18, 12);
            this.exportToggleButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.exportToggleButton.Name = "exportToggleButton";
            this.exportToggleButton.Size = new System.Drawing.Size(127, 27);
            this.exportToggleButton.TabIndex = 0;
            this.exportToggleButton.Text = "Toggle Export";
            this.exportToggleButton.UseVisualStyleBackColor = true;
            this.exportToggleButton.Click += new System.EventHandler(this.ImportExportToggleButton_Click);
            // 
            // exportButtons
            // 
            this.exportButtons.Location = new System.Drawing.Point(2, 50);
            this.exportButtons.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.exportButtons.Name = "exportButtons";
            this.exportButtons.Size = new System.Drawing.Size(313, 243);
            this.exportButtons.TabIndex = 4;
            // 
            // importTab
            // 
            this.importTab.Controls.Add(this.importToggleButton);
            this.importTab.Controls.Add(this.importButtons);
            this.importTab.Location = new System.Drawing.Point(4, 24);
            this.importTab.Name = "importTab";
            this.importTab.Padding = new System.Windows.Forms.Padding(3);
            this.importTab.Size = new System.Drawing.Size(320, 296);
            this.importTab.TabIndex = 2;
            this.importTab.Text = "Import";
            this.importTab.UseVisualStyleBackColor = true;
            // 
            // importToggleButton
            // 
            this.importToggleButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.importToggleButton.Location = new System.Drawing.Point(18, 12);
            this.importToggleButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importToggleButton.Name = "importToggleButton";
            this.importToggleButton.Size = new System.Drawing.Size(127, 27);
            this.importToggleButton.TabIndex = 6;
            this.importToggleButton.Text = "Toggle Import";
            this.importToggleButton.UseVisualStyleBackColor = true;
            this.importToggleButton.Click += new System.EventHandler(this.ImportExportToggleButton_Click);
            // 
            // importButtons
            // 
            this.importButtons.Location = new System.Drawing.Point(3, 50);
            this.importButtons.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importButtons.Name = "importButtons";
            this.importButtons.Size = new System.Drawing.Size(313, 243);
            this.importButtons.TabIndex = 5;
            // 
            // checkedOutLabel
            // 
            this.checkedOutLabel.AutoSize = true;
            this.checkedOutLabel.BackColor = System.Drawing.Color.White;
            this.checkedOutLabel.Location = new System.Drawing.Point(233, 64);
            this.checkedOutLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.checkedOutLabel.Name = "checkedOutLabel";
            this.checkedOutLabel.Size = new System.Drawing.Size(78, 30);
            this.checkedOutLabel.TabIndex = 10;
            this.checkedOutLabel.Text = "Currently\r\nChecked-Out";
            this.checkedOutLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.checkedOutLabel.Visible = false;
            // 
            // viewCheckedoutButton
            // 
            this.viewCheckedoutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.viewCheckedoutButton.Location = new System.Drawing.Point(572, 30);
            this.viewCheckedoutButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.viewCheckedoutButton.Name = "viewCheckedoutButton";
            this.viewCheckedoutButton.Size = new System.Drawing.Size(107, 24);
            this.viewCheckedoutButton.TabIndex = 12;
            this.viewCheckedoutButton.Text = "View Check-Out";
            this.viewCheckedoutButton.UseVisualStyleBackColor = true;
            this.viewCheckedoutButton.Visible = false;
            this.viewCheckedoutButton.Click += new System.EventHandler(this.viewCheckedoutButton_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(12, 20);
            // 
            // checkoutDock
            // 
            this.checkoutDock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkoutDock.AutoSize = true;
            this.checkoutDock.Location = new System.Drawing.Point(24, 3);
            this.checkoutDock.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkoutDock.MinimumSize = new System.Drawing.Size(12, 12);
            this.checkoutDock.Name = "checkoutDock";
            this.checkoutDock.Size = new System.Drawing.Size(12, 12);
            this.checkoutDock.TabIndex = 11;
            this.checkoutDock.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.boxDisplayDock);
            this.flowLayoutPanel1.Controls.Add(this.checkoutDock);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(329, 60);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(40, 18);
            this.flowLayoutPanel1.TabIndex = 14;
            // 
            // boxDisplayDock
            // 
            this.boxDisplayDock.AutoSize = true;
            this.boxDisplayDock.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.boxDisplayDock.Location = new System.Drawing.Point(4, 3);
            this.boxDisplayDock.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.boxDisplayDock.MinimumSize = new System.Drawing.Size(12, 12);
            this.boxDisplayDock.Name = "boxDisplayDock";
            this.boxDisplayDock.Size = new System.Drawing.Size(12, 12);
            this.boxDisplayDock.TabIndex = 15;
            // 
            // openDropDown
            // 
            this.openDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openACollectionButton,
            this.createANewCollectionButton});
            this.openDropDown.Name = "openDropDown";
            this.openDropDown.Size = new System.Drawing.Size(48, 20);
            this.openDropDown.Text = "Open";
            // 
            // openACollectionButton
            // 
            this.openACollectionButton.Name = "openACollectionButton";
            this.openACollectionButton.Size = new System.Drawing.Size(199, 22);
            this.openACollectionButton.Text = "Open a Collection";
            this.openACollectionButton.Click += new System.EventHandler(this.openACollectionButton_Click);
            // 
            // createANewCollectionButton
            // 
            this.createANewCollectionButton.Name = "createANewCollectionButton";
            this.createANewCollectionButton.Size = new System.Drawing.Size(199, 22);
            this.createANewCollectionButton.Text = "Create a new Collection";
            this.createANewCollectionButton.Click += new System.EventHandler(this.createANewCollectionToolStripMenuItem_Click);
            // 
            // collectionOptionsDropDown
            // 
            this.collectionOptionsDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableBattleStatOverrideButton,
            this.enableDefaultFormOverrideButton});
            this.collectionOptionsDropDown.Enabled = false;
            this.collectionOptionsDropDown.Name = "collectionOptionsDropDown";
            this.collectionOptionsDropDown.Size = new System.Drawing.Size(118, 20);
            this.collectionOptionsDropDown.Text = "Collection Options";
            // 
            // enableBattleStatOverrideButton
            // 
            this.enableBattleStatOverrideButton.CheckOnClick = true;
            this.enableBattleStatOverrideButton.Name = "enableBattleStatOverrideButton";
            this.enableBattleStatOverrideButton.Size = new System.Drawing.Size(229, 22);
            this.enableBattleStatOverrideButton.Text = "Enable Battle Stat Override";
            this.enableBattleStatOverrideButton.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.enableBattleStatOverrideButton.ToolTipText = "Overrides exported pku\'s nature with its stat nature\r\nand its IVs with its hyper " +
    "training, if specified. Only\r\napplies to formats that don\'t use these battle sta" +
    "ts.";
            this.enableBattleStatOverrideButton.CheckedChanged += new System.EventHandler(this.enableBattleStatOverrideButton_CheckedChanged);
            // 
            // enableDefaultFormOverrideButton
            // 
            this.enableDefaultFormOverrideButton.CheckOnClick = true;
            this.enableDefaultFormOverrideButton.Name = "enableDefaultFormOverrideButton";
            this.enableDefaultFormOverrideButton.Size = new System.Drawing.Size(229, 22);
            this.enableDefaultFormOverrideButton.Text = "Enable Default Form Override";
            this.enableDefaultFormOverrideButton.ToolTipText = "Tries casting pku to its default form, if it cannot otherwise be exported.";
            this.enableDefaultFormOverrideButton.Click += new System.EventHandler(this.enableDefaultFormOverrideButton_Click);
            // 
            // toolBar
            // 
            this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openDropDown,
            this.collectionOptionsDropDown,
            this.boxOptionsDropDown,
            this.settingsDropDown});
            this.toolBar.Location = new System.Drawing.Point(0, 0);
            this.toolBar.Name = "toolBar";
            this.toolBar.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.toolBar.Size = new System.Drawing.Size(692, 24);
            this.toolBar.TabIndex = 13;
            this.toolBar.Text = "toolBar";
            // 
            // boxOptionsDropDown
            // 
            this.boxOptionsDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeBoxTypeToolStripMenuItem,
            this.addNewBoxButton,
            this.removeCurrentBoxButton,
            this.openBoxInFileExplorerButton});
            this.boxOptionsDropDown.Enabled = false;
            this.boxOptionsDropDown.Name = "boxOptionsDropDown";
            this.boxOptionsDropDown.Size = new System.Drawing.Size(84, 20);
            this.boxOptionsDropDown.Text = "Box Options";
            // 
            // changeBoxTypeToolStripMenuItem
            // 
            this.changeBoxTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.boxOptionsList,
            this.boxOptions30,
            this.boxOptions60,
            this.boxOptions96});
            this.changeBoxTypeToolStripMenuItem.Name = "changeBoxTypeToolStripMenuItem";
            this.changeBoxTypeToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.changeBoxTypeToolStripMenuItem.Text = "Change Box Type";
            // 
            // boxOptionsList
            // 
            this.boxOptionsList.Name = "boxOptionsList";
            this.boxOptionsList.Size = new System.Drawing.Size(92, 22);
            this.boxOptionsList.Text = "List";
            this.boxOptionsList.Click += new System.EventHandler(this.boxOptionsType_Click);
            // 
            // boxOptions30
            // 
            this.boxOptions30.Name = "boxOptions30";
            this.boxOptions30.Size = new System.Drawing.Size(92, 22);
            this.boxOptions30.Text = "30";
            this.boxOptions30.Click += new System.EventHandler(this.boxOptionsType_Click);
            // 
            // boxOptions60
            // 
            this.boxOptions60.Name = "boxOptions60";
            this.boxOptions60.Size = new System.Drawing.Size(92, 22);
            this.boxOptions60.Text = "60";
            this.boxOptions60.Click += new System.EventHandler(this.boxOptionsType_Click);
            // 
            // boxOptions96
            // 
            this.boxOptions96.Name = "boxOptions96";
            this.boxOptions96.Size = new System.Drawing.Size(92, 22);
            this.boxOptions96.Text = "96";
            this.boxOptions96.Click += new System.EventHandler(this.boxOptionsType_Click);
            // 
            // addNewBoxButton
            // 
            this.addNewBoxButton.Name = "addNewBoxButton";
            this.addNewBoxButton.Size = new System.Drawing.Size(206, 22);
            this.addNewBoxButton.Text = "Add New Box";
            this.addNewBoxButton.Click += new System.EventHandler(this.addNewBoxButton_Click);
            // 
            // removeCurrentBoxButton
            // 
            this.removeCurrentBoxButton.Name = "removeCurrentBoxButton";
            this.removeCurrentBoxButton.Size = new System.Drawing.Size(206, 22);
            this.removeCurrentBoxButton.Text = "Remove Current Box";
            this.removeCurrentBoxButton.Click += new System.EventHandler(this.removeCurrentBoxButton_Click);
            // 
            // openBoxInFileExplorerButton
            // 
            this.openBoxInFileExplorerButton.Name = "openBoxInFileExplorerButton";
            this.openBoxInFileExplorerButton.Size = new System.Drawing.Size(206, 22);
            this.openBoxInFileExplorerButton.Text = "Open Box In File Explorer";
            this.openBoxInFileExplorerButton.Click += new System.EventHandler(this.openBoxInFileExplorerToolStripMenuItem_Click);
            // 
            // settingsDropDown
            // 
            this.settingsDropDown.CheckOnClick = true;
            this.settingsDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sendToRecycleButton,
            this.askBeforeAutoAddButton,
            this.hideDiscordPresenceButton,
            this.aboutButton});
            this.settingsDropDown.Name = "settingsDropDown";
            this.settingsDropDown.Size = new System.Drawing.Size(61, 20);
            this.settingsDropDown.Text = "Settings";
            // 
            // sendToRecycleButton
            // 
            this.sendToRecycleButton.CheckOnClick = true;
            this.sendToRecycleButton.Name = "sendToRecycleButton";
            this.sendToRecycleButton.Size = new System.Drawing.Size(192, 22);
            this.sendToRecycleButton.Text = "Send to Recycle";
            this.sendToRecycleButton.ToolTipText = "Enabling this sends released Pokemon to the Recycle Bin rather than deleting them" +
    " permanently.";
            this.sendToRecycleButton.CheckedChanged += new System.EventHandler(this.sendToRecycleToolStripMenuItem_CheckedChanged);
            // 
            // askBeforeAutoAddButton
            // 
            this.askBeforeAutoAddButton.CheckOnClick = true;
            this.askBeforeAutoAddButton.Name = "askBeforeAutoAddButton";
            this.askBeforeAutoAddButton.Size = new System.Drawing.Size(192, 22);
            this.askBeforeAutoAddButton.Text = "Ask Before Auto-Add";
            this.askBeforeAutoAddButton.CheckedChanged += new System.EventHandler(this.askBeforeAutoAddToolStripMenuItem_CheckedChanged);
            this.askBeforeAutoAddButton.Click += new System.EventHandler(this.askBeforeAutoAddToolStripMenuItem_CheckedChanged);
            // 
            // hideDiscordPresenceButton
            // 
            this.hideDiscordPresenceButton.CheckOnClick = true;
            this.hideDiscordPresenceButton.Name = "hideDiscordPresenceButton";
            this.hideDiscordPresenceButton.Size = new System.Drawing.Size(192, 22);
            this.hideDiscordPresenceButton.Text = "Hide Discord Presence";
            this.hideDiscordPresenceButton.CheckedChanged += new System.EventHandler(this.hideDiscordPresenceToolStripMenuItem_CheckedChanged);
            this.hideDiscordPresenceButton.Click += new System.EventHandler(this.hideDiscordPresenceToolStripMenuItem_CheckedChanged);
            // 
            // aboutButton
            // 
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.Size = new System.Drawing.Size(192, 22);
            this.aboutButton.Text = "About";
            this.aboutButton.Visible = false;
            this.aboutButton.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // refreshBoxButton
            // 
            this.refreshBoxButton.Enabled = false;
            this.refreshBoxButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refreshBoxButton.Location = new System.Drawing.Point(480, 30);
            this.refreshBoxButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.refreshBoxButton.Name = "refreshBoxButton";
            this.refreshBoxButton.Size = new System.Drawing.Size(84, 24);
            this.refreshBoxButton.TabIndex = 15;
            this.refreshBoxButton.Text = "Refresh Box";
            this.refreshBoxButton.UseVisualStyleBackColor = true;
            this.refreshBoxButton.Click += new System.EventHandler(this.refreshBox_Click);
            // 
            // ManagerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(692, 356);
            this.Controls.Add(this.refreshBoxButton);
            this.Controls.Add(this.checkedOutLabel);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.viewCheckedoutButton);
            this.Controls.Add(this.LHSTabs);
            this.Controls.Add(this.boxSelector);
            this.Controls.Add(this.toolBar);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.toolBar;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(615, 395);
            this.Name = "ManagerWindow";
            this.Text = "pkuManager";
            this.LHSTabs.ResumeLayout(false);
            this.summaryTab.ResumeLayout(false);
            this.summaryTab.PerformLayout();
            this.exportTab.ResumeLayout(false);
            this.importTab.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.toolBar.ResumeLayout(false);
            this.toolBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox boxSelector;
        private System.Windows.Forms.ToolStripMenuItem openBoxInExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBoxNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBoxSizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem customToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBoxImageToolStripMenuItem;
        private System.Windows.Forms.TabControl LHSTabs;
        private System.Windows.Forms.TabPage summaryTab;
        private System.Windows.Forms.TabPage exportTab;
        private System.Windows.Forms.TextBox speciesTextBox;
        private System.Windows.Forms.TextBox otTextBox;
        private System.Windows.Forms.TextBox nicknameTextBox;
        private System.Windows.Forms.Label speciesLabel;
        private System.Windows.Forms.Label otLabel;
        private System.Windows.Forms.Label nicknameLabel;
        private System.Windows.Forms.Label gameLabel;
        private System.Windows.Forms.TextBox gameTextBox;
        private System.Windows.Forms.Label locationLabel;
        private System.Windows.Forms.TextBox locationTextBox;
        private System.Windows.Forms.FlowLayoutPanel exportButtons;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.Panel checkoutDock;
        private System.Windows.Forms.Button viewCheckedoutButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button exportToggleButton;
        private System.Windows.Forms.Label checkedOutLabel;
        private System.Windows.Forms.Panel boxDisplayDock;
        private System.Windows.Forms.ToolStripMenuItem openDropDown;
        private System.Windows.Forms.ToolStripMenuItem collectionOptionsDropDown;
        private System.Windows.Forms.MenuStrip toolBar;
        private System.Windows.Forms.ToolStripMenuItem enableBattleStatOverrideButton;
        private System.Windows.Forms.ToolStripMenuItem boxOptionsDropDown;
        private System.Windows.Forms.ToolStripMenuItem changeBoxTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boxOptionsList;
        private System.Windows.Forms.ToolStripMenuItem boxOptions30;
        private System.Windows.Forms.ToolStripMenuItem boxOptions60;
        private System.Windows.Forms.ToolStripMenuItem boxOptions96;
        private System.Windows.Forms.ToolStripMenuItem addNewBoxButton;
        private System.Windows.Forms.ToolStripMenuItem removeCurrentBoxButton;
        private System.Windows.Forms.ToolStripMenuItem openBoxInFileExplorerButton;
        private System.Windows.Forms.Label formsLabel;
        private System.Windows.Forms.TextBox formsTextBox;
        private System.Windows.Forms.TextBox appearanceTextBox;
        private System.Windows.Forms.Label appearanceLabel;
        private System.Windows.Forms.TabPage importTab;
        private System.Windows.Forms.Button importToggleButton;
        private System.Windows.Forms.FlowLayoutPanel importButtons;
        private System.Windows.Forms.ToolStripMenuItem settingsDropDown;
        private System.Windows.Forms.ToolStripMenuItem sendToRecycleButton;
        private System.Windows.Forms.ToolStripMenuItem askBeforeAutoAddButton;
        private System.Windows.Forms.ToolStripMenuItem hideDiscordPresenceButton;
        private System.Windows.Forms.ToolStripMenuItem aboutButton;
        private System.Windows.Forms.Button refreshBoxButton;
        private System.Windows.Forms.ToolStripMenuItem openACollectionButton;
        private System.Windows.Forms.ToolStripMenuItem createANewCollectionButton;
        private System.Windows.Forms.ToolStripMenuItem enableDefaultFormOverrideButton;
    }
}

