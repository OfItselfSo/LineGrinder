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
    /// A class to encapsulate a isolation plot segments which are arcs. These
    /// are objects which describe the lines which will need to be routed
    /// on the circuit board.
    /// </summary>
    /// <history>
    ///    30 Jul 10  Cynic - Started
    /// </history>
    public class IsoPlotSegment_Arc : IsoPlotSegment
    {
        int xCenter = int.MinValue;
        int yCenter = int.MinValue;
        int radius = int.MinValue;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public IsoPlotSegment_Arc(int x0In, int y0In, int x1In, int y1In) : base()
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
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public override string ToString()
        {
            return "IsoPlotSegment_Arc (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current xCenter value
        /// </summary>
        /// <history>
        ///    04 Aug 10  Cynic - Started
        /// </history>
        public int XCenter
        {
            get
            {
                return xCenter;
            }
            set
            {
                xCenter = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current yCenter value
        /// </summary>
        /// <history>
        ///    04 Aug 10  Cynic - Started
        /// </history>
        public int YCenter
        {
            get
            {
                return yCenter;
            }
            set
            {
                yCenter = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current radius value
        /// </summary>
        /// <history>
        ///    04 Aug 10  Cynic - Started
        /// </history>
        public int Radius
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
        /// Converts the contents of the isoPlotSegment to an appropriate GCodeLine object
        /// </summary>
        /// <returns>list of GCodeLine objects or null for fail</returns>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        ///    27 Sep 10  Cynic - Converted to return a list of GCodeLine objects
        /// </history>
        public override List<GCodeLine> GetGCodeLines(GCodeFileStateMachine stateMachine)
        {
            float fX0;
            float fY0;
            float fX1;
            float fY1;
            float fXCenter;
            float fYCenter;
            float fRadius;
            List<GCodeLine> retList = new List<GCodeLine>();

            // set this up now
            fXCenter = ((float)XCenter) / stateMachine.IsoPlotPointsPerAppUnit;
            fYCenter = ((float)YCenter) / stateMachine.IsoPlotPointsPerAppUnit;
            fRadius = ((float)Radius) / stateMachine.IsoPlotPointsPerAppUnit;

            // build this now
            GCodeLine_Arc arcObj = new GCodeLine_Arc(fXCenter, fYCenter, fRadius);

            // convert the plot coords in the isoPlotSegment
            if (this.NeedFlipOnConversionToGCode == true)
            {
                // flip them around
                fX0 = ((float)X1) / stateMachine.IsoPlotPointsPerAppUnit;
                fY0 = ((float)Y1) / stateMachine.IsoPlotPointsPerAppUnit;
                fX1 = ((float)X0) / stateMachine.IsoPlotPointsPerAppUnit;
                fY1 = ((float)Y0) / stateMachine.IsoPlotPointsPerAppUnit;
                arcObj.SetPointsForGCodeOutput(fX0, fY0, fX1, fY1);
                arcObj.SetPointsForGPlotOutput(fX1, fY1, fX0, fY0);
            }
            else
            {
                fX0 = ((float)X0) / stateMachine.IsoPlotPointsPerAppUnit;
                fY0 = ((float)Y0) / stateMachine.IsoPlotPointsPerAppUnit;
                fX1 = ((float)X1) / stateMachine.IsoPlotPointsPerAppUnit;
                fY1 = ((float)Y1) / stateMachine.IsoPlotPointsPerAppUnit;
                arcObj.SetPointsForGCodeOutput(fX0, fY0, fX1, fY1);
                arcObj.SetPointsForGPlotOutput(fX0, fY0, fX1, fY1);
            }

            // now set the direction
            if (this.NeedFlipOnConversionToGCode == true)
            {
                arcObj.ArcDirection = GCodeLine_Arc.GetOtherArcDirection(GCodeLine_Arc.DEFAULT_ARC_DIRECTION);
            }
            else
            {
                arcObj.ArcDirection = GCodeLine_Arc.DEFAULT_ARC_DIRECTION;
            }
            // return it
            retList.Add(arcObj);
            return retList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the action the plot requires based on the current context
        /// </summary>
        /// <param name="graphicsObj">a graphics object on which to plot</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <returns>an enum value indicating what next action to take</returns>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public override IsoPlotSegment.PlotActionEnum PerformPlotIsoStep3Action(Graphics graphicsObj, ref int errorValue, ref string errorString)
        {
            float sweepAngle = 0;
            Pen workingPen = null;

           // DebugMessage("PerformPlotIsoStep3Action: " + this.ToString());

            // GetPen
            workingPen = this.LinePen;
            Rectangle boundingRect = MiscGraphicsUtils.GetBoundingRectForCircle(xCenter, yCenter, radius);
            float startAngle = MiscGraphicsUtils.GetCCWAngleOfLineToXAxis(xCenter, yCenter, x0, y0);
            float endAngle = MiscGraphicsUtils.GetCCWAngleOfLineToXAxis(xCenter, yCenter, x1, y1);
         
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
            return PlotActionEnum.PlotAction_Continue;
        }
    }
}
