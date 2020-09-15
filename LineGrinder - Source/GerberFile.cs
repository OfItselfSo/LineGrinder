using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OISCommon;
using System.ComponentModel;

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
    /// A class to encapsulate a Gerber file in an easily manipulable manner
    /// </summary>
    /// <history>
    ///    06 Jul 10  Cynic - Started
    /// </history>
    public class GerberFile : OISObjBase
    {
        // this is the path and name of the source Gerber file
        private string gerberFilePathAndName = null;

        // this is the current file source
        private List<GerberLine> sourceLines = new List<GerberLine>();

        // our state machine. Each line of a Gerber File can assume many things based
        // on the state of the previously executed commands
        GerberFileStateMachine stateMachine = new GerberFileStateMachine();

        public const string RS274PARAMDELIMITER = @"%";
        public const char RS274PARAMDELIMITER_CHAR = '%';
        public const string RS274FORMATPARAM = @"FS";
        public const string RS274MODEPARAM = @"MO";
        public const string RS274AMPARAM = @"AM";
        public const string RS274APDEFPARAM = @"AD";
        public const string RS274OFPARAM = @"OF";
        public const string RS274INPARAM = @"IN";
        public const string RS274IPPARAM = @"IP";
        public const string RS274LPPARAM = @"LP";
        public const string RS274LNPARAM = @"LN";
        public const string RS274SFA1B1PARAM = @"SFA1B1";
        public const string RS274SFPARAM = @"SF";
        public const string RS274BLOCKTERMINATOR = @"*";
        public const char RS274BLOCKTERMINATOR_CHAR = '*';

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value derived from the gerber file or gCode file although possibly
        //       it may be origin compensated

        // These are the values we ADD to the existing X and Y coords from the DCodes
        // in order to set the origin approximately at zero
        private float plotXCoordOriginAdjust = 0;
        private float plotYCoordOriginAdjust = 0;

        // we track these as we add lines in order to build an appropriately sized plot
        private float minDCodeXCoord = float.MaxValue;
        private float minDCodeYCoord = float.MaxValue;
        private float maxDCodeXCoord = float.MinValue;
        private float maxDCodeYCoord = float.MinValue;

        // these are built from the reference pins
        private float midDCodeXCoord = 0;
        private float midDCodeYCoord = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    06 Jul 10  Cynic - Started
        /// </history>
        public GerberFile()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets whether the file is populated. We assume it is populated if there
        /// is at least one source line
        /// </summary>
        /// <history>
        ///    12 Jul 10  Cynic - Started
        /// </history>
        public bool IsPopulated
        {
            get
            {
                if (SourceLines.Count > 0) return true;
                else return false;;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/sets the current gerber source files path and name
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        public string GerberFilePathAndName
        {
            get
            {
                return gerberFilePathAndName;
            }
            set
            {
                gerberFilePathAndName = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs fixups after the file has been opened and read to 
        /// adjust things so that code originally intended for a plotter works
        /// better in an isolation mode environment.
        /// </summary>
        /// <param name="errStr">we return the error string in here</param>
        /// <returns>true all ok, false a failure was detected</returns>
        /// <history>
        ///    15 Jan 11  Cynic - Started
        /// </history>
        public bool PerformOpenPostProcessingFixups(out string errStr)
        {
            // did we ever get a situation in which we are drawing lines but 
            // they were not for isolation routing?
            if (StateMachine.CurrentLinesAreNotForIsolationHasBeenEnabled == true)
            {
                // yes we did, we have to find all the GerberLine_DCode objects
                // and see if the application actually drew traces in for them
                // in prior X and Y commands. This can happen when some programs
                // generate flood fill commands. They draw the outline in thin traces
                // then generate a G36 code for the flood fill and runover the outline
                // again. If we leave the first codes in there the flooded area gets
                // cut into slightly by the isolation traces around it - which is not
                // the intended effect. On a gerber plot they would just be exposed twice

                // we go through each line in the gerber file and build a list of the ones
                // which are marked as CurrentLineIsNotForIsolation. Then we go back and see
                // if there are any lines before or after that which that lie under those lines
                // If so, we mark them as DoNotDisplay.
            }

            errStr = "";
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs checks after the file has been opened and read to 
        /// see if it is valid and useable
        /// </summary>
        /// <param name="errStr">we return the error string in here</param>
        /// <param name="applicationUnits">the units (inches or mm) the app is currently
        /// configured for</param>
        /// <returns>true all ok, false a failure was detected</returns>
        /// <history>
        ///    09 Aug 10  Cynic - Started
        /// </history>
        public bool PerformOpenPostProcessingChecks(out string errStr,ApplicationUnitsEnum applicationUnits)
        {
            // check to make sure there are valid Min and Max coodinates
            if (minDCodeXCoord == float.MaxValue)
            {
                errStr = "minimum X coordinate not found in D Codes - something wrong with this file.";
                return false;
            }
            if (minDCodeYCoord == float.MaxValue)
            {
                errStr = "minimum Y coordinate not found in D Codes - something wrong with this file.";
                return false;
            }
            if (maxDCodeXCoord == float.MinValue)
            {
                errStr = "maximum X coordinate not found in D Codes - something wrong with this file.";
                return false;
            }
            if (maxDCodeYCoord == float.MinValue)
            {
                errStr = "maximum Y coordinate not found in D Codes - something wrong with this file.";
                return false;
            }
            // we have to make sure the UNITS used by the Gerber file are the 
            // same as the UNITS currently set in this application
            if (this.GerberFileUnits == ApplicationUnitsEnum.INCHES)
            {
                if (applicationUnits ==ApplicationUnitsEnum.MILLIMETERS)
                {
                    errStr = "Application units set to millimeters and the Gerber file uses inches. Adjust the options on the Settings tab.";
                    return false;
                }
            }
            else if (this.GerberFileUnits == ApplicationUnitsEnum.MILLIMETERS)
            {
                if (applicationUnits ==ApplicationUnitsEnum.INCHES)
                {
                    errStr = "Application units set to inches and the Gerber file uses millimeters. Adjust the options on the Settings tab.";
                    return false;
                }
            }
            if (this.StateMachine.GerberFileCoordinateMode == GerberCoordinateModeEnum.COORDINATE_INCREMENTAL)
            {
                errStr = "Incremental coordinate mode is not supported. Please use absolute coordinates.";
                return false;
            }

            //TODO test these
            /*
             * 
                        if (SourceLines.Count ==0)
                        {
                            errStr = "PerformGerberToGCodeStep1: SourceLines.Count ==0";
                            LogMessage(errStr);
                            return 100;
                        }
                        if (plotSize.Width <= 0)
                        {
                            errStr = "PerformGerberToGCodeStep1: plotSize.Width <= 0";
                            LogMessage(errStr);
                            return 101;
                        }
                        if (plotSize.Height <= 0)
                        {
                            errStr = "PerformGerberToGCodeStep1: plotSize.Height <= 0";
                            LogMessage(errStr);
                            return 102;
                        }
                        if (isoPlotPointsPerAppUnit <= 0)
                        {
                            errStr = "PerformGerberToGCodeStep1: isoPlotPointsPerAppUnit <= 0";
                            LogMessage(errStr);
                            return 103;
                        }

             * 
             */

            errStr = "";
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Keeps a record of the min and max XY coords. This is not as simple
        /// as it seems because we have to also keep track of the aperture 
        /// diameters
        /// </summary>
        /// <history>
        ///    08 Aug 10  Cynic - Moved this code in here
        /// </history>
        public void RecordMinMaxXYCoords(float workingXCoord, float workingYCoord)
        {
            /*
            float tmpMaxX = workingXCoord + ApertureCollection.GetMaxApertureIncrementalDimension() + StateMachine.GerberFileManager.GetMaxToolWidthForEnabledOperationMode();
            float tmpMaxY = workingYCoord + ApertureCollection.GetMaxApertureIncrementalDimension() + StateMachine.GerberFileManager.GetMaxToolWidthForEnabledOperationMode();
            float tmpMinX = workingXCoord - ApertureCollection.GetMaxApertureIncrementalDimension() - StateMachine.GerberFileManager.GetMaxToolWidthForEnabledOperationMode();
            float tmpMinY = workingYCoord - ApertureCollection.GetMaxApertureIncrementalDimension() - StateMachine.GerberFileManager.GetMaxToolWidthForEnabledOperationMode();
             */
            float tmpMaxX = workingXCoord;
            float tmpMaxY = workingYCoord;
            float tmpMinX = workingXCoord;
            float tmpMinY = workingYCoord;

            // we set the min and max XY here
            if (tmpMinX < minDCodeXCoord) minDCodeXCoord = tmpMinX;
            if (tmpMinY < minDCodeYCoord) minDCodeYCoord = tmpMinY;
            // max valus
            if (tmpMaxX > maxDCodeXCoord) maxDCodeXCoord = tmpMaxX;
            if (tmpMaxY > maxDCodeYCoord) maxDCodeYCoord = tmpMaxY;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the value we use when flipping in the X direction
        /// </summary>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
        public float XFlipMax
        {
            get
            {
                return StateMachine.XFlipMax;
            }
            set
            {
                StateMachine.XFlipMax = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the value we use when flipping in the Y direction
        /// </summary>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
        public float YFlipMax
        {
            get
            {
                return StateMachine.YFlipMax;
            }
            set
            {
                StateMachine.YFlipMax = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the min plot X Coordinate value. There is no set accessor
        /// this value is derived from the max/min X coord and the origin compensation
        /// </summary>
        /// <history>
        ///    09 Jul 10  Cynic - Started
        /// </history>
        public float MinPlotXCoord
        {
            get
            {
                return minDCodeXCoord + plotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the min plot Y Coordinate value There is no set accessor
        /// this value is derived from the max/min Y coord and the origin compensation
        /// </summary>
        /// <history>
        ///    09 Jul 10  Cynic - Started
        /// </history>
        public float MinPlotYCoord
        {
            get
            {
                return minDCodeYCoord + plotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the max plot X Coordinate value There is no set accessor
        /// this value is derived from the max/min X coord and the origin compensation
        /// </summary>
        /// <history>
        ///    09 Jul 10  Cynic - Started
        /// </history>
        public float MaxPlotXCoord
        {
            get
            {
                return maxDCodeXCoord + plotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the max plot Y Coordinate value There is no set accessor
        /// this value is derived from the max/min Y coord and the origin compensation
        /// </summary>
        /// <history>
        ///    09 Jul 10  Cynic - Started
        /// </history>
        public float MaxPlotYCoord
        {
            get
            {
                return maxDCodeYCoord + plotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the min X Coordinate value
        /// </summary>
        /// <history>
        ///    09 Jul 10  Cynic - Started
        /// </history>
        public float MinDCodeXCoord
        {
            get
            {
                return minDCodeXCoord;
            }
            set
            {
                minDCodeXCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the min Y Coordinate value
        /// </summary>
        /// <history>
        ///    09 Jul 10  Cynic - Started
        /// </history>
        public float MinDCodeYCoord
        {
            get
            {
                return minDCodeYCoord;
            }
            set
            {
                minDCodeYCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the mid X Coordinate value. These are derived from the reference
        /// pins and may be zero if they are not set
        /// </summary>
        /// <history>
        ///    10 Sep 10  Cynic - Started
        /// </history>
        public float MidDCodeXCoord
        {
            get
            {
                return midDCodeXCoord;
            }
            set
            {
                midDCodeXCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the mid Y Coordinate value. These are derived from the reference
        /// pins and may be zero if they are not set
        /// </summary>
        /// <history>
        ///    10 Sep 10  Cynic - Started
        /// </history>
        public float MidDCodeYCoord
        {
            get
            {
                return midDCodeYCoord;
            }
            set
            {
                midDCodeYCoord = value;
            }
        }
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the max X Coordinate value
        /// </summary>
        /// <history>
        ///    09 Jul 10  Cynic - Started
        /// </history>
        public float MaxDCodeXCoord
        {
            get
            {
                return maxDCodeXCoord;
            }
            set
            {
                maxDCodeXCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the max Y Coordinate value
        /// </summary>
        /// <history>
        ///    09 Jul 10  Cynic - Started
        /// </history>
        public float MaxDCodeYCoord
        {
            get
            {
                return maxDCodeYCoord;
            }
            set
            {
                maxDCodeYCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current X Coordinate value with origin compensation
        /// </summary>
        /// <history>
        ///    10 Sep 10  Cynic - Started
        /// </history>
        public float ConvertXCoordToOriginCompensated(float x0)
        {
            return x0 + PlotXCoordOriginAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current Y Coordinate value with origin compensation
        /// </summary>
        /// <history>
        ///    10 Sep 10  Cynic - Started
        /// </history>
        public float ConvertYCoordToOriginCompensated(float y0)
        {
            return y0 + PlotYCoordOriginAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current X Coordinate value with flipping applied (if necessary)
        /// </summary>
        /// <history>
        ///   10 Sep 10  Cynic - Started
        /// </history>
        public float ConvertXCoordToOriginCompensatedFlipped(float xCoordToFlip)
        {
            if (StateMachine.GerberFileManager.IsoFlipMode == FileManager.IsoFlipModeEnum.X_Flip)
            {
                return (stateMachine.XFlipMax - ConvertXCoordToOriginCompensated(xCoordToFlip));
            }
            // not an X flip. Just return this
            return ConvertXCoordToOriginCompensated(xCoordToFlip);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current Y Coordinate value with flipping applied (if necessary)
        /// </summary>
        /// <history>
        ///   10 Sep 10  Cynic - Started
        /// </history>
        public float ConvertYCoordToOriginCompensatedFlipped(float yCoordToFlip)
        {
            if (StateMachine.GerberFileManager.IsoFlipMode == FileManager.IsoFlipModeEnum.Y_Flip)
            {
                return (stateMachine.YFlipMax - ConvertYCoordToOriginCompensated(yCoordToFlip));
            }
            // not an Y flip. Just return this
            return ConvertYCoordToOriginCompensated(yCoordToFlip);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current dimension mode. There is no set accessor
        /// as this is derived from the header processing.
        /// </summary>
        /// <history>
        ///    12 Jul 10  Cynic - Started
        /// </history>
        public ApplicationUnitsEnum GerberFileUnits
        {
            get
            {
                return StateMachine.GerberFileUnits;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets state machine. Will never set or get a null value.
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
        public GerberFileStateMachine StateMachine
        {
            get
            {
                if (stateMachine == null) stateMachine = new GerberFileStateMachine();
                return stateMachine;
            }
            set
            {
                stateMachine = value;
                if (stateMachine == null) stateMachine = new GerberFileStateMachine();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the aperture collection. 
        /// </summary>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public GerberApertureCollection ApertureCollection
        {
            get
            {
                return StateMachine.ApertureCollection;
            }
            set
            {
                StateMachine.ApertureCollection = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Gerber file source. Will never set or get a null value.
        /// </summary>
        /// <history>
        ///    06 Jul 10  Cynic - Started
        /// </history>
        [BrowsableAttribute(false)]
        public List<GerberLine> SourceLines
        {
            get
            {
                if (sourceLines == null) sourceLines = new List<GerberLine>();
                return sourceLines;
            }
            set
            {
                sourceLines = value;
                if (sourceLines == null) sourceLines = new List<GerberLine>();
                LogMessage("SourceLines Set, lines =" + sourceLines.Count.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns all lines as a string array. Will never get a null value.
        /// </summary>
        /// <history>
        ///    06 Jul 10  Cynic - Started
        /// </history>
        public string[] GetRawSourceLinesAsArray()
        {
            List<string> retObj = new List<string>();

            // get all of the lines into the list
            foreach (GerberLine gLineObj in SourceLines)
            {
                retObj.Add(gLineObj.RawLineStr);
            }

            return retObj.ToArray();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a Gerber file line to the SourceLines
        /// </summary>
        /// <param name="lineStr">The line to add</param>
        /// <param name="lineNumber">The line number</param>
        /// <history>
        ///    06 Jul 10  Cynic - Started
        /// </history>
        public int AddLine(string lineStr, int lineNumber)
        {
            int retInt;
            string tmpLine1;
            char[] paramCharsToTrim = { RS274PARAMDELIMITER_CHAR, ' ' };
            char[] blockSplitChars= { RS274BLOCKTERMINATOR_CHAR };
            string[] blockArray = null;

            if(lineStr==null) return 100;

            // we may have multiple blocks on the same line
            blockArray = lineStr.Trim().Split(blockSplitChars, StringSplitOptions.RemoveEmptyEntries);
            // this should never be null
            if (blockArray == null) return 200;

            // for each block in the parameter (usually only one)
            foreach (string workingLine1 in blockArray)
            {
                // we must strip off any parameter delimiters - these will either be
                // the first or last. Is it just a trailing delim?
                if (workingLine1 == RS274PARAMDELIMITER) continue;
                // trim off fore and after
                tmpLine1 = workingLine1.Trim(paramCharsToTrim);

                // we must detect the line type based on the first two letters
                // Are we a FORMAT PARAMETER?
                if (tmpLine1.StartsWith(RS274FORMATPARAM) == true)
                {
                    // we are a FORMAT PARAMETER
                    GerberLine_FSCode fsObj = new GerberLine_FSCode(lineStr, tmpLine1, lineNumber);
                    retInt = fsObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(fs), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 300;
                    }
                    // it is good, add it
                    sourceLines.Add(fsObj);
                    // record this as well
                    StateMachine.FormatParameter = fsObj;
                    continue;
                }
                // Are we a MODE PARAMETER?
                else if (tmpLine1.StartsWith(RS274MODEPARAM) == true)
                {
                    // we are a MODE PARAMETER
                    GerberLine_MOCode moObj = new GerberLine_MOCode(lineStr, tmpLine1, lineNumber);
                    retInt = moObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(mo), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 400;
                    }
                    // it is good, add it
                    sourceLines.Add(moObj);
                    // record this as well
                    StateMachine.GerberFileUnits = moObj.GerberFileUnits;
                    continue;
                }
                // Are we a ADD PARAMETER?
                else if (tmpLine1.StartsWith(RS274APDEFPARAM) == true)
                {
                    // we are a APDEF PARAMETER
                    GerberLine_ADCode adObj = new GerberLine_ADCode(lineStr, tmpLine1, lineNumber);
                    retInt = adObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(ad), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 500;
                    }
                    // it is good, add it
                    sourceLines.Add(adObj);
                    // add this aperture object to the collection
                    (ApertureCollection as ICollection<GerberLine_ADCode>).Add(adObj);
                    continue;
                }
                // Are we a AM PARAMETER?
                else if (tmpLine1.StartsWith(RS274AMPARAM) == true)
                {
                    // if we get here we are a line of unknown type
                    LogMessage("Aperture Macrow (%AM) >" + lineStr + "< Specified on line " + lineNumber.ToString());
                    throw new NotImplementedException("Unsupported Aperture Macro (%AM) code on line " + lineNumber.ToString() + ".\n\nPlease configure your software to not output aperture macros in the Gerber code - most software can do this.\nEagle users please see the \"Configuring Eagle\" help page as it is a bit tricky.");
                }
                // Are we a IN PARAMETER?
                else if (tmpLine1.StartsWith(RS274INPARAM) == true)
                {
                    // we are a IN PARAMETER
                    GerberLine_INCode lnObj = new GerberLine_INCode(lineStr, tmpLine1, lineNumber);
                    retInt = lnObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(IN), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 500;
                    }
                    // it is good, add it
                    sourceLines.Add(lnObj);
                    continue;
                }
                // Are we a IP PARAMETER?
                else if (tmpLine1.StartsWith(RS274IPPARAM) == true)
                {
                    // we are a IP PARAMETER
                    GerberLine_IPCode ipObj = new GerberLine_IPCode(lineStr, tmpLine1, lineNumber);
                    retInt = ipObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(IP), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 500;
                    }
                    // it is good, add it
                    sourceLines.Add(ipObj);
                    continue;
                }
                // Are we a LP PARAMETER?
                else if (tmpLine1.StartsWith(RS274LPPARAM) == true)
                {
                    // we are a LP PARAMETER
                    GerberLine_LPCode lpObj = new GerberLine_LPCode(lineStr, tmpLine1, lineNumber);
                    retInt = lpObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(LP), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 500;
                    }
                    // it is good, add it
                    sourceLines.Add(lpObj);
                    continue;
                }
                // Are we a LN PARAMETER?
                else if (tmpLine1.StartsWith(RS274LNPARAM) == true)
                {
                    // we are a LN PARAMETER
                    GerberLine_LNCode lnObj = new GerberLine_LNCode(lineStr, tmpLine1, lineNumber);
                    retInt = lnObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(LN), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 500;
                    }
                    // it is good, add it
                    sourceLines.Add(lnObj);
                    continue;
                }
                // Are we a OF PARAMETER?
                else if (tmpLine1.StartsWith(RS274OFPARAM) == true)
                {
                    // we are a OF PARAMETER
                    GerberLine_OFCode ofObj = new GerberLine_OFCode(lineStr, tmpLine1, lineNumber);
                    retInt = ofObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(OF), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 550;
                    }
                    // it is good, add it
                    sourceLines.Add(ofObj);
                    continue;
                }
                // Are we a SFA1B1 PARAMETER? This is the only type of SF parameter we support
                // if it is an SF of any other type we generate an error
                else if (tmpLine1.StartsWith(RS274SFPARAM) == true)
                {
                    // we are a SFA1B1 PARAMETER
                    GerberLine_SFCode lnObj = new GerberLine_SFCode(lineStr, tmpLine1, lineNumber);
                    retInt = lnObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(SFA1B1), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 560;
                    }
                    // it is good, add it
                    sourceLines.Add(lnObj);
                    continue;
                }
                // Are we a D Code? These can start with D, X or Y depending if the line is modal
                else if ((tmpLine1.StartsWith("D") == true) || (tmpLine1.StartsWith("X") == true) || (tmpLine1.StartsWith("Y")== true)) 
                {
                    // we are a D Code
                    GerberLine_DCode dObj = new GerberLine_DCode(lineStr, tmpLine1, lineNumber);
                    retInt = dObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(d), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 600;
                    }
                    // it is good, add it
                    sourceLines.Add(dObj);
                    // are we a set aperture DCode?
                    if (dObj.CurrentDCode > 3)
                    {
                        // yes we are, set the aperture now
                        stateMachine.CurrentAperture = stateMachine.ApertureCollection.GetApertureByID(dObj.CurrentDCode);
                    }

                    // if the D code is a D01 (draw line) or D03 Flash Aperture we will actually make a mark
                    // on the plot. We need to record the max and min coordinates in this case
                    if (dObj.CurrentDCode == 1)
                    {
                        // record both the start and the end points for a draw line
                        RecordMinMaxXYCoords(StateMachine.LastDCodeXCoord, StateMachine.LastDCodeYCoord);
                        RecordMinMaxXYCoords(dObj.DCodeXCoord, dObj.DCodeYCoord);
                    }
                    else if (dObj.CurrentDCode == 3)
                    {
                        // record just the end points for a flash aperture
                        RecordMinMaxXYCoords(dObj.DCodeXCoord, dObj.DCodeYCoord);
                        // also record these centerpoints to our list
                        stateMachine.PadCenterPointList.Add(new GerberPad(dObj.DCodeXCoord, dObj.DCodeYCoord, (stateMachine.CurrentAperture.GetApertureDimension())));
                    }
                    // we also need to update the StateMachine with the latest X,Y coords and the implied DCode
                    StateMachine.LastDCode = dObj.CurrentDCode;
                    StateMachine.LastDCodeXCoord = dObj.DCodeXCoord;
                    StateMachine.LastDCodeYCoord = dObj.DCodeYCoord;
                    continue;
                }
                // Are we a M Code? These can start with M, Note that MO parameters have been checked above
                else if (tmpLine1.StartsWith("M") == true)
                {
                    // we are a M Code
                    GerberLine_MCode mObj = new GerberLine_MCode(lineStr, tmpLine1, lineNumber);
                    retInt = mObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(m), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 700;
                    }
                    // it is good, add it
                    sourceLines.Add(mObj);
                    continue;
                }
                // Are we a G36 Code? These can start with G36
                else if (tmpLine1.StartsWith("G36") == true)
                {
                    // we are a G36 Code
                    GerberLine_G36Code g36Obj = new GerberLine_G36Code(lineStr, tmpLine1, lineNumber);
                    retInt = g36Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g36), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 836;
                    }
                    // it is good, add it
                    sourceLines.Add(g36Obj);
                    continue;
                }
                // Are we a G37 Code? These can start with G37
                else if (tmpLine1.StartsWith("G37") == true)
                {
                    // we are a G37 Code
                    GerberLine_G37Code g37Obj = new GerberLine_G37Code(lineStr, tmpLine1, lineNumber);
                    retInt = g37Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g37), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 837;
                    }
                    // it is good, add it
                    sourceLines.Add(g37Obj);
                    continue;
                }
                // Are we a G54 Code? These can start with G54
                else if (tmpLine1.StartsWith("G54") == true)
                {
                    // we are a G54 Code
                    GerberLine_G54Code g54Obj = new GerberLine_G54Code(lineStr, tmpLine1, lineNumber);
                    retInt = g54Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g54), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 854;
                    }
                    // it is good, add it
                    sourceLines.Add(g54Obj);
                    // also set the aperture now
                    stateMachine.CurrentAperture = stateMachine.ApertureCollection.GetApertureByID(g54Obj.CurrentDCode);
                    continue;
                }
                // Are we a G Code? These can start with G
                else if (tmpLine1.StartsWith("G") == true)
                {
                    // we are a G Code
                    GerberLine_GCode gObj = new GerberLine_GCode(lineStr, tmpLine1, lineNumber);
                    retInt = gObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 800;
                    }
                    // it is good, add it
                    sourceLines.Add(gObj);
                    continue;
                }
                else
                {
                    // if we get here we are a line of unknown type
                    LogMessage("Unknown Gerber line type >" + lineStr + "< on line " + lineNumber.ToString());
                    if (lineStr.Length > 20)
                    {
                        throw new NotImplementedException("Cannot cope with unknown Gerber code on line " + lineNumber.ToString() + " code is >" + lineStr.Substring(0, 20) + "<");
                    }
                    else
                    {
                        throw new NotImplementedException("Cannot cope with unknown Gerber code on line " + lineNumber.ToString() + " code is >" + lineStr + "<");
                    }
                }

            }

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Figures out the mid point of all pads flagged as reference pins.
        /// </summary>
        /// <param name="errStr">an error string</param>
        /// 
        /// <returns>z success, -ve err not reported, +ve err reported</returns>
        /// <history>
        ///    10 Sep 10  Cynic - Started
        /// </history>
        public int GetMidPointFromReferencePins(out float midX, out float midY, ref string errStr)
        {
            int refPinCount = 0;
            float minX=float.MaxValue;
            float maxX=float.MinValue;
            float minY=float.MaxValue;
            float maxY=float.MinValue;

            errStr="";
            midX = 0;
            midY = 0;

            // go through all the pads, we just look for the min and max and 
            // assume those are the extremities
            foreach (GerberPad padObj in StateMachine.PadCenterPointList)
            {
                // test it 
                if(padObj.IsRefPin == false) continue;
                refPinCount++;
                if(padObj.X0 < minX) minX = padObj.X0;
                if(padObj.X0 > maxX) maxX = padObj.X0;
                if(padObj.Y0 < minY) minY = padObj.Y0;
                if(padObj.Y0 > maxY) maxY = padObj.Y0;
            }

            // did we find at least two?
            if(refPinCount<2)
            {
                errStr = refPinCount.ToString()+ " ref pin pads were found in the Gerber file. At least two are required.\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                LogMessage("GetMidPointFromReferencePins: " + errStr);
                return -201;
            }

            // these are the midpoints
            midX = minX + ((maxX - minX) / 2);
            midY = minY + ((maxY - minY) / 2);
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flags all of the reference pin pads in the PadCenterPointList of this 
        /// object and performs some checks.
        /// </summary>
        /// <param name="errStr">an error string</param>
        /// <returns>z success, -ve err not reported, +ve err reported</returns>
        /// <history>
        ///    10 Sep 10  Cynic - Started
        /// </history>
        public int SetReferencePins(ref string errStr)
        {
            errStr = "";

            LogMessage("SetReferencePins called");

            // can we find any reference pins? The PadCenterPoint list will have these from the Gerber file
            if (StateMachine.PadCenterPointList.Count == 0)
            {
                errStr = "No pads were found in the Gerber file.\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                LogMessage("SetReferencePins: " + errStr);
                return -201;
            }
            // ok we have some pads have we got any of the required size?
            List<GerberPad> refPadsList = new List<GerberPad>();
            foreach (GerberPad padObj in StateMachine.PadCenterPointList)
            {
                //DebugMessage("padObj.PadDiameter = " + padObj.PadDiameter.ToString());
                // reset it 
                padObj.IsRefPin = false;
                // test it
                if (padObj.PadDiameter != StateMachine.GerberFileManager.ReferencePinPadDiameter) continue;
                // mark it
                padObj.IsRefPin = true;
                // add it
                refPadsList.Add(padObj);
            }
            if (refPadsList.Count == 0)
            {
                errStr = "No Reference Pin pads were found in the Gerber file. The File Manager thinks they should have diameter: " + StateMachine.GerberFileManager.ReferencePinPadDiameter.ToString() + "\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                LogMessage("SetReferencePins: " + errStr);
                return -202;
            }
            // we have to have at least 2 pins
            if (refPadsList.Count < 2)
            {
                errStr = "Only one Reference Pin pad of diameter: " + StateMachine.GerberFileManager.ReferencePinPadDiameter.ToString() + " was found in the Gerber file. At least two are required.\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                LogMessage("SetReferencePins: " + errStr);
                return -203;
            }
            if (refPadsList.Count > StateMachine.GerberFileManager.ReferencePinsMaxNumber)
            {
                errStr = refPadsList.Count.ToString() + " Reference Pin pads of diameter: " + StateMachine.GerberFileManager.ReferencePinPadDiameter.ToString() + " were found in the Gerber file. According to the File Manager ReferencePinsMaxNumber setting this is unlikely to be correct.\n\nDo other pins on PCB schematic have the Reference Pin Diameter of " + StateMachine.GerberFileManager.ReferencePinPadDiameter.ToString() + "?";
                LogMessage("SetReferencePins: " + errStr);
                return -204;
            }
            // are the reference pins all co-linear with at least one other 
            foreach (GerberPad gcPadObj in refPadsList)
            {
                bool axiallyCoLinear = false;
                foreach (GerberPad testObj in refPadsList)
                {
                    // is it the same point? do not test
                    if ((gcPadObj.X0 == testObj.X0) && (gcPadObj.Y0 == testObj.Y0)) continue;
                    // do only the XCoords match
                    if (gcPadObj.X0 == testObj.X0)
                    {
                        // flag it
                        axiallyCoLinear = true;
                        continue;
                    }
                    // do only the YCoords match
                    if (gcPadObj.Y0 == testObj.Y0)
                    {
                        // flag it
                        axiallyCoLinear = true;
                        continue;
                    }
                    // if we get here the gcPadObj is not axially colinear with the testObj
                }
                // is the gcPadObj axially colinear with any other object?
                if (axiallyCoLinear == false)
                {
                    // no, we could not find any other objects. This is an error
                    errStr = "The Reference Pin pad at postion (" + gcPadObj.X0.ToString() + "," + gcPadObj.Y0.ToString() + ") is not axially co-linear with any other Reference Pin pad.\n\nAll Reference Pin Pads on the PCB schematic must be on a rectangle or line parallel to the X or Y axis.";
                    LogMessage("SetReferencePins: " + errStr);
                    return -206;

                }
            }
            // OK, everything looks good, the reference pin pads seem to be setup correctly
            LogMessage("SetReferencePins successful completion");
            return 0;

        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current X Coord Origin Adjust value. These are compensating
        /// values we apply to the current coordinates in order to make the
        /// smallest X coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// </summary>
        /// <history>
        ///    08 Aug 10  Cynic - Started
        /// </history>
        public float PlotXCoordOriginAdjust
        {
            get
            {
                return plotXCoordOriginAdjust;
            }
            set
            {
                plotXCoordOriginAdjust = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Y Coord Origin Adjust value. These are compensating
        /// values we apply to the current coordinates in order to make the
        /// smallest Y coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// </summary>
        /// <history>
        ///    08 Aug 10  Cynic - Started
        /// </history>
        public float PlotYCoordOriginAdjust
        {
            get
            {
                return plotYCoordOriginAdjust;
            }
            set
            {
                plotYCoordOriginAdjust = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Applies the current coordinate Origin Adjust value. These are compensating
        /// values we ADD to the current coordinates in order to make the
        /// smallest XY coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// </summary>
        /// <param name="xCoordAdjust">x origin adjuster</param>
        /// <param name="yCoordAdjust">y origin adjuster</param>
        /// <history>
        ///    08 Aug 10  Cynic - Started
        /// </history>
        public void SetPlotOriginCoordinateAdjustments(float xCoordAdjust, float yCoordAdjust)
        {
            LogMessage("Gerber Origin coord adjustments: xCoordAdjust=" + xCoordAdjust.ToString() + " yCoordAdjust=" + yCoordAdjust.ToString());
            // just run through and apply it to each GerberLine whether it uses it or not
            foreach (GerberLine gLine in SourceLines)
            {
                gLine.PlotXCoordOriginAdjust = xCoordAdjust;
                gLine.PlotYCoordOriginAdjust = yCoordAdjust;
            }
            PlotXCoordOriginAdjust = xCoordAdjust;
            PlotYCoordOriginAdjust = yCoordAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gerber file options to use. Never gets/sets a null value
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        public FileManager GerberFileManager
        {
            get
            {
                return StateMachine.GerberFileManager;
            }
            set
            {
                StateMachine.GerberFileManager = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Applies the some adjustments to the max X and Y so that the plot is
        /// slightly larger than the outside of the furthest object + isolation
        /// distance
        /// </summary>
        /// <param name="xMaxAdjust">new X Max value</param>
        /// <param name="yMaxAdjust">new Y Max value</param>
        /// <history>
        ///    09 Aug 10  Cynic - Started
        /// </history>
        public void SetMaxPlotCoordinateAdjustments(float xMaxAdjust, float yMaxAdjust)
        {
            MaxDCodeXCoord = xMaxAdjust;
            MaxDCodeYCoord = yMaxAdjust;
        }
 
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Plots the Gerber file contents on the designated graphics object
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public void PlotGerberFile(Graphics graphicsObj, float isoPlotPointsPerAppUnit)
        {
            int errInt=0;
            string errStr="";
            GerberLine.PlotActionEnum errAction = GerberLine.PlotActionEnum.PlotAction_End;

            // set the StateMachine
            StateMachine.ResetForPlot();
            StateMachine.IsoPlotPointsPerAppUnit = isoPlotPointsPerAppUnit;

            foreach (GerberLine lineObj in SourceLines)
            {
                errAction = lineObj.PerformPlotGerberAction(graphicsObj, StateMachine, ref errInt, ref errStr);
                if (errAction == GerberLine.PlotActionEnum.PlotAction_Continue)
                {
                    // all is well
                    continue;
                }
                if (errAction == GerberLine.PlotActionEnum.PlotAction_End)
                {
                    // we are all done
                    return;
                }
                else if (errAction == GerberLine.PlotActionEnum.PlotAction_FailWithError)
                {
                    // handle this error
                    LogMessage("Plot Failed on obj:" + lineObj.ToString() + " at line number " + lineObj.LineNumber.ToString());
                    return;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Processes a gerber file into GCode
        /// </summary>
        /// <param name="errStr">we return error messages here</param>
        /// <param name="plotSize">the size of the plot we create our GCode templates on. This is
        /// <param name="builderObjOut">the GCodeBuilder object we create in this step</param>
        /// the graphical object we use to figure out our isolation paths. These size should be
        /// able to take the max gerberCoord*virtualCoordPerPlotUnit listed in the Gerber file</param>
        /// <param name="virtualCoordPerPlotUnit">the number of coordinates in the GerberFile per plot unit. For example
        /// if the GerberFile says X=1.456 and this is 1000 then we would goto X=1456 in the plot</param>
        /// <remarks>
        /// NOTE: this is Step1 in the Graphical Stigmergy (GS) process. After this function completes
        ///       we will have an GCodeBuilder object which contains an array of ints (the isolation plot)
        ///       and the values in that array (called pixels) will be annotated with values which indicate
        ///       which Gerber Objects use them, how they use them (edge or background). If other
        ///       Gerber Objects share the same pixel the pixel contains the ID of an overlay object
        ///       which then contains the above information for one or more Gerber objects.
        /// </remarks>
        /// <history>
        ///    22 Jul 10  Cynic - Started
        /// </history>
        public int PerformGerberToGCodeStep1(out GCodeBuilder builderObjOut, Size plotSize, float virtualCoordPerPlotUnit, ref string errStr)
        {
            int errInt = 0;
            int lineCount = 0;
            GerberLine.PlotActionEnum errAction = GerberLine.PlotActionEnum.PlotAction_End;

            // init this
            builderObjOut = null;

            // set the StateMachine
            StateMachine.ResetForPlot();
            StateMachine.IsoPlotPointsPerAppUnit = virtualCoordPerPlotUnit;

            // we gotta have one
            if (SourceLines.Count ==0)
            {
                errStr = "PerformGerberToGCodeStep1: SourceLines.Count ==0";
                LogMessage(errStr);
                return 100;
            }
            if (plotSize.Width <= 0)
            {
                errStr = "PerformGerberToGCodeStep1: plotSize.Width <= 0";
                LogMessage(errStr);
                return 101;
            }
            if (plotSize.Height <= 0)
            {
                errStr = "PerformGerberToGCodeStep1: plotSize.Height <= 0";
                LogMessage(errStr);
                return 102;
            }
            if (virtualCoordPerPlotUnit <= 0)
            {
                errStr = "PerformGerberToGCodeStep1: virtualCoordPerPlotUnit <= 0";
                LogMessage(errStr);
                return 103;
            }

            // create GCodeBuilder with a 2D array equivalent to the plotsize
            builderObjOut = new GCodeBuilder(plotSize.Width, plotSize.Height);
            StateMachine.ResetForPlot();
            // give the manager to the builder object now
            builderObjOut.GCodeBuilderFileManager = GerberFileManager;

            // now we carefully draw each item on the array using our custom
            // drawing tools. See the alogrythm document for what is going on here
            foreach (GerberLine lineObj in this.SourceLines)
            {
                lineCount++;
              //  DebugMessage("Processing Line" + lineCount.ToString());
                errAction = lineObj.PerformPlotIsoStep1Action(builderObjOut, StateMachine, ref errInt, ref errStr);
                if (errAction == GerberLine.PlotActionEnum.PlotAction_Continue)
                {
                    // all is well
                    continue;
                }
                if (errAction == GerberLine.PlotActionEnum.PlotAction_End)
                {
                    // we are all done for this stage
                    break;
                }
                else if (errAction == GerberLine.PlotActionEnum.PlotAction_FailWithError)
                {
                    // handle this error
                    errStr = "GCode Plot Failed on obj:" + lineObj.ToString();
                    LogMessage(errStr);
                    return 201;
                }
            }
            return 0;
        }

    }
}
