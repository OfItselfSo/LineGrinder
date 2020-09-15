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
    /// A class to encapsulate a GCode file in an easily manipulable manner
    /// </summary>
    /// <history>
    ///    02 Aug 10  Cynic - Started
    /// </history>
    public class GCodeFile : OISObjBase
    {
        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value derived from the gerber file or gCode file although possibly
        //       it may be origin compensated

        // this is the current file source
        private List<GCodeLine> sourceLines = new List<GCodeLine>();

        // our state machine. Each line of a GCode File can assume many things based
        // on the state of the previously executed commands
        GCodeFileStateMachine stateMachine = new GCodeFileStateMachine();

        // indicates if the file has been saved
        private bool hasBeenSaved = true;

        // These are the values ADDed to the existing X and Y coords from the Gerber
        // file DCodes in order to set the origin approximately at zero. We SUBTRACT
        // them from the X and Y coordinates in order to set the origin back to 
        // the original position in the output GCode file. Note that these are only
        // for internal use, plotting and calculating. We have a different set
        // of adjustments, applied just before output for user specfied coordinate adjustments
        private float plotXCoordOriginAdjust = 0;
        private float plotYCoordOriginAdjust = 0;

        // these are used mostly in post processing to generate the BedFlattening code
        // these are origin compensated values
        private float gXMinValue = float.MaxValue;
        private float gYMinValue = float.MaxValue;
        private float gXMaxValue = float.MinValue;
        private float gYMaxValue = float.MinValue;
        // these are origin uncompensated values
        private float xMinValue = float.MaxValue;
        private float yMinValue = float.MaxValue;
        private float xMaxValue = float.MinValue;
        private float yMaxValue = float.MinValue;

        // this is an upper bound used when cutting pockets
        private int MAX_POCKETING_PASSES = 1000;

        // this is used for display only
        public const float TOUCHDOWN_HOLE_DISPLAY_DIAMETER = 0.02f;


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public GCodeFile()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gcode file options to use. Never gets/sets a null value
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        public FileManager GCodeFileManager
        {
            get
            {
                return StateMachine.GCodeFileManager;
            }
            set
            {
                StateMachine.GCodeFileManager = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Applies the current coordinate Origin Adjust value. These are the values 
        /// ADDed to the existing X and Y coords from the Gerber file DCodes in order
        /// to set the origin approximately at zero. We SUBTRACT them from the X and 
        /// Y coordinates in order to set the origin back to he original position in
        /// the output GCode file
        /// </summary>
        /// <param name="xCoordAdjust">x origin adjuster</param>
        /// <param name="yCoordAdjust">y origin adjuster</param>
        /// <history>
        ///    11 Aug 10  Cynic - Started
        /// </history>
        public void SetPlotOriginCoordinateAdjustments(float xCoordAdjust, float yCoordAdjust)
        {
            // just run through and apply it to each GerberLine whether it uses it or not
            foreach (GCodeLine gLine in SourceLines)
            {
                gLine.PlotXCoordOriginAdjust = xCoordAdjust;
                gLine.PlotYCoordOriginAdjust = yCoordAdjust;
            }
            PlotXCoordOriginAdjust = xCoordAdjust;
            PlotYCoordOriginAdjust = yCoordAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current X Coord Origin Adjust value. These are compensating
        /// values ADDED to the gerber coordinates in order to make the
        /// smallest X coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// We SUBTRACT this value from the GCode parameters in order to put them 
        /// back to where they were.
        /// </summary>
        /// <history>
        ///    11 Aug 10  Cynic - Started
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
        /// values ADDED to the gerber coordinates in order to make the
        /// smallest Y coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// We SUBTRACT this value from the GCode parameters in order to put them 
        /// back to where they were.
        /// </summary>
        /// <history>
        ///    11 Aug 10  Cynic - Started
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
        /// Gets/Sets the hasBeenSaved flag
        /// </summary>
        /// <param name="lineObj">the line object to add</param>
        /// <history>
        ///    06 Jul 10  Cynic - Started
        /// </history>
        public bool HasBeenSaved
        {
            get
            {
                return hasBeenSaved;
            }
            set
            {
                hasBeenSaved = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a GCodeLine object to the SourceLines
        /// </summary>
        /// <param name="lineObj">the line object to add</param>
        /// <history>
        ///    06 Jul 10  Cynic - Started
        /// </history>
        public int AddLine(GCodeLine lineObj)
        {
            if (lineObj == null) return 100;
            sourceLines.Add(lineObj);
            // also set this now
            lineObj.PlotXCoordOriginAdjust = PlotXCoordOriginAdjust;
            lineObj.PlotYCoordOriginAdjust = PlotYCoordOriginAdjust;
            return 0;
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
                else return false; ;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Completely resets this class
        /// </summary>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public void Reset()
        {
            stateMachine = new GCodeFileStateMachine();
            SourceLines = new List<GCodeLine>();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the GCode file source. Will never set or get a null value.
        /// </summary>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        [BrowsableAttribute(false)]
        public List<GCodeLine> SourceLines
        {
            get
            {
                if (sourceLines == null) sourceLines = new List<GCodeLine>();
                return sourceLines;
            }
            set
            {
                sourceLines = value;
                if (sourceLines == null) sourceLines = new List<GCodeLine>();
                LogMessage("SourceLines Set, lines =" + sourceLines.Count.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns all lines as a StringBuilder. Will never get a null value.
        /// </summary>
        /// <param name="toolHeadSettingsIn">the Tool Head Parameters (depth, xy speed etc)</param>
        /// <returns>A string builder containing the properly formatted lines</returns>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        ///    05 Sep 10  Cynic - Now passing in the toolHeadSettings
        /// </history>
        public StringBuilder GetGCodeLinesAsText(ToolHeadParameters toolHeadSettingsIn)
        {
            ToolHeadParameters originalSettings=null;
            StringBuilder sb = new StringBuilder();
            
            // remember this
            originalSettings = StateMachine.ToolHeadSetup;
            try
            {
                // if toolHeadSettingsIn == null this will auto reset to default values
                StateMachine.ToolHeadSetup = toolHeadSettingsIn;
                // get all of the lines into the list
                foreach (GCodeLine gLineObj in SourceLines)
                {
                    sb.Append(gLineObj.GetGCodeLine(StateMachine));
                }
            }
            finally
            {
                // restore this
                StateMachine.ToolHeadSetup = originalSettings;
            }

            return sb;
        }
 
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the gXYCompensatedMax/Min values to their uncompensated equivalents
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        public void ConvertXYCompensatedMaxMinToUncompensated()
        {
            if (AreXYCompensatedMaxMinOk() == false) return;
            xMinValue = gXMinValue + this.PlotXCoordOriginAdjust;
            yMinValue = gYMinValue + this.PlotYCoordOriginAdjust;
            xMaxValue = gXMaxValue + this.PlotXCoordOriginAdjust;
            yMaxValue = gYMaxValue + this.PlotYCoordOriginAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the gXYCompensatedMax/Min values
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        public void SetXYCompensatedMaxMin()
        {
            // reset these
            gXMinValue = float.MaxValue;
            gYMinValue = float.MaxValue;
            gXMaxValue = float.MinValue;
            gYMaxValue = float.MinValue;

            // run through all the lines checking as we go
            foreach (GCodeLine gLineObj in SourceLines)
            {
                if ((gLineObj is GCodeLine_Line) == true)
                {
                    GCodeLine_Line tmpLine = (GCodeLine_Line)gLineObj;
                    // check mins
                    if ((tmpLine.gX0OffsetCompensated) < gXMinValue) gXMinValue = tmpLine.gX0OffsetCompensated;
                    if ((tmpLine.gY0OffsetCompensated) < gYMinValue) gYMinValue = tmpLine.gY0OffsetCompensated;
                    if ((tmpLine.gX1OffsetCompensated) < gXMinValue) gXMinValue = tmpLine.gX1OffsetCompensated;
                    if ((tmpLine.gY1OffsetCompensated) < gYMinValue) gYMinValue = tmpLine.gY1OffsetCompensated;
                    // max values
                    if ((tmpLine.gX0OffsetCompensated) > gXMaxValue) gXMaxValue = tmpLine.gX0OffsetCompensated;
                    if ((tmpLine.gY0OffsetCompensated) > gYMaxValue) gYMaxValue = tmpLine.gY0OffsetCompensated;
                    if ((tmpLine.gX1OffsetCompensated) > gXMaxValue) gXMaxValue = tmpLine.gX1OffsetCompensated;
                    if ((tmpLine.gY1OffsetCompensated) > gYMaxValue) gYMaxValue = tmpLine.gY1OffsetCompensated;
                }
                else if ((gLineObj is GCodeLine_Arc) == true)
                {
                    GCodeLine_Arc tmpLine = (GCodeLine_Arc)gLineObj;
                    // check mins
                    if ((tmpLine.gX0OffsetCompensated) < gXMinValue) gXMinValue = tmpLine.gX0OffsetCompensated;
                    if ((tmpLine.gY0OffsetCompensated) < gYMinValue) gYMinValue = tmpLine.gY0OffsetCompensated;
                    if ((tmpLine.gX1OffsetCompensated) < gXMinValue) gXMinValue = tmpLine.gX1OffsetCompensated;
                    if ((tmpLine.gY1OffsetCompensated) < gYMinValue) gYMinValue = tmpLine.gY1OffsetCompensated;
                    // max values
                    if ((tmpLine.gX0OffsetCompensated) > gXMaxValue) gXMaxValue = tmpLine.gX0OffsetCompensated;
                    if ((tmpLine.gY0OffsetCompensated) > gYMaxValue) gYMaxValue = tmpLine.gY0OffsetCompensated;
                    if ((tmpLine.gX1OffsetCompensated) > gXMaxValue) gXMaxValue = tmpLine.gX1OffsetCompensated;
                    if ((tmpLine.gY1OffsetCompensated) > gYMaxValue) gYMaxValue = tmpLine.gY1OffsetCompensated;
                }
                else if ((gLineObj is GCodeLine_RapidMove) == true)
                {
/*
                    GCodeLine_RapidMove tmpLine = (GCodeLine_RapidMove)gLineObj;
                    // check mins
                    if ((tmpLine.gX0OffsetCompensated) < gXMinValue) gXMinValue = tmpLine.gX0OffsetCompensated;
                    if ((tmpLine.gY0OffsetCompensated) < gYMinValue) gYMinValue = tmpLine.gY0OffsetCompensated;
                    // max values
                    if ((tmpLine.gX0OffsetCompensated) > gXMaxValue) gXMaxValue = tmpLine.gX0OffsetCompensated;
                    if ((tmpLine.gY0OffsetCompensated) > gYMaxValue) gYMaxValue = tmpLine.gY0OffsetCompensated;
 */ 
                }
            }
            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// tests the gXYCompensatedMax/Min values to see if they are not at defaults
        /// </summary>
        /// <returns>true all is well, false one or more at defaults</returns>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        public bool AreXYCompensatedMaxMinOk()
        {
            // test these
            if (gXMinValue == float.MaxValue) return false;
            if (gYMinValue == float.MaxValue) return false;
            if (gXMaxValue == float.MinValue) return false;
            if (gYMaxValue == float.MinValue) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// tests the gXYCompensatedMax/Min values to see if they enclose an area
        /// </summary>
        /// <returns>true all is well, false one or more at defaults</returns>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        public bool DoXYCompensatedMaxMinEncloseAnArea()
        {
            if (AreXYCompensatedMaxMinOk() == false) return false;
            // test these
            if (gXMinValue == gXMaxValue) return false;
            if (gXMaxValue == gYMaxValue) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the minimum X value used in the gcode plot. A prior call to 
        ///  SetXYCompensatedMaxMin() must have been made to set this value.
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        public float GXMinValue
        {
            get
            {
                return gXMinValue;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the minimum Y value used in the gcode plot. A prior call to 
        ///  SetXYCompensatedMaxMin() must have been made to set this value.
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        public float GYMinValue
        {
            get
            {
                return gYMinValue;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the maximum X value used in the gcode plot. A prior call to 
        ///  SetXYCompensatedMaxMin() must have been made to set this value.
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        public float GXMaxValue
        {
            get
            {
                return gXMaxValue;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the maximum Y value used in the gcode plot. A prior call to 
        ///  SetXYCompensatedMaxMin() must have been made to set this value.
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        public float GYMaxValue
        {
            get
            {
                return gYMaxValue;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// tests the xYMax/Min values to see if they are not at defaults
        /// </summary>
        /// <returns>true all is well, false one or more at defaults</returns>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        public bool AreXYMaxMinOk()
        {
            // test these
            if (xMinValue == float.MaxValue) return false;
            if (yMinValue == float.MaxValue) return false;
            if (xMaxValue == float.MinValue) return false;
            if (yMaxValue == float.MinValue) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// tests the xY Max/Min values to see if they enclose an area
        /// </summary>
        /// <returns>true all is well, false one or more at defaults</returns>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        public bool DoXYMaxMinEncloseAnArea()
        {
            if (AreXYMaxMinOk() == false) return false;
            // test these
            if (xMinValue == xMaxValue) return false;
            if (xMaxValue == yMaxValue) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the minimum X value used in the gcode plot. A prior call to 
        ///  SetXYMaxMin() must have been made to set this value.
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        public float XMinValue
        {
            get
            {
                return xMinValue;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the minimum Y value used in the gcode plot. A prior call to 
        ///  SetXYMaxMin() must have been made to set this value.
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        public float YMinValue
        {
            get
            {
                return yMinValue;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the maximum X value used in the gcode plot. A prior call to 
        ///  SetXYMaxMin() must have been made to set this value.
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        public float XMaxValue
        {
            get
            {
                return xMaxValue;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the maximum Y value used in the gcode plot. A prior call to 
        ///  SetXYMaxMin() must have been made to set this value.
        /// </summary>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        /// </history>
        public float YMaxValue
        {
            get
            {
                return yMaxValue;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets state machine. Will never set or get a null value.
        /// </summary>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public GCodeFileStateMachine StateMachine
        {
            get
            {
                if (stateMachine == null) stateMachine = new GCodeFileStateMachine();
                return stateMachine;
            }
            set
            {
                stateMachine = value;
                if (stateMachine == null) stateMachine = new GCodeFileStateMachine();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Virtual Coords Per Unit
        /// </summary>
        /// <history>
        ///    13 Jul 10  Cynic - Started
        /// </history>
        public float IsoPlotPointsPerAppUnit
        {
            get
            {
                return StateMachine.IsoPlotPointsPerAppUnit;
            }
            set
            {
                StateMachine.IsoPlotPointsPerAppUnit = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Plots the GCode file contents on the designated graphics object
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="wantEndPointMarkers">if true we draw the endpoints of the gcodes
        /// in a different color</param>
        /// <param name="isoPlotPointsPerAppUnit">the virtual coordinates per unit</param>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public void PlotGCodeFile(Graphics graphicsObj, float isoPlotPointsPerAppUnit, bool wantEndPointMarkers)
        {
            int errInt = 0;
            string errStr = "";
            GCodeLine.PlotActionEnum errAction = GCodeLine.PlotActionEnum.PlotAction_End;

            // set the StateMachine
            StateMachine.ResetForPlot();
            StateMachine.IsoPlotPointsPerAppUnit = isoPlotPointsPerAppUnit;
            int penWidth = (int)Math.Round((StateMachine.IsolationWidth * isoPlotPointsPerAppUnit));

            // set up our pens+brushes
            StateMachine.PlotBorderPen = new Pen(StateMachine.PlotLineColor, penWidth);
           // StateMachine.PlotBorderPen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;

            foreach (GCodeLine lineObj in SourceLines)
            {
                errAction = lineObj.PerformPlotGCodeAction(graphicsObj, StateMachine, wantEndPointMarkers, ref errInt, ref errStr);
                if (errAction == GCodeLine.PlotActionEnum.PlotAction_Continue)
                {
                    // all is well
                    continue;
                }
                if (errAction == GCodeLine.PlotActionEnum.PlotAction_End)
                {
                    // we are all done
                    return;
                }
                else if (errAction == GCodeLine.PlotActionEnum.PlotAction_FailWithError)
                {
                    // handle this error
                    LogMessage("Plot Failed on obj:" + lineObj.ToString());
                    continue; // temp for testing
                    /// tempreturn;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Generates the gcode to mill a pocket in the surface. We use this for
        /// BedFlattening
        /// </summary>
        /// <remarks>the gcode file is assumed to have been populated with the standard headers.
        /// We just add our lines onto the end.</remarks>
        /// <param name="lX">low x coord</param>
        /// <param name="lY">low y coord</param>
        /// <param name="hX">high x coord</param>
        /// <param name="hY">high y coord</param>
        /// <param name="millWidth">the diameter of the mill doing the pocketing</param>
        /// <param name="overlapScaleFactor">the amount of overlap we require. Expressed as decimal fraction. 0.25 =25%</param>
        /// <param name="errStr">the error string</param>
        /// <history>
        ///    25 Aug 10  Cynic - Started
        /// </history>
        public int GeneratePocketGCode(float lX, float lY, float hX, float hY, float millWidth, float overlapScaleFactor, ref string errStr)
        {
            int i = 0;
            GCodeLine_ZMove zLine = null;
            GCodeLine_RapidMove rmLine = null;
            GCodeLine_Line gcLine = null;
            GCodeLine_Comment coLine = null;
            errStr = "";
            float centerX;
            float centerY;
            float lXCutCoord;
            float lYCutCoord;
            float hXCutCoord;
            float hYCutCoord;
            float lastXCoord;
            float lastYCoord;

            // test these
            if ((lX == float.MaxValue) || 
                (lY == float.MaxValue) ||
                (hX == float.MinValue) ||
                (hY == float.MinValue))
            {
                LogMessage("GeneratePocketGCode: One or more of the lX,lY,hXor hY coordinates are invalid.");
                errStr="The X and Y coordinates of the pocket rectangle are invalid.";
                return 1024;
            }
            // do we enclose an area?
            if ((lX == hX) || (lY == hY))
            {
                LogMessage("GeneratePocketGCode: The lX,lY,hXor hY coordinates do not enclose an area.");
                errStr = "The X and Y coordinates of the pocket rectangle do not enclose an area.";
                return 1025;
            }
            // are we inverted
            if ((lX > hX) || (lY > hY))
            {
                LogMessage("GeneratePocketGCode: The lX,lY,hXor hY coordinates are inverted.");
                errStr = "The X and Y coordinates of the pocket rectangle are inverted.";
                return 1026;
            }
            // more checks
            if (millWidth < 0)
            {
                LogMessage("GeneratePocketGCode: The millWidth is invalid.");
                errStr = "The millWidth is invalid.";
                return 1027;
            }
            // more checks
            if ((overlapScaleFactor > 1) || (overlapScaleFactor < 0))
            {
                LogMessage("GeneratePocketGCode: The overlapScaleFactor is invalid.");
                errStr = "The overlapScaleFactor is invalid.";
                return 1028;
            }
            if (StateMachine.CurrentZFeedrate <= 0)
            {
                LogMessage("GeneratePocketGCode: The zFeedRate is invalid.");
                errStr = "The zFeedRate is invalid.";
                return 1029;
            }
            if (StateMachine.CurrentXYFeedrate <= 0)
            {
                LogMessage("GeneratePocketGCode: The xyFeedRate is invalid.");
                errStr = "The xyFeedRate is invalid.";
                return 1030;
            }
            if (StateMachine.ZCoordForCut > StateMachine.ZCoordForClear)
            {
                LogMessage("GeneratePocketGCode: The zCutLevel > zClearLevel. This cannot be correct.");
                errStr = "The zCutLevel > zClearLevel. This cannot be correct.";
                return 1031;
            }
            // test to see if the pocket can be cut with this mill
            if ((millWidth >= (hX - lX)) || (millWidth >= (hY - lY)))
            {
                LogMessage("GeneratePocketGCode: The mill diameter is bigger than the pocket area.");
                errStr = "The mill diameter is bigger than the pocket area. Pocket cannot be cut with this mill.";
                return 1031;
            }

            // calculate the center point
            centerX = ((hX - lX) / 2f)+lX;
            centerY = ((hY - lY) / 2f)+lY;

            // the first offset distance is the millWidth, after that we adjust by
            float incrementalDistance = millWidth * overlapScaleFactor;

            coLine = new GCodeLine_Comment("... start ...");
            this.AddLine(coLine);

            // figure out the new corner coordinates - compensating for milling
            // bit diameter
            lXCutCoord = lX + (millWidth / 2);
            lYCutCoord = lY + (millWidth / 2);
            hXCutCoord = hX - (millWidth / 2);
            hYCutCoord = hY - (millWidth / 2);

            // G00 rapid move tool head to the destX,destY
            rmLine = new GCodeLine_RapidMove(hXCutCoord, hYCutCoord);
            this.AddLine(rmLine);

            // G01 - put the bit into the work piece
            zLine = new GCodeLine_ZMove(GCodeLine_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForCut);
            zLine.WantLinearMove = true;
            this.AddLine(zLine);

            // do the vertical down leg 
            gcLine = new GCodeLine_Line(hXCutCoord, hYCutCoord, hXCutCoord, lYCutCoord);
            this.AddLine(gcLine);

            // do the low horizontal leg 
            gcLine = new GCodeLine_Line(hXCutCoord, lYCutCoord, lXCutCoord, lYCutCoord);
            this.AddLine(gcLine);

            // do the vertical up leg 
            gcLine = new GCodeLine_Line(lXCutCoord, lYCutCoord, lXCutCoord, hYCutCoord);
            this.AddLine(gcLine);

            // do the high horizontal leg 
            gcLine = new GCodeLine_Line(lXCutCoord, hYCutCoord, hXCutCoord, hYCutCoord);
            this.AddLine(gcLine);
            lastXCoord = hXCutCoord;
            lastYCoord = hYCutCoord;

            // now do the rest of the pocket passes. This is encoded as a for loop
            // because I do not like endless loops. MAX_POCKETING_PASSES should be
            // pretty high so that it is not reached unless the finish tests fail
            for (i = 0; i < MAX_POCKETING_PASSES; i++)
            {
                // figure out the new corner coordinates - compensating for milling
                // bit diameter
                lXCutCoord = lXCutCoord + incrementalDistance;
                lYCutCoord = lYCutCoord + incrementalDistance;
                hXCutCoord = hXCutCoord - incrementalDistance;
                hYCutCoord = hYCutCoord - incrementalDistance;

                // perform tests
                if ((lXCutCoord >= centerX) || (lYCutCoord >= centerY) || (hXCutCoord <= centerX) || (hYCutCoord <= centerY))
                {
                    // we are on the last cut - just figure out which is the longer dimension
                    // and run a single cut down that
                    if ((lX - lY) > (hX - hY))
                    {
                        // we have to move to the new start position
                        gcLine = new GCodeLine_Line(lastXCoord, lastYCoord, hXCutCoord, lYCutCoord);
                        this.AddLine(gcLine);
                        // vertical is the longer dimension, hold X constant, run down Y
                        gcLine = new GCodeLine_Line(hXCutCoord, hYCutCoord, hXCutCoord, lYCutCoord);
                        this.AddLine(gcLine);
                        lastXCoord = hXCutCoord;
                        lastYCoord = hYCutCoord;
                    }
                    else
                    {
                        // we have to move to the new start position
                        gcLine = new GCodeLine_Line(lastXCoord, lastYCoord, hXCutCoord, hYCutCoord);
                        this.AddLine(gcLine);
                        // horizontal is the longer dimension, hold Y constant, run down X
                        gcLine = new GCodeLine_Line(hXCutCoord, hYCutCoord, lXCutCoord, hYCutCoord);
                        this.AddLine(gcLine);
                        lastXCoord = hXCutCoord;
                        lastYCoord = hYCutCoord;
                    }
                    // leave now
                    break;
                }

                coLine = new GCodeLine_Comment("... pass ...");
                this.AddLine(coLine);

                // we have to move to the new start position
                gcLine = new GCodeLine_Line(lastXCoord, lastYCoord, hXCutCoord, hYCutCoord);
                this.AddLine(gcLine);

                // do the vertical down leg, this will also move it to (hXCutCoord, hYCutCoord)
                gcLine = new GCodeLine_Line(hXCutCoord, hYCutCoord, hXCutCoord, lYCutCoord);
                this.AddLine(gcLine);

                // do the low horizontal leg 
                gcLine = new GCodeLine_Line(hXCutCoord, lYCutCoord, lXCutCoord, lYCutCoord);
                this.AddLine(gcLine);

                // do the vertical up leg 
                gcLine = new GCodeLine_Line(lXCutCoord, lYCutCoord, lXCutCoord, hYCutCoord);
                this.AddLine(gcLine);

                // do the high horizontal leg 
                gcLine = new GCodeLine_Line(lXCutCoord, hYCutCoord, hXCutCoord, hYCutCoord);
                this.AddLine(gcLine);
                lastXCoord = hXCutCoord;
                lastYCoord = hYCutCoord;
            }

            // one last test
            if (i >= MAX_POCKETING_PASSES)
            {
                LogMessage("GeneratePocketGCode: The the maximum number of pocketing passes was reached.");
                errStr = "The the maximum number of pocketing passes was reached. The gcode file is not correct. Please see the logs.";
                return 1036;
            }
            coLine = new GCodeLine_Comment("... end ...");
            this.AddLine(coLine);

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Generates the gcode for a drill hole. Defaults to 
        /// GCodeLine_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForCut
        /// </summary>
        /// <remarks>the gcode file is assumed to have been populated with the standard headers.
        /// We just add our lines onto the end.</remarks>
        /// <param name="x0">x coord</param>
        /// <param name="y0">y coord</param>
        /// <param name="errStr">the error string</param>
        /// <param name="drillWidth">the drill width. This is mostly used for plotting</param>
        /// <history>
        ///    31 Aug 10  Cynic - Started
        ///    05 Sep 10  Cynic - Added drill width as a parameter
        /// </history>
        public int AddDrillCodeLines(float x0, float y0, float drillWidth, ref string errStr)
        {
            return AddDrillCodeLines(x0, y0, GCodeLine_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForCut, drillWidth, FileManager.DEFAULT_DRILLDWELL_TIME, ref errStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Generates the gcode for a drill hole
        /// </summary>
        /// <remarks>the gcode file is assumed to have been populated with the standard headers.
        /// We just add our lines onto the end.</remarks>
        /// <param name="x0">x coord</param>
        /// <param name="y0">y coord</param>
        /// <param name="cutLevel">the cut level we drill to</param>
        /// <param name="drillDwellTime">drill dwell time at bottom of hole</param>
        /// <param name="drillWidth">the drill width. This is mostly used for plotting</param>
        /// <param name="errStr">the error string</param>
        /// <history>
        ///    26 Aug 10  Cynic - Started
        ///    05 Sep 10  Cynic - Added drill width as a parameter
        /// </history>
        public int AddDrillCodeLines(float x0, float y0, GCodeLine_ZMove.GCodeZMoveHeightEnum cutLevel, float drillWidth, float drillDwellTime, ref string errStr)
        {
            GCodeLine_ZMove zLine = null;
            GCodeLine_RapidMove rmLine = null;
            GCodeLine_Dwell dwLine = null;
            errStr = "";

            // test these
            if ((x0 == float.MaxValue) ||
                (y0 == float.MaxValue) ||
                (x0 == float.MinValue) ||
                (y0 == float.MinValue))
            {
                LogMessage("AddDrillCodeLines: One or more of the x0,y0 coordinates are invalid.");
                errStr = "The X and Y coordinates of the drill target are invalid.";
                return 1024;
            }

            if (StateMachine.CurrentZFeedrate <= 0)
            {
                LogMessage("AddDrillCodeLines: The zFeedRate is invalid.");
                errStr = "The zFeedRate is invalid.";
                return 1029;
            }
            if (StateMachine.CurrentXYFeedrate <= 0)
            {
                LogMessage("AddDrillCodeLines: The xyFeedRate is invalid.");
                errStr = "The xyFeedRate is invalid.";
                return 1030;
            }

            // we do not display a non sensible value here
            if (drillWidth <= 0) drillWidth = 0.125f;

            // G00 rapid move tool head to the x0, y0
            rmLine = new GCodeLine_RapidMove(x0, y0);
            this.AddLine(rmLine);

            // G00 - put the bit into the work piece
            zLine = new GCodeLine_ZMove(cutLevel);
            zLine.SetGCodePlotDrillValues(x0, y0, drillWidth);
            zLine.WantLinearMove = true;
            this.AddLine(zLine);

            // dwell at the bottom 
            dwLine = new GCodeLine_Dwell(drillDwellTime);
            this.AddLine(dwLine);

            // G00 - pull the bit out of the work piece
            zLine = new GCodeLine_ZMove(GCodeLine_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForClear);
            zLine.SetGCodePlotDrillValues(x0, y0, drillWidth);
            this.AddLine(zLine);

            return 0;
        }

    }
}
