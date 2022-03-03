namespace LineGrinder
{
    partial class ctlFileManagersDisplay
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.buttonRemoveAll = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.labelFileManagers = new System.Windows.Forms.Label();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.listBoxFileManagers = new System.Windows.Forms.ListBox();
            this.labelFileManagerProperties = new System.Windows.Forms.Label();
            this.propertyGridFileManager = new System.Windows.Forms.PropertyGrid();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.MinimumSize = new System.Drawing.Size(350, 375);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.buttonRemoveAll);
            this.splitContainer1.Panel1.Controls.Add(this.buttonReset);
            this.splitContainer1.Panel1.Controls.Add(this.labelFileManagers);
            this.splitContainer1.Panel1.Controls.Add(this.buttonRemove);
            this.splitContainer1.Panel1.Controls.Add(this.buttonAdd);
            this.splitContainer1.Panel1.Controls.Add(this.listBoxFileManagers);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.labelFileManagerProperties);
            this.splitContainer1.Panel2.Controls.Add(this.propertyGridFileManager);
            this.splitContainer1.Size = new System.Drawing.Size(462, 434);
            this.splitContainer1.SplitterDistance = 232;
            this.splitContainer1.TabIndex = 0;
            // 
            // buttonRemoveAll
            // 
            this.buttonRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemoveAll.Location = new System.Drawing.Point(107, 412);
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size(78, 20);
            this.buttonRemoveAll.TabIndex = 10;
            this.buttonRemoveAll.Text = "Remove All...";
            this.buttonRemoveAll.UseVisualStyleBackColor = true;
            this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Location = new System.Drawing.Point(179, 412);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(52, 20);
            this.buttonReset.TabIndex = 9;
            this.buttonReset.Text = "Reset...";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // labelFileManagers
            // 
            this.labelFileManagers.AutoSize = true;
            this.labelFileManagers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFileManagers.ForeColor = System.Drawing.Color.DarkRed;
            this.labelFileManagers.Location = new System.Drawing.Point(3, 4);
            this.labelFileManagers.Name = "labelFileManagers";
            this.labelFileManagers.Size = new System.Drawing.Size(210, 16);
            this.labelFileManagers.TabIndex = 8;
            this.labelFileManagers.Text = "Gerber files with names like...";
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemove.Location = new System.Drawing.Point(42, 412);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(64, 20);
            this.buttonRemove.TabIndex = 2;
            this.buttonRemove.Text = "Remove...";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdd.Location = new System.Drawing.Point(1, 412);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(40, 20);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = "Add...";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // listBoxFileManagers
            // 
            this.listBoxFileManagers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxFileManagers.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxFileManagers.FormattingEnabled = true;
            this.listBoxFileManagers.ItemHeight = 16;
            this.listBoxFileManagers.Location = new System.Drawing.Point(0, 24);
            this.listBoxFileManagers.Name = "listBoxFileManagers";
            this.listBoxFileManagers.Size = new System.Drawing.Size(229, 372);
            this.listBoxFileManagers.TabIndex = 0;
            this.listBoxFileManagers.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // labelFileManagerProperties
            // 
            this.labelFileManagerProperties.AutoSize = true;
            this.labelFileManagerProperties.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFileManagerProperties.ForeColor = System.Drawing.Color.DarkRed;
            this.labelFileManagerProperties.Location = new System.Drawing.Point(7, 4);
            this.labelFileManagerProperties.Name = "labelFileManagerProperties";
            this.labelFileManagerProperties.Size = new System.Drawing.Size(146, 16);
            this.labelFileManagerProperties.TabIndex = 9;
            this.labelFileManagerProperties.Text = "... use these options";
            // 
            // propertyGridFileManager
            // 
            this.propertyGridFileManager.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridFileManager.Location = new System.Drawing.Point(3, 24);
            this.propertyGridFileManager.Name = "propertyGridFileManager";
            this.propertyGridFileManager.Size = new System.Drawing.Size(223, 407);
            this.propertyGridFileManager.TabIndex = 0;
            this.propertyGridFileManager.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridFileManager_PropertyValueChanged);
            // 
            // ctlFileManagersDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ctlFileManagersDisplay";
            this.Size = new System.Drawing.Size(462, 434);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox listBoxFileManagers;
        private System.Windows.Forms.PropertyGrid propertyGridFileManager;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Label labelFileManagerProperties;
        private System.Windows.Forms.Label labelFileManagers;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonRemoveAll;
    }
}

