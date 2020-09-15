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
    /// A class to encapsulate a isolation plot segments which are lines. These
    /// are objects which describe the lines which will need to be routed
    /// on the circuit board.
    /// </summary>
    /// <history>
    ///    30 Jul 10  Cynic - Started
    /// </history>
    public class IsoPlotSegment_Line : IsoPlotSegment
    {
        // this is the number of "skips" we introduce into the line 
        // so as to place tabs in it. This is used for edge milling so 
        // the center piece remains attached to the larger board.
        public const int DEFAULT_NUMBER_OF_TABS = 0;
        private int numberOfTabs = DEFAULT_NUMBER_OF_TABS;

        // this is the length of the tab in application units
        public const float DEFAULT_TAB_LENGTH = 0.125f;
        private float tabLength = DEFAULT_TAB_LENGTH;

        // this is a calculated value a call to CalcIsoSegmentLength must be made first
        public float isoSegmentLength = -1;

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
        public IsoPlotSegment_Line(int x0In, int y0In, int x1In, int y1In) : base()
        {
            x0 = x0In;
            y0 = y0In;
            x1 = x1In;
            y1 = y1In;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets or sets the number of tabs, will never get or set <0
        /// </summary>
        /// <history>
        ///    27 Sep 10  Cynic - Started
        /// </history>
        public int NumberOfTabs
        {
            get
            {
                if (numberOfTabs < 0) numberOfTabs = 0;
                return numberOfTabs;
            }
            set
            {
                numberOfTabs = value;
                if (numberOfTabs < 0) numberOfTabs = 0;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets or sets the tab length (in application units), will never get or set <0
        /// </summary>
        /// <history>
        ///    27 Sep 10  Cynic - Started
        /// </history>
        public float TabLength
        {
            get
            {
                if (tabLength < 0) tabLength = 0;
                return tabLength;
            }
            set
            {
                tabLength = value;
                if (tabLength < 0) tabLength = 0;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets or sets the tab length (in application units), this is a calculated
        /// value a call to CalcIsoSegmentLength must be made first
        /// </summary>
        /// <history>
        ///    28 Sep 10  Cynic - Started
        /// </history>
        public float IsoSegmentLength
        {
            get
            {
                return isoSegmentLength;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the length of the isoplot segment (in application units)
        /// </summary>
        /// <history>
        ///    27 Sep 10  Cynic - Started
        /// </history>
        public float CalcIsoSegmentLength(GCodeFileStateMachine stateMachine)
        {
            float fX0;
            float fY0;
            float fX1;
            float fY1;
            if(stateMachine==null) return 0;
            fX0 = ((float)X0) / stateMachine.IsoPlotPointsPerAppUnit;
            fY0 = ((float)Y0) / stateMachine.IsoPlotPointsPerAppUnit;
            fX1 = ((float)X1) / stateMachine.IsoPlotPointsPerAppUnit;
            fY1 = ((float)Y1) / stateMachine.IsoPlotPointsPerAppUnit;
            isoSegmentLength = (float)Math.Sqrt(((fX1 - fX0) * (fX1 - fX0)) + ((fY1 - fY0) * (fY1 - fY0)));
            return isoSegmentLength;
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
            return "IsoPlotSegment_Line (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We cannot always accept the number of tabs that have been applied. This
        /// tests this here
        /// </summary>
        /// <returns>list of GCodeLine objects or null for fail</returns>
        /// <history>
        ///    28 Sep 10  Cynic - Started
        /// </history>
        public int CalcAdjustedNumberOfTabs(GCodeFileStateMachine stateMachine)
        {
            int adjustedNumTabs;
            // figure out our adjusted number of tabs
            adjustedNumTabs = 0;
            if (NumberOfTabs <= 0) adjustedNumTabs = 0;
            else
            {
                float accumulatedTabLen = 0;
                float halfSegLen = this.CalcIsoSegmentLength(stateMachine) / 2;
                // add our Number of tabs in ensuring the total sum of the 
                // tab lengths is not greater than half the total length
                for (int i = 0; i < NumberOfTabs; i++)
                {
                    accumulatedTabLen += TabLength;
                    if (accumulatedTabLen >= halfSegLen) break;
                    // we can safely add this tab
                    adjustedNumTabs++;
                }
            }
            return adjustedNumTabs;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the contents of the isoPlotSegment to an appropriate GCodeLine object
        /// </summary>
        /// <returns>list of GCodeLine objects or null for fail</returns>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        ///    27 Sep 10  Cynic - Converted to return a list of GCodeLine objects
        /// </history>
        public override List<GCodeLine> GetGCodeLines(GCodeFileStateMachine stateMachine)
        {
            float fX0;
            float fY0;
            float fX1;
            float fY1;
            float tmpX;
            float tmpY;
            int adjustedNumTabs;
            GCodeLine_Line lineObj = null;
            List<GCodeLine> retList = new List<GCodeLine>();
            List<PointF> ptListMidCenters = new List<PointF>();

            // convert the plot coords in the isoPlotSegment
            if (this.NeedFlipOnConversionToGCode == true)
            {
                // flip them around
                fX0 = ((float)X1) / stateMachine.IsoPlotPointsPerAppUnit;
                fY0 = ((float)Y1) / stateMachine.IsoPlotPointsPerAppUnit;
                fX1 = ((float)X0) / stateMachine.IsoPlotPointsPerAppUnit;
                fY1 = ((float)Y0) / stateMachine.IsoPlotPointsPerAppUnit;
            }
            else
            {
                fX0 = ((float)X0) / stateMachine.IsoPlotPointsPerAppUnit;
                fY0 = ((float)Y0) / stateMachine.IsoPlotPointsPerAppUnit;
                fX1 = ((float)X1) / stateMachine.IsoPlotPointsPerAppUnit;
                fY1 = ((float)Y1) / stateMachine.IsoPlotPointsPerAppUnit;
            }

           // DebugMessage("IsoSegmentLength=" + IsoSegmentLength(stateMachine).ToString());
            // figure out our adjusted number of tabs
            adjustedNumTabs = 0;
            if (NumberOfTabs <= 0) adjustedNumTabs = 0;
            else
            {
                adjustedNumTabs = CalcAdjustedNumberOfTabs(stateMachine);
            }

          //  DebugMessage("NumberOfTabs=" + NumberOfTabs.ToString());
          //  DebugMessage("adjustedNumTabs=" + adjustedNumTabs.ToString());
           // DebugMessage("TabLength=" + TabLength.ToString());

            // do we need to break the line to add tabs?
            if ((NumberOfTabs <= 0) || (adjustedNumTabs<=0) || (TabLength<=0))
            {
                // no we do not, add it and return it
                lineObj = new GCodeLine_Line(fX0, fY0, fX1, fY1);
               // DebugMessage("A1 " + lineObj.ToString());
                retList.Add(lineObj);
                return retList;
            }
            // ####
            // if we get here we need to break the line into sections
            // ####
            float xDist = (fX1 - fX0) / ((float)(adjustedNumTabs + 1));
            float yDist = (fY1 - fY0) / ((float)(adjustedNumTabs + 1));

            // init these now, the loop requires it
            tmpX = fX0;
            tmpY = fY0;

            // now add the intermediate points to the array
            for (int i = 0; i < adjustedNumTabs; i++)
            {
                // find the next point
                tmpX += xDist;
                tmpY += yDist;
                // add the point
                ptListMidCenters.Add(new PointF(tmpX, tmpY));
            }

            //DebugMessage("ptListMidCenters.Count=" + ptListMidCenters.Count.ToString());

            // at this point we have a list of mid point centers. We still
            // need to adjust these so they represent a definable width
            // figure out the angle
            float lineAngle = 0;
            lineAngle = (float)Math.Round(Math.Atan2((fY1 - fY0), (fX1 - fX0)), 3);
            // the offset will be half the distance of the TabLength projected onto
            // each axis, figure out the projected lengths, but the end point describes
            // the center of the bit, so we back it off by 1/2 a diameter
            float xProjLen = ((TabLength + stateMachine.ToolHeadSetup.ToolWidth) / 2 * (float)Math.Round(Math.Cos(lineAngle), 3));
            float yProjLen = ((TabLength + stateMachine.ToolHeadSetup.ToolWidth) / 2 * (float)Math.Round(Math.Sin(lineAngle), 3));
            //float xProjLen = (TabLength * (float)Math.Round(Math.Cos(lineAngle), 3)) + (stateMachine.ToolHeadSetup.ToolWidth / 2);
            //float yProjLen = (TabLength * (float)Math.Round(Math.Sin(lineAngle), 3)) + (stateMachine.ToolHeadSetup.ToolWidth / 2);
            // DebugMessage("lineAngle="+lineAngle.ToString()+" xProjLen=" + xProjLen.ToString() + " yProjLen=" + yProjLen.ToString());

            // now build the line segments and add them 
            tmpX = fX0;
            tmpY = fY0;
            foreach (PointF ptf in ptListMidCenters)
            {
            //    DebugMessage("Pointf x=" + ptf.X.ToString() + " Y=" + ptf.Y.ToString());
                lineObj = new GCodeLine_Line(tmpX, tmpY, ptf.X - xProjLen, ptf.Y - yProjLen);
            //    DebugMessage("A2 " + lineObj.ToString());
                retList.Add(lineObj);
                // set the new start points
                tmpX = ptf.X + xProjLen;
                tmpY = ptf.Y + yProjLen;
            }
            // add one last segment
            lineObj = new GCodeLine_Line(tmpX, tmpY, fX1, fY1);
            //DebugMessage("A3 " + lineObj.ToString());
            //DebugMessage("");
            retList.Add(lineObj);
            // return this
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
            Pen workingPen = null;

            //DebugMessage("PerformPlotIsoStep3Action: " + this.ToString());

            // GetPen
            workingPen = this.LinePen;
            // Draw line
            graphicsObj.DrawLine(workingPen, X0, Y0, X1, Y1);
            return PlotActionEnum.PlotAction_Continue;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// provide a sorting algorythm on the IsoSegmentLength
        /// </summary>
        /// <returns>an int inticating the sort order</returns>
        /// <history>
        ///    28 Sep 10  Cynic - Started
        /// </history>
        public static Comparison<IsoPlotSegment_Line> LengthComparison =
           delegate(IsoPlotSegment_Line p1, IsoPlotSegment_Line p2)
           {
               // p2 and p1 are reversed here so we sort descending
               return p2.IsoSegmentLength.CompareTo(p1.IsoSegmentLength);
           };
    }
}
