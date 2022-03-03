namespace LineGrinder
{
    partial class frmFileManagerChooser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileManagerChooser));
            this.buttonFileOptIsoCut = new System.Windows.Forms.Button();
            this.buttonFileOptEdgeMill = new System.Windows.Forms.Button();
            this.buttonFileOptTextLabel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonFileOptExcellon = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonFileOptIsoCut
            // 
            this.buttonFileOptIsoCut.Location = new System.Drawing.Point(15, 88);
            this.buttonFileOptIsoCut.Name = "buttonFileOptIsoCut";
            this.buttonFileOptIsoCut.Size = new System.Drawing.Size(104, 23);
            this.buttonFileOptIsoCut.TabIndex = 0;
            this.buttonFileOptIsoCut.Text = "Isolation Cut";
            this.buttonFileOptIsoCut.UseVisualStyleBackColor = true;
            this.buttonFileOptIsoCut.Click += new System.EventHandler(this.buttonFileOptIsoCut_Click);
            // 
            // buttonFileOptEdgeMill
            // 
            this.buttonFileOptEdgeMill.Location = new System.Drawing.Point(15, 133);
            this.buttonFileOptEdgeMill.Name = "buttonFileOptEdgeMill";
            this.buttonFileOptEdgeMill.Size = new System.Drawing.Size(104, 23);
            this.buttonFileOptEdgeMill.TabIndex = 1;
            this.buttonFileOptEdgeMill.Text = "Board Edge Mill";
            this.buttonFileOptEdgeMill.UseVisualStyleBackColor = true;
            this.buttonFileOptEdgeMill.Click += new System.EventHandler(this.buttonFileOptEdgeMill_Click);
            // 
            // buttonFileOptTextLabel
            // 
            this.buttonFileOptTextLabel.Enabled = false;
            this.buttonFileOptTextLabel.Location = new System.Drawing.Point(396, 225);
            this.buttonFileOptTextLabel.Name = "buttonFileOptTextLabel";
            this.buttonFileOptTextLabel.Size = new System.Drawing.Size(44, 18);
            this.buttonFileOptTextLabel.TabIndex = 2;
            this.buttonFileOptTextLabel.Text = "Text and Labels";
            this.buttonFileOptTextLabel.UseVisualStyleBackColor = true;
            this.buttonFileOptTextLabel.Visible = false;
            this.buttonFileOptTextLabel.Click += new System.EventHandler(this.buttonFileOptTextLabel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(72, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(305, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Choose the Type of File Manager to Create";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(125, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(16, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "<-";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(125, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "<-";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Enabled = false;
            this.label4.Location = new System.Drawing.Point(318, 228);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "<-";
            this.label4.Visible = false;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(25, 30);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(399, 49);
            this.textBox1.TabIndex = 10;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancel.Location = new System.Drawing.Point(187, 225);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(125, 181);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(16, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "<-";
            // 
            // buttonFileOptExcellon
            // 
            this.buttonFileOptExcellon.Location = new System.Drawing.Point(15, 176);
            this.buttonFileOptExcellon.Name = "buttonFileOptExcellon";
            this.buttonFileOptExcellon.Size = new System.Drawing.Size(104, 23);
            this.buttonFileOptExcellon.TabIndex = 12;
            this.buttonFileOptExcellon.Text = "Excellon";
            this.buttonFileOptExcellon.UseVisualStyleBackColor = true;
            this.buttonFileOptExcellon.Click += new System.EventHandler(this.buttonFileOptExcellon_Click);
            // 
            // textBox2
            // 
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Location = new System.Drawing.Point(141, 88);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(263, 32);
            this.textBox2.TabIndex = 14;
            this.textBox2.Text = "Creates a File Manager suitable for defining Isolation Cut and Reference Pin GCod" +
    "e.";
            // 
            // textBox3
            // 
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.Location = new System.Drawing.Point(141, 133);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(263, 32);
            this.textBox3.TabIndex = 15;
            this.textBox3.Text = "Creates a File Manager suitable for generating Edge Milling and Bed Flattening GC" +
    "ode.";
            // 
            // textBox4
            // 
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Enabled = false;
            this.textBox4.Location = new System.Drawing.Point(350, 219);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(40, 25);
            this.textBox4.TabIndex = 16;
            this.textBox4.Text = "Creates a File Manager suitable for cutting text and silkscreen layers.";
            this.textBox4.Visible = false;
            // 
            // textBox5
            // 
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox5.Location = new System.Drawing.Point(141, 177);
            this.textBox5.Multiline = true;
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.Size = new System.Drawing.Size(263, 32);
            this.textBox5.TabIndex = 17;
            this.textBox5.Text = "Creates a File Manager suitable for defining drill patterns from Excellon files.";
            // 
            // frmFileManagerChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(449, 261);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.buttonFileOptExcellon);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonFileOptTextLabel);
            this.Controls.Add(this.buttonFileOptEdgeMill);
            this.Controls.Add(this.buttonFileOptIsoCut);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "frmFileManagerChooser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose the Type of File Manager To Create";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonFileOptIsoCut;
        private System.Windows.Forms.Button buttonFileOptEdgeMill;
        private System.Windows.Forms.Button buttonFileOptTextLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonFileOptExcellon;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
    }
}

