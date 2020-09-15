using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;     
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
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
    /// A class to keep track of a wildcard in a filename and the properties, 
    /// settings and actions to take when that file is opened.
    /// </summary>
    /// <remarks>we do not inherit from OISBase here because that class is
    /// not serializable</remarks>
    /// <history>
    ///    23 Aug 10  Cynic - Started
    /// </history>
    [DataContract]
    public class FileManager 
    {
        // IMPORTANT NOTE: The SetBrowsableAttributesAppropriateToOperationMode() 
        //   call over in the ctlFileManagersDisplay will attempt to make certain
        //   options readonly based on the setting of the current operation mode 
        //   in the FileManagers object. If each and every property here does not 
        //   have a [ReadOnlyAttribute(false)] attribute set on it then they will
        //   ALL get marked as readOnly. So make sure each property has a 
        //   [ReadOnlyAttribute(false)] attribute whether it needs it or not!!!

        // IMPORTANT NOTE; If you add a property here make sure to also add a line
        //   in the IsAtDefaults() and Reset() functions.

        // this determines the method used to flip the display
        // so that bottom copper isolation modes can be cut
        public enum BedFlatteningSizeModeEnum
        {
            Add_Margin_To_Border,
            Absolute_Size
        }

        // this determines the method used to flip the display
        // so that bottom copper isolation modes can be cut
        public enum IsoFlipModeEnum
        {
            No_Flip,
            X_Flip,
            Y_Flip,
        }

        // this determines the style of the option
        public enum OperationModeEnum
        {
            Default,
            IsolationCut,
            TextAndLabels,
            BoardEdgeMill,
            Excellon,
        }
        public const OperationModeEnum DEFAULT_OPERATION_MODE = OperationModeEnum.Default;
        [DataMember]
        private OperationModeEnum operationMode = DEFAULT_OPERATION_MODE;

        // this determines whether we automatically adjust the origin
        // in the output GCode file
        public const bool DEFAULT_AUTOADJUSTORIGIN = false;
        [DataMember]
        private bool autoAdjustOrigin = DEFAULT_AUTOADJUSTORIGIN;

        // this determines whether we automatically generate GCodes when
        // the gerber file is opened
        public const bool DEFAULT_AUTOGENERATE_GCODE = false;
        [DataMember]
        private bool autoGenerateGCode = DEFAULT_AUTOGENERATE_GCODE;

        // this will cause line numbers to be output to the gcode file
        public const bool DEFAULT_SHOWGCODE_LINENUMBERS = false;
        [DataMember]
        private bool showGCodeLineNumbers = DEFAULT_SHOWGCODE_LINENUMBERS;

        // this determines whether we warn if we are overwriting on a save
        public const bool DEFAULT_OVERWRITEWARN = true;
        [DataMember]
        private bool warnAboutOverwriting = DEFAULT_OVERWRITEWARN;

        // this is the string which we use to find matching filenames
        public const string DEFAULT_OPMODE_FILENAME = "<<<Default File Manager>>>";
        public const string DEFAULT_FILENAME_PATTERN = "<edit this value in the property panel>";
        [DataMember]
        private string filenamePattern = DEFAULT_FILENAME_PATTERN;
        public const string DEFAULT_DESCRIPTION_OPMODE_DEFAULT = "Default Settings";
        public const string DEFAULT_DESCRIPTION = "";
        [DataMember]
        private string description = DEFAULT_DESCRIPTION;

        // this is how long we dwell in the bottom when drilling
        public const float DEFAULT_DRILLDWELL_TIME = 0.250f;

        // there are 25.4 mm to the inch
        private const float INCHTOMMSCALERx10 = 254;

        // ####################################################################
        // ##### Isolation Cut category variables
        // ####################################################################
        #region Isolation Cut category variables

        public const string DEFAULT_ISOGCODEFILE_OUTPUTEXTENSION = "_ISOLATION_GCODE.ngc";
        [DataMember]
        private string isoGCodeFileOutputExtension = DEFAULT_ISOGCODEFILE_OUTPUTEXTENSION;

        public const IsoFlipModeEnum DEFAULT_ISOFLIP_MODE = IsoFlipModeEnum.No_Flip;
        [DataMember]
        private IsoFlipModeEnum isoFlipMode = DEFAULT_ISOFLIP_MODE;

        // this determines whether we generate isocut GCodes
        public const bool DEFAULT_ISOCUTGCODE_ENABLED = true;
        [DataMember]
        private bool isoCutGCodeEnabled = DEFAULT_ISOCUTGCODE_ENABLED;

        // this is the distance into the material we cut this will be negative 
        // because zero is the surface of the pcb we are cutting
        public const float DEFAULT_ISOZCUTLEVEL = -0.002f;
        public const float DEFAULT_ISOALT1ZCUTLEVEL = -0.002f;
        [DataMember]
        private float isoZCutLevel = DEFAULT_ISOZCUTLEVEL;

        // this is the distance above the PCB we move the z axis to
        // so that we can hop from cut to cut. It will be positive
        // because it is above the surface of the pcb we are cutting
        public const float DEFAULT_ISOZMOVELEVEL = 0.10f;
        [DataMember]
        private float isoZMoveLevel = DEFAULT_ISOZMOVELEVEL;

        // this is the distance above the PCB we move the z axis to
        // so that we can move about. It will be positive
        // because it is above the surface of the pcb we are cutting
        // and should be large enough to clear all hold downs and clamps etc
        public const float DEFAULT_ISOZCLEARLEVEL = 0.250f;
        [DataMember]
        private float isoZClearLevel = DEFAULT_ISOZCLEARLEVEL;

        // this is the speed at which the tool (in application units per minute) moves vertically into the work.
        public const float DEFAULT_ISOZFEEDRATE = 20f;
        [DataMember]
        private float isoZFeedRate = DEFAULT_ISOZFEEDRATE;

        // this is the speed at which the tool (in application units per minute) moves horizontally over the work.
        public const float DEFAULT_ISOXYFEEDRATE = 15f;
        [DataMember]
        private float isoXYFeedRate = DEFAULT_ISOXYFEEDRATE;

        // this is essentially the diameter of the line the isolation milling bit cuts at the IsoZCutLevel
        public const float DEFAULT_ISOCUT_WIDTH = 0.010f;
        [DataMember]
        private float isoCutWidth = DEFAULT_ISOCUT_WIDTH;

        // this will cause the milling bit to dip into each pad to provide
        // a drilling center hole
        public const bool DEFAULT_ISOPADTOUCHDOWNS_WANTED = true;
        [DataMember]
        private bool isoPadTouchDownsWanted = DEFAULT_ISOPADTOUCHDOWNS_WANTED;

        // this is essentially the distance into the work the isolation milling bit moves
        // when performing pad touchdowns.
        private const float DEFAULT_ISOPADTOUCHDOWN_ZLEVEL = -0.005f;
        [DataMember]
        private float isoPadTouchDownZLevel = DEFAULT_ISOPADTOUCHDOWN_ZLEVEL;

        #endregion

        // ####################################################################
        // ##### Edge Mill category variables
        // ####################################################################
        #region Edge Mill category variables

        // this determines whether we generate EdgeMill GCodes
        public const bool DEFAULT_EDGEMILLINGGCODE_ENABLED = true;
        [DataMember]
        private bool edgeMillingGCodeEnabled = DEFAULT_EDGEMILLINGGCODE_ENABLED;

        public const string DEFAULT_EDGEMILL_OUTPUTEXTENSION = "_EDGEMILL_GCODE.ngc";
        [DataMember]
        private string edgeMillGCodeFileOutputExtension = DEFAULT_EDGEMILL_OUTPUTEXTENSION;

        // this is the distance into the material we cut this will be negative 
        // because zero is the surface of the pcb we are cutting
        public const float DEFAULT_EDGEMILLZCUTLEVEL = -0.2f;
        [DataMember]
        private float edgeMillZCutLevel = DEFAULT_EDGEMILLZCUTLEVEL;

        // this is the distance above the PCB we move the z axis to
        // so that we can hop from cut to cut. It will be positive
        // because it is above the surface of the pcb we are cutting
        public const float DEFAULT_EDGEMILLZMOVELEVEL = 0.10f;
        [DataMember]
        private float zEdgeMillMoveLevel = DEFAULT_EDGEMILLZMOVELEVEL;

        // this is the distance above the PCB we move the z axis to
        // so that we can move about. It will be positive
        // because it is above the surface of the pcb we are cutting
        // and should be large enough to clear all hold downs and clamps etc
        public const float DEFAULT_ZEDGEMILLCLEARLEVEL = 0.25f;
        [DataMember]
        private float edgeMillZClearLevel = DEFAULT_ZEDGEMILLCLEARLEVEL;

        // this is the speed at which the tool (in application units per minute) moves vertically into the work.
        public const float DEFAULT_EDGEMILLZFEEDRATE = 10f;
        [DataMember]
        private float edgeMillZFeedRate = DEFAULT_EDGEMILLZFEEDRATE;

        // this is the speed at which the tool (in application units per minute) moves horizontally over the work.
        public const float DEFAULT_EDGEMILLXYFEEDRATE = 5f;
        [DataMember]
        private float edgeMillXYFeedRate = DEFAULT_EDGEMILLXYFEEDRATE;

        // this is essentially the diameter of the hole the edge cutting mill bit cuts at the EdgeMillZCutLevel
        private const float DEFAULT_EDGEMILLCUT_WIDTH = 0.125f;
        [DataMember]
        private float edgeMillCutWidth = DEFAULT_EDGEMILLCUT_WIDTH;

        // this is the number of tabs we will place on the board edges
        private const int DEFAULT_EDGEMILLNUM_TABS = 4;
        [DataMember]
        private int edgeMillNumTabs = DEFAULT_EDGEMILLNUM_TABS;

        // this is width of the tabs
        private const float DEFAULT_EDGEMILLTAB_WIDTH = 0.125f;
        [DataMember]
        private float edgeMillTabWidth = DEFAULT_EDGEMILLTAB_WIDTH;

        #endregion

        // ####################################################################
        // ##### Bed Flattening category variables
        // ####################################################################
        #region Bed Flattening category variables

        public const BedFlatteningSizeModeEnum DEFAULT_BEDFLATTENINGSIZE_MODE = BedFlatteningSizeModeEnum.Add_Margin_To_Border;
        [DataMember]
        private BedFlatteningSizeModeEnum bedFlatteningSizeMode = DEFAULT_BEDFLATTENINGSIZE_MODE;

        // this determines whether we generate bed flattening GCodes
        // when processing in BoardEdgeMill mode
        public const bool DEFAULT_BEDFLATTENINGGCODE_ENABLED = true;
        [DataMember]
        private bool bedFlatteningGCodeEnabled = DEFAULT_BEDFLATTENINGGCODE_ENABLED;

        // this is the output file name extension for the bed flattening GCode file
        public const string DEFAULT_BEDFLATTENINGGCODEFILE_OUTPUTEXTENSION = "_BEDFLATTEN_GCODE.ngc";
        [DataMember]
        private string bedFlatteningGCodeFileOutputExtension = DEFAULT_BEDFLATTENINGGCODEFILE_OUTPUTEXTENSION;

        // this is the absolute size of the bed flattening code we mill
        // in the X direction
        public const float DEFAULT_BEDFLATTENINGSIZE_X = 3.0f;
        [DataMember]
        private float bedFlatteningSizeX = DEFAULT_BEDFLATTENINGSIZE_X;

        // this is the absolute size of the bed flattening code we mill
        // in the Y direction
        public const float DEFAULT_BEDFLATTENINGSIZE_Y = 2.0f;
        [DataMember]
        private float bedFlatteningSizeY = DEFAULT_BEDFLATTENINGSIZE_Y;

        // this is the distance into the material we cut this will be negative 
        // because zero is the surface of the pcb we are cutting
        public const float DEFAULT_BEDFLATTENINGZCUTLEVEL = -0.025f;
        [DataMember]
        private float bedFlatteningZCutLevel = DEFAULT_BEDFLATTENINGZCUTLEVEL;

        // this is the distance above the PCB we move the z axis to
        // so that we can move about. It will be positive
        // because it is above the surface of the pcb we are cutting
        // and should be large enough to clear all hold downs and clamps etc
        public const float DEFAULT_BEDFLATTENINGZCLEARLEVEL = 0.25f;
        [DataMember]
        private float bedFlatteningZClearLevel = DEFAULT_BEDFLATTENINGZCLEARLEVEL;

        // this is the speed at which the tool (in application units per minute) moves vertically into the work.
        public const float DEFAULT_BEDFLATTENINGZFEEDRATE = 20f;
        [DataMember]
        private float bedFlatteningZFeedRate = DEFAULT_BEDFLATTENINGZFEEDRATE;

        // this is the speed at which the tool (in application units per minute) moves horizontally over the work.
        public const float DEFAULT_BEDFLATTENINGXYFEEDRATE = 30f;
        [DataMember]
        private float bedFlatteningXYFeedRate = DEFAULT_BEDFLATTENINGXYFEEDRATE;

        // this is diameter of the bed flattening mill bit at the BedFlatteningZCutLevel
        private const float DEFAULT_BEDFLATTENINGMILL_WIDTH = 0.5f;
        [DataMember]
        private float bedFlatteningMillWidth = DEFAULT_BEDFLATTENINGMILL_WIDTH;

        // this is extra margin around the PCB border which is added onto the bed flattening code.
        private const float DEFAULT_BEDFLATTENING_MARGIN = 0.5f;
        [DataMember]
        private float bedFlatteningMargin = DEFAULT_BEDFLATTENING_MARGIN;

        #endregion

        // ####################################################################
        // ##### Reference Pins category variables
        // ####################################################################
        #region Reference Pins category variables

        // this is the diameter of the pads on the PCB we should use as reference pins.
        public const float DEFAULT_REFERENCEPINSPADDIAMETER = 0.125f;
        [DataMember]
        private float referencePinPadDiameter = DEFAULT_REFERENCEPINSPADDIAMETER;

        // this determines whether we generate reference pins GCodes
        public const bool DEFAULT_REFERENCEPINGCODE_ENABLED = true;
        [DataMember]
        private bool referencePinGCodeEnabled = DEFAULT_REFERENCEPINGCODE_ENABLED;

        // this is the output file name extension for the bed flattening GCode file
        public const string DEFAULT_REFERENCEPINSGCODEFILE_OUTPUTEXTENSION = "_REFPINS_GCODE.ngc";
        [DataMember]
        private string referencePinsGCodeFileOutputExtension = DEFAULT_REFERENCEPINSGCODEFILE_OUTPUTEXTENSION;

        // this is the distance into the material we cut this will be negative 
        // because zero is the surface of the pcb we are cutting
        public const float DEFAULT_REFERENCEPINSZDRILLDEPTH = -0.3f;
        [DataMember]
        private float referencePinsZDrillDepth = DEFAULT_REFERENCEPINSZDRILLDEPTH;

        // this is the distance above the PCB we move the z axis to
        // so that we can move about. It will be positive
        // because it is above the surface of the pcb we are cutting
        // and should be large enough to clear all hold downs and clamps etc
        public const float DEFAULT_REFERENCEPINSZCLEARLEVEL = .250f;
        [DataMember]
        private float referencePinsZClearLevel = DEFAULT_REFERENCEPINSZCLEARLEVEL;

        // this is the speed at which the tool (in application units per minute) moves vertically into the work.
        public const float DEFAULT_REFERENCEPINSZFEEDRATE = 20f;
        [DataMember]
        private float referencePinsZFeedRate = DEFAULT_REFERENCEPINSZFEEDRATE;

        // this is the speed at which the tool (in application units per minute) moves horizontally over the work.
        public const float DEFAULT_REFERENCEPINSXYFEEDRATE = 20f;
        [DataMember]
        private float referencePinsXYFeedRate = DEFAULT_REFERENCEPINSXYFEEDRATE;

        // this is the maximum number of reference pins we expect to see in the Gerber file
        private const int DEFAULT_REFERENCEPINS_MAXNUMBER = 6;
        [DataMember]
        private int referencePinsMaxNumber = DEFAULT_REFERENCEPINS_MAXNUMBER;

        #endregion

        // ####################################################################
        // ##### Excellon category variables
        // ####################################################################
        #region Excellon category variables

        // this determines whether we generate Excellon Drilling GCodes
        public const bool DEFAULT_DRILLINGGCODE_ENABLED = true;
        [DataMember]
        private bool drillingGCodeEnabled = DEFAULT_DRILLINGGCODE_ENABLED;

        // this determines the method used to deal with leading zeros in the excellon file
        public enum ExcellonDrillingCoordinateZerosModeEnum
        {
            FixedDecimalPoint,
            OmitLeadingZeros,
          //not supported  OmitTrailingZeros, 
        }
        public const ExcellonDrillingCoordinateZerosModeEnum DEFAULT_DRILLING_COORDINATEZEROS_MODE = ExcellonDrillingCoordinateZerosModeEnum.FixedDecimalPoint;
        [DataMember]
        private ExcellonDrillingCoordinateZerosModeEnum drillingDrillingCoordinateZerosMode = DEFAULT_DRILLING_COORDINATEZEROS_MODE;

        // this is the number of decimal places used in the excellon file
        public const int DRILLING_NUMBER_OF_DECIMAL_PLACES = 3;
        [DataMember]
        private int drillingNumberOfDecimalPlaces = DRILLING_NUMBER_OF_DECIMAL_PLACES;

        // this is the output file name extension for the bed flattening GCode file
        public const string DEFAULT_DRILLINGGCODEFILE_OUTPUTEXTENSION = "_DRILL_GCODE.ngc";
        [DataMember]
        private string drillingGCodeFileOutputExtension = DEFAULT_DRILLINGGCODEFILE_OUTPUTEXTENSION;

        // this is the distance into the material we cut this will be negative 
        // because zero is the surface of the pcb we are cutting
        public const float DEFAULT_DRILLINGZDEPTH = -0.20f;
        [DataMember]
        private float drillingZDepth = DEFAULT_DRILLINGZDEPTH;

        // this is the distance above the PCB we move the z axis to
        // so that we can move about. It will be positive
        // because it is above the surface of the pcb we are cutting
        // and should be large enough to clear all hold downs and clamps etc
        public const float DEFAULT_DRILLINGZCLEARLEVEL = 0.25f;
        [DataMember]
        private float drillingZClearLevel = DEFAULT_DRILLINGZCLEARLEVEL;

        // this is the speed at which the tool (in application units per minute) moves vertically into the work.
        public const float DEFAULT_DRILLZFEEDRATE = 20f;
        [DataMember]
        private float drillingZFeedRate = DEFAULT_DRILLZFEEDRATE;

        // this is the speed at which the tool (in application units per minute) moves horizontally over the work.
        public const float DEFAULT_DRILLINGZFEEDRATE = 30f;
        [DataMember]
        private float drillingXYFeedRate = DEFAULT_DRILLINGZFEEDRATE;

        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public FileManager()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public FileManager(OperationModeEnum operationModeIn)
        {
            // set this now
            operationMode = operationModeIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public FileManager(string fileWildCardTextIn)
        {
            // add it through the property to sanity check it
            FilenamePattern = fileWildCardTextIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We sometimes need to know the max tool width so we can calculate 
        /// borders etc. This provides an estimate
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public float GetMaxToolWidthForEnabledOperationMode()
        {
            switch (OperationMode)
            {
                case OperationModeEnum.BoardEdgeMill:
                    // we do not need to take bedflattening items into account here
                    // they are not calculated
                    return EdgeMillCutWidth;
                case OperationModeEnum.Excellon:
                    // excellon does not have a setting for this. We just use this
                    return DEFAULT_ISOCUT_WIDTH;
                case OperationModeEnum.IsolationCut:
                    // we do not need to take reference pins into account here
                    // they are not calculated
                    return IsoCutWidth;
                case OperationModeEnum.TextAndLabels:
                case OperationModeEnum.Default:
                default:
                    return DEFAULT_ISOCUT_WIDTH;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the FileManager class is currently at its default values
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public bool IsAtDefaults()
        {
            return IsAtDefaults(false);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the FileManager class is currently at its default values
        /// </summary>
        /// <param name="doNotTestFilenamePattern">if true we do not test the
        /// filenamepattern, operationmode or description</param>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public bool IsAtDefaults(bool doNotTestFilenamePattern)
        {
            if (doNotTestFilenamePattern == false)
            {
                // file manager
                if (description != DEFAULT_DESCRIPTION) return false;
                if (operationMode != DEFAULT_OPERATION_MODE) return false;
                if (filenamePattern != DEFAULT_FILENAME_PATTERN) return false;
            }

            // general
            if (autoAdjustOrigin != DEFAULT_AUTOADJUSTORIGIN) return false;
            if (autoGenerateGCode != DEFAULT_AUTOGENERATE_GCODE) return false;
            if (showGCodeLineNumbers != DEFAULT_SHOWGCODE_LINENUMBERS) return false;

            // edgeMill cuts
            if (edgeMillingGCodeEnabled != DEFAULT_EDGEMILLINGGCODE_ENABLED) return false;
            if (edgeMillGCodeFileOutputExtension != DEFAULT_EDGEMILL_OUTPUTEXTENSION) return false;
            if (edgeMillZCutLevel != DEFAULT_EDGEMILLZCUTLEVEL) return false;
            if (zEdgeMillMoveLevel != DEFAULT_EDGEMILLZMOVELEVEL) return false;
            if (edgeMillZClearLevel != DEFAULT_ZEDGEMILLCLEARLEVEL) return false;
            if (edgeMillZFeedRate != DEFAULT_EDGEMILLZFEEDRATE) return false;
            if (edgeMillXYFeedRate != DEFAULT_EDGEMILLXYFEEDRATE) return false;
            if (edgeMillCutWidth != DEFAULT_EDGEMILLCUT_WIDTH) return false;
            if (edgeMillNumTabs != DEFAULT_EDGEMILLNUM_TABS) return false;
            if (edgeMillTabWidth != DEFAULT_EDGEMILLTAB_WIDTH) return false;

            // bed flattening
            if (bedFlatteningSizeMode != DEFAULT_BEDFLATTENINGSIZE_MODE) return false;
            if (bedFlatteningSizeX != DEFAULT_BEDFLATTENINGSIZE_X) return false;
            if (bedFlatteningSizeY != DEFAULT_BEDFLATTENINGSIZE_Y) return false;
            if (bedFlatteningGCodeEnabled != DEFAULT_BEDFLATTENINGGCODE_ENABLED) return false;
            if (bedFlatteningGCodeFileOutputExtension != DEFAULT_BEDFLATTENINGGCODEFILE_OUTPUTEXTENSION) return false;
            if (bedFlatteningZCutLevel != DEFAULT_BEDFLATTENINGZCUTLEVEL) return false;
            if (bedFlatteningZClearLevel != DEFAULT_BEDFLATTENINGZCLEARLEVEL) return false;
            if (bedFlatteningZFeedRate != DEFAULT_BEDFLATTENINGZFEEDRATE) return false;
            if (bedFlatteningXYFeedRate != DEFAULT_BEDFLATTENINGXYFEEDRATE) return false;
            if (bedFlatteningMillWidth != DEFAULT_BEDFLATTENINGMILL_WIDTH) return false;
            if (bedFlatteningMargin != DEFAULT_BEDFLATTENING_MARGIN) return false;

            // iso cuts
            if (isoGCodeFileOutputExtension != DEFAULT_ISOGCODEFILE_OUTPUTEXTENSION) return false;
            if (isoFlipMode != DEFAULT_ISOFLIP_MODE) return false;
            if (isoZCutLevel != DEFAULT_ISOZCUTLEVEL) return false;
            if (isoZMoveLevel != DEFAULT_ISOZMOVELEVEL) return false;
            if (isoZClearLevel != DEFAULT_ISOZCLEARLEVEL) return false;
            if (isoZFeedRate != DEFAULT_ISOZFEEDRATE) return false;
            if (isoXYFeedRate != DEFAULT_ISOXYFEEDRATE) return false;
            if (isoCutWidth != DEFAULT_ISOCUT_WIDTH) return false;
            if (isoPadTouchDownsWanted != DEFAULT_ISOPADTOUCHDOWNS_WANTED) return false;
            if (isoPadTouchDownZLevel != DEFAULT_ISOPADTOUCHDOWN_ZLEVEL) return false;
            if (isoCutGCodeEnabled != DEFAULT_ISOCUTGCODE_ENABLED) return false;

            // ref pins
            if (referencePinGCodeEnabled != DEFAULT_REFERENCEPINGCODE_ENABLED) return false;
            if (referencePinsGCodeFileOutputExtension != DEFAULT_REFERENCEPINSGCODEFILE_OUTPUTEXTENSION) return false;
            if (referencePinsZDrillDepth != DEFAULT_REFERENCEPINSZDRILLDEPTH) return false;
            if (referencePinsZClearLevel != DEFAULT_REFERENCEPINSZCLEARLEVEL) return false;
            if (referencePinsZFeedRate != DEFAULT_REFERENCEPINSZFEEDRATE) return false;
            if (referencePinsXYFeedRate != DEFAULT_REFERENCEPINSXYFEEDRATE) return false;
            if (referencePinsMaxNumber != DEFAULT_REFERENCEPINS_MAXNUMBER) return false;
            if (referencePinPadDiameter != DEFAULT_REFERENCEPINSPADDIAMETER) return false;

            // excellon
            if (drillingDrillingCoordinateZerosMode != DEFAULT_DRILLING_COORDINATEZEROS_MODE) return false;
            if (drillingNumberOfDecimalPlaces != DRILLING_NUMBER_OF_DECIMAL_PLACES) return false;
            if (drillingGCodeFileOutputExtension != DEFAULT_DRILLINGGCODEFILE_OUTPUTEXTENSION) return false;
            if (drillingZDepth != DEFAULT_DRILLINGZDEPTH) return false;
            if (drillingZClearLevel != DEFAULT_DRILLINGZCLEARLEVEL) return false;
            if (drillingZFeedRate != DEFAULT_DRILLZFEEDRATE) return false;
            if (drillingXYFeedRate != DEFAULT_DRILLINGZFEEDRATE) return false;
            if (drillingGCodeEnabled != DEFAULT_DRILLINGGCODE_ENABLED) return false;

            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets all values in the file manager to their defaults
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        public void Reset()
        {
            // reset everything
            Reset(true);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets all values in the file manager to their defaults, optionally
        /// excluding certain fundamental values
        /// </summary>
        /// <param name="alsoResetFilenamePattern">if true we do not reset the 
        /// FileNamePattern, Description and OperationMode</param>
        /// <history>
        ///    31 Aug 10  Cynic - Started
        /// </history>
        public void Reset(bool alsoResetFilenamePattern)
        {
            if(alsoResetFilenamePattern==true)
            {
            // file manager
            description = DEFAULT_DESCRIPTION;
            filenamePattern = DEFAULT_FILENAME_PATTERN;
            operationMode = DEFAULT_OPERATION_MODE;
            }

            // general
            autoAdjustOrigin = DEFAULT_AUTOADJUSTORIGIN;
            autoGenerateGCode = DEFAULT_AUTOGENERATE_GCODE;
            showGCodeLineNumbers = DEFAULT_SHOWGCODE_LINENUMBERS;

            // edgeMillcuts
            edgeMillGCodeFileOutputExtension = DEFAULT_EDGEMILL_OUTPUTEXTENSION;
            edgeMillZCutLevel = DEFAULT_EDGEMILLZCUTLEVEL;
            zEdgeMillMoveLevel = DEFAULT_EDGEMILLZMOVELEVEL;
            edgeMillZClearLevel = DEFAULT_ZEDGEMILLCLEARLEVEL;
            edgeMillZFeedRate = DEFAULT_EDGEMILLZFEEDRATE;
            edgeMillXYFeedRate = DEFAULT_EDGEMILLXYFEEDRATE;
            edgeMillCutWidth = DEFAULT_EDGEMILLCUT_WIDTH;
            edgeMillNumTabs = DEFAULT_EDGEMILLNUM_TABS;
            edgeMillTabWidth = DEFAULT_EDGEMILLTAB_WIDTH;

            // bed flattening
            bedFlatteningSizeMode = DEFAULT_BEDFLATTENINGSIZE_MODE;
            bedFlatteningSizeX = DEFAULT_BEDFLATTENINGSIZE_X;
            bedFlatteningSizeY = DEFAULT_BEDFLATTENINGSIZE_Y;
            bedFlatteningGCodeEnabled = DEFAULT_BEDFLATTENINGGCODE_ENABLED;
            bedFlatteningGCodeFileOutputExtension = DEFAULT_BEDFLATTENINGGCODEFILE_OUTPUTEXTENSION;
            bedFlatteningZCutLevel = DEFAULT_BEDFLATTENINGZCUTLEVEL;
            bedFlatteningZClearLevel = DEFAULT_BEDFLATTENINGZCLEARLEVEL;
            bedFlatteningZFeedRate = DEFAULT_BEDFLATTENINGZFEEDRATE;
            bedFlatteningXYFeedRate = DEFAULT_BEDFLATTENINGXYFEEDRATE;
            bedFlatteningMillWidth = DEFAULT_BEDFLATTENINGMILL_WIDTH;
            bedFlatteningMargin = DEFAULT_BEDFLATTENING_MARGIN;
            edgeMillingGCodeEnabled = DEFAULT_EDGEMILLINGGCODE_ENABLED;

            // isocuts
            isoFlipMode = DEFAULT_ISOFLIP_MODE;
            isoZCutLevel = DEFAULT_ISOZCUTLEVEL;
            isoGCodeFileOutputExtension = DEFAULT_ISOGCODEFILE_OUTPUTEXTENSION;
            isoZMoveLevel = DEFAULT_ISOZMOVELEVEL;
            isoZClearLevel = DEFAULT_ISOZCLEARLEVEL;
            isoZFeedRate = DEFAULT_ISOZFEEDRATE;
            isoXYFeedRate = DEFAULT_ISOXYFEEDRATE;
            isoCutWidth = DEFAULT_ISOCUT_WIDTH;
            isoPadTouchDownsWanted = DEFAULT_ISOPADTOUCHDOWNS_WANTED;
            isoPadTouchDownZLevel = DEFAULT_ISOPADTOUCHDOWN_ZLEVEL;
            isoCutGCodeEnabled = DEFAULT_ISOCUTGCODE_ENABLED;

            // ref pins
            referencePinGCodeEnabled = DEFAULT_REFERENCEPINGCODE_ENABLED;
            referencePinsGCodeFileOutputExtension = DEFAULT_REFERENCEPINSGCODEFILE_OUTPUTEXTENSION;
            referencePinsZDrillDepth = DEFAULT_REFERENCEPINSZDRILLDEPTH;
            referencePinsZClearLevel = DEFAULT_REFERENCEPINSZCLEARLEVEL;
            referencePinsZFeedRate = DEFAULT_REFERENCEPINSZFEEDRATE;
            referencePinsXYFeedRate = DEFAULT_REFERENCEPINSXYFEEDRATE;
            referencePinsMaxNumber = DEFAULT_REFERENCEPINS_MAXNUMBER;
            referencePinPadDiameter = DEFAULT_REFERENCEPINSPADDIAMETER;

            // excellon
            drillingDrillingCoordinateZerosMode = DEFAULT_DRILLING_COORDINATEZEROS_MODE;
            drillingNumberOfDecimalPlaces = DRILLING_NUMBER_OF_DECIMAL_PLACES;
            drillingGCodeFileOutputExtension = DEFAULT_DRILLINGGCODEFILE_OUTPUTEXTENSION;
            drillingZDepth = DEFAULT_DRILLINGZDEPTH;
            drillingZClearLevel = DEFAULT_DRILLINGZCLEARLEVEL;
            drillingZFeedRate = DEFAULT_DRILLZFEEDRATE;
            drillingXYFeedRate = DEFAULT_DRILLINGZFEEDRATE;
            drillingGCodeEnabled = DEFAULT_DRILLINGGCODE_ENABLED;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts all convertable settings from inches to MM
        /// </summary>
        /// <history>
        ///    20 Nov 10  Cynic - Started
        /// </history>
        public void ConvertFromInchToMM()
        {
            // filemanager
            // n/a description 
            // n/a operationMode 
            // n/a filenamePattern 

            //general
            // n/a autoAdjustOrigin 
            // n/a autoGenerateGCode 
            // n/a showGCodeLineNumbers 

            //edgeMillcuts
            // n/a edgeMillingGCodeEnabled
            // n/a edgeMillGCodeFileOutputExtension
            edgeMillZCutLevel = (edgeMillZCutLevel * INCHTOMMSCALERx10) / 10;
            zEdgeMillMoveLevel = (zEdgeMillMoveLevel * INCHTOMMSCALERx10) / 10;
            edgeMillZClearLevel = (edgeMillZClearLevel * INCHTOMMSCALERx10) / 10;
            edgeMillZFeedRate = (edgeMillZFeedRate * INCHTOMMSCALERx10) / 10;
            edgeMillXYFeedRate = (edgeMillXYFeedRate * INCHTOMMSCALERx10) / 10;
            edgeMillCutWidth = (edgeMillCutWidth * INCHTOMMSCALERx10) / 10;
            // n/a edgeMillNumTabs
            edgeMillTabWidth = (edgeMillTabWidth * INCHTOMMSCALERx10) / 10;

            //bedflattening
            // n/a bedFlatteningSizeMode
            bedFlatteningSizeX = (bedFlatteningSizeX * INCHTOMMSCALERx10) / 10;
            bedFlatteningSizeY = (bedFlatteningSizeY * INCHTOMMSCALERx10) / 10;
            // n/a bedFlatteningGCodeEnabled
            // n/a bedFlatteningGCodeFileOutputExtension
            bedFlatteningZCutLevel = (bedFlatteningZCutLevel * INCHTOMMSCALERx10) / 10;
            bedFlatteningZClearLevel = (bedFlatteningZClearLevel * INCHTOMMSCALERx10) / 10;
            bedFlatteningZFeedRate = (bedFlatteningZFeedRate * INCHTOMMSCALERx10) / 10;
            bedFlatteningXYFeedRate = (bedFlatteningXYFeedRate * INCHTOMMSCALERx10) / 10;
            bedFlatteningMillWidth = (bedFlatteningMillWidth * INCHTOMMSCALERx10) / 10;
            bedFlatteningMargin = (bedFlatteningMargin * INCHTOMMSCALERx10) / 10;

            //isocuts
            // n/a isoGCodeFileOutputExtension 
            // n/a isoFlipMode 
            isoZCutLevel = (isoZCutLevel * INCHTOMMSCALERx10) / 10;
            isoZMoveLevel = (isoZMoveLevel * INCHTOMMSCALERx10) / 10;
            isoZClearLevel = (isoZClearLevel * INCHTOMMSCALERx10) / 10;
            isoZFeedRate = (isoZFeedRate * INCHTOMMSCALERx10) / 10;
            isoXYFeedRate = (isoXYFeedRate * INCHTOMMSCALERx10) / 10;
            isoCutWidth = (isoCutWidth * INCHTOMMSCALERx10) / 10;
            // n/a isoPadTouchDownsWanted 
            isoPadTouchDownZLevel = (isoPadTouchDownZLevel * INCHTOMMSCALERx10) / 10;
            // n/a isoCutGCodeEnabled

            //refpins
            // n/a referencePinGCodeEnabled 
            // n/a referencePinsGCodeFileOutputExtension 
            referencePinsZDrillDepth = (referencePinsZDrillDepth * INCHTOMMSCALERx10) / 10;
            referencePinsZClearLevel = (referencePinsZClearLevel * INCHTOMMSCALERx10) / 10;
            referencePinsZFeedRate = (referencePinsZFeedRate * INCHTOMMSCALERx10) / 10;
            referencePinsXYFeedRate = (referencePinsXYFeedRate * INCHTOMMSCALERx10) / 10;
            // n/a referencePinsMaxNumber
            referencePinPadDiameter = (referencePinPadDiameter * INCHTOMMSCALERx10) / 10;

            //excellon
            // n/a drillingDrillingCoordinateZerosMode
            // n/a drillingNumberOfDecimalPlaces
            // n/a drillingGCodeFileOutputExtension
            drillingZDepth = (drillingZDepth * INCHTOMMSCALERx10) / 10;
            drillingZClearLevel = (drillingZClearLevel * INCHTOMMSCALERx10) / 10;
            drillingZFeedRate = (drillingZFeedRate * INCHTOMMSCALERx10) / 10;
            drillingXYFeedRate = (drillingXYFeedRate * INCHTOMMSCALERx10) / 10;
            // n/a drillingGCodeEnabled
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts all convertable settings from MM to Inch
        /// </summary>
        /// <history>
        ///    20 Nov 10  Cynic - Started
        /// </history>
        public void ConvertFromMMToInch()
        {
            // filemanager
            // n/a description 
            // n/a operationMode 
            // n/a filenamePattern 

            //general
            // n/a autoAdjustOrigin 
            // n/a autoGenerateGCode 
            // n/a showGCodeLineNumbers 

            //edgeMillcuts
            // n/a edgeMillingGCodeEnabled
            // n/a edgeMillGCodeFileOutputExtension
            edgeMillZCutLevel = (edgeMillZCutLevel * 10) / INCHTOMMSCALERx10;
            zEdgeMillMoveLevel = (zEdgeMillMoveLevel * 10) / INCHTOMMSCALERx10;
            edgeMillZClearLevel = (edgeMillZClearLevel * 10) / INCHTOMMSCALERx10;
            edgeMillZFeedRate = (edgeMillZFeedRate * 10) / INCHTOMMSCALERx10;
            edgeMillXYFeedRate = (edgeMillXYFeedRate * 10) / INCHTOMMSCALERx10;
            edgeMillCutWidth = (edgeMillCutWidth * 10) / INCHTOMMSCALERx10;
            // n/a edgeMillNumTabs
            edgeMillTabWidth = (edgeMillTabWidth * 10) / INCHTOMMSCALERx10;

            //bedflattening
            // n/a bedFlatteningSizeMode
            bedFlatteningSizeX = (bedFlatteningSizeX * 10) / INCHTOMMSCALERx10;
            bedFlatteningSizeY = (bedFlatteningSizeY * 10) / INCHTOMMSCALERx10;
            // n/a bedFlatteningGCodeEnabled
            // n/a bedFlatteningGCodeFileOutputExtension
            bedFlatteningZCutLevel = (bedFlatteningZCutLevel * 10) / INCHTOMMSCALERx10;
            bedFlatteningZClearLevel = (bedFlatteningZClearLevel * 10) / INCHTOMMSCALERx10;
            bedFlatteningZFeedRate = (bedFlatteningZFeedRate * 10) / INCHTOMMSCALERx10;
            bedFlatteningXYFeedRate = (bedFlatteningXYFeedRate * 10) / INCHTOMMSCALERx10;
            bedFlatteningMillWidth = (bedFlatteningMillWidth * 10) / INCHTOMMSCALERx10;
            bedFlatteningMargin = (bedFlatteningMargin * 10) / INCHTOMMSCALERx10;

            //isocuts
            // n/a isoGCodeFileOutputExtension 
            // n/a isoFlipMode 
            isoZCutLevel = (isoZCutLevel * 10) / INCHTOMMSCALERx10;
            isoZMoveLevel = (isoZMoveLevel * 10) / INCHTOMMSCALERx10;
            isoZClearLevel = (isoZClearLevel * 10) / INCHTOMMSCALERx10;
            isoZFeedRate = (isoZFeedRate * 10) / INCHTOMMSCALERx10;
            isoXYFeedRate = (isoXYFeedRate * 10) / INCHTOMMSCALERx10;
            isoCutWidth = (isoCutWidth * 10) / INCHTOMMSCALERx10;
            // n/a isoPadTouchDownsWanted 
            isoPadTouchDownZLevel = (isoPadTouchDownZLevel * 10) / INCHTOMMSCALERx10;
            // n/a isoCutGCodeEnabled

            //refpins
            // n/a referencePinGCodeEnabled 
            // n/a referencePinsGCodeFileOutputExtension 
            referencePinsZDrillDepth = (referencePinsZDrillDepth * 10) / INCHTOMMSCALERx10;
            referencePinsZClearLevel = (referencePinsZClearLevel * 10) / INCHTOMMSCALERx10;
            referencePinsZFeedRate = (referencePinsZFeedRate * 10) / INCHTOMMSCALERx10;
            referencePinsXYFeedRate = (referencePinsXYFeedRate * 10) / INCHTOMMSCALERx10;
            // n/a referencePinsMaxNumber
            referencePinPadDiameter = (referencePinPadDiameter * 10) / INCHTOMMSCALERx10;

            //excellon
            // n/a drillingDrillingCoordinateZerosMode
            // n/a drillingNumberOfDecimalPlaces
            // n/a drillingGCodeFileOutputExtension
            drillingZDepth = (drillingZDepth * 10) / INCHTOMMSCALERx10;
            drillingZClearLevel = (drillingZClearLevel * 10) / INCHTOMMSCALERx10;
            drillingZFeedRate = (drillingZFeedRate * 10) / INCHTOMMSCALERx10;
            drillingXYFeedRate = (drillingXYFeedRate * 10) / INCHTOMMSCALERx10;
            // n/a drillingGCodeEnabled
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Perform a deep Clone of the FileManagersBase object.
        /// </summary>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        ///    09 Sep 10  Cynic - Converted from Serializable to DataContractSerializer
        /// </history>
        public static FileManager DeepClone(FileManager sourceObj)
        {
            MemoryStream stream1 = new MemoryStream();

            //Serialize the FileManager object to a memory stream using DataContractSerializer.
            DataContractSerializer serializer = new DataContractSerializer(sourceObj.GetType());
            serializer.WriteObject(stream1, sourceObj);

            stream1.Position = 0;

            //Deserialize the Record object back into a new record object.
            FileManager outMgr = (FileManager)serializer.ReadObject(stream1);
            return outMgr;

/*
            if (!typeof(FileManager).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "sourceObj");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(sourceObj, null))
            {
                return default(FileManager);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, sourceObj);
                stream.Seek(0, SeekOrigin.Begin);
                return (FileManager)formatter.Deserialize(stream);
            }
 */ 
        }

        // ####################################################################
        // ##### File Manager category items
        // ####################################################################
        #region File Manager category items

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the operationMode value. This determines the usage the fileoption
        /// object will be used for and the options it will present
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        [CategoryAttribute(" File Manager")]
        [DescriptionAttribute("Gerber files from various layers require different operations to be performed on them. This setting determines the operations performed on the file which matches the specified FilenamePattern.")]
        [ReadOnlyAttribute(true)]
        [BrowsableAttribute(true)]
        public OperationModeEnum OperationMode
        {
            get
            {
                return operationMode;
            }
            set
            {
                operationMode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the filenamePattern value. This is the value we use
        /// to match the filename to see if the other options set in this file apply
        /// </summary>
        /// <remarks>will never get or set null or empty string</remarks>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("The part of the filename to match. If this text is present in the filename, then the options and actions in this File Manager are applied to that file. This name should be changed to match the Gerber or Excellon file you wish to process with these options.")]
        [CategoryAttribute(" File Manager")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public string FilenamePattern
        {
            get
            {
                if (operationMode == OperationModeEnum.Default) return DEFAULT_OPMODE_FILENAME;
                if ((filenamePattern == null) || (filenamePattern.Length == 0)) filenamePattern = DEFAULT_FILENAME_PATTERN;
                return filenamePattern;
            }
            set
            {
                // never accept a name change on default
                if (operationMode == OperationModeEnum.Default) return;
                filenamePattern = value;
                if ((filenamePattern == null) || (filenamePattern.Length == 0)) filenamePattern = DEFAULT_FILENAME_PATTERN;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the description value. 
        /// </summary>
        /// <remarks>will never get or set null or empty string</remarks>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("User specified text. Can be used for documentation purposes.")]
        [CategoryAttribute(" File Manager")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public string Description
        {
            get
            {
                if (operationMode == OperationModeEnum.Default) 
                {
                    return DEFAULT_DESCRIPTION_OPMODE_DEFAULT;
                }
                else
                {
                    if ((description == null) || (description.Length == 0)) description = DEFAULT_DESCRIPTION;
                    if (description == DEFAULT_DESCRIPTION_OPMODE_DEFAULT) description = DEFAULT_DESCRIPTION;
                }
                return description;
            }
            set
            {
                description = value;
                if ((description == null) || (description.Length == 0)) description = DEFAULT_DESCRIPTION;
            }
        }

        #endregion

        // ####################################################################
        // ##### General category items
        // ####################################################################
        #region General category items

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the autoAdjustOrigin flag
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("Automatically adjusts the (0,0) origin based on the Gerber file contents. Only useful for single sided boards.")]
        [CategoryAttribute(" General")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(false)]
        public bool AutoAdjustOrigin
        {
            get
            {
                // we never adjust origin anymore, bad idea
                return false;
                //return autoAdjustOrigin;
            }
            set
            {
                autoAdjustOrigin = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the autoGenerateGCode flag
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("Automatically generates GCode when the Gerber or Excellon file is opened. The type of GCode generated depends on the OperationMode and configured options.")]
        [CategoryAttribute(" General")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public bool AutoGenerateGCode
        {
            get
            {
                return autoGenerateGCode;
            }
            set
            {
                autoGenerateGCode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the showGCodeLineNumbers flag
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("Places line numbers in the GCode File. Most people do not want these.")]
        [CategoryAttribute(" General")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public bool ShowGCodeLineNumbers
        {
            get
            {
                return showGCodeLineNumbers;
            }
            set
            {
                showGCodeLineNumbers = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the warnAboutOverwriting flag
        /// </summary>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("Generates a warning if the save of the GCode file will overwrite an existing GCode file.")]
        [CategoryAttribute(" General")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public bool WarnAboutOverwriting
        {
            get
            {
                return warnAboutOverwriting;
            }
            set
            {
                warnAboutOverwriting = value;
            }
        }

        #endregion

        // ####################################################################
        // ##### Board Edge Mill category items
        // ####################################################################
        #region Board Edge Mill category items

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the edgeMillingGCodeEnabled flag 
        /// </summary>
        /// <history>
        ///    07 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This option enables the generation of the GCode file for Edge Milling. These cuts define the board outline.")]
        [CategoryAttribute("Board Edge Milling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public bool EdgeMillingGCodeEnabled
        {
            get
            {
                return edgeMillingGCodeEnabled;
            }
            set
            {
                edgeMillingGCodeEnabled = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the edgeMillGCodeFileOutputExtension value. This is the file we use
        /// save the gcode under 
        /// </summary>
        /// <remarks>will never get or set null or empty string</remarks>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("The filename extension to use when the Edge Mill GCode file is saved. The existing filename extension will be stripped off and replaced with this text.")]
        [CategoryAttribute("Board Edge Milling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public string EdgeMillGCodeFileOutputExtension
        {
            get
            {
                if ((edgeMillGCodeFileOutputExtension == null) || (edgeMillGCodeFileOutputExtension.Length == 0)) edgeMillGCodeFileOutputExtension = DEFAULT_EDGEMILL_OUTPUTEXTENSION;
                return edgeMillGCodeFileOutputExtension;
            }
            set
            {
                edgeMillGCodeFileOutputExtension = value;
                if ((edgeMillGCodeFileOutputExtension == null) || (edgeMillGCodeFileOutputExtension.Length == 0)) edgeMillGCodeFileOutputExtension = DEFAULT_EDGEMILL_OUTPUTEXTENSION;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the edgeMillZCutLevel. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance into the material we cut defined in application units (inches,mm). The value should be negative and larger than the thickness of the PCB board since you wish to cut through it in this mode.")]
        [CategoryAttribute("Board Edge Milling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float EdgeMillZCutLevel
        {
            get
            {
                return edgeMillZCutLevel;
            }
            set
            {
                edgeMillZCutLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the zEdgeMillMoveLevel. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance above the PCB we move the z axis to so that we can hop from cut to cut. It will be positive because it is above the surface of the pcb being cut.")]
        [CategoryAttribute("Board Edge Milling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float EdgeMillZMoveLevel
        {
            get
            {
                return zEdgeMillMoveLevel;
            }
            set
            {
                zEdgeMillMoveLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the edgeMillZClearLevel. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance above the PCB we move the z axis to so that we can move about. It will be positive because it is above the surface of the pcb being cut and should be large enough to clear all hold downs and clamps etc.")]
        [CategoryAttribute("Board Edge Milling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float EdgeMillZClearLevel
        {
            get
            {
                return edgeMillZClearLevel;
            }
            set
            {
                edgeMillZClearLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the edgeMillZFeedRate. Will never get or set a negative
        /// or zero value. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the speed at which the tool (in application units per minute) moves vertically into the work.")]
        [CategoryAttribute("Board Edge Milling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float EdgeMillZFeedRate
        {
            get
            {
                if (edgeMillZFeedRate <= 0) edgeMillZFeedRate = DEFAULT_EDGEMILLZFEEDRATE;
                return edgeMillZFeedRate;
            }
            set
            {
                edgeMillZFeedRate = value;
                if (edgeMillZFeedRate <= 0) edgeMillZFeedRate = DEFAULT_EDGEMILLZFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the edgeMillXYFeedRate. Will never get or set a negative
        /// or xyero value. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the speed at which the tool (in application units per minute) moves horizontally over the work.")]
        [CategoryAttribute("Board Edge Milling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float EdgeMillXYFeedRate
        {
            get
            {
                if (edgeMillXYFeedRate <= 0) edgeMillXYFeedRate = DEFAULT_EDGEMILLXYFEEDRATE;
                return edgeMillXYFeedRate;
            }
            set
            {
                edgeMillXYFeedRate = value;
                if (edgeMillXYFeedRate <= 0) edgeMillXYFeedRate = DEFAULT_EDGEMILLXYFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the edgeMillCutWidth. Will never get or set a negative
        /// or xyero value. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the width of the line the Edge Cutting milling bit cuts when at the EdgeMillZCutLevel")]
        [CategoryAttribute("Board Edge Milling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float EdgeMillCutWidth
        {
            get
            {
                if (edgeMillCutWidth <= 0) edgeMillCutWidth = DEFAULT_EDGEMILLCUT_WIDTH;
                return edgeMillCutWidth;
            }
            set
            {
                edgeMillCutWidth = value;
                if (edgeMillCutWidth <= 0) edgeMillCutWidth = DEFAULT_EDGEMILLCUT_WIDTH;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the edgeMillNumTabs. Will never get or set a negative
        ///  value. 
        /// </summary>
        /// <history>
        ///    27 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the number of tabs left between the inner board and the larger blank PCB. These serve to hold the board in place. Set to zero for no tabs - but make sure the board being cut out is secured some other way.")]
        [CategoryAttribute("Board Edge Milling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public int EdgeMillNumTabs
        {
            get
            {
                if (edgeMillNumTabs < 0) edgeMillNumTabs = DEFAULT_EDGEMILLNUM_TABS;
                return edgeMillNumTabs;
            }
            set
            {
                edgeMillNumTabs = value;
                if (edgeMillNumTabs < 0) edgeMillNumTabs = DEFAULT_EDGEMILLNUM_TABS;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the edgeMillNumTabs. Will never get or set a negative
        /// or zero value. 
        /// </summary>
        /// <history>
        ///    27 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the width of the tabs (in Application Units) left between the inner board and the larger blank PCB. These serve to hold the board in place.")]
        [CategoryAttribute("Board Edge Milling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float EdgeMillTabWidth
        {
            get
            {
                if (edgeMillTabWidth <= 0) edgeMillTabWidth = DEFAULT_EDGEMILLTAB_WIDTH;
                return edgeMillTabWidth;
            }
            set
            {
                edgeMillTabWidth = value;
                if (edgeMillTabWidth <= 0) edgeMillTabWidth = DEFAULT_EDGEMILLTAB_WIDTH;
            }
        }

        #endregion

        // ####################################################################
        // ##### Bed Flattening category items
        // ####################################################################
        #region Bed Flattening category items

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bedFlatteningSizeMode value. This determines how we calculate
        /// the size of the bed flattening area.
        /// </summary>
        /// <history>
        ///    12 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This determines how the size of the bed flattening area is calculated. In Margin Mode a margin is applied to the existing plot border. In Absolute Mode the X and Y size options are used.")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public BedFlatteningSizeModeEnum BedFlatteningSizeMode
        {
            get
            {
                return bedFlatteningSizeMode;
            }
            set
            {
                bedFlatteningSizeMode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bedFlatteningSizeX. Never gets or sets a value <=0
        /// </summary>
        /// <history>
        ///    12 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the X size (in application units) of the bed flattening GCode. Only used when the BedFlatteningSizeMode is set to Absolute_Size.")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float BedFlatteningSizeX
        {
            get
            {
                if (bedFlatteningSizeX <= 0) bedFlatteningSizeX = DEFAULT_BEDFLATTENINGSIZE_X;
                return bedFlatteningSizeX;
            }
            set
            {
                bedFlatteningSizeX = value;
                if (bedFlatteningSizeX <= 0) bedFlatteningSizeX = DEFAULT_BEDFLATTENINGSIZE_X;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bedFlatteningSizeY. Never gets or sets a value <=0
        /// </summary>
        /// <history>
        ///    12 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the Y size (in application units) of the bed flattening GCode. Only used when the BedFlatteningSizeMode is set to Absolute_Size.")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float BedFlatteningSizeY
        {
            get
            {
                if (bedFlatteningSizeY <= 0) bedFlatteningSizeY = DEFAULT_BEDFLATTENINGSIZE_Y;
                return bedFlatteningSizeY;
            }
            set
            {
                bedFlatteningSizeY = value;
                if (bedFlatteningSizeY <= 0) bedFlatteningSizeY = DEFAULT_BEDFLATTENINGSIZE_Y;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets thebedFlatteningGCodeEnabled. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This option generates an optional GCode file which will mill the bed flat when run. This makes sure the bed is perfectly flat relative to the bit and is usually performed on a scrap piece of wood mounted on the bed.")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public bool BedFlatteningGCodeEnabled
        {
            get
            {
                return bedFlatteningGCodeEnabled;
            }
            set
            {
                bedFlatteningGCodeEnabled = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bedFlatteningGCodeFileOutputExtension value. This is the file we use
        /// save the bed flattening gcode under 
        /// </summary>
        /// <remarks>will never get or set null or empty string</remarks>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the output file name extension for the Bed Flattening GCode file (if generated).")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public string BedFlatteningGCodeFileOutputExtension
        {
            get
            {
                if ((bedFlatteningGCodeFileOutputExtension == null) || (bedFlatteningGCodeFileOutputExtension.Length == 0)) bedFlatteningGCodeFileOutputExtension = DEFAULT_BEDFLATTENINGGCODEFILE_OUTPUTEXTENSION;
                return bedFlatteningGCodeFileOutputExtension;
            }
            set
            {
                bedFlatteningGCodeFileOutputExtension = value;
                if ((bedFlatteningGCodeFileOutputExtension == null) || (bedFlatteningGCodeFileOutputExtension.Length == 0)) bedFlatteningGCodeFileOutputExtension = DEFAULT_BEDFLATTENINGGCODEFILE_OUTPUTEXTENSION;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bedFlatteningZCutLevel. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance into the bed material we mill defined in application units (inches,mm). The value should be negative and need only be large enough to ensure the entire surface is milled flat.")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float BedFlatteningZCutLevel
        {
            get
            {
                return bedFlatteningZCutLevel;
            }
            set
            {
                bedFlatteningZCutLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bedFlatteningZClearLevel. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance above the bed we move the z axis to so that we can move about. It will be positive because it is above the surface of the pcb being cut and should be large enough to clear all hold downs and clamps etc.")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float BedFlatteningZClearLevel
        {
            get
            {
                return bedFlatteningZClearLevel;
            }
            set
            {
                bedFlatteningZClearLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bedFlatteningZFeedRate. Will never get or set a negative
        /// or zero value. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the speed at which the tool (in application units per minute) moves vertically into the work.")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float BedFlatteningZFeedRate
        {
            get
            {
                if (bedFlatteningZFeedRate <= 0) bedFlatteningZFeedRate = DEFAULT_BEDFLATTENINGZFEEDRATE;
                return bedFlatteningZFeedRate;
            }
            set
            {
                bedFlatteningZFeedRate = value;
                if (bedFlatteningZFeedRate <= 0) bedFlatteningZFeedRate = DEFAULT_BEDFLATTENINGZFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bedFlatteningXYFeedRate. Will never get or set a negative
        /// or xyero value. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the speed at which the tool (in application units per minute) moves horizontally over the work.")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float BedFlatteningXYFeedRate
        {
            get
            {
                if (bedFlatteningXYFeedRate <= 0) bedFlatteningXYFeedRate = DEFAULT_BEDFLATTENINGXYFEEDRATE;
                return bedFlatteningXYFeedRate;
            }
            set
            {
                bedFlatteningXYFeedRate = value;
                if (bedFlatteningXYFeedRate <= 0) bedFlatteningXYFeedRate = DEFAULT_BEDFLATTENINGXYFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bedFlatteningMillWidth. Will never get or set a negative
        /// or xyero value. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the diameter of the Bed Flattening milling bit when at the BedFlatteningZCutLevel")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float BedFlatteningMillWidth
        {
            get
            {
                if (bedFlatteningMillWidth <= 0) bedFlatteningMillWidth = DEFAULT_BEDFLATTENINGMILL_WIDTH;
                return bedFlatteningMillWidth;
            }
            set
            {
                bedFlatteningMillWidth = value;
                if (bedFlatteningMillWidth <= 0) bedFlatteningMillWidth = DEFAULT_BEDFLATTENINGMILL_WIDTH;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bedFlatteningMargin. Will never get or set a negative
        /// or xyero value. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("The size of the generated bed flattening pattern is the size of the PCB board outline with this margin added around each dimension. Only used when the BedFlatteningSizeMode is set to Add_Margin_To_Border.")]
        [CategoryAttribute("Bed Flattening")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float BedFlatteningMargin
        {
            get
            {
                if (bedFlatteningMargin <= 0) bedFlatteningMargin = DEFAULT_BEDFLATTENING_MARGIN;
                return bedFlatteningMargin;
            }
            set
            {
                bedFlatteningMargin = value;
                if (bedFlatteningMargin <= 0) bedFlatteningMargin = DEFAULT_BEDFLATTENING_MARGIN;
            }
        }

        #endregion

        // ####################################################################
        // ##### Isolation Cut category items
        // ####################################################################
        #region Isolation Cut category items

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoCutGCodeEnabled flag 
        /// </summary>
        /// <history>
        ///    07 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This option indicates that the GCode file for Isolation Cuts is to be generated. These cuts outline the traces on the PCB, thus electrically isolating them.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public bool IsoCutGCodeEnabled
        {
            get
            {
                return isoCutGCodeEnabled;
            }
            set
            {
                isoCutGCodeEnabled = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoGCodeFileOutputExtension value. This is the file we use
        /// save the gcode under 
        /// </summary>
        /// <remarks>will never get or set null or empty string</remarks>
        /// <history>
        ///    23 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("The filename extension to use when the Isolation GCode file is saved. The existing filename extension will be stripped off and replaced with this text.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public string IsoGCodeFileOutputExtension
        {
            get
            {
                if ((isoGCodeFileOutputExtension == null) || (isoGCodeFileOutputExtension.Length == 0)) isoGCodeFileOutputExtension = DEFAULT_ISOGCODEFILE_OUTPUTEXTENSION;
                return isoGCodeFileOutputExtension;
            }
            set
            {
                isoGCodeFileOutputExtension = value;
                if ((isoGCodeFileOutputExtension == null) || (isoGCodeFileOutputExtension.Length == 0)) isoGCodeFileOutputExtension = DEFAULT_ISOGCODEFILE_OUTPUTEXTENSION;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoFlipMode value. This determines how we flip the x or y
        /// coordinates so that we can cut bottom copper isolation traces.
        /// </summary>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This determines how we flip the X or Y coordinate so that bottom layer isolation traces line up with the ones cut on the top layer. Usually this is No_Flip for the top layer and X_Flip for bottom layers.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public IsoFlipModeEnum IsoFlipMode
        {
            get
            {
                return isoFlipMode;
            }
            set
            {
                isoFlipMode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoZCutLevel. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance into the material we cut defined in application units (inches,mm). The value should be negative as zero is traditionally the surface of the pcb being cut.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float IsoZCutLevel
        {
            get
            {
                return isoZCutLevel;
            }
            set
            {
                isoZCutLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoZMoveLevel. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance above the PCB we move the z axis to so that we can hop from cut to cut. It will be positive because it is above the surface of the pcb being cut.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float IsoZMoveLevel
        {
            get
            {
                return isoZMoveLevel;
            }
            set
            {
                isoZMoveLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoZClearLevel. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance above the PCB we move the z axis to so that we can move about. It will be positive because it is above the surface of the pcb being cut and should be large enough to clear all hold downs and clamps etc.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float IsoZClearLevel
        {
            get
            {
                return isoZClearLevel;
            }
            set
            {
                isoZClearLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoZFeedRate. Will never get or set a negative
        /// or zero value. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the speed at which the tool (in application units per minute) moves vertically into the work.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float IsoZFeedRate
        {
            get
            {
                if (isoZFeedRate <= 0) isoZFeedRate = DEFAULT_ISOZFEEDRATE;
                return isoZFeedRate;
            }
            set
            {
                isoZFeedRate = value;
                if (isoZFeedRate <= 0) isoZFeedRate = DEFAULT_ISOZFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoXYFeedRate. Will never get or set a negative
        /// or xyero value. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the speed at which the tool (in application units per minute) moves horizontally over the work.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float IsoXYFeedRate
        {
            get
            {
                if (isoXYFeedRate <= 0) isoXYFeedRate = DEFAULT_ISOXYFEEDRATE;
                return isoXYFeedRate;
            }
            set
            {
                isoXYFeedRate = value;
                if (isoXYFeedRate <= 0) isoXYFeedRate = DEFAULT_ISOXYFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoCutWidth. Will never get or set a negative
        /// or xyero value. 
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the width of the line the isolation milling bit cuts when at the IsoZCutLevel.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float IsoCutWidth
        {
            get
            {
                if (isoCutWidth <= 0) isoCutWidth = DEFAULT_ISOCUT_WIDTH;
                return isoCutWidth;
            }
            set
            {
                isoCutWidth = value;
                if (isoCutWidth <= 0) isoCutWidth = DEFAULT_ISOCUT_WIDTH;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoPadTouchDownsWanted
        /// or xyero value. 
        /// </summary>
        /// <history>
        ///    30 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This causes the isolation milling bit to touch down in the center of each pad to a distance of IsoPadTouchDownZLevel and hence provides a centering point for manual drilling.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public bool IsoPadTouchDownsWanted
        {
            get
            {
                return isoPadTouchDownsWanted;
            }
            set
            {
                isoPadTouchDownsWanted = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoPadTouchDownsWanted
        /// or xyero value. 
        /// </summary>
        /// <history>
        ///    30 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance into the work the isolation milling bit moves when creating pad touchdowns and should be negative because it is below the surface of the PCB. It is intended to provide a centering point for the manual drilling of through holes.")]
        [CategoryAttribute("Isolation Cuts")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float IsoPadTouchDownZLevel
        {
            get
            {
                return isoPadTouchDownZLevel;
            }
            set
            {
                isoPadTouchDownZLevel = value;
            }
        }

        #endregion

        // ####################################################################
        // ##### Reference Pins category items
        // ####################################################################
        #region Reference Pins category items

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets thereferencePinGCodeEnabled. 
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This option indicates if a Reference Pin GCode file is to be generated.")]
        [CategoryAttribute("Reference Pins")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public bool ReferencePinGCodeEnabled
        {
            get
            {
                return referencePinGCodeEnabled;
            }
            set
            {
                referencePinGCodeEnabled = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the referencePinsGCodeFileOutputExtension value. This is the file we use
        /// save the bed flattening gcode under 
        /// </summary>
        /// <remarks>will never get or set null or empty string</remarks>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the output file name extension for the Reference Pin GCode file (if generated).")]
        [CategoryAttribute("Reference Pins")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public string ReferencePinsGCodeFileOutputExtension
        {
            get
            {
                if ((referencePinsGCodeFileOutputExtension == null) || (referencePinsGCodeFileOutputExtension.Length == 0)) referencePinsGCodeFileOutputExtension = DEFAULT_REFERENCEPINSGCODEFILE_OUTPUTEXTENSION;
                return referencePinsGCodeFileOutputExtension;
            }
            set
            {
                referencePinsGCodeFileOutputExtension = value;
                if ((referencePinsGCodeFileOutputExtension == null) || (referencePinsGCodeFileOutputExtension.Length == 0)) referencePinsGCodeFileOutputExtension = DEFAULT_REFERENCEPINSGCODEFILE_OUTPUTEXTENSION;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the referencePinsZDrillDepth. 
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance into the bed material we drill, defined in application units (inches,mm), so the reference pins can be set. The value should be negative and should be large enough to ensure the pin is embedded firmly and protrudes above the surface of the PCB.")]
        [CategoryAttribute("Reference Pins")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float ReferencePinsZDrillDepth
        {
            get
            {
                return referencePinsZDrillDepth;
            }
            set
            {
                referencePinsZDrillDepth = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the referencePinsZClearLevel. 
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance above the bed we move the z axis to so that we can move about. It will be positive because it is above the surface of the pcb being cut and should be large enough to clear all hold downs and clamps etc.")]
        [CategoryAttribute("Reference Pins")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float ReferencePinsZClearLevel
        {
            get
            {
                return referencePinsZClearLevel;
            }
            set
            {
                referencePinsZClearLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the referencePinsZFeedRate. Will never get or set a negative
        /// or zero value. 
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the speed at which the drill (in application units per minute) moves vertically into the work.")]
        [CategoryAttribute("Reference Pins")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float ReferencePinsZFeedRate
        {
            get
            {
                if (referencePinsZFeedRate <= 0) referencePinsZFeedRate = DEFAULT_REFERENCEPINSZFEEDRATE;
                return referencePinsZFeedRate;
            }
            set
            {
                referencePinsZFeedRate = value;
                if (referencePinsZFeedRate <= 0) referencePinsZFeedRate = DEFAULT_REFERENCEPINSZFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the referencePinsXYFeedRate. Will never get or set a negative
        /// or zero value. 
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the speed at which the toolhead (in application units per minute) moves horizontally over the work.")]
        [CategoryAttribute("Reference Pins")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float ReferencePinsXYFeedRate
        {
            get
            {
                if (referencePinsXYFeedRate <= 0) referencePinsXYFeedRate = DEFAULT_REFERENCEPINSXYFEEDRATE;
                return referencePinsXYFeedRate;
            }
            set
            {
                referencePinsXYFeedRate = value;
                if (referencePinsXYFeedRate <= 0) referencePinsXYFeedRate = DEFAULT_REFERENCEPINSXYFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the referencePinsMaxNumber. Can be positive or negative
        /// or xyero value. 
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the maximum number of Reference Pins we expect to see in the Gerber File. It is used as a sanity check when generating the GCode.")]
        [CategoryAttribute("Reference Pins")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public int ReferencePinsMaxNumber
        {
            get
            {
                return referencePinsMaxNumber;
            }
            set
            {
                referencePinsMaxNumber = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the referencePinPadDiameter. 
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is diameter of the pads on the PCB which should be used as markers for Reference Pins. Only pads intended to be used as reference pins should be this size and the pads should be positioned in a rectangular or linear formation.")]
        [CategoryAttribute("Reference Pins")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float ReferencePinPadDiameter
        {
            get
            {
                if (referencePinPadDiameter <= 0) referencePinPadDiameter = DEFAULT_REFERENCEPINSPADDIAMETER;
                return referencePinPadDiameter;
            }
            set
            {
                referencePinPadDiameter = value;
                if (referencePinPadDiameter <= 0) referencePinPadDiameter = DEFAULT_REFERENCEPINSPADDIAMETER;
            }
        }


        #endregion

        // ####################################################################
        // ##### Excellon category items
        // ####################################################################
        #region Excellon category items

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Excellon drillingDrillingCoordinateZerosMode value. 
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This determines how the coordinates on the XY drill positions are scaled. The Excellon protocol (there are many different ones) does not provide any consistent method to specify this in the file itself - so you have to explicitly state it here.")]
        [CategoryAttribute("Excellon Drilling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public ExcellonDrillingCoordinateZerosModeEnum DrillingCoordinateZerosMode
        {
            get
            {
                return drillingDrillingCoordinateZerosMode;
            }
            set
            {
                drillingDrillingCoordinateZerosMode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the number of decimal places in the excellon file. Cannot be negative
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This indicates the number of decimal places used on the XY drill positions and hence how those values are scaled. The Excellon protocol (there are many different ones) does not provide any consistent method to specify this in the file itself - so you have to explicitly state it here.")]
        [CategoryAttribute("Excellon Drilling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public int DrillingNumberOfDecimalPlaces
        {
            get
            {
                if (drillingNumberOfDecimalPlaces < 0) drillingNumberOfDecimalPlaces = DRILLING_NUMBER_OF_DECIMAL_PLACES;
                return drillingNumberOfDecimalPlaces;
            }
            set
            {
                drillingNumberOfDecimalPlaces = value;
                if (drillingNumberOfDecimalPlaces < 0) drillingNumberOfDecimalPlaces = DRILLING_NUMBER_OF_DECIMAL_PLACES;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the drillingGCodeFileOutputExtension value. This is the file we use
        /// save the drill gcode under 
        /// </summary>
        /// <remarks>will never get or set null or empty string</remarks>
        /// <history>
        ///    03 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the output file name extension for the Drill GCode file (if generated).")]
        [CategoryAttribute("Excellon Drilling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public string DrillingGCodeFileOutputExtension
        {
            get
            {
                if ((drillingGCodeFileOutputExtension == null) || (drillingGCodeFileOutputExtension.Length == 0)) drillingGCodeFileOutputExtension = DEFAULT_DRILLINGGCODEFILE_OUTPUTEXTENSION;
                return drillingGCodeFileOutputExtension;
            }
            set
            {
                drillingGCodeFileOutputExtension = value;
                if ((drillingGCodeFileOutputExtension == null) || (drillingGCodeFileOutputExtension.Length == 0)) drillingGCodeFileOutputExtension = DEFAULT_DRILLINGGCODEFILE_OUTPUTEXTENSION;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the drillingZDepth. 
        /// </summary>
        /// <history>
        ///    03 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance into the PCB and bed material we drill, defined in application units (inches,mm). The value should be negative and should be large enough to ensure the drill completely exits the far side of the PCB.")]
        [CategoryAttribute("Excellon Drilling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float DrillingZDepth
        {
            get
            {
                return drillingZDepth;
            }
            set
            {
                drillingZDepth = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the drillingZClearLevel. 
        /// </summary>
        /// <history>
        ///    03 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the distance above the bed we move the z axis to so that we can move about. It will be positive because it is above the surface of the pcb we are drilling.")]
        [CategoryAttribute("Excellon Drilling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float DrillingZClearLevel
        {
            get
            {
                return drillingZClearLevel;
            }
            set
            {
                drillingZClearLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the drillingZFeedRate. Will never get or set a negative
        /// or zero value. 
        /// </summary>
        /// <history>
        ///    03 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the speed at which the drill (in application units per minute) moves vertically into the work.")]
        [CategoryAttribute("Excellon Drilling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float DrillingZFeedRate
        {
            get
            {
                if (drillingZFeedRate <= 0) drillingZFeedRate = DEFAULT_DRILLZFEEDRATE;
                return drillingZFeedRate;
            }
            set
            {
                drillingZFeedRate = value;
                if (drillingZFeedRate <= 0) drillingZFeedRate = DEFAULT_DRILLZFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the drillingXYFeedRate. Will never get or set a negative
        /// or zero value. 
        /// </summary>
        /// <history>
        ///    03 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This is the speed at which the toolhead (in application units per minute) moves horizontally over the work.")]
        [CategoryAttribute("Excellon Drilling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public float DrillingXYFeedRate
        {
            get
            {
                if (drillingXYFeedRate <= 0) drillingXYFeedRate = DEFAULT_DRILLINGZFEEDRATE;
                return drillingXYFeedRate;
            }
            set
            {
                drillingXYFeedRate = value;
                if (drillingXYFeedRate <= 0) drillingXYFeedRate = DEFAULT_DRILLINGZFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the drillingGCodeEnabled flag 
        /// </summary>
        /// <history>
        ///    07 Sep 10  Cynic - Started
        /// </history>
        [DescriptionAttribute("This option enables the conversion of the Excellon file into GCode suitable for drilling the holes for pads and vias. When the GCode file is run, you will be prompted to change the drill for different hole sizes.")]
        [CategoryAttribute("Excellon Drilling")]
        [ReadOnlyAttribute(false)]
        [BrowsableAttribute(true)]
        public bool DrillingGCodeEnabled
        {
            get
            {
                return drillingGCodeEnabled;
            }
            set
            {
                drillingGCodeEnabled = value;
            }
        }

        #endregion

    }
}
