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
    public class GCodeCmd_Arc : GCodeCmd
    {
        public const GCodeCmd_ArcDirectionEnum DEFAULT_ARC_DIRECTION = GCodeCmd_ArcDirectionEnum.GCodeArcDirection_CCW;
        private GCodeCmd_ArcDirectionEnum arcDirection = DEFAULT_ARC_DIRECTION;

        private const int ARBITRARY_MIN_NUMBER_OF_CELLS_FOR_0_DEGREE_ARC = 10;

        private int x0 = -1;
        private int y0 = -1;
        private int x1 = -1;
        private int y1 = -1;
        private int xCenter = 0;
        private int yCenter = 0;
        private int radius = 0;
        private bool wantClockWise = false;
        // this is bit of a hangover from the gerber spec so we know how to draw the line
        private bool wantMultiQuadrant = false;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0In">X0 coord</param>
        /// <param name="y0In">Y0 coord</param>
        /// <param name="x1In">X1 coord</param>
        /// <param name="y1In">Y1 coord</param>
        /// <param name="cx">center X</param>
        /// <param name="cy">center Y</param>
        /// <param name="radiusIn">the radius</param>
        /// <param name="wantClockWiseIn">the wantClockWise value</param>
        /// <param name="wantMultiQuadrantIn">if true the arc is being drawn in multi quadrant mode</param>
        /// <param name="isoPlotCellsInObjectIn">the number of isoplot cells used to build this object</param>
        /// <param name="isoPlotCellsInObjectIn">the number of isoplot cells used to build this object</param>
        public GCodeCmd_Arc(int x0In, int y0In, int x1In, int y1In, int cx, int cy, int radiusIn, bool wantClockWiseIn, bool wantMultiQuadrantIn, int isoPlotCellsInObjectIn, int debugIDIn) : base(isoPlotCellsInObjectIn, debugIDIn)
        {
            x0 = x0In;
            y0 = y0In;
            x1 = x1In;
            y1 = y1In;
            xCenter = cx;
            yCenter = cy;
            radius = radiusIn;
            wantClockWise = wantClockWiseIn;
            wantMultiQuadrant = wantMultiQuadrantIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides ToString()
        /// </summary>
        public override string ToString()
        {
            return "GCodeCmd_Arc (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ") xCenter=" + xCenter.ToString() + " yCenter=" + yCenter.ToString() + " radius=" + radius.ToString() + ", wantClockWise=" + wantClockWise.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts an arc direction and returns the opposite direction. Makes it 
        /// easy to flip directions if we need to do so.
        /// </summary>
        public static GCodeCmd_ArcDirectionEnum GetOtherArcDirection(GCodeCmd_ArcDirectionEnum arcDirIn)
        {
            if (arcDirIn == GCodeCmd_ArcDirectionEnum.GCodeArcDirection_CCW) return GCodeCmd_ArcDirectionEnum.GCodeArcDirection_CW;
            else return GCodeCmd_ArcDirectionEnum.GCodeArcDirection_CCW;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current arc Direction
        /// </summary>
        public GCodeCmd_ArcDirectionEnum ArcDirection
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
        /// Gets a reversed arc direction
        /// </summary>
        private string GetReversedArcDirection(string archDirectionIn)
        {
            if (archDirectionIn == GCodeCmd.GCODEWORD_MOVEINCIRCLECCW) return GCodeCmd.GCODEWORD_MOVEINCIRCLECW;
            else return GCodeCmd.GCODEWORD_MOVEINCIRCLECCW;
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

            string gcodeWordArcDirection = GCodeCmd.GCODEWORD_MOVEINCIRCLECCW;
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
                LogMessage("GetGCodeCmd: stateMachine == null");
                return "M5 ERROR: stateMachine==null";
            }

            if (DoNotEmitToGCode == true)
            {
                return sb.ToString();
            }

            // set our arc direction code word
            if (arcDirection == GCodeCmd_ArcDirectionEnum.GCodeArcDirection_CCW)
            {
                gcodeWordArcDirection = GCodeCmd.GCODEWORD_MOVEINCIRCLECCW;
            }
            else
            {
                gcodeWordArcDirection = GCodeCmd.GCODEWORD_MOVEINCIRCLECW;
            }

            // we need to reverse the direction if we are flipping
            if(stateMachine.MirrorOnConversionToGCode == IsoFlipModeEnum.X_Flip)
            {
                gcodeWordArcDirection = GetReversedArcDirection(gcodeWordArcDirection);
            }

            float gX0OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_X(stateMachine, x0), 3);
            float gY0OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_Y(stateMachine, y0), 3);
            float gX1OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_X(stateMachine, x1), 3);
            float gY1OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_Y(stateMachine, y1), 3);
            float gXCenterOffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_X(stateMachine, xCenter), 3);
            float gYCenterOffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_Y(stateMachine, yCenter), 3);

            float gX0OffsetCompensated_Rounded = (float)Math.Round(gX0OffsetCompensated_Raw, 3);
            float gY0OffsetCompensated_Rounded = (float)Math.Round(gY0OffsetCompensated_Raw, 3);
            float gX1OffsetCompensated_Rounded = (float)Math.Round(gX1OffsetCompensated_Raw, 3);
            float gY1OffsetCompensated_Rounded = (float)Math.Round(gY1OffsetCompensated_Raw, 3);

            // are we drawing this value in reverse?
            if (this.ReverseOnConversionToGCode == true)
            {
                float tmpX = gX0OffsetCompensated_Rounded;
                float tmpY = gY0OffsetCompensated_Rounded;
                gX0OffsetCompensated_Rounded = gX1OffsetCompensated_Rounded;
                gY0OffsetCompensated_Rounded = gY1OffsetCompensated_Rounded;
                gX1OffsetCompensated_Rounded = tmpX;
                gY1OffsetCompensated_Rounded = tmpY;
                // do this again
                gcodeWordArcDirection = GetReversedArcDirection(gcodeWordArcDirection);
            }


            float gXCenterOffsetCompensated_Rounded = (float)Math.Round(gXCenterOffsetCompensated_Raw, 4);   
            float gYCenterOffsetCompensated_Rounded = (float)Math.Round(gYCenterOffsetCompensated_Raw, 4);   

            float gXLastGCodeCoord_Rounded = (float)Math.Round(stateMachine.LastGCodeXCoord, 3);
            float gYLastGCodeCoord_Rounded = (float)Math.Round(stateMachine.LastGCodeYCoord, 3);

            // sb.Append("(ZZZZZ---->GO2Arc)");
            sb.Append(stateMachine.LineTerminator);

            // are we currently on our start coord?
            if ((gX0OffsetCompensated_Rounded == gXLastGCodeCoord_Rounded) && (gY0OffsetCompensated_Rounded == gYLastGCodeCoord_Rounded))
            {
                // yes we are, we do not need to move the tool head there. Is our Z Axis correctly set?
                if (Math.Round(stateMachine.LastGCodeZCoord, 3) != Math.Round(stateMachine.ZCoordForCut, 3))
                {
                    // no it is not, send it down to to the CUT depth
                    if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
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
                if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                // we must calc our offsets to the center, the g?CenterOffsetCompensated_Rounded are actually the center coordinates
                xCenterOffset = gXCenterOffsetCompensated_Rounded - gX0OffsetCompensated_Rounded;
                yCenterOffset = gYCenterOffsetCompensated_Rounded - gY0OffsetCompensated_Rounded;

                // The way the G02,G03 commands work is they have a start point, an offset to the center
                // and the endpoint of the arc. It must be true that the endpoint of the arc is actually on
                // the arc itself. If this is not the case then most machine controllers will throw an error.

                // Given the new chain calculation method this problem does not appear to exist. If needed 
                // the commented out code below tells what to do

                // assume the true endpoints are the ones we got
                xTrueEndPoint = gX1OffsetCompensated_Rounded;
                yTrueEndPoint = gY1OffsetCompensated_Rounded;

/* not operational any more, and did not work well 
                // The endpoint error can happen due to the way we interpolate things that our endpoint is a bit off the arc
                // so we calculate the best endpoint actually on the arc and use that in the G02/G03 command
                // then we add a little compensating linear move (at the same Z position) to get us to the 
                // point we need to end at. Usually this little "step" in the arc etc is so small it is invisible

                // we need the radius, calculated off the start point
                radius = MiscGraphicsUtils.GetDistanceBetweenTwoPoints(gX0OffsetCompensated_Raw, gY0OffsetCompensated_Raw, gXCenterOffsetCompensated_Rounded, gYCenterOffsetCompensated_Rounded);
*/

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

                stateMachine.LastGCodeXCoord = gX1OffsetCompensated_Rounded;
                stateMachine.LastGCodeYCoord = gY1OffsetCompensated_Rounded;
            }
            else
            {
                // no, we are not on the start coord. Is our Z Axis correctly set?
                if ((((Math.Round(stateMachine.LastGCodeZCoord, 3) == Math.Round(stateMachine.ZCoordForMove, 3)) || (Math.Round(stateMachine.LastGCodeZCoord, 3) == Math.Round(stateMachine.ZCoordForClear, 3)))) == false)
                {
                    // no it is not, pull it up to the MOVE distance
                    if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
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
                if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVERAPID + " " + GCODEWORD_XAXIS + gX0OffsetCompensated_Rounded.ToString() + " " + GCODEWORD_YAXIS + gY0OffsetCompensated_Rounded.ToString());
                stateMachine.LastGCodeXCoord = gX0OffsetCompensated_Rounded;
                stateMachine.LastGCodeYCoord = gY0OffsetCompensated_Rounded;
                sb.Append(stateMachine.LineTerminator);
                
                // now put our Z down to the cut depth
                if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
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
                if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                // we must calc our offsets to the center
                xCenterOffset = (float)Math.Round(gXCenterOffsetCompensated_Rounded - stateMachine.LastGCodeXCoord, 4);
                yCenterOffset = (float)Math.Round(gYCenterOffsetCompensated_Rounded - stateMachine.LastGCodeYCoord, 4);

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
                xEndDistance = (float)Math.Round(gX1OffsetCompensated_Rounded - gXCenterOffsetCompensated_Rounded, 4);
                yEndDistance = (float)Math.Round(gY1OffsetCompensated_Rounded - gYCenterOffsetCompensated_Rounded, 4);

                if (xEndDistance != 0)
                {
                    // we need the angle
                    theta = Math.Atan2(yEndDistance, xEndDistance);
                    // calc the true end points
                    xTrueEndPoint = (float)Math.Round(gXCenterOffsetCompensated_Rounded + (radius * Math.Cos(theta)), 4);
                    yTrueEndPoint = (float)Math.Round(gYCenterOffsetCompensated_Rounded + (radius * Math.Sin(theta)), 4);
                }
                else
                {
                    // assume the true endpoints are the ones we got
                    xTrueEndPoint = gX1OffsetCompensated_Rounded;
                    yTrueEndPoint = gY1OffsetCompensated_Rounded;
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
                if ((xTrueEndPoint != gX1OffsetCompensated_Rounded) || (yTrueEndPoint != gY1OffsetCompensated_Rounded))
                {
                    if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                    sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_XAXIS + gX1OffsetCompensated_Rounded.ToString() + " " + GCODEWORD_YAXIS + gY1OffsetCompensated_Rounded.ToString());
                    sb.Append(stateMachine.LineTerminator);
                  //  DebugTODO("remove this line");
                  //  sb.Append("(YYYYYY---->xCenter=" + X1CenterOffsetCompensated.ToString() + " Ycenter=" + Y1CenterOffsetCompensated.ToString()+")");
                  //  sb.Append(stateMachine.LineTerminator);
                  //  sb.Append("(YYYYYY---->AutoGenerated Compensation)");
                  //  sb.Append(stateMachine.LineTerminator);
                }
                stateMachine.LastGCodeXCoord = gX1OffsetCompensated_Rounded;
                stateMachine.LastGCodeYCoord = gY1OffsetCompensated_Rounded;
            }

            //DebugTODO("for diagnostics only");
            //sb.Append(stateMachine.LineTerminator);

            return sb.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the action the plot requires based on the current context. Note
        /// that the coordinates stored in GCodeCmds in here are not in isoPlot coords.
        /// They coords are are in 
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

            if (DoNotEmitToGCode== true)
            {
                return PlotActionEnum.PlotAction_Continue;
            }

            // get the current X,Y coordinates, these will already be scaled, origin compensated and flipped
            float startXCoord = x0;
            float startYCoord = y0;

            // get the end X,Y coordinate
            float endXCoord = x1;
            float endYCoord = y1;

            // get the arc center point, by adding on the specified offsets. Note we do not involve
            // the end coords here. These are calculated off the start coords
            float arcCenterXCoord = xCenter;
            float arcCenterYCoord = yCenter;

            // get our points
            PointF startPoint = new PointF(startXCoord, startYCoord);
            PointF endPoint = new PointF(endXCoord, endYCoord);
            PointF centerPoint = new PointF(arcCenterXCoord, arcCenterYCoord);

            //DebugMessage("PG startPoint=" + startPoint.ToString() + ", endPoint=" + endPoint.ToString() + ", centerPoint=" + centerPoint.ToString() + ", radius=" + radius);

            // A NOTE on the direction of angles here. Gerber can specifiy clockwise or counter clockwise. .NET only wants its angles specified counter 
            // clockwise when it draws. GCode can go either way. However, the arcs drawn here are segments of a larger arc (or cicle) and these are 
            // built out of isoplot segments. The isoplot segment collector always goes around arcs in a counter clockwise direction. This essentially
            // converts any clockwise arcs into counter clockwise ones.

            // this means that all start points, end points and sweep angles are ALWAYS listed as CounterClockwise here even if the original gerber was
            // clockwise. This means we do not have to do any conversion for the display even if the original angle was clockwise. 

            // get our start and sweep angles
            MiscGraphicsUtils.GetCounterClockwiseStartAndSweepAnglesFrom2PointsAndCenter(startPoint, endPoint, centerPoint, out float startAngleInDegrees, out float sweepAngleInDegrees);

            //DebugMessage("  PGa startAngleInDegrees=" + startAngleInDegrees.ToString() + ", sweepAngleInDegrees=" + sweepAngleInDegrees.ToString());

            // are we dealing with a sweep angle of 0 degrees?
            if(sweepAngleInDegrees==0)
            {
                // yes, this actually may be a full circle or it may just be a tiny little arc segment with an angle so small
                // that it cannot be drawn. We can get an idea by looking at the number of isoPlotCells the arc uses. A small
                // number indicates that we are actually a circle
                if (IsoPlotCellsInObject >= ARBITRARY_MIN_NUMBER_OF_CELLS_FOR_0_DEGREE_ARC) sweepAngleInDegrees = 360;
            }

            // GetPen, it will have been set up to have the proper width and color
            workingPen = stateMachine.PlotBorderPen;
            workingBrush = stateMachine.PlotBorderBrush;

            // create an enclosing rectangle 
            RectangleF boundingRect = new RectangleF(arcCenterXCoord - radius, arcCenterYCoord - radius, 2 * radius, 2 * radius);

            //DebugTODO("remove this");
            //if (DebugID == 23) workingPen = Pens.Red;
            //if (DebugID == 25) workingPen = Pens.Green;
            //if (DebugID == 20) workingPen = Pens.Blue;
            //if (DebugID != 9)
            //{
            //    return PlotActionEnum.PlotAction_Continue;
            //}

            // Draw arc
            graphicsObj.DrawArc(workingPen, boundingRect, startAngleInDegrees, sweepAngleInDegrees);

            // if we did not do nothing or a full circle draw these in
            if (((sweepAngleInDegrees == 0) || (sweepAngleInDegrees == 360)) == false)
            {
                // now draw in the circular end points of the line
                MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, startXCoord, startYCoord, workingPen.Width, workingPen.Width);
                MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, endXCoord, endYCoord, workingPen.Width, workingPen.Width);
            }

            return PlotActionEnum.PlotAction_Continue;

        }
    }
}

