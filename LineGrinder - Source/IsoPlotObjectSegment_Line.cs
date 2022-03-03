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
    public class IsoPlotObjectSegment_Line : IsoPlotObjectSegment
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

        // if we create a new debug Id we start it from here
        public const int DEFAULT_NEW_DEBUG_ID_MULTIPLIER = 100000;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0In">X0 coord</param>
        /// <param name="y0In">Y0 coord</param>
        /// <param name="x1In">X1 coord</param>
        /// <param name="y1In">Y1 coord</param>
        /// <param name="isoPlotObjID">the isoplot builder ID this segment represents</param>
        public IsoPlotObjectSegment_Line(int isoPlotObjID, int x0In, int y0In, int x1In, int y1In) : base(isoPlotObjID)
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
        /// Gets the length (in application units), this is a calculated
        /// value a call to CalcIsoSegmentLength must be made first
        /// </summary>
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
        public float CalcIsoSegmentLength(GCodeFileStateMachine stateMachine)
        {
            float fX0;
            float fY0;
            float fX1;
            float fY1;
            if(stateMachine==null) return 0;
            fX0 = ((float)X0);
            fY0 = ((float)Y0);
            fX1 = ((float)X1);
            fY1 = ((float)Y1);
            isoSegmentLength = (float)Math.Sqrt(((fX1 - fX0) * (fX1 - fX0)) + ((fY1 - fY0) * (fY1 - fY0)));
            return isoSegmentLength;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides ToString()
        /// </summary>
        public override string ToString()
        {
            return "IsoPlotSegment_Line (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We cannot always accept the number of tabs that have been applied. This
        /// tests this here
        /// </summary>
        /// <returns>list of GCodeCmd objects or null for fail</returns>
        public int CalcAdjustedNumberOfTabs(GCodeFileStateMachine stateMachine)
        {
            int adjustedNumTabs;
            
            // calc the length of a tab in isoplot units
            float TabLenInIsoPlotUnits = TabLength * stateMachine.IsoPlotPointsPerAppUnit;

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
                    accumulatedTabLen += TabLenInIsoPlotUnits;
                    if (accumulatedTabLen >= halfSegLen) break;
                    // we can safely add this tab
                    adjustedNumTabs++;
                }
            }
            return adjustedNumTabs;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the contents of the isoPlotSegment to an appropriate GCodeCmd object
        /// </summary>
        /// <returns>list of GCodeCmd objects or null for fail</returns>
        public override List<GCodeCmd> GetGCodeCmds(GCodeFileStateMachine stateMachine)
        {
            int adjustedNumTabs;
            GCodeCmd_Line lineObj = null;
            GCodeCmd_Line lineObjSegment = null;
            List<GCodeCmd> retList = new List<GCodeCmd>();
            float fX0;
            float fY0;
            float fX1;
            float fY1;
            List<PointF> ptListMidCenters = new List<PointF>();

            // build the line now
            if (ReverseOnConversionToGCode == true)
            {
                // NOTE we use the chainedStart_X as point x1,y1 here.
                lineObj = new GCodeCmd_Line(x0, y0, ChainedStart_X, ChainedStart_Y, this.PointsAddedCount, this.DebugID);
                fX0 = (float)x0;
                fY0 = (float)y0;
                fX1 = (float)ChainedStart_X;
                fY1 = (float)ChainedStart_Y;
                lineObj.ReverseOnConversionToGCode = true;
            }
            else
            {
                // NOTE we use the chainedStart_X as point x0,y0 here.
                lineObj = new GCodeCmd_Line(ChainedStart_X, ChainedStart_Y, x1, y1, this.PointsAddedCount, this.DebugID);
                fX0 = (float)ChainedStart_X;
                fY0 = (float)ChainedStart_Y;
                fX1 = (float)x1;
                fY1 = (float)y1;
                lineObj.ReverseOnConversionToGCode = false;
            }
            lineObj.DoNotEmitToGCode = DoNotEmitToGCode;

            //DebugMessage("IsoSegmentLength=" + IsoSegmentLength(stateMachine).ToString());

            // figure out our adjusted number of tabs
            adjustedNumTabs = 0;
            if (NumberOfTabs <= 0) adjustedNumTabs = 0;
            else
            {
                adjustedNumTabs = CalcAdjustedNumberOfTabs(stateMachine);
            }

            //DebugMessage("NumberOfTabs=" + NumberOfTabs.ToString());
            //DebugMessage("adjustedNumTabs=" + adjustedNumTabs.ToString());
            //DebugMessage("TabLength=" + TabLength.ToString());

            // do we need to break the line to add tabs?
            if ((NumberOfTabs <= 0) || (adjustedNumTabs<=0) || (TabLength<=0))
            {
                // no we do not, add it and return it as a single line in a list
                retList.Add(lineObj);
                return retList;
            }

            // calc the length of a tab in isoplot units
            float TabLenInIsoPlotUnits = TabLength * stateMachine.IsoPlotPointsPerAppUnit;
            // we use half of half of the Isolation width to compensate for the width of the bit
            float millRadiusCompensatedTabLenInIsoPlotUnits = TabLenInIsoPlotUnits + ((stateMachine.IsolationWidth * stateMachine.IsoPlotPointsPerAppUnit) / 2);
            // this is the amount of a segment repressented by a tab
            float pctOfSegmentInASingleTab = millRadiusCompensatedTabLenInIsoPlotUnits / IsoSegmentLength;
            // we deduct half a tab width from the midpoint to calc the new start and end points
            float pctOfSegmentInASingleHalfTab = pctOfSegmentInASingleTab / 2;

            // set this now
            int numSegments = adjustedNumTabs + 1;

            // figure out the set of points starting and ending each line segment
            // add the original line start point
            ptListMidCenters.Add(new PointF(fX0, fY0));

            // this is based on the line partitioning algorythm courtesy of Tom Sirgedas
            // https://stackoverflow.com/questions/3542402/partition-line-into-equal-parts
            for (float i = 1; i < numSegments; i++)
            {
                float weightedAverage = (i / numSegments);
                float workingNearSidePct = pctOfSegmentInASingleHalfTab;
                float workingFarSidePct = pctOfSegmentInASingleHalfTab;

                // calc the start of the segment
                float startX = fX0 * (1 - (weightedAverage - workingNearSidePct)) +fX1 * (weightedAverage - workingNearSidePct);
                float startY = fY0 * (1 - (weightedAverage - workingNearSidePct)) +fY1 * (weightedAverage - workingNearSidePct);
                // calc the end of the segment
                float endX = fX0 * (1 - (weightedAverage + workingFarSidePct)) +fX1 * (weightedAverage + workingFarSidePct);
                float endY = fY0 * (1 - (weightedAverage + workingFarSidePct)) +fY1 * (weightedAverage + workingFarSidePct);
                // add the point about this center
                ptListMidCenters.Add(new PointF(startX, startY));
                ptListMidCenters.Add(new PointF(endX, endY));
            }

            // add the original line end point
            ptListMidCenters.Add(new PointF(fX1, fY1));

            // by this point we have a list of points, each pair represents a line we should draw
            // now we go through the points list and build our lines from them
            for (int i=0;i<ptListMidCenters.Count;i+=2)
            {
                float startX = ptListMidCenters[i].X;
                float startY = ptListMidCenters[i].Y;
                float endX = ptListMidCenters[i+1].X;
                float endY = ptListMidCenters[i+1].Y;

                // convert to ints
                int startXInt = (int)Math.Round(startX, 0);
                int startYInt = (int)Math.Round(startY, 0);
                int endXInt = (int)Math.Round(endX, 0);
                int endYInt = (int)Math.Round(endY, 0);

                // create a new line, give it a debugID based off the original full length one, and copy other info in as well
                lineObjSegment = new GCodeCmd_Line(startXInt, startYInt, endXInt, endYInt, this.PointsAddedCount, (int)((this.DebugID * DEFAULT_NEW_DEBUG_ID_MULTIPLIER) + i));
                lineObjSegment.ReverseOnConversionToGCode = this.ReverseOnConversionToGCode;
                // add it
                retList.Add(lineObjSegment);
            }

            // was the original line object reversed? If so we have to reverse the order of the segments
            if (this.ReverseOnConversionToGCode == true)
            {
                retList.Reverse();            
            }

            // return this
            return retList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// provide a sorting algorythm on the IsoSegmentLength
        /// </summary>
        /// <returns>an int indicating the sort order</returns>
        public static Comparison<IsoPlotObjectSegment_Line> LengthComparison =
           delegate(IsoPlotObjectSegment_Line p1, IsoPlotObjectSegment_Line p2)
           {
               // p2 and p1 are reversed here so we sort descending
               return p2.IsoSegmentLength.CompareTo(p1.IsoSegmentLength);
           };
    }
}

