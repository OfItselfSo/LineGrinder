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
    /// Offers the user the choice of which kind of FileManagers object to create
    /// </summary>
    /// <history>
    ///    23 Aug 10  Cynic - Started
    /// </history>
    public partial class frmFileManagerChooser : frmOISBase
    {
        private FileManager defaultFileManagerObject = null;
        private FileManager outputFileManagerObject = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public frmFileManagerChooser(FileManager defaultFileManagerObjectIn)
        {
            InitializeComponent();
            DefaultFileManagerObject = defaultFileManagerObjectIn;
            DialogResult = DialogResult.Cancel;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/sets the default file option object. Will never get/set null
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public FileManager DefaultFileManagerObject
        {
            get
            {
                if (defaultFileManagerObject == null) defaultFileManagerObject = new FileManager(FileManager.OperationModeEnum.Default);
                return defaultFileManagerObject;
            }
            set
            {
                defaultFileManagerObject = value;
                if (defaultFileManagerObject == null) defaultFileManagerObject = new FileManager(FileManager.OperationModeEnum.Default);

            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the output file option object. Will acquire its default
        /// values from the DefaultFileManagerObject. Will be null if form cancelled
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public FileManager OutputFileManagerObject
        {
            get
            {
                return outputFileManagerObject;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a press on the iso cut button
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        private void buttonFileOptIsoCut_Click(object sender, EventArgs e)
        {
            // clone the default object so we pick up the settings
            outputFileManagerObject = FileManager.DeepClone(DefaultFileManagerObject);
            // set this now
            outputFileManagerObject.OperationMode = FileManager.OperationModeEnum.IsolationCut;
            DialogResult = DialogResult.OK;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a press on the edge mill button
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        private void buttonFileOptEdgeMill_Click(object sender, EventArgs e)
        {
            // clone the default object so we pick up the settings
            outputFileManagerObject = FileManager.DeepClone(DefaultFileManagerObject);
            // set this now
            outputFileManagerObject.OperationMode = FileManager.OperationModeEnum.BoardEdgeMill;
            DialogResult = DialogResult.OK;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a press on the excellon button
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        private void buttonFileOptExcellon_Click(object sender, EventArgs e)
        {
            // clone the default object so we pick up the settings
            outputFileManagerObject = FileManager.DeepClone(DefaultFileManagerObject);
            // set this now
            outputFileManagerObject.OperationMode = FileManager.OperationModeEnum.Excellon;
            DialogResult = DialogResult.OK;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a press on the text and label button
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        private void buttonFileOptTextLabel_Click(object sender, EventArgs e)
        {
            OISMessageBox("Sorry, this mode is not available in this version.");
/*
            // clone the default object so we pick up the settings
            outputFileManagerObject = FileManager.DeepClone(DefaultFileManagerObject);
            // set this now
            outputFileManagerObject.OperationMode = FileManager.OperationModeEnum.TextAndLabels;
            DialogResult = DialogResult.OK;
 */
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a press on the defaults button
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        private void buttonFileOptDefaults_Click(object sender, EventArgs e)
        {
            DefaultFileManagerObject.Reset();
            outputFileManagerObject = null;
            DialogResult = DialogResult.OK;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a press on the cancel button
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
