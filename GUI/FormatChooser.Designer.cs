namespace pkuManager.GUI
{
    partial class FormatChooser
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
            this.selectionPromptLabel = new System.Windows.Forms.Label();
            this.invalidFormatNoticeLabel = new System.Windows.Forms.Label();
            this.formatComboBox = new System.Windows.Forms.ComboBox();
            this.confirmButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // selectionPromptLabel
            // 
            this.selectionPromptLabel.AutoSize = true;
            this.selectionPromptLabel.Location = new System.Drawing.Point(83, 26);
            this.selectionPromptLabel.Name = "selectionPromptLabel";
            this.selectionPromptLabel.Size = new System.Drawing.Size(142, 15);
            this.selectionPromptLabel.TabIndex = 0;
            this.selectionPromptLabel.Text = "Select a format to port to:";
            // 
            // invalidFormatNoticeLabel
            // 
            this.invalidFormatNoticeLabel.AutoSize = true;
            this.invalidFormatNoticeLabel.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.invalidFormatNoticeLabel.Location = new System.Drawing.Point(8, 110);
            this.invalidFormatNoticeLabel.Name = "invalidFormatNoticeLabel";
            this.invalidFormatNoticeLabel.Size = new System.Drawing.Size(290, 12);
            this.invalidFormatNoticeLabel.TabIndex = 1;
            this.invalidFormatNoticeLabel.Text = "If a format does not appear, then the pku cannot be ported to it.";
            // 
            // formatComboBox
            // 
            this.formatComboBox.FormattingEnabled = true;
            this.formatComboBox.Location = new System.Drawing.Point(60, 53);
            this.formatComboBox.Name = "formatComboBox";
            this.formatComboBox.Size = new System.Drawing.Size(105, 23);
            this.formatComboBox.TabIndex = 2;
            // 
            // confirmButton
            // 
            this.confirmButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.confirmButton.Location = new System.Drawing.Point(171, 53);
            this.confirmButton.Name = "confirmButton";
            this.confirmButton.Size = new System.Drawing.Size(75, 23);
            this.confirmButton.TabIndex = 3;
            this.confirmButton.Text = "Confirm";
            this.confirmButton.UseVisualStyleBackColor = true;
            // 
            // FormatChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(314, 141);
            this.Controls.Add(this.confirmButton);
            this.Controls.Add(this.formatComboBox);
            this.Controls.Add(this.invalidFormatNoticeLabel);
            this.Controls.Add(this.selectionPromptLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormatChooser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose Format";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label selectionPromptLabel;
        private System.Windows.Forms.Label invalidFormatNoticeLabel;
        private System.Windows.Forms.ComboBox formatComboBox;
        private System.Windows.Forms.Button confirmButton;
    }
}