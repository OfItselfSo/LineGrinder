using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using OISCommon;

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
    /// A control to contain portions of filenames and the actions we take
    /// when we open a gerber file with that name. 
    /// 
    /// For example, these configurations enable us to see
    /// a file with a name like [something]_bottom.ngc and because we match
    /// "_bottom.ngc" we know to flip it before creating the gcode,  
    /// the output extension and various other actions we might want to take
    /// with it
    /// </summary>
    /// <history>
    ///    10 Aug 10  Cynic - Started
    /// </history>
    public partial class ctlFileManagersDisplay : ctlOISBase
    {
        private BindingList<FileManager> fileManagersList = new BindingList<FileManager>();
        // this keeps track of whether any options changed
        private bool optionsChanged = false;

        private ApplicationUnitsEnum applicationUnits = ApplicationImplicitSettings.DEFAULT_APPLICATION_UNITS;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        public ctlFileManagersDisplay()
        {
            InitializeComponent();
            listBox1.DataSource = fileManagersList;
            listBox1.DisplayMember = "FilenamePattern";
            SyncButtonStates();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the currently set Application Units as an enum
        /// </summary>
        /// <history>
        ///    23 Nov 10  Cynic - Started
        /// </history>
        public ApplicationUnitsEnum ApplicationUnits
        {
            get
            {
                return applicationUnits;
            }
            set
            {
                applicationUnits = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts an incoming filename and returns the matching options object.
        /// We never return null. If we do not have a match we return a default
        /// object. The caller should check IsAtDefaults() on it upon return
        /// </summary>
        /// <param name="filePathAndNameIn">file name to find the object for, can contain a path</param>
        /// <history>
        ///    10 Aug Jul 10  Cynic - Started
        /// </history>
        public FileManager GetMatchingFileManagersObject(string filePathAndNameIn)
        {
            if (filePathAndNameIn == null) return new FileManager();
            if (filePathAndNameIn.Length == 0) return new FileManager();

            LogMessage("GetMatchingFileManagersObject called with>" + filePathAndNameIn);

            // strip the path (if any) off the filename
            string fileName = Path.GetFileName(filePathAndNameIn);
            if ((fileName == null) || (fileName.Length == 0))
            {
                LogMessage("GetMatchingFileManagersObject no filename after stripping path off>" + filePathAndNameIn);
                return new FileManager();
            }

            // now pattern match each FileManagers object in our list
            foreach(FileManager optObj in FileManagersList)
            {
                if (filePathAndNameIn.Contains(optObj.FilenamePattern) == true)
                {
                    // we found it
                    return optObj;
                }            
            }

            // not found, return a default object
            return GetDefaultFileManagerObject();
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/sets the list of file options. Will never get/set a null
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        [Browsable(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public BindingList<FileManager> FileManagersList
        {
            get
            {
                if (fileManagersList == null) fileManagersList = new BindingList<FileManager>();
                return fileManagersList;
            }
            set
            {
                fileManagersList = value;
                if (fileManagersList == null) fileManagersList = new BindingList<FileManager>();
                ResetListBoxContents();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We have to reset the listbox contents. This is pretty ugly but neither
        /// Invalidate() or Refresh() seem to work with this binding
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        private void ResetListBoxContents()
        {
            listBox1.DataSource = null;
            listBox1.DataSource = fileManagersList;
            listBox1.DisplayMember = "FilenamePattern";
            GetDefaultFileManagerObject();
            SyncButtonStates();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the currently selected file manager by name
        /// </summary>
        /// <param name="selectedManagerFilenamePattern">the file name pattern we match</param>
        /// <history>
        ///    31 Aug 10  Cynic - Started
        /// </history>
        public void SetSelectedManagerByName(string selectedManagerFilenamePattern)
        {
            if((selectedManagerFilenamePattern==null) || (selectedManagerFilenamePattern.Length==0)) return;
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                if (((FileManager)listBox1.Items[i]).FilenamePattern == selectedManagerFilenamePattern)
                {
                    listBox1.SelectedIndex = i;
                    return;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the name of the currently selected file manager
        /// </summary>
        /// <returns>the filename pattern of the selected file manager</returns>
        /// <history>
        ///    31 Aug 10  Cynic - Started
        /// </history>
        public string GetSelectedManagerName()
        {
            if(listBox1.SelectedItem == null) return null;
            if((listBox1.SelectedItem is FileManager)==false) return null;
            return ((FileManager)listBox1.SelectedItem).FilenamePattern;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the default file options object we just make this up new each time
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        ///    12 Sep 10  Cynic - No longer in List, we make it up each time
        /// </history>
        public FileManager GetDefaultFileManagerObject()
        {
            // if we get here it was not found
            FileManager tmpObj = new FileManager();
            tmpObj.OperationMode = FileManager.OperationModeEnum.Default;
            return tmpObj;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts all convertable values in the file managers to mm. It is assumed
        /// that the values are currently in inches. This is not checked.
        /// </summary>
        /// <history>
        ///    20 Nov 10  Cynic - Started
        /// </history>
        public void ConvertAllFileManagersToMM()
        {
            // now convert each filemanager object in the list
            foreach (FileManager optObj in FileManagersList)
            {
                optObj.ConvertFromInchToMM();
            }
            // reset the list box contents
            ResetListBoxContents();
            optionsChanged = true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts all convertable values in the file managers to inches It is assumed
        /// that the values are currently in mm. This is not checked.
        /// </summary>
        /// <history>
        ///    20 Nov 10  Cynic - Started
        /// </history>
        public void ConvertAllFileManagersToInches()
        {
            // now convert each filemanager object in the list
            foreach (FileManager optObj in FileManagersList)
            {
                optObj.ConvertFromMMToInch();
            }
            // reset the list box contents
            ResetListBoxContents();
            optionsChanged = true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/sets the optionsChanged flag
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        public bool OptionsChanged
        {
            get
            {
                 return optionsChanged;
            }
            set
            {
                optionsChanged = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a selected object changed event in the listbox
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FileManager selectedManager = (FileManager)listBox1.SelectedItem;
            propertyGrid1.SelectedObject = selectedManager;
            SetPropertyStateAppropriateToOperationMode(selectedManager);
            // this double sort is necessary. Or sometimes the weird reflection
            // code in SetPropertyStateAppropriateToOperationMode will take two
            // activations to properly display the properties
            propertyGrid1.PropertySort = PropertySort.Alphabetical;
            propertyGrid1.PropertySort = PropertySort.Categorized;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a property value changed event in the listbox
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            // always flag this
            optionsChanged = true;
            // we need to update the listbox if the FilenamePattern property
            // changes. This is so the user sees the new name
            if (e.ChangedItem.Label == "FilenamePattern")
            {
                ResetListBoxContents();
                return;
            }
            if (e.ChangedItem.Label == "OperationMode")
            {
                if ((e.ChangedItem.Value is FileManager.OperationModeEnum) == false) return;
                if ((propertyGrid1.SelectedObject is FileManager) == false) return;
                SetPropertyStateAppropriateToOperationMode((FileManager)propertyGrid1.SelectedObject);                
            }            
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the index of the specfied object
        /// </summary>
        /// <returns>index or zero for not found</returns>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public int GetIndexForFileManagersObject(FileManager fileManagerObj)
        {
            if (fileManagerObj == null) return -1;
            return FileManagersList.IndexOf(fileManagerObj);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Selects the file options object based on the filename pattern
        /// </summary>
        /// <returns>index or zero for not found</returns>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public void SelectFileManagersObjectByFilenamePattern(string filenamePatternIn)
        {
            if ((filenamePatternIn == null) || (filenamePatternIn.Length == 0)) return;

            for (int index = 0; index < FileManagersList.Count; index++)
            {
                if (FileManagersList[index].FilenamePattern == filenamePatternIn)
                {
                    listBox1.SelectedIndex = index;
                    return;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a file option object to the current display
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        public void AddFileManager(FileManager fileManagerObj)
        {
            optionsChanged = true;
            // do we have to convert? We do if it is not the default of inches
            if (ApplicationUnits == ApplicationUnitsEnum.MILLIMETERS)
            {
                fileManagerObj.ConvertFromInchToMM();
            }
            fileManagersList.Add(fileManagerObj);
            ResetListBoxContents();
            // this will create the default object if it is not present
            GetDefaultFileManagerObject();
            SyncButtonStates();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the reset button
        /// </summary>
        /// <history>
        ///    31 Aug 10  Cynic - Started
        /// </history>
        private void buttonReset_Click(object sender, EventArgs e)
        {
            DialogResult dlgRes = OISMessageBox_YesNo("The options in the selected File Manager will be reset to their default state. The FileNamePattern, Description and OperationMode items will not be changed.\n\nDo you wish to proceed?");
            if (dlgRes != DialogResult.Yes) return;
            optionsChanged = true;
            FileManager selectedManager = (FileManager)listBox1.SelectedItem;
            if (selectedManager == null) return;
            selectedManager.Reset(false);
            // select the newly reset item
            listBox1_SelectedIndexChanged(this, new EventArgs());
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the remove button
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        private void buttonRemove_Click(object sender, EventArgs e)
        {
            DialogResult dlgRes = OISMessageBox_YesNo("This option will remove the selected File Manager.\n\nDo you wish to proceed?");
            if (dlgRes != DialogResult.Yes) return;
            // remove the current selection in the list box
            int index = listBox1.SelectedIndex;
            if (index < 0) return;
            optionsChanged = true;
            // cannot remove when datasource is set
            listBox1.DataSource = null;
            fileManagersList.RemoveAt(index);
            listBox1.DataSource = fileManagersList;
            listBox1.DisplayMember = "FilenamePattern";
            // this will create the default object if it is not present
            GetDefaultFileManagerObject();
            SyncButtonStates();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Builds and returns a default file manager for a specified file extension
        /// </summary>
        /// <param name="fileExt">the file extension</param>
        /// <param name="addItToDisplay">true, add to the display, false, do not add</param>
        /// <param name="opMode">the operation mode of the file manager to create</param>
        /// <history>
        ///    07 Oct 10  Cynic - Started
        /// </history>
        public FileManager GetDefaultFileManagerForExtension(string fileExt, FileManager.OperationModeEnum opMode, bool addItToDisplay)
        {
            FileManager mgrObject = null;
            if ((fileExt==null) || (fileExt.Length==0)) return null;

            // get a default file manager
            mgrObject = GetDefaultFileManagerObject();
            mgrObject.OperationMode = opMode;
            mgrObject.FilenamePattern = fileExt;

            // add it, if required
            if (addItToDisplay == true)
            {
                AddFileManager(mgrObject);
            }

            return mgrObject;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the add button
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // ask the user what type of file manager they would like to use
            frmFileManagerChooser optChooser = new frmFileManagerChooser(GetDefaultFileManagerObject());
            // this is modal
            optChooser.ShowDialog();
            if (optChooser.DialogResult != DialogResult.OK) return;
            if (optChooser.OutputFileManagerObject == null) return;
            AddFileManager(optChooser.OutputFileManagerObject);
            // select the newly added item
            int index = GetIndexForFileManagersObject(optChooser.OutputFileManagerObject);
            listBox1.SelectedIndex = index;
            SyncButtonStates();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the remove button enabled or disabled state
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        private void SyncButtonStates()
        {
            if (fileManagersList.Count() == 0)
            {
                buttonRemove.Enabled = false;
            }
            else
            {
                buttonRemove.Enabled = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the readonly state of the properties so the user can only adjust 
        /// the ones appropriate to the current operation mode
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        private void SetPropertyStateAppropriateToOperationMode(FileManager optObject)
        {
            bool isoCutROState = false;
            bool edgeMillROState = false;
            bool excellonROState = false;
            //      bool textLabelROState = false;

            if (optObject == null) return;

            // IMPORTANT NOTE: this function will attempt to make certain
            //   options readonly based on the setting of the current operation mode 
            //   in the FileManagers object. If each and every property in the FileManagers 
            //   object does not have a [ReadOnlyAttribute(false)] attribute set on it then
            //   the act of setting one property to readonly will mark them ALL as readonly.
            //   So make sure each property in the FileManagers object has a 
            //   [ReadOnlyAttribute(false)] attribute whether it needs it or not!!!

            switch (optObject.OperationMode)
            {
                case FileManager.OperationModeEnum.IsolationCut:
                    isoCutROState = false;
                    edgeMillROState = true;
                    excellonROState = true;
               //     textLabelROState = true;
                    break;
                case FileManager.OperationModeEnum.BoardEdgeMill:
                    isoCutROState = true;
                    edgeMillROState = false;
                    excellonROState = true;
                    //textLabelROState = true;
                    break;
                case FileManager.OperationModeEnum.TextAndLabels:
                    isoCutROState = true;
                    edgeMillROState = true;
                    excellonROState = true;
                    //    textLabelROState = false;
                    break;
                case FileManager.OperationModeEnum.Excellon:
                    isoCutROState = true;
                    edgeMillROState = true;
                    excellonROState = false;
                    //    textLabelROState = false;
                    break;
                default:
                    isoCutROState = false;
                    edgeMillROState = false;
                    excellonROState = false;
                //    textLabelROState = false;
                    break;
            }
            // isocut
            SetPropertyBrowsableState(optObject, "IsoGCodeFileOutputExtension", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoFlipMode", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoZCutLevel", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoZMoveLevel", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoZClearLevel", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoZFeedRate", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoXYFeedRate", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoCutWidth", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoPadTouchDownsWanted", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoPadTouchDownZLevel", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoCutGCodeEnabled", isoCutROState);

            // edge mill
            SetPropertyBrowsableState(optObject, "EdgeMillGCodeFileOutputExtension", edgeMillROState);
            SetPropertyBrowsableState(optObject, "EdgeMillZCutLevel", edgeMillROState);
            SetPropertyBrowsableState(optObject, "EdgeMillZMoveLevel", edgeMillROState);
            SetPropertyBrowsableState(optObject, "EdgeMillZClearLevel", edgeMillROState);
            SetPropertyBrowsableState(optObject, "EdgeMillZFeedRate", edgeMillROState);
            SetPropertyBrowsableState(optObject, "EdgeMillXYFeedRate", edgeMillROState);
            SetPropertyBrowsableState(optObject, "EdgeMillCutWidth", edgeMillROState);
            SetPropertyBrowsableState(optObject, "EdgeMillingGCodeEnabled", edgeMillROState);
            SetPropertyBrowsableState(optObject, "EdgeMillNumTabs", edgeMillROState);
            SetPropertyBrowsableState(optObject, "EdgeMillTabWidth", edgeMillROState);

            // bed flattening
            SetPropertyBrowsableState(optObject, "BedFlatteningSizeMode", edgeMillROState);
            SetPropertyBrowsableState(optObject, "BedFlatteningSizeX", edgeMillROState);
            SetPropertyBrowsableState(optObject, "BedFlatteningSizeY", edgeMillROState);
            SetPropertyBrowsableState(optObject, "BedFlatteningGCodeEnabled", edgeMillROState);
            SetPropertyBrowsableState(optObject, "BedFlatteningGCodeFileOutputExtension", edgeMillROState);
            SetPropertyBrowsableState(optObject, "BedFlatteningZCutLevel", edgeMillROState);
            SetPropertyBrowsableState(optObject, "BedFlatteningZClearLevel", edgeMillROState);
            SetPropertyBrowsableState(optObject, "BedFlatteningZFeedRate", edgeMillROState);
            SetPropertyBrowsableState(optObject, "BedFlatteningXYFeedRate", edgeMillROState);
            SetPropertyBrowsableState(optObject, "BedFlatteningMillWidth", edgeMillROState);
            SetPropertyBrowsableState(optObject, "BedFlatteningMargin", edgeMillROState);

            // reference pins
            SetPropertyBrowsableState(optObject, "ReferencePinGCodeEnabled", isoCutROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsGCodeFileOutputExtension", isoCutROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsZDrillDepth", isoCutROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsZClearLevel", isoCutROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsZFeedRate", isoCutROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsXYFeedRate", isoCutROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsMaxNumber", isoCutROState);
            SetPropertyBrowsableState(optObject, "ReferencePinPadDiameter", isoCutROState);

            // excellon
            SetPropertyBrowsableState(optObject, "DrillingCoordinateZerosMode", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingNumberOfDecimalPlaces", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingGCodeFileOutputExtension", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingZDepth", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingZClearLevel", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingZFeedRate", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingXYFeedRate", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingGCodeEnabled", excellonROState);
            
            // text and labels
            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the readonly attribute on a property so the user cannot adjust if if
        /// it is not appropriate to the current operation mode
        /// </summary>
        /// <remarks>
        /// Credits: the basic mechanisim for performing this operation is derived
        /// from: http://www.codeproject.com/KB/tabs/ExploringPropertyGrid.aspx</remarks>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        private void SetPropertyReadOnlyState(FileManager fileManagersObj, string propertyName, bool readOnlyState)
        {
            if (fileManagersObj == null) return;
            if ((propertyName == null) || (propertyName.Length == 0)) return;

            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(fileManagersObj.GetType())[propertyName];
            ReadOnlyAttribute attrib = (ReadOnlyAttribute)descriptor.Attributes[typeof(ReadOnlyAttribute)];
            FieldInfo isReadOnly = attrib.GetType().GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
            isReadOnly.SetValue(attrib, readOnlyState);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the readonly attribute on a property so the user cannot adjust if if
        /// it is not appropriate to the current operation mode
        /// </summary>
        /// <remarks>
        /// Credits: the basic mechanisim for performing this operation is derived
        /// from: http://www.codeproject.com/KB/tabs/ExploringPropertyGrid.aspx</remarks>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        private void SetPropertyBrowsableState(FileManager fileManagersObj, string propertyName, bool isNonBrowsable)
        {
            bool wantBrowsable;
            if (fileManagersObj == null) return;
            if ((propertyName == null) || (propertyName.Length == 0)) return;

            if (isNonBrowsable == true) wantBrowsable = false;
            else wantBrowsable = true;

            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(fileManagersObj.GetType())[propertyName];
            BrowsableAttribute attrib = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
            FieldInfo browsable = attrib.GetType().GetField("browsable", BindingFlags.NonPublic | BindingFlags.Instance);
            browsable.SetValue(attrib, wantBrowsable);
        }

    }
}
