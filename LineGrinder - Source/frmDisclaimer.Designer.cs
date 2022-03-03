namespace LineGrinder
{
    partial class frmDisclaimer
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
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.buttonDoNotAgree = new System.Windows.Forms.Button();
            this.richTextBoxLicense = new System.Windows.Forms.RichTextBox();
            this.buttonAgree = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTitle.Location = new System.Drawing.Point(81, 6);
            this.textBoxTitle.Multiline = true;
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.ReadOnly = true;
            this.textBoxTitle.Size = new System.Drawing.Size(309, 24);
            this.textBoxTitle.TabIndex = 5;
            this.textBoxTitle.Text = "Line Grinder";
            this.textBoxTitle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonDoNotAgree
            // 
            this.buttonDoNotAgree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDoNotAgree.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonDoNotAgree.Location = new System.Drawing.Point(66, 206);
            this.buttonDoNotAgree.Name = "buttonDoNotAgree";
            this.buttonDoNotAgree.Size = new System.Drawing.Size(136, 23);
            this.buttonDoNotAgree.TabIndex = 0;
            this.buttonDoNotAgree.Text = "I Do &Not Agree";
            this.buttonDoNotAgree.UseVisualStyleBackColor = true;
            this.buttonDoNotAgree.Click += new System.EventHandler(this.buttonDoNotAgree_Click);
            // 
            // richTextBoxLicense
            // 
            this.richTextBoxLicense.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxLicense.Location = new System.Drawing.Point(26, 36);
            this.richTextBoxLicense.Name = "richTextBoxLicense";
            this.richTextBoxLicense.ReadOnly = true;
            this.richTextBoxLicense.Size = new System.Drawing.Size(418, 159);
            this.richTextBoxLicense.TabIndex = 1;
            this.richTextBoxLicense.Text = "";
            // 
            // buttonAgree
            // 
            this.buttonAgree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAgree.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAgree.Location = new System.Drawing.Point(236, 206);
            this.buttonAgree.Name = "buttonAgree";
            this.buttonAgree.Size = new System.Drawing.Size(169, 23);
            this.buttonAgree.TabIndex = 6;
            this.buttonAgree.Text = "I Understand and &Agree";
            this.buttonAgree.UseVisualStyleBackColor = true;
            this.buttonAgree.Click += new System.EventHandler(this.buttonAgree_Click);
            // 
            // frmDisclaimer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonDoNotAgree;
            this.ClientSize = new System.Drawing.Size(471, 238);
            this.Controls.Add(this.buttonAgree);
            this.Controls.Add(this.richTextBoxLicense);
            this.Controls.Add(this.buttonDoNotAgree);
            this.Controls.Add(this.textBoxTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDisclaimer";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Line Grinder: Disclaimer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.Button buttonDoNotAgree;
        private System.Windows.Forms.RichTextBox richTextBoxLicense;
        private System.Windows.Forms.Button buttonAgree;
    }
}

