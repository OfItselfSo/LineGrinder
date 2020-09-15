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
    /// A class to encapsulate a Excellon file in an easily manipulable manner
    /// </summary>
    /// <history>
    ///    01 Sep 10  Cynic - Started
    /// </history>
    public class ExcellonFile : OISObjBase
    {
        // this is the path and name of the source Excellon file
        private string excellonFilePathAndName = null;

        // this is the current file source
        private List<ExcellonLine> sourceLines = new List<ExcellonLine>();

        // our state machine. Each line of a Excellon File can assume many things based
        // on the state of the previously executed commands
        ExcellonFileStateMachine stateMachine = new ExcellonFileStateMachine();

        public const string ANSI349COMMENTDELIMITER = @";";

/*
        public const string RS274PARAMDELIMITER = @"%";
        public const char RS274PARAMDELIMITER_CHAR = '%';
        public const string RS274FORMATPARAM = @"FS";
        public const string RS274MODEPARAM = @"MO";
        public const string RS274APDEFPARAM = @"AD";
        public const string RS274BLOCKTERMINATOR = @"*";
        public const char RS274BLOCKTERMINATOR_CHAR = '*';
*/

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value derived from the excellon file or gCode file although possibly
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

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public ExcellonFile()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets whether the file is populated. We assume it is populated if there
        /// is at least one source line
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
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
        /// Gets/sets the current excellon source files path and name
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public string ExcellonFilePathAndName
        {
            get
            {
                return excellonFilePathAndName;
            }
            set
            {
                excellonFilePathAndName = value;
            }
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
        ///    01 Sep 10  Cynic - Started
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

            // we have to make sure the UNITS used by the Excellon file are the 
            // same as the UNITS currently set in this application
            if (this.StateMachine.ExcellonFileUnits == ApplicationUnitsEnum.INCHES)
            {
                if (applicationUnits == ApplicationUnitsEnum.MILLIMETERS)
                {
                    errStr = "Application units set to millimeters and the Gerber file uses inches. Adjust the options on the Settings tab.";
                    return false;
                }
            }
            else if (this.StateMachine.ExcellonFileUnits == ApplicationUnitsEnum.MILLIMETERS)
            {
                if (applicationUnits == ApplicationUnitsEnum.INCHES)
                {
                    errStr = "Application units set to inches and the Gerber file uses millimeters. Adjust the options on the Settings tab.";
                    return false;
                }
            }

            errStr = "";
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Keeps a record of the min and max XY coords.
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Moved this code in here
        /// </history>
        public void RecordMinMaxXYCoords(float workingXCoord, float workingYCoord)
        {
            // we set the min and max XY here
            if (workingXCoord < minDCodeXCoord) minDCodeXCoord = workingXCoord;
            if (workingYCoord < minDCodeYCoord) minDCodeYCoord = workingYCoord;
            // max valus
            if (workingXCoord > maxDCodeXCoord) maxDCodeXCoord = workingXCoord;
            if (workingYCoord > maxDCodeYCoord) maxDCodeYCoord = workingYCoord;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the value we use when flipping in the X direction
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        /// Gets/Sets the max X Coordinate value
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        /// Gets/Sets state machine. Will never set or get a null value.
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public ExcellonFileStateMachine StateMachine
        {
            get
            {
                if (stateMachine == null) stateMachine = new ExcellonFileStateMachine();
                return stateMachine;
            }
            set
            {
                stateMachine = value;
                if (stateMachine == null) stateMachine = new ExcellonFileStateMachine();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the tool table collection. 
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public List<ExcellonLine_ToolTable> ToolCollection
        {
            get
            {
                return StateMachine.ToolCollection;
            }
            set
            {
                StateMachine.ToolCollection = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Excellon file source. Will never set or get a null value.
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        [BrowsableAttribute(false)]
        public List<ExcellonLine> SourceLines
        {
            get
            {
                if (sourceLines == null) sourceLines = new List<ExcellonLine>();
                return sourceLines;
            }
            set
            {
                sourceLines = value;
                if (sourceLines == null) sourceLines = new List<ExcellonLine>();
                LogMessage("SourceLines Set, lines =" + sourceLines.Count.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns all lines as a string array. Will never get a null value.
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public string[] GetRawSourceLinesAsArray()
        {
            List<string> retObj = new List<string>();

            // get all of the lines into the list
            foreach (ExcellonLine gLineObj in SourceLines)
            {
                retObj.Add(gLineObj.RawLineStr);
            }

            return retObj.ToArray();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a Excellon file line to the SourceLines
        /// </summary>
        /// <param name="lineStr">The line to add</param>
        /// <param name="lineNumber">The line number</param>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public int AddLine(string lineStr, int lineNumber)
        {
            int retInt;
            string tmpLine1;

            if(lineStr==null) return 100;
            // trim it up, remove comments
            tmpLine1 = GerberParseUtils.RemoveTrailingCommentsFromString(lineStr, ANSI349COMMENTDELIMITER).Trim();

             // Are we a INCH header Code? 
            if (tmpLine1.StartsWith("INCH") == true)
            {
                ExcellonLine_Misc mObj = new ExcellonLine_Misc(lineStr, tmpLine1, lineNumber);
                retInt = mObj.ParseLine(tmpLine1, StateMachine);
                if (retInt != 0)
                {
                    LogMessage("lineStr(INCH), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                    return 702;
                }
                // it is good, add it
                sourceLines.Add(mObj);
                // set the dimension mode now
                StateMachine.ExcellonFileUnits = ApplicationUnitsEnum.INCHES;
                return 0;
            }
            // Are we a METRIC header Code? 
            else if (tmpLine1.StartsWith("METRIC") == true)
            {
                ExcellonLine_Misc mObj = new ExcellonLine_Misc(lineStr, tmpLine1, lineNumber);
                retInt = mObj.ParseLine(tmpLine1, StateMachine);
                if (retInt != 0)
                {
                    LogMessage("lineStr(METRIC), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                    return 703;
                }
                // it is good, add it
                sourceLines.Add(mObj);
                // set the dimension mode now
                StateMachine.ExcellonFileUnits = ApplicationUnitsEnum.MILLIMETERS;
                return 0;
            }
            else if (tmpLine1.StartsWith("M") == true)
            {
                ExcellonLine_MCode mObj = new ExcellonLine_MCode(lineStr, tmpLine1, lineNumber);
                retInt = mObj.ParseLine(tmpLine1, StateMachine);
                if (retInt != 0)
                {
                    LogMessage("lineStr(M), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                    return 700;
                }
                // it is good, add it
                sourceLines.Add(mObj);
                // are we M72?
                if (mObj.CurrentMCode == 72)
                {
                    // set the dimension mode now
                    StateMachine.ExcellonFileUnits = ApplicationUnitsEnum.INCHES;
                }
                // are we M71?
                if (mObj.CurrentMCode == 71)
                {
                    // set the dimension mode now
                    StateMachine.ExcellonFileUnits = ApplicationUnitsEnum.MILLIMETERS;
                }
                return 0;
            }
            else if (tmpLine1.StartsWith("G") == true)
            {
                ExcellonLine_GCode gObj = new ExcellonLine_GCode(lineStr, tmpLine1, lineNumber);
                retInt = gObj.ParseLine(tmpLine1, StateMachine);
                if (retInt != 0)
                {
                    LogMessage("lineStr(G), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                    return 7001;
                }
                // it is good, add it
                sourceLines.Add(gObj);
                return 0;
            }
            else if (tmpLine1.StartsWith("ICI") == true)
            {
                // this is Incremental Input of Part Program Coordinates
                if (tmpLine1.Contains("OFF") == false)
                {
                    LogMessage("lIncremental coordinate mode not supported. Error on line " + lineNumber.ToString());
                    return 3001;
                }
                else
                {
                    // we just ignore this
                    ExcellonLine_Misc mObj = new ExcellonLine_Misc(lineStr, tmpLine1, lineNumber);
                    retInt = mObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(ICI), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 3002;
                    }
                    // it is good, add it
                    sourceLines.Add(mObj);
                    return 0;
                }
            }
            // Are we a T Code? 
            else if (tmpLine1.StartsWith("T") == true)
            {
                 // Are we a "TCST" flag
                if (tmpLine1.Contains("TCST") == true)
                {
                    // we just ignore this
                    ExcellonLine_Misc mObj = new ExcellonLine_Misc(lineStr, tmpLine1, lineNumber);
                    retInt = mObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(TCST), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 807;
                    }
                    // it is good, add it
                    sourceLines.Add(mObj);
                    return 0;
                }
                else
                {
                    // this is a tool change, have we seen the end of the header?
                    if (lineNumber > StateMachine.HeaderEndLine)
                    {
                        // yes, we have. This must be a tool change
                        ExcellonLine_ToolChange tObj = new ExcellonLine_ToolChange(lineStr, tmpLine1, lineNumber);
                        retInt = tObj.ParseLine(tmpLine1, StateMachine);
                        if (retInt != 0)
                        {
                            LogMessage("lineStr(ToolChange), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                            return 7011;
                        }
                        // it is good, add it
                        sourceLines.Add(tObj);
                    }
                    else
                    {
                        // no, we have not. This must be a tool table definition
                        ExcellonLine_ToolTable tObj = new ExcellonLine_ToolTable(lineStr, tmpLine1, lineNumber);
                        retInt = tObj.ParseLine(tmpLine1, StateMachine);
                        if (retInt != 0)
                        {
                            LogMessage("lineStr(ToolTable), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                            return 7012;
                        }
                        // it is good, add it
                        sourceLines.Add(tObj);
                        // add this to the tool collection
                        StateMachine.ToolCollection.Add(tObj);
                    }
                    return 0;
                }
            }
            // Are we a "%" header Stop Code? 
            else if (tmpLine1.StartsWith("%") == true)
            {
                ExcellonLine_Misc mObj = new ExcellonLine_Misc(lineStr, tmpLine1, lineNumber);
                retInt = mObj.ParseLine(tmpLine1, StateMachine);
                if (retInt != 0)
                {
                    LogMessage("lineStr(%), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                    return 704;
                }
                // it is good, add it
                sourceLines.Add(mObj);
                // note this now
                StateMachine.HeaderEndLine = lineNumber;
                return 0;
            }
            else if ((tmpLine1.StartsWith("X") || (tmpLine1.StartsWith("Y"))) == true)
            {
                ExcellonLine_XYCode xyObj = new ExcellonLine_XYCode(lineStr, tmpLine1, lineNumber);
                retInt = xyObj.ParseLine(tmpLine1, StateMachine);
                if (retInt != 0)
                {
                    LogMessage("lineStr(XY), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                    return 705;
                }
                // it is good, add it
                sourceLines.Add(xyObj);
                // record this
                RecordMinMaxXYCoords(xyObj.XCoord, xyObj.YCoord);
                return 0;
            }
            else if (tmpLine1.StartsWith("R") == true)
            {
                // several commands can start with R
                if ((tmpLine1.Contains("R,T") == true) ||
                    (tmpLine1.Contains("R,C") == true) ||
                    (tmpLine1.Contains("R,D") == true) ||
                    (tmpLine1.Contains("R,H") == true))
                {
                    // "R,T" just Reset Tool Data - ignore
                    // "R,C" just Reset clocks - ignore
                    // "R,CP" just Reset program clocks - ignore
                    // "R,CR" just Reset run clocks - ignore
                    // "R,D" just Reset All Cutter Distances - ignore
                    // "R,H" just RReset All Hit Counters - ignore
                    ExcellonLine_Misc mObj = new ExcellonLine_Misc(lineStr, tmpLine1, lineNumber);
                    retInt = mObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(R,), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 804;
                    }
                    // it is good, add it
                    sourceLines.Add(mObj);
                    return 0;
                }
                else
                {
                    // assume this
                    ExcellonLine_RCode rObj = new ExcellonLine_RCode(lineStr, tmpLine1, lineNumber);
                    retInt = rObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(R), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 706;
                    }
                    // it is good, add it
                    sourceLines.Add(rObj);
                }
                return 0;
            }
            // Are we a "VER" flag
            else if (tmpLine1.StartsWith("VER") == true)
            {
                // we just ignore this
                ExcellonLine_Misc mObj = new ExcellonLine_Misc(lineStr, tmpLine1, lineNumber);
                retInt = mObj.ParseLine(tmpLine1, StateMachine);
                if (retInt != 0)
                {
                    LogMessage("lineStr(ver), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                    return 805;
                }
                // it is good, add it
                sourceLines.Add(mObj);
                return 0;
            }
            // Are we a "FMAT" flag
            else if (tmpLine1.StartsWith("FMAT") == true)
            {
                // we just ignore this
                ExcellonLine_Misc mObj = new ExcellonLine_Misc(lineStr, tmpLine1, lineNumber);
                retInt = mObj.ParseLine(tmpLine1, StateMachine);
                if (retInt != 0)
                {
                    LogMessage("lineStr(FMAT), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                    return 806;
                }
                // it is good, add it
                sourceLines.Add(mObj);
                return 0;
            }
            // Are we a "ATC" flag
            else if (tmpLine1.StartsWith("ATC") == true)
            {
                // we just ignore this
                ExcellonLine_Misc mObj = new ExcellonLine_Misc(lineStr, tmpLine1, lineNumber);
                retInt = mObj.ParseLine(tmpLine1, StateMachine);
                if (retInt != 0)
                {
                    LogMessage("lineStr(ATC), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                    return 8071;
                }
                // it is good, add it
                sourceLines.Add(mObj);
                return 0;
            }
            else if (tmpLine1.Length == 0)
            {
                // probably all comments or blank
                ExcellonLine_Misc mObj = new ExcellonLine_Misc(lineStr, tmpLine1, lineNumber);
                retInt = mObj.ParseLine(tmpLine1, StateMachine);
                if (retInt != 0)
                {
                    LogMessage("lineStr(%), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                    return 704;
                }
                // it is good, add it
                sourceLines.Add(mObj);
                return 0;
            }
            else
            {
                // if we get here we are a line of unknown type
                LogMessage("Unknown Excellon line type >" + lineStr + "< on line " + lineNumber.ToString());
                if (lineStr.Length > 20)
                {
                    throw new NotImplementedException("Cannot cope with unknown Excellon code on line " + lineNumber.ToString() + " code is >" + lineStr.Substring(0, 20) + "<");
                }
                else
                {
                    throw new NotImplementedException("Cannot cope with unknown Excellon code on line " + lineNumber.ToString() + " code is >" + lineStr + "<");
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current X Coord Origin Adjust value. These are compensating
        /// values we apply to the current coordinates in order to make the
        /// smallest X coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public void SetPlotOriginCoordinateAdjustments(float xCoordAdjust, float yCoordAdjust)
        {
            LogMessage("Excellon Origin coord adjustments: xCoordAdjust=" + xCoordAdjust.ToString() + " yCoordAdjust=" + yCoordAdjust.ToString());
            // just run through and apply it to each ExcellonLine whether it uses it or not
            foreach (ExcellonLine gLine in SourceLines)
            {
                gLine.PlotXCoordOriginAdjust = xCoordAdjust;
                gLine.PlotYCoordOriginAdjust = yCoordAdjust;
            }
            PlotXCoordOriginAdjust = xCoordAdjust;
            PlotYCoordOriginAdjust = yCoordAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the excellon file options to use. Never gets/sets a null value
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public FileManager ExcellonFileManager
        {
            get
            {
                return StateMachine.ExcellonFileManager;
            }
            set
            {
                StateMachine.ExcellonFileManager = value;
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
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public void SetMaxPlotCoordinateAdjustments(float xMaxAdjust, float yMaxAdjust)
        {
            MaxDCodeXCoord = xMaxAdjust;
            MaxDCodeYCoord = yMaxAdjust;
        }
 
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Plots the Excellon file contents on the designated graphics object
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public void PlotExcellonFile(Graphics graphicsObj, float isoPlotPointsPerAppUnit)
        {
            // not supported yet, can only plot the converted GCode version
        }

    }
}
