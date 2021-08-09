namespace pkuManager
{
    partial class WarningWindow
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
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // descLabel
            // 
            this.descLabel.AutoSize = true;
            this.descLabel.Location = new System.Drawing.Point(68, 31);
            this.descLabel.MaximumSize = new System.Drawing.Size(350, 0);
            this.descLabel.Name = "descLabel";
            this.descLabel.Size = new System.Drawing.Size(314, 26);
            this.descLabel.TabIndex = 5;
            this.descLabel.Text = "The following warnings and errors must be acknowledged before exporting to Format" +
    " (extension)";
            // 
            // acceptButton
            // 
            this.acceptButton.Location = new System.Drawing.Point(362, 357);
            this.acceptButton.Name = "acceptButton";
            this.acceptButton.Size = new System.Drawing.Size(75, 23);
            this.acceptButton.TabIndex = 6;
            this.acceptButton.Text = "Accept";
            this.acceptButton.UseVisualStyleBackColor = true;
            // 
            // warningPanel
            // 
            this.warningPanel.AutoScroll = true;
            this.warningPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.warningPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.warningPanel.Location = new System.Drawing.Point(3, 16);
            this.warningPanel.Name = "warningPanel";
            this.warningPanel.Size = new System.Drawing.Size(197, 214);
            this.warningPanel.TabIndex = 0;
            this.warningPanel.WrapContents = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.warningPanel);
            this.groupBox1.Location = new System.Drawing.Point(12, 80);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(205, 233);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Warnings";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.errorPanel);
            this.groupBox2.Location = new System.Drawing.Point(232, 80);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(205, 233);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Errors";
            // 
            // errorPanel
            // 
            this.errorPanel.AutoScroll = true;
            this.errorPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.errorPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.errorPanel.Location = new System.Drawing.Point(3, 16);
            this.errorPanel.Name = "errorPanel";
            this.errorPanel.Size = new System.Drawing.Size(197, 214);
            this.errorPanel.TabIndex = 0;
            this.errorPanel.WrapContents = false;
            // 
            // WarningWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(463, 404);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.acceptButton);
            this.Controls.Add(this.descLabel);
            this.Controls.Add(this.groupBox1);
            this.Name = "WarningWindow";
            this.Text = "Export Warning (Format)";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
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
    }
}