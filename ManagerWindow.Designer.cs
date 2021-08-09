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
            this.pkuImport = new System.Windows.Forms.Button();
            this.boxSelector = new System.Windows.Forms.ComboBox();
            this.boxOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBoxNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBoxSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.customToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBoxImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openBoxInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Summary = new System.Windows.Forms.TabPage();
            this.filenameLabel = new System.Windows.Forms.Label();
            this.filenameTextBox = new System.Windows.Forms.TextBox();
            this.gameLabel = new System.Windows.Forms.Label();
            this.gameTextBox = new System.Windows.Forms.TextBox();
            this.speciesTextBox = new System.Windows.Forms.TextBox();
            this.otTextBox = new System.Windows.Forms.TextBox();
            this.nicknameTextBox = new System.Windows.Forms.TextBox();
            this.speciesLabel = new System.Windows.Forms.Label();
            this.otLabel = new System.Windows.Forms.Label();
            this.nicknameLabel = new System.Windows.Forms.Label();
            this.Export = new System.Windows.Forms.TabPage();
            this.exportButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.refreshButton = new System.Windows.Forms.Button();
            this.pssPicker = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.openExplorerButton = new System.Windows.Forms.Button();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.boxDisplayDock = new System.Windows.Forms.Panel();
            this.checkoutDock = new System.Windows.Forms.Panel();
            this.viewCheckedoutButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.Summary.SuspendLayout();
            this.Export.SuspendLayout();
            this.SuspendLayout();
            // 
            // pkuImport
            // 
            this.pkuImport.Enabled = false;
            this.pkuImport.Location = new System.Drawing.Point(113, 12);
            this.pkuImport.Name = "pkuImport";
            this.pkuImport.Size = new System.Drawing.Size(75, 23);
            this.pkuImport.TabIndex = 0;
            this.pkuImport.Text = "Import .pku";
            this.pkuImport.UseVisualStyleBackColor = true;
            this.pkuImport.Click += new System.EventHandler(this.importPkuButton_Click);
            // 
            // boxSelector
            // 
            this.boxSelector.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.boxSelector.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.boxSelector.FormattingEnabled = true;
            this.boxSelector.Location = new System.Drawing.Point(271, 27);
            this.boxSelector.Name = "boxSelector";
            this.boxSelector.Size = new System.Drawing.Size(121, 21);
            this.boxSelector.TabIndex = 4;
            this.boxSelector.SelectedIndexChanged += new System.EventHandler(this.boxSelector_SelectedIndexChanged);
            // 
            // boxOptionsToolStripMenuItem
            // 
            this.boxOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeBoxNameToolStripMenuItem,
            this.changeBoxSizeToolStripMenuItem,
            this.changeBoxImageToolStripMenuItem,
            this.openBoxInExplorerToolStripMenuItem});
            this.boxOptionsToolStripMenuItem.Name = "boxOptionsToolStripMenuItem";
            this.boxOptionsToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.boxOptionsToolStripMenuItem.Text = "Box Options";
            // 
            // changeBoxNameToolStripMenuItem
            // 
            this.changeBoxNameToolStripMenuItem.Name = "changeBoxNameToolStripMenuItem";
            this.changeBoxNameToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
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
            this.changeBoxSizeToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.changeBoxSizeToolStripMenuItem.Text = "Change box size";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(116, 22);
            this.toolStripMenuItem2.Text = "30";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(116, 22);
            this.toolStripMenuItem3.Text = "60";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(116, 22);
            this.toolStripMenuItem4.Text = "90";
            // 
            // customToolStripMenuItem
            // 
            this.customToolStripMenuItem.Name = "customToolStripMenuItem";
            this.customToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.customToolStripMenuItem.Text = "Custom";
            // 
            // changeBoxImageToolStripMenuItem
            // 
            this.changeBoxImageToolStripMenuItem.Name = "changeBoxImageToolStripMenuItem";
            this.changeBoxImageToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.changeBoxImageToolStripMenuItem.Text = "Change box image";
            // 
            // openBoxInExplorerToolStripMenuItem
            // 
            this.openBoxInExplorerToolStripMenuItem.Name = "openBoxInExplorerToolStripMenuItem";
            this.openBoxInExplorerToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.openBoxInExplorerToolStripMenuItem.Text = "Open box in explorer";
            this.openBoxInExplorerToolStripMenuItem.Click += new System.EventHandler(this.openExplorerButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Summary);
            this.tabControl1.Controls.Add(this.Export);
            this.tabControl1.Location = new System.Drawing.Point(0, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(268, 311);
            this.tabControl1.TabIndex = 7;
            // 
            // Summary
            // 
            this.Summary.Controls.Add(this.filenameLabel);
            this.Summary.Controls.Add(this.filenameTextBox);
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
            this.Summary.Size = new System.Drawing.Size(260, 285);
            this.Summary.TabIndex = 0;
            this.Summary.Text = "Summary";
            this.Summary.UseVisualStyleBackColor = true;
            // 
            // filenameLabel
            // 
            this.filenameLabel.AutoSize = true;
            this.filenameLabel.Location = new System.Drawing.Point(13, 256);
            this.filenameLabel.Name = "filenameLabel";
            this.filenameLabel.Size = new System.Drawing.Size(55, 13);
            this.filenameLabel.TabIndex = 9;
            this.filenameLabel.Text = "Filename: ";
            // 
            // filenameTextBox
            // 
            this.filenameTextBox.Location = new System.Drawing.Point(84, 253);
            this.filenameTextBox.Name = "filenameTextBox";
            this.filenameTextBox.ReadOnly = true;
            this.filenameTextBox.Size = new System.Drawing.Size(100, 20);
            this.filenameTextBox.TabIndex = 8;
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
            this.Export.Controls.Add(this.exportButtons);
            this.Export.Location = new System.Drawing.Point(4, 22);
            this.Export.Name = "Export";
            this.Export.Padding = new System.Windows.Forms.Padding(3);
            this.Export.Size = new System.Drawing.Size(260, 285);
            this.Export.TabIndex = 1;
            this.Export.Text = "Export";
            this.Export.UseVisualStyleBackColor = true;
            // 
            // exportButtons
            // 
            this.exportButtons.Location = new System.Drawing.Point(0, 0);
            this.exportButtons.Name = "exportButtons";
            this.exportButtons.Size = new System.Drawing.Size(260, 261);
            this.exportButtons.TabIndex = 4;
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(407, 27);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(75, 21);
            this.refreshButton.TabIndex = 8;
            this.refreshButton.Text = "Refresh";
            this.toolTip1.SetToolTip(this.refreshButton, "Refreshes the list of boxes and their contents");
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // pssPicker
            // 
            this.pssPicker.Location = new System.Drawing.Point(190, 12);
            this.pssPicker.Name = "pssPicker";
            this.pssPicker.Size = new System.Drawing.Size(75, 23);
            this.pssPicker.TabIndex = 9;
            this.pssPicker.Text = "Pick a PSS";
            this.pssPicker.UseVisualStyleBackColor = true;
            this.pssPicker.Click += new System.EventHandler(this.setPSSDirectory_Click);
            // 
            // openExplorerButton
            // 
            this.openExplorerButton.Location = new System.Drawing.Point(488, 27);
            this.openExplorerButton.Name = "openExplorerButton";
            this.openExplorerButton.Size = new System.Drawing.Size(93, 21);
            this.openExplorerButton.TabIndex = 11;
            this.openExplorerButton.Text = "Open in Explorer";
            this.toolTip1.SetToolTip(this.openExplorerButton, "Opens the current box in file explorer");
            this.openExplorerButton.UseVisualStyleBackColor = true;
            this.openExplorerButton.Click += new System.EventHandler(this.openExplorerButton_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(12, 20);
            // 
            // boxDisplayDock
            // 
            this.boxDisplayDock.Location = new System.Drawing.Point(271, 54);
            this.boxDisplayDock.Name = "boxDisplayDock";
            this.boxDisplayDock.Size = new System.Drawing.Size(425, 284);
            this.boxDisplayDock.TabIndex = 10;
            // 
            // checkoutDock
            // 
            this.checkoutDock.Location = new System.Drawing.Point(711, 54);
            this.checkoutDock.Name = "checkoutDock";
            this.checkoutDock.Size = new System.Drawing.Size(311, 284);
            this.checkoutDock.TabIndex = 11;
            this.checkoutDock.Visible = false;
            // 
            // viewCheckedoutButton
            // 
            this.viewCheckedoutButton.Location = new System.Drawing.Point(587, 27);
            this.viewCheckedoutButton.Name = "viewCheckedoutButton";
            this.viewCheckedoutButton.Size = new System.Drawing.Size(92, 20);
            this.viewCheckedoutButton.TabIndex = 12;
            this.viewCheckedoutButton.Text = "View Check-Out";
            this.toolTip1.SetToolTip(this.viewCheckedoutButton, "Toggles the checked-out box");
            this.viewCheckedoutButton.UseVisualStyleBackColor = true;
            this.viewCheckedoutButton.Click += new System.EventHandler(this.viewCheckedoutButton_Click);
            // 
            // ManagerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1028, 344);
            this.Controls.Add(this.viewCheckedoutButton);
            this.Controls.Add(this.checkoutDock);
            this.Controls.Add(this.openExplorerButton);
            this.Controls.Add(this.boxDisplayDock);
            this.Controls.Add(this.pssPicker);
            this.Controls.Add(this.pkuImport);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.boxSelector);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(720, 383);
            this.Name = "ManagerWindow";
            this.Text = "pku Manager";
            this.tabControl1.ResumeLayout(false);
            this.Summary.ResumeLayout(false);
            this.Summary.PerformLayout();
            this.Export.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button pkuImport;
        private System.Windows.Forms.ComboBox boxSelector;
        private System.Windows.Forms.ToolStripMenuItem boxOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openBoxInExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBoxNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBoxSizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem customToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBoxImageToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Summary;
        private System.Windows.Forms.TabPage Export;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button pssPicker;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox speciesTextBox;
        private System.Windows.Forms.TextBox otTextBox;
        private System.Windows.Forms.TextBox nicknameTextBox;
        private System.Windows.Forms.Label speciesLabel;
        private System.Windows.Forms.Label otLabel;
        private System.Windows.Forms.Label nicknameLabel;
        private System.Windows.Forms.Label gameLabel;
        private System.Windows.Forms.TextBox gameTextBox;
        private System.Windows.Forms.Label filenameLabel;
        private System.Windows.Forms.TextBox filenameTextBox;
        private System.Windows.Forms.FlowLayoutPanel exportButtons;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.Panel boxDisplayDock;
        private System.Windows.Forms.Button openExplorerButton;
        private System.Windows.Forms.Panel checkoutDock;
        private System.Windows.Forms.Button viewCheckedoutButton;
    }
}

