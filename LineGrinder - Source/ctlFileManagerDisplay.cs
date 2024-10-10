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
    public partial class ctlFileManagersDisplay : ctlOISBase
    {
        private BindingList<FileManager> fileManagersList = new BindingList<FileManager>();
        // this keeps track of whether any options changed
        private bool optionsChanged = false;

        private ApplicationUnitsEnum defaultApplicationUnits = ApplicationImplicitSettings.DEFAULT_APPLICATION_UNITS;
        private ApplicationUnitsEnum outputApplicationUnits = ApplicationImplicitSettings.DEFAULT_APPLICATION_UNITS;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public ctlFileManagersDisplay()
        {
            InitializeComponent();
            listBoxFileManagers.DataSource = fileManagersList;
            listBoxFileManagers.DisplayMember = "FilenamePattern";
            SyncButtonStates();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the currently set Default Application Units as an enum
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ApplicationUnitsEnum DefaultApplicationUnits
        {
            get
            {
                return defaultApplicationUnits;
            }
            set
            {
                defaultApplicationUnits = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the currently set Output Application Units as an enum
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ApplicationUnitsEnum OutputApplicationUnits
        {
            get
            {
                return outputApplicationUnits;
            }
            set
            {
                outputApplicationUnits = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts an incoming filename and returns the matching file manager object.
        /// We never return null. If we do not have a match we return a default
        /// object. The caller should check IsAtDefaults() on it upon return
        /// </summary>
        /// <param name="filePathAndNameIn">file name to find the object for, can contain a path</param>
        /// <param name="wantDeepClone">if true we return a deep clone</param>
        public FileManager GetMatchingFileManager(string filePathAndNameIn, bool wantDeepClone)
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
                    if (wantDeepClone == true) return FileManager.DeepClone(optObj);
                    else return optObj;
                }            
            }

            // not found, return a default object
            return GetDefaultFileManagerObject();
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/sets the list of file options. Will never get/set a null
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        private void ResetListBoxContents()
        {
            listBoxFileManagers.DataSource = null;
            listBoxFileManagers.DataSource = fileManagersList;
            listBoxFileManagers.DisplayMember = "FilenamePattern";
            GetDefaultFileManagerObject();
            SyncButtonStates();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the currently selected file manager by name
        /// </summary>
        /// <param name="selectedManagerFilenamePattern">the file name pattern we match</param>
        public void SetSelectedManagerByName(string selectedManagerFilenamePattern)
        {
            if((selectedManagerFilenamePattern==null) || (selectedManagerFilenamePattern.Length==0)) return;
            for (int i = 0; i < listBoxFileManagers.Items.Count; i++)
            {
                if (((FileManager)listBoxFileManagers.Items[i]).FilenamePattern == selectedManagerFilenamePattern)
                {
                    listBoxFileManagers.SelectedIndex = i;
                    return;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the name of the currently selected file manager
        /// </summary>
        /// <returns>the filename pattern of the selected file manager</returns>
        public string GetSelectedManagerName()
        {
            if(listBoxFileManagers.SelectedItem == null) return null;
            if((listBoxFileManagers.SelectedItem is FileManager)==false) return null;
            return ((FileManager)listBoxFileManagers.SelectedItem).FilenamePattern;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the default file options object we just make this up new each time
        /// </summary>
        public FileManager GetDefaultFileManagerObject()
        {
            // if we get here it was not found
            FileManager tmpObj = new FileManager();
            tmpObj.OperationMode = FileManager.OperationModeEnum.Default;
            return tmpObj;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/sets the optionsChanged flag
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FileManager selectedManager = (FileManager)listBoxFileManagers.SelectedItem;
            propertyGridFileManager.SelectedObject = selectedManager;
            SetPropertyStateAppropriateToOperationMode(selectedManager);
            // this double sort is necessary. Or sometimes the weird reflection
            // code in SetPropertyStateAppropriateToOperationMode will take two
            // activations to properly display the properties
            propertyGridFileManager.PropertySort = PropertySort.Alphabetical;
            propertyGridFileManager.PropertySort = PropertySort.Categorized;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a property value changed event in the listbox
        /// </summary>
        private void propertyGridFileManager_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
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
                if ((propertyGridFileManager.SelectedObject is FileManager) == false) return;
                SetPropertyStateAppropriateToOperationMode((FileManager)propertyGridFileManager.SelectedObject);                
            }            
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the index of the specfied object
        /// </summary>
        /// <returns>index or zero for not found</returns>
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
        public void SelectFileManagersObjectByFilenamePattern(string filenamePatternIn)
        {
            if ((filenamePatternIn == null) || (filenamePatternIn.Length == 0)) return;

            for (int index = 0; index < FileManagersList.Count; index++)
            {
                if (FileManagersList[index].FilenamePattern == filenamePatternIn)
                {
                    listBoxFileManagers.SelectedIndex = index;
                    return;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a file option object to the current display. Note that all file 
        /// managers are created in INCHES by default. This is what sets them 
        /// to MM if the default application units setting is in MM.
        /// </summary>
        public void AddFileManager(FileManager fileManagerObj)
        {
            optionsChanged = true;
            // do we have to convert? We do if it is not the default of inches
            if (DefaultApplicationUnits == ApplicationUnitsEnum.MILLIMETERS)
            {
                // this will automatically convert
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
        private void buttonReset_Click(object sender, EventArgs e)
        {
            DialogResult dlgRes = OISMessageBox_YesNo("The options in the selected File Manager will be reset to their default state. The FileNamePattern, Description and OperationMode items will not be changed.\n\nDo you wish to proceed?");
            if (dlgRes != DialogResult.Yes) return;
            optionsChanged = true;
            FileManager selectedManager = (FileManager)listBoxFileManagers.SelectedItem;
            if (selectedManager == null) return;
            selectedManager.Reset(false, DefaultApplicationUnits);
            // select the newly reset item
            listBox1_SelectedIndexChanged(this, new EventArgs());
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the remove button
        /// </summary>
        private void buttonRemove_Click(object sender, EventArgs e)
        {
            DialogResult dlgRes = OISMessageBox_YesNo("This option will remove the selected File Manager.\n\nDo you wish to proceed?");
            if (dlgRes != DialogResult.Yes) return;
            // removed selected
            RemoveSelectedFileManager();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the remove all button
        /// </summary>
        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            DialogResult dlgRes = OISMessageBox_YesNo("This option will remove all File Managers.\n\nDo you wish to proceed?");
            if (dlgRes != DialogResult.Yes) return;
            // remove all
            RemoveAllFileManagers();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Removes the selected file manager
        /// </summary>
        public void RemoveSelectedFileManager()
        {
            // remove the current selection in the list box
            int index = listBoxFileManagers.SelectedIndex;
            if (index < 0) return;
            optionsChanged = true;
            // cannot remove when datasource is set
            listBoxFileManagers.DataSource = null;
            fileManagersList.RemoveAt(index);
            listBoxFileManagers.DataSource = fileManagersList;
            listBoxFileManagers.DisplayMember = "FilenamePattern";
            // this will create the default object if it is not present
            GetDefaultFileManagerObject();
            SyncButtonStates();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Removes all file managers
        /// </summary>
        public void RemoveAllFileManagers()
        {
            optionsChanged = true;
            // cannot remove when datasource is set
            listBoxFileManagers.DataSource = null;
            fileManagersList.Clear();
            listBoxFileManagers.DataSource = fileManagersList;
            listBoxFileManagers.DisplayMember = "FilenamePattern";
            // this will create the default object if it is not present
            GetDefaultFileManagerObject();
            SyncButtonStates();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a new file manager
        /// </summary>
        public void AddNewFileManager()
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
            listBoxFileManagers.SelectedIndex = index;
            SyncButtonStates();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Builds and returns a default file manager for a specified file extension
        /// </summary>
        /// <param name="fileExt">the file extension</param>
        /// <param name="addItToDisplay">true, add to the display, false, do not add</param>
        /// <param name="opMode">the operation mode of the file manager to create</param>
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
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            AddNewFileManager();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the remove button enabled or disabled state
        /// </summary>
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
        /// Syncs the enabled state of the control. This is the overall enabled/disabled
        /// state of the control. We are disabled when a file is open
        /// </summary>
        /// <param name="ctrlEnabledState">the enabled state for the control</param>
        public void SyncEnabledState(bool ctrlEnabledState)
        {
            // these stay enabled
            //labelFileManagers.Enabled = ctrlEnabledState;
            //labelFileManagerProperties.Enabled = ctrlEnabledState;
            //listBoxFileManagers.Enabled = ctrlEnabledState;

            propertyGridFileManager.Enabled = ctrlEnabledState;
            buttonAdd.Enabled = ctrlEnabledState;
            buttonRemove.Enabled = ctrlEnabledState;
            buttonRemoveAll.Enabled = ctrlEnabledState;
            buttonReset.Enabled = ctrlEnabledState;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the readonly state of the properties so the user can only adjust 
        /// the ones appropriate to the current operation mode
        /// </summary>
        private void SetPropertyStateAppropriateToOperationMode(FileManager optObject)
        {
            bool isoCutROState = false;
            bool ignoreFillState = false;
            bool isoCutRefPinROState = false;
            bool edgeMillROState = false;
            bool excellonROState = false;
            bool ignorePadROState = false;
            bool ignoreDrillROState = false;
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
                    ignorePadROState = false;
                    ignoreDrillROState = true;
                    ignoreFillState = false;
                    isoCutRefPinROState = false;
                    edgeMillROState = true;
                    excellonROState = true;
               //     textLabelROState = true;
                    break;
                case FileManager.OperationModeEnum.BoardEdgeMill:
                    isoCutROState = true;
                    ignorePadROState = true;
                    ignoreDrillROState = true;
                    ignoreFillState = true;
                    isoCutRefPinROState = true;
                    edgeMillROState = false;
                    excellonROState = true;
                    //textLabelROState = true;
                    break;
                case FileManager.OperationModeEnum.TextAndLabels:
                    isoCutROState = true;
                    ignorePadROState = true;
                    ignoreDrillROState = true;
                    ignoreFillState = true;
                    isoCutRefPinROState = true;
                    edgeMillROState = true;
                    excellonROState = true;
                    //    textLabelROState = false;
                    break;
                case FileManager.OperationModeEnum.Excellon:
                    isoCutROState = true;
                    ignorePadROState = true;
                    ignoreDrillROState = false;
                    ignoreFillState = true;
                    isoCutRefPinROState = true;
                    edgeMillROState = true;
                    excellonROState = false;
                    //    textLabelROState = false;
                    break;
                default:
                    isoCutROState = false;
                    ignorePadROState = true;
                    ignoreDrillROState = true;
                    ignoreFillState = true;
                    isoCutRefPinROState = false;
                    edgeMillROState = false;
                    excellonROState = false;
                //    textLabelROState = false;
                    break;
            }

            // ignore pad
            SetPropertyBrowsableState(optObject, "IgnorePadEnabled", ignorePadROState);
            SetPropertyBrowsableState(optObject, "IgnorePadDiameter", ignorePadROState);

            // ignore drill
            SetPropertyBrowsableState(optObject, "IgnoreDrillEnabled", ignoreDrillROState);
            SetPropertyBrowsableState(optObject, "IgnoreDrillDiameter", ignoreDrillROState);

            // isocut
            SetPropertyBrowsableState(optObject, "IsoGCodeFileOutputExtension", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoFlipMode", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoFlipAxisFoundBy", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoZCutLevel", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoZMoveLevel", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoZClearLevel", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoZFeedRate", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoXYFeedRate", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoCutWidth", isoCutROState);
            SetPropertyBrowsableState(optObject, "IsoPadTouchDownsWanted", isoCutROState);
            SetPropertyBrowsableState(optObject, "IgnoreFillAreas", ignoreFillState);
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
            SetPropertyBrowsableState(optObject, "EdgeMillFlipMode", edgeMillROState);
            SetPropertyBrowsableState(optObject, "EdgeMillFlipAxisFoundBy", edgeMillROState);

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
            SetPropertyBrowsableState(optObject, "ReferencePinGCodeEnabled", isoCutRefPinROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsAreIsoRouted", isoCutRefPinROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsGCodeFileOutputExtension", isoCutRefPinROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsZDrillDepth", isoCutRefPinROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsZClearLevel", isoCutRefPinROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsZFeedRate", isoCutRefPinROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsXYFeedRate", isoCutRefPinROState);
            SetPropertyBrowsableState(optObject, "ReferencePinsMaxNumber", isoCutRefPinROState);
            SetPropertyBrowsableState(optObject, "ReferencePinPadDiameter", isoCutRefPinROState);

            // excellon
            SetPropertyBrowsableState(optObject, "DrillFlipMode", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillFlipAxisFoundBy", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingCoordinateZerosMode", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingNumberOfDecimalPlaces", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingGCodeFileOutputExtension", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingZDepth", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingZClearLevel", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingZFeedRate", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingXYFeedRate", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingGCodeEnabled", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingReferencePinsEnabled", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingReferencePinDiameter", excellonROState);
            SetPropertyBrowsableState(optObject, "DrillingReferencePinsMaxNumber", excellonROState);
           
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
        private void SetPropertyBrowsableState(FileManager fileManagersObj, string propertyName, bool isNonBrowsable)
        {
            bool wantBrowsable;
            if (fileManagersObj == null) return;
            if ((propertyName == null) || (propertyName.Length == 0)) return;

            if (isNonBrowsable == true) wantBrowsable = false;
            else wantBrowsable = true;

            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(fileManagersObj.GetType())[propertyName];
            if(descriptor != null) {
	            BrowsableAttribute attrib = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
	            if(attrib != null) {
    		        FieldInfo browsable = attrib.GetType().GetField("browsable", BindingFlags.NonPublic | BindingFlags.Instance);
    		        if(browsable != null) {
            			browsable.SetValue(attrib, wantBrowsable);
            		}
            	}
            }
        }

    }
}

