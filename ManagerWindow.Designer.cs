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
            this.components = new System.ComponentModel.Container();
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
            this.Summary = new System.Windows.Forms.TabPage();
            this.locationLabel = new System.Windows.Forms.Label();
            this.locationTextBox = new System.Windows.Forms.TextBox();
            this.gameLabel = new System.Windows.Forms.Label();
            this.gameTextBox = new System.Windows.Forms.TextBox();
            this.speciesTextBox = new System.Windows.Forms.TextBox();
            this.otTextBox = new System.Windows.Forms.TextBox();
            this.nicknameTextBox = new System.Windows.Forms.TextBox();
            this.speciesLabel = new System.Windows.Forms.Label();
            this.otLabel = new System.Windows.Forms.Label();
            this.nicknameLabel = new System.Windows.Forms.Label();
            this.Export = new System.Windows.Forms.TabPage();
            this.checkedOutLabel = new System.Windows.Forms.Label();
            this.exportToggleButton = new System.Windows.Forms.Button();
            this.exportButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.viewCheckedoutButton = new System.Windows.Forms.Button();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutDock = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.boxDisplayDock = new System.Windows.Forms.Panel();
            this.openACollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importAPKUToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boxOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBoxTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boxOptionsList = new System.Windows.Forms.ToolStripMenuItem();
            this.boxOptions30 = new System.Windows.Forms.ToolStripMenuItem();
            this.boxOptions60 = new System.Windows.Forms.ToolStripMenuItem();
            this.boxOptions96 = new System.Windows.Forms.ToolStripMenuItem();
            this.enableBattleStatOverrideButton = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.LHSTabs.SuspendLayout();
            this.Summary.SuspendLayout();
            this.Export.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // boxSelector
            // 
            this.boxSelector.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.boxSelector.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.boxSelector.FormattingEnabled = true;
            this.boxSelector.Location = new System.Drawing.Point(285, 27);
            this.boxSelector.Name = "boxSelector";
            this.boxSelector.Size = new System.Drawing.Size(121, 21);
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
            this.LHSTabs.Controls.Add(this.Summary);
            this.LHSTabs.Controls.Add(this.Export);
            this.LHSTabs.Location = new System.Drawing.Point(0, 27);
            this.LHSTabs.Name = "LHSTabs";
            this.LHSTabs.SelectedIndex = 0;
            this.LHSTabs.Size = new System.Drawing.Size(281, 311);
            this.LHSTabs.TabIndex = 7;
            // 
            // Summary
            // 
            this.Summary.Controls.Add(this.locationLabel);
            this.Summary.Controls.Add(this.locationTextBox);
            this.Summary.Controls.Add(this.gameLabel);
            this.Summary.Controls.Add(this.gameTextBox);
            this.Summary.Controls.Add(this.speciesTextBox);
            this.Summary.Controls.Add(this.otTextBox);
            this.Summary.Controls.Add(this.nicknameTextBox);
            this.Summary.Controls.Add(this.speciesLabel);
            this.Summary.Controls.Add(this.otLabel);
            this.Summary.Controls.Add(this.nicknameLabel);
            this.Summary.Location = new System.Drawing.Point(4, 22);
            this.Summary.Name = "Summary";
            this.Summary.Padding = new System.Windows.Forms.Padding(3);
            this.Summary.Size = new System.Drawing.Size(273, 285);
            this.Summary.TabIndex = 0;
            this.Summary.Text = "Summary";
            this.Summary.UseVisualStyleBackColor = true;
            // 
            // locationLabel
            // 
            this.locationLabel.AutoSize = true;
            this.locationLabel.Location = new System.Drawing.Point(13, 256);
            this.locationLabel.Name = "locationLabel";
            this.locationLabel.Size = new System.Drawing.Size(55, 13);
            this.locationLabel.TabIndex = 9;
            this.locationLabel.Text = "Filename: ";
            // 
            // locationTextBox
            // 
            this.locationTextBox.Location = new System.Drawing.Point(84, 253);
            this.locationTextBox.Name = "locationTextBox";
            this.locationTextBox.ReadOnly = true;
            this.locationTextBox.Size = new System.Drawing.Size(100, 20);
            this.locationTextBox.TabIndex = 8;
            // 
            // gameLabel
            // 
            this.gameLabel.AutoSize = true;
            this.gameLabel.Location = new System.Drawing.Point(10, 89);
            this.gameLabel.Name = "gameLabel";
            this.gameLabel.Size = new System.Drawing.Size(68, 13);
            this.gameLabel.TabIndex = 7;
            this.gameLabel.Text = "Origin Game:";
            // 
            // gameTextBox
            // 
            this.gameTextBox.Location = new System.Drawing.Point(84, 86);
            this.gameTextBox.Name = "gameTextBox";
            this.gameTextBox.ReadOnly = true;
            this.gameTextBox.Size = new System.Drawing.Size(100, 20);
            this.gameTextBox.TabIndex = 6;
            // 
            // speciesTextBox
            // 
            this.speciesTextBox.Location = new System.Drawing.Point(84, 34);
            this.speciesTextBox.Name = "speciesTextBox";
            this.speciesTextBox.ReadOnly = true;
            this.speciesTextBox.Size = new System.Drawing.Size(100, 20);
            this.speciesTextBox.TabIndex = 5;
            // 
            // otTextBox
            // 
            this.otTextBox.Location = new System.Drawing.Point(84, 60);
            this.otTextBox.Name = "otTextBox";
            this.otTextBox.ReadOnly = true;
            this.otTextBox.Size = new System.Drawing.Size(100, 20);
            this.otTextBox.TabIndex = 4;
            // 
            // nicknameTextBox
            // 
            this.nicknameTextBox.Location = new System.Drawing.Point(84, 7);
            this.nicknameTextBox.Name = "nicknameTextBox";
            this.nicknameTextBox.ReadOnly = true;
            this.nicknameTextBox.Size = new System.Drawing.Size(100, 20);
            this.nicknameTextBox.TabIndex = 3;
            // 
            // speciesLabel
            // 
            this.speciesLabel.AutoSize = true;
            this.speciesLabel.Location = new System.Drawing.Point(10, 37);
            this.speciesLabel.Name = "speciesLabel";
            this.speciesLabel.Size = new System.Drawing.Size(48, 13);
            this.speciesLabel.TabIndex = 2;
            this.speciesLabel.Text = "Species:";
            // 
            // otLabel
            // 
            this.otLabel.AutoSize = true;
            this.otLabel.Location = new System.Drawing.Point(10, 63);
            this.otLabel.Name = "otLabel";
            this.otLabel.Size = new System.Drawing.Size(50, 13);
            this.otLabel.TabIndex = 1;
            this.otLabel.Text = "True OT:";
            // 
            // nicknameLabel
            // 
            this.nicknameLabel.AutoSize = true;
            this.nicknameLabel.Location = new System.Drawing.Point(10, 10);
            this.nicknameLabel.Name = "nicknameLabel";
            this.nicknameLabel.Size = new System.Drawing.Size(58, 13);
            this.nicknameLabel.TabIndex = 0;
            this.nicknameLabel.Text = "Nickname:";
            // 
            // Export
            // 
            this.Export.Controls.Add(this.checkedOutLabel);
            this.Export.Controls.Add(this.exportToggleButton);
            this.Export.Controls.Add(this.exportButtons);
            this.Export.Location = new System.Drawing.Point(4, 22);
            this.Export.Name = "Export";
            this.Export.Padding = new System.Windows.Forms.Padding(3);
            this.Export.Size = new System.Drawing.Size(273, 285);
            this.Export.TabIndex = 1;
            this.Export.Text = "Export";
            this.Export.UseVisualStyleBackColor = true;
            // 
            // checkedOutLabel
            // 
            this.checkedOutLabel.AutoSize = true;
            this.checkedOutLabel.BackColor = System.Drawing.Color.White;
            this.checkedOutLabel.Location = new System.Drawing.Point(187, 3);
            this.checkedOutLabel.Name = "checkedOutLabel";
            this.checkedOutLabel.Size = new System.Drawing.Size(70, 26);
            this.checkedOutLabel.TabIndex = 10;
            this.checkedOutLabel.Text = "Currently\r\nChecked-Out";
            this.checkedOutLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.checkedOutLabel.Visible = false;
            // 
            // exportToggleButton
            // 
            this.exportToggleButton.Location = new System.Drawing.Point(15, 10);
            this.exportToggleButton.Name = "exportToggleButton";
            this.exportToggleButton.Size = new System.Drawing.Size(109, 23);
            this.exportToggleButton.TabIndex = 0;
            this.exportToggleButton.Text = "Toggle Export";
            this.exportToggleButton.UseVisualStyleBackColor = true;
            this.exportToggleButton.Click += new System.EventHandler(this.exportToggleButton_Click);
            // 
            // exportButtons
            // 
            this.exportButtons.Location = new System.Drawing.Point(2, 43);
            this.exportButtons.Name = "exportButtons";
            this.exportButtons.Size = new System.Drawing.Size(268, 242);
            this.exportButtons.TabIndex = 4;
            // 
            // viewCheckedoutButton
            // 
            this.viewCheckedoutButton.Location = new System.Drawing.Point(412, 27);
            this.viewCheckedoutButton.Name = "viewCheckedoutButton";
            this.viewCheckedoutButton.Size = new System.Drawing.Size(92, 21);
            this.viewCheckedoutButton.TabIndex = 12;
            this.viewCheckedoutButton.Text = "View Check-Out";
            this.viewCheckedoutButton.UseVisualStyleBackColor = true;
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
            this.checkoutDock.Location = new System.Drawing.Point(19, 3);
            this.checkoutDock.MinimumSize = new System.Drawing.Size(10, 10);
            this.checkoutDock.Name = "checkoutDock";
            this.checkoutDock.Size = new System.Drawing.Size(10, 10);
            this.checkoutDock.TabIndex = 11;
            this.checkoutDock.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.boxDisplayDock);
            this.flowLayoutPanel1.Controls.Add(this.checkoutDock);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(282, 52);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(32, 16);
            this.flowLayoutPanel1.TabIndex = 14;
            // 
            // boxDisplayDock
            // 
            this.boxDisplayDock.AutoSize = true;
            this.boxDisplayDock.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.boxDisplayDock.Location = new System.Drawing.Point(3, 3);
            this.boxDisplayDock.MinimumSize = new System.Drawing.Size(10, 10);
            this.boxDisplayDock.Name = "boxDisplayDock";
            this.boxDisplayDock.Size = new System.Drawing.Size(10, 10);
            this.boxDisplayDock.TabIndex = 15;
            // 
            // openACollectionToolStripMenuItem
            // 
            this.openACollectionToolStripMenuItem.Name = "openACollectionToolStripMenuItem";
            this.openACollectionToolStripMenuItem.Size = new System.Drawing.Size(114, 20);
            this.openACollectionToolStripMenuItem.Text = "Open a Collection";
            this.openACollectionToolStripMenuItem.Click += new System.EventHandler(this.setCollectionDirectory_Click);
            // 
            // importAPKUToolStripMenuItem
            // 
            this.importAPKUToolStripMenuItem.Enabled = false;
            this.importAPKUToolStripMenuItem.Name = "importAPKUToolStripMenuItem";
            this.importAPKUToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
            this.importAPKUToolStripMenuItem.Text = "Import a PKU";
            this.importAPKUToolStripMenuItem.Click += new System.EventHandler(this.importPkuButton_Click);
            // 
            // boxOptionsToolStripMenuItem
            // 
            this.boxOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeBoxTypeToolStripMenuItem,
            this.enableBattleStatOverrideButton});
            this.boxOptionsToolStripMenuItem.Enabled = false;
            this.boxOptionsToolStripMenuItem.Name = "boxOptionsToolStripMenuItem";
            this.boxOptionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.boxOptionsToolStripMenuItem.Text = "Options";
            // 
            // changeBoxTypeToolStripMenuItem
            // 
            this.changeBoxTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.boxOptionsList,
            this.boxOptions30,
            this.boxOptions60,
            this.boxOptions96});
            this.changeBoxTypeToolStripMenuItem.Name = "changeBoxTypeToolStripMenuItem";
            this.changeBoxTypeToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
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
            // enableBattleStatOverrideButton
            // 
            this.enableBattleStatOverrideButton.CheckOnClick = true;
            this.enableBattleStatOverrideButton.Name = "enableBattleStatOverrideButton";
            this.enableBattleStatOverrideButton.Size = new System.Drawing.Size(213, 22);
            this.enableBattleStatOverrideButton.Text = "Enable Battle Stat Override";
            this.enableBattleStatOverrideButton.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.enableBattleStatOverrideButton.ToolTipText = resources.GetString("enableBattleStatOverrideButton.ToolTipText");
            this.enableBattleStatOverrideButton.CheckedChanged += new System.EventHandler(this.enableBattleStatOverrideButton_CheckedChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openACollectionToolStripMenuItem,
            this.importAPKUToolStripMenuItem,
            this.boxOptionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(704, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 100;
            this.toolTip1.ShowAlways = true;
            // 
            // ManagerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(704, 344);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.viewCheckedoutButton);
            this.Controls.Add(this.LHSTabs);
            this.Controls.Add(this.boxSelector);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(720, 383);
            this.Name = "ManagerWindow";
            this.Text = "pku Manager";
            this.LHSTabs.ResumeLayout(false);
            this.Summary.ResumeLayout(false);
            this.Summary.PerformLayout();
            this.Export.ResumeLayout(false);
            this.Export.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
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
        private System.Windows.Forms.TabPage Summary;
        private System.Windows.Forms.TabPage Export;
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
        private System.Windows.Forms.ToolStripMenuItem openACollectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importAPKUToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boxOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBoxTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boxOptionsList;
        private System.Windows.Forms.ToolStripMenuItem boxOptions30;
        private System.Windows.Forms.ToolStripMenuItem boxOptions60;
        private System.Windows.Forms.ToolStripMenuItem boxOptions96;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem enableBattleStatOverrideButton;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

