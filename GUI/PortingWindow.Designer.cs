namespace pkuManager.GUI
{
    partial class PortingWindow
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
            this.descLabel = new System.Windows.Forms.Label();
            this.acceptButton = new System.Windows.Forms.Button();
            this.warningPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.errorPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.notesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // descLabel
            // 
            this.descLabel.AutoSize = true;
            this.descLabel.Location = new System.Drawing.Point(53, 23);
            this.descLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.descLabel.MaximumSize = new System.Drawing.Size(408, 0);
            this.descLabel.Name = "descLabel";
            this.descLabel.Size = new System.Drawing.Size(403, 30);
            this.descLabel.TabIndex = 5;
            this.descLabel.Text = "The following warnings and errors must be acknowledged before exporting to Format" +
    " (extension)";
            this.descLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // acceptButton
            // 
            this.acceptButton.Location = new System.Drawing.Point(396, 379);
            this.acceptButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.acceptButton.Name = "acceptButton";
            this.acceptButton.Size = new System.Drawing.Size(88, 27);
            this.acceptButton.TabIndex = 6;
            this.acceptButton.Text = "Accept";
            this.acceptButton.UseVisualStyleBackColor = true;
            // 
            // warningPanel
            // 
            this.warningPanel.AutoScroll = true;
            this.warningPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.warningPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.warningPanel.Location = new System.Drawing.Point(4, 19);
            this.warningPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.warningPanel.Name = "warningPanel";
            this.warningPanel.Size = new System.Drawing.Size(195, 247);
            this.warningPanel.TabIndex = 0;
            this.warningPanel.WrapContents = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.warningPanel);
            this.groupBox1.Location = new System.Drawing.Point(39, 69);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(204, 269);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Warnings";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.errorPanel);
            this.groupBox2.Location = new System.Drawing.Point(267, 69);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(204, 269);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Errors";
            // 
            // errorPanel
            // 
            this.errorPanel.AutoScroll = true;
            this.errorPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.errorPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.errorPanel.Location = new System.Drawing.Point(4, 19);
            this.errorPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.errorPanel.Name = "errorPanel";
            this.errorPanel.Size = new System.Drawing.Size(195, 247);
            this.errorPanel.TabIndex = 0;
            this.errorPanel.WrapContents = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.notesPanel);
            this.groupBox3.Location = new System.Drawing.Point(39, 360);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Size = new System.Drawing.Size(333, 139);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Notes";
            // 
            // notesPanel
            // 
            this.notesPanel.AutoScroll = true;
            this.notesPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.notesPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.notesPanel.Location = new System.Drawing.Point(4, 19);
            this.notesPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.notesPanel.Name = "notesPanel";
            this.notesPanel.Size = new System.Drawing.Size(325, 117);
            this.notesPanel.TabIndex = 0;
            this.notesPanel.WrapContents = false;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(396, 412);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(88, 27);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // PortingWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 528);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.acceptButton);
            this.Controls.Add(this.descLabel);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "PortingWindow";
            this.Text = "Export Warning (Format)";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label descLabel;
        private System.Windows.Forms.Button acceptButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        protected System.Windows.Forms.FlowLayoutPanel warningPanel;
        protected System.Windows.Forms.FlowLayoutPanel errorPanel;
        private System.Windows.Forms.GroupBox groupBox3;
        protected System.Windows.Forms.FlowLayoutPanel notesPanel;
        private System.Windows.Forms.Button cancelButton;
    }
}