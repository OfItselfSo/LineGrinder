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
    /// A class to encapsulate a GCode arc object
    /// </summary>
    /// <history>
    ///    06 Aug 10  Cynic - Started
    /// </history>
    public class GCodeLine_Arc : GCodeLine
    {
        // this tells the gcode interpreter which way to draw the arc
        public enum GCodeArcDirectionEnum
        {
            GCodeArcDirection_CW,
            GCodeArcDirection_CCW,
        }
        public const GCodeArcDirectionEnum DEFAULT_ARC_DIRECTION = GCodeArcDirectionEnum.GCodeArcDirection_CCW;
        private GCodeArcDirectionEnum arcDirection = DEFAULT_ARC_DIRECTION;

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        float xCenter = -1;
        float yCenter = -1;
        float radius = -1;

        // these are the endpoints of the line we use when writing 
        // out the gcode text
        float gx0 = -1;
        float gy0 = -1;
        float gx1 = -1;
        float gy1 = -1;

        // these are the endpoints of the line we use when writing 
        // out the gcode plot, they will be the same points but
        // in (possibly) a different order to cope with the insanity
        // of the .NET graphics tools
        float px0 = -1;
        float py0 = -1;
        float px1 = -1;
        float py1 = -1;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="radiusIn">radius of arc</param>
        /// <param name="xCenterIn">X center point</param>
        /// <param name="yCenterIn">Y center point</param>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public GCodeLine_Arc(float xCenterIn, float yCenterIn, float radiusIn)
            : base()
        {
            xCenter = xCenterIn;
            yCenter = yCenterIn;
            radius = radiusIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the points we use when we output GCode
        /// </summary>
        /// <param name="gx0In">gX0 coord</param>
        /// <param name="gy0In">gY0 coord</param>
        /// <param name="gx1In">gX1 coord</param>
        /// <param name="gy1In">gY1 coord</param>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        public void SetPointsForGCodeOutput(float gx0In, float gy0In, float gx1In, float gy1In)
        {
            gx0 = gx0In;
            gy0 = gy0In;
            gx1 = gx1In;
            gy1 = gy1In;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the points we use when we plot GCode on screen
        /// </summary>
        /// <param name="px0In">pX0 coord</param>
        /// <param name="py0In">pY0 coord</param>
        /// <param name="px1In">pX1 coord</param>
        /// <param name="py1In">pY1 coord</param>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        public void SetPointsForGPlotOutput(float px0In, float py0In, float px1In, float py1In)
        {
            px0 = px0In;
            py0 = py0In;
            px1 = px1In;
            py1 = py1In;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides ToString()
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public override string ToString()
        {
            return "GCodeLine_Arc (" + gx0.ToString() + "," + gy0.ToString() + ") (" + gx1.ToString() + "," + gy1.ToString() + ") xCenter=" + xCenter.ToString() + " yCenter=" + yCenter.ToString() + " radius=" + radius.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts an arc direction and returns the opposite direction. Makes it 
        /// easy to flip directions if we need to do so.
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public static GCodeArcDirectionEnum GetOtherArcDirection(GCodeArcDirectionEnum arcDirIn)
        {
            if (arcDirIn == GCodeArcDirectionEnum.GCodeArcDirection_CCW) return GCodeArcDirectionEnum.GCodeArcDirection_CW;
            else return GCodeArcDirectionEnum.GCodeArcDirection_CCW;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current arc Direction
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public GCodeArcDirectionEnum ArcDirection
        {
            get
            {
                return arcDirection;
            }
            set
            {
                arcDirection = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current radius value
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current xCenter value, but applies offset compensation
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public float X1CenterOffsetCompensated
        {
            get
            {
                return xCenter - this.PlotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current yCenter value, but applies offset compensation
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public float Y1CenterOffsetCompensated
        {
            get
            {
                return yCenter - this.PlotYCoordOriginAdjust;
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
                return (float)Math.Round((gx0 - this.PlotXCoordOriginAdjust), 3);
                //return gx0 - this.PlotXCoordOriginAdjust;
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
                return (float)Math.Round((gy0 - this.PlotYCoordOriginAdjust), 3);
                //return gy0 - this.PlotYCoordOriginAdjust;
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
                return (float)Math.Round((gx1 - this.PlotXCoordOriginAdjust), 3);
                //return gx1 - this.PlotXCoordOriginAdjust;
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
                return (float)Math.Round((gy1 - this.PlotYCoordOriginAdjust), 3);
               // return gy1 - this.PlotYCoordOriginAdjust;
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
        ///    06 Aug 10  Cynic - Started
        ///    03 Oct 10  Cynic - added Math.Round on coordinate tests
        /// </history>
        public override string GetGCodeLine(GCodeFileStateMachine stateMachine)
        {
            StringBuilder sb = new StringBuilder();
            string gcodeWordArcDirection = GCodeLine.GCODEWORD_MOVEINCIRCLECCW;
            float xCenterOffset = -1;
            float yCenterOffset = -1;
            double radius;
            double theta;
            float xTrueEndPoint;
            float yTrueEndPoint;
            float xEndDistance;
            float yEndDistance;

            if (stateMachine == null)
            {
                LogMessage("GetGCodeLine: stateMachine == null");
                return "M5 ERROR: stateMachine==null";
            }

            // set our arc direction code word
            if (arcDirection == GCodeArcDirectionEnum.GCodeArcDirection_CCW)
            {
                gcodeWordArcDirection = GCodeLine.GCODEWORD_MOVEINCIRCLECCW;
            }
            else
            {
                gcodeWordArcDirection = GCodeLine.GCODEWORD_MOVEINCIRCLECW;
            }

           // sb.Append("(ZZZZZ---->GO2Arc)");
            sb.Append(stateMachine.LineTerminator);

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
                // we must calc our offsets to the center
                xCenterOffset = (float)Math.Round(this.X1CenterOffsetCompensated - stateMachine.LastGCodeXCoord, 4);
                yCenterOffset = (float)Math.Round(this.Y1CenterOffsetCompensated - stateMachine.LastGCodeYCoord, 4);
                
                // The way the G02,G03 commands work is they have a start point, an offset to the center
                // and the endpoint of the arc. It must be true that the endpoint of the arc is actually on
                // the arc itself. If this is not the case then most machine controllers will throw an error
                // it can happen, due to the way we interpolate things that our endpoint is a bit off the arc
                // so we calculate the best endpoint actually on the arc and use that in the G02/G03 command
                // then we add a little compensating linear move (at the same Z position) to get us to the 
                // point we need to end at. Usually this little "step" in the arc etc is so small it is invisible

                // we need the radius
                radius=Math.Sqrt((xCenterOffset*xCenterOffset)+(yCenterOffset*yCenterOffset));

                // we need the xdistance and ydistances of the end point to the center
                xEndDistance = (float)Math.Round(gX1OffsetCompensated - X1CenterOffsetCompensated, 4);
                yEndDistance = (float)Math.Round(gY1OffsetCompensated - Y1CenterOffsetCompensated, 4);

                if (xEndDistance != 0)
                {
                    // we need the angle
                    theta = Math.Atan2(yEndDistance, xEndDistance);
                    // calc the true end points
                    xTrueEndPoint = (float)Math.Round(X1CenterOffsetCompensated+(radius * Math.Cos(theta)), 4);
                    yTrueEndPoint = (float)Math.Round(Y1CenterOffsetCompensated+(radius * Math.Sin(theta)), 4);
                }
                else
                {
                    // assume the true endpoints are the ones we got
                    xTrueEndPoint = gX1OffsetCompensated;
                    yTrueEndPoint = gY1OffsetCompensated;
                }

                // now emit the GCode with the true endpoint
                sb.Append(gcodeWordArcDirection + " " + GCODEWORD_XAXIS + xTrueEndPoint.ToString() + " " + GCODEWORD_YAXIS + yTrueEndPoint.ToString() + " " + GCODEWORD_XARCCENTER + xCenterOffset.ToString() + " " + GCODEWORD_YARCCENTER + yCenterOffset.ToString());               
                // do we need to adjust the feedrate?
                if (stateMachine.LastFeedRate != stateMachine.CurrentXYFeedrate)
                {
                    // yes we do 
                    sb.Append(" " + GCODEWORD_FEEDRATE+stateMachine.CurrentXYFeedrate.ToString());
                    // remember this now
                    stateMachine.LastFeedRate = stateMachine.CurrentXYFeedrate;
                }
                sb.Append(stateMachine.LineTerminator);

                // now do we need to add a little linear run so we actually end up on the 
                // true XY end point
                if ((xTrueEndPoint != gX1OffsetCompensated) || (yTrueEndPoint != gY1OffsetCompensated))
                {
                    if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                    sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_XAXIS + gX1OffsetCompensated.ToString() + " " + GCODEWORD_YAXIS + gY1OffsetCompensated.ToString());
                    sb.Append(stateMachine.LineTerminator);
               //     DebugTODO("remove this line");
                //    sb.Append("(XXXXXX---->xCenter=" + X1CenterOffsetCompensated.ToString() + " Ycenter=" + Y1CenterOffsetCompensated.ToString()+")");
               //     sb.Append(stateMachine.LineTerminator);
               //     sb.Append("(XXXXXX---->AutoGenerated Compensation)");
               //     sb.Append(stateMachine.LineTerminator);
                }
                stateMachine.LastGCodeXCoord = gX1OffsetCompensated;
                stateMachine.LastGCodeYCoord = gY1OffsetCompensated;
            }
            else
            {
                // no, we are not on the start coord. Is our Z Axis correctly set?
                if ((((Math.Round(stateMachine.LastGCodeZCoord, 3) == Math.Round(stateMachine.ZCoordForMove, 3)) || (Math.Round(stateMachine.LastGCodeZCoord, 3) == Math.Round(stateMachine.ZCoordForClear, 3)))) == false)
                {
                    // no it is not, pull it up to the MOVE distance
                    if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                    sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_ZAXIS + stateMachine.ZCoordForMove.ToString());
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

                // now move to the end of the arc, cutting as we go
                if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                // we must calc our offsets to the center
                xCenterOffset = (float)Math.Round(this.X1CenterOffsetCompensated - stateMachine.LastGCodeXCoord, 4);
                yCenterOffset = (float)Math.Round(this.Y1CenterOffsetCompensated - stateMachine.LastGCodeYCoord, 4);

                // The way the G02,G03 commands work is they have a start point, an offset to the center
                // and the endpoint of the arc. It must be true that the endpoint of the arc is actually on
                // the arc itself. If this is not the case then most machine controllers will throw an error
                // it can happen, due to the way we interpolate things that our endpoint is a bit off the arc
                // so we calculate the best endpoint actually on the arc and use that in the G02/G03 command
                // then we add a little compensating linear move (at the same Z position) to get us to the 
                // point we need to end at. Usually this little "step" in the arc etc is so small it is invisible

                // we need the radius
                radius = Math.Sqrt((xCenterOffset * xCenterOffset) + (yCenterOffset * yCenterOffset));

                // we need the xdistance and ydistances of the end point to the center
                xEndDistance = (float)Math.Round(gX1OffsetCompensated - X1CenterOffsetCompensated, 4);
                yEndDistance = (float)Math.Round(gY1OffsetCompensated - Y1CenterOffsetCompensated, 4);

                if (xEndDistance != 0)
                {
                    // we need the angle
                    theta = Math.Atan2(yEndDistance, xEndDistance);
                    // calc the true end points
                    xTrueEndPoint = (float)Math.Round(X1CenterOffsetCompensated + (radius * Math.Cos(theta)), 4);
                    yTrueEndPoint = (float)Math.Round(Y1CenterOffsetCompensated + (radius * Math.Sin(theta)), 4);
                }
                else
                {
                    // assume the true endpoints are the ones we got
                    xTrueEndPoint = gX1OffsetCompensated;
                    yTrueEndPoint = gY1OffsetCompensated;
                }

                // now emit the GCode with the true endpoint
                sb.Append(gcodeWordArcDirection + " " + GCODEWORD_XAXIS + xTrueEndPoint.ToString() + " " + GCODEWORD_YAXIS + yTrueEndPoint.ToString() + " " + GCODEWORD_XARCCENTER + xCenterOffset.ToString() + " " + GCODEWORD_YARCCENTER + yCenterOffset.ToString());
                // do we need to adjust the feedrate?
                if (stateMachine.LastFeedRate != stateMachine.CurrentXYFeedrate)
                {
                    // yes we do 
                    sb.Append(" " + GCODEWORD_FEEDRATE + stateMachine.CurrentXYFeedrate.ToString());
                    // remember this now
                    stateMachine.LastFeedRate = stateMachine.CurrentXYFeedrate;
                }
                sb.Append(stateMachine.LineTerminator);

                // now do we need to add a little linear run so we actually end up on the 
                // true XY end point
                if ((xTrueEndPoint != gX1OffsetCompensated) || (yTrueEndPoint != gY1OffsetCompensated))
                {
                    if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                    sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_XAXIS + gX1OffsetCompensated.ToString() + " " + GCODEWORD_YAXIS + gY1OffsetCompensated.ToString());
                    sb.Append(stateMachine.LineTerminator);
                  //  DebugTODO("remove this line");
                  //  sb.Append("(YYYYYY---->xCenter=" + X1CenterOffsetCompensated.ToString() + " Ycenter=" + Y1CenterOffsetCompensated.ToString()+")");
                  //  sb.Append(stateMachine.LineTerminator);
                  //  sb.Append("(YYYYYY---->AutoGenerated Compensation)");
                  //  sb.Append(stateMachine.LineTerminator);
                }
                stateMachine.LastGCodeXCoord = gX1OffsetCompensated;
                stateMachine.LastGCodeYCoord = gY1OffsetCompensated;
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
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public override GCodeLine.PlotActionEnum PerformPlotGCodeAction(Graphics graphicsObj, GCodeFileStateMachine stateMachine, bool wantEndPointMarkers, ref int errorValue, ref string errorString)
        {
            Pen workingPen = null;
            Pen cutLinePen = null;
            Brush workingBrush = null;

            float startAngle = 0;
            float endAngle = 0;
            float sweepAngle = 0;

            //DebugMessage("PerformPlotIsoStep3Action: " + this.ToString());

            int xCenterInt = (int)Math.Round((xCenter * stateMachine.IsoPlotPointsPerAppUnit));
            int yCenterInt = (int)Math.Round((yCenter * stateMachine.IsoPlotPointsPerAppUnit));
            int radiusInt = (int)Math.Round((radius * stateMachine.IsoPlotPointsPerAppUnit));
            int px0Int = (int)Math.Round((px0 * stateMachine.IsoPlotPointsPerAppUnit));
            int py0Int = (int)Math.Round((py0 * stateMachine.IsoPlotPointsPerAppUnit));
            int px1Int = (int)Math.Round((px1 * stateMachine.IsoPlotPointsPerAppUnit));
            int py1Int = (int)Math.Round((py1 * stateMachine.IsoPlotPointsPerAppUnit));

            // GetPen, it will have been set up to have the proper width and color
            workingPen = stateMachine.PlotBorderPen;
            cutLinePen = stateMachine.PlotCutLinePen;
            workingBrush = stateMachine.PlotBorderBrush;

            Rectangle boundingRect = MiscGraphicsUtils.GetBoundingRectForCircle(xCenterInt, yCenterInt, radiusInt);
            startAngle = MiscGraphicsUtils.GetCCWAngleOfLineToXAxis(xCenterInt, yCenterInt, px0Int, py0Int);
            endAngle = MiscGraphicsUtils.GetCCWAngleOfLineToXAxis(xCenterInt, yCenterInt, px1Int, py1Int);

            if (endAngle == 0)
            {
                sweepAngle = 360 - startAngle;
            }
            else
            {
                sweepAngle = endAngle - startAngle;
            }

            //DebugMessage("boundingRect X=" + boundingRect.X.ToString() + " Y=" + boundingRect.Y.ToString() + " width=" + boundingRect.Width.ToString() + " height=" + boundingRect.Height.ToString());
            //DebugMessage("  startAngle=" + startAngle.ToString() + " endAngle=" + endAngle.ToString() + " sweepAngle=" + sweepAngle.ToString());

            // Draw arc
            graphicsObj.DrawArc(workingPen, boundingRect, startAngle, sweepAngle);

            // now draw in the circular end points of the line
            MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, workingBrush, px0Int, py0Int, workingPen.Width, workingPen.Width);
            MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, workingBrush, px0Int, py0Int, workingPen.Width, workingPen.Width);

            // are we to show the cutlines
         //   if (stateMachine.ShowGCodeCutLines == true)
         //   {
         //       graphicsObj.DrawArc(cutLinePen, boundingRect, startAngle, sweepAngle);
         //   }
            return PlotActionEnum.PlotAction_Continue;

        }
    }
}
