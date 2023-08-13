using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Xml;
using System.Runtime.Serialization;
using System.Diagnostics;
using OISCommon;

using System.Drawing.Imaging;

/// +------------------------------------------------------------------------------------------------------------------------------+
/// ¦                                                   TERMS OF USE: MIT License                                                  ¦
/// +------------------------------------------------------------------------------------------------------------------------------¦
/// ¦Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    ¦
/// ¦files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    ¦
/// ¦modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software¦
/// ¦is furnished to do so, subject to the following conditions:                                                                   ¦
/// ¦                                                                                                                              ¦
/// ¦The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.¦
/// ¦                                                                                                                              ¦
/// ¦THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE          ¦
/// ¦WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR         ¦
/// ¦COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,   ¦
/// ¦ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                         ¦
/// +------------------------------------------------------------------------------------------------------------------------------+

namespace LineGrinder
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// The main form for the LineGrinder application
    /// </summary>
    public partial class frmMain1 : frmOISBase
    {
        // misc constants
#if DEBUG
        private const string DEBUG_LOG_DIRECTORY = @"C:\Dump\Project Logs";
#else
        private const string RELEASE_LOG_DIRECTORY = @"Logs";
#endif
        private const string DEFAULT_GERBERDISPLAYTEXT = "No Gerber file is open.";
        private const string DEFAULT_ISOLATIONGCODE_DISPLAYTEXT = "No Gerber to Isolation GCode conversion has been performed.";
        private const string DEFAULT_EDGEMILLGCODE_DISPLAYTEXT = "No Gerber to Edge Mill GCode conversion has been performed.";
        private const string DEFAULT_BEDFLATTENINGGCODE_DISPLAYTEXT = "No Gerber to Bed Flattening GCode conversion has been performed.";
        private const string DEFAULT_REFPINGCODE_DISPLAYTEXT = "No Gerber to Reference Pin GCode conversion has been performed.";
        private const string DEFAULT_CONFIG_FILENAME = "LineGriderSettings.xml";
        private const string LINEGRINDER_MAINHELP_FILE = "LineGrinderHelp_TableOfContents.html";
        private const string LINEGRINDER_HELPDIR = "Help";

        // app constants
        private const string APPLICATION_NAME = "Line Grinder";
        private const string APPLICATION_VERSION = "03.02";
        private const string APPLICATION_HOME = @"http://www.OfItselfSo.com/LineGrinder/LineGrinder.php";

        private const string WARN01="The Line Grinder software is released under the MIT License. There";
        private const string WARN02="is no warranty or guarantee that the GCode files it produces";
        private const string WARN03="are without error. You use it, and run its output code entirely";
        private const string WARN04="at your own risk. In particular: ";
        private const string WARN05="";
        private const string WARN06="THE LINE GRINDER SOFTWARE, AND THE OUTPUT CODE IT GENERATES, ARE ";
        private const string WARN07="PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR ";
        private const string WARN08="IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF ";
        private const string WARN09="MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND ";
        private const string WARN10="NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT ";
        private const string WARN11="HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, ";
        private const string WARN12="WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING ";
        private const string WARN13="FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR ITS OUTPUT";
        private const string WARN14="OR FROM THE USE OR OTHER DEALINGS IN THE SOFTWARE. ";

        // this is the current gerber file we read from the disk
        private GerberFile currentGerberFile = new GerberFile();
        // this is the current excellon file we read from the disk
        private ExcellonFile currentExcellonFile = new ExcellonFile();
        // this is the current isoplot builder we built from the currentGerberFile
        private IsoPlotBuilder currentIsoPlotBuilder = null;
        // this is the current gcode file we built from the currentIsoPlotBuilder
        private GCodeFile currentIsolationGCodeFile = new GCodeFile();
        // this is the current gcode file we built from the currentIsoPlotBuilder
        private GCodeFile currentEdgeMillGCodeFile = new GCodeFile();
        // this is the current bed flattening file we built
        private GCodeFile currentBedFlatteningGCodeFile = new GCodeFile();
        // this is the current reference pin file we built
        private GCodeFile currentReferencePinGCodeFile = new GCodeFile();
        // this is the current drill file we built
        private GCodeFile currentDrillGCodeFile = new GCodeFile();
        // this is the file manager for the currently open gerber or excellon file
        private FileManager currentFileManager = new FileManager();

        // these are settings the user does not explicitly configure such as form size
        // or some boolean screen control states
        private ApplicationImplicitSettings implictUserSettings = null;
        // these are settings the user configures such as file managers
        private ApplicationExplicitSettings explictUserSettings = null;

        // our MostRecentlyUsed filename list
        private frmOISMRUList mruList = null;

        private bool gerberToGCodeStep1Successful = false;
        private bool gerberToGCodeStep2Successful = false;
        private bool gerberToGCodeStep3Successful = false;

        // there are 25.4 mm to the inch
        private const int INCHTOMMSCALERx10 = 254;
        // This is the margin we add on to isoplots in order to ensure we do not have any 
        // isoCell coordinates go negative when drawing in GS
        private const float ISOPLOT_MARGIN_PERCENT = 0.10F;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public frmMain1()
        {
            bool retBOOL = false;

            // set the current directory equal to the exe directory. We do this because
            // people can start from a link and if the start-in directory is not right
            // it can put the log file in strange places
            Directory.SetCurrentDirectory(Application.StartupPath);

            // set up the Singleton g_Logger instance.
            if (g_Logger == null)
            {
                // did not work, nothing will start say so now in a generic way
                OISMessageBox("Logger Class Failed to Initialize. Nothing will work well.");
                return;
            }
            // record this in the logger for everybodys use
            g_Logger.ApplicationMainForm = this;
            g_Logger.DefaultDialogBoxTitle = APPLICATION_NAME;
            try
            {
                // set the icon for this form and for all subsequent forms
                g_Logger.AppIcon = new Icon(GetType(), "gear.ico");
                this.Icon = new Icon(GetType(), "gear.ico");
            }
            catch (Exception)
            {
            }

            // Register the global error handler as soon as we can in Main
            // to make sure that we catch as many exceptions as possible
            // this is a last resort. All execeptions should really be trapped
            // and handled by the code.
            OISGlobalExceptions eh = new OISGlobalExceptions();
            Application.ThreadException += new ThreadExceptionEventHandler(eh.OnThreadException);

            // set the culture so our numbers convert consistently
            System.Threading.Thread.CurrentThread.CurrentCulture = g_Logger.GetDefaultCulture();

            InitializeComponent();

            // set up our logging
#if DEBUG
            retBOOL = g_Logger.InitLogging(DEBUG_LOG_DIRECTORY, APPLICATION_NAME, false);
#else
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            string logfileDir = Path.Combine(appPath, RELEASE_LOG_DIRECTORY);
            retBOOL = g_Logger.InitLogging(logfileDir, APPLICATION_NAME, false);
#endif
            if (retBOOL == false)
            {
                // did not work, nothing will start say so now in a generic way
                OISMessageBox("The log file failed to create. No log file will be recorded.");
            }
            // pump out the header
            g_Logger.EmitStandardLogfileheader(APPLICATION_NAME);
            LogMessage("");
            LogMessage("Version: " + APPLICATION_VERSION);
            LogMessage("");

            // we set up the mousewheel to report to us
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseWheel);

            // set this now
            mruList = new frmOISMRUList();

            // set the screen display now
            InitScreenDisplay();
            // set this so the plot viewer can use it
            ctlPlotViewer1.MouseCursorDisplayControl = textBoxMouseCursorDisplay;

            // now recover the last configuration settings - if saved, we only do this if 
            // the control key is not pressed. This allows the user to start with the
            // Shift key pressed and reset to defaults
            if ((Control.ModifierKeys & Keys.Shift) == 0)
            {
                try
                {
                    implictUserSettings = new ApplicationImplicitSettings();
                    try
                    {
                        // we do not want to trigger user activated events when setting things
                        // up on startup
                        //suppressUserActivatedEvents = true;
                        // if we got here the above lines did not fail
                        MoveImplicitUserSettingsToScreen();
                        ReadExplictUserSettings(true);
                        MoveExplicitUserSettingsToScreen();
                    }
                    finally
                    {
                        //suppressUserActivatedEvents = false;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Error recovering previous application settings. Msg=" + ex.Message);
                }
            }
            ResetApplicationForNewFile();

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Initializes the screen display to defaults
        /// </summary>
        private void InitScreenDisplay()
        {
            LoadPlotMagnificationComboBox();
            IsoPlotPointsPerAppUnitIN = ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_IN;
            IsoPlotPointsPerAppUnitMM = ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_MM;
            DefaultApplicationUnits = ApplicationImplicitSettings.DEFAULT_APPLICATION_UNITS;
            OutputApplicationUnits = ApplicationImplicitSettings.DEFAULT_APPLICATION_UNITS;
            
            // this creates is if not present
            ctlFileManagersDisplay1.GetDefaultFileManagerObject();

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Gerber file. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public GerberFile CurrentGerberFile
        {
            get
            {
                if (currentGerberFile == null) currentGerberFile = new GerberFile();
                return currentGerberFile;
            }
            set
            {
                currentGerberFile = value;
                if (currentGerberFile == null) currentGerberFile = new GerberFile();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Excellon file. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public ExcellonFile CurrentExcellonFile
        {
            get
            {
                if (currentExcellonFile == null) currentExcellonFile = new ExcellonFile();
                return currentExcellonFile;
            }
            set
            {
                currentExcellonFile = value;
                if (currentExcellonFile == null) currentExcellonFile = new ExcellonFile();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the GCode file. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public GCodeFile CurrentIsolationGCodeFile
        {
            get
            {
                if (currentIsolationGCodeFile == null) currentIsolationGCodeFile = new GCodeFile();
                return currentIsolationGCodeFile;
            }
            set
            {
                currentIsolationGCodeFile = value;
                if (currentIsolationGCodeFile == null) currentIsolationGCodeFile = new GCodeFile();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Bed Flattening GCode file. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public GCodeFile CurrentBedFlatteningGCodeFile
        {
            get
            {
                if (currentBedFlatteningGCodeFile == null) currentBedFlatteningGCodeFile = new GCodeFile();
                return currentBedFlatteningGCodeFile;
            }
            set
            {
                currentBedFlatteningGCodeFile = value;
                if (currentBedFlatteningGCodeFile == null) currentBedFlatteningGCodeFile = new GCodeFile();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Edge Mill GCode file. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public GCodeFile CurrentEdgeMillGCodeFile
        {
            get
            {
                if (currentEdgeMillGCodeFile == null) currentEdgeMillGCodeFile = new GCodeFile();
                return currentEdgeMillGCodeFile;
            }
            set
            {
                currentEdgeMillGCodeFile = value;
                if (currentEdgeMillGCodeFile == null) currentEdgeMillGCodeFile = new GCodeFile();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Reference Pin GCode file. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public GCodeFile CurrentReferencePinGCodeFile
        {
            get
            {
                if (currentReferencePinGCodeFile == null) currentReferencePinGCodeFile = new GCodeFile();
                return currentReferencePinGCodeFile;
            }
            set
            {
                currentReferencePinGCodeFile = value;
                if (currentReferencePinGCodeFile == null) currentReferencePinGCodeFile = new GCodeFile();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Drill GCode file. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public GCodeFile CurrentDrillGCodeFile
        {
            get
            {
                if (currentDrillGCodeFile == null) currentDrillGCodeFile = new GCodeFile();
                return currentDrillGCodeFile;
            }
            set
            {
                currentDrillGCodeFile = value;
                if (currentDrillGCodeFile == null) currentDrillGCodeFile = new GCodeFile();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the currently operational file manager. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public FileManager CurrentFileManager
        {
            get
            {
                if (currentFileManager == null) currentFileManager = ctlFileManagersDisplay1.GetDefaultFileManagerObject();
                return currentFileManager;
            }
            set
            {
                currentFileManager = value;
                if (currentFileManager == null) currentFileManager = ctlFileManagersDisplay1.GetDefaultFileManagerObject();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the GCode Builder. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public IsoPlotBuilder CurrentIsoPlotBuilder
        {
            get
            {
                if (currentIsoPlotBuilder == null) currentIsoPlotBuilder = new IsoPlotBuilder(ctlPlotViewer.DEFAULT_PLOT_WIDTH, ctlPlotViewer.DEFAULT_PLOT_HEIGHT);
                return currentIsoPlotBuilder;
            }
            set
            {
                currentIsoPlotBuilder = value;
                if (currentIsoPlotBuilder == null) currentIsoPlotBuilder = new IsoPlotBuilder(ctlPlotViewer.DEFAULT_PLOT_WIDTH, ctlPlotViewer.DEFAULT_PLOT_HEIGHT);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything necessary to clear the current files and reset the 
        /// application for a new file.
        /// </summary>
        private void ResetApplicationForNewFile()
        {
            SyncSettingsEnabledStateToReality();

            // always update this with whatever we are currently using
            ctlFileManagersDisplay1.DefaultApplicationUnits = DefaultApplicationUnits;

            // reset some flags
            gerberToGCodeStep1Successful = false;
            gerberToGCodeStep2Successful = false;
            gerberToGCodeStep3Successful = false;

            CurrentExcellonFile = new ExcellonFile();
            CurrentGerberFile = new GerberFile();
            CurrentIsolationGCodeFile = new GCodeFile();
            CurrentBedFlatteningGCodeFile = new GCodeFile();
            CurrentEdgeMillGCodeFile = new GCodeFile();
            CurrentReferencePinGCodeFile = new GCodeFile();
            CurrentDrillGCodeFile = new GCodeFile();
            CurrentIsoPlotBuilder = new IsoPlotBuilder(ctlPlotViewer.DEFAULT_PLOT_WIDTH, ctlPlotViewer.DEFAULT_PLOT_HEIGHT);
            currentIsoPlotBuilder = null;
            this.ctlPlotViewer1.IsoPlotBuilderToDisplay = null;

            // I know its deprecated, In this case this really, really needs to be here
            GC.Collect();
       
            CurrentFileManager = ctlFileManagersDisplay1.GetDefaultFileManagerObject();
            // reset our tabs
            richTextBoxGerberCode.Text = DEFAULT_GERBERDISPLAYTEXT;
            textBoxOpenGerberFileName.Clear();
            richTextBoxIsolationGCode.Text = DEFAULT_ISOLATIONGCODE_DISPLAYTEXT;
            textBoxIsolationGCodeFileName.Clear();
            richTextBoxEdgeMillGCode.Text = DEFAULT_EDGEMILLGCODE_DISPLAYTEXT;
            textBoxEdgeMillGCodeFileName.Clear();
            richTextBoxBedFlatteningGCode.Text = DEFAULT_BEDFLATTENINGGCODE_DISPLAYTEXT;
            textBoxBedFlatteningGCodeFileName.Clear();
            richTextBoxRefPinGCode.Text = DEFAULT_REFPINGCODE_DISPLAYTEXT;
            textBoxRefPinGCodeFileName.Clear();

            // reset our plot
            ctlPlotViewer1.Reset();
            ctlPlotViewer1.GerberFileToDisplay = new GerberFile();
            ctlPlotViewer1.ExcellonFileToDisplay = new ExcellonFile();
            SetStatusLine("");
            ResetMagnificationComboBox();
            SyncPlotViewerToMagnificationOnScreen((string)comboBoxMagnification.SelectedItem);
            textBoxActiveFileManager.Text = "";
            ClearPlotViewBitmap();

            // this one is probably not all that necessary
            GC.Collect();

            // sync things
            SyncFormVisualsToGerberExcellonAndGCodeState();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything necessary to reset everything for a new convert to gcode
        /// event
        /// </summary>
        private void ResetApplicationForNewConvertToGCode()
        {
            // reset some flags
            gerberToGCodeStep1Successful = false;
            gerberToGCodeStep2Successful = false;
            gerberToGCodeStep3Successful = false;

            CurrentIsolationGCodeFile = new GCodeFile();
            CurrentBedFlatteningGCodeFile = new GCodeFile();
            CurrentEdgeMillGCodeFile = new GCodeFile();
            CurrentReferencePinGCodeFile = new GCodeFile();
            CurrentDrillGCodeFile = new GCodeFile();
            currentIsoPlotBuilder = null;
            this.ctlPlotViewer1.IsoPlotBuilderToDisplay = null;

            // I know its deprecated, In this case this really, really needs to be here
            GC.Collect();
            CurrentIsoPlotBuilder = new IsoPlotBuilder(ctlPlotViewer.DEFAULT_PLOT_WIDTH, ctlPlotViewer.DEFAULT_PLOT_HEIGHT);
            ClearPlotViewBitmap();
            radioButtonMainViewGerberPlot.Checked = true;
            tabControl1.SelectedTab = tabPagePlot;

            // sync things
            SyncFormVisualsToGerberExcellonAndGCodeState();
        }

        // ####################################################################
        // ##### Plot Display Options and Handlers
        // ####################################################################
        #region Plot Display Options and Handlers

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the SyncGoToFileManagerButton to screen reality
        /// </summary>
        private void SyncGoToFileManagerButton()
        {
            if ((textBoxActiveFileManager.Text == null) || (textBoxActiveFileManager.Text.Length == 0))
            {
                buttonGoToFileManager.Enabled = false;
            }
            else
            {
                buttonGoToFileManager.Enabled = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a click on the goto file manager button
        /// </summary>
        private void buttonGoToFileManager_Click(object sender, EventArgs e)
        {
            ctlFileManagersDisplay1.SelectFileManagersObjectByFilenamePattern(textBoxActiveFileManager.Text);
            // pop over to the settings tab
            tabControl1.SelectedTab = this.tabPageSettings;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle an Selection Change Committed event on the magnification combo box
        /// </summary>
        private void comboBoxMagnification_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SyncPlotViewerToMagnificationOnScreen((string)comboBoxMagnification.SelectedItem);
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a key down event on the magnification combo box. We get these
        /// when the user types in a value instead of choosing one of the options
        /// </summary>
        private void comboBoxMagnification_KeyDown(object sender, KeyEventArgs e)
        {
            // look for the enter key and send it
            if (e.KeyCode == Keys.Enter)
            {
                SyncPlotViewerToMagnificationOnScreen((string)comboBoxMagnification.Text);
                ctlPlotViewer1.Invalidate();
                e.Handled = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a click on the set plot magnification to 100% button
        /// </summary>
        private void buttonMagnification100_Click(object sender, EventArgs e)
        {
            ctlPlotViewer1.MagnificationLevel = ctlPlotViewer.DEFAULT_MAGNIFICATION_LEVEL;
            ctlPlotViewer1.SetScrollBarMaxMinLimits();
            ctlPlotViewer1.Invalidate();
            SyncMagnificationOnScreenToPlotViewer();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Loads the plot scale combo box
        /// </summary>
        private void LoadPlotMagnificationComboBox()
        {
            comboBoxMagnification.Items.Clear();
            foreach (float magnificationLevel in ctlPlotViewer.DEFAULT_MAGNIFICATION_LEVELS)
            {
                // add it in and bolt on a percent sign to make it look nice
                comboBoxMagnification.Items.Add(ConvertFloatMagnificationValueToDisplayString(magnificationLevel));
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the plot scale combo box to defaults
        /// </summary>
        private void ResetMagnificationComboBox()
        {
            comboBoxMagnification.SelectedItem = ConvertFloatMagnificationValueToDisplayString(ctlPlotViewer.DEFAULT_MAGNIFICATION_LEVEL);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the plotViewer control to the currently set magnification on screen
        /// </summary>
        /// <param name="comboboxText">the text from the combobox indicating the new
        /// value of the magnification</param>
        private void SyncPlotViewerToMagnificationOnScreen(string comboboxText)
        {
            float currentManification = ConvertDisplayStringMagnificationToFloat(comboboxText);
            if (currentManification <= 0) currentManification = ctlPlotViewer.DEFAULT_MAGNIFICATION_LEVEL;
            ctlPlotViewer1.MagnificationLevel = currentManification;
            ctlPlotViewer1.SetScrollBarMaxMinLimits();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the plotViewer control to the currently set magnification on screen
        /// </summary>
        private void SyncMagnificationOnScreenToPlotViewer()
        {
            comboBoxMagnification.SelectedItem = ConvertFloatMagnificationValueToDisplayString(ctlPlotViewer1.MagnificationLevel);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a magnification level to a user acceptable display value
        /// </summary>
        /// <returns>The mag level as a string</returns>
        private string ConvertFloatMagnificationValueToDisplayString(float magnificationLevel)
        {
            return (magnificationLevel * 100).ToString() + "%";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a a user acceptable magnification level to a float value
        /// </summary>
        /// <returns>the converted value or -1 for fail</returns>
        private float ConvertDisplayStringMagnificationToFloat(string magLevel)
        {
            if ((magLevel == null) || (magLevel.Length == 0)) return -1;
            try
            {
                return (float)((Convert.ToDouble(magLevel.Trim().Replace("%",""))/100));
            }
            catch
            {
                return -1;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a plot view radio button click
        /// </summary>
        private void radioButtonMainViewGerberPlot_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a IsoStep1 plot view radio button click
        /// </summary>
        private void radioButtonIsoPlotStep1_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a step 2 plot view radio button click
        /// </summary>
        private void radioButtonIsoPlotStep2_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a step 3 plot view radio button click
        /// </summary>
        private void radioButtonIsoPlotStep3_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a view GCode only plot view radio button click
        /// </summary>
        private void radioButtonMainViewIsoGCodePlot_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a view Edge Mill plot view radio button click
        /// </summary>
        private void radioButtonMainViewEdgeMillGCode_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a view Bed Flatten plot view radio button click
        /// </summary>
        private void radioButtonMainViewBedFlattenGCode_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a view Reference Pins plot view radio button click
        /// </summary>
        private void radioButtonMainViewReferencePinsGCode_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a view No Plot plot view radio button click
        /// </summary>
        private void radioButtonNoPlot_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a view Drill GCode plot view radio button click
        /// </summary>
        private void radioButtonMainViewDrillGCode_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the ShowGerberCenterLines checkbox
        /// </summary>
        private void checkBoxShowGerberCenterLines_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the ShowOrigin checkbox
        /// </summary>
        private void checkBoxShowOrigin_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the FlipAxis checkbox
        /// </summary>
        private void checkBoxShowFlipAxis_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the FlipAxis checkbox
        /// </summary>
        private void checkBoxShowGCodeOrigin_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the ShowGerberApertures checkbox
        /// </summary>
        private void checkBoxShowGerberApertures_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// handles a click on the show gerber plot on isolation plots
        /// </summary>
        private void checkBoxIsoPlotShowGerber_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// handles a click on the show gerber plot on tmp iso plots
        /// </summary>
        private void checkBoxOnGCodePlotShowGerber_CheckedChanged(object sender, EventArgs e)
        {
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// does the work of setting the plot display control to display the proper
        /// mode
        /// </summary>
        private void SyncGCodePlotToRadioButtons()
        {
            // always set these now
            CurrentGerberFile.StateMachine.ShowGerberCenterLines = checkBoxShowGerberCenterLines.Checked;
            CurrentGerberFile.StateMachine.ShowGerberApertures = checkBoxShowGerberApertures.Checked;

            if ((radioButtonIsoPlotStep1.Enabled == true) && (radioButtonIsoPlotStep1.Checked == true))
            {
                ctlPlotViewer1.DisplayMode = DisplayModeEnum.DisplayMode_ISOSTEP1;
                ctlPlotViewer1.GerberFileToDisplay = CurrentGerberFile;
                if (checkBoxOnGCodePlotShowGerber.Checked == true) ctlPlotViewer1.ShowGerberOnGCode = true;
                else ctlPlotViewer1.ShowGerberOnGCode = false;
                if (checkBoxShowOrigin.Checked == true) ctlPlotViewer1.ShowOrigin = true;
                else ctlPlotViewer1.ShowOrigin = false;
                if (checkBoxShowGCodeOrigin.Checked == true) ctlPlotViewer1.ShowGCodeOrigin = true;
                else ctlPlotViewer1.ShowGCodeOrigin = false;
                if (checkBoxShowFlipAxis.Checked == true) ctlPlotViewer1.ShowFlipAxis = true;
                else ctlPlotViewer1.ShowFlipAxis = false;
                SetIsolationArrayBitmapAppropriateToDisplayMode();
                // send the builder and GCode objects to the plot viewer
                ctlPlotViewer1.IsoPlotBuilderToDisplay = CurrentIsoPlotBuilder;
                ctlPlotViewer1.GCodeFileToDisplay = CurrentIsolationGCodeFile;
            }
            else if ((radioButtonIsoPlotStep2.Enabled == true) && (radioButtonIsoPlotStep2.Checked == true))
            {
                ctlPlotViewer1.DisplayMode = DisplayModeEnum.DisplayMode_ISOSTEP2;
                ctlPlotViewer1.GerberFileToDisplay = CurrentGerberFile;
                if (checkBoxOnGCodePlotShowGerber.Checked == true) ctlPlotViewer1.ShowGerberOnGCode = true;
                else ctlPlotViewer1.ShowGerberOnGCode = false;
                if (checkBoxShowOrigin.Checked == true) ctlPlotViewer1.ShowOrigin = true;
                else ctlPlotViewer1.ShowOrigin = false;
                if (checkBoxShowGCodeOrigin.Checked == true) ctlPlotViewer1.ShowGCodeOrigin = true;
                else ctlPlotViewer1.ShowGCodeOrigin = false;
                if (checkBoxShowFlipAxis.Checked == true) ctlPlotViewer1.ShowFlipAxis = true;
                else ctlPlotViewer1.ShowFlipAxis = false;
                SetIsolationArrayBitmapAppropriateToDisplayMode();
                // send the builder and GCode objects to the plot viewer
                ctlPlotViewer1.IsoPlotBuilderToDisplay = CurrentIsoPlotBuilder;
                ctlPlotViewer1.GCodeFileToDisplay = CurrentIsolationGCodeFile;
            }
            else if ((radioButtonIsoPlotStep3.Enabled == true) && (radioButtonIsoPlotStep3.Checked == true))
            {
                ctlPlotViewer1.DisplayMode = DisplayModeEnum.DisplayMode_ISOSTEP3;
                ctlPlotViewer1.GerberFileToDisplay = CurrentGerberFile;
                if (checkBoxOnGCodePlotShowGerber.Checked == true) ctlPlotViewer1.ShowGerberOnGCode = true;
                else ctlPlotViewer1.ShowGerberOnGCode = false;
                if (checkBoxShowOrigin.Checked == true) ctlPlotViewer1.ShowOrigin = true;
                else ctlPlotViewer1.ShowOrigin = false;
                if (checkBoxShowGCodeOrigin.Checked == true) ctlPlotViewer1.ShowGCodeOrigin = true;
                else ctlPlotViewer1.ShowGCodeOrigin = false;
                if (checkBoxShowFlipAxis.Checked == true) ctlPlotViewer1.ShowFlipAxis = true;
                else ctlPlotViewer1.ShowFlipAxis = false;
                SetIsolationArrayBitmapAppropriateToDisplayMode();
                // send the builder and GCode objects to the plot viewer
                ctlPlotViewer1.IsoPlotBuilderToDisplay = CurrentIsoPlotBuilder;
                ctlPlotViewer1.GCodeFileToDisplay = CurrentIsolationGCodeFile;
            }
            else if ((radioButtonMainViewIsoGCodePlot.Enabled == true) && (radioButtonMainViewIsoGCodePlot.Checked == true))
            {
                ctlPlotViewer1.DisplayMode = DisplayModeEnum.DisplayMode_GCODEONLY;
                ctlPlotViewer1.GerberFileToDisplay = CurrentGerberFile;
                if (checkBoxOnGCodePlotShowGerber.Checked == true) ctlPlotViewer1.ShowGerberOnGCode = true;
                else ctlPlotViewer1.ShowGerberOnGCode = false;
                if (checkBoxShowOrigin.Checked == true) ctlPlotViewer1.ShowOrigin = true;
                else ctlPlotViewer1.ShowOrigin = false;
                if (checkBoxShowGCodeOrigin.Checked == true) ctlPlotViewer1.ShowGCodeOrigin = true;
                else ctlPlotViewer1.ShowGCodeOrigin = false;
                if (checkBoxShowFlipAxis.Checked == true) ctlPlotViewer1.ShowFlipAxis = true;
                else ctlPlotViewer1.ShowFlipAxis = false;
                // send the builder and GCode objects to the plot viewer
                ctlPlotViewer1.IsoPlotBuilderToDisplay = CurrentIsoPlotBuilder;
                ctlPlotViewer1.GCodeFileToDisplay = CurrentIsolationGCodeFile;
            }
            else if ((radioButtonMainViewEdgeMillGCode.Enabled == true) && (radioButtonMainViewEdgeMillGCode.Checked==true))
            {
                ctlPlotViewer1.DisplayMode = DisplayModeEnum.DisplayMode_GCODEONLY;
                ctlPlotViewer1.GerberFileToDisplay = CurrentGerberFile;
                if (checkBoxOnGCodePlotShowGerber.Checked == true) ctlPlotViewer1.ShowGerberOnGCode = true;
                else ctlPlotViewer1.ShowGerberOnGCode = false;
                if (checkBoxShowOrigin.Checked == true) ctlPlotViewer1.ShowOrigin = true;
                else ctlPlotViewer1.ShowOrigin = false;
                if (checkBoxShowGCodeOrigin.Checked == true) ctlPlotViewer1.ShowGCodeOrigin = true;
                else ctlPlotViewer1.ShowGCodeOrigin = false;
                if (checkBoxShowFlipAxis.Checked == true) ctlPlotViewer1.ShowFlipAxis = true;
                else ctlPlotViewer1.ShowFlipAxis = false;
                // send the builder and GCode objects to the plot viewer
                ctlPlotViewer1.IsoPlotBuilderToDisplay = CurrentIsoPlotBuilder;
                ctlPlotViewer1.GCodeFileToDisplay = CurrentEdgeMillGCodeFile;
            }
            else if ((radioButtonMainViewBedFlattenGCode.Enabled == true) && (radioButtonMainViewBedFlattenGCode.Checked == true))
            {
                ctlPlotViewer1.DisplayMode = DisplayModeEnum.DisplayMode_GCODEONLY;
                ctlPlotViewer1.GerberFileToDisplay = CurrentGerberFile;
                if (checkBoxOnGCodePlotShowGerber.Checked == true) ctlPlotViewer1.ShowGerberOnGCode = true;
                else ctlPlotViewer1.ShowGerberOnGCode = false;
                if (checkBoxShowOrigin.Checked == true) ctlPlotViewer1.ShowOrigin = true;
                else ctlPlotViewer1.ShowOrigin = false;
                if (checkBoxShowGCodeOrigin.Checked == true) ctlPlotViewer1.ShowGCodeOrigin = true;
                else ctlPlotViewer1.ShowGCodeOrigin = false;
                if (checkBoxShowFlipAxis.Checked == true) ctlPlotViewer1.ShowFlipAxis = true;
                else ctlPlotViewer1.ShowFlipAxis = false;
                // send the builder and GCode objects to the plot viewer
                ctlPlotViewer1.IsoPlotBuilderToDisplay = CurrentIsoPlotBuilder;
                ctlPlotViewer1.GCodeFileToDisplay = CurrentBedFlatteningGCodeFile;
            }
            else if ((radioButtonMainViewReferencePinsGCode.Enabled == true) && (radioButtonMainViewReferencePinsGCode.Checked == true))
            {
                ctlPlotViewer1.DisplayMode = DisplayModeEnum.DisplayMode_GCODEONLY;
                ctlPlotViewer1.GerberFileToDisplay = CurrentGerberFile;
                if (checkBoxOnGCodePlotShowGerber.Checked == true) ctlPlotViewer1.ShowGerberOnGCode = true;
                else ctlPlotViewer1.ShowGerberOnGCode = false;
                if (checkBoxShowOrigin.Checked == true) ctlPlotViewer1.ShowOrigin = true;
                else ctlPlotViewer1.ShowOrigin = false;
                if (checkBoxShowGCodeOrigin.Checked == true) ctlPlotViewer1.ShowGCodeOrigin = true;
                else ctlPlotViewer1.ShowGCodeOrigin = false;
                if (checkBoxShowFlipAxis.Checked == true) ctlPlotViewer1.ShowFlipAxis = true;
                else ctlPlotViewer1.ShowFlipAxis = false;
                // send the builder and GCode objects to the plot viewer
                ctlPlotViewer1.IsoPlotBuilderToDisplay = CurrentIsoPlotBuilder;
                ctlPlotViewer1.GCodeFileToDisplay = CurrentReferencePinGCodeFile;
            }
            else if ((radioButtonMainViewDrillGCode.Enabled == true) && (radioButtonMainViewDrillGCode.Checked == true))
            {
                ctlPlotViewer1.DisplayMode = DisplayModeEnum.DisplayMode_GCODEONLY;
                ctlPlotViewer1.GerberFileToDisplay = null;
                ctlPlotViewer1.ExcellonFileToDisplay = CurrentExcellonFile;
                if (checkBoxOnGCodePlotShowGerber.Checked == true) ctlPlotViewer1.ShowGerberOnGCode = true;
                else ctlPlotViewer1.ShowGerberOnGCode = false;
                if (checkBoxShowOrigin.Checked == true) ctlPlotViewer1.ShowOrigin = true;
                else ctlPlotViewer1.ShowOrigin = false;
                if (checkBoxShowGCodeOrigin.Checked == true) ctlPlotViewer1.ShowGCodeOrigin = true;
                else ctlPlotViewer1.ShowGCodeOrigin = false;
                if (checkBoxShowFlipAxis.Checked == true) ctlPlotViewer1.ShowFlipAxis = true;
                else ctlPlotViewer1.ShowFlipAxis = false;
                // send the builder and GCode objects to the plot viewer
                ctlPlotViewer1.IsoPlotBuilderToDisplay = null;
                ctlPlotViewer1.GCodeFileToDisplay = CurrentDrillGCodeFile;
            }
            else if ((radioButtonMainViewGerberPlot.Enabled == true) && (radioButtonMainViewGerberPlot.Checked == true))
            {
                ctlPlotViewer1.DisplayMode = DisplayModeEnum.DisplayMode_GERBERONLY;
                ctlPlotViewer1.GerberFileToDisplay = CurrentGerberFile;
                ctlPlotViewer1.ExcellonFileToDisplay = CurrentExcellonFile;
                ctlPlotViewer1.ShowGerberOnGCode = false;
                ctlPlotViewer1.IsoPlotBuilderToDisplay = null;
                ctlPlotViewer1.GCodeFileToDisplay = null;
                if (checkBoxShowOrigin.Checked == true) ctlPlotViewer1.ShowOrigin = true;
                else ctlPlotViewer1.ShowOrigin = false;
                if (checkBoxShowGCodeOrigin.Checked == true) ctlPlotViewer1.ShowGCodeOrigin = true;
                else ctlPlotViewer1.ShowGCodeOrigin = false;
                if (checkBoxShowFlipAxis.Checked == true) ctlPlotViewer1.ShowFlipAxis = true;
                else ctlPlotViewer1.ShowFlipAxis = false;
            }
            else 
            {
                if (checkBoxShowOrigin.Checked == true) ctlPlotViewer1.ShowOrigin = true;
                else ctlPlotViewer1.ShowOrigin = false;
                if (checkBoxShowGCodeOrigin.Checked == true) ctlPlotViewer1.ShowGCodeOrigin = true;
                else ctlPlotViewer1.ShowGCodeOrigin = false;
                if (checkBoxShowFlipAxis.Checked == true) ctlPlotViewer1.ShowFlipAxis = true;
                else ctlPlotViewer1.ShowFlipAxis = false;
                ctlPlotViewer1.DisplayMode = DisplayModeEnum.DisplayMode_NONE;
                ctlPlotViewer1.ShowGerberOnGCode = false;
                ctlPlotViewer1.GerberFileToDisplay = null;
                ctlPlotViewer1.ExcellonFileToDisplay = null;
                ctlPlotViewer1.IsoPlotBuilderToDisplay = null;
                ctlPlotViewer1.GCodeFileToDisplay = null;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the visible tabs to the current state
        /// </summary>
        private void SyncVisibleTabs()
        {
            if (this.IsValidGerberFileOpen() == true)
            {
                if (tabControl1.TabPages.Contains(tabPageGerberCode) == false)
                {
                    tabControl1.TabPages.Add(tabPageGerberCode);
                }
            }
            else
            {
                tabControl1.TabPages.Remove(tabPageGerberCode);
            }

            if (this.IsValidExcellonFileOpen() == true)
            {
                if (tabControl1.TabPages.Contains(tabPageExcellonFile) == false)
                {
                    tabControl1.TabPages.Add(tabPageExcellonFile);
                }
            }
            else
            {
                tabControl1.TabPages.Remove(tabPageExcellonFile);
            }

            if (this.IsValidIsolationGCodeFileOpen() == true)
            {
                if (tabControl1.TabPages.Contains(tabPageIsolationGCode) == false)
                {
                    tabControl1.TabPages.Add(tabPageIsolationGCode);
                }
            }
            else
            {
                tabControl1.TabPages.Remove(tabPageIsolationGCode);
            }

            if (this.IsValidEdgeMillGCodeFileOpen() == true)
            {
                if (tabControl1.TabPages.Contains(tabPageEdgeMillGCode) == false)
                {
                    tabControl1.TabPages.Add(tabPageEdgeMillGCode);
                }                
            }
            else
            {
                tabControl1.TabPages.Remove(tabPageEdgeMillGCode);
            }

            if (this.IsValidBedFlatteningGCodeFileOpen() == true)
            {
                if (tabControl1.TabPages.Contains(tabPageBedFlatteningGCode) == false)
                {
                    tabControl1.TabPages.Add(tabPageBedFlatteningGCode);
                }
            }
            else
            {
                tabControl1.TabPages.Remove(tabPageBedFlatteningGCode);
            }

            if (this.IsValidRefPinGCodeFileOpen() == true)
            {
                if (tabControl1.TabPages.Contains(tabPageRefPinGCode) == false)
                {
                    tabControl1.TabPages.Add(tabPageRefPinGCode);
                }
            }
            else
            {
                tabControl1.TabPages.Remove(tabPageRefPinGCode);
            }

            if (this.IsValidDrillGCodeFileOpen() == true)
            {
                if (tabControl1.TabPages.Contains(tabPageDrillGCode) == false)
                {
                    tabControl1.TabPages.Add(tabPageDrillGCode);
                }
            }
            else
            {
                tabControl1.TabPages.Remove(tabPageDrillGCode);
            }

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the save buttons enabled state to the current state
        /// </summary>
        private void SyncSaveButtonEnabledState()
        {
            // turn everything off - re-enable what we need below
            buttonSaveIsolationGCode.Enabled = false;
            buttonSaveIsolationGCodeAs.Enabled = false;
            buttonSaveEdgeMillGCode.Enabled = false;
            buttonSaveEdgeMillGCodeAs.Enabled = false;
            buttonSaveBedFlatteningGCode.Enabled = false;
            buttonSaveBedFlatteningGCodeAs.Enabled = false;
            buttonSaveRefPinGCode.Enabled = false;
            buttonSaveRefPinGCodeAs.Enabled = false;
            buttonSaveDrillGCode.Enabled = false;
            buttonSaveDrillGCodeAs.Enabled = false;


            // got an Isocode file
            if (this.IsValidIsolationGCodeFileOpen() == true)
            {
                buttonSaveIsolationGCode.Enabled = true;
                buttonSaveIsolationGCodeAs.Enabled = true;
            }

            // got an Edge Mill file
            if (this.IsValidEdgeMillGCodeFileOpen() == true)
            {
                buttonSaveEdgeMillGCode.Enabled = true;
                buttonSaveEdgeMillGCodeAs.Enabled = true;
            }

            // got a BedFlattening file
            if (this.IsValidBedFlatteningGCodeFileOpen() == true)
            {
                buttonSaveBedFlatteningGCode.Enabled = true;
                buttonSaveBedFlatteningGCodeAs.Enabled = true;
            }

            // got a RefPin file
            if (this.IsValidRefPinGCodeFileOpen() == true)
            {
                buttonSaveRefPinGCode.Enabled = true;
                buttonSaveRefPinGCodeAs.Enabled = true;
            }

            // got a Drill file
            if (this.IsValidDrillGCodeFileOpen() == true)
            {
                buttonSaveDrillGCode.Enabled = true;
                buttonSaveDrillGCodeAs.Enabled = true;
            }

        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up the form visuals so that the permitted actions reflect the
        /// current state of the Gerber file open and GCode
        /// </summary>
        private void SyncFormVisualsToGerberExcellonAndGCodeState()
        {
            SyncSettingsEnabledStateToReality();
            SyncGoToFileManagerButton();
            SyncVisibleTabs();
            SyncSaveButtonEnabledState();
            SyncPlotVisualsToGerberAndGCodeState();
            EnsureNoDisabledPlotOptionsAreSelected();
            SyncGCodePlotToRadioButtons();
            ctlPlotViewer1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If any of the disabled plot radio buttons are selected this will reset
        /// it to the gerber display option so that it is sensible
        /// </summary>
        private void EnsureNoDisabledPlotOptionsAreSelected()
        {
            if ((radioButtonIsoPlotStep1.Enabled == false) && (radioButtonIsoPlotStep1.Checked == true))
            {
                radioButtonNoPlot.Checked = true;
                return;
            }
            if ((radioButtonIsoPlotStep2.Enabled == false) && (radioButtonIsoPlotStep2.Checked == true))
            {
                radioButtonNoPlot.Checked = true;
                return;
            }
            if ((radioButtonIsoPlotStep3.Enabled == false) && (radioButtonIsoPlotStep3.Checked == true))
            {
                radioButtonNoPlot.Checked = true;
                return;
            }
            if ((radioButtonMainViewEdgeMillGCode.Enabled == false) && (radioButtonMainViewEdgeMillGCode.Checked == true))
            {
                radioButtonNoPlot.Checked = true;
                return;
            }
            if ((radioButtonMainViewBedFlattenGCode.Enabled == false) && (radioButtonMainViewBedFlattenGCode.Checked == true))
            {
                radioButtonNoPlot.Checked = true;
                return;
            }
            if ((radioButtonMainViewReferencePinsGCode.Enabled == false) && (radioButtonMainViewReferencePinsGCode.Checked == true))
            {
                radioButtonNoPlot.Checked = true;
                return;
            }
            if ((radioButtonMainViewDrillGCode.Enabled == false) && (radioButtonMainViewDrillGCode.Checked == true))
            {
                radioButtonNoPlot.Checked = true;
                return;
            }
            if ((radioButtonMainViewGerberPlot.Enabled == false) && (radioButtonMainViewGerberPlot.Checked == true))
            {
                radioButtonNoPlot.Checked = true;
                return;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the radio buttons and checkboxes on the plot to the current
        /// Gerber and GCode processsing state
        /// </summary>
        private void SyncPlotVisualsToGerberAndGCodeState()
        {
            // always disable all available options. We will re-enable
            // the appropriate ones
            radioButtonNoPlot.Enabled = true;
            radioButtonMainViewGerberPlot.Enabled = false;
            radioButtonIsoPlotStep1.Enabled = false;
            radioButtonIsoPlotStep2.Enabled = false;
            radioButtonIsoPlotStep3.Enabled = false;
            radioButtonMainViewIsoGCodePlot.Enabled = false;
            radioButtonMainViewEdgeMillGCode.Enabled = false;
            radioButtonMainViewBedFlattenGCode.Enabled = false;
            radioButtonMainViewReferencePinsGCode.Enabled = false;
            radioButtonMainViewDrillGCode.Enabled = false;
            // the Gerber secondary checkboxes
            labelOnGerberPlots.Enabled = false;
            labelOnGerberPlots.Enabled = false;
            checkBoxShowGerberCenterLines.Enabled = false;
            checkBoxShowGerberApertures.Enabled = false;
            checkBoxShowOrigin.Enabled = false;
            checkBoxShowGCodeOrigin.Enabled = false;
            checkBoxShowFlipAxis.Enabled = false;
            labelOnGCodePlots.Enabled = false;
            checkBoxOnGCodePlotShowGerber.Enabled = false;
            // disable this button
            buttonConvertToGCode.Enabled = false;

            if (this.IsValidGerberFileOpen() == true)
            {
                // enable these
                radioButtonMainViewGerberPlot.Enabled = true;
                buttonConvertToGCode.Enabled = true;
                labelOnGerberPlots.Enabled = true;
                labelOnGerberPlots.Enabled = true;
                checkBoxShowGerberCenterLines.Enabled = true;
                checkBoxShowOrigin.Enabled = true;
                checkBoxShowGCodeOrigin.Enabled = true;
                checkBoxShowFlipAxis.Enabled = true;
                checkBoxShowGCodeOrigin.Enabled = true;
                checkBoxShowFlipAxis.Enabled = true;
                checkBoxShowGerberApertures.Enabled = true;
            }
            else if (this.IsValidExcellonFileOpen() == true)
            {
                // enable these
                radioButtonMainViewGerberPlot.Enabled = true;
                buttonConvertToGCode.Enabled = true;
                labelOnGerberPlots.Enabled = true;
            }
            else
            {
                // force this state
                radioButtonNoPlot.Checked = true;
                labelOnGerberPlots.Enabled = false;
                return;
            }

            // got an Isocode file
            if (this.IsValidIsolationGCodeFileOpen() == true)
            {
                radioButtonIsoPlotStep1.Enabled = true;
                radioButtonIsoPlotStep2.Enabled = true;
                radioButtonIsoPlotStep3.Enabled = true;
                radioButtonMainViewIsoGCodePlot.Enabled = true;
                labelOnGCodePlots.Enabled = true;
                checkBoxOnGCodePlotShowGerber.Enabled = true;
            }

            // got an Edge Mill file
            if (this.IsValidEdgeMillGCodeFileOpen() == true)
            {
                radioButtonMainViewEdgeMillGCode.Enabled = true;
                labelOnGCodePlots.Enabled = true;
                checkBoxOnGCodePlotShowGerber.Enabled = true;
            }

            // got a BedFlattening file
            if (this.IsValidBedFlatteningGCodeFileOpen() == true)
            {
                radioButtonMainViewBedFlattenGCode.Enabled = true;
                labelOnGCodePlots.Enabled = true;
                checkBoxOnGCodePlotShowGerber.Enabled = true;
            }

            // got a RefPin file
            if (this.IsValidRefPinGCodeFileOpen() == true)
            {
                radioButtonMainViewReferencePinsGCode.Enabled = true;
                labelOnGCodePlots.Enabled = true;
                checkBoxOnGCodePlotShowGerber.Enabled = true;
            }

            // got an Drill file
            if (this.IsValidDrillGCodeFileOpen() == true)
            {
                radioButtonMainViewDrillGCode.Enabled = true;
                labelOnGCodePlots.Enabled = true;
                checkBoxOnGCodePlotShowGerber.Enabled = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the enabled state of all settings to to reality
        /// </summary>
        public void SyncSettingsEnabledStateToReality()
        {
            bool settingsEnabledState = true;
            if (IsAnyFileOpen() == true)
            {
                settingsEnabledState = false;
            }
            // this goes in reverse
            labelConfigChangesDisabled.Visible = !settingsEnabledState;

            buttonAddNewFileManager.Enabled = settingsEnabledState;
            buttonRemoveSelectedFileManager.Enabled = settingsEnabledState;
            buttonRemoveAllFileManagers.Enabled = settingsEnabledState;

            groupBoxQuickFileManagerSetup.Enabled = settingsEnabledState;
            ctlFileManagersDisplay1.SyncEnabledState(settingsEnabledState);
            SyncIsoPlotUnitsEnabledStateToReality();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the state of the isoplot units box to reality
        /// </summary>
        public void SyncIsoPlotUnitsEnabledStateToReality()
        {
            bool inchEnabledState = true;
            bool mmEnabledState = false;
            bool groupEnabledState = true;

            // if any file is open everything is disabled we cannot change this
            if (IsAnyFileOpen()==true)
            {
                inchEnabledState = false;
                mmEnabledState = false;
                groupEnabledState = false;
            }
            else if(radioButtonDefaultUnitsAreMM.Checked==true)
            {
                inchEnabledState = false;
                mmEnabledState = true;
                groupEnabledState = true;
            }
            else
            {
                // assume radioButtonDefaultUnitsAreIN.Checked==true
                inchEnabledState = true;
                mmEnabledState = false;
                groupEnabledState = true;
            }

            // now set everything
            groupBoxDefaultApplicationUnits.Enabled = groupEnabledState;
            radioButtonDefaultUnitsAreIN.Enabled = groupEnabledState;
            radioButtonDefaultUnitsAreMM.Enabled = groupEnabledState;
            labelIsoPlotPointsIN.Enabled = inchEnabledState;
            labelIsoPlotPointsMM.Enabled = mmEnabledState;
            textBoxIsoPlotPointsPerIN.Enabled = inchEnabledState;
            textBoxIsoPlotPointsPerMM.Enabled = mmEnabledState;
            buttonDefaultIsoPtsPerIN.Enabled = inchEnabledState;
            buttonDefaultIsoPtsPerMM.Enabled = mmEnabledState;

            groupBoxOutputUnits.Enabled = groupEnabledState;
            radioButtonOutputUnitsAreIN.Enabled = groupEnabledState;
            radioButtonOutputUnitsAreMM.Enabled = groupEnabledState;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if any file is open
        /// </summary>
        /// <returns>true at least one file is open, false none are</returns>
        public bool IsAnyFileOpen()
        {
            if (this.IsValidGerberFileOpen() == true) return true;
            if (this.IsValidExcellonFileOpen() == true) return true;
            if (this.IsValidIsolationGCodeFileOpen() == true) return true;
            if (this.IsValidEdgeMillGCodeFileOpen() == true) return true;
            if (this.IsValidBedFlatteningGCodeFileOpen() == true) return true;
            if (this.IsValidRefPinGCodeFileOpen() == true) return true;
            if (this.IsValidDrillGCodeFileOpen() == true) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the IsolationArray as a bitmap and sets it on the plot display
        /// this makes it possible for the user to see what is going on at the 
        /// various stages and debug things
        /// </summary>
        /// <returns>z success, nz fail</returns>
        private int SetIsolationArrayBitmapAppropriateToDisplayMode()
        {
            Cursor tmpCursor = Cursor.Current;

            // do we need a bitmap
            if ((ctlPlotViewer1.DisplayMode == DisplayModeEnum.DisplayMode_ISOSTEP1) ||
               (ctlPlotViewer1.DisplayMode == DisplayModeEnum.DisplayMode_ISOSTEP2) ||
               (ctlPlotViewer1.DisplayMode == DisplayModeEnum.DisplayMode_ISOSTEP3))
            {
                // we do in these modes, but is an appropriate bitmap already set?
                if (ctlPlotViewer1.BitmapMode == ctlPlotViewer1.DisplayMode)
                {
                    // yes it is
                    return 0;
                }
                //set the cursor, this can take some time
                tmpCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    // get the bitmap
                    Bitmap testBmp = CurrentIsoPlotBuilder.GetIsolationArrayAsBitmap(ctlPlotViewer1.DisplayMode);
                    if (testBmp == null)
                    {
                        LogMessage("SetIsolationArrayBitmapAppropriateToDisplayMode testBmp == null");
                        return 123;
                    }
                    ClearPlotViewBitmap();
                    ctlPlotViewer1.SetBackgroundBitmap(testBmp,ctlPlotViewer1.DisplayMode);
                }
                finally
                {
                    // restore the cursor back to what it was
                    Cursor.Current = tmpCursor;
                }
            }
            else
            {
                // we do not need a bitmap
               // ctlPlotViewer1.SetBackgroundBitmap(null, ctlPlotViewer1.DisplayMode);
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clears the bitmap out of the plot viewer and disposes of it properly
        /// </summary>
        /// <returns>z success, nz fail</returns>
        private void ClearPlotViewBitmap()
        {
            Bitmap existingBitmap = ctlPlotViewer1.BackgroundBitmap;
            ctlPlotViewer1.SetBackgroundBitmap(null, DisplayModeEnum.DisplayMode_GERBERONLY);
            // dispose of the existing bitmap
            if (existingBitmap != null)
            {
                existingBitmap.Dispose();
                existingBitmap = null;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the status line text
        /// </summary>
        /// <param name="statusTextIn">text to write in the status bar</param>
        private void SetStatusLine(string statusTextIn)
        {
            if (statusTextIn == null) this.textBoxStatusLine.Text = "";
            else textBoxStatusLine.Text = statusTextIn;
        }

        #endregion

        // ####################################################################
        // ##### Form Configuration Save and Restore Code
        // ####################################################################
        #region Form Configuration Save and Restore Code

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the currently set Default Application Units as an enum
        /// </summary>
        private ApplicationUnitsEnum DefaultApplicationUnits
        {
            get
            {
                if (radioButtonDefaultUnitsAreMM.Checked == true) return ApplicationUnitsEnum.MILLIMETERS;
                else return ApplicationUnitsEnum.INCHES;
            }
            set
            {
                if (value == ApplicationUnitsEnum.MILLIMETERS) radioButtonDefaultUnitsAreMM.Checked = true;
                else radioButtonDefaultUnitsAreIN.Checked = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the currently set Output Application Units as an enum
        /// </summary>
        private ApplicationUnitsEnum OutputApplicationUnits
        {
            get
            {
                if (radioButtonOutputUnitsAreMM.Checked == true) return ApplicationUnitsEnum.MILLIMETERS;
                else return ApplicationUnitsEnum.INCHES;
            }
            set
            {
                if (value == ApplicationUnitsEnum.MILLIMETERS) radioButtonOutputUnitsAreMM.Checked = true;
                else radioButtonOutputUnitsAreIN.Checked = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the currently set IsoPlotPointsPerAppUnit. This depends on the 
        /// current application unit setting
        /// </summary>
        /// <param name="currentUnits">the current application units</param>
        private int GetIsoPlotPointsPerAppUnit(ApplicationUnitsEnum currentUnits)
        {
            // are we inches or mm?
            if (currentUnits == ApplicationUnitsEnum.MILLIMETERS) return IsoPlotPointsPerAppUnitMM;
            else return IsoPlotPointsPerAppUnitIN; // assume inches
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the currently set IsoPlotPointsPerAppUnit. This is for MM
        /// </summary>
        private int IsoPlotPointsPerAppUnitMM
        {
            get
            {
                int retInt;
                // we are millimeters
                try
                {
                    retInt = Convert.ToInt32(textBoxIsoPlotPointsPerMM.Text);
                }
                catch
                {
                    retInt = ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_MM;
                }
                if (retInt <= 0) return ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_MM;
                return retInt;
            }
            set
            {
                // we are millimeters
                if (value <= 0) textBoxIsoPlotPointsPerMM.Text = ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_MM.ToString();
                textBoxIsoPlotPointsPerMM.Text = value.ToString(); ;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the currently set IsoPlotPointsPerAppUnit. This is for INCHES
        /// </summary>
        private int IsoPlotPointsPerAppUnitIN
        {
            get
            {
                // assume inches
                int retInt;
                try
                {
                    retInt = Convert.ToInt32(textBoxIsoPlotPointsPerIN.Text);
                }
                catch
                {
                    retInt = ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_IN;
                }
                if (retInt <= 0) return ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_IN;
                return retInt;
            }
            set
            {
                // assume inches
                if (value <= 0) textBoxIsoPlotPointsPerIN.Text = ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_IN.ToString();
                textBoxIsoPlotPointsPerIN.Text = value.ToString(); ;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle click on the radioButtonDefaultUnitsAreIN radio button
        /// </summary>
        private void radioButtonDefaultUnitsAreIN_CheckedChanged(object sender, EventArgs e)
        {
            // always update this with whatever we are currently using
            ctlFileManagersDisplay1.DefaultApplicationUnits = DefaultApplicationUnits;
            SyncIsoPlotUnitsEnabledStateToReality();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle click on the radioButtonDefaultUnitsAreMM radio button
        /// </summary>
        private void radioButtonDefaultUnitsAreMM_CheckedChanged(object sender, EventArgs e)
        {
            // always update this with whatever we are currently using
            ctlFileManagersDisplay1.DefaultApplicationUnits = DefaultApplicationUnits;
            SyncIsoPlotUnitsEnabledStateToReality();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle click on the buttonDefaultIsoPtsPerIN_Click button
        /// </summary>
        private void buttonDefaultIsoPtsPerIN_Click(object sender, EventArgs e)
        {
            // just set the default
            textBoxIsoPlotPointsPerIN.Text = ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_IN.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle click on the buttonDefaultIsoPtsPerMM_Click button
        /// </summary>
        private void buttonDefaultIsoPtsPerMM_Click(object sender, EventArgs e)
        {
            // just set the default
            textBoxIsoPlotPointsPerMM.Text = ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_MM.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle click on the buttonDefaultIsoPtsPerAppUnit
        /// </summary>
        private void buttonDefaultIsoPtsPerAppUnit_Click(object sender, EventArgs e)
        {
            // if a file is open warn the user his way
            if ((IsValidGerberFileOpen() == true) || (IsValidExcellonFileOpen() == true))
            {
                DialogResult dlgRes = OISMessageBox_YesNo("Changing this option will close the current Gerber, Excellon and GCode files.\n\nDo you wish to proceed and set this value to its default level?");
                if (dlgRes != DialogResult.Yes)
                {
                    return;
                }
            }
            else
            {
                // warn the user this way
                DialogResult dlgRes = OISMessageBox_YesNo("Do you really wish to set this value to its default level?");
                if (dlgRes != DialogResult.Yes) return;
            }
            // user wants to do it
            ResetApplicationForNewFile();

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Ensures the isoplot points per app unit box only accepts integers
        /// </summary>
        private void textBoxIsoPlotPointsPerAppUnit_KeyPress(object sender, KeyPressEventArgs e)
        {
            // test for backspace, delete etc
            if (Char.IsControl(e.KeyChar)==true)
            {
                return;
            }
            // this box is integer only - reject anything non-integer
            if (Char.IsDigit(e.KeyChar) == false)
            {
                e.Handled = true;
                return;
            }
            // we have a digit, is a file open? if not all is ok
            if ((IsValidGerberFileOpen() == true) || (IsValidExcellonFileOpen() == true))
            {
                DialogResult dlgRes = OISMessageBox_YesNo("Changing this option will close the current Gerber, Excellon and GCode files. Do you wish to proceed?");
                if (dlgRes != DialogResult.Yes)
                {
                    e.Handled = true;
                    return;
                }
                // user wants to do it
                ResetApplicationForNewFile();
            }
            else
            {
                // reset this
                ResetApplicationForNewFile();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything necessary to read the explicit configuration settings
        /// </summary>
        /// <param name="bSilent">if true we do not tell the user on error</param>
        /// <returns>z success, nz fail</returns>
        private int ReadExplictUserSettings(bool bSilent)
        {
            string appPath="";
            string errStr;
            XmlReader reader = null;
#if DEBUG
            appPath = DEBUG_LOG_DIRECTORY;
#else
            appPath = Path.GetDirectoryName(Application.ExecutablePath);
#endif
            try
            {
                string filePathAndName = Path.Combine(appPath, DEFAULT_CONFIG_FILENAME);
                // now read the configuration file
                reader = XmlReader.Create(filePathAndName);
                DataContractSerializer serializerIn = new DataContractSerializer(typeof(ApplicationExplicitSettings));
                ExplicitUserSettings = (ApplicationExplicitSettings)serializerIn.ReadObject(reader);
            }
            catch (Exception ex)
            {
                errStr = "Error reading configuration: " + ex.ToString();
                LogMessage(errStr);
                if (bSilent == false) OISMessageBox(errStr);
                return 100;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything necessary to write the explict configuration settings
        /// </summary>
        /// <param name="bSilent">if true we do not tell the user on error</param>
        /// <returns>z success, nz fail</returns>
        private int WriteExplicitUserSettings(bool bSilent)
        {
            string appPath="";
            string errStr;
            XmlWriter writer=null;
#if DEBUG
            appPath = DEBUG_LOG_DIRECTORY;
#else
            appPath = Path.GetDirectoryName(Application.ExecutablePath);
#endif
            try
            {
                string filePathAndName = Path.Combine(appPath, DEFAULT_CONFIG_FILENAME);
                // now write the configuration file
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                writer = XmlWriter.Create(filePathAndName, settings);
                DataContractSerializer serializer = new DataContractSerializer(typeof(ApplicationExplicitSettings));
                serializer.WriteObject(writer, ExplicitUserSettings);
            }
            catch (Exception ex)
            {
                errStr = "Error saving configuration: " + ex.ToString();
                LogMessage(errStr);
                if (bSilent == false) OISMessageBox(errStr);
                return 100;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Moves the implicit configuration settings from settings file to the screen
        /// </summary>
        private void MoveImplicitUserSettingsToScreen()
        {
            // implicit settings
            this.Size = ImplicitUserSettings.FormSize;
            MRUList.FileList = ImplicitUserSettings.MRUFileList;
            // populate the application units combobox contents
            DefaultApplicationUnits = ImplicitUserSettings.DefaultApplicationUnits;
            IsoPlotPointsPerAppUnitIN = ImplicitUserSettings.IsoPlotPointsPerAppUnitIN;
            IsoPlotPointsPerAppUnitMM = ImplicitUserSettings.IsoPlotPointsPerAppUnitMM;
            OutputApplicationUnits = ImplicitUserSettings.OutputApplicationUnits;

            checkBoxShowGerberApertures.Checked = ImplicitUserSettings.ShowGerberApertures;
            checkBoxShowGerberCenterLines.Checked = ImplicitUserSettings.ShowGerberCenterLines;
            checkBoxShowOrigin.Checked = ImplicitUserSettings.ShowOrigin;
            checkBoxShowGCodeOrigin.Checked = ImplicitUserSettings.ShowGCodeOrigin;
            checkBoxShowFlipAxis.Checked = ImplicitUserSettings.ShowFlipAxis;
            checkBoxOnGCodePlotShowGerber.Checked = ImplicitUserSettings.ShowGerberOnGCodePlots;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Moves the explicit configuration settings from settings file to the screen
        /// </summary>
        private void MoveExplicitUserSettingsToScreen()
        {
            // load the File Manager, first get the list
            ctlFileManagersDisplay1.FileManagersList = ExplicitUserSettings.FileManagersList;
            ctlFileManagersDisplay1.OptionsChanged = false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the configuration settings the user explicitly specifies have
        /// changed. The contents here need to be synced with the actions in
        /// SetUserImplicitUserSettings()
        /// </summary>
        private bool DoWeNeedToSaveExplicitUserSettings()
        {
            // test the Explicit User Configuration items
            if (ImplicitUserSettings.DefaultApplicationUnits != DefaultApplicationUnits) return true;
            if (ImplicitUserSettings.OutputApplicationUnits != OutputApplicationUnits) return true;
            if (ImplicitUserSettings.IsoPlotPointsPerAppUnitIN != IsoPlotPointsPerAppUnitIN) return true;
            if (ImplicitUserSettings.IsoPlotPointsPerAppUnitMM != IsoPlotPointsPerAppUnitMM) return true;
            if (ctlFileManagersDisplay1.OptionsChanged == true) return true;
            return false;
        }
        
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the form settings which the user does not really specify. These
        /// are things like form size etc.
        /// </summary>
        private void SetImplicitUserSettings()
        {
            ImplicitUserSettings.FormSize = this.Size;
            ImplicitUserSettings.MRUFileList = MRUList.FileList;
            ImplicitUserSettings.DefaultApplicationUnits = DefaultApplicationUnits;
            ImplicitUserSettings.OutputApplicationUnits = OutputApplicationUnits;
            ImplicitUserSettings.IsoPlotPointsPerAppUnitIN = IsoPlotPointsPerAppUnitIN;
            ImplicitUserSettings.IsoPlotPointsPerAppUnitMM = IsoPlotPointsPerAppUnitMM;
            ImplicitUserSettings.ShowGerberApertures = checkBoxShowGerberApertures.Checked;
            ImplicitUserSettings.ShowGerberCenterLines = checkBoxShowGerberCenterLines.Checked;
            ImplicitUserSettings.ShowOrigin = checkBoxShowOrigin.Checked;
            ImplicitUserSettings.ShowGCodeOrigin = checkBoxShowGCodeOrigin.Checked;
            ImplicitUserSettings.ShowFlipAxis = checkBoxShowFlipAxis.Checked;
            ImplicitUserSettings.ShowGerberOnGCodePlots = this.checkBoxOnGCodePlotShowGerber.Checked;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the implicit user config settings object. Will never get or set null
        /// </summary>
        public ApplicationImplicitSettings ImplicitUserSettings
        {
            get
            {
                if (implictUserSettings == null) implictUserSettings = new ApplicationImplicitSettings();
                return implictUserSettings;
            }
            set
            {
                implictUserSettings = value;
                if (implictUserSettings == null) implictUserSettings = new ApplicationImplicitSettings();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the explicit user config settings object. Will never get or set null
        /// </summary>
        public ApplicationExplicitSettings ExplicitUserSettings
        {
            get
            {
                if (explictUserSettings == null) explictUserSettings = new ApplicationExplicitSettings();
                return explictUserSettings;
            }
            set
            {
                explictUserSettings = value;
                if (explictUserSettings == null) explictUserSettings = new ApplicationExplicitSettings();
            }
        }

        #endregion

        // ####################################################################
        // ##### File Button Handlers and Events
        // ####################################################################
        #region File Button Handlers and Events

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the View Logfile button
        /// </summary>
        private void buttonViewLogfile_Click(object sender, EventArgs e)
        {
            try
            {
                // launch the file
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo();

                // these should never be null
                string dirName = g_Logger.LogFileDirectory;
                string fileName = g_Logger.LogFileName;
                if (Path.IsPathRooted(dirName) != true)
                {
                    pi.FileName = Path.Combine(System.Windows.Forms.Application.StartupPath, dirName);
                    pi.FileName = Path.Combine(pi.FileName, fileName);
                }
                else
                {
                    pi.FileName = Path.Combine(dirName, fileName);
                }
                // we expect the quick start guide to be immediately below the exe directory
                p.StartInfo = pi;
                p.Start();
            }
            catch (Exception ex)
            {
                OISMessageBox("Cannot view the log file an because of an error.\n\n" + ex.Message);
                return;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Configuration button
        /// </summary>
        private void buttonSaveConfiguration_Click(object sender, EventArgs e)
        {
            SetImplicitUserSettings();
            ImplicitUserSettings.Save();
            int retInt = WriteExplicitUserSettings(false);
            if (retInt == 0) OISMessageBox("The configuration settings have now been saved.");
            else
            {
                LogMessage("Call to WriteExplicitUserSettings returned " + retInt.ToString());
            }
            // saving the configuration marks this as changed
            ctlFileManagersDisplay1.OptionsChanged = false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Isolation GCode button
        /// </summary>
        private void buttonSaveIsolationGCode_Click(object sender, EventArgs e)
        {
            Cursor tmpCursor = Cursor.Current;
            string errStr = "";
            int retInt;

            LogMessage("buttonSaveIsolationGCode called");

            if (CurrentIsolationGCodeFile.IsPopulated == false)
            {
                OISMessageBox("The current Isolation GCode file is empty?.\n\nHave you converted the Gerber file into GCode.");
                return;
            }
            if (CurrentIsolationGCodeFile.GCodeFileManager.IsAtDefaults() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("No File Manager could be found to match the Gerber file name. Is it ok to use the default options to save the GCode file?\n\nIf not, adjust the configuration options on the Settings tab and re-try.");
                if (dlgRes == DialogResult.No)
                {
                    OISMessageBox("Save GCode operation cancelled by user request.");
                    return;
                }
                else
                {
                    // just log it
                    LogMessage("Using default settings for GCode Save.");
                }
            }
            // get the best file name
            string fileNameAndPath = GetBestGCodeOutputFileNameAndPath(ref errStr, CurrentGerberFile.GerberFilePathAndName, CurrentIsolationGCodeFile.GCodeFileManager.IsoGCodeFileOutputExtension);
            if ((fileNameAndPath == null) || (fileNameAndPath.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }
    
            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now save the file 
                retInt = SaveGCodeFile(fileNameAndPath, richTextBoxIsolationGCode.Lines, CurrentIsolationGCodeFile.StateMachine.LineTerminator, CurrentIsolationGCodeFile.GCodeFileManager.WarnAboutOverwriting, ref errStr, false);
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonSaveIsolationGCode_Click call to SaveGCodeFile returned" + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred saving the GCode File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be saved because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }
            // set this now                        
            ImplicitUserSettings.LastGCodeDirectory = Path.GetDirectoryName(fileNameAndPath);
            // flag that the gcode file has been saved
            CurrentIsolationGCodeFile.HasBeenSaved = true;
            textBoxIsolationGCodeFileName.Text = fileNameAndPath;

#if !DEBUG
            OISMessageBox("The Isolation GCode file has been saved under the name\n\n" + fileNameAndPath);
#endif
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Isolation GCode As button
        /// </summary>
        private void buttonSaveIsolationGCodeAs_Click(object sender, EventArgs e)
        {
            Cursor tmpCursor = Cursor.Current;
            string errStr = "";
            int retInt;
            string fileNameAndPath = "";

            LogMessage("buttonSaveIsolationGCodeAs called");

            if (CurrentIsolationGCodeFile.IsPopulated == false)
            {
                OISMessageBox("The current Isolation GCode file is empty?.\n\nHave you converted the Gerber file into GCode.");
                return;
            }
            // should never happen
            if (CurrentIsolationGCodeFile.GCodeFileManager.IsAtDefaults() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("No File Manager could be found to match this Gerber file name. Is it ok to use the default options to save the GCode file?\n\nIf not, adjust the configuration options on the Settings tab and re-try.");
                if (dlgRes == DialogResult.No)
                {
                    OISMessageBox("Save GCode operation cancelled by user request.");
                    return;
                }
                else
                {
                    // just log it
                    LogMessage("Using default settings for GCode Save.");
                }
            }
            string bestName = GetBestGCodeOutputFileNameAndPath(ref errStr, CurrentGerberFile.GerberFilePathAndName, CurrentIsolationGCodeFile.GCodeFileManager.IsoGCodeFileOutputExtension);
            if ((bestName == null) || (bestName.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }

            // pick the save file name
            retInt = PickGCodeFileSaveName(ImplicitUserSettings.LastGCodeDirectory, Path.GetFileName(bestName), ref fileNameAndPath);
            if(retInt<0)
            {
                // log this
                LogMessage("buttonSaveIsolationGCodeAs call to PickFile was cancelled");
                return;
            }
            if (retInt > 0)
            {
                // log this
                LogMessage("buttonSaveIsolationGCodeAs call to PickFile returned error of " + retInt.ToString());
                OISMessageBox("Error " + retInt.ToString() + " occurred when choosing the output filename.\n\nPlease see the log file.");
                return;
            }
            if ((fileNameAndPath == null) || (fileNameAndPath.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }
            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now save the file 
                retInt = SaveGCodeFile(fileNameAndPath, this.richTextBoxIsolationGCode.Lines, CurrentIsolationGCodeFile.StateMachine.LineTerminator, CurrentIsolationGCodeFile.GCodeFileManager.WarnAboutOverwriting, ref errStr, false);
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonSaveIsolationGCodeAs_Click call to SaveGCodeFile returned" + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred saving the Isolation GCode File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be saved because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }
            // set this now
            ImplicitUserSettings.LastGCodeDirectory = Path.GetDirectoryName(fileNameAndPath);
            // flag that the gcode file has been saved
            CurrentIsolationGCodeFile.HasBeenSaved = true;
            textBoxIsolationGCodeFileName.Text = fileNameAndPath;

#if !DEBUG
            OISMessageBox("The Isolation GCode file has been saved under the name\n\n" + fileNameAndPath);
#endif
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Edge Mill GCode button
        /// </summary>
        private void buttonSaveEdgeMillGCode_Click(object sender, EventArgs e)
        {
            Cursor tmpCursor = Cursor.Current;
            string errStr = "";
            int retInt;

            LogMessage("buttonSaveEdgeMillGCode called");

            if (CurrentEdgeMillGCodeFile.IsPopulated == false)
            {
                OISMessageBox("The current Edge Mill GCode file is empty?.\n\nHave you converted the Gerber file into GCode.");
                return;
            }
            if (CurrentEdgeMillGCodeFile.GCodeFileManager.IsAtDefaults() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("No File Manager could be found to match the Gerber file name. Is it ok to use the default options to save the GCode file?\n\nIf not, adjust the configuration options on the Settings tab and re-try.");
                if (dlgRes == DialogResult.No)
                {
                    OISMessageBox("Save GCode operation cancelled by user request.");
                    return;
                }
                else
                {
                    // just log it
                    LogMessage("Using default settings for GCode Save.");
                }
            }
            // get the best file name
            string fileNameAndPath = GetBestGCodeOutputFileNameAndPath(ref errStr, CurrentGerberFile.GerberFilePathAndName, CurrentEdgeMillGCodeFile.GCodeFileManager.EdgeMillGCodeFileOutputExtension);
            if ((fileNameAndPath == null) || (fileNameAndPath.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }

            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now save the file 
                retInt = SaveGCodeFile(fileNameAndPath, this.richTextBoxEdgeMillGCode.Lines, CurrentEdgeMillGCodeFile.StateMachine.LineTerminator, CurrentEdgeMillGCodeFile.GCodeFileManager.WarnAboutOverwriting, ref errStr, false);
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonSaveEdgeMillGCode_Click call to SaveGCodeFile returned" + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred saving the GCode File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be saved because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }
            // set this now                        
            ImplicitUserSettings.LastGCodeDirectory = Path.GetDirectoryName(fileNameAndPath);
            // flag that the gcode file has been saved
            CurrentEdgeMillGCodeFile.HasBeenSaved = true;
            textBoxEdgeMillGCodeFileName.Text = fileNameAndPath;

#if !DEBUG
            OISMessageBox("The Edge Mill GCode file has been saved under the name\n\n" + fileNameAndPath);
#endif
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Edge Mill GCode As button
        /// </summary>
        private void buttonSaveEdgeMillGCodeAs_Click(object sender, EventArgs e)
        {
            Cursor tmpCursor = Cursor.Current;
            string errStr = "";
            int retInt;
            string fileNameAndPath = "";

            LogMessage("buttonSaveEdgeMillGCodeAs called");

            if (CurrentEdgeMillGCodeFile.IsPopulated == false)
            {
                OISMessageBox("The current Edge Mill GCode file is empty?.\n\nHave you converted the Gerber file into GCode.");
                return;
            }
            // should never happen
            if (CurrentEdgeMillGCodeFile.GCodeFileManager.IsAtDefaults() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("No File Manager could be found to match this Gerber file name. Is it ok to use the default options to save the GCode file?\n\nIf not, adjust the configuration options on the Settings tab and re-try.");
                if (dlgRes == DialogResult.No)
                {
                    OISMessageBox("Save GCode operation cancelled by user request.");
                    return;
                }
                else
                {
                    // just log it
                    LogMessage("Using default settings for GCode Save.");
                }
            }
            string bestName = GetBestGCodeOutputFileNameAndPath(ref errStr, CurrentGerberFile.GerberFilePathAndName, CurrentEdgeMillGCodeFile.GCodeFileManager.EdgeMillGCodeFileOutputExtension);
            if ((bestName == null) || (bestName.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }

            // pick the save file name
            retInt = PickGCodeFileSaveName(ImplicitUserSettings.LastGCodeDirectory, Path.GetFileName(bestName), ref fileNameAndPath);
            if (retInt < 0)
            {
                // log this
                LogMessage("buttonSaveEdgeMillGCodeAs call to PickFile was cancelled");
                return;
            }
            if (retInt > 0)
            {
                // log this
                LogMessage("buttonSaveEdgeMillGCodeAs call to PickFile returned error of " + retInt.ToString());
                OISMessageBox("Error " + retInt.ToString() + " occurred when choosing the output filename.\n\nPlease see the log file.");
                return;
            }
            if ((fileNameAndPath == null) || (fileNameAndPath.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }
            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now save the file 
                retInt = SaveGCodeFile(fileNameAndPath, this.richTextBoxEdgeMillGCode.Lines, CurrentEdgeMillGCodeFile.StateMachine.LineTerminator,  CurrentEdgeMillGCodeFile.GCodeFileManager.WarnAboutOverwriting, ref errStr, false);
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonSaveEdgeMillGCodeAs_Click call to SaveGCodeFile returned" + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred saving the EdgeMill GCode File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be saved because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }
            // set this now
            ImplicitUserSettings.LastGCodeDirectory = Path.GetDirectoryName(fileNameAndPath);
            // flag that the gcode file has been saved
            CurrentEdgeMillGCodeFile.HasBeenSaved = true;
            textBoxEdgeMillGCodeFileName.Text = fileNameAndPath;

#if !DEBUG
            OISMessageBox("The Edge Mill GCode file has been saved under the name\n\n" + fileNameAndPath);
#endif
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Reference Pins GCode button
        /// </summary>
        private void buttonSaveRefPinGCode_Click(object sender, EventArgs e)
        {
            Cursor tmpCursor = Cursor.Current;
            string errStr = "";
            int retInt;

            LogMessage("buttonSaveRefPinGCode called");

            if (CurrentReferencePinGCodeFile.IsPopulated == false)
            {
                OISMessageBox("The current Reference Pins GCode file is empty?.\n\nHave you converted the Gerber file into GCode.");
                return;
            }
            if (CurrentReferencePinGCodeFile.GCodeFileManager.IsAtDefaults() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("No File Manager could be found to match the Gerber file name. Is it ok to use the default options to save the GCode file?\n\nIf not, adjust the configuration options on the Settings tab and re-try.");
                if (dlgRes == DialogResult.No)
                {
                    OISMessageBox("Save GCode operation cancelled by user request.");
                    return;
                }
                else
                {
                    // just log it
                    LogMessage("Using default settings for GCode Save.");
                }
            }
            // get the best file name
            string fileNameAndPath = GetBestGCodeOutputFileNameAndPath(ref errStr, CurrentGerberFile.GerberFilePathAndName, CurrentReferencePinGCodeFile.GCodeFileManager.ReferencePinsGCodeFileOutputExtension);
            if ((fileNameAndPath == null) || (fileNameAndPath.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }

            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now save the file 
                retInt = SaveGCodeFile(fileNameAndPath, this.richTextBoxRefPinGCode.Lines, CurrentEdgeMillGCodeFile.StateMachine.LineTerminator, CurrentReferencePinGCodeFile.GCodeFileManager.WarnAboutOverwriting, ref errStr, false);
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonSaveRefPinGCode_Click call to SaveGCodeFile returned" + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred saving the GCode File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be saved because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }
            // set this now                        
            ImplicitUserSettings.LastGCodeDirectory = Path.GetDirectoryName(fileNameAndPath);
            // flag that the gcode file has been saved
            CurrentReferencePinGCodeFile.HasBeenSaved = true;
            textBoxRefPinGCodeFileName.Text = fileNameAndPath;

#if !DEBUG
            OISMessageBox("The Reference Pins GCode file has been saved under the name\n\n" + fileNameAndPath);
#endif
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Reference Pins GCode As button
        /// </summary>
        private void buttonSaveRefPinGCodeAs_Click(object sender, EventArgs e)
        {
            Cursor tmpCursor = Cursor.Current;
            string errStr = "";
            int retInt;
            string fileNameAndPath = "";

            LogMessage("buttonSaveRefPinGCodeAs called");

            if (CurrentReferencePinGCodeFile.IsPopulated == false)
            {
                OISMessageBox("The current Reference Pins GCode file is empty?.\n\nHave you converted the Gerber file into GCode.");
                return;
            }
            // should never happen
            if (CurrentReferencePinGCodeFile.GCodeFileManager.IsAtDefaults() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("No File Manager could be found to match this Gerber file name. Is it ok to use the default options to save the GCode file?\n\nIf not, adjust the configuration options on the Settings tab and re-try.");
                if (dlgRes == DialogResult.No)
                {
                    OISMessageBox("Save GCode operation cancelled by user request.");
                    return;
                }
                else
                {
                    // just log it
                    LogMessage("Using default settings for GCode Save.");
                }
            }
            string bestName = GetBestGCodeOutputFileNameAndPath(ref errStr, CurrentGerberFile.GerberFilePathAndName, CurrentReferencePinGCodeFile.GCodeFileManager.ReferencePinsGCodeFileOutputExtension);
            if ((bestName == null) || (bestName.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }

            // pick the save file name
            retInt = PickGCodeFileSaveName(ImplicitUserSettings.LastGCodeDirectory, Path.GetFileName(bestName), ref fileNameAndPath);
            if (retInt < 0)
            {
                // log this
                LogMessage("buttonSaveRefPinGCodeAs call to PickFile was cancelled");
                return;
            }
            if (retInt > 0)
            {
                // log this
                LogMessage("buttonSaveRefPinGCodeAs call to PickFile returned error of " + retInt.ToString());
                OISMessageBox("Error " + retInt.ToString() + " occurred when choosing the output filename.\n\nPlease see the log file.");
                return;
            }
            if ((fileNameAndPath == null) || (fileNameAndPath.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }
            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now save the file 
                retInt = SaveGCodeFile(fileNameAndPath, richTextBoxRefPinGCode.Lines, CurrentReferencePinGCodeFile.StateMachine.LineTerminator,  CurrentReferencePinGCodeFile.GCodeFileManager.WarnAboutOverwriting, ref errStr, false);
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonSaveRefPinGCodeAs_Click call to SaveGCodeFile returned" + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred saving the RefPin GCode File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be saved because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }
            // set this now
            ImplicitUserSettings.LastGCodeDirectory = Path.GetDirectoryName(fileNameAndPath);
            // flag that the gcode file has been saved
            CurrentReferencePinGCodeFile.HasBeenSaved = true;
            textBoxRefPinGCodeFileName.Text = fileNameAndPath;

#if !DEBUG
            OISMessageBox("The Reference Pins GCode file has been saved under the name\n\n" + fileNameAndPath);
#endif
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Bed Flattening GCode button
        /// </summary>
        private void buttonSaveBedFlatteningGCode_Click(object sender, EventArgs e)
        {
            Cursor tmpCursor = Cursor.Current;
            string errStr = "";
            int retInt;

            LogMessage("buttonSaveBedFlatteningGCode called");

            if (CurrentBedFlatteningGCodeFile.IsPopulated == false)
            {
                OISMessageBox("The current Bed Flattening GCode file is empty?.\n\nHave you converted the Gerber file into GCode.");
                return;
            }
            if (CurrentBedFlatteningGCodeFile.GCodeFileManager.IsAtDefaults() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("No File Manager could be found to match the Gerber file name. Is it ok to use the default options to save the GCode file?\n\nIf not, adjust the configuration options on the Settings tab and re-try.");
                if (dlgRes == DialogResult.No)
                {
                    OISMessageBox("Save GCode operation cancelled by user request.");
                    return;
                }
                else
                {
                    // just log it
                    LogMessage("Using default settings for GCode Save.");
                }
            }
            // get the best file name
            string fileNameAndPath = GetBestGCodeOutputFileNameAndPath(ref errStr, CurrentGerberFile.GerberFilePathAndName, CurrentBedFlatteningGCodeFile.GCodeFileManager.BedFlatteningGCodeFileOutputExtension);
            if ((fileNameAndPath == null) || (fileNameAndPath.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }

            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now save the file 
                retInt = SaveGCodeFile(fileNameAndPath, this.richTextBoxBedFlatteningGCode.Lines, CurrentBedFlatteningGCodeFile.StateMachine.LineTerminator, CurrentBedFlatteningGCodeFile.GCodeFileManager.WarnAboutOverwriting, ref errStr, false);
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonSaveBedFlatteningGCode_Click call to SaveGCodeFile returned" + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred saving the GCode File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be saved because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }
            // set this now                        
            ImplicitUserSettings.LastGCodeDirectory = Path.GetDirectoryName(fileNameAndPath);
            // flag that the gcode file has been saved
            CurrentBedFlatteningGCodeFile.HasBeenSaved = true;
            textBoxBedFlatteningGCodeFileName.Text = fileNameAndPath;

#if !DEBUG
            OISMessageBox("The Bed Flattening GCode file has been saved under the name\n\n" + fileNameAndPath);
#endif
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Bed Flattening GCode As button
        /// </summary>
        private void buttonSaveBedFlatteningGCodeAs_Click(object sender, EventArgs e)
        {
            Cursor tmpCursor = Cursor.Current;
            string errStr = "";
            int retInt;
            string fileNameAndPath = "";

            LogMessage("buttonSaveBedFlatteningGCodeAs called");

            if (CurrentBedFlatteningGCodeFile.IsPopulated == false)
            {
                OISMessageBox("The current Bed Flattening GCode file is empty?.\n\nHave you converted the Gerber file into GCode.");
                return;
            }
            // should never happen
            if (CurrentBedFlatteningGCodeFile.GCodeFileManager.IsAtDefaults() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("No File Manager could be found to match this Gerber file name. Is it ok to use the default options to save the GCode file?\n\nIf not, adjust the configuration options on the Settings tab and re-try.");
                if (dlgRes == DialogResult.No)
                {
                    OISMessageBox("Save GCode operation cancelled by user request.");
                    return;
                }
                else
                {
                    // just log it
                    LogMessage("Using default settings for GCode Save.");
                }
            }
            string bestName = GetBestGCodeOutputFileNameAndPath(ref errStr, CurrentGerberFile.GerberFilePathAndName, CurrentBedFlatteningGCodeFile.GCodeFileManager.BedFlatteningGCodeFileOutputExtension);
            if ((bestName == null) || (bestName.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }

            // pick the save file name
            retInt = PickGCodeFileSaveName(ImplicitUserSettings.LastGCodeDirectory, Path.GetFileName(bestName), ref fileNameAndPath);
            if (retInt < 0)
            {
                // log this
                LogMessage("buttonSaveBed FlatteningGCodeAs call to PickFile was cancelled");
                return;
            }
            if (retInt > 0)
            {
                // log this
                LogMessage("buttonSaveBedFlatteningGCodeAs call to PickFile returned error of " + retInt.ToString());
                OISMessageBox("Error " + retInt.ToString() + " occurred when choosing the output filename.\n\nPlease see the log file.");
                return;
            }
            if ((fileNameAndPath == null) || (fileNameAndPath.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }
            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now save the file 
                retInt = SaveGCodeFile(fileNameAndPath, richTextBoxBedFlatteningGCode.Lines, CurrentBedFlatteningGCodeFile.StateMachine.LineTerminator, CurrentBedFlatteningGCodeFile.GCodeFileManager.WarnAboutOverwriting, ref errStr, false);
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonSaveBedFlatteningGCodeAs_Click call to SaveGCodeFile returned" + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred saving the BedFlattening GCode File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be saved because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }
            // set this now
            ImplicitUserSettings.LastGCodeDirectory = Path.GetDirectoryName(fileNameAndPath);
            // flag that the gcode file has been saved
            CurrentBedFlatteningGCodeFile.HasBeenSaved = true;
            textBoxBedFlatteningGCodeFileName.Text = fileNameAndPath;

#if !DEBUG
            OISMessageBox("The Bed Flattening GCode file has been saved under the name\n\n" + fileNameAndPath);
#endif
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Drill GCode button
        /// </summary>
        private void buttonSaveDrillGCode_Click(object sender, EventArgs e)
        {
            Cursor tmpCursor = Cursor.Current;
            string errStr = "";
            int retInt;

            LogMessage("buttonSaveDrillGCode called");

            if (CurrentDrillGCodeFile.IsPopulated == false)
            {
                OISMessageBox("The current Drill GCode file is empty?.\n\nHave you converted the Excellon file into GCode.");
                return;
            }
            if (CurrentDrillGCodeFile.GCodeFileManager.IsAtDefaults() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("No File Manager could be found to match the Excellon file name. Is it ok to use the default options to save the GCode file?\n\nIf not, adjust the configuration options on the Settings tab and re-try.");
                if (dlgRes == DialogResult.No)
                {
                    OISMessageBox("Save GCode operation cancelled by user request.");
                    return;
                }
                else
                {
                    // just log it
                    LogMessage("Using default settings for GCode Save.");
                }
            }
            // get the best file name
            string fileNameAndPath = GetBestGCodeOutputFileNameAndPath(ref errStr, CurrentExcellonFile.ExcellonFilePathAndName, CurrentDrillGCodeFile.GCodeFileManager.DrillingGCodeFileOutputExtension);
            if ((fileNameAndPath == null) || (fileNameAndPath.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }

            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now save the file 
                retInt = SaveGCodeFile(fileNameAndPath, this.richTextBoxDrillGCode.Lines, CurrentDrillGCodeFile.StateMachine.LineTerminator, CurrentDrillGCodeFile.GCodeFileManager.WarnAboutOverwriting, ref errStr, false);
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonSaveDrillGCode_Click call to SaveGCodeFile returned" + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred saving the GCode File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be saved because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }
            // set this now                        
            ImplicitUserSettings.LastGCodeDirectory = Path.GetDirectoryName(fileNameAndPath);
            // flag that the gcode file has been saved
            CurrentDrillGCodeFile.HasBeenSaved = true;
            textBoxDrillGCodeFileName.Text = fileNameAndPath;

#if !DEBUG
            OISMessageBox("The Drilling GCode file has been saved under the name\n\n" + fileNameAndPath);
#endif
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Save Drill GCode As button
        /// </summary>
        private void buttonSaveDrillGCodeAs_Click(object sender, EventArgs e)
        {
            Cursor tmpCursor = Cursor.Current;
            string errStr = "";
            int retInt;
            string fileNameAndPath = "";

            LogMessage("buttonSaveDrillGCodeAs called");

            if (CurrentDrillGCodeFile.IsPopulated == false)
            {
                OISMessageBox("The current Drilling GCode file is empty?.\n\nHave you converted the Excellon file into GCode.");
                return;
            }
            // should never happen
            if (CurrentDrillGCodeFile.GCodeFileManager.IsAtDefaults() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("No File Manager could be found to match this Excellon file name. Is it ok to use the default options to save the GCode file?\n\nIf not, adjust the configuration options on the Settings tab and re-try.");
                if (dlgRes == DialogResult.No)
                {
                    OISMessageBox("Save GCode operation cancelled by user request.");
                    return;
                }
                else
                {
                    // just log it
                    LogMessage("Using default settings for GCode Save.");
                }
            }
            string bestName = GetBestGCodeOutputFileNameAndPath(ref errStr, CurrentExcellonFile.ExcellonFilePathAndName, CurrentDrillGCodeFile.GCodeFileManager.DrillingGCodeFileOutputExtension);
            if ((bestName == null) || (bestName.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }

            // pick the save file name
            retInt = PickGCodeFileSaveName(ImplicitUserSettings.LastGCodeDirectory, Path.GetFileName(bestName), ref fileNameAndPath);
            if (retInt < 0)
            {
                // log this
                LogMessage("buttonSaveBed DrillGCodeAs call to PickFile was cancelled");
                return;
            }
            if (retInt > 0)
            {
                // log this
                LogMessage("buttonSaveDrillGCodeAs call to PickFile returned error of " + retInt.ToString());
                OISMessageBox("Error " + retInt.ToString() + " occurred when choosing the output filename.\n\nPlease see the log file.");
                return;
            }
            if ((fileNameAndPath == null) || (fileNameAndPath.Length == 0))
            {
                // something badly wrong
                OISMessageBox("No output filename could be constructed. Error message is:" + errStr + "\n\nPlease see the log file for more information.");
                return;
            }
            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now save the file 
                retInt = SaveGCodeFile(fileNameAndPath, richTextBoxDrillGCode.Lines, CurrentDrillGCodeFile.StateMachine.LineTerminator, CurrentDrillGCodeFile.GCodeFileManager.WarnAboutOverwriting, ref errStr, false);
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonSaveDrillGCodeAs_Click call to SaveGCodeFile returned" + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred saving the Drill GCode File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be saved because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }
            // set this now
            ImplicitUserSettings.LastGCodeDirectory = Path.GetDirectoryName(fileNameAndPath);
            // flag that the gcode file has been saved
            CurrentDrillGCodeFile.HasBeenSaved = true;
            textBoxDrillGCodeFileName.Text = fileNameAndPath;

#if !DEBUG
            OISMessageBox("The Drilling GCode file has been saved under the name\n\n" + fileNameAndPath);
#endif
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the open file button
        /// </summary>
        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            int retInt;
            Cursor tmpCursor = Cursor.Current;
            FileManager fileManagerObj=null;
            ExcellonFile outputExcellonFile = null;
            GerberFile outputGerberFile = null;
            string errStr="";
            DialogResult dlgRes;

            LogMessage("buttonOpenFile_Click called");

            // we show a message once regarding the license
            if (implictUserSettings != null)
            {
                if (implictUserSettings.OKWithDisclaimer == false)
                {
                    frmDisclaimer disFrm = new frmDisclaimer(APPLICATION_VERSION);
                    disFrm.ShowDialog();
                    if (disFrm.OKWithDisclaimer == false) return;
                    implictUserSettings.OKWithDisclaimer = disFrm.OKWithDisclaimer;
                }
            }
            else
            {
                frmDisclaimer disFrm = new frmDisclaimer(APPLICATION_VERSION);
                disFrm.ShowDialog();
                if (disFrm.OKWithDisclaimer == false) return;
            }

            bool retBool = AreAnyGCodeFilesUnsaved();
            if (retBool == true)
            {
#if !DEBUG
                dlgRes = OISMessageBox_YesNo("Some of the generated GCode files is unsaved. Opening a new file will close those files.\n\nDo you wish to continue?");
                if (dlgRes != DialogResult.Yes) return;
#endif
            }

            string filePathAndName="";
            retInt = PickFile(ImplicitUserSettings.LastOpenFileDirectory, ImplicitUserSettings.LastOpenFileName, ref filePathAndName);
            if(retInt<0)
            {
                // log this
                LogMessage("buttonOpenFile_Click call to PickFile was cancelled");
                return;
            }
            if (retInt > 0)
            {
                // log this
                LogMessage("buttonOpenFile_Click call to PickFile returned error of "+retInt.ToString());
                OISMessageBox("Error " + retInt.ToString() + " occurred when choosing the Gerber File.\n\nPlease see the log file.");
                return;
            }
            if ((filePathAndName == null) || (filePathAndName.Length == 0))
            {
                // log this
                LogMessage("buttonOpenFile_Click call to PickFile returned ((filePathAndName == null) || (filePathAndName.Length == 0))");
                OISMessageBox("Error no file name returned when choosing the Gerber File.\n\nPlease see the log file.");
                return;
            }
            LogMessage("Preparing to open file: " + filePathAndName);

            // reset everything
            ResetApplicationForNewFile();

            // if we get here the pick was successful. Add the file to the MRU list
            MRUList.AddFileNameToTop(filePathAndName);
            // add the directory to our configuration settings so we return to 
            // it automatically next time we start
            ImplicitUserSettings.LastOpenFileDirectory = Path.GetDirectoryName(filePathAndName);
            ImplicitUserSettings.LastOpenFileName = Path.GetFileName(filePathAndName);

            // now get the FileManager object for the specified File
            fileManagerObj = ctlFileManagersDisplay1.GetMatchingFileManager(filePathAndName, true);
            if ((fileManagerObj == null) || (fileManagerObj.OperationMode == FileManager.OperationModeEnum.Default))
            {
                // lets check to see if we can guess the design tool that output this
                List<FileManager.KnownDesignTool> toolList = FileManager.CheckExtensionForKnownToolType(filePathAndName);
                // did we get one?
                if (toolList.Count > 0)
                {
                    // set this now. Maybe later get a bit more sophisticated and offer a pick box if there are multiples
                    FileManager.KnownDesignTool currentTool = toolList[0];
                    dlgRes = OISMessageBox_YesNo("No File Manager could be found for this file. File Managers contain the configuration options for the file.\n\nIt looks like you are using " + currentTool.ToString() + ". Would you like to create a set of default File Managers for this software now?");
                    if (dlgRes == DialogResult.Yes)
                    {
                        if ((currentTool is FileManager.KnownDesignTool.DESIGN_SPARK) == true)
                        {
                            // simulate a press on this button
                            buttonQuickSetupDesignSpark_Click(this, new EventArgs());
                        }
                        else if ((currentTool is FileManager.KnownDesignTool.KICAD) == true)
                        {
                            // simulate a press on this button
                            buttonQuickSetupKiCad_Click(this, new EventArgs());
                        }
                        else if ((currentTool is FileManager.KnownDesignTool.EASY_EDA) == true)
                        {
                            // simulate a press on this button
                            buttonQuickSetupEasyEDA_Click(this, new EventArgs());
                        }
                        else
                        {
                            // should never happen
                            OISMessageBox("There was an error creating the File Manager. File Managers Not Found.\n\nPlease see the log file.");
                            return;
                        }
                    }
                }
            }

            // try again. now get the FileManager object for the specified File
            fileManagerObj = ctlFileManagersDisplay1.GetMatchingFileManager(filePathAndName, true);
            if ((fileManagerObj == null) || (fileManagerObj.OperationMode == FileManager.OperationModeEnum.Default))
            {
                dlgRes = OISMessageBox_YesNo("No File Manager could be found for this file. File Managers contain the configuration options for the file.\n\nWould you like to create a new File Manager now?");
                if (dlgRes != DialogResult.Yes) return;

                // ask the user what type of file manager they would like to use
                frmFileManagerChooser optChooser = new frmFileManagerChooser(ctlFileManagersDisplay1.GetDefaultFileManagerObject());
                // this is modal
                optChooser.ShowDialog();
                if (optChooser.DialogResult != DialogResult.OK) return;
                fileManagerObj = optChooser.OutputFileManagerObject;
                if (fileManagerObj == null)
                {
                    // should never happen
                    OISMessageBox("There was an error creating the File Manager.\n\nPlease see the log file.");
                    return;
                }
                else
                {
                    // populate a few more properties and add it
                    fileManagerObj.FilenamePattern = Path.GetExtension(filePathAndName);
                    fileManagerObj.Description = "AutoGenerated";
                    if (fileManagerObj.OperationMode == FileManager.OperationModeEnum.IsolationCut)
                    {
                        fileManagerObj.ReferencePinGCodeEnabled = false;
                    }
                    ctlFileManagersDisplay1.AddFileManager(fileManagerObj);
                    // tell the user
                    OISMessageBox("A new "+fileManagerObj.OperationMode.ToString()+ " File Manager for files of type >" + fileManagerObj.FilenamePattern + "< has now been created on the Settings Tab.\n\nThe configuation options in this File Manager will be at their defaults and should be reviewed before running any GCode it generates.");
                }
            }
            if ((fileManagerObj == null) || (fileManagerObj.OperationMode == FileManager.OperationModeEnum.Default))
            {
                OISMessageBox("Could not find File Manager object for this file. No operations will be possible.\n\nFix this by configuring a File Manager on the Settings tab.");
                return;
            }
            // set this now
            CurrentFileManager = fileManagerObj;
            // set the properties panel on the settings so the user sees this file manager open
            ctlFileManagersDisplay1.SetSelectedManagerByName(fileManagerObj.FilenamePattern);
            // note it down here too
            textBoxActiveFileManager.Text = fileManagerObj.FilenamePattern;
            LogMessage("OpenFile, found matching manager:" + CurrentFileManager.FilenamePattern);

            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now open the file we returned
                if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.Excellon)
                {
                    // we assume the file is an excellon file
                    retInt = OpenExcellonFile(filePathAndName, fileManagerObj, out outputExcellonFile, false, ref errStr);
                }
                else if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.BoardEdgeMill)
                {
                    // assume this is a gerber file
                    retInt = OpenGerberFile(filePathAndName, fileManagerObj, out outputGerberFile, false, ref errStr);
                }
                else if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.IsolationCut)
                {
                    // assume this is a gerber file
                    retInt = OpenGerberFile(filePathAndName, fileManagerObj, out outputGerberFile, false, ref errStr);
                }
                else
                {
                    OISMessageBox("Unsupported Operation Mode in File Manager.\n\nCannot open this file.");
                    return;
                }
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonOpenFile_Click call to OpenGerberorExcellonFile returned " + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred opening the File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be opened because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }

            // set up the form visuals appropriate to the operating mode
            if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.Excellon)
            {
                if (outputExcellonFile == null)
                {
                    // we found something we could not cope with
                    OISMessageBox("Sorry, the Excellon file could not be opened because of errors\n\nPlease see the logs.");
                    return;
                }
                if (outputExcellonFile.SourceLines.Count==0)
                {
                    // we found something we could not cope with
                    OISMessageBox("The Excellon file appears to be empty.\n\nYou may wish to inspect that file.");
                    return;
                }
                CurrentExcellonFile = outputExcellonFile;

                // set the plot origin at zero, and compensate the maxX, maxY by our Origin adjustments
                ClearPlotViewBitmap();
                ctlPlotViewer1.GerberFileToDisplay = null;
                ctlPlotViewer1.ExcellonFileToDisplay = CurrentExcellonFile;
                ctlPlotViewer1.MagnificationLevel = ctlPlotViewer.DEFAULT_MAGNIFICATION_LEVEL;
                ctlPlotViewer1.ScreenUnits = CurrentExcellonFile.ExcellonFileUnits;
                ctlPlotViewer1.IsoPlotPointsPerAppUnit = GetIsoPlotPointsPerAppUnit(CurrentExcellonFile.ExcellonFileUnits);
                ctlPlotViewer1.GcodeOriginAtCenter = CurrentExcellonFile.GCodeOriginAtCenter;
                ctlPlotViewer1.SetPlotObjectLimits(0, 0, CurrentExcellonFile.MaxPlotXCoord, CurrentExcellonFile.MaxPlotYCoord, CurrentExcellonFile.MidPlotXCoord, CurrentExcellonFile.MidPlotYCoord);
                ctlPlotViewer1.MagnificationLevel = ctlPlotViewer.DEFAULT_MAGNIFICATION_LEVEL;
                ctlPlotViewer1.SetScrollBarMaxMinLimits();
                ctlPlotViewer1.ShowPlot();

                // now set screen display
                radioButtonMainViewGerberPlot.Checked = true;
                tabControl1.SelectedTab = tabPagePlot;
                richTextBoxExcellonCode.Lines = CurrentExcellonFile.GetRawSourceLinesAsArray();
                textBoxOpenExcellonFileName.Text = filePathAndName;
                SetStatusLine(Path.GetFileName(filePathAndName));
                SyncFormVisualsToGerberExcellonAndGCodeState();

                if (CurrentExcellonFile.ExcellonFileManager.AutoGenerateGCode == false)
                {
                    // we are done, say all is well
                    //OISMessageBox("The Excellon file opened successfully.");
                }
                else
                {
                    // carry on with the conversion
                    buttonConvertToGCode_Click(this, new EventArgs());
                }
            }
            else
            {

                if (outputGerberFile == null)
                {
                    // we found something we could not cope with
                    OISMessageBox("Sorry, the Gerber file could not be opened because of errors\n\nPlease see the logs.");
                    return;
                }
                if (outputGerberFile.SourceLines.Count==0)
                {
                    // we found something we could not cope with
                    OISMessageBox("The Gerber file appears to be empty.\n\nYou may wish to inspect that file.");
                    return;
                }
                CurrentGerberFile = outputGerberFile;

                // set the plot origin at zero, and compensate the maxX, maxY by our Origin adjustments
                ClearPlotViewBitmap();
                ctlPlotViewer1.MagnificationLevel = ctlPlotViewer.DEFAULT_MAGNIFICATION_LEVEL;
                ctlPlotViewer1.GerberFileToDisplay = outputGerberFile;
                ctlPlotViewer1.ScreenUnits = CurrentGerberFile.GerberFileUnits;
                ctlPlotViewer1.IsoPlotPointsPerAppUnit = GetIsoPlotPointsPerAppUnit(CurrentGerberFile.GerberFileUnits);
                ctlPlotViewer1.GcodeOriginAtCenter = outputGerberFile.GCodeOriginAtCenter;
                ctlPlotViewer1.SetPlotObjectLimits(0, 0, outputGerberFile.MaxPlotXCoord, outputGerberFile.MaxPlotYCoord, outputGerberFile.MidPlotXCoord, outputGerberFile.MidPlotYCoord);                
                ctlPlotViewer1.SetScrollBarMaxMinLimits();
                ctlPlotViewer1.ShowPlot();

                // now set screen display
                radioButtonMainViewGerberPlot.Checked = true;
                tabControl1.SelectedTab = tabPagePlot;
                richTextBoxGerberCode.Lines = CurrentGerberFile.GetRawSourceLinesAsArray();
                textBoxOpenGerberFileName.Text = filePathAndName;
                SetStatusLine(Path.GetFileName(filePathAndName));
                SyncFormVisualsToGerberExcellonAndGCodeState();
                ctlPlotViewer1.SetScrollBarMaxMinLimits();

                // set the settings enabled state
                SyncSettingsEnabledStateToReality(); 

                if (CurrentGerberFile.GerberFileManager.AutoGenerateGCode == false)
                {
#if !DEBUG
                    // we are done, say all is well
                    //OISMessageBox("The Gerber file opened successfully.");
#endif
                }
                else
                {
                    // carry on with the conversion
                    buttonConvertToGCode_Click(this, new EventArgs());
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the open recent file button
        /// </summary>
        private void buttonRecentFiles_Click(object sender, EventArgs e)
        {
            string errStr = "";
            FileManager fileManagerObj = null;
            ExcellonFile outputExcellonFile = null;
            GerberFile outputGerberFile = null;
            DialogResult dlgRes;
            string filePathAndName = "";
            int retInt;

            LogMessage("buttonRecentFiles_Click called");

            bool retBool = AreAnyGCodeFilesUnsaved();
            if (retBool == true)
            {
                dlgRes = OISMessageBox_YesNo("Some of the generated GCode files is unsaved. Opening a new file will close those files.\n\nDo you wish to continue?");
                if (dlgRes != DialogResult.Yes) return;
            }

            Cursor tmpCursor = Cursor.Current;

            // Show it modal
            MRUList.ShowDialog();
            // did the user cancel
            dlgRes = MRUList.DialogResult;
            if (dlgRes != DialogResult.OK)
            {
                // not an error, return quietly
                LogMessage(" buttonRecentFiles_Click - user cancelled open");
                return;
            }
            // did we get a file?
            filePathAndName = MRUList.SelectedFileName;
            if ((filePathAndName == null) || (filePathAndName.Length == 0))
            {
                LogMessage("OpenRecentFile - (filePathAndName == null) || (filePathAndName.Length == 0)");
                OISMessageBox("Error no file returned by the MRU List.\n\nPlease see the log file.");
                return;
            }

            LogMessage("OpenRecentFile - preparing to open file>" + filePathAndName + "<");
            LogMessage("Preparing to open file: " + filePathAndName);

            // reset everything
            ResetApplicationForNewFile();

            // if we get here the pick was successful. Add the file to the MRU list
            MRUList.AddFileNameToTop(filePathAndName);
            // add the directory to our configuration settings so we return to 
            // it automatically next time we start
            ImplicitUserSettings.LastOpenFileDirectory = Path.GetDirectoryName(filePathAndName);
            ImplicitUserSettings.LastOpenFileName = Path.GetFileName(filePathAndName);

            // now get the FileManager object for the specified File
            fileManagerObj = ctlFileManagersDisplay1.GetMatchingFileManager(filePathAndName, true);
            if ((fileManagerObj == null) || (fileManagerObj.OperationMode == FileManager.OperationModeEnum.Default))
            {
                dlgRes = OISMessageBox_YesNo("No File Manager could be found for this file. File Managers contain the configuration options for the file.\n\nWould you like to create a new File Manager now?");
                if (dlgRes != DialogResult.Yes) return;

                // ask the user what type of file manager they would like to use
                frmFileManagerChooser optChooser = new frmFileManagerChooser(ctlFileManagersDisplay1.GetDefaultFileManagerObject());
                // this is modal
                optChooser.ShowDialog();
                if (optChooser.DialogResult != DialogResult.OK) return;
                fileManagerObj = optChooser.OutputFileManagerObject;
                if (fileManagerObj == null)
                {
                    // should never happen
                    OISMessageBox("There was an error creating the File Manager.\n\nPlease see the log file.");
                    return;
                }
                else
                {
                    // populate a few more properties and add it
                    fileManagerObj.FilenamePattern = Path.GetExtension(filePathAndName);
                    fileManagerObj.Description = "AutoGenerated";
                    if (fileManagerObj.OperationMode == FileManager.OperationModeEnum.IsolationCut)
                    {
                        fileManagerObj.ReferencePinGCodeEnabled = false;
                    }
                    ctlFileManagersDisplay1.AddFileManager(fileManagerObj);
                    // tell the user
                    OISMessageBox("A new " + fileManagerObj.OperationMode.ToString() + " File Manager for files of type >" + fileManagerObj.FilenamePattern + "< has now been created on the Settings Tab.\n\nThe configuation options in this File Manager will be at their defaults and should be reviewed before running any GCode it generates.");
                }
            }
            if ((fileManagerObj == null) || (fileManagerObj.OperationMode == FileManager.OperationModeEnum.Default))
            {
                OISMessageBox("Could not find File Manager object for this file. No operations will be possible.\n\nFix this by configuring a File Manager on the Settings tab.");
                return;
            }
            // set this now
            CurrentFileManager = fileManagerObj;
            textBoxActiveFileManager.Text = fileManagerObj.FilenamePattern;
            LogMessage("OpenFile, found matching manager:" + CurrentFileManager.FilenamePattern);

            // set the cursor, this can take some time
            tmpCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // now open the file we returned
                if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.Excellon)
                {
                    // we assume the file is an excellon file
                    retInt = OpenExcellonFile(filePathAndName, fileManagerObj, out outputExcellonFile, false, ref errStr);
                }
                else
                {
                    // we assume the file is a gerber file
                    retInt = OpenGerberFile(filePathAndName, fileManagerObj, out outputGerberFile, false, ref errStr);
                }
                if (retInt != 0)
                {
                    // log this
                    LogMessage("buttonOpenFile_Click call to OpenGerberorExcellonFile returned " + retInt.ToString());
                    // yes it did, was it reported?
                    if (retInt < 0)
                    {
                        // no it was not, we always report it
                        OISMessageBox("Error " + retInt.ToString() + " occurred opening the File.\n\nPlease see the log file.");
                    }
                    return;
                }
            }
            catch (NotImplementedException ex)
            {
                // we found something we could not cope with
                OISMessageBox("Sorry, this file cannot be opened because some of its contents are not yet supported. Error message was:\n\n" + ex.Message);
                return;
            }
            finally
            {
                // restore the cursor back to what it was
                Cursor.Current = tmpCursor;
            }

            // set up the form visuals appropriate to the operating mode
            if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.Excellon)
            {
                if (outputExcellonFile == null)
                {
                    // we found something we could not cope with
                    OISMessageBox("Sorry, the Excellon file could not be opened because of errors\n\nPlease see the logs.");
                    return;
                }
                if (outputExcellonFile.SourceLines.Count == 0)
                {
                    // we found something we could not cope with
                    OISMessageBox("The Excellon file appears to be empty.\n\nYou may wish to inspect that file.");
                    return;
                }
                CurrentExcellonFile = outputExcellonFile;

                // set the plot origin at zero, and compensate the maxX, maxY by our Origin adjustments
                ClearPlotViewBitmap();
                ctlPlotViewer1.MagnificationLevel = ctlPlotViewer.DEFAULT_MAGNIFICATION_LEVEL;
                ctlPlotViewer1.GerberFileToDisplay = null;
                ctlPlotViewer1.ScreenUnits = CurrentExcellonFile.ExcellonFileUnits; ;
                ctlPlotViewer1.IsoPlotPointsPerAppUnit = GetIsoPlotPointsPerAppUnit(CurrentExcellonFile.ExcellonFileUnits);
                ctlPlotViewer1.GcodeOriginAtCenter = CurrentExcellonFile.GCodeOriginAtCenter;
                ctlPlotViewer1.SetPlotObjectLimits(0, 0, CurrentExcellonFile.MaxPlotXCoord, CurrentExcellonFile.MaxPlotYCoord, CurrentExcellonFile.MidPlotXCoord, CurrentExcellonFile.MidPlotYCoord);
                ctlPlotViewer1.SetScrollBarMaxMinLimits();
                ctlPlotViewer1.ShowPlot();

                // now set screen display
                radioButtonNoPlot.Checked = true;
                tabControl1.SelectedTab = tabPagePlot;
                richTextBoxExcellonCode.Lines = CurrentExcellonFile.GetRawSourceLinesAsArray();
                textBoxOpenExcellonFileName.Text = filePathAndName;
                SetStatusLine(Path.GetFileName(filePathAndName));
                SyncFormVisualsToGerberExcellonAndGCodeState();

                // say all is well
                //OISMessageBox("The Excellon file opened successfully.");

            }
            else
            {

                if (outputGerberFile == null)
                {
                    // we found something we could not cope with
                    OISMessageBox("Sorry, the Gerber file could not be opened because of errors\n\nPlease see the logs.");
                    return;
                }
                if (outputGerberFile.SourceLines.Count == 0)
                {
                    // we found something we could not cope with
                    OISMessageBox("The Gerber file appears to be empty.\n\nYou may wish to inspect that file.");
                    return;
                }
                CurrentGerberFile = outputGerberFile;


                // set the plot origin at zero, and compensate the maxX, maxY by our Origin adjustments
                ClearPlotViewBitmap();
                ctlPlotViewer1.MagnificationLevel = ctlPlotViewer.DEFAULT_MAGNIFICATION_LEVEL;
                ctlPlotViewer1.GerberFileToDisplay = outputGerberFile;
                ctlPlotViewer1.ScreenUnits = CurrentGerberFile.GerberFileUnits;
                ctlPlotViewer1.IsoPlotPointsPerAppUnit = GetIsoPlotPointsPerAppUnit(CurrentGerberFile.GerberFileUnits);
                ctlPlotViewer1.GcodeOriginAtCenter = outputGerberFile.GCodeOriginAtCenter;
                ctlPlotViewer1.SetPlotObjectLimits(0, 0, outputGerberFile.MaxPlotXCoord, outputGerberFile.MaxPlotYCoord, outputGerberFile.MidPlotXCoord, outputGerberFile.MidPlotYCoord);
                ctlPlotViewer1.SetScrollBarMaxMinLimits();
                ctlPlotViewer1.ShowPlot();

                // now set screen display
                radioButtonMainViewGerberPlot.Checked = true;
                tabControl1.SelectedTab = tabPagePlot;
                richTextBoxGerberCode.Lines = CurrentGerberFile.GetRawSourceLinesAsArray();
                textBoxOpenGerberFileName.Text = filePathAndName;
                SetStatusLine(Path.GetFileName(filePathAndName));
                SyncFormVisualsToGerberExcellonAndGCodeState();
                ctlPlotViewer1.SetScrollBarMaxMinLimits();

                // say all is well
#if !DEBUG
                //OISMessageBox("The Gerber file opened successfully.");
#endif
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the convert Gerber to GCode button
        /// </summary>
        private void buttonConvertToGCode_Click(object sender, EventArgs e)
        {
            TimeSpan timeTaken;
            int retInt;
            string errStr="";
            Cursor tmpCursor = Cursor.Current;
            GCodeFile outGCodeFileObj = null;
            IsoPlotBuilder outIsoPlotBuilderObj = null;

            LogMessage("buttonConvertToGCode_Click called");

            // start timer
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // reset things
            ResetApplicationForNewConvertToGCode();

            // perform the conversion now
            if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.IsolationCut)
            {
                // we require this here
                if (IsValidGerberFileOpen() == false)
                {
                    OISMessageBox("No Gerber File Is Open");
                    return;
                }

                // we have to have one of these enabled - or there is no point
                if (((CurrentFileManager.IsoCutGCodeEnabled == true) || (CurrentFileManager.ReferencePinGCodeEnabled == true))==false)
                {
                    OISMessageBox("The File Manager does not have either of IsoCutGCodeEnabled or ReferencePinGCodeEnabled. No GCode can be created.\n\nYou should consider adjusting the File Manager.");
                    return;
                }

                if (CurrentFileManager.IsoCutGCodeEnabled == true)
                {
                    // set the cursor, this can take some time
                    tmpCursor = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        // now create GCodes we need to set our toolhead parameters here
                        CurrentGerberFile.StateMachine.ToolHeadSetup = new ToolHeadParameters(4, CurrentFileManager, ToolHeadParametersModeEnum.ISOCUT);
                        retInt = CreateIsolationCutGCode(CurrentFileManager, CurrentGerberFile, out outGCodeFileObj, out outIsoPlotBuilderObj, ref errStr);
                    }
                    finally
                    {
                        // restore the cursor back to what it was
                        Cursor.Current = tmpCursor;
                    }
                    if (retInt < 0)
                    {
                        // not reported, report it now
                        OISMessageBox("Error " + retInt.ToString() + " returned when creating GCode file. Error text is>" + errStr + "<\n\nPlease see the log file.");
                        return;
                    }
                    else if (retInt > 0)
                    {
                        // it is reported, just log it
                        LogMessage("buttonConvertToGCode_Click: CreateIsolationCutGCode returned " + retInt.ToString() + " returned when creating GCode file. Error text is>" + errStr + "<");
                        return;
                    }
                    if (outGCodeFileObj == null)
                    {
                        LogMessage("buttonConvertToGCode_Click:  isostage outGCodeFileObj == null");
                        return;
                    }
                    if (outIsoPlotBuilderObj == null)
                    {
                        LogMessage("buttonConvertToGCode_Click: outIsoPlotBuilderObj == null");
                        return;
                    }
                    // set it now
                    CurrentIsolationGCodeFile = outGCodeFileObj;
                    CurrentIsoPlotBuilder = outIsoPlotBuilderObj;
                    CurrentIsolationGCodeFile.HasBeenSaved = false;
                    // we need to set our toolhead parameters here
                    CurrentIsolationGCodeFile.StateMachine.ToolHeadSetup = new ToolHeadParameters(5, CurrentFileManager, ToolHeadParametersModeEnum.ISOCUT);
                    richTextBoxIsolationGCode.Text = CurrentIsolationGCodeFile.GetGCodeCmdsAsText().ToString();
                } // bottom of if (CurrentFileManager.IsoCutGCodeEnabled == true)

                // stop the clock
                timer.Stop();
                timeTaken = timer.Elapsed;
                string isoCutElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
                LogMessage("GCode Conversion: isoCut elapsed time is " + isoCutElapsedTime);

                timer = Stopwatch.StartNew();

                // test to see if we should also build a Reference Pin GCode file, if we did not find reference pins we ignore this
                // the user will already have been warned and and accepted that they cannot generate this
                if ((CurrentFileManager.ReferencePinGCodeEnabled == true) && (CurrentGerberFile.StateMachine.ReferencePinsFound==true))
                {
                    // set the cursor, this can take some time
                    tmpCursor = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        // now create GCodes we need to set our toolhead parameters here
                        CurrentGerberFile.StateMachine.ToolHeadSetup = new ToolHeadParameters(6, CurrentFileManager, ToolHeadParametersModeEnum.REFPINS);
                        // now create GCodes
                        retInt = CreateReferencePinGCode(CurrentFileManager, CurrentGerberFile, out outGCodeFileObj, ref errStr);
                    }
                    finally
                    {
                        // restore the cursor back to what it was
                        Cursor.Current = tmpCursor;
                    }
                    if (retInt < 0)
                    {
                        // not reported, report it now
                        OISMessageBox("Error " + retInt.ToString() + " returned when creating Reference Pin GCode file. Error text is>" + errStr + "<\n\nPlease see the log file.");
                        return;
                    }
                    else if (retInt > 0)
                    {
                        // it is reported, just log it
                        LogMessage("buttonConvertToGCode_Click: Create Reference Pin GCode returned " + retInt.ToString() + " returned when creating GCode file. Error text is>" + errStr + "<");
                        return;
                    }
                    if (outGCodeFileObj == null)
                    {
                        LogMessage("buttonConvertToGCode_Click:  refpinstage outGCodeFileObj == null");
                        OISMessageBox("Error converting Gerber file.\n\nPlease see the logs");
                        return;
                    }
                    // set it now
                    CurrentReferencePinGCodeFile = outGCodeFileObj;
                    CurrentReferencePinGCodeFile.HasBeenSaved = false;
                    // we need to set our toolhead parameters here
                    CurrentReferencePinGCodeFile.StateMachine.ToolHeadSetup = new ToolHeadParameters(10, CurrentFileManager, ToolHeadParametersModeEnum.REFPINS);
                    richTextBoxRefPinGCode.Text = CurrentReferencePinGCodeFile.GetGCodeCmdsAsText().ToString();

                    // stop the clock
                    timer.Stop();
                    timeTaken = timer.Elapsed;
                    string refPinElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
                    LogMessage("GCode Conversion: refPin elapsed time is " + refPinElapsedTime);

                    timer = Stopwatch.StartNew();

                } // if (CurrentFileManager.ReferencePinGCodeEnabled == true)
            } // bottom of if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.IsolationCut)
            else if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.BoardEdgeMill)
            {
                // we require this here
                if (IsValidGerberFileOpen() == false)
                {
                    OISMessageBox("No Gerber File Is Open");
                    return;
                }

                // we have to have one of these enabled - or there is no point
                if (((CurrentFileManager.EdgeMillingGCodeEnabled == true) || (CurrentFileManager.BedFlatteningGCodeEnabled == true))==false)
                {
                    OISMessageBox("The File Manager does not have either of EdgeMillingGCodeEnabled or BedFlatteningGCodeEnabled. No GCode can be created.\n\nYou should consider adjusting the File Manager.");
                    return;
                }
                // cannot do bed flattening with out edgemilling enabled
                if ((CurrentFileManager.EdgeMillingGCodeEnabled == false) && (CurrentFileManager.BedFlatteningGCodeEnabled == true))
                {
                    OISMessageBox("The File Manager does not have EdgeMillingGCodeEnabled. BedFlattening GCode cannot be generated without first generating the EdgeMilling GCode.\n\nYou should consider adjusting the File Manager.");
                    return;
                }
                if (CurrentFileManager.EdgeMillingGCodeEnabled == true)
                {
                    // this also uses the isolation GCodem converter, set the cursor, this can take some time
                    tmpCursor = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        // now create GCodes, we need to set our toolhead parameters here
                        CurrentGerberFile.StateMachine.ToolHeadSetup = new ToolHeadParameters(11, CurrentFileManager, ToolHeadParametersModeEnum.EDGEMILL);
                        retInt = CreateIsolationCutGCode(CurrentFileManager, CurrentGerberFile, out outGCodeFileObj, out outIsoPlotBuilderObj, ref errStr);
                    }
                    finally
                    {
                        // restore the cursor back to what it was
                        Cursor.Current = tmpCursor;
                    }
                    if (retInt < 0)
                    {
                        // not reported, report it now
                        OISMessageBox("Error " + retInt.ToString() + " returned when creating GCode file. Error text is>" + errStr + "<\n\nPlease see the log file.");
                        return;
                    }
                    else if (retInt > 0)
                    {
                        // it is reported, just log it
                        LogMessage("buttonConvertToGCode_Click: CreateIsolationCutGCode returned " + retInt.ToString() + " returned when creating GCode file. Error text is>" + errStr + "<");
                        return;
                    }
                    if (outGCodeFileObj == null)
                    {
                        LogMessage("buttonConvertToGCode_Click: outGCodeFileObj == null");
                        OISMessageBox("Error converting Gerber file.\n\nPlease see the logs");
                        return;
                    }
                    if (outIsoPlotBuilderObj == null)
                    {
                        LogMessage("buttonConvertToGCode_Click: outIsoPlotBuilderObj == null");
                        OISMessageBox("Error converting Gerber file.\n\nPlease see the logs");
                        return;
                    }
                    // set it now
                    CurrentEdgeMillGCodeFile = outGCodeFileObj;
                    CurrentIsoPlotBuilder = outIsoPlotBuilderObj;
                    CurrentEdgeMillGCodeFile.HasBeenSaved = false;
                    // we need to set our toolhead parameters here
                    CurrentEdgeMillGCodeFile.StateMachine.ToolHeadSetup = new ToolHeadParameters(12, CurrentFileManager, ToolHeadParametersModeEnum.EDGEMILL);
                    richTextBoxEdgeMillGCode.Text = CurrentEdgeMillGCodeFile.GetGCodeCmdsAsText().ToString();
                }

                // stop the clock
                timer.Stop();
                timeTaken = timer.Elapsed;
                string edgeMillElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
                LogMessage("GCode Conversion: edgeMill elapsed time is " + edgeMillElapsedTime);

                timer = Stopwatch.StartNew();

                // test to see if we should also build a Bed Flattening GCode file
                if (CurrentFileManager.BedFlatteningGCodeEnabled == true)
                {
                    // set the cursor, this can take some time
                    tmpCursor = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        // now create GCodes, we need to set our toolhead parameters here
                        CurrentGerberFile.StateMachine.ToolHeadSetup = new ToolHeadParameters(13, CurrentFileManager, ToolHeadParametersModeEnum.BEDFLATTENING);
                        retInt = CreateBedFlatteningGCode(CurrentFileManager, CurrentGerberFile, CurrentEdgeMillGCodeFile, out outGCodeFileObj, ref errStr);
                    }
                    finally
                    {
                        // restore the cursor back to what it was
                        Cursor.Current = tmpCursor;
                    }
                    if (retInt < 0)
                    {
                        // not reported, report it now
                        OISMessageBox("Error " + retInt.ToString() + " returned when creating Bed Flattening GCode file. Error text is>" + errStr + "<\n\nPlease see the log file.");
                        return;
                    }
                    else if (retInt > 0)
                    {
                        // it is reported, just log it
                        LogMessage("buttonConvertToGCode_Click: CreateBedFlatteningGCode returned " + retInt.ToString() + " returned when creating GCode file. Error text is>" + errStr + "<");
                        return;
                    }
                    // set it now
                    CurrentBedFlatteningGCodeFile = outGCodeFileObj;
                    CurrentBedFlatteningGCodeFile.HasBeenSaved = false;
                    // we need to set our toolhead parameters here
                    CurrentBedFlatteningGCodeFile.StateMachine.ToolHeadSetup = new ToolHeadParameters(38, CurrentFileManager, ToolHeadParametersModeEnum.BEDFLATTENING);
                    richTextBoxBedFlatteningGCode.Text = CurrentBedFlatteningGCodeFile.GetGCodeCmdsAsText().ToString();
                }

                // stop the clock
                timer.Stop();
                timeTaken = timer.Elapsed;
                string bedFlatElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
                LogMessage("GCode Conversion: bedFlat elapsed time is " + bedFlatElapsedTime);

            }
            else if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.Excellon)
            {
                // we require this here
                if (IsValidExcellonFileOpen() == false)
                {
                    OISMessageBox("No Excellon File Is Open");
                    return;
                }
                // we have to have one of these enabled - or there is no point
                if (CurrentFileManager.DrillingGCodeEnabled ==false)
                {
                    OISMessageBox("The File Manager does not have DrillingGCodeEnabled. No GCode can be created.\n\nYou should consider adjusting the File Manager.");
                    return;
                }

                // set the cursor, this can take some time
                tmpCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    // now create the GCodes, we need to set our toolhead parameters here
                    CurrentExcellonFile.StateMachine.ToolHeadSetup = new ToolHeadParameters(14, CurrentFileManager, ToolHeadParametersModeEnum.EXCELLONDRILL);
                    retInt = CreateDrillGCode(CurrentFileManager, CurrentExcellonFile, out outGCodeFileObj, ref errStr);
                }
                finally
                {
                    // restore the cursor back to what it was
                    Cursor.Current = tmpCursor;
                }
                if (retInt < 0)
                {
                    // not reported, report it now
                    OISMessageBox("Error " + retInt.ToString() + " returned when creating Drill GCode file. Error text is>" + errStr + "<\n\nPlease see the log file.");
                    return;
                }
                else if (retInt > 0)
                {
                    // it is reported, just log it
                    LogMessage("buttonConvertToGCode_Click: CreateDrillGCode returned " + retInt.ToString() + " returned when creating GCode file. Error text is>" + errStr + "<");
                    return;
                }
                if (outGCodeFileObj == null)
                {
                    LogMessage("buttonConvertToGCode_Click: outGCodeFileObj == null");
                    OISMessageBox("Error converting Excellon file.\n\nPlease see the logs");
                    return;
                }
                // set it now
                CurrentDrillGCodeFile = outGCodeFileObj;
                CurrentDrillGCodeFile.HasBeenSaved = false;
                // we need to set our toolhead parameters here
                CurrentDrillGCodeFile.StateMachine.ToolHeadSetup = new ToolHeadParameters(15, CurrentFileManager, ToolHeadParametersModeEnum.EXCELLONDRILL);
                richTextBoxDrillGCode.Text = CurrentDrillGCodeFile.GetGCodeCmdsAsText().ToString();

                // stop the clock
                timer.Stop();
                timeTaken = timer.Elapsed;
                string drillElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
                LogMessage("GCode Conversion: drill elapsed time is " + drillElapsedTime);

            }
            else if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.TextAndLabels)
            {
            }
            else
            {
                // this means the File Manager was at the defaults. This should not happen
            }

            timer = Stopwatch.StartNew();

            // set this
            ClearPlotViewBitmap();

            // stop the clock
            timer.Stop();
            timeTaken = timer.Elapsed;
            string clearPlotElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
            LogMessage("GCode Conversion:  clearPlot elapsed time is " + clearPlotElapsedTime);

            timer = Stopwatch.StartNew();

#if !DEBUG
            PostSucessfulConversionMessage();
#endif

            SyncFormVisualsToGerberExcellonAndGCodeState();
            // now display the converted file
            if (IsValidIsolationGCodeFileOpen() == true)
            {
                radioButtonMainViewIsoGCodePlot.Checked = true;
            }
            else if (IsValidEdgeMillGCodeFileOpen() == true)
            {
                radioButtonMainViewEdgeMillGCode.Checked = true;
            }
            else if (IsValidBedFlatteningGCodeFileOpen() == true)
            {
                radioButtonMainViewBedFlattenGCode.Checked = true;
            }
            else if (IsValidRefPinGCodeFileOpen() == true)
            {
                // we don't do this
            }
            else if (IsValidDrillGCodeFileOpen() == true)
            {
                radioButtonMainViewDrillGCode.Checked = true;
            }

            SyncFormVisualsToGerberExcellonAndGCodeState();

            // stop the clock
            timer.Stop();
            timeTaken = timer.Elapsed;
            string syncElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
            LogMessage("GCode Conversion: sync elapsed time is " + syncElapsedTime);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Posts a message indicating which files have been converted
        /// </summary>
        private void DisplayConvertedFile()
        {
            StringBuilder sb = new StringBuilder();

            if (IsValidIsolationGCodeFileOpen() == true)
            {
                radioButtonMainViewIsoGCodePlot.Checked = true;
            }
            else if (IsValidEdgeMillGCodeFileOpen() == true)
            {
            }
            else if (IsValidBedFlatteningGCodeFileOpen() == true)
            {
            }
            else if (IsValidRefPinGCodeFileOpen() == true)
            {
            }
            else if (IsValidDrillGCodeFileOpen() == true)
            {
            }
         }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Posts a message indicating which files have been converted
        /// </summary>
        private void PostSucessfulConversionMessage()
        {
            StringBuilder sb = new StringBuilder();

            // set up the lead in text
            sb.Append("The conversion of the Gerber file to GCode was successful. ");

            if (IsValidIsolationGCodeFileOpen() == true)
            {
                sb.Append("An Isolation GCode File");
            }
            else if (IsValidEdgeMillGCodeFileOpen() == true)
            {
                sb.Append("An Edge Mill GCode File");
            }
            else if (IsValidBedFlatteningGCodeFileOpen() == true)
            {
                sb.Append("A Bed Flattening GCode File");
            }
            else if (IsValidRefPinGCodeFileOpen() == true)
            {
                sb.Append("A Reference Pins GCode File");
            }
            else if (IsValidDrillGCodeFileOpen() == true)
            {
                sb.Append("A Drill GCode File");
            }
            sb.Append(" was generated.\n\nA new tab with the GCode text has been created and the output will be displayed in the plot viewer.");

            OISMessageBox(sb.ToString());
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Clear all button
        /// </summary>
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            DialogResult dlgRes = OISMessageBox_YesNo("This option will clear all Gerber and GCode file information.\n\nDo you wish to proceed?");
            if (dlgRes != DialogResult.Yes)
            {
                return;
            }
            // just do it
            this.ResetApplicationForNewFile();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the help button
        /// </summary>
        private void buttonHelp_Click(object sender, EventArgs e)
        {
#if DEBUG
            LogMessage(" buttonHelp_Click");
            ResetApplicationForNewConvertToGCode();
            //LaunchHelpFile(@"C:\Projects\LineGrinder\Help", LINEGRINDER_MAINHELP_FILE);
#else
            LogMessage(" buttonHelp_Click");
            LaunchHelpFile(LINEGRINDER_HELPDIR, LINEGRINDER_MAINHELP_FILE);
#endif

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the close button
        /// </summary>
        private void buttonExit_Click(object sender, EventArgs e)
        {
            // the form_closing handler performs all of the 
            // necessary checks
            this.Close();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the about button
        /// </summary>
        private void buttonAbout_Click(object sender, EventArgs e)
        {
            frmAbout aboutForm = new frmAbout(APPLICATION_VERSION);
            aboutForm.ShowDialog();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tests if any of the generated GCode files need to be saved. And prompts if they do.
        /// </summary>
        /// <returns>user wants to save, false user does not want to save</returns>
        private bool TestIfGCodeFilesHaveBeenSavedAndPromptIfNot()
        {
            bool mustPromptFlag = false;
            StringBuilder sb = new StringBuilder();

            // set up the lead in text
            sb.Append("The following GCode files have been generated but not saved:\n\n");

            if (CurrentIsolationGCodeFile.HasBeenSaved == false)
            {
                sb.Append("Isolation GCode File\n");
                mustPromptFlag = true;
            }
            if (CurrentEdgeMillGCodeFile.HasBeenSaved == false)
            {
                sb.Append("Edge Mill GCode File\n");
                mustPromptFlag = true;
            }
            if (CurrentBedFlatteningGCodeFile.HasBeenSaved == false)
            {
                sb.Append("Bed Flattening GCode File\n");
                mustPromptFlag = true;
            }
            if (CurrentReferencePinGCodeFile.HasBeenSaved == false)
            {
                sb.Append("Reference Pins GCode File\n");
                mustPromptFlag = true;
            }
            if (CurrentDrillGCodeFile.HasBeenSaved == false)
            {
                sb.Append("Drill GCode File\n");
                mustPromptFlag = true;
            }

            if (mustPromptFlag == true)
            {
#if !DEBUG
                sb.Append("\nDo you still wish to close this application?");
                DialogResult dlgRes = OISMessageBox_YesNo(sb.ToString());
                if (dlgRes != DialogResult.Yes) return true;
                return false;
#endif
            }
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tests if any of the generated GCode files need to be saved.
        /// </summary>
        /// <returns>true files are unsaved, false no files are unsaved</returns>
        private bool AreAnyGCodeFilesUnsaved()
        {
            if (CurrentIsolationGCodeFile.HasBeenSaved == false) return true;
            if (CurrentEdgeMillGCodeFile.HasBeenSaved == false) return true;
            if (CurrentBedFlatteningGCodeFile.HasBeenSaved == false) return true;
            if (CurrentReferencePinGCodeFile.HasBeenSaved == false) return true;
            if (CurrentDrillGCodeFile.HasBeenSaved == false) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The form closing handler
        /// </summary>
        private void frmMain1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogMessage("frmMain1_FormClosing called");

            // put the non user specified configuration settings in place now
            SetImplicitUserSettings();

            if (TestIfGCodeFilesHaveBeenSavedAndPromptIfNot() == true)
            {
                LogMessage("frmMain1_FormClosing close cancelled");
                e.Cancel = true;
                return;
            }
            // we always save implicit settings on close, unless the Shift key is pressed
            if ((Control.ModifierKeys & Keys.Shift) == 0)
            {
                LogMessage("frmMain1_FormClosing close ImplicitUserSettings.Save called");
                ImplicitUserSettings.Save();
            }
            if (DoWeNeedToSaveExplicitUserSettings() == true)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("The configuration options have changed.\n\nDo you wish to save them?");
                if (dlgRes == DialogResult.Yes)
                {
                    LogMessage("frmMain1_FormClosing close SetUserImplicitUserSettings() called");
                    WriteExplicitUserSettings(false);
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a mouse wheel event
        /// </summary>
        private void frmMain_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // which tab is current
            if (tabControl1.SelectedTab == tabPagePlot)
            {
                // on the plot tab we just send the event to the ctlPlotViewer
                ctlPlotViewer1.HandleMouseWheelEvent(this, e);
                // sync this
                SyncMagnificationOnScreenToPlotViewer();
            }
            else
            {
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Quick Setup (DesignSpark) button
        /// </summary>
        private void buttonQuickSetupDesignSpark_Click(object sender, EventArgs e)
        {
            FileManager mgrObj;
            FileManager defMgrObj;

            // get the default manager
            defMgrObj = ctlFileManagersDisplay1.GetDefaultFileManagerObject();

            // now create the bottom copper
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.IsolationCut;
            mgrObj.IsoFlipMode = FlipModeEnum.X_Flip;
            mgrObj.ReferencePinGCodeEnabled = false;
            mgrObj.FilenamePattern = FileManager.KNOWN_EXT_BOTCOPPER_DSPARK;
            mgrObj.Description = "DesignSpark Bottom Layer";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the top copper
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.IsolationCut;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = FileManager.KNOWN_EXT_TOPCOPPER_DSPARK;
            mgrObj.Description = "DesignSpark Top Layer";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the board outline
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.BoardEdgeMill;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = FileManager.KNOWN_EXT_EDGECUT_DSPARK;
            mgrObj.Description = "DesignSpark Board Outline: This plot must be manually added in DesignSpark";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the excellon
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.Excellon;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = FileManager.KNOWN_EXT_EXEL_DRILL_DSPARK;
            mgrObj.DrillingNumberOfDecimalPlaces = 0;
            mgrObj.Description = "DesignSpark Excellon Drill file.";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            OISMessageBox("Template File Managers suitable for DesignSpark have been added.\n\nThe parameters will be at their default settings. You should check them to be sure they are set appropriate to your requirements.");

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Quick Setup (Eagle) button
        /// </summary>
        private void buttonQuickSetupEagle_Click(object sender, EventArgs e)
        {
            FileManager mgrObj;
            FileManager defMgrObj;

            // get the default manager
            defMgrObj = ctlFileManagersDisplay1.GetDefaultFileManagerObject();

            // now create the bottom copper
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.IsolationCut;
            mgrObj.IsoFlipMode = FlipModeEnum.X_Flip;
            mgrObj.ReferencePinGCodeEnabled = false;
            mgrObj.FilenamePattern = "copper_bottom.gbr";
            mgrObj.Description = "Eagle Bottom Layer";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the top copper
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.IsolationCut;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = "copper_top.gbr";
            mgrObj.Description = "Eagle Top Layer";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the board outline
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.BoardEdgeMill;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = "profile.gbr";
            mgrObj.Description = "Eagle Board Outline";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the excellon
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.Excellon;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = ".xln";
            mgrObj.Description = "Eagle Excellon Drill file.";
            mgrObj.DrillingCoordinateZerosMode = FileManager.ExcellonDrillingCoordinateZerosModeEnum.FixedDecimalPoint;
            mgrObj.DrillingNumberOfDecimalPlaces = 3;
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            OISMessageBox("Template File Managers suitable for Eagle  have been added.\n\nThe parameters will be at their default settings. You should check them to be sure they are set appropriate to your requirements.");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Quick Setup (EasyEDA) button
        /// </summary>
        private void buttonQuickSetupEasyEDA_Click(object sender, EventArgs e)
        {
            FileManager mgrObj;
            FileManager defMgrObj;

            // get the default manager
            defMgrObj = ctlFileManagersDisplay1.GetDefaultFileManagerObject();

            // now create the bottom copper
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.IsolationCut;
            mgrObj.IsoFlipMode = FlipModeEnum.X_Flip;
            mgrObj.ReferencePinGCodeEnabled = false;
            mgrObj.FilenamePattern = "_BottomLayer.GBL";
            mgrObj.Description = "EasyEDA Bottom Layer";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the top copper
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.IsolationCut;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = "_TopLayer.GTL";
            mgrObj.Description = "EasyEDA Top Layer";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the board outline
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.BoardEdgeMill;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = "_BoardOutlineLayer.GKO";
            mgrObj.Description = "EasyEDA Board Outline";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the excellon file
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.Excellon;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = "_NPTH_Through.DRL";
            mgrObj.DrillingCoordinateZerosMode = FileManager.ExcellonDrillingCoordinateZerosModeEnum.FixedDecimalPoint;
            mgrObj.DrillingNumberOfDecimalPlaces = 3;
            mgrObj.Description = "EasyEDA Excellon Drill file.";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the excellon file
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.Excellon;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = "_PTH_Through.DRL";
            mgrObj.DrillingCoordinateZerosMode = FileManager.ExcellonDrillingCoordinateZerosModeEnum.FixedDecimalPoint;
            mgrObj.DrillingNumberOfDecimalPlaces = 3;
            mgrObj.Description = "EasyEDA Excellon Drill file.";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the excellon file
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.Excellon;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = "_PTH_Through_Via.DRL";
            mgrObj.DrillingCoordinateZerosMode = FileManager.ExcellonDrillingCoordinateZerosModeEnum.FixedDecimalPoint;
            mgrObj.DrillingNumberOfDecimalPlaces = 3;
            mgrObj.Description = "EasyEDA Excellon Drill file.";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            OISMessageBox("Template File Managers suitable for EasyEDA  have been added.\n\nThe parameters will be at their default settings. You should check them to be sure they are set appropriate to your requirements.");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Quick Setup (KiCad) button
        /// </summary>
        private void buttonQuickSetupKiCad_Click(object sender, EventArgs e)
        {
            FileManager mgrObj;
            FileManager defMgrObj;

            // get the default manager
            defMgrObj = ctlFileManagersDisplay1.GetDefaultFileManagerObject();

            // now create the bottom copper
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.IsolationCut;
            mgrObj.IsoFlipMode = FlipModeEnum.X_Flip;
            mgrObj.ReferencePinGCodeEnabled = false;
            mgrObj.FilenamePattern = FileManager.KNOWN_EXT_BOTCOPPER_KICAD;
            mgrObj.Description = "KiCad Bottom Layer";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the top copper
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.IsolationCut;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.ReferencePinGCodeEnabled = false;
            mgrObj.FilenamePattern = FileManager.KNOWN_EXT_TOPCOPPER_KICAD;
            mgrObj.Description = "KiCad Top Layer";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the board outline
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.BoardEdgeMill;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = FileManager.KNOWN_EXT_EDGECUT_KICAD;
            mgrObj.Description = "KiCad Board Outline";
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the excellon for plated holes
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.Excellon;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = FileManager.KNOWN_EXT_EXEL_DRILL_PTH_KICAD;
            mgrObj.Description = "KiCad Excellon Plated Drill file.";
            mgrObj.DrillingNumberOfDecimalPlaces = 0;
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the excellon for nonplated holes
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.Excellon;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = FileManager.KNOWN_EXT_EXEL_DRILL_NPTH_KICAD;
            mgrObj.Description = "KiCad Excellon non-Plated Drill file.";
            mgrObj.DrillingNumberOfDecimalPlaces = 0;
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            // now create the excellon for plated and nonplated holes
            mgrObj = FileManager.DeepClone(defMgrObj);
            mgrObj.OperationMode = FileManager.OperationModeEnum.Excellon;
            mgrObj.IsoFlipMode = FlipModeEnum.No_Flip;
            mgrObj.FilenamePattern = FileManager.KNOWN_EXT_EXEL_DRILL_KICAD;
            mgrObj.Description = "KiCad Excellon Generic Drill file.";
            mgrObj.DrillingNumberOfDecimalPlaces = 0;
            ctlFileManagersDisplay1.AddFileManager(mgrObj);

            OISMessageBox("Template File Managers suitable for KiCad  have been added.\n\nThe parameters will be at their default settings. You should check them to be sure they are set appropriate to your requirements.");
        }




#endregion

        // ####################################################################
        // ##### File Open and Save Code
        // ####################################################################
#region File Open and Save Code

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the best output filename for the gcode file
        /// </summary>
        /// <param name="errStr">the error string</param>
        /// <param name="gerberFile">the gerber file to get the best name for</param>
        /// <param name="fileExtension">the file extension to use</param>
        /// <returns>file path and name for success or null for fail</returns>
        private string GetBestGCodeOutputFileNameAndPath(ref string errStr, string sourceFilePathAndName, string fileExtension)
        {
            errStr = "";
            if ((fileExtension == null) || (fileExtension.Length == 0))
            {
                errStr = "No GCode file extension supplied";
                return null;
            }
            if ((sourceFilePathAndName == null) || (sourceFilePathAndName.Length == 0))
            {
                errStr = "No source file path and name supplied";
                return null;
            }

            // get the filename without the extension
            string filePathAndNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePathAndName);
            // get the directory name
            string directoryName = Path.GetDirectoryName(sourceFilePathAndName);
            // build it back up, stick on our bit and send it off
            return Path.Combine(directoryName, filePathAndNameWithoutExtension + fileExtension);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Picks a Gerber or Excellon File off the disk
        /// </summary>
        /// <param name="filePathAndName">the full file path and name</param>
        /// <param name="initialDirectory">the initial directory</param>
        /// <param name="suggestedFile">The suggested file</param>
        /// <returns>z success, -ve cancel, +ve fail</returns>
        private int PickFile(string initialDirectory, string suggestedFile, ref string filePathAndName)
        {
            filePathAndName = string.Empty;

            LogMessage("PickFile called");
            OpenFileDialog ofDialog = new OpenFileDialog();

            // TODO set a extension here for the various PCB file software
            ofDialog.Filter = "Gerber files (*.gbr)|*.gbr|Excellon files (*.drl)|*.drl|All files (*.*)|*.*";
            ofDialog.FilterIndex = 3;
            // can we set the initial directory? Perform some checks
            // if we can't set it we just go with whatever windows has
            // as a default
            if (initialDirectory != null)
            {
                if (Directory.Exists(initialDirectory) == true)
                {
                    // set it
                    ofDialog.InitialDirectory = initialDirectory;
                }
            }
            // can we set the suggested File? 
            if (suggestedFile != null)
            {
                // set it
                ofDialog.FileName = suggestedFile;
            }
            ofDialog.Title = "Choose Gerber or Excellon File";
            // Show it
            DialogResult dlgRes = ofDialog.ShowDialog();
            if ( dlgRes != DialogResult.OK)
            {
                LogMessage("PickFile user cancelled operation");
                return -1;
            }
            // get the file name now
            filePathAndName = ofDialog.FileName;
            if((filePathAndName==null) || (filePathAndName.Length==0))
            {
                LogMessage("PickFile empty file returned");
                return 98;
            }
            // return it
            return 0;
        }

         /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Picks a GCode SaveFileName
        /// </summary>
        /// <param name="filePathAndName">the full file path and name</param>
        /// <param name="initialDirectory">the initial directory</param>
        /// <param name="suggestedFile">The suggested file</param>
        /// <returns>z success, -ve cancel, +ve fail</returns>
        private int PickGCodeFileSaveName(string initialDirectory, string suggestedFile, ref string filePathAndName)
        {
            LogMessage("PickGCodeFileSaveName called");

            SaveFileDialog sfDialog = new SaveFileDialog();
            sfDialog.RestoreDirectory = true;
            sfDialog.Filter = "GCode files (*.ngc)|*.ngc|All files (*.*)|*.*";
            sfDialog.FilterIndex = 2;

            // can we set the initial directory? Perform some checks
            // if we can't set it we just go with whatever windows has
            // as a default
            if (initialDirectory != null)
            {
                if (Directory.Exists(initialDirectory) == true)
                {
                    // set it
                    sfDialog.InitialDirectory = initialDirectory;
                }
            }
            // can we set the suggested File? 
            if (suggestedFile != null)
            {
                // set it
                sfDialog.FileName = suggestedFile;
            }
            sfDialog.Title = "Save GCode File As";

            // Show it
            DialogResult dlgRes = sfDialog.ShowDialog();
            if (dlgRes != DialogResult.OK)
            {
                LogMessage("PickGCodeFileSaveName user cancelled operation");
                return -1;
            }
            // get the file name now
            filePathAndName = sfDialog.FileName;
            if ((filePathAndName == null) || (filePathAndName.Length == 0))
            {
                LogMessage("PickGCodeFileSaveName empty file returned");
                return 98;
            }
            // return it
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Saves the GCode file, note we expect the GCode Text in here. Usually this
        /// is pulled from the Rich Text Box. This makes sure that we output exactly
        /// what the user sees and prevents any parameter changes between creation of
        /// GCode and the saving of the file affecting the output in any way
        /// </summary>
        /// <param name="filePathAndName">the full file path and name</param>
        /// <param name="bSilent">if true we do not pop up notices indicating
        /// failures</param>
        /// <param name="lineTerminator">the line termination string "\r\n" or "\n"</param>
        /// <param name="warnAboutOverwriting">if true we warn about overwriting</param>
        /// <param name="gcodeFileText">The array containing the gcode lines containing the GCode file to save</param>
        /// <param name="errStr">we return the erros string here</param>
        /// <returns>z success, -ve fail user not notified, +ve fail user notified</returns>
        private int SaveGCodeFile(string filePathAndName, string[] gcodeFileText, string lineTerminator, bool warnAboutOverwriting, ref string errStr, bool bSilent)
        {
            TextWriter textWriter = null;

            LogMessage("SaveGCodeFile called");

            if ((filePathAndName == null) || (filePathAndName.Length < 3))
            {
                LogMessage("SaveGCodeFile filePathAndName==null");
                errStr = "No GCode file name supplied.";
                if (bSilent == false)
                {
                    OISMessageBox("No GCode file name supplied.\n\nPlease see the log file.");
                    return 101;
                }
                else return -101;
            }
            if (gcodeFileText == null)
            {
                LogMessage("SaveGCodeFile gcodeFileText == null");
                errStr = "No GCode file text supplied.";
                if (bSilent == false)
                {
                    OISMessageBox("No GCode file text supplied.\n\nPlease see the log file.");
                    return 101;
                }
                else return -101;
            }

            // test the directory name for sanity
            string directoryName = Path.GetDirectoryName(filePathAndName);
            if (Directory.Exists(directoryName) == false)
            {
                DialogResult dlgRes = OISMessageBox_YesNo("The directory\n\n" + directoryName + "\n\ndoes not exist. Would you like to create it now?");
                if (dlgRes == DialogResult.No)
                {
                    errStr = "GCode file save cancelled by user request.";
                    if (bSilent == false)
                    {
                        OISMessageBox("GCode file save cancelled by user request.");
                        return 102;
                    }
                    else return -102;
                }
                LogMessage("Creating directory>"+directoryName);
                try
                {
                    Directory.CreateDirectory(directoryName);
                }
                catch (Exception ex)
                {
                    errStr = "Error creating directory. Message was:"+ex.Message;
                    if (bSilent == false)
                    {
                        OISMessageBox("Error creating directory. Message was:"+ex.Message);
                        return 103;
                    }
                    else return -103;
                }      
            }

            // will we be overwriting an existing file
            if (warnAboutOverwriting == true)
            {
#if !DEBUG
                // we have to test
                if(File.Exists(filePathAndName)==true)
                {
                    DialogResult dlgRes = OISMessageBox_YesNo("The file\n\n" + filePathAndName + "\n\nalready exists. Do you wish to overwrite it?");
                    if (dlgRes == DialogResult.No)
                    {
                        errStr = "GCode file save cancelled by user request.";
                        if (bSilent == false)
                        {
                            OISMessageBox("GCode file save cancelled by user request.");
                            return 104;
                        }
                        else return -104;
                    }
                }
#endif
            }

            LogMessage("SaveGCodeFile: preparing to save:" + filePathAndName);

            try
            {
                // now write out the file
                textWriter = new StreamWriter(filePathAndName);
                // we supply our own terminators
                textWriter.NewLine = "";
                // output each line
                foreach (string lineStr in gcodeFileText)
                {
                    // write a line of text to the file
                    if((lineTerminator==null) || (lineTerminator.Length==0)) textWriter.WriteLine(lineStr);
                    else textWriter.WriteLine(lineStr + lineTerminator);
                }
            }
            catch (Exception ex)
            {
                errStr = "Error writing to file. Message was:" + ex.Message;
                if (bSilent == false)
                {
                    OISMessageBox("Error writing to file. Message was:" + ex.Message);
                    return 104;
                }
                else return -104;
            }
            finally
            {
                // close the stream
                if(textWriter!=null) textWriter.Close();
            }

            LogMessage("SaveGCodeFile: save successful");

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Opens up a Excellon file
        /// </summary>
        /// <param name="fileManagerObj">the file manager we use to interpret the file contents</param>
        /// <param name="filePathAndName">the full file path and name</param>
        /// <param name="bSilent">if true we do not pop up notices indicating failures </param>
        /// <param name="outputExcellonFile">we return the excellon file in this</param>
        /// <param name="errStr">an error string for silent returns</param>
        /// <returns>z success, -ve fail user not notified, +ve fail user notified</returns>
        private int OpenExcellonFile(string filePathAndName, FileManager fileManagerObj, out ExcellonFile outputExcellonFile, bool bSilent, ref string errStr)
        {
            int retInt;

            outputExcellonFile = null;
            errStr = "";

            if ((filePathAndName == null) || (filePathAndName.Length < 3))
            {
                LogMessage("OpenExcellonFile filePathAndName==null");
                errStr = "No Excellon file name supplied.\n\nPlease see the log file.";
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 101;
                }
                else return -101;
            }

            LogMessage("OpenExcellonFile: filePathAndName=" + filePathAndName);

            if (fileManagerObj == null)
            {
                errStr = "No File Manager supplied.\n\nPlease see the log file.";
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 102;
                }
                else return -102;
            }

            // now check the file manager object for the specified Excellon File
            if (fileManagerObj.OperationMode == FileManager.OperationModeEnum.Default)
            {
                errStr = "Could not find File Manager object for this Excellon file. No operations will be possible.\n\nFix this by configuring a File Manager on the Settings tab.";
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 201;
                }
                else return -201;
            }

            // get a new excellon file
            outputExcellonFile = new ExcellonFile();
            outputExcellonFile.ExcellonFileManager = fileManagerObj;
            // read it into our ExcellonFile
            retInt = ReadExcellonFile(filePathAndName, ref outputExcellonFile, bSilent);
            // did it error?
            if (retInt != 0)
            {
                // log this
                LogMessage("OpenExcellonFile call to ReadExcellonFile returned" + retInt.ToString());
                // yes it did, was it reported?
                if (retInt < 0)
                {
                    errStr = "Error " + retInt.ToString() + " occurred reading the Excellon File.\n\nPlease see the log file.";
                    // no it was not, should we report it
                    if (bSilent == false)
                    {
                        OISMessageBox(errStr);
                        return 109;
                    }
                    return -109;
                }
                else return 1034;
            }

            // set the file path and name now
            outputExcellonFile.ExcellonFilePathAndName = filePathAndName;
            // set our application units now
            outputExcellonFile.StateMachine.IsoPlotPointsPerAppUnit = GetIsoPlotPointsPerAppUnit(outputExcellonFile.ExcellonFileUnits);
            // ensure the units in the file manager match the units in the file
            outputExcellonFile.ExcellonFileManager.SyncUnitsToFile(outputExcellonFile.ExcellonFileUnits);

            // always post process it
            bool retBool = outputExcellonFile.PerformOpenPostProcessingChecks(out errStr);
            if (retBool != true)
            {
                LogMessage("OpenExcellonFile Error in call to PerformOpenPostProcessingChecks. Error message is>" + errStr);
                errStr = "Error in file when performing header processing. Error message is\n\n" + errStr;
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 105;
                }
                return -105;
            }

            outputExcellonFile.SetPadCenterPointList();

            // set the ignore pins
            outputExcellonFile.SetIgnoreTools(ref errStr);

            // find the mid point now before margin adjustments, If we have reference pins this point will later be set with a better value
            outputExcellonFile.MidDCodeXCoord = 0 + (outputExcellonFile.MaxDCodeXCoord + outputExcellonFile.MinDCodeXCoord) / 2;  // the origin is always zero
            outputExcellonFile.MidDCodeYCoord = 0 + (outputExcellonFile.MaxDCodeYCoord + outputExcellonFile.MinDCodeYCoord) / 2;  // the origin is always zero
            LogMessage("OpenExcellonFile MidPoint from PlotVals= (MidX,MidY)=(" + outputExcellonFile.MidDCodeXCoord.ToString() + "," + outputExcellonFile.MidDCodeYCoord.ToString() + ")");

            // figure out a margin here
            float xMargin = Math.Abs((outputExcellonFile.MaxDCodeXCoord - outputExcellonFile.MinDCodeXCoord) * ISOPLOT_MARGIN_PERCENT);
            float yMargin = Math.Abs((outputExcellonFile.MaxDCodeYCoord - outputExcellonFile.MinDCodeYCoord) * ISOPLOT_MARGIN_PERCENT);

            // set the new minimum value
            float tmpXMin = outputExcellonFile.MinDCodeXCoord - xMargin;
            float tmpYMin = outputExcellonFile.MinDCodeYCoord - yMargin;
            float xOffset = tmpXMin * -1;
            float yOffset = tmpYMin * -1;
            // set our offset to move the lowest point we have near the origin
            outputExcellonFile.SetExcellonPlotOriginCoordinateAdjustments(xOffset, yOffset);

            //DebugMessage("ExcellonFile (xMargin,yMargin)=(" + xMargin.ToString() + "," + yMargin.ToString() + ")");
            //DebugMessage("ExcellonFile (MinDCodeXCoord,MinDCodeYCoord)=(" + outputExcellonFile.MinDCodeXCoord.ToString() + "," + outputExcellonFile.MinDCodeYCoord.ToString() + ")");
            LogMessage("ExcellonFile (OffsetX,OffsetY)=(" + xOffset.ToString() + "," + yOffset.ToString() + ")");

            // set the new plot maximum value
            float tmpXMax = outputExcellonFile.MaxDCodeXCoord + xMargin;
            float tmpYMax = outputExcellonFile.MaxDCodeYCoord + yMargin;
            // we have to adjust these a bit to give us some wiggle room
            outputExcellonFile.MaxDCodeXCoord = tmpXMax;
            outputExcellonFile.MaxDCodeYCoord = tmpYMax;

            LogMessage("ExcellonFile newXMax=" + tmpXMax.ToString() + ", newYMax=" + tmpYMax.ToString() + ", newXMin=" + tmpXMin.ToString() + ", newYMin=" + tmpYMin);

            if (outputExcellonFile.ExcellonFileManager.DrillingReferencePinsEnabled == true)
            {

                retInt = outputExcellonFile.SetReferencePins(ref errStr);
                if (retInt != 0)
                {
                    // record this
                    outputExcellonFile.StateMachine.ReferencePinsFound = false;
                    LogMessage("OpenExcellonFile Error in call to GetReferencePinList. Error message is>" + errStr);
                    if (bSilent == true)
                    {
                        return 105;
                    }
                    DialogResult dlgRes = OISMessageBox_YesNo("Reference pins are enabled in the File Manager but no Reference Pins of size " + outputExcellonFile.ExcellonFileManager.ReferencePinPadDiameter.ToString() + " could be found. The precise alignment of double sided boards may not be possible.\n\nOpen anyways?");
                    if (dlgRes == DialogResult.No)
                    {
                        // we already asked the user no need to warn again
                        return 106;
                    }
                    // we are opening anyways
                    outputExcellonFile.StateMachine.IgnoreReferencePinsIfNotFound = true;
                }
                else
                {
                    // record this
                    outputExcellonFile.StateMachine.ReferencePinsFound = true;
                }
            }
            else
            {
                // reference pins not enabled, ignore
                outputExcellonFile.StateMachine.IgnoreReferencePinsIfNotFound = true;
            }

            if (outputExcellonFile.StateMachine.ReferencePinsFound == true)
            {
                LogMessage("OpenExcellonFile outputExcellonFile.StateMachine.ReferencePinsFound == true");
                // now find the midpoint values
                float midX = 0;
                float midY = 0;
                retInt = outputExcellonFile.GetMidPointFromReferencePins(out midX, out midY, ref errStr);
                if (retInt != 0)
                {
                    LogMessage("OpenExcellonFile Error in call to GetMidPointFromReferencePins. Error message is>" + errStr);
                    if (bSilent == false)
                    {
                        OISMessageBox(errStr);
                        return 107;
                    }
                    return -107;
                }
                // set these values now. Note these points are the center of the plot between
                // the origin and the center of the reference pins
                outputExcellonFile.MidDCodeXCoord = midX;
                outputExcellonFile.MidDCodeYCoord = midY;
                LogMessage("OpenExcellonFile Midpoint from refPins = (MidX,MidY)=(" + midX.ToString() + "," + midY.ToString() + ")");

            }
            else // bottom of if (outputExcellonFile.StateMachine.ReferencePinsFound == true)
            {
                // we have no reference pin. We have already built the midpoints from the max and min coordinates in the file
                // and hope for the best
                LogMessage("OpenExcellonFile outputExcellonFile.StateMachine.ReferencePinsFound == false");
            }

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Opens up a Gerber file
        /// </summary>
        /// <param name="fileManagerObj">the file manager we use to interpret the file contents</param>
        /// <param name="filePathAndName">the full file path and name</param>
        /// <param name="bSilent">if true we do not pop up notices indicating failures</param>
        /// <param name="outputExcellonFile">we return the excellon file in this</param>
        /// <param name="errStr">an error string for silent returns</param>
        /// <returns>z success, -ve fail user not notified, +ve fail user notified</returns>
        private int OpenGerberFile(string filePathAndName, FileManager fileManagerObj, out GerberFile outputGerberFile, bool bSilent, ref string errStr)
        {
            int retInt;
            bool retBool;

            outputGerberFile = null;
            errStr = "";

            if ((filePathAndName == null) || (filePathAndName.Length < 3))
            {
                LogMessage("OpenGerberFile filePathAndName==null");
                errStr = "No Gerber file name supplied.\n\nPlease see the log file.";
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 101;
                }
                else return -101;
            }

            LogMessage("OpenGerberFile: filePathAndName=" + filePathAndName);

            if (fileManagerObj == null)
            {
                errStr = "No File Manager supplied.\n\nPlease see the log file.";
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 102;
                }
                else return -102;
            }

            // now check the  file manager object for the specified GerberFile
            if (fileManagerObj.OperationMode == FileManager.OperationModeEnum.Default)
            {
                errStr = "Could not find File Manager object for this Gerber File. No operations will be possible.\n\nFix this by configuring a File Manager on the Settings tab.";
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 201;
                }
                else return -201;
            }

            // get a new gerber file
            outputGerberFile = new GerberFile();
            outputGerberFile.GerberFileManager = fileManagerObj;
            // read it into our Gerber File
            retInt = ReadGerberFile(filePathAndName, ref outputGerberFile, bSilent);
            // did it error?
            if (retInt != 0)
            {
                // log this
                LogMessage("OpenGerberFile call to ReadGerberFile returned" + retInt.ToString());
                errStr = "Error " + retInt.ToString() + " occurred reading the Gerber File.\n\nPlease see the log file.";
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 109;
                }
                return -109;
            }

            // now we have the commands in the outputGerberFile, we have to process them into GerberLine objects 
            retInt = outputGerberFile.ProcessGerberCommands();
            if (retInt != 0)
            {
                // log this
                LogMessage("OpenGerberFile call to ProcessGerberCommands returned" + retInt.ToString());
                errStr = "Error " + retInt.ToString() + " occurred processing the Gerber Commands.\n\nPlease see the log file.";
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 110;
                }
                return -110;
            }

            // set the file path and name now
            outputGerberFile.GerberFilePathAndName = filePathAndName;
            // set our application units now
            outputGerberFile.StateMachine.IsoPlotPointsPerAppUnit = GetIsoPlotPointsPerAppUnit(outputGerberFile.GerberFileUnits);
            // ensure the units in the file manager match the units in the file
            outputGerberFile.GerberFileManager.SyncUnitsToFile(outputGerberFile.GerberFileUnits);

            // always post process check it
            retBool = outputGerberFile.PerformOpenPostProcessingFixups(out errStr);
            if (retBool != true)
            {
                LogMessage("OpenGerberFile Error in call to PerformOpenPostProcessingFixups. Error message is>" + errStr);
                errStr = "Error in file when performing header processing. Error message is\n\n" + errStr;
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 104;
                }
                return -104;
            }

            // set our min and max sizes now, these will later be adjusted with margins etc
            outputGerberFile.FindAllMinMaxPlotSizesAndSetInGerberFile();

            // find the mid point now before margin adjustments, If we have reference pins this point will later be set with a better value
            outputGerberFile.MidDCodeXCoord = 0 + (outputGerberFile.MaxDCodeXCoord + outputGerberFile.MinDCodeXCoord) / 2;  // the origin is always zero
            outputGerberFile.MidDCodeYCoord = 0 + (outputGerberFile.MaxDCodeYCoord + outputGerberFile.MinDCodeYCoord) / 2;  // the origin is always zero
            LogMessage("OpenGerberFile MidPoint from PlotVals= (MidX,MidY)=(" + outputGerberFile.MidDCodeXCoord.ToString() + "," + outputGerberFile.MidDCodeYCoord.ToString() + ")");

            // always post process check it
            retBool = outputGerberFile.PerformOpenPostProcessingChecks(out errStr);
            if (retBool != true)
            {
                LogMessage("OpenGerberFile Error in call to PerformOpenPostProcessingChecks. Error message is>" + errStr);
                errStr = "Error in file when performing header processing. Error message is\n\n" + errStr;
                if (bSilent == false)
                {
                    OISMessageBox(errStr);
                    return 105;
                }
                return -105;
            }

            // #####
            // ##### we have set the min,max and mid coordinates of the gerber file we just opened
            // ##### now we need to adjust them to compensate for the margins
            // #####

            // we want the smallest XY DCode coordinates we found to be approximately zero
            // get them now.

            // figure out a margin here
            float xMargin = Math.Abs((outputGerberFile.MaxDCodeXCoord - outputGerberFile.MinDCodeXCoord) * ISOPLOT_MARGIN_PERCENT);
            float yMargin = Math.Abs((outputGerberFile.MaxDCodeYCoord - outputGerberFile.MinDCodeYCoord) * ISOPLOT_MARGIN_PERCENT);

            // set the new minimum value
            float tmpXMin = outputGerberFile.MinDCodeXCoord - xMargin;
            float tmpYMin = outputGerberFile.MinDCodeYCoord - yMargin;
            float xOffset = tmpXMin * -1;
            float yOffset = tmpYMin * -1;
            // set our offset to move the lowest point we have near the origin
            outputGerberFile.SetGerberPlotOriginCoordinateAdjustments(xOffset, yOffset);

            //DebugMessage("GerberFile (xMargin,yMargin)=(" + xMargin.ToString() + "," + yMargin.ToString() + ")");
            //DebugMessage("GerberFile (MinDCodeXCoord,MinDCodeYCoord)=(" + outputGerberFile.MinDCodeXCoord.ToString() + "," + outputGerberFile.MinDCodeYCoord.ToString() + ")");
            LogMessage("GerberFile (OffsetX,OffsetY)=(" + xOffset.ToString() + "," + yOffset.ToString() + ")");

            // set the new plot maximum value
            float tmpXMax = outputGerberFile.MaxDCodeXCoord + xMargin;
            float tmpYMax = outputGerberFile.MaxDCodeYCoord + yMargin;
            // we have to adjust these a bit to give us some wiggle room
            outputGerberFile.MaxDCodeXCoord = tmpXMax;
            outputGerberFile.MaxDCodeYCoord = tmpYMax;

            LogMessage("GerberFile newXMax=" + tmpXMax.ToString() + ", newYMax=" + tmpYMax.ToString() + ", newXMin=" + tmpXMin.ToString() + ", newYMin=" + tmpYMin);

            // in certain operation modes we might have a set of reference pins or ignore pins
            if (outputGerberFile.GerberFileManager.OperationMode == FileManager.OperationModeEnum.IsolationCut)
            {
                // now ignore pins
                if (outputGerberFile.GerberFileManager.IgnorePadEnabled == true)
                {
                    outputGerberFile.SetIgnorePins(ref errStr);
                }

                // now reference pins
                if (outputGerberFile.GerberFileManager.ReferencePinGCodeEnabled == true)
                {
                    retInt = outputGerberFile.SetReferencePins(ref errStr);
                    if (retInt != 0)
                    {
                        // record this
                        outputGerberFile.StateMachine.ReferencePinsFound = false;
                        LogMessage("OpenGerberFile Error in call to GetReferencePinList. Error message is>" + errStr);
                        if (bSilent == true)
                        {
                            return 105;
                        }
                        DialogResult dlgRes = OISMessageBox_YesNo("Reference pins are enabled in the File Manager but no Reference Pins of size " + outputGerberFile.GerberFileManager.ReferencePinPadDiameter.ToString() + " could be found. The precise alignment of double sided boards may not be possible.\n\nOpen anyways?");
                        if (dlgRes == DialogResult.No)
                        {
                            // we already asked the user no need to warn again
                            return 106;
                        }
                        // we are opening anyways
                        outputGerberFile.StateMachine.IgnoreReferencePinsIfNotFound = true;
                    }
                    else
                    {
                        // record this
                        outputGerberFile.StateMachine.ReferencePinsFound = true;
                    }
                }
                else
                {
                    // we are opening anyways
                    outputGerberFile.StateMachine.IgnoreReferencePinsIfNotFound = true;
                }

                if (outputGerberFile.StateMachine.ReferencePinsFound == true) 
                {
                    LogMessage("OpenGerberFile outputGerberFile.StateMachine.ReferencePinsFound == true");
                    // now find the midpoint values
                    float midX = 0;
                    float midY = 0;
                    retInt = outputGerberFile.GetMidPointFromReferencePins(out midX, out midY, ref errStr);
                    if (retInt != 0)
                    {
                        LogMessage("OpenGerberFile Error in call to GetMidPointFromReferencePins. Error message is>" + errStr);
                        if (bSilent == false)
                        {
                            OISMessageBox(errStr);
                            return 107;
                        }
                        return -107;
                    }
                    // set these values now. Note these points are the center of the plot between
                    // the origin and the center of the reference pins
                    outputGerberFile.MidDCodeXCoord = midX;
                    outputGerberFile.MidDCodeYCoord = midY;
                    LogMessage("OpenGerberFile Midpoint from refPins = (MidX,MidY)=(" + midX.ToString() + "," + midY.ToString() + ")");

                }
                else // bottom of if (outputGerberFile.StateMachine.ReferencePinsFound == true)
                {
                    // we have no reference pin. We have already built the midpoints from the max and min coordinates in the file
                    // and hope for the best
                    LogMessage("OpenGerberFile outputGerberFile.StateMachine.ReferencePinsFound == false");
                }

            } // bottom of if (outputGerberFile.GerberFileManager.OperationMode == FileManager.OperationModeEnum.IsolationCut)


            LogMessage("OpenGerberFile complete");

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything necessary to open a Gerber file and display it.
        /// </summary>
        /// <param name="filePath">path to the file</param>
        /// <param name="gerberFileToPopulate">gerber file to populate</param>
        /// <param name="bSilent">if true we do not post notice boxes</param>
        /// <remarks>The gerber file is assumed to have been reset and ready to load. 
        /// We do not reset it in here</remarks>
        /// <returns>z success, -ve fail user not notified, +ve fail user notified</returns>
        private int ReadGerberFile(string fileNameAndPath, ref GerberFile gerberFileToPopulate, bool bSilent)
        {
            string line;
            int retInt;
            int lineNumber = 0;

            if (gerberFileToPopulate == null) return -11;
            if (fileNameAndPath == null) return -12;

            LogMessage("ReadGerberFile: Opening file >" + fileNameAndPath + "<");

            if (File.Exists(fileNameAndPath) == false)
            {
                LogMessage("ReadGerberFile The file " + fileNameAndPath + " does not exist.");
                if (bSilent == false)
                {
                    OISMessageBox("The file \n\n" + fileNameAndPath + "\n\ndoes not exist. Please see the log file.");
                    return 23;
                }
                else return -23;
            }

            StreamReader fileReader = null;
            try
            {
                // create a reader and read each line into the GerberFile
                fileReader = new StreamReader(fileNameAndPath);
                while ((line = fileReader.ReadLine()) != null)
                {
                    lineNumber++;
                    retInt = gerberFileToPopulate.AddLine(line, lineNumber);
                    if (retInt != 0)
                    {
                        LogMessage("ReadGerberFile gerberFileToPopulate.AddLine returned " + retInt.ToString() + " when adding line number " + lineNumber.ToString());
                        LogMessage("line number " + lineNumber.ToString()+ " contents >"+line+"<");
                        if (bSilent == false)
                        {
                            OISMessageBox("Error "+retInt.ToString()+" occurred when adding line number "+lineNumber.ToString()+"\n\nPlease see the log file.");
                            return 24;
                        }
                        else return -24;
                    }
                }
            }
            finally
            {
                if (fileReader != null)
                {
                    fileReader.Close();
                }
            }
            return 0;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything necessary to open a Excellon file and display it.
        /// </summary>
        /// <param name="filePath">path to the file</param>
        /// <param name="excellonFileToPopulate">excellon file to populate</param>
        /// <param name="bSilent">if true we do not post notice boxes</param>
        /// <returns>z success, -ve fail user not notified, +ve fail user notified</returns>
        private int ReadExcellonFile(string fileNameAndPath, ref ExcellonFile excellonFileToPopulate, bool bSilent)
        {
            string line;
            int retInt;
            int lineNumber = 0;

            if (excellonFileToPopulate == null) return -11;
            if (fileNameAndPath == null) return -12;

            LogMessage("ReadExcellonFile: Opening file >" + fileNameAndPath + "<");

            if (File.Exists(fileNameAndPath) == false)
            {
                LogMessage("ReadExcellonFile The file " + fileNameAndPath + " does not exist.");
                if (bSilent == false)
                {
                    OISMessageBox("The file \n\n" + fileNameAndPath + "\n\ndoes not exist. Please see the log file.");
                    return 23;
                }
                else return -23;
            }

            StreamReader fileReader = null;
            try
            {
                // create a reader and read each line into the ExcellonFile
                fileReader = new StreamReader(fileNameAndPath);
                while ((line = fileReader.ReadLine()) != null)
                {
                    lineNumber++;
                    retInt = excellonFileToPopulate.AddLine(line, lineNumber);
                    if (retInt != 0)
                    {
                        LogMessage("ReadExcellonFile excellonFileToPopulate.AddLine returned " + retInt.ToString() + " when adding line number " + lineNumber.ToString());
                        LogMessage("lne number " + lineNumber.ToString() + " contents >" + line + "<");
                        if (bSilent == false)
                        {
                            OISMessageBox("Error " + retInt.ToString() + " occurred when adding line number " + lineNumber.ToString() + "\n\nPlease see the log file.");
                            return 24;
                        }
                        else return -24;
                    }
                }
            }
            finally
            {
                if (fileReader != null)
                {
                    fileReader.Close();
                }
            }
            return 0;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets or Sets our MRU list form. Will never get or set a null value
        /// </summary>
        private frmOISMRUList MRUList
        {
            get
            {
                if(mruList==null) mruList = new frmOISMRUList();
                return mruList;
            }
            set
            {
                mruList = value;
                if(mruList==null) mruList = new frmOISMRUList();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a valid gerber file has been opened.
        /// </summary>
        private bool IsValidGerberFileOpen()
        {
            if (currentGerberFile == null) return false;
            if (currentGerberFile.SourceLines.Count == 0) return false;
            // TODO successful header processing check?
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a valid Excellon file has been opened.
        /// </summary>
        private bool IsValidExcellonFileOpen()
        {
            if (currentExcellonFile == null) return false;
            if (currentExcellonFile.SourceLines.Count == 0) return false;
            // TODO successful header processing check?
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a valid isolation GCode has been created
        /// </summary>
        private bool IsValidIsolationGCodeFileOpen()
        {
            if (currentIsolationGCodeFile == null) return false;
            if (currentIsolationGCodeFile.SourceLines.Count == 0) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a valid edge mill GCode has been created
        /// </summary>
        private bool IsValidEdgeMillGCodeFileOpen()
        {
            if (currentEdgeMillGCodeFile == null) return false;
            if (currentEdgeMillGCodeFile.SourceLines.Count == 0) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a valid bed flattening GCode has been created
        /// </summary>
        private bool IsValidBedFlatteningGCodeFileOpen()
        {
            if (currentBedFlatteningGCodeFile == null) return false;
            if (currentBedFlatteningGCodeFile.SourceLines.Count == 0) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a valid ref pin GCode has been created
        /// </summary>
        private bool IsValidRefPinGCodeFileOpen()
        {
            if (currentReferencePinGCodeFile == null) return false;
            if (currentReferencePinGCodeFile.SourceLines.Count == 0) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a valid drill GCode has been created
        /// </summary>
        private bool IsValidDrillGCodeFileOpen()
        {
            if (currentDrillGCodeFile == null) return false;
            if (currentDrillGCodeFile.SourceLines.Count == 0) return false;
            return true;
        }

#endregion

        // ####################################################################
        // ##### Gerber to GCode Conversion Routines
        // ####################################################################
#region Gerber to GCode Conversion Routines

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Builds an empty GCodeFile but populates it with the current application
        /// parameters and settings
        /// </summary>
        /// <param name="gerberFile">the gerber file which will be converted
        /// to the GCode file</param>
        /// <param name="fileManagerObj">the file manager</param>
        private GCodeFile BuildEmptyGCodeFileWithCurrentApplicationParameters(GerberFile gerberFile, FileManager fileManagerObj)
        {
            GCodeFile gcFile = new GCodeFile();

            if (gerberFile == null) return null;
            if (fileManagerObj == null) return null;

            // set some configuration option
            gcFile.StateMachine.ToolHeadSetup = gerberFile.StateMachine.ToolHeadSetup;
            gcFile.StateMachine.SourceUnits = gerberFile.GerberFileUnits;
            gcFile.StateMachine.IsoPlotPointsPerAppUnit = GetIsoPlotPointsPerAppUnit(gerberFile.GerberFileUnits);
            gcFile.StateMachine.OutputUnits = OutputApplicationUnits;

            // set our min an max values now
            gcFile.XMinValue = gerberFile.MinPlotXCoord;
            gcFile.YMinValue = gerberFile.MinPlotYCoord;
            gcFile.XMaxValue = gerberFile.MaxPlotXCoord;
            gcFile.YMaxValue = gerberFile.MaxPlotYCoord;

            if (fileManagerObj.GCodeOriginAtCenter == true)
            {
                // user wants the origin set to the equivalent of the center of the gerber plot
                gcFile.GCodeOutputPlotOriginAdjust_X = gerberFile.MidPlotXCoord;
                gcFile.GCodeOutputPlotOriginAdjust_Y = gerberFile.MidPlotYCoord;
            }
            else
            {
                // leave it at 0,0
                gcFile.GCodeOutputPlotOriginAdjust_X = 0;
                gcFile.GCodeOutputPlotOriginAdjust_Y = 0;
            }
            // always give it the flip Axis
            gcFile.GCodeMirrorAxisPlotCoord_X = gerberFile.MidPlotXCoord;
            // are we actually flipping 
            gcFile.MirrorOnConversionToGCode = gerberFile.FlipMode;

            // get absolute offsets
            float absoluteOffset_X = 0;
            float absoluteOffset_Y = 0;
            bool retBool = gerberFile.GetAbsoluteOffsets(out absoluteOffset_X, out absoluteOffset_Y);
            if(retBool==true)
            {
                gcFile.AbsoluteOffset_X = absoluteOffset_X;
                gcFile.AbsoluteOffset_Y = absoluteOffset_Y;
            }

            return gcFile;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Builds an empty GCodeFile but populates it with the current application
        /// parameters and settings
        /// </summary>
        /// <param name="excellonFile">the excellon file which will be converted
        /// <param name="fileManagerObj">the file manager</param>
        /// to the GCode file</param>
        private GCodeFile BuildEmptyGCodeFileWithCurrentApplicationParameters(ExcellonFile excellonFile, FileManager fileManagerObj)
        {
            GCodeFile gcFile = new GCodeFile();

            if (excellonFile == null) return null;
            if (fileManagerObj == null) return null;

            // set some configuration option
            gcFile.StateMachine.ToolHeadSetup = excellonFile.StateMachine.ToolHeadSetup;
            gcFile.StateMachine.SourceUnits = excellonFile.ExcellonFileUnits;
            gcFile.StateMachine.IsoPlotPointsPerAppUnit = GetIsoPlotPointsPerAppUnit(excellonFile.ExcellonFileUnits);
            gcFile.StateMachine.OutputUnits = OutputApplicationUnits;

            // set our min an max values now
            gcFile.XMinValue = excellonFile.MinPlotXCoord;
            gcFile.YMinValue = excellonFile.MinPlotYCoord;
            gcFile.XMaxValue = excellonFile.MaxPlotXCoord;
            gcFile.YMaxValue = excellonFile.MaxPlotYCoord;

            if (fileManagerObj.GCodeOriginAtCenter == true)
            {
                // user wants the origin set to the equivalent of the center of the excellon plot
                gcFile.GCodeOutputPlotOriginAdjust_X = excellonFile.MidPlotXCoord;
                gcFile.GCodeOutputPlotOriginAdjust_Y = excellonFile.MidPlotYCoord;
            }
            else
            {
                // leave it at 0,0
                gcFile.GCodeOutputPlotOriginAdjust_X = 0;
                gcFile.GCodeOutputPlotOriginAdjust_Y = 0;
            }
            // always give it the flip Axis
            gcFile.GCodeMirrorAxisPlotCoord_X = excellonFile.MidPlotXCoord;

            // are we actually flipping 
            gcFile.MirrorOnConversionToGCode = excellonFile.FlipMode;

            // get absolute offsets
            float absoluteOffset_X = 0;
            float absoluteOffset_Y = 0;
            bool retBool = excellonFile.GetAbsoluteOffsets(out absoluteOffset_X, out absoluteOffset_Y);
            if (retBool == true)
            {
                gcFile.AbsoluteOffset_X = absoluteOffset_X;
                gcFile.AbsoluteOffset_Y = absoluteOffset_Y;
            }

            return gcFile;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Populates GCodeFile with suitable header lines. This code assumes
        /// the input GCodeFile is empty but has been populated with the 
        /// current application parameters
        /// </summary>
        /// <param name="gcFile">the gcodeFile to populate</param>
        /// <param name="workingGerberFile">the current gerber file</param>
        /// <param name="workingExcellonFile">the current excellon file</param>
        /// <remarks>one and only one of the currentGerberfile or currentExcellonFile must be non null</remarks>
        private void PopulateGCodeFileWithStandardHeaderLines(ref GCodeFile gcFile, GerberFile workingGerberFile, ExcellonFile workingExcellonFile)
        {
            // just call this with the default of start spindle
            PopulateGCodeFileWithStandardHeaderLines(ref gcFile, workingGerberFile, workingExcellonFile, true);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Populates GCodeFile with suitable header lines. This code assumes
        /// the input GCodeFile is empty but has been populated with the 
        /// current application parameters
        /// </summary>
        /// <param name="gcFile">the gcodeFile to populate</param>
        /// <param name="workingGerberFile">the current gerber file</param>
        /// <param name="workingExcellonFile">the current excellon file</param>
        /// <param name="wantSpindleStartCodes">if true we add in codes to start the spindle and dwell</param>
        /// <remarks>one and only one of the currentGerberfile or currentExcellonFile must be non null</remarks>
        private void PopulateGCodeFileWithStandardHeaderLines(ref GCodeFile gcFile, GerberFile workingGerberFile, ExcellonFile workingExcellonFile, bool wantSpindleStartCodes)
        {
            GCodeCmd_Comment coLine = null;
            GCodeCmd_CommandWord cwLine = null;
            GCodeCmd_SetPosition spLine = null;
            GCodeCmd_Dwell dwLine = null;

            // sanity check
            if (gcFile == null) return;
            if ((workingGerberFile == null) && (workingExcellonFile == null)) return;

            coLine = new GCodeCmd_Comment("Generated By: " + APPLICATION_NAME + " Version: " + APPLICATION_VERSION);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment("Home Page: " + APPLICATION_HOME);
            gcFile.AddLine(coLine);
            if (CurrentGerberFile.GerberFilePathAndName != null)
            {
                coLine = new GCodeCmd_Comment("Generated from file: " + CurrentGerberFile.GerberFilePathAndName);
                gcFile.AddLine(coLine);
            }
            coLine = new GCodeCmd_Comment("Date Generated: " + DateTime.Now.ToString("f"));
            gcFile.AddLine(coLine);

            // blank
            coLine = new GCodeCmd_Comment("");
            gcFile.AddLine(coLine);

            // Standard comment header 
            coLine = new GCodeCmd_Comment(WARN01);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN02);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN03);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN04);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN05);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN06);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN07);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN08);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN09);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN10);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN11);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN12);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN13);
            gcFile.AddLine(coLine);
            coLine = new GCodeCmd_Comment(WARN14);
            gcFile.AddLine(coLine);
            // blank
            coLine = new GCodeCmd_Comment("");
            gcFile.AddLine(coLine);

            if (gcFile.StateMachine.OutputUnits == ApplicationUnitsEnum.INCHES)
            {
                // G20 
                cwLine = new GCodeCmd_CommandWord(GCodeCmd.GCODEWORD_UNIT_IN, "Use Inches");
                gcFile.AddLine(cwLine);
            }
            else
            {
                // G21 
                cwLine = new GCodeCmd_CommandWord(GCodeCmd.GCODEWORD_UNIT_MM, "Use MilliMeters");
                gcFile.AddLine(cwLine);
            }

            // G90 Set Absolute Coordinates
            cwLine = new GCodeCmd_CommandWord(GCodeCmd.GCODEWORD_COORDMODE_ABSOLUTE, "Set Absolute Coordinates");
            gcFile.AddLine(cwLine);

            // G17 xy plane selection 
            cwLine = new GCodeCmd_CommandWord(GCodeCmd.GCODEWORD_XYPLANE, "XY plane selection");
            gcFile.AddLine(cwLine);

            if (workingGerberFile != null)
            {
                // assume we are in the lower right hand corner
                int workingPlotOrigin_X = 0;
                int workingPlotOrigin_Y = 0;

                // are we centering the origin
                if (workingGerberFile.GerberFileManager.GCodeOriginAtCenter == true)
                {
                    // override this 
                    workingPlotOrigin_X = (int)(workingGerberFile.MidDCodeXCoord * gcFile.StateMachine.IsoPlotPointsPerAppUnit) - (int)(gcFile.StateMachine.AbsoluteOffset_X);
                    workingPlotOrigin_Y = (int)(workingGerberFile.MidDCodeYCoord * gcFile.StateMachine.IsoPlotPointsPerAppUnit) - (int)(gcFile.StateMachine.AbsoluteOffset_Y);
                }
                // note there is a bug here. On the X flip boards the origin is not right if origin is 0,0. It is ok if the origin is in the center.
                // the ability to place the gcode origin in the lower left has been disabled in the configuration manager

                //// G92 assume our tool is now over the origin
                spLine = new GCodeCmd_SetPosition(workingPlotOrigin_X, workingPlotOrigin_Y, "SetPosition");
                gcFile.AddLine(spLine);
            }
            else if (workingExcellonFile != null)
            {
                // assume we are in the lower right hand corner
                int workingPlotOrigin_X = 0;
                int workingPlotOrigin_Y = 0;

                // are we centering the origin
                if (workingExcellonFile.ExcellonFileManager.GCodeOriginAtCenter == true)
                {
                    // override this 
                    workingPlotOrigin_X = (int)(workingExcellonFile.MidDCodeXCoord * gcFile.StateMachine.IsoPlotPointsPerAppUnit) - (int)(gcFile.StateMachine.AbsoluteOffset_X);
                    workingPlotOrigin_Y = (int)(workingExcellonFile.MidDCodeYCoord * gcFile.StateMachine.IsoPlotPointsPerAppUnit) - (int)(gcFile.StateMachine.AbsoluteOffset_Y);
                }
                // note there is a bug here. On the X flip boards the origin is not right if origin is 0,0. It is ok if the origin is in the center.
                // the ability to place the gcode origin in the lower left has been disabled in the configuration manager

                //// G92 assume our tool is now over the origin
                spLine = new GCodeCmd_SetPosition(workingPlotOrigin_X, workingPlotOrigin_Y, "SetPosition");
                gcFile.AddLine(spLine);
            }

            // not everthing wants this
            if (wantSpindleStartCodes == true)
            {
                // M03 start spindle
                cwLine = new GCodeCmd_CommandWord(GCodeCmd.GCODEWORD_SPINDLESTART_CW, "Start spindle");
                gcFile.AddLine(cwLine);

                // G04 Dwell 
                dwLine = new GCodeCmd_Dwell(GCodeCmd_Dwell.DEFAULT_DWELL_TIME, "Pause to let the spindle start");
                gcFile.AddLine(dwLine);
            }

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Populates GCodeFile with suitable footer lines. This code assumes
        /// the input GCodeFile is populated and we are bolting these onto the end.
        /// </summary>
        /// <param name="gcFile">the gcodeFile to populate</param>
        /// <param name="workingGerberFile">the current gerber file</param>
        /// <param name="workingExcellonFile">the current excellon file</param>
        /// <remarks>one and only one of the currentGerberfile or currentExcellonFile must be non null</remarks>
        private void PopulateGCodeFileWithStandardFooterLines(ref GCodeFile gcFile, GerberFile workingGerberFile, ExcellonFile workingExcellonFile)
        {
            GCodeCmd_CommandWord cwLine = null;
            GCodeCmd_ZMove zLine = null;
            GCodeCmd_RapidMove rmLine = null;

            // sanity check
            if (gcFile == null) return;

            // G00 - pull bit off the work piece
            zLine = new GCodeCmd_ZMove(GCodeCmd_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForClear, "Clear workspace");
            gcFile.AddLine(zLine);

            if (workingGerberFile != null)
            {
                int workingPlotOrigin_X = (int)(gcFile.StateMachine.GCodeOutputPlotOriginAdjust_X * gcFile.StateMachine.IsoPlotPointsPerAppUnit);
                int workingPlotOrigin_Y = (int)(gcFile.StateMachine.GCodeOutputPlotOriginAdjust_Y * gcFile.StateMachine.IsoPlotPointsPerAppUnit);
                // G00 rapid move to the origin
                rmLine = new GCodeCmd_RapidMove(workingPlotOrigin_X, workingPlotOrigin_Y, "Move to Origin");
                gcFile.AddLine(rmLine);
            }
            else if (workingExcellonFile != null)
            {
                // G00 rapid move to the origin
                int workingPlotOrigin_X = (int)(gcFile.StateMachine.GCodeOutputPlotOriginAdjust_X * gcFile.StateMachine.IsoPlotPointsPerAppUnit);
                int workingPlotOrigin_Y = (int)(gcFile.StateMachine.GCodeOutputPlotOriginAdjust_Y * gcFile.StateMachine.IsoPlotPointsPerAppUnit);
                rmLine = new GCodeCmd_RapidMove(workingPlotOrigin_X, workingPlotOrigin_Y, "Move to Origin");
                gcFile.AddLine(rmLine);
            }

            // M05 stop spindle
            cwLine = new GCodeCmd_CommandWord(GCodeCmd.GCODEWORD_SPINDLESTOP, "Stop spindle");
            gcFile.AddLine(cwLine);

            // M02 Program end
            cwLine = new GCodeCmd_CommandWord(GCodeCmd.GCODEWORD_PROGRAMEND, "Program End");
            gcFile.AddLine(cwLine);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Manages the process of producing a bed flattening GCode file
        /// </summary>
        /// <param name="errStr">an error string</param>
        /// <param name="fileManagerObj">the file manager</param>
        /// <param name="gerberFile">the gerber file</param>
        /// <param name="edgeMillGCodeFile">the isolation Gcode file which produced the edge mill code</param>
        /// <param name="outGCodeFile">we return the refpins gcode here</param>
        /// <returns>z success, -ve err not reported, +ve err reported</returns>
        private int CreateBedFlatteningGCode(FileManager fileManagerObj, GerberFile gerberFile, GCodeFile edgeMillGCodeFile, out GCodeFile outGCodeFile, ref string errStr)
        {

            errStr = "";
            DateTime conversionStart;
            DateTime conversionEnd;
            int retInt;
            float xMin = 0;
            float xMax;
            float yMin;
            float yMax;

            LogMessage("CreateBedFlatteningGCode called");
            outGCodeFile = null;

            if (fileManagerObj == null)
            {
                errStr = "Generating BedFlattening GCode: No fileManagerObj - please see the logs.";
                return -50;
            }
            if (fileManagerObj.OperationMode != FileManager.OperationModeEnum.BoardEdgeMill)
            {
                errStr = "Generating BedFlattening GCode: File Manager is not in BoardEdgMill mode.\n\nPlease see the logs.";
                return -51;
            }

            if (edgeMillGCodeFile == null)
            {
                errStr = "Generating BedFlattening GCode: Internal error. edgeMillGCodeFile == null";
                return -99;
            }
            if (edgeMillGCodeFile.SourceLines.Count == 0)
            {
                errStr = "Generating BedFlattening GCode: No lines were found in the EdgeMill GCode file.\n\nAre there PCB traces in that file?";
                return -100;
            }
            if (gerberFile == null)
            {
                errStr = "Generating BedFlattening GCode: Internal error. gerberFile == null";
                return -101;
            }
            if (gerberFile.SourceLines.Count == 0)
            {
                errStr = "Generating ReferencePin GCode: No lines were found in the gerberFile file.\n\nAre there PCB traces in that file?";
                return -102;
            }

            // find the mill width
            float millWidth = fileManagerObj.BedFlatteningMillWidth;
            if (millWidth <= 0)
            {
                errStr = "Generating BedFlattening GCode: The BedFlatteningMillWidth is less than or equal to zero. Please adjust the File Manager.";
                return -103;
            }
            const float DEFAULT_OVERLAP_PERCENT = 0.50f;
            float overlapScaleFactor = DEFAULT_OVERLAP_PERCENT;
            try
            {

                if (fileManagerObj.BedFlatteningSizeMode == FileManager.BedFlatteningSizeModeEnum.Add_Margin_To_Border)
                {
                    // find the max and min values of all coordinates
                    if (edgeMillGCodeFile.AreXYMaxMinOk() == false)
                    {
                        errStr = "Generating BedFlattening GCode: The maximum and minimum XY coordinates could not be found in the GCode file.\n\nIs there a PCB border outline in that file?";
                        return -101;
                    }
                    if (edgeMillGCodeFile.DoXYMaxMinEncloseAnArea() == false)
                    {
                        errStr = "Generating BedFlattening GCode: The maximum and minimum XY coordinates in the GCode file do not enclose an area.\n\nIs there a PCB border outline in that file?";
                        return -102;
                    }
                    // this is the margin we use for the bed flattening code
                    float edgeMargin = fileManagerObj.BedFlatteningMargin;
                    if (edgeMargin < 0) edgeMargin = 0;
                    // NOTE: because the XY max/min are taken from the CurrentEdgeMillGCodeFile the 
                    //       values for the max/min are the centerline of the cut around the border
                    //       This means the BedFlattening code will have an inherent margin of 
                    //       1/2 EdgeMill cut width withoug the addition of any extra edge margin.
                    xMin = edgeMillGCodeFile.XMinValue - edgeMargin;
                    yMin = edgeMillGCodeFile.YMinValue - edgeMargin;
                    xMax = edgeMillGCodeFile.XMaxValue + edgeMargin;
                    yMax = edgeMillGCodeFile.YMaxValue + edgeMargin;
                }
                else
                {
                    if (fileManagerObj.BedFlatteningSizeX <= millWidth)
                    {
                        errStr = "Generating BedFlattening GCode: The BedFlatteningSizeX parameter in the File Manager is less than or equal to the width of the mill.\n\nIt is not possible to generate any GCode.";
                        return -5102;
                    }
                    if (fileManagerObj.BedFlatteningSizeY <= millWidth)
                    {
                        errStr = "Generating BedFlattening GCode: The BedFlatteningSizeY parameter in the File Manager is less than or equal to the width of the mill.\n\nIt is not possible to generate any GCode.";
                        return -5103;
                    }
                    xMin = 0f;
                    yMin = 0f;
                    xMax = fileManagerObj.BedFlatteningSizeX;
                    yMax = fileManagerObj.BedFlatteningSizeY;
                }

                // start the clock
                conversionStart = DateTime.Now;

                LogMessage("CreateBedFlatteningGCode: Building Empty GCode File");
                // build a new GCode object with the application parameters here
                outGCodeFile = BuildEmptyGCodeFileWithCurrentApplicationParameters(gerberFile, fileManagerObj);
                // populate with file specific options from the FileManagers
                outGCodeFile.GCodeFileManager = fileManagerObj;

                // set it up with the standard header lines
                PopulateGCodeFileWithStandardHeaderLines(ref outGCodeFile, gerberFile, null, true);

                LogMessage("CreateBedFlatteningGCode: ConvertIsolationSegmentsToGCode starting");
                // now generate a pocket suitable for bed flattening
                retInt = outGCodeFile.GeneratePocketGCode(currentGerberFile.StateMachine.IsoPlotPointsPerAppUnit,
                                                    xMin,
                                                    yMin,
                                                    xMax,
                                                    yMax,
                                                    millWidth,
                                                    overlapScaleFactor,
                                                    ref errStr);
                if (retInt != 0)
                {
                    LogMessage("CreateBedFlatteningGCode: GeneratePocketGCode Error, err=" + retInt.ToString() + ", ErrStr=" + errStr);
                    return -371;
                }
                LogMessage("CreateBedFlatteningGCode: GeneratePocketGCode successful");

                // now add the footer lines
                PopulateGCodeFileWithStandardFooterLines(ref outGCodeFile, gerberFile, null);

                // stop the clock
                conversionEnd = DateTime.Now;
                LogMessage("GCode Conversion: total elapsed time is " + OISUtils.ConvertTimeSpanToHumanReadableTimeInterval(conversionEnd.Subtract(conversionStart)));
                return 0;
            }
            catch (Exception ex)
            {
                LogMessage("CreateBedFlatteningGCode Exception occurred: " + ex.Message);
                LogMessage("CreateBedFlatteningGCode stack trace=" + ex.StackTrace);
                OISMessageBox("An error occurred when converting to GCode.\n\n" + ex.Message + "\n\nPlease see the log file for more details.");
                return 900;
            }

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Manages the process of producing a reference pin GCode file. The gerber
        /// file is expected to contain the reference pins among the pads in
        /// its PadCenterPointList. It is also assume that these have been checked
        /// </summary>
        /// <param name="errStr">an error string</param>
        /// <param name="fileManagerObj">the file manager</param>
        /// <param name="gerberFile">the gerber file</param>
        /// <param name="outGCodeFile">we return the refpins gcode here</param>
        /// <returns>z success, -ve err not reported, +ve err reported</returns>
        private int CreateReferencePinGCode(FileManager fileManagerObj, GerberFile gerberFile, out GCodeFile outGCodeFile, ref string errStr)
        {
            outGCodeFile = null;

            errStr = "";
            DateTime conversionStart;
            DateTime conversionEnd;
            int retInt;

            LogMessage("CreateReferencePinGCode called");
            
            outGCodeFile = null;

            if (fileManagerObj == null)
            {
                errStr = "Generating ReferencePin GCode: No fileManagerObj - please see the logs.";
                return -50;
            }
            if (fileManagerObj.OperationMode != FileManager.OperationModeEnum.IsolationCut)
            {
                errStr = "Generating ReferencePin GCode: File Manager is not in IsolationCut mode.\n\nPlease see the logs.";
                return -51;
            }

            if (gerberFile == null)
            {
                errStr = "Generating ReferencePin GCode: Internal error. gerberFile == null";
                return -101;
            }
            if (gerberFile.SourceLines.Count == 0)
            {
                errStr = "Generating ReferencePin GCode: No lines were found in the gerberFile file.\n\nAre there PCB traces in that file?";
                return -102;
            }

            try
            {
                // start the clock
                conversionStart = DateTime.Now;

                LogMessage("CreateReferencePinGCode: Building Empty GCode File");
                // build a new GCode object with the application parameters here
                outGCodeFile = BuildEmptyGCodeFileWithCurrentApplicationParameters(gerberFile, fileManagerObj);
                // populate with file specific options from the FileManagers
                outGCodeFile.GCodeFileManager = fileManagerObj;

                // set it up with the standard header lines
                PopulateGCodeFileWithStandardHeaderLines(ref outGCodeFile, gerberFile, null, true);

                LogMessage("CreateReferencePinGCode: Create RefPins GCode starting");

                // can we find any reference pins? The PadCenterPoint list will have these from the Gerber file
                if (gerberFile.StateMachine.PadCenterPointList.Count == 0)
                {
                    errStr = "No pads were found in the Gerber file.\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                    OISMessageBox(errStr);
                    return 201;
                }
                // ok we have some pads have we got any of the required size?
                List<GerberPad> refPadsList = new List<GerberPad>();
                foreach (GerberPad padObj in gerberFile.StateMachine.PadCenterPointList)
                {
                    // test it
                    if (padObj.IsRefPin == false) continue;
                    // we found one, add it
                    refPadsList.Add(padObj);
                    //DebugMessage("padObj.PadDiameter = " + padObj.PadDiameter.ToString());
                }
                if (refPadsList.Count == 0)
                {
                    errStr = "No Reference Pin pads were found in the Gerber file. The File Manager thinks they should have diameter: " + fileManagerObj.ReferencePinPadDiameter.ToString() + "\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                    OISMessageBox(errStr);
                    return 202;
                }
                // we have to have at least 2 pins
                if (refPadsList.Count < 2)
                {
                    errStr = "Only one Reference Pin pad of diameter: " + fileManagerObj.ReferencePinPadDiameter.ToString() + " was found in the Gerber file. At least two are required.\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                    OISMessageBox(errStr);
                    return 203;
                }

                // OK, everything looks good, the reference pin pads seem to be setup correctly
                foreach (GerberPad gcPadObj in refPadsList)
                {
                    int tmpX = (int)(gerberFile.ConvertXCoordToOriginCompensatedFlipped(gcPadObj.X0) * gerberFile.StateMachine.IsoPlotPointsPerAppUnit);
                    int tmpY = (int)(gerberFile.ConvertYCoordToOriginCompensatedFlipped(gcPadObj.Y0) * gerberFile.StateMachine.IsoPlotPointsPerAppUnit);
                    //DebugMessage("zxtmpX=" + tmpX.ToString() + ", zxtmpY=" + tmpY.ToString());

                    // now drill the hole for our reference pin
                    retInt = outGCodeFile.AddPadTouchDownOrDrillLine(tmpX, tmpY, fileManagerObj.ReferencePinPadDiameter, ref errStr);
                    if (retInt != 0)
                    {
                        LogMessage("CreateReferencePinGCode: AddDrillCodeLines Error, err=" + retInt.ToString() + ", ErrStr=" + errStr);
                        return -371;
                    }
                }

                LogMessage("CreateReferencePinGCode: GenerateRefPinGCode successful");

                // now add the footer lines
                PopulateGCodeFileWithStandardFooterLines(ref outGCodeFile, gerberFile, null);
                // stop the clock
                conversionEnd = DateTime.Now;
                LogMessage("GCode Ref Pins Conversion: total elapsed time is " + OISUtils.ConvertTimeSpanToHumanReadableTimeInterval(conversionEnd.Subtract(conversionStart)));
                return 0;
            }
            catch (Exception ex)
            {
                LogMessage("CreateReferencePinGCode Exception occurred: " + ex.Message);
                LogMessage("CreateReferencePinGCode stack trace=" + ex.StackTrace);
                OISMessageBox("An error occurred when converting to GCode.\n\n"+ex.Message+"\n\nPlease see the log file for more details.");
                return 900;

            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Manages the process of producing a drill GCode file
        /// </summary>
        /// <param name="errStr">an error string</param>
        /// <param name="fileManagerObj">the file manager</param>
        /// <param name="excellonFile">the excellon file we process</param>
        /// <param name="outGCodeFile">the gcode file we return</param>
        /// <returns>z success, -ve err not reported, +ve err reported</returns>
        private int CreateDrillGCode(FileManager fileManagerObj, ExcellonFile excellonFile, out GCodeFile outGCodeFile, ref string errStr)
        {
            outGCodeFile = null;

            DateTime conversionStart;
            DateTime conversionEnd;
            int retInt;

            LogMessage("CreateDrillGCode called");

            errStr = "";
            outGCodeFile = null;

            if (fileManagerObj == null)
            {
                errStr = "Generating Drill GCode: No fileManagerObj - please see the logs.";
                return -50;
            }
            if (fileManagerObj.OperationMode != FileManager.OperationModeEnum.Excellon)
            {
                errStr = "Generating Drill GCode: File Manager is not in Excellon mode.\n\nPlease see the logs.";
                return -51;
            }

            if (excellonFile == null)
            {
                errStr = "Generating Drill GCode: Internal error. excellonFile == null";
                return -101;
            }
            // this code is only ever derived from the excellon code
            if (excellonFile.SourceLines.Count == 0)
            {
                errStr = "Generating Drill GCode: No lines were found in the excellon file.\n\nIs there a Excellon file open?";
                return -102;
            }

            try
            {
                // start the clock
                conversionStart = DateTime.Now;

                LogMessage("CreateDrillGCode: Building Empty GCode File");
                // build a new GCode object with the application parameters here
                outGCodeFile = BuildEmptyGCodeFileWithCurrentApplicationParameters(excellonFile, fileManagerObj);
                // populate with file specific options from the FileManagers
                outGCodeFile.GCodeFileManager = fileManagerObj;

                // set it up with the standard header lines
                PopulateGCodeFileWithStandardHeaderLines(ref outGCodeFile, null, excellonFile, true);

                LogMessage("CreateDrillGCode: ConvertToGCode starting");

                // We Build the GCode file by converting each Excellon lines. The excellon line Object
                // knows how to convert itself
                List<GCodeCmd> gcLineList = null;
                foreach (ExcellonLine lineObj in excellonFile.SourceLines)
                {
                    retInt = lineObj.GetGCodeCmd(excellonFile.StateMachine, out gcLineList);
                    if (retInt != 0)
                    {
                        errStr = "Error " + retInt.ToString() + " returned when converting excellon file line number: " + lineObj.LineNumber.ToString();
                        LogMessage(errStr);
                        return -342;
                    }
                    if (gcLineList == null)
                    {
                        // this should never happen
                        errStr = "Null line list returned when converting excellon file line number: " + lineObj.LineNumber.ToString();
                        LogMessage(errStr);
                        return -343;
                    }
                    // add the lines to our GCode file
                    foreach (GCodeCmd gcLine in gcLineList)
                    {
                        outGCodeFile.AddLine(gcLine);
                    }
                }

                // now add the footer lines
                PopulateGCodeFileWithStandardFooterLines(ref outGCodeFile, null, excellonFile);

                // stop the clock
                conversionEnd = DateTime.Now;
                LogMessage("GCode Drill File conversion: total elapsed time is " + OISUtils.ConvertTimeSpanToHumanReadableTimeInterval(conversionEnd.Subtract(conversionStart)));
                return 0;
            }
            catch (Exception ex)
            {
                LogMessage("CreateDrillGCode Exception occurred: " + ex.Message);
                LogMessage("CreateDrillGCode stack trace=" + ex.StackTrace);
                OISMessageBox("An error occurred when converting to GCode.\n\n" + ex.Message + "\n\nPlease see the log file for more details.");
                return 900;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Manages the process of converting the Gerber file to isolation cut GCode
        /// </summary>
        /// <param name="errStr">an error string</param>
        /// <param name="gerberFileToConvert">the gerber file we convert</param>
        /// <param name="fileManagerObj">the file manager object to use</param>
        /// <param name="outputIsoPlotBuilder">the IsoPlotObject we built</param>
        /// <param name="outputGCodeFile">the GCodeFile we created</param>
        /// <returns>z success, -ve err not reported, +ve err reported</returns>
        private int CreateIsolationCutGCode(FileManager fileManagerObj, GerberFile gerberFileToConvert, out GCodeFile outputGCodeFile, out IsoPlotBuilder outputIsoPlotBuilder, ref string errStr)
        {
            GCodeCmd_Comment coLine;
            errStr = "";
            int retInt;
            TimeSpan timeTaken;

            LogMessage("CreateIsolationCutGCode called");

            outputGCodeFile = null;
            outputIsoPlotBuilder = null;
            errStr = "";

            // reset these flags
            gerberToGCodeStep1Successful = false;
            gerberToGCodeStep2Successful = false;
            gerberToGCodeStep3Successful = false;


            if (fileManagerObj == null)
            {
                OISMessageBox("No File Manager Object.\n\nPlease see the log file.");
                return 98;
            }
            if (gerberFileToConvert == null)
            {
                OISMessageBox("No Gerber File.\n\nPlease see the log file.");
                return 99;
            }

            if (gerberFileToConvert.SourceLines.Count == 0)
            {
                OISMessageBox("No Gerber File is open.\n\nPlease see the log file.");
                return 100;
            }

            try
            {
                // start the clock
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // #################
                // ### Perform Step 1
                // #################
                LogMessage("CreateIsolationCutGCode gerberFileToConvert.PerformGerberToGCodeStep1 starting");
                retInt = gerberFileToConvert.PerformGerberToGCodeStep1(out outputIsoPlotBuilder, ctlPlotViewer1.VirtualPlotSize, ctlPlotViewer1.IsoPlotPointsPerAppUnit, ref errStr);
                if (retInt != 0)
                {
                    LogMessage("PerformGerberToGCodeStep1 Error, err=" + retInt.ToString() + ", ErrStr=" + errStr);
                    return -271;
                }
                if (outputIsoPlotBuilder == null)
                {
                    LogMessage("PerformGerberToGCodeStep1 Error, isoPlotObjOut==null");
                    return -272;
                }
                // set this now
                outputIsoPlotBuilder.CurrentFileManager = CurrentFileManager;
                LogMessage("CreateIsolationCutGCode: gerberFileToConvert.PerformGerberToGCodeStep1 successful");
                gerberToGCodeStep1Successful = true;

                // stop the clock
                timer.Stop();
                timeTaken = timer.Elapsed;
                string isoStep1ElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
                LogMessage("GCode Conversion: isoStep1 elapsed time is " + isoStep1ElapsedTime);

                // #################
                // ### perform Step 2
                // #################
                timer  = Stopwatch.StartNew();

                LogMessage("CreateIsolationCutGCode IsoPlotBuilder.PerformSecondaryIsolationArrayProcessing starting");
                retInt = outputIsoPlotBuilder.PerformSecondaryIsolationArrayProcessing(ref errStr);
                if (retInt != 0)
                {
                    LogMessage("PerformSecondaryIsolationArrayProcessing Error, err=" + retInt.ToString() + ", ErrStr=" + errStr);
                    return -371;
                }
                LogMessage("CreateIsolationCutGCode: PerformSecondaryIsolationArrayProcessing successful");
                gerberToGCodeStep2Successful = true;

                // stop the clock
                timer.Stop();
                timeTaken = timer.Elapsed;
                string isoStep2ElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
                //      isoStep2End = DateTime.Now;
                LogMessage("GCode Conversion: isoStep2 elapsed time is " + isoStep2ElapsedTime);

                LogMessage("CreateIsolationCutGCode: Building Empty GCode File");
                // build a new GCode object with the application parameters here
                outputGCodeFile = BuildEmptyGCodeFileWithCurrentApplicationParameters(gerberFileToConvert, fileManagerObj);
                // populate with file specific options from the FileManagers
                outputGCodeFile.GCodeFileManager = fileManagerObj;

                // set it up with the standard header lines
                PopulateGCodeFileWithStandardHeaderLines(ref outputGCodeFile, gerberFileToConvert, null, true);

                // #################
                // ### perform Step 3
                // #################
                timer = Stopwatch.StartNew();

                LogMessage("CreateIsolationCutGCode: ConvertIsolationSegmentsToGCode starting");
                // now convert the isolation segments in the IsoPlotBuilder to GCodeCmd objects in the GCodeFile
                retInt = outputIsoPlotBuilder.ConvertIsolationSegmentsToGCode(ref outputGCodeFile, ref errStr);
                if (retInt != 0)
                {
                    LogMessage("CreateIsolationCutGCode: ConvertIsolationSegmentsToGCode Error, err=" + retInt.ToString() + ", ErrStr=" + errStr);
                    return -372;
                }
                LogMessage("CreateIsolationCutGCode: ConvertIsolationSegmentsToGCode successful");
                gerberToGCodeStep3Successful = true;

                // stop the clock
                timer.Stop();
                timeTaken = timer.Elapsed;
                string isoStep3ElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
                LogMessage("GCode Conversion: isoStep3 elapsed time is " + isoStep3ElapsedTime);

                // #################
                // ### perform Step 4
                // #################
                timer = Stopwatch.StartNew();

                // did the user want pad touchdowns?
                if ((fileManagerObj.OperationMode == FileManager.OperationModeEnum.IsolationCut) &&
                    (fileManagerObj.IsoPadTouchDownsWanted == true))
                {
                    // yes, the user wants pad touchdowns
                    coLine = new GCodeCmd_Comment("... pad touchdown start ...");
                    outputGCodeFile.AddLine(coLine);

                    foreach (GerberPad padObj in gerberFileToConvert.StateMachine.PadCenterPointList)
                    {
                        // we do not do reference pins
                        if (padObj.IsRefPin == true) continue;
                        // we do not do ignore pins
                        if (padObj.IgnoreDueToSize == true) continue;
                        // generate the code
                        int tmpX = (int)(gerberFileToConvert.ConvertXCoordToOriginCompensatedFlipped(padObj.X0) * gerberFileToConvert.StateMachine.IsoPlotPointsPerAppUnit);
                        int tmpY = (int)(gerberFileToConvert.ConvertYCoordToOriginCompensatedFlipped(padObj.Y0) * gerberFileToConvert.StateMachine.IsoPlotPointsPerAppUnit);

                        // this is just a touchdown it should come in at the isoWidth
                        int touchdownDisplayDiameter = (int)((gerberFileToConvert.StateMachine.IsolationWidth * gerberFileToConvert.StateMachine.IsoPlotPointsPerAppUnit));
                        retInt = outputGCodeFile.AddPadTouchDownOrDrillLine(tmpX, tmpY, GCodeCmd_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForAlt1Cut, touchdownDisplayDiameter, FileManager.DEFAULT_DRILLDWELL_TIME, ref errStr);
                        if (retInt != 0)
                        {
                            LogMessage("GCode Conversion: AddDrillCodeLines Error, err=" + retInt.ToString() + ", ErrStr=" + errStr);
                            return -501;
                        }
                    }
                    coLine = new GCodeCmd_Comment("... pad touchdown end ...");
                    outputGCodeFile.AddLine(coLine);
                }

                // now add the footer lines
                PopulateGCodeFileWithStandardFooterLines(ref outputGCodeFile, gerberFileToConvert, null);

                // stop the clock
                timer.Stop();
                timeTaken = timer.Elapsed;
                string isoStep4ElapsedTime = timeTaken.ToString(@"m\:ss\.fff");
                LogMessage("GCode Conversion: isoStep4 elapsed time is " + isoStep4ElapsedTime);

                return 0;
            }
            catch (Exception ex)
            {
                LogMessage("CreateIsolationCutGCode Exception occurred: " + ex.Message);
                LogMessage("CreateIsolationCutGCode stack trace=" + ex.StackTrace);
                OISMessageBox("An error occurred when converting to GCode.\n\n"+ex.Message+"\n\nPlease see the log file for more details.");
                return 900;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GerberToGCodeStep1Successful flag
        /// </summary>
        public bool GerberToGCodeStep1Successful
        {
            get
            {
                return gerberToGCodeStep1Successful;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GerberToGCodeStep2Successful flag
        /// </summary>
        public bool GerberToGCodeStep2Successful
        {
            get
            {
                return gerberToGCodeStep2Successful;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GerberToGCodeStep3Successful flag
        /// </summary>
        public bool GerberToGCodeStep3Successful
        {
            get
            {
                return gerberToGCodeStep3Successful;
            }
        }


        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Removes all file managers
        /// </summary>
        private void buttonRemoveAllFileManagers_Click(object sender, EventArgs e)
        {
            DialogResult dlgRes = OISMessageBox_YesNo("This option will remove all File Managers.\n\nDo you wish to proceed?");
            if (dlgRes != DialogResult.Yes) return;
            // remove all
            this.ctlFileManagersDisplay1.RemoveAllFileManagers();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Removes all file managers
        /// </summary>
        private void buttonRemoveSelectedFileManager_Click(object sender, EventArgs e)
        {
            DialogResult dlgRes = OISMessageBox_YesNo("This option will remove the selected File Manager.\n\nDo you wish to proceed?");
            if (dlgRes != DialogResult.Yes) return;
            // remove selected
            this.ctlFileManagersDisplay1.RemoveSelectedFileManager();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a new file manager
        /// </summary>
        private void buttonAddNewFileManager_Click(object sender, EventArgs e)
        {
            // add new
            this.ctlFileManagersDisplay1.AddNewFileManager();
        }

    }
}

