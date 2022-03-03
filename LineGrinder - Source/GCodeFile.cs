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
    public class GCodeFile : OISObjBase
    {
        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value derived from the gerber file or gCode file although possibly
        //       it may be origin compensated

        // this is the current file source
        private List<GCodeCmd> sourceLines = new List<GCodeCmd>();

        // our state machine. Each line of a GCode File can assume many things based
        // on the state of the previously executed commands
        GCodeFileStateMachine stateMachine = new GCodeFileStateMachine();

        // indicates if the file has been saved
        private bool hasBeenSaved = true;

         // these are origin compensated values
        private float xMinValue = float.MaxValue;
        private float yMinValue = float.MaxValue;
        private float xMaxValue = float.MinValue;
        private float yMaxValue = float.MinValue;

        // this is an upper bound used when cutting pockets
        private const int MAX_POCKETING_PASSES = 1000;


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GCodeFile()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gcode file options to use. Never gets/sets a null value
        /// </summary>
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
        /// Gets/Sets the hasBeenSaved flag
        /// </summary>
        /// <param name="lineObj">the line object to add</param>
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
        /// Adds a GCodeCmd object to the SourceLines
        /// </summary>
        /// <param name="lineObj">the line object to add</param>
        public int AddLine(GCodeCmd lineObj)
        {
            if (lineObj == null) return 100;
            sourceLines.Add(lineObj);
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets whether the file is populated. We assume it is populated if there
        /// is at least one source line
        /// </summary>
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
        public void Reset()
        {
            stateMachine = new GCodeFileStateMachine();
            SourceLines = new List<GCodeCmd>();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the GCode file source. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        public List<GCodeCmd> SourceLines
        {
            get
            {
                if (sourceLines == null) sourceLines = new List<GCodeCmd>();
                return sourceLines;
            }
            set
            {
                sourceLines = value;
                if (sourceLines == null) sourceLines = new List<GCodeCmd>();
                LogMessage("SourceLines Set, lines =" + sourceLines.Count.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns all lines as a StringBuilder. Will never get a null value.
        /// </summary>
        /// <param name="toolHeadSettingsIn">the Tool Head Parameters (depth, xy speed etc)</param>
        /// <returns>A string builder containing the properly formatted lines</returns>
        public StringBuilder GetGCodeCmdsAsText()
        {
            StringBuilder sb = new StringBuilder();

            // get all of the lines into the list
            foreach (GCodeCmd gLineObj in SourceLines)
            {
                sb.Append(gLineObj.GetGCodeCmd(StateMachine));
            }

            return sb;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the mirrorOnConversionToGCode flag
        /// This is is what indicates if we should output GCode mirror flipped around 
        /// the vertical center axis
        /// </summary>
        public IsoFlipModeEnum MirrorOnConversionToGCode
        {
            get
            {
                return StateMachine.MirrorOnConversionToGCode;
            }
            set
            {
                StateMachine.MirrorOnConversionToGCode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gCodeMirrorAxisPlotCoord_X value
        /// </summary>
        public float GCodeMirrorAxisPlotCoord_X
        {
            get
            {
                return StateMachine.GCodeMirrorAxisPlotCoord_X;
            }
            set
            {
                StateMachine.GCodeMirrorAxisPlotCoord_X = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the GCodeOutputPlotOriginAdjust_X value
        /// </summary>
        public float GCodeOutputPlotOriginAdjust_X
        {
            get
            {
                return StateMachine.GCodeOutputPlotOriginAdjust_X;
            }
            set
            {
                StateMachine.GCodeOutputPlotOriginAdjust_X = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the GCodeOutputPlotOriginAdjust_X value
        /// </summary>
        public float GCodeOutputPlotOriginAdjust_Y
        {
            get
            {
                return StateMachine.GCodeOutputPlotOriginAdjust_Y;
            }
            set
            {
                StateMachine.GCodeOutputPlotOriginAdjust_Y = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the absoluteOffset_X used in the gcode plot. 
        /// </summary>
        public float AbsoluteOffset_X
        {
            get
            {
                return StateMachine.AbsoluteOffset_X;
            }
            set
            {
                StateMachine.AbsoluteOffset_X = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the absoluteOffset_Y used in the gcode plot. 
        /// </summary>
        public float AbsoluteOffset_Y
        {
            get
            {
                return StateMachine.AbsoluteOffset_Y;
            }
            set
            {
                StateMachine.AbsoluteOffset_Y = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// tests the gXYCompensatedMax/Min values to see if they are not at defaults
        /// </summary>
        /// <returns>true all is well, false one or more at defaults</returns>
        public bool AreXYCompensatedMaxMinOk()
        {
            // test these
            if (XMinValue == float.MaxValue) return false;
            if (YMinValue == float.MaxValue) return false;
            if (XMaxValue == float.MinValue) return false;
            if (YMaxValue == float.MinValue) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// tests the gXYCompensatedMax/Min values to see if they enclose an area
        /// </summary>
        /// <returns>true all is well, false one or more at defaults</returns>
        public bool DoXYCompensatedMaxMinEncloseAnArea()
        {
            if (AreXYCompensatedMaxMinOk() == false) return false;
            // test these
            if (XMinValue == XMaxValue) return false;
            if (XMaxValue == YMaxValue) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// tests the xYMax/Min values to see if they are not at defaults
        /// </summary>
        /// <returns>true all is well, false one or more at defaults</returns>
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
        /// Gets the minimum X value used in the gcode plot. 
        /// </summary>
        public float XMinValue
        {
            get
            {
                return xMinValue;
            }
            set
            {
                xMinValue = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the minimum Y value used in the gcode plot. 
        /// </summary>
        public float YMinValue
        {
            get
            {
                return yMinValue;
            }
            set
            {
                yMinValue = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the maximum X value used in the gcode plot. 
        /// </summary>
        public float XMaxValue
        {
            get
            {
                return xMaxValue;
            }
            set
            {
                xMaxValue = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the maximum Y value used in the gcode plot. 
        /// </summary>
        public float YMaxValue
        {
            get
            {
                return yMaxValue;
            }
            set
            {
                yMaxValue = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets state machine. Will never set or get a null value.
        /// </summary>
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
        public void PlotGCodeFile(Graphics graphicsObj, bool wantEndPointMarkers)
        {
            int errInt = 0;
            string errStr = "";
            PlotActionEnum errAction = PlotActionEnum.PlotAction_End;

            // set the StateMachine
            StateMachine.ResetForPlot();
            int penWidth = (int)Math.Round((StateMachine.IsolationWidth * StateMachine.IsoPlotPointsPerAppUnit));

            // set up our pens+brushes
            StateMachine.PlotBorderPen = new Pen(StateMachine.PlotLineColor, penWidth);
            // StateMachine.PlotBorderPen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;

            foreach (GCodeCmd lineObj in SourceLines)
            {
                errAction = lineObj.PerformPlotGCodeAction(graphicsObj, StateMachine, wantEndPointMarkers, ref errInt, ref errStr);
                if (errAction == PlotActionEnum.PlotAction_Continue)
                {
                    // all is well
                    continue;
                }
                if (errAction == PlotActionEnum.PlotAction_End)
                {
                    // we are all done
                    return;
                }
                else if (errAction == PlotActionEnum.PlotAction_FailWithError)
                {
                    // handle this error
                    LogMessage("Plot Failed on obj:" + lineObj.ToString());
                    return;
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
        public int GeneratePocketGCode(float isoPlotPointsPerAppUnit, float lX, float lY, float hX, float hY, float millWidth, float overlapScaleFactor, ref string errStr)
        {
            int i = 0;
            GCodeCmd_ZMove zLine = null;
            GCodeCmd_RapidMove rmLine = null;
            GCodeCmd_Line gcLine = null;
            GCodeCmd_Comment coLine = null;
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
            if(isoPlotPointsPerAppUnit<=0)
            {
                LogMessage("GeneratePocketGCode: isoPlotPointsPerAppUnit<=0");
                errStr = "isoPlotPointsPerAppUnit is invalid.";
                return 1023;
            }

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
            centerX = (((hX - lX) / 2f)+lX) * isoPlotPointsPerAppUnit;
            centerY = (((hY - lY) / 2f)+lY) * isoPlotPointsPerAppUnit;

            // the first offset distance is the millWidth, after that we adjust by
            float incrementalDistance = (millWidth * overlapScaleFactor) * isoPlotPointsPerAppUnit;

            coLine = new GCodeCmd_Comment("... start ...");
            this.AddLine(coLine);

            // figure out the new corner coordinates - compensating for milling
            // bit diameter
            lXCutCoord = (lX + (millWidth / 2)) * isoPlotPointsPerAppUnit;
            lYCutCoord = (lY + (millWidth / 2)) * isoPlotPointsPerAppUnit;
            hXCutCoord = (hX - (millWidth / 2)) * isoPlotPointsPerAppUnit;
            hYCutCoord = (hY - (millWidth / 2)) * isoPlotPointsPerAppUnit;

            // G00 rapid move tool head to the destX,destY
            rmLine = new GCodeCmd_RapidMove((int)hXCutCoord, (int)hYCutCoord);
            this.AddLine(rmLine);

            // G01 - put the bit into the work piece
            zLine = new GCodeCmd_ZMove(GCodeCmd_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForCut);
            zLine.WantLinearMove = true;
            this.AddLine(zLine);

            // do the vertical down leg 
            gcLine = new GCodeCmd_Line((int)hXCutCoord, (int)hYCutCoord, (int)hXCutCoord, (int)lYCutCoord);
            this.AddLine(gcLine);

            // do the low horizontal leg 
            gcLine = new GCodeCmd_Line((int)hXCutCoord, (int)lYCutCoord, (int)lXCutCoord, (int)lYCutCoord);
            this.AddLine(gcLine);

            // do the vertical up leg 
            gcLine = new GCodeCmd_Line((int)lXCutCoord, (int)lYCutCoord, (int)lXCutCoord, (int)hYCutCoord);
            this.AddLine(gcLine);

            // do the high horizontal leg 
            gcLine = new GCodeCmd_Line((int)lXCutCoord, (int)hYCutCoord, (int)hXCutCoord, (int)hYCutCoord);
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
                        gcLine = new GCodeCmd_Line((int)lastXCoord, (int)lastYCoord, (int)hXCutCoord, (int)lYCutCoord);
                        this.AddLine(gcLine);
                        // vertical is the longer dimension, hold X constant, run down Y
                        gcLine = new GCodeCmd_Line((int)hXCutCoord, (int)hYCutCoord, (int)hXCutCoord, (int)lYCutCoord);
                        this.AddLine(gcLine);
                        lastXCoord = hXCutCoord;
                        lastYCoord = hYCutCoord;
                    }
                    else
                    {
                        // we have to move to the new start position
                        gcLine = new GCodeCmd_Line((int)lastXCoord, (int)lastYCoord, (int)hXCutCoord, (int)hYCutCoord);
                        this.AddLine(gcLine);
                        // horizontal is the longer dimension, hold Y constant, run down X
                        gcLine = new GCodeCmd_Line((int)hXCutCoord, (int)hYCutCoord, (int)lXCutCoord, (int)hYCutCoord);
                        this.AddLine(gcLine);
                        lastXCoord = hXCutCoord;
                        lastYCoord = hYCutCoord;
                    }
                    // leave now
                    break;
                }

                coLine = new GCodeCmd_Comment("... pass ...");
                this.AddLine(coLine);

                // we have to move to the new start position
                gcLine = new GCodeCmd_Line((int)lastXCoord, (int)lastYCoord, (int)hXCutCoord, (int)hYCutCoord);
                this.AddLine(gcLine);

                // do the vertical down leg, this will also move it to (hXCutCoord, hYCutCoord)
                gcLine = new GCodeCmd_Line((int)hXCutCoord, (int)hYCutCoord, (int)hXCutCoord, (int)lYCutCoord);
                this.AddLine(gcLine);

                // do the low horizontal leg 
                gcLine = new GCodeCmd_Line((int)hXCutCoord, (int)lYCutCoord, (int)lXCutCoord, (int)lYCutCoord);
                this.AddLine(gcLine);

                // do the vertical up leg 
                gcLine = new GCodeCmd_Line((int)lXCutCoord, (int)lYCutCoord, (int)lXCutCoord, (int)hYCutCoord);
                this.AddLine(gcLine);

                // do the high horizontal leg 
                gcLine = new GCodeCmd_Line((int)lXCutCoord, (int)hYCutCoord, (int)hXCutCoord, (int)hYCutCoord);
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
            coLine = new GCodeCmd_Comment("... end ...");
            this.AddLine(coLine);

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Generates the gcode for a pad touchdown or drill. Defaults to 
        /// GCodeCmd_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForCut
        /// </summary>
        /// <remarks>the gcode file is assumed to have been populated with the standard headers.
        /// We just add our lines onto the end.</remarks>
        /// <param name="x0">x coord</param>
        /// <param name="y0">y coord</param>
        /// <param name="errStr">the error string</param>
        /// <param name="drillWidth">the drill width. This is mostly used for plotting</param>
        public int AddPadTouchDownOrDrillLine(int x0, int y0, float drillWidth, ref string errStr)
        {
            return AddPadTouchDownOrDrillLine(x0, y0, GCodeCmd_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForCut, drillWidth, FileManager.DEFAULT_DRILLDWELL_TIME, ref errStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Generates the gcode for a pad touchdown or drill
        /// </summary>
        /// <remarks>the gcode file is assumed to have been populated with the standard headers.
        /// We just add our lines onto the end.</remarks>
        /// <param name="x0">x coord</param>
        /// <param name="y0">y coord</param>
        /// <param name="cutLevel">the cut level we drill to</param>
        /// <param name="drillDwellTime">drill dwell time at bottom of hole</param>
        /// <param name="drillWidth">the drill width. This is mostly used for plotting</param>
        /// <param name="errStr">the error string</param>
        public int AddPadTouchDownOrDrillLine(int x0, int y0, GCodeCmd_ZMove.GCodeZMoveHeightEnum cutLevel, float drillWidth, float drillDwellTime, ref string errStr)
        {
            GCodeCmd_ZMove zLine = null;
            GCodeCmd_RapidMove rmLine = null;
            GCodeCmd_Dwell dwLine = null;
            errStr = "";

            // test these
            if ((x0 == int.MaxValue) ||
                (y0 == int.MaxValue) ||
                (x0 == int.MinValue) ||
                (y0 == int.MinValue))
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
            rmLine = new GCodeCmd_RapidMove(x0, y0);
            this.AddLine(rmLine);

            // G00 - put the bit into the work piece
            zLine = new GCodeCmd_ZMove(cutLevel);
            zLine.SetGCodePlotDrillValues(x0, y0, drillWidth);
            zLine.WantLinearMove = true;
            this.AddLine(zLine);

            // dwell at the bottom 
            dwLine = new GCodeCmd_Dwell(drillDwellTime);
            this.AddLine(dwLine);

            // G00 - pull the bit out of the work piece
            zLine = new GCodeCmd_ZMove(GCodeCmd_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForClear);
            zLine.SetGCodePlotDrillValues(x0, y0, drillWidth);
            this.AddLine(zLine);
            return 0;
        }

    }
}

