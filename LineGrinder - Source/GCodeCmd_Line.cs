using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
    /// A class to encapsulate a GCode line object
    /// </summary>
    public class GCodeCmd_Line : GCodeCmd
    {
        private int x0 = -1;
        private int y0 = -1;
        private int x1 = -1;
        private int y1 = -1;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0In">X0 coord</param>
        /// <param name="y0In">Y0 coord</param>
        /// <param name="x1In">X1 coord</param>
        /// <param name="y1In">Y1 coord</param>
        public GCodeCmd_Line(int x0In, int y0In, int x1In, int y1In) : base(0, 0)
        {
            x0 = x0In;
            y0 = y0In;
            x1 = x1In;
            y1 = y1In;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0In">X0 coord</param>
        /// <param name="y0In">Y0 coord</param>
        /// <param name="x1In">X1 coord</param>
        /// <param name="y1In">Y1 coord</param>
        /// <param name="isoPlotCellsInObjectIn">the number of isoplot cells used to build this object</param>
        /// <param name="debugIDIn">the debug Id</param>
        public GCodeCmd_Line(int x0In, int y0In, int x1In, int y1In, int isoPlotCellsInObjectIn, int debugIDIn) : base(isoPlotCellsInObjectIn, debugIDIn)
        {
            x0 = x0In;
            y0 = y0In;
            x1 = x1In;
            y1 = y1In;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides ToString()
        /// </summary>
        public override string ToString()
        {
            return "GCodeCmd_Line (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ") " ;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GCode lines for this object as they would be written to the text file. Will 
        /// never return a null value. Can often return more than one line if the 
        /// current context in the stateMachine requires it
        /// </summary>
        /// <param name="stateMachine">the stateMachine</param>
        public override string GetGCodeCmd(GCodeFileStateMachine stateMachine)
        {
            StringBuilder sb = new StringBuilder();

            if (stateMachine == null)
            {
                LogMessage("GetGCodeCmd: stateMachine == null");
                return "M5 ERROR: stateMachine==null";
            }

            if (DoNotEmitToGCode == true)
            {
                return sb.ToString();
            }

            float gX0OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_X(stateMachine, x0), 3);
            float gY0OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_Y(stateMachine, y0), 3);
            float gX1OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_X(stateMachine, x1), 3);
            float gY1OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_Y(stateMachine, y1), 3);

            float gX0OffsetCompensated_Rounded = (float)Math.Round(gX0OffsetCompensated_Raw, 3);
            float gY0OffsetCompensated_Rounded = (float)Math.Round(gY0OffsetCompensated_Raw, 3);
            float gX1OffsetCompensated_Rounded = (float)Math.Round(gX1OffsetCompensated_Raw, 3);
            float gY1OffsetCompensated_Rounded = (float)Math.Round(gY1OffsetCompensated_Raw, 3);

            // are we drawing this value in reverse?
            if(this.ReverseOnConversionToGCode==true)
            {
                float tmpX = gX0OffsetCompensated_Rounded;
                float tmpY = gY0OffsetCompensated_Rounded;
                gX0OffsetCompensated_Rounded = gX1OffsetCompensated_Rounded;
                gY0OffsetCompensated_Rounded = gY1OffsetCompensated_Rounded;
                gX1OffsetCompensated_Rounded = tmpX;
                gY1OffsetCompensated_Rounded = tmpY;
            }

            float gXLastGCodeCoord_Rounded = (float)Math.Round(stateMachine.LastGCodeXCoord, 3);
            float gYLastGCodeCoord_Rounded = (float)Math.Round(stateMachine.LastGCodeYCoord, 3);

            // are we currently on our start coord?
            if ((gX0OffsetCompensated_Rounded == gXLastGCodeCoord_Rounded) && (gY0OffsetCompensated_Rounded == gYLastGCodeCoord_Rounded))
            {
                // yes we are, we do not need to move the tool head there. Is our Z Axis correctly set?
                if (Math.Round(stateMachine.LastGCodeZCoord, 3) != Math.Round(stateMachine.ZCoordForCut, 3))
                {
                    // no it is not, send it down to to the CUT depth
                    if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                    sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_ZAXIS + ConvertCoordToDesiredUnitSystem(stateMachine.ZCoordForCut, stateMachine.SourceUnits, stateMachine.OutputUnits).ToString());
                    stateMachine.LastGCodeZCoord = stateMachine.ZCoordForCut;
                    // do we need to adjust the feedrate?
                    if (stateMachine.LastFeedRate != stateMachine.CurrentZFeedrate)
                    {
                        // yes we do 
                        sb.Append(" " + GCODEWORD_FEEDRATE + ConvertCoordToDesiredUnitSystem(stateMachine.CurrentZFeedrate, stateMachine.SourceUnits, stateMachine.OutputUnits).ToString());
                        // remember this now
                        stateMachine.LastFeedRate = stateMachine.CurrentZFeedrate;
                    }
                    sb.Append(stateMachine.LineTerminator);
                }

                // now move the tool head, cutting as we go
                if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_XAXIS + gX1OffsetCompensated_Rounded.ToString() + " " + GCODEWORD_YAXIS + gY1OffsetCompensated_Rounded.ToString());
                stateMachine.LastGCodeXCoord = gX1OffsetCompensated_Rounded;
                stateMachine.LastGCodeYCoord = gY1OffsetCompensated_Rounded;
                // do we need to adjust the feedrate?
                if (stateMachine.LastFeedRate != stateMachine.CurrentXYFeedrate)
                {
                    // yes we do 
                    sb.Append(" " + GCODEWORD_FEEDRATE+ ConvertCoordToDesiredUnitSystem(stateMachine.CurrentXYFeedrate, stateMachine.SourceUnits, stateMachine.OutputUnits).ToString());
                    // remember this now
                    stateMachine.LastFeedRate = stateMachine.CurrentXYFeedrate;
                }
                sb.Append(stateMachine.LineTerminator);
            }
            else
            {
                // no, we are not on the start coord. Is our Z Axis correctly set?
                if ((((Math.Round(stateMachine.LastGCodeZCoord, 3) == Math.Round(stateMachine.ZCoordForMove, 3)) || (Math.Round(stateMachine.LastGCodeZCoord, 3) == Math.Round(stateMachine.ZCoordForClear, 3)))) == false)
                {
                    // no it is not, pull it up to the MOVE distance
                    if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                    sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_ZAXIS + ConvertCoordToDesiredUnitSystem(stateMachine.ZCoordForMove, stateMachine.SourceUnits, stateMachine.OutputUnits).ToString() );
                    stateMachine.LastGCodeZCoord = stateMachine.ZCoordForMove;
                    // do we need to adjust the feedrate?
                    if (stateMachine.LastFeedRate != stateMachine.CurrentZFeedrate)
                    {
                        // yes we do 
                        sb.Append(" " + GCODEWORD_FEEDRATE + ConvertCoordToDesiredUnitSystem(stateMachine.CurrentZFeedrate, stateMachine.SourceUnits, stateMachine.OutputUnits).ToString());
                        // remember this now
                        stateMachine.LastFeedRate = stateMachine.CurrentZFeedrate;
                    }
                    sb.Append(stateMachine.LineTerminator);
                }

                // Now move quickly to the start, we know our Z is now be ok and above the pcb
                if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVERAPID + " " + GCODEWORD_XAXIS + gX0OffsetCompensated_Rounded.ToString() + " " + GCODEWORD_YAXIS + gY0OffsetCompensated_Rounded.ToString());
                stateMachine.LastGCodeXCoord = gX0OffsetCompensated_Rounded;
                stateMachine.LastGCodeYCoord = gY0OffsetCompensated_Rounded;
                sb.Append(stateMachine.LineTerminator);
          
                // now put our Z down to the cut depth
                if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_ZAXIS + ConvertCoordToDesiredUnitSystem(stateMachine.ZCoordForCut, stateMachine.SourceUnits, stateMachine.OutputUnits).ToString());
                stateMachine.LastGCodeZCoord = stateMachine.ZCoordForCut;
                // do we need to adjust the feedrate?
                if (stateMachine.LastFeedRate != stateMachine.CurrentZFeedrate)
                {
                    // yes we do 
                    sb.Append(" " + GCODEWORD_FEEDRATE + ConvertCoordToDesiredUnitSystem(stateMachine.CurrentZFeedrate, stateMachine.SourceUnits, stateMachine.OutputUnits).ToString());
                    // remember this now
                    stateMachine.LastFeedRate = stateMachine.CurrentZFeedrate;
                }
                sb.Append(stateMachine.LineTerminator);

                // now move to the end of the line, cutting as we go
                if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_XAXIS + gX1OffsetCompensated_Rounded.ToString() + " " + GCODEWORD_YAXIS + gY1OffsetCompensated_Rounded.ToString());
                stateMachine.LastGCodeXCoord = gX1OffsetCompensated_Rounded;
                stateMachine.LastGCodeYCoord = gY1OffsetCompensated_Rounded;
                // do we need to adjust the feedrate?
                if (stateMachine.LastFeedRate != stateMachine.CurrentXYFeedrate)
                {
                    // yes we do 
                    sb.Append(" " + GCODEWORD_FEEDRATE + ConvertCoordToDesiredUnitSystem(stateMachine.CurrentXYFeedrate, stateMachine.SourceUnits, stateMachine.OutputUnits).ToString());
                    // remember this now
                    stateMachine.LastFeedRate = stateMachine.CurrentXYFeedrate;
                }
                sb.Append(stateMachine.LineTerminator);
            }
            //DebugTODO("for diagnostics only");
            //sb.Append(stateMachine.LineTerminator);
            return sb.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the action the plot requires based on the current context
        /// </summary>
        /// <param name="graphicsObj">a graphics object on which to plot</param>
        /// <param name="stateMachine">the gcode plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <param name="wantEndPointMarkers">if true we draw the endpoints of the gcodes
        /// in a different color</param>
        /// <returns>an enum value indicating what next action to take</returns>
        public override PlotActionEnum PerformPlotGCodeAction(Graphics graphicsObj, GCodeFileStateMachine stateMachine, bool wantEndPointMarkers, ref int errorValue, ref string errorString)
        {
            Pen workingPen = null;
            Brush workingBrush = null;

            if (stateMachine == null)
            {
                errorValue = 999;
                errorString = "PerformPlotGCodeAction (Line) stateMachine == null";
                return PlotActionEnum.PlotAction_FailWithError;
            }

            if (DoNotEmitToGCode == true)
            {
                return PlotActionEnum.PlotAction_Continue;
            }

            // GetPen, it will have been set up to have the proper width and color
            workingPen = stateMachine.PlotBorderPen;
            workingBrush = stateMachine.PlotBorderBrush;

            //DebugTODO("remove this");
            //if (DebugID == 3) workingPen = Pens.Red;
            //if (DebugID == 1) workingPen = Pens.Green;
            //if (DebugID == 6) workingPen = Pens.Blue;
            //if (DebugID == 4) workingPen = Pens.DarkOrange;

            //if (DebugID != 3)
            //{
            //    return PlotActionEnum.PlotAction_Continue;
            //}

            // Draw the line
            graphicsObj.DrawLine(workingPen, x0, y0, x1, y1);
            // now draw in the circular end points of the line
            MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x0, y0, workingPen.Width, workingPen.Width);
            MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x1, y1, workingPen.Width, workingPen.Width);

            return PlotActionEnum.PlotAction_Continue;

        }
    }
}

