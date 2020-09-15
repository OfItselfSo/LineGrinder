namespace LineGrinder
{
    partial class frmMain1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain1));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPagePlot = new System.Windows.Forms.TabPage();
            this.buttonGoToFileManager = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxActiveFileManager = new System.Windows.Forms.TextBox();
            this.buttonMagnification100 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxMagnification = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelOnAllPlots = new System.Windows.Forms.Label();
            this.checkBoxShowOrigin = new System.Windows.Forms.CheckBox();
            this.radioButtonNoPlot = new System.Windows.Forms.RadioButton();
            this.radioButtonMainViewDrillGCode = new System.Windows.Forms.RadioButton();
            this.radioButtonMainViewReferencePinsGCode = new System.Windows.Forms.RadioButton();
            this.radioButtonMainViewBedFlattenGCode = new System.Windows.Forms.RadioButton();
            this.checkBoxOnGCodePlotShowGerber = new System.Windows.Forms.CheckBox();
            this.labelOnGCodePlots = new System.Windows.Forms.Label();
            this.checkBoxShowGerberApertures = new System.Windows.Forms.CheckBox();
            this.labelOnGerberPlots = new System.Windows.Forms.Label();
            this.checkBoxShowGerberCenterLines = new System.Windows.Forms.CheckBox();
            this.radioButtonIsoGCodePlot = new System.Windows.Forms.RadioButton();
            this.radioButtonIsoPlotStep3 = new System.Windows.Forms.RadioButton();
            this.radioButtonIsoPlotStep2 = new System.Windows.Forms.RadioButton();
            this.radioButtonMainViewEdgeMillGCode = new System.Windows.Forms.RadioButton();
            this.radioButtonIsoPlotStep1 = new System.Windows.Forms.RadioButton();
            this.radioButtonMainViewGerberPlot = new System.Windows.Forms.RadioButton();
            this.ctlPlotViewer1 = new LineGrinder.ctlPlotViewer();
            this.tabPageGerberCode = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxOpenGerberFileName = new System.Windows.Forms.TextBox();
            this.richTextBoxGerberCode = new System.Windows.Forms.RichTextBox();
            this.tabPageExcellonFile = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxOpenExcellonFileName = new System.Windows.Forms.TextBox();
            this.richTextBoxExcellonCode = new System.Windows.Forms.RichTextBox();
            this.tabPageIsolationGCode = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxIsolationGCodeFileName = new System.Windows.Forms.TextBox();
            this.richTextBoxIsolationGCode = new System.Windows.Forms.RichTextBox();
            this.tabPageEdgeMillGCode = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxEdgeMillGCodeFileName = new System.Windows.Forms.TextBox();
            this.richTextBoxEdgeMillGCode = new System.Windows.Forms.RichTextBox();
            this.tabPageBedFlatteningGCode = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxBedFlatteningGCodeFileName = new System.Windows.Forms.TextBox();
            this.richTextBoxBedFlatteningGCode = new System.Windows.Forms.RichTextBox();
            this.tabPageRefPinGCode = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.textBoxRefPinGCodeFileName = new System.Windows.Forms.TextBox();
            this.richTextBoxRefPinGCode = new System.Windows.Forms.RichTextBox();
            this.tabPageDrillGCode = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxDrillGCodeFileName = new System.Windows.Forms.TextBox();
            this.richTextBoxDrillGCode = new System.Windows.Forms.RichTextBox();
            this.tabPageSettings = new System.Windows.Forms.TabPage();
            this.buttonDefaultIsoPtsPerAppUnit = new System.Windows.Forms.Button();
            this.buttonViewLogfile = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonQuickSetupKiCad = new System.Windows.Forms.Button();
            this.buttonQuickSetupEagle = new System.Windows.Forms.Button();
            this.buttonQuickSetupDesignSpark = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxFileManagerTitle = new System.Windows.Forms.TextBox();
            this.textBoxFileManagerHeader = new System.Windows.Forms.TextBox();
            this.ctlFileManagersDisplay1 = new LineGrinder.ctlFileManagersDisplay();
            this.buttonSaveConfiguration = new System.Windows.Forms.Button();
            this.labelIsoPlotPointsPerAppUnit = new System.Windows.Forms.Label();
            this.textBoxIsoPlotPointsPerAppUnit = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxApplicationUnits = new System.Windows.Forms.ComboBox();
            this.buttonOpenFile = new System.Windows.Forms.Button();
            this.buttonRecentFiles = new System.Windows.Forms.Button();
            this.buttonConvertToGCode = new System.Windows.Forms.Button();
            this.buttonSaveIsolationGCode = new System.Windows.Forms.Button();
            this.buttonSaveIsolationGCodeAs = new System.Windows.Forms.Button();
            this.buttonClearAll = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.buttonAbout = new System.Windows.Forms.Button();
            this.textBoxStatusLine = new System.Windows.Forms.TextBox();
            this.buttonSaveEdgeMillGCodeAs = new System.Windows.Forms.Button();
            this.buttonSaveEdgeMillGCode = new System.Windows.Forms.Button();
            this.buttonSaveBedFlatteningGCodeAs = new System.Windows.Forms.Button();
            this.buttonSaveBedFlatteningGCode = new System.Windows.Forms.Button();
            this.buttonSaveRefPinGCodeAs = new System.Windows.Forms.Button();
            this.buttonSaveRefPinGCode = new System.Windows.Forms.Button();
            this.buttonSaveDrillGCode = new System.Windows.Forms.Button();
            this.buttonSaveDrillGCodeAs = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPagePlot.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPageGerberCode.SuspendLayout();
            this.tabPageExcellonFile.SuspendLayout();
            this.tabPageIsolationGCode.SuspendLayout();
            this.tabPageEdgeMillGCode.SuspendLayout();
            this.tabPageBedFlatteningGCode.SuspendLayout();
            this.tabPageRefPinGCode.SuspendLayout();
            this.tabPageDrillGCode.SuspendLayout();
            this.tabPageSettings.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPagePlot);
            this.tabControl1.Controls.Add(this.tabPageGerberCode);
            this.tabControl1.Controls.Add(this.tabPageExcellonFile);
            this.tabControl1.Controls.Add(this.tabPageIsolationGCode);
            this.tabControl1.Controls.Add(this.tabPageEdgeMillGCode);
            this.tabControl1.Controls.Add(this.tabPageBedFlatteningGCode);
            this.tabControl1.Controls.Add(this.tabPageRefPinGCode);
            this.tabControl1.Controls.Add(this.tabPageDrillGCode);
            this.tabControl1.Controls.Add(this.tabPageSettings);
            this.tabControl1.Location = new System.Drawing.Point(3, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(745, 536);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPagePlot
            // 
            this.tabPagePlot.Controls.Add(this.buttonGoToFileManager);
            this.tabPagePlot.Controls.Add(this.label5);
            this.tabPagePlot.Controls.Add(this.textBoxActiveFileManager);
            this.tabPagePlot.Controls.Add(this.buttonMagnification100);
            this.tabPagePlot.Controls.Add(this.label4);
            this.tabPagePlot.Controls.Add(this.comboBoxMagnification);
            this.tabPagePlot.Controls.Add(this.groupBox1);
            this.tabPagePlot.Controls.Add(this.ctlPlotViewer1);
            this.tabPagePlot.Location = new System.Drawing.Point(4, 22);
            this.tabPagePlot.Name = "tabPagePlot";
            this.tabPagePlot.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePlot.Size = new System.Drawing.Size(737, 510);
            this.tabPagePlot.TabIndex = 0;
            this.tabPagePlot.Text = "Plot View";
            this.tabPagePlot.ToolTipText = "A view of the Gerber, GCode or intermediate conversion stages.";
            this.tabPagePlot.UseVisualStyleBackColor = true;
            // 
            // buttonGoToFileManager
            // 
            this.buttonGoToFileManager.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonGoToFileManager.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGoToFileManager.Location = new System.Drawing.Point(93, 456);
            this.buttonGoToFileManager.Name = "buttonGoToFileManager";
            this.buttonGoToFileManager.Size = new System.Drawing.Size(37, 20);
            this.buttonGoToFileManager.TabIndex = 23;
            this.buttonGoToFileManager.Text = "Go";
            this.buttonGoToFileManager.UseVisualStyleBackColor = true;
            this.buttonGoToFileManager.Click += new System.EventHandler(this.buttonGoToFileManager_Click);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 461);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "File Manager";
            // 
            // textBoxActiveFileManager
            // 
            this.textBoxActiveFileManager.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxActiveFileManager.Location = new System.Drawing.Point(3, 477);
            this.textBoxActiveFileManager.Name = "textBoxActiveFileManager";
            this.textBoxActiveFileManager.ReadOnly = true;
            this.textBoxActiveFileManager.Size = new System.Drawing.Size(127, 20);
            this.textBoxActiveFileManager.TabIndex = 21;
            // 
            // buttonMagnification100
            // 
            this.buttonMagnification100.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonMagnification100.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonMagnification100.Location = new System.Drawing.Point(93, 408);
            this.buttonMagnification100.Name = "buttonMagnification100";
            this.buttonMagnification100.Size = new System.Drawing.Size(37, 20);
            this.buttonMagnification100.TabIndex = 20;
            this.buttonMagnification100.Text = "100%";
            this.buttonMagnification100.UseVisualStyleBackColor = true;
            this.buttonMagnification100.Click += new System.EventHandler(this.buttonMagnification100_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 414);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Magnification";
            // 
            // comboBoxMagnification
            // 
            this.comboBoxMagnification.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxMagnification.FormattingEnabled = true;
            this.comboBoxMagnification.Location = new System.Drawing.Point(3, 430);
            this.comboBoxMagnification.Name = "comboBoxMagnification";
            this.comboBoxMagnification.Size = new System.Drawing.Size(127, 21);
            this.comboBoxMagnification.TabIndex = 18;
            this.comboBoxMagnification.SelectionChangeCommitted += new System.EventHandler(this.comboBoxMagnification_SelectionChangeCommitted);
            this.comboBoxMagnification.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxMagnification_KeyDown);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelOnAllPlots);
            this.groupBox1.Controls.Add(this.checkBoxShowOrigin);
            this.groupBox1.Controls.Add(this.radioButtonNoPlot);
            this.groupBox1.Controls.Add(this.radioButtonMainViewDrillGCode);
            this.groupBox1.Controls.Add(this.radioButtonMainViewReferencePinsGCode);
            this.groupBox1.Controls.Add(this.radioButtonMainViewBedFlattenGCode);
            this.groupBox1.Controls.Add(this.checkBoxOnGCodePlotShowGerber);
            this.groupBox1.Controls.Add(this.labelOnGCodePlots);
            this.groupBox1.Controls.Add(this.checkBoxShowGerberApertures);
            this.groupBox1.Controls.Add(this.labelOnGerberPlots);
            this.groupBox1.Controls.Add(this.checkBoxShowGerberCenterLines);
            this.groupBox1.Controls.Add(this.radioButtonIsoGCodePlot);
            this.groupBox1.Controls.Add(this.radioButtonIsoPlotStep3);
            this.groupBox1.Controls.Add(this.radioButtonIsoPlotStep2);
            this.groupBox1.Controls.Add(this.radioButtonMainViewEdgeMillGCode);
            this.groupBox1.Controls.Add(this.radioButtonIsoPlotStep1);
            this.groupBox1.Controls.Add(this.radioButtonMainViewGerberPlot);
            this.groupBox1.Location = new System.Drawing.Point(3, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(127, 353);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plot View Shows ...";
            // 
            // labelOnAllPlots
            // 
            this.labelOnAllPlots.AutoSize = true;
            this.labelOnAllPlots.Location = new System.Drawing.Point(15, 202);
            this.labelOnAllPlots.Name = "labelOnAllPlots";
            this.labelOnAllPlots.Size = new System.Drawing.Size(69, 13);
            this.labelOnAllPlots.TabIndex = 18;
            this.labelOnAllPlots.Text = "On All plots...";
            // 
            // checkBoxShowOrigin
            // 
            this.checkBoxShowOrigin.AutoSize = true;
            this.checkBoxShowOrigin.Location = new System.Drawing.Point(9, 218);
            this.checkBoxShowOrigin.Name = "checkBoxShowOrigin";
            this.checkBoxShowOrigin.Size = new System.Drawing.Size(107, 17);
            this.checkBoxShowOrigin.TabIndex = 17;
            this.checkBoxShowOrigin.Text = "Show (0,0) Origin";
            this.checkBoxShowOrigin.UseVisualStyleBackColor = true;
            this.checkBoxShowOrigin.CheckedChanged += new System.EventHandler(this.checkBoxShowOrigin_CheckedChanged);
            // 
            // radioButtonNoPlot
            // 
            this.radioButtonNoPlot.AutoSize = true;
            this.radioButtonNoPlot.Checked = true;
            this.radioButtonNoPlot.Location = new System.Drawing.Point(9, 21);
            this.radioButtonNoPlot.Name = "radioButtonNoPlot";
            this.radioButtonNoPlot.Size = new System.Drawing.Size(60, 17);
            this.radioButtonNoPlot.TabIndex = 16;
            this.radioButtonNoPlot.TabStop = true;
            this.radioButtonNoPlot.Text = "No Plot";
            this.radioButtonNoPlot.UseVisualStyleBackColor = true;
            this.radioButtonNoPlot.CheckedChanged += new System.EventHandler(this.radioButtonNoPlot_CheckedChanged);
            // 
            // radioButtonMainViewDrillGCode
            // 
            this.radioButtonMainViewDrillGCode.AutoSize = true;
            this.radioButtonMainViewDrillGCode.Location = new System.Drawing.Point(9, 174);
            this.radioButtonMainViewDrillGCode.Name = "radioButtonMainViewDrillGCode";
            this.radioButtonMainViewDrillGCode.Size = new System.Drawing.Size(78, 17);
            this.radioButtonMainViewDrillGCode.TabIndex = 15;
            this.radioButtonMainViewDrillGCode.Text = "Drill GCode";
            this.radioButtonMainViewDrillGCode.UseVisualStyleBackColor = true;
            this.radioButtonMainViewDrillGCode.CheckedChanged += new System.EventHandler(this.radioButtonMainViewDrillGCode_CheckedChanged);
            // 
            // radioButtonMainViewReferencePinsGCode
            // 
            this.radioButtonMainViewReferencePinsGCode.AutoSize = true;
            this.radioButtonMainViewReferencePinsGCode.Location = new System.Drawing.Point(9, 157);
            this.radioButtonMainViewReferencePinsGCode.Name = "radioButtonMainViewReferencePinsGCode";
            this.radioButtonMainViewReferencePinsGCode.Size = new System.Drawing.Size(101, 17);
            this.radioButtonMainViewReferencePinsGCode.TabIndex = 14;
            this.radioButtonMainViewReferencePinsGCode.Text = "Ref Pins GCode";
            this.radioButtonMainViewReferencePinsGCode.UseVisualStyleBackColor = true;
            this.radioButtonMainViewReferencePinsGCode.CheckedChanged += new System.EventHandler(this.radioButtonMainViewReferencePinsGCode_CheckedChanged);
            // 
            // radioButtonMainViewBedFlattenGCode
            // 
            this.radioButtonMainViewBedFlattenGCode.AutoSize = true;
            this.radioButtonMainViewBedFlattenGCode.Location = new System.Drawing.Point(9, 140);
            this.radioButtonMainViewBedFlattenGCode.Name = "radioButtonMainViewBedFlattenGCode";
            this.radioButtonMainViewBedFlattenGCode.Size = new System.Drawing.Size(115, 17);
            this.radioButtonMainViewBedFlattenGCode.TabIndex = 13;
            this.radioButtonMainViewBedFlattenGCode.Text = "Bed Flatten GCode";
            this.radioButtonMainViewBedFlattenGCode.UseVisualStyleBackColor = true;
            this.radioButtonMainViewBedFlattenGCode.CheckedChanged += new System.EventHandler(this.radioButtonMainViewBedFlattenGCode_CheckedChanged);
            // 
            // checkBoxOnGCodePlotShowGerber
            // 
            this.checkBoxOnGCodePlotShowGerber.AutoSize = true;
            this.checkBoxOnGCodePlotShowGerber.Location = new System.Drawing.Point(9, 323);
            this.checkBoxOnGCodePlotShowGerber.Name = "checkBoxOnGCodePlotShowGerber";
            this.checkBoxOnGCodePlotShowGerber.Size = new System.Drawing.Size(109, 17);
            this.checkBoxOnGCodePlotShowGerber.TabIndex = 12;
            this.checkBoxOnGCodePlotShowGerber.Text = "Show Gerber Plot";
            this.checkBoxOnGCodePlotShowGerber.UseVisualStyleBackColor = true;
            this.checkBoxOnGCodePlotShowGerber.CheckedChanged += new System.EventHandler(this.checkBoxOnGCodePlotShowGerber_CheckedChanged);
            // 
            // labelOnGCodePlots
            // 
            this.labelOnGCodePlots.AutoSize = true;
            this.labelOnGCodePlots.Location = new System.Drawing.Point(15, 308);
            this.labelOnGCodePlots.Name = "labelOnGCodePlots";
            this.labelOnGCodePlots.Size = new System.Drawing.Size(92, 13);
            this.labelOnGCodePlots.TabIndex = 11;
            this.labelOnGCodePlots.Text = "On GCode Plots...";
            // 
            // checkBoxShowGerberApertures
            // 
            this.checkBoxShowGerberApertures.AutoSize = true;
            this.checkBoxShowGerberApertures.Location = new System.Drawing.Point(9, 279);
            this.checkBoxShowGerberApertures.Name = "checkBoxShowGerberApertures";
            this.checkBoxShowGerberApertures.Size = new System.Drawing.Size(101, 17);
            this.checkBoxShowGerberApertures.TabIndex = 8;
            this.checkBoxShowGerberApertures.Text = "Show Apertures";
            this.checkBoxShowGerberApertures.UseVisualStyleBackColor = true;
            this.checkBoxShowGerberApertures.CheckedChanged += new System.EventHandler(this.checkBoxShowGerberApertures_CheckedChanged);
            // 
            // labelOnGerberPlots
            // 
            this.labelOnGerberPlots.AutoSize = true;
            this.labelOnGerberPlots.Location = new System.Drawing.Point(15, 245);
            this.labelOnGerberPlots.Name = "labelOnGerberPlots";
            this.labelOnGerberPlots.Size = new System.Drawing.Size(90, 13);
            this.labelOnGerberPlots.TabIndex = 7;
            this.labelOnGerberPlots.Text = "On Gerber plots...";
            // 
            // checkBoxShowGerberCenterLines
            // 
            this.checkBoxShowGerberCenterLines.AutoSize = true;
            this.checkBoxShowGerberCenterLines.Location = new System.Drawing.Point(9, 261);
            this.checkBoxShowGerberCenterLines.Name = "checkBoxShowGerberCenterLines";
            this.checkBoxShowGerberCenterLines.Size = new System.Drawing.Size(115, 17);
            this.checkBoxShowGerberCenterLines.TabIndex = 6;
            this.checkBoxShowGerberCenterLines.Text = "Show Center Lines";
            this.checkBoxShowGerberCenterLines.UseVisualStyleBackColor = true;
            this.checkBoxShowGerberCenterLines.CheckedChanged += new System.EventHandler(this.checkBoxShowGerberCenterLines_CheckedChanged);
            // 
            // radioButtonIsoGCodePlot
            // 
            this.radioButtonIsoGCodePlot.AutoSize = true;
            this.radioButtonIsoGCodePlot.Location = new System.Drawing.Point(9, 106);
            this.radioButtonIsoGCodePlot.Name = "radioButtonIsoGCodePlot";
            this.radioButtonIsoGCodePlot.Size = new System.Drawing.Size(100, 17);
            this.radioButtonIsoGCodePlot.TabIndex = 5;
            this.radioButtonIsoGCodePlot.Text = "Isolation GCode";
            this.radioButtonIsoGCodePlot.UseVisualStyleBackColor = true;
            this.radioButtonIsoGCodePlot.CheckedChanged += new System.EventHandler(this.radioButtonIsoGCodePlot_CheckedChanged);
            // 
            // radioButtonIsoPlotStep3
            // 
            this.radioButtonIsoPlotStep3.AutoSize = true;
            this.radioButtonIsoPlotStep3.Location = new System.Drawing.Point(9, 89);
            this.radioButtonIsoPlotStep3.Name = "radioButtonIsoPlotStep3";
            this.radioButtonIsoPlotStep3.Size = new System.Drawing.Size(112, 17);
            this.radioButtonIsoPlotStep3.TabIndex = 4;
            this.radioButtonIsoPlotStep3.Text = "Tmp IsoPlot Step3";
            this.radioButtonIsoPlotStep3.UseVisualStyleBackColor = true;
            this.radioButtonIsoPlotStep3.CheckedChanged += new System.EventHandler(this.radioButtonIsoPlotStep3_CheckedChanged);
            // 
            // radioButtonIsoPlotStep2
            // 
            this.radioButtonIsoPlotStep2.AutoSize = true;
            this.radioButtonIsoPlotStep2.Location = new System.Drawing.Point(9, 72);
            this.radioButtonIsoPlotStep2.Name = "radioButtonIsoPlotStep2";
            this.radioButtonIsoPlotStep2.Size = new System.Drawing.Size(112, 17);
            this.radioButtonIsoPlotStep2.TabIndex = 3;
            this.radioButtonIsoPlotStep2.Text = "Tmp IsoPlot Step2";
            this.radioButtonIsoPlotStep2.UseVisualStyleBackColor = true;
            this.radioButtonIsoPlotStep2.CheckedChanged += new System.EventHandler(this.radioButtonIsoPlotStep2_CheckedChanged);
            // 
            // radioButtonMainViewEdgeMillGCode
            // 
            this.radioButtonMainViewEdgeMillGCode.AutoSize = true;
            this.radioButtonMainViewEdgeMillGCode.Location = new System.Drawing.Point(9, 123);
            this.radioButtonMainViewEdgeMillGCode.Name = "radioButtonMainViewEdgeMillGCode";
            this.radioButtonMainViewEdgeMillGCode.Size = new System.Drawing.Size(104, 17);
            this.radioButtonMainViewEdgeMillGCode.TabIndex = 2;
            this.radioButtonMainViewEdgeMillGCode.Text = "Edge Mill GCode";
            this.radioButtonMainViewEdgeMillGCode.UseVisualStyleBackColor = true;
            this.radioButtonMainViewEdgeMillGCode.CheckedChanged += new System.EventHandler(this.radioButtonMainViewEdgeMillGCode_CheckedChanged);
            // 
            // radioButtonIsoPlotStep1
            // 
            this.radioButtonIsoPlotStep1.AutoSize = true;
            this.radioButtonIsoPlotStep1.Location = new System.Drawing.Point(9, 55);
            this.radioButtonIsoPlotStep1.Name = "radioButtonIsoPlotStep1";
            this.radioButtonIsoPlotStep1.Size = new System.Drawing.Size(112, 17);
            this.radioButtonIsoPlotStep1.TabIndex = 1;
            this.radioButtonIsoPlotStep1.Text = "Tmp IsoPlot Step1";
            this.radioButtonIsoPlotStep1.UseVisualStyleBackColor = true;
            this.radioButtonIsoPlotStep1.CheckedChanged += new System.EventHandler(this.radioButtonIsoPlotStep1_CheckedChanged);
            // 
            // radioButtonMainViewGerberPlot
            // 
            this.radioButtonMainViewGerberPlot.AutoSize = true;
            this.radioButtonMainViewGerberPlot.Location = new System.Drawing.Point(9, 38);
            this.radioButtonMainViewGerberPlot.Name = "radioButtonMainViewGerberPlot";
            this.radioButtonMainViewGerberPlot.Size = new System.Drawing.Size(78, 17);
            this.radioButtonMainViewGerberPlot.TabIndex = 0;
            this.radioButtonMainViewGerberPlot.Text = "Gerber Plot";
            this.radioButtonMainViewGerberPlot.UseVisualStyleBackColor = true;
            this.radioButtonMainViewGerberPlot.CheckedChanged += new System.EventHandler(this.radioButtonMainViewGerberPlot_CheckedChanged);
            // 
            // ctlPlotViewer1
            // 
            this.ctlPlotViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ctlPlotViewer1.ApplicationUnits = LineGrinder.ApplicationUnitsEnum.INCHES;
            this.ctlPlotViewer1.Location = new System.Drawing.Point(133, 2);
            this.ctlPlotViewer1.MagnificationLevel = 1F;
            this.ctlPlotViewer1.Name = "ctlPlotViewer1";
            this.ctlPlotViewer1.ShowGerberOnGCode = false;
            this.ctlPlotViewer1.ShowOrigin = false;
            this.ctlPlotViewer1.Size = new System.Drawing.Size(603, 505);
            this.ctlPlotViewer1.TabIndex = 2;
            // 
            // tabPageGerberCode
            // 
            this.tabPageGerberCode.Controls.Add(this.label1);
            this.tabPageGerberCode.Controls.Add(this.textBoxOpenGerberFileName);
            this.tabPageGerberCode.Controls.Add(this.richTextBoxGerberCode);
            this.tabPageGerberCode.Location = new System.Drawing.Point(4, 22);
            this.tabPageGerberCode.Name = "tabPageGerberCode";
            this.tabPageGerberCode.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGerberCode.Size = new System.Drawing.Size(737, 510);
            this.tabPageGerberCode.TabIndex = 2;
            this.tabPageGerberCode.Text = "Gerber File";
            this.tabPageGerberCode.ToolTipText = "The Gerber Code for Conversion";
            this.tabPageGerberCode.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "File:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxOpenGerberFileName
            // 
            this.textBoxOpenGerberFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOpenGerberFileName.Location = new System.Drawing.Point(49, 6);
            this.textBoxOpenGerberFileName.Name = "textBoxOpenGerberFileName";
            this.textBoxOpenGerberFileName.ReadOnly = true;
            this.textBoxOpenGerberFileName.Size = new System.Drawing.Size(594, 20);
            this.textBoxOpenGerberFileName.TabIndex = 1;
            // 
            // richTextBoxGerberCode
            // 
            this.richTextBoxGerberCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxGerberCode.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxGerberCode.Location = new System.Drawing.Point(0, 31);
            this.richTextBoxGerberCode.Name = "richTextBoxGerberCode";
            this.richTextBoxGerberCode.ReadOnly = true;
            this.richTextBoxGerberCode.Size = new System.Drawing.Size(645, 488);
            this.richTextBoxGerberCode.TabIndex = 0;
            this.richTextBoxGerberCode.Text = "";
            this.richTextBoxGerberCode.WordWrap = false;
            // 
            // tabPageExcellonFile
            // 
            this.tabPageExcellonFile.Controls.Add(this.label3);
            this.tabPageExcellonFile.Controls.Add(this.textBoxOpenExcellonFileName);
            this.tabPageExcellonFile.Controls.Add(this.richTextBoxExcellonCode);
            this.tabPageExcellonFile.Location = new System.Drawing.Point(4, 22);
            this.tabPageExcellonFile.Name = "tabPageExcellonFile";
            this.tabPageExcellonFile.Size = new System.Drawing.Size(737, 510);
            this.tabPageExcellonFile.TabIndex = 7;
            this.tabPageExcellonFile.Text = "Excellon File";
            this.tabPageExcellonFile.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "File:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxOpenExcellonFileName
            // 
            this.textBoxOpenExcellonFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOpenExcellonFileName.Location = new System.Drawing.Point(49, 4);
            this.textBoxOpenExcellonFileName.Name = "textBoxOpenExcellonFileName";
            this.textBoxOpenExcellonFileName.ReadOnly = true;
            this.textBoxOpenExcellonFileName.Size = new System.Drawing.Size(638, 20);
            this.textBoxOpenExcellonFileName.TabIndex = 3;
            // 
            // richTextBoxExcellonCode
            // 
            this.richTextBoxExcellonCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxExcellonCode.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxExcellonCode.Location = new System.Drawing.Point(0, 29);
            this.richTextBoxExcellonCode.Name = "richTextBoxExcellonCode";
            this.richTextBoxExcellonCode.ReadOnly = true;
            this.richTextBoxExcellonCode.Size = new System.Drawing.Size(690, 477);
            this.richTextBoxExcellonCode.TabIndex = 2;
            this.richTextBoxExcellonCode.Text = "";
            this.richTextBoxExcellonCode.WordWrap = false;
            // 
            // tabPageIsolationGCode
            // 
            this.tabPageIsolationGCode.Controls.Add(this.label7);
            this.tabPageIsolationGCode.Controls.Add(this.textBoxIsolationGCodeFileName);
            this.tabPageIsolationGCode.Controls.Add(this.richTextBoxIsolationGCode);
            this.tabPageIsolationGCode.Location = new System.Drawing.Point(4, 22);
            this.tabPageIsolationGCode.Name = "tabPageIsolationGCode";
            this.tabPageIsolationGCode.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageIsolationGCode.Size = new System.Drawing.Size(737, 510);
            this.tabPageIsolationGCode.TabIndex = 3;
            this.tabPageIsolationGCode.Text = "Isolation GCode";
            this.tabPageIsolationGCode.ToolTipText = "The Isolation GCode generated from the supplied Gerber file.";
            this.tabPageIsolationGCode.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(17, 10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(26, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "File:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxIsolationGCodeFileName
            // 
            this.textBoxIsolationGCodeFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIsolationGCodeFileName.Location = new System.Drawing.Point(49, 7);
            this.textBoxIsolationGCodeFileName.Name = "textBoxIsolationGCodeFileName";
            this.textBoxIsolationGCodeFileName.ReadOnly = true;
            this.textBoxIsolationGCodeFileName.Size = new System.Drawing.Size(639, 20);
            this.textBoxIsolationGCodeFileName.TabIndex = 3;
            // 
            // richTextBoxIsolationGCode
            // 
            this.richTextBoxIsolationGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxIsolationGCode.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxIsolationGCode.Location = new System.Drawing.Point(0, 33);
            this.richTextBoxIsolationGCode.Name = "richTextBoxIsolationGCode";
            this.richTextBoxIsolationGCode.ReadOnly = true;
            this.richTextBoxIsolationGCode.Size = new System.Drawing.Size(689, 473);
            this.richTextBoxIsolationGCode.TabIndex = 1;
            this.richTextBoxIsolationGCode.Text = "";
            this.richTextBoxIsolationGCode.WordWrap = false;
            // 
            // tabPageEdgeMillGCode
            // 
            this.tabPageEdgeMillGCode.Controls.Add(this.label8);
            this.tabPageEdgeMillGCode.Controls.Add(this.textBoxEdgeMillGCodeFileName);
            this.tabPageEdgeMillGCode.Controls.Add(this.richTextBoxEdgeMillGCode);
            this.tabPageEdgeMillGCode.Location = new System.Drawing.Point(4, 22);
            this.tabPageEdgeMillGCode.Name = "tabPageEdgeMillGCode";
            this.tabPageEdgeMillGCode.Size = new System.Drawing.Size(737, 510);
            this.tabPageEdgeMillGCode.TabIndex = 4;
            this.tabPageEdgeMillGCode.Text = "Edge Mill GCode";
            this.tabPageEdgeMillGCode.ToolTipText = "GCode which can cut out the edges of the PCB from a larger sheet.";
            this.tabPageEdgeMillGCode.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(17, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(26, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "File:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxEdgeMillGCodeFileName
            // 
            this.textBoxEdgeMillGCodeFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxEdgeMillGCodeFileName.Location = new System.Drawing.Point(49, 6);
            this.textBoxEdgeMillGCodeFileName.Name = "textBoxEdgeMillGCodeFileName";
            this.textBoxEdgeMillGCodeFileName.ReadOnly = true;
            this.textBoxEdgeMillGCodeFileName.Size = new System.Drawing.Size(597, 20);
            this.textBoxEdgeMillGCodeFileName.TabIndex = 6;
            // 
            // richTextBoxEdgeMillGCode
            // 
            this.richTextBoxEdgeMillGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxEdgeMillGCode.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxEdgeMillGCode.Location = new System.Drawing.Point(0, 32);
            this.richTextBoxEdgeMillGCode.Name = "richTextBoxEdgeMillGCode";
            this.richTextBoxEdgeMillGCode.ReadOnly = true;
            this.richTextBoxEdgeMillGCode.Size = new System.Drawing.Size(646, 484);
            this.richTextBoxEdgeMillGCode.TabIndex = 5;
            this.richTextBoxEdgeMillGCode.Text = "";
            this.richTextBoxEdgeMillGCode.WordWrap = false;
            // 
            // tabPageBedFlatteningGCode
            // 
            this.tabPageBedFlatteningGCode.Controls.Add(this.label9);
            this.tabPageBedFlatteningGCode.Controls.Add(this.textBoxBedFlatteningGCodeFileName);
            this.tabPageBedFlatteningGCode.Controls.Add(this.richTextBoxBedFlatteningGCode);
            this.tabPageBedFlatteningGCode.Location = new System.Drawing.Point(4, 22);
            this.tabPageBedFlatteningGCode.Name = "tabPageBedFlatteningGCode";
            this.tabPageBedFlatteningGCode.Size = new System.Drawing.Size(737, 510);
            this.tabPageBedFlatteningGCode.TabIndex = 5;
            this.tabPageBedFlatteningGCode.Text = "Bed Flattening GCode";
            this.tabPageBedFlatteningGCode.ToolTipText = "GCode which can true up, and make flat, the bed of the mill.";
            this.tabPageBedFlatteningGCode.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(17, 8);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(26, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "File:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxBedFlatteningGCodeFileName
            // 
            this.textBoxBedFlatteningGCodeFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBedFlatteningGCodeFileName.Location = new System.Drawing.Point(49, 5);
            this.textBoxBedFlatteningGCodeFileName.Name = "textBoxBedFlatteningGCodeFileName";
            this.textBoxBedFlatteningGCodeFileName.ReadOnly = true;
            this.textBoxBedFlatteningGCodeFileName.Size = new System.Drawing.Size(642, 20);
            this.textBoxBedFlatteningGCodeFileName.TabIndex = 8;
            // 
            // richTextBoxBedFlatteningGCode
            // 
            this.richTextBoxBedFlatteningGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxBedFlatteningGCode.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxBedFlatteningGCode.Location = new System.Drawing.Point(0, 31);
            this.richTextBoxBedFlatteningGCode.Name = "richTextBoxBedFlatteningGCode";
            this.richTextBoxBedFlatteningGCode.ReadOnly = true;
            this.richTextBoxBedFlatteningGCode.Size = new System.Drawing.Size(691, 473);
            this.richTextBoxBedFlatteningGCode.TabIndex = 7;
            this.richTextBoxBedFlatteningGCode.Text = "";
            this.richTextBoxBedFlatteningGCode.WordWrap = false;
            // 
            // tabPageRefPinGCode
            // 
            this.tabPageRefPinGCode.Controls.Add(this.label10);
            this.tabPageRefPinGCode.Controls.Add(this.textBoxRefPinGCodeFileName);
            this.tabPageRefPinGCode.Controls.Add(this.richTextBoxRefPinGCode);
            this.tabPageRefPinGCode.Location = new System.Drawing.Point(4, 22);
            this.tabPageRefPinGCode.Name = "tabPageRefPinGCode";
            this.tabPageRefPinGCode.Size = new System.Drawing.Size(737, 510);
            this.tabPageRefPinGCode.TabIndex = 6;
            this.tabPageRefPinGCode.Text = "RefPin GCode";
            this.tabPageRefPinGCode.ToolTipText = "GCode which can drill the holes for the Reference Pins needed to align double sid" +
                "ed PCB\'s.";
            this.tabPageRefPinGCode.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(17, 8);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(26, 13);
            this.label10.TabIndex = 12;
            this.label10.Text = "File:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxRefPinGCodeFileName
            // 
            this.textBoxRefPinGCodeFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRefPinGCodeFileName.Location = new System.Drawing.Point(49, 5);
            this.textBoxRefPinGCodeFileName.Name = "textBoxRefPinGCodeFileName";
            this.textBoxRefPinGCodeFileName.ReadOnly = true;
            this.textBoxRefPinGCodeFileName.Size = new System.Drawing.Size(642, 20);
            this.textBoxRefPinGCodeFileName.TabIndex = 11;
            // 
            // richTextBoxRefPinGCode
            // 
            this.richTextBoxRefPinGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxRefPinGCode.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxRefPinGCode.Location = new System.Drawing.Point(0, 31);
            this.richTextBoxRefPinGCode.Name = "richTextBoxRefPinGCode";
            this.richTextBoxRefPinGCode.ReadOnly = true;
            this.richTextBoxRefPinGCode.Size = new System.Drawing.Size(691, 473);
            this.richTextBoxRefPinGCode.TabIndex = 10;
            this.richTextBoxRefPinGCode.Text = "";
            this.richTextBoxRefPinGCode.WordWrap = false;
            // 
            // tabPageDrillGCode
            // 
            this.tabPageDrillGCode.Controls.Add(this.label6);
            this.tabPageDrillGCode.Controls.Add(this.textBoxDrillGCodeFileName);
            this.tabPageDrillGCode.Controls.Add(this.richTextBoxDrillGCode);
            this.tabPageDrillGCode.Location = new System.Drawing.Point(4, 22);
            this.tabPageDrillGCode.Name = "tabPageDrillGCode";
            this.tabPageDrillGCode.Size = new System.Drawing.Size(737, 510);
            this.tabPageDrillGCode.TabIndex = 8;
            this.tabPageDrillGCode.Text = "Drill GCode";
            this.tabPageDrillGCode.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(26, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "File:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxDrillGCodeFileName
            // 
            this.textBoxDrillGCodeFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDrillGCodeFileName.Location = new System.Drawing.Point(50, 6);
            this.textBoxDrillGCodeFileName.Name = "textBoxDrillGCodeFileName";
            this.textBoxDrillGCodeFileName.ReadOnly = true;
            this.textBoxDrillGCodeFileName.Size = new System.Drawing.Size(640, 20);
            this.textBoxDrillGCodeFileName.TabIndex = 14;
            // 
            // richTextBoxDrillGCode
            // 
            this.richTextBoxDrillGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxDrillGCode.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxDrillGCode.Location = new System.Drawing.Point(1, 32);
            this.richTextBoxDrillGCode.Name = "richTextBoxDrillGCode";
            this.richTextBoxDrillGCode.ReadOnly = true;
            this.richTextBoxDrillGCode.Size = new System.Drawing.Size(691, 473);
            this.richTextBoxDrillGCode.TabIndex = 13;
            this.richTextBoxDrillGCode.Text = "";
            this.richTextBoxDrillGCode.WordWrap = false;
            // 
            // tabPageSettings
            // 
            this.tabPageSettings.Controls.Add(this.buttonDefaultIsoPtsPerAppUnit);
            this.tabPageSettings.Controls.Add(this.buttonViewLogfile);
            this.tabPageSettings.Controls.Add(this.groupBox3);
            this.tabPageSettings.Controls.Add(this.groupBox2);
            this.tabPageSettings.Controls.Add(this.buttonSaveConfiguration);
            this.tabPageSettings.Controls.Add(this.labelIsoPlotPointsPerAppUnit);
            this.tabPageSettings.Controls.Add(this.textBoxIsoPlotPointsPerAppUnit);
            this.tabPageSettings.Controls.Add(this.label2);
            this.tabPageSettings.Controls.Add(this.comboBoxApplicationUnits);
            this.tabPageSettings.Location = new System.Drawing.Point(4, 22);
            this.tabPageSettings.Name = "tabPageSettings";
            this.tabPageSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSettings.Size = new System.Drawing.Size(737, 510);
            this.tabPageSettings.TabIndex = 1;
            this.tabPageSettings.Text = "Settings";
            this.tabPageSettings.ToolTipText = "Configuration Settings";
            this.tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // buttonDefaultIsoPtsPerAppUnit
            // 
            this.buttonDefaultIsoPtsPerAppUnit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDefaultIsoPtsPerAppUnit.Location = new System.Drawing.Point(647, 77);
            this.buttonDefaultIsoPtsPerAppUnit.Name = "buttonDefaultIsoPtsPerAppUnit";
            this.buttonDefaultIsoPtsPerAppUnit.Size = new System.Drawing.Size(52, 20);
            this.buttonDefaultIsoPtsPerAppUnit.TabIndex = 10;
            this.buttonDefaultIsoPtsPerAppUnit.Text = "Default";
            this.buttonDefaultIsoPtsPerAppUnit.UseVisualStyleBackColor = true;
            this.buttonDefaultIsoPtsPerAppUnit.Click += new System.EventHandler(this.buttonDefaultIsoPtsPerAppUnit_Click);
            // 
            // buttonViewLogfile
            // 
            this.buttonViewLogfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonViewLogfile.Location = new System.Drawing.Point(572, 221);
            this.buttonViewLogfile.Name = "buttonViewLogfile";
            this.buttonViewLogfile.Size = new System.Drawing.Size(135, 23);
            this.buttonViewLogfile.TabIndex = 9;
            this.buttonViewLogfile.Text = "View Logfile";
            this.buttonViewLogfile.UseVisualStyleBackColor = true;
            this.buttonViewLogfile.Click += new System.EventHandler(this.buttonViewLogfile_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.buttonQuickSetupKiCad);
            this.groupBox3.Controls.Add(this.buttonQuickSetupEagle);
            this.groupBox3.Controls.Add(this.buttonQuickSetupDesignSpark);
            this.groupBox3.Location = new System.Drawing.Point(541, 342);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(196, 146);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Quick File Manager Setup";
            // 
            // buttonQuickSetupKiCad
            // 
            this.buttonQuickSetupKiCad.Location = new System.Drawing.Point(10, 115);
            this.buttonQuickSetupKiCad.Name = "buttonQuickSetupKiCad";
            this.buttonQuickSetupKiCad.Size = new System.Drawing.Size(176, 23);
            this.buttonQuickSetupKiCad.TabIndex = 2;
            this.buttonQuickSetupKiCad.Text = "Add Managers for KiCad";
            this.buttonQuickSetupKiCad.UseVisualStyleBackColor = true;
            this.buttonQuickSetupKiCad.Click += new System.EventHandler(this.buttonQuickSetupKiCad_Click);
            // 
            // buttonQuickSetupEagle
            // 
            this.buttonQuickSetupEagle.Location = new System.Drawing.Point(9, 74);
            this.buttonQuickSetupEagle.Name = "buttonQuickSetupEagle";
            this.buttonQuickSetupEagle.Size = new System.Drawing.Size(176, 23);
            this.buttonQuickSetupEagle.TabIndex = 1;
            this.buttonQuickSetupEagle.Text = "Add Managers for Eagle";
            this.buttonQuickSetupEagle.UseVisualStyleBackColor = true;
            this.buttonQuickSetupEagle.Click += new System.EventHandler(this.buttonQuickSetupEagle_Click);
            // 
            // buttonQuickSetupDesignSpark
            // 
            this.buttonQuickSetupDesignSpark.Location = new System.Drawing.Point(10, 33);
            this.buttonQuickSetupDesignSpark.Name = "buttonQuickSetupDesignSpark";
            this.buttonQuickSetupDesignSpark.Size = new System.Drawing.Size(176, 23);
            this.buttonQuickSetupDesignSpark.TabIndex = 0;
            this.buttonQuickSetupDesignSpark.Text = "Add Managers for DesignSpark";
            this.buttonQuickSetupDesignSpark.UseVisualStyleBackColor = true;
            this.buttonQuickSetupDesignSpark.Click += new System.EventHandler(this.buttonQuickSetupDesignSpark_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.textBoxFileManagerTitle);
            this.groupBox2.Controls.Add(this.textBoxFileManagerHeader);
            this.groupBox2.Controls.Add(this.ctlFileManagersDisplay1);
            this.groupBox2.Location = new System.Drawing.Point(3, 1);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(532, 506);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            // 
            // textBoxFileManagerTitle
            // 
            this.textBoxFileManagerTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFileManagerTitle.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxFileManagerTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxFileManagerTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxFileManagerTitle.Location = new System.Drawing.Point(41, 16);
            this.textBoxFileManagerTitle.Multiline = true;
            this.textBoxFileManagerTitle.Name = "textBoxFileManagerTitle";
            this.textBoxFileManagerTitle.Size = new System.Drawing.Size(451, 18);
            this.textBoxFileManagerTitle.TabIndex = 13;
            this.textBoxFileManagerTitle.Text = "File Managers";
            this.textBoxFileManagerTitle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxFileManagerHeader
            // 
            this.textBoxFileManagerHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFileManagerHeader.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxFileManagerHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxFileManagerHeader.Location = new System.Drawing.Point(41, 40);
            this.textBoxFileManagerHeader.Multiline = true;
            this.textBoxFileManagerHeader.Name = "textBoxFileManagerHeader";
            this.textBoxFileManagerHeader.Size = new System.Drawing.Size(451, 49);
            this.textBoxFileManagerHeader.TabIndex = 11;
            this.textBoxFileManagerHeader.Text = resources.GetString("textBoxFileManagerHeader.Text");
            this.textBoxFileManagerHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ctlFileManagersDisplay1
            // 
            this.ctlFileManagersDisplay1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ctlFileManagersDisplay1.ApplicationUnits = LineGrinder.ApplicationUnitsEnum.INCHES;
            this.ctlFileManagersDisplay1.Location = new System.Drawing.Point(4, 86);
            this.ctlFileManagersDisplay1.Name = "ctlFileManagersDisplay1";
            this.ctlFileManagersDisplay1.OptionsChanged = false;
            this.ctlFileManagersDisplay1.Size = new System.Drawing.Size(522, 416);
            this.ctlFileManagersDisplay1.TabIndex = 6;
            // 
            // buttonSaveConfiguration
            // 
            this.buttonSaveConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveConfiguration.Location = new System.Drawing.Point(572, 185);
            this.buttonSaveConfiguration.Name = "buttonSaveConfiguration";
            this.buttonSaveConfiguration.Size = new System.Drawing.Size(135, 23);
            this.buttonSaveConfiguration.TabIndex = 6;
            this.buttonSaveConfiguration.Text = "Save Configuration";
            this.buttonSaveConfiguration.UseVisualStyleBackColor = true;
            this.buttonSaveConfiguration.Click += new System.EventHandler(this.buttonSaveConfiguration_Click);
            // 
            // labelIsoPlotPointsPerAppUnit
            // 
            this.labelIsoPlotPointsPerAppUnit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelIsoPlotPointsPerAppUnit.AutoSize = true;
            this.labelIsoPlotPointsPerAppUnit.Location = new System.Drawing.Point(559, 63);
            this.labelIsoPlotPointsPerAppUnit.Name = "labelIsoPlotPointsPerAppUnit";
            this.labelIsoPlotPointsPerAppUnit.Size = new System.Drawing.Size(170, 13);
            this.labelIsoPlotPointsPerAppUnit.TabIndex = 4;
            this.labelIsoPlotPointsPerAppUnit.Text = "Iso Plot Points Per Application Unit";
            // 
            // textBoxIsoPlotPointsPerAppUnit
            // 
            this.textBoxIsoPlotPointsPerAppUnit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIsoPlotPointsPerAppUnit.Location = new System.Drawing.Point(572, 77);
            this.textBoxIsoPlotPointsPerAppUnit.Name = "textBoxIsoPlotPointsPerAppUnit";
            this.textBoxIsoPlotPointsPerAppUnit.Size = new System.Drawing.Size(69, 20);
            this.textBoxIsoPlotPointsPerAppUnit.TabIndex = 3;
            this.textBoxIsoPlotPointsPerAppUnit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxIsoPlotPointsPerAppUnit_KeyPress);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(559, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Application Units";
            // 
            // comboBoxApplicationUnits
            // 
            this.comboBoxApplicationUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxApplicationUnits.FormattingEnabled = true;
            this.comboBoxApplicationUnits.Location = new System.Drawing.Point(572, 34);
            this.comboBoxApplicationUnits.Name = "comboBoxApplicationUnits";
            this.comboBoxApplicationUnits.Size = new System.Drawing.Size(131, 21);
            this.comboBoxApplicationUnits.TabIndex = 1;
            this.comboBoxApplicationUnits.SelectedIndexChanged += new System.EventHandler(this.comboBoxApplicationUnits_SelectedIndexChanged);
            // 
            // buttonOpenFile
            // 
            this.buttonOpenFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenFile.Location = new System.Drawing.Point(754, 87);
            this.buttonOpenFile.Name = "buttonOpenFile";
            this.buttonOpenFile.Size = new System.Drawing.Size(148, 36);
            this.buttonOpenFile.TabIndex = 14;
            this.buttonOpenFile.Text = "&Open Gerber/Drill File...";
            this.buttonOpenFile.UseVisualStyleBackColor = true;
            this.buttonOpenFile.Click += new System.EventHandler(this.buttonOpenFile_Click);
            // 
            // buttonRecentFiles
            // 
            this.buttonRecentFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRecentFiles.Location = new System.Drawing.Point(754, 123);
            this.buttonRecentFiles.Name = "buttonRecentFiles";
            this.buttonRecentFiles.Size = new System.Drawing.Size(148, 23);
            this.buttonRecentFiles.TabIndex = 13;
            this.buttonRecentFiles.Text = "Open &Recent File...";
            this.buttonRecentFiles.UseVisualStyleBackColor = true;
            this.buttonRecentFiles.Click += new System.EventHandler(this.buttonRecentFiles_Click);
            // 
            // buttonConvertToGCode
            // 
            this.buttonConvertToGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConvertToGCode.Location = new System.Drawing.Point(754, 147);
            this.buttonConvertToGCode.Name = "buttonConvertToGCode";
            this.buttonConvertToGCode.Size = new System.Drawing.Size(148, 36);
            this.buttonConvertToGCode.TabIndex = 17;
            this.buttonConvertToGCode.Text = "&Convert to GCode";
            this.buttonConvertToGCode.UseVisualStyleBackColor = true;
            this.buttonConvertToGCode.Click += new System.EventHandler(this.buttonConvertToGCode_Click);
            // 
            // buttonSaveIsolationGCode
            // 
            this.buttonSaveIsolationGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveIsolationGCode.Location = new System.Drawing.Point(754, 224);
            this.buttonSaveIsolationGCode.Name = "buttonSaveIsolationGCode";
            this.buttonSaveIsolationGCode.Size = new System.Drawing.Size(148, 23);
            this.buttonSaveIsolationGCode.TabIndex = 18;
            this.buttonSaveIsolationGCode.Text = "&Save Iso GCode...";
            this.buttonSaveIsolationGCode.UseVisualStyleBackColor = true;
            this.buttonSaveIsolationGCode.Click += new System.EventHandler(this.buttonSaveIsolationGCode_Click);
            // 
            // buttonSaveIsolationGCodeAs
            // 
            this.buttonSaveIsolationGCodeAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveIsolationGCodeAs.Location = new System.Drawing.Point(754, 355);
            this.buttonSaveIsolationGCodeAs.Name = "buttonSaveIsolationGCodeAs";
            this.buttonSaveIsolationGCodeAs.Size = new System.Drawing.Size(148, 23);
            this.buttonSaveIsolationGCodeAs.TabIndex = 19;
            this.buttonSaveIsolationGCodeAs.Text = "Save Iso GCode As...";
            this.buttonSaveIsolationGCodeAs.UseVisualStyleBackColor = true;
            this.buttonSaveIsolationGCodeAs.Click += new System.EventHandler(this.buttonSaveIsolationGCodeAs_Click);
            // 
            // buttonClearAll
            // 
            this.buttonClearAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearAll.Location = new System.Drawing.Point(754, 183);
            this.buttonClearAll.Name = "buttonClearAll";
            this.buttonClearAll.Size = new System.Drawing.Size(148, 23);
            this.buttonClearAll.TabIndex = 21;
            this.buttonClearAll.Text = "Clear A&ll...";
            this.buttonClearAll.UseVisualStyleBackColor = true;
            this.buttonClearAll.Click += new System.EventHandler(this.buttonClearAll_Click);
            // 
            // buttonExit
            // 
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.Location = new System.Drawing.Point(754, 517);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(148, 23);
            this.buttonExit.TabIndex = 22;
            this.buttonExit.Text = "E&xit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonHelp
            // 
            this.buttonHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHelp.Location = new System.Drawing.Point(754, 491);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(148, 23);
            this.buttonHelp.TabIndex = 23;
            this.buttonHelp.Text = "&Help";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
            // 
            // buttonAbout
            // 
            this.buttonAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbout.Image = global::LineGrinder.Properties.Resources.gear_s;
            this.buttonAbout.Location = new System.Drawing.Point(754, 4);
            this.buttonAbout.Name = "buttonAbout";
            this.buttonAbout.Size = new System.Drawing.Size(148, 72);
            this.buttonAbout.TabIndex = 24;
            this.buttonAbout.Text = "About Line Grinder";
            this.buttonAbout.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonAbout.UseVisualStyleBackColor = true;
            this.buttonAbout.Click += new System.EventHandler(this.buttonAbout_Click);
            // 
            // textBoxStatusLine
            // 
            this.textBoxStatusLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStatusLine.Location = new System.Drawing.Point(3, 548);
            this.textBoxStatusLine.Name = "textBoxStatusLine";
            this.textBoxStatusLine.ReadOnly = true;
            this.textBoxStatusLine.Size = new System.Drawing.Size(745, 20);
            this.textBoxStatusLine.TabIndex = 25;
            // 
            // buttonSaveEdgeMillGCodeAs
            // 
            this.buttonSaveEdgeMillGCodeAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveEdgeMillGCodeAs.Location = new System.Drawing.Point(754, 401);
            this.buttonSaveEdgeMillGCodeAs.Name = "buttonSaveEdgeMillGCodeAs";
            this.buttonSaveEdgeMillGCodeAs.Size = new System.Drawing.Size(148, 23);
            this.buttonSaveEdgeMillGCodeAs.TabIndex = 27;
            this.buttonSaveEdgeMillGCodeAs.Text = "Save EMill GCode As...";
            this.buttonSaveEdgeMillGCodeAs.UseVisualStyleBackColor = true;
            this.buttonSaveEdgeMillGCodeAs.Click += new System.EventHandler(this.buttonSaveEdgeMillGCodeAs_Click);
            // 
            // buttonSaveEdgeMillGCode
            // 
            this.buttonSaveEdgeMillGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveEdgeMillGCode.Location = new System.Drawing.Point(754, 270);
            this.buttonSaveEdgeMillGCode.Name = "buttonSaveEdgeMillGCode";
            this.buttonSaveEdgeMillGCode.Size = new System.Drawing.Size(148, 23);
            this.buttonSaveEdgeMillGCode.TabIndex = 26;
            this.buttonSaveEdgeMillGCode.Text = "Save EdgeMill GCode...";
            this.buttonSaveEdgeMillGCode.UseVisualStyleBackColor = true;
            this.buttonSaveEdgeMillGCode.Click += new System.EventHandler(this.buttonSaveEdgeMillGCode_Click);
            // 
            // buttonSaveBedFlatteningGCodeAs
            // 
            this.buttonSaveBedFlatteningGCodeAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveBedFlatteningGCodeAs.Location = new System.Drawing.Point(754, 424);
            this.buttonSaveBedFlatteningGCodeAs.Name = "buttonSaveBedFlatteningGCodeAs";
            this.buttonSaveBedFlatteningGCodeAs.Size = new System.Drawing.Size(148, 23);
            this.buttonSaveBedFlatteningGCodeAs.TabIndex = 29;
            this.buttonSaveBedFlatteningGCodeAs.Text = "Save BedF GCode As...";
            this.buttonSaveBedFlatteningGCodeAs.UseVisualStyleBackColor = true;
            this.buttonSaveBedFlatteningGCodeAs.Click += new System.EventHandler(this.buttonSaveBedFlatteningGCodeAs_Click);
            // 
            // buttonSaveBedFlatteningGCode
            // 
            this.buttonSaveBedFlatteningGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveBedFlatteningGCode.Location = new System.Drawing.Point(754, 293);
            this.buttonSaveBedFlatteningGCode.Name = "buttonSaveBedFlatteningGCode";
            this.buttonSaveBedFlatteningGCode.Size = new System.Drawing.Size(148, 23);
            this.buttonSaveBedFlatteningGCode.TabIndex = 28;
            this.buttonSaveBedFlatteningGCode.Text = "Save BedF GCode...";
            this.buttonSaveBedFlatteningGCode.UseVisualStyleBackColor = true;
            this.buttonSaveBedFlatteningGCode.Click += new System.EventHandler(this.buttonSaveBedFlatteningGCode_Click);
            // 
            // buttonSaveRefPinGCodeAs
            // 
            this.buttonSaveRefPinGCodeAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveRefPinGCodeAs.Location = new System.Drawing.Point(754, 378);
            this.buttonSaveRefPinGCodeAs.Name = "buttonSaveRefPinGCodeAs";
            this.buttonSaveRefPinGCodeAs.Size = new System.Drawing.Size(148, 23);
            this.buttonSaveRefPinGCodeAs.TabIndex = 31;
            this.buttonSaveRefPinGCodeAs.Text = "Save RPin GCode As...";
            this.buttonSaveRefPinGCodeAs.UseVisualStyleBackColor = true;
            this.buttonSaveRefPinGCodeAs.Click += new System.EventHandler(this.buttonSaveRefPinGCodeAs_Click);
            // 
            // buttonSaveRefPinGCode
            // 
            this.buttonSaveRefPinGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveRefPinGCode.Location = new System.Drawing.Point(754, 247);
            this.buttonSaveRefPinGCode.Name = "buttonSaveRefPinGCode";
            this.buttonSaveRefPinGCode.Size = new System.Drawing.Size(148, 23);
            this.buttonSaveRefPinGCode.TabIndex = 30;
            this.buttonSaveRefPinGCode.Text = "Save RefPin GCode...";
            this.buttonSaveRefPinGCode.UseVisualStyleBackColor = true;
            this.buttonSaveRefPinGCode.Click += new System.EventHandler(this.buttonSaveRefPinGCode_Click);
            // 
            // buttonSaveDrillGCode
            // 
            this.buttonSaveDrillGCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveDrillGCode.Location = new System.Drawing.Point(754, 316);
            this.buttonSaveDrillGCode.Name = "buttonSaveDrillGCode";
            this.buttonSaveDrillGCode.Size = new System.Drawing.Size(148, 23);
            this.buttonSaveDrillGCode.TabIndex = 32;
            this.buttonSaveDrillGCode.Text = "Save Drill GCode...";
            this.buttonSaveDrillGCode.UseVisualStyleBackColor = true;
            this.buttonSaveDrillGCode.Click += new System.EventHandler(this.buttonSaveDrillGCode_Click);
            // 
            // buttonSaveDrillGCodeAs
            // 
            this.buttonSaveDrillGCodeAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveDrillGCodeAs.Location = new System.Drawing.Point(754, 447);
            this.buttonSaveDrillGCodeAs.Name = "buttonSaveDrillGCodeAs";
            this.buttonSaveDrillGCodeAs.Size = new System.Drawing.Size(148, 23);
            this.buttonSaveDrillGCodeAs.TabIndex = 33;
            this.buttonSaveDrillGCodeAs.Text = "Save Drill GCode As...";
            this.buttonSaveDrillGCodeAs.UseVisualStyleBackColor = true;
            this.buttonSaveDrillGCodeAs.Click += new System.EventHandler(this.buttonSaveDrillGCodeAs_Click);
            // 
            // frmMain1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 573);
            this.Controls.Add(this.buttonSaveDrillGCodeAs);
            this.Controls.Add(this.buttonSaveDrillGCode);
            this.Controls.Add(this.buttonSaveRefPinGCodeAs);
            this.Controls.Add(this.buttonSaveRefPinGCode);
            this.Controls.Add(this.buttonSaveBedFlatteningGCodeAs);
            this.Controls.Add(this.buttonSaveBedFlatteningGCode);
            this.Controls.Add(this.buttonSaveEdgeMillGCodeAs);
            this.Controls.Add(this.buttonSaveEdgeMillGCode);
            this.Controls.Add(this.textBoxStatusLine);
            this.Controls.Add(this.buttonAbout);
            this.Controls.Add(this.buttonHelp);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonClearAll);
            this.Controls.Add(this.buttonSaveIsolationGCodeAs);
            this.Controls.Add(this.buttonSaveIsolationGCode);
            this.Controls.Add(this.buttonConvertToGCode);
            this.Controls.Add(this.buttonOpenFile);
            this.Controls.Add(this.buttonRecentFiles);
            this.Controls.Add(this.tabControl1);
            this.Location = new System.Drawing.Point(0, 0);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "frmMain1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Line Grinder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain1_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPagePlot.ResumeLayout(false);
            this.tabPagePlot.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPageGerberCode.ResumeLayout(false);
            this.tabPageGerberCode.PerformLayout();
            this.tabPageExcellonFile.ResumeLayout(false);
            this.tabPageExcellonFile.PerformLayout();
            this.tabPageIsolationGCode.ResumeLayout(false);
            this.tabPageIsolationGCode.PerformLayout();
            this.tabPageEdgeMillGCode.ResumeLayout(false);
            this.tabPageEdgeMillGCode.PerformLayout();
            this.tabPageBedFlatteningGCode.ResumeLayout(false);
            this.tabPageBedFlatteningGCode.PerformLayout();
            this.tabPageRefPinGCode.ResumeLayout(false);
            this.tabPageRefPinGCode.PerformLayout();
            this.tabPageDrillGCode.ResumeLayout(false);
            this.tabPageDrillGCode.PerformLayout();
            this.tabPageSettings.ResumeLayout(false);
            this.tabPageSettings.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPagePlot;
        private System.Windows.Forms.TabPage tabPageGerberCode;
        private System.Windows.Forms.TabPage tabPageIsolationGCode;
        private System.Windows.Forms.TabPage tabPageEdgeMillGCode;
        private System.Windows.Forms.TabPage tabPageBedFlatteningGCode;
        private System.Windows.Forms.TabPage tabPageRefPinGCode;
        private System.Windows.Forms.TabPage tabPageSettings;
        private System.Windows.Forms.TextBox textBoxOpenGerberFileName;
        private System.Windows.Forms.RichTextBox richTextBoxGerberCode;
        private System.Windows.Forms.Label label1;
        private ctlPlotViewer ctlPlotViewer1;
        private System.Windows.Forms.RichTextBox richTextBoxIsolationGCode;
        private System.Windows.Forms.Button buttonOpenFile;
        private System.Windows.Forms.Button buttonRecentFiles;
        private System.Windows.Forms.Button buttonConvertToGCode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxApplicationUnits;
        private System.Windows.Forms.Label labelIsoPlotPointsPerAppUnit;
        private System.Windows.Forms.TextBox textBoxIsoPlotPointsPerAppUnit;
        private System.Windows.Forms.Button buttonSaveIsolationGCode;
        private System.Windows.Forms.Button buttonSaveIsolationGCodeAs;
        private System.Windows.Forms.Button buttonSaveConfiguration;
        private System.Windows.Forms.GroupBox groupBox2;
        private ctlFileManagersDisplay ctlFileManagersDisplay1;
        private System.Windows.Forms.Button buttonClearAll;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.Button buttonAbout;
        private System.Windows.Forms.TextBox textBoxStatusLine;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxShowGerberApertures;
        private System.Windows.Forms.Label labelOnGerberPlots;
        private System.Windows.Forms.CheckBox checkBoxShowGerberCenterLines;
        private System.Windows.Forms.RadioButton radioButtonIsoGCodePlot;
        private System.Windows.Forms.RadioButton radioButtonIsoPlotStep3;
        private System.Windows.Forms.RadioButton radioButtonIsoPlotStep2;
        private System.Windows.Forms.RadioButton radioButtonMainViewEdgeMillGCode;
        private System.Windows.Forms.RadioButton radioButtonIsoPlotStep1;
        private System.Windows.Forms.RadioButton radioButtonMainViewGerberPlot;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxMagnification;
        private System.Windows.Forms.Button buttonMagnification100;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxActiveFileManager;
        private System.Windows.Forms.Button buttonGoToFileManager;
        private System.Windows.Forms.TextBox textBoxFileManagerHeader;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button buttonQuickSetupKiCad;
        private System.Windows.Forms.Button buttonQuickSetupEagle;
        private System.Windows.Forms.Button buttonQuickSetupDesignSpark;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxIsolationGCodeFileName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxEdgeMillGCodeFileName;
        private System.Windows.Forms.RichTextBox richTextBoxEdgeMillGCode;
        private System.Windows.Forms.TextBox textBoxBedFlatteningGCodeFileName;
        private System.Windows.Forms.RichTextBox richTextBoxBedFlatteningGCode;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBoxRefPinGCodeFileName;
        private System.Windows.Forms.RichTextBox richTextBoxRefPinGCode;
        private System.Windows.Forms.Button buttonSaveEdgeMillGCodeAs;
        private System.Windows.Forms.Button buttonSaveEdgeMillGCode;
        private System.Windows.Forms.Button buttonSaveBedFlatteningGCodeAs;
        private System.Windows.Forms.Button buttonSaveBedFlatteningGCode;
        private System.Windows.Forms.Button buttonSaveRefPinGCodeAs;
        private System.Windows.Forms.Button buttonSaveRefPinGCode;
        private System.Windows.Forms.CheckBox checkBoxOnGCodePlotShowGerber;
        private System.Windows.Forms.Label labelOnGCodePlots;
        private System.Windows.Forms.RadioButton radioButtonMainViewBedFlattenGCode;
        private System.Windows.Forms.TextBox textBoxFileManagerTitle;
        private System.Windows.Forms.RadioButton radioButtonMainViewReferencePinsGCode;
        private System.Windows.Forms.TabPage tabPageExcellonFile;
        private System.Windows.Forms.TextBox textBoxOpenExcellonFileName;
        private System.Windows.Forms.RichTextBox richTextBoxExcellonCode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPageDrillGCode;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxDrillGCodeFileName;
        private System.Windows.Forms.RichTextBox richTextBoxDrillGCode;
        private System.Windows.Forms.RadioButton radioButtonNoPlot;
        private System.Windows.Forms.RadioButton radioButtonMainViewDrillGCode;
        private System.Windows.Forms.Button buttonSaveDrillGCode;
        private System.Windows.Forms.Button buttonSaveDrillGCodeAs;
        private System.Windows.Forms.Button buttonViewLogfile;
        private System.Windows.Forms.Button buttonDefaultIsoPtsPerAppUnit;
        private System.Windows.Forms.CheckBox checkBoxShowOrigin;
        private System.Windows.Forms.Label labelOnAllPlots;
    }
}

