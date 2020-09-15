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
    /// <history>
    ///    02 Aug 10  Cynic - Started
    /// </history>
    public class GCodeLine_Line : GCodeLine
    {
        // these are the endpoints of the line
        float x0 = -1;
        float y0 = -1;
        float x1 = -1;
        float y1 = -1;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public GCodeLine_Line(float x0In, float y0In, float x1In, float y1In) : base()
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
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public override string ToString()
        {
            return "GCodeLine_Line (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current x0 value
        /// </summary>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public float X0
        {
            get
            {
                return x0;
            }
            set
            {
                x0 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current y0 value
        /// </summary>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public float Y0
        {
            get
            {
                return y0;
            }
            set
            {
                y0 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current x1 value
        /// </summary>
        /// <history>
        ///    31 Jul 10  Cynic - Started
        /// </history>
        public float X1
        {
            get
            {
                return x1;
            }
            set
            {
                x1 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current y1 value
        /// </summary>
        /// <history>
        ///    31 Jul 10  Cynic - Started
        /// </history>
        public float Y1
        {
            get
            {
                return y1;
            }
            set
            {
                y1 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current gx0 value, but applies offset compensation
        /// </summary>
        /// <history>
        ///    11 Aug 10  Cynic - Started
        ///    03 Oct 10  Cynic - added Math.Round 
        /// </history>
        public float gX0OffsetCompensated
        {
            get
            {
                return (float)Math.Round((x0 - this.PlotXCoordOriginAdjust), 3);
                //return x0 - this.PlotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current gy0 value, but applies offset compensation
        /// </summary>
        /// <history>
        ///    11 Aug 10  Cynic - Started
        ///    03 Oct 10  Cynic - added Math.Round 
        /// </history>
        public float gY0OffsetCompensated
        {
            get
            {
                return (float)Math.Round((y0 - this.PlotYCoordOriginAdjust), 3);
                //return y0 - this.PlotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current gx0 value, but applies offset compensation
        /// </summary>
        /// <history>
        ///    11 Aug 10  Cynic - Started
        ///    03 Oct 10  Cynic - added Math.Round 
        /// </history>
        public float gX1OffsetCompensated
        {
            get
            {
                return (float)Math.Round((x1 - this.PlotXCoordOriginAdjust), 3);
                //return x1 - this.PlotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current gy0 value, but applies offset compensation
        /// </summary>
        /// <history>
        ///    11 Aug 10  Cynic - Started
        ///    03 Oct 10  Cynic - added Math.Round 
        /// </history>
        public float gY1OffsetCompensated
        {
            get
            {
                return (float)Math.Round((y1 - this.PlotYCoordOriginAdjust), 3);
                //return y1 - this.PlotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current gx0 value, but applies offset and user compensation
        /// </summary>
        /// <history>
        ///    26 Feb 11  Cynic - Started
        /// </history>
        public float gX0OffsetAndUserCompensated
        {
            get
            {
                return 0; //gX0OffsetAndUserCompensated + StateMachine.;
                //return x0 - this.PlotXCoordOriginAdjust;

// working on getting this going. line does not know about state machine - will have to 
// pass it in or put value in somewhere else

            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current gy0 value, but applies offset and user compensation
        /// </summary>
        /// <history>
        ///    26 Feb 11  Cynic - Started
        /// </history>
        public float gY0OffsetAndUserCompensated
        {
            get
            {
                return 0;// (float)Math.Round((y0 - this.PlotYCoordOriginAdjust), 3);
                //return y0 - this.PlotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current gx0 value, but applies offset and user compensation
        /// </summary>
        /// <history>
        ///    26 Feb 11  Cynic - Started
        /// </history>
        public float gX1OffsetAndUserCompensated
        {
            get
            {
                return (float)Math.Round((x1 - this.PlotXCoordOriginAdjust), 3);
                //return x1 - this.PlotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current gy0 value, but applies offset and user compensation
        /// </summary>
        /// <history>
        ///    26 Feb 11  Cynic - Started
        /// </history>
        public float gY1OffsetAndUserCompensated
        {
            get
            {
                return (float)Math.Round((y1 - this.PlotYCoordOriginAdjust), 3);
                //return y1 - this.PlotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GCode lines for this object as they would be written to the text file. Will 
        /// never return a null value. Can often return more than one line if the 
        /// current context in the stateMachine requires it
        /// </summary>
        /// <param name="stateMachine">the stateMachine</param>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        ///    03 Oct 10  Cynic - added Math.Round on coordinate tests
        /// </history>
        public override string GetGCodeLine(GCodeFileStateMachine stateMachine)
        {
            StringBuilder sb = new StringBuilder();

            if (stateMachine == null)
            {
                LogMessage("GetGCodeLine: stateMachine == null");
                return "M5 ERROR: stateMachine==null";
            }

            // are we currently on our start coord?
            if ((Math.Round(gX0OffsetCompensated, 3) == Math.Round(stateMachine.LastGCodeXCoord, 3)) && (Math.Round(gY0OffsetCompensated, 3) == Math.Round(stateMachine.LastGCodeYCoord, 3)))
            {
                // yes we are, we do not need to move the tool head there. Is our Z Axis correctly set?
                if (Math.Round(stateMachine.LastGCodeZCoord, 3) != Math.Round(stateMachine.ZCoordForCut, 3))
                {
                    // no it is not, send it down to to the CUT depth
                    if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                    sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_ZAXIS + stateMachine.ZCoordForCut.ToString());
                    stateMachine.LastGCodeZCoord = stateMachine.ZCoordForCut;
                    // do we need to adjust the feedrate?
                    if (stateMachine.LastFeedRate != stateMachine.CurrentZFeedrate)
                    {
                        // yes we do 
                        sb.Append(" " + GCODEWORD_FEEDRATE + stateMachine.CurrentZFeedrate.ToString());
                        // remember this now
                        stateMachine.LastFeedRate = stateMachine.CurrentZFeedrate;
                    }
                    sb.Append(stateMachine.LineTerminator);
                }

                // now move the tool head, cutting as we go
                if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_XAXIS + gX1OffsetCompensated.ToString() + " " + GCODEWORD_YAXIS + gY1OffsetCompensated.ToString());
                stateMachine.LastGCodeXCoord = gX1OffsetCompensated;
                stateMachine.LastGCodeYCoord = gY1OffsetCompensated;
                // do we need to adjust the feedrate?
                if (stateMachine.LastFeedRate != stateMachine.CurrentXYFeedrate)
                {
                    // yes we do 
                    sb.Append(" " + GCODEWORD_FEEDRATE+stateMachine.CurrentXYFeedrate.ToString());
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
                    if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                    sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_ZAXIS + stateMachine.ZCoordForMove.ToString() );
                    stateMachine.LastGCodeZCoord = stateMachine.ZCoordForMove;
                    // do we need to adjust the feedrate?
                    if (stateMachine.LastFeedRate != stateMachine.CurrentZFeedrate)
                    {
                        // yes we do 
                        sb.Append(" " + GCODEWORD_FEEDRATE + stateMachine.CurrentZFeedrate.ToString());
                        // remember this now
                        stateMachine.LastFeedRate = stateMachine.CurrentZFeedrate;
                    }
                    sb.Append(stateMachine.LineTerminator);
                }

                // Now move quickly to the start, we know our Z is now be ok and above the pcb
                if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVERAPID + " " + GCODEWORD_XAXIS + gX0OffsetCompensated.ToString() + " " + GCODEWORD_YAXIS + gY0OffsetCompensated.ToString());
                stateMachine.LastGCodeXCoord = gX0OffsetCompensated;
                stateMachine.LastGCodeYCoord = gY0OffsetCompensated;
                sb.Append(stateMachine.LineTerminator);
          
                // now put our Z down to the cut depth
                if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_ZAXIS + stateMachine.ZCoordForCut.ToString());
                stateMachine.LastGCodeZCoord = stateMachine.ZCoordForCut;
                // do we need to adjust the feedrate?
                if (stateMachine.LastFeedRate != stateMachine.CurrentZFeedrate)
                {
                    // yes we do 
                    sb.Append(" " + GCODEWORD_FEEDRATE + stateMachine.CurrentZFeedrate.ToString());
                    // remember this now
                    stateMachine.LastFeedRate = stateMachine.CurrentZFeedrate;
                }
                sb.Append(stateMachine.LineTerminator);

                // now move to the end of the line, cutting as we go
                if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_XAXIS + gX1OffsetCompensated.ToString() + " " + GCODEWORD_YAXIS + gY1OffsetCompensated.ToString());
                stateMachine.LastGCodeXCoord = gX1OffsetCompensated;
                stateMachine.LastGCodeYCoord = gY1OffsetCompensated;
                // do we need to adjust the feedrate?
                if (stateMachine.LastFeedRate != stateMachine.CurrentXYFeedrate)
                {
                    // yes we do 
                    sb.Append(" " + GCODEWORD_FEEDRATE + stateMachine.CurrentXYFeedrate.ToString());
                    // remember this now
                    stateMachine.LastFeedRate = stateMachine.CurrentXYFeedrate;
                }
                sb.Append(stateMachine.LineTerminator);
            }
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
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public override GCodeLine.PlotActionEnum PerformPlotGCodeAction(Graphics graphicsObj, GCodeFileStateMachine stateMachine, bool wantEndPointMarkers, ref int errorValue, ref string errorString)
        {
            Pen workingPen = null;
            Pen cutLinePen = null;
            Brush workingBrush = null;

            if (stateMachine == null)
            {
                errorValue = 999;
                errorString = "PerformPlotGCodeAction (Line) stateMachine == null";
                return GCodeLine.PlotActionEnum.PlotAction_FailWithError;
            }

            // GetPen, it will have been set up to have the proper width and color
            workingPen = stateMachine.PlotBorderPen;
            cutLinePen = stateMachine.PlotCutLinePen;
            workingBrush = stateMachine.PlotBorderBrush;

            // Draw lin
            graphicsObj.DrawLine(workingPen, X0 * stateMachine.IsoPlotPointsPerAppUnit, Y0 * stateMachine.IsoPlotPointsPerAppUnit, X1 * stateMachine.IsoPlotPointsPerAppUnit, Y1 * stateMachine.IsoPlotPointsPerAppUnit);
            // now draw in the circular end points of the line
            MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, workingBrush, (int)Math.Round((X0 * stateMachine.IsoPlotPointsPerAppUnit)), (int)Math.Round((Y0 * stateMachine.IsoPlotPointsPerAppUnit)), workingPen.Width, workingPen.Width);
            MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, workingBrush, (int)Math.Round((X1 * stateMachine.IsoPlotPointsPerAppUnit)), (int)Math.Round((Y1 * stateMachine.IsoPlotPointsPerAppUnit)), workingPen.Width, workingPen.Width);

            // are we to show the cutlines
         //   if (stateMachine.ShowGCodeCutLines == true)
         //   {
         //       graphicsObj.DrawLine(cutLinePen, X0 * stateMachine.IsoPlotPointsPerAppUnit, Y0 * stateMachine.IsoPlotPointsPerAppUnit, X1 * stateMachine.IsoPlotPointsPerAppUnit, Y1 * stateMachine.IsoPlotPointsPerAppUnit);
         //   }

            return PlotActionEnum.PlotAction_Continue;

        }
    }
}
