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
    public class IsoPlotObjectSegment_Arc : IsoPlotObjectSegment
    {
        int xCenter = int.MinValue;
        int yCenter = int.MinValue;
        int radius = int.MinValue;
        bool wantClockWise = false;
        bool isMultiQuadrantArc = false;

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
        /// <param name="isoPlotObjID">the isoplot builder ID this segment represents</param>
        /// <param name="radiusIn">the radius</param>
        /// <param name="wantClockWiseIn">the wantClockWise value</param>
        /// <param name="isMultiQuadrantArcIn">the arc was specified in gerber multiquadrant mode</param>
        public IsoPlotObjectSegment_Arc(int isoPlotObjID, int x0In, int y0In, int x1In, int y1In, int cx, int cy, int radiusIn, bool wantClockWiseIn, bool isMultiQuadrantArcIn) : base(isoPlotObjID)
        {
            x0 = x0In;
            y0 = y0In;
            x1 = x1In;
            y1 = y1In;
            xCenter = cx;
            yCenter = cy;
            radius = radiusIn;
            wantClockWise = wantClockWiseIn;
            isMultiQuadrantArc = isMultiQuadrantArcIn;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides ToString()
        /// </summary>
        public override string ToString()
        {
            return "IsoPlotSegment_Arc (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether the segment needs to pay attention to multi quadrant mode
        /// </summary>
        public bool IsMultiQuadrantArc
        {
            get
            {
                return isMultiQuadrantArc;
            }
            set
            {
                isMultiQuadrantArc = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current wantClockWise value
        /// </summary>
        public bool WantClockWise
        {
            get
            {
                return wantClockWise;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current xCenter value
        /// </summary>
        public int XCenter
        {
            get
            {
                return xCenter;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current yCenter value
        /// </summary>
        public int YCenter
        {
            get
            {
                return yCenter;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current radius value
        /// </summary>
        public int Radius
        {
            get
            {
                return radius;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the contents of the isoPlotSegment to an appropriate sequence of GCodeCmd objects
        /// </summary>
        /// <returns>list of GCodeCmd objects or null for fail</returns>
        public override List<GCodeCmd> GetGCodeCmds(GCodeFileStateMachine stateMachine)
        {
            GCodeCmd_Arc arcObj = null;

            List<GCodeCmd> retList = new List<GCodeCmd>();

            // build the arc now
            if (ReverseOnConversionToGCode == true)
            {
                // NOTE we use the chainedStart_X as point x1,y1 here.
                arcObj = new GCodeCmd_Arc(x0, y0, ChainedStart_X, ChainedStart_Y, xCenter, yCenter, radius, wantClockWise, isMultiQuadrantArc, this.PointsAddedCount, this.DebugID);
                arcObj.ReverseOnConversionToGCode = true;
            }
            else
            {
                // NOTE we use the chainedStart_X as point x0,y0 here.
                arcObj = new GCodeCmd_Arc(ChainedStart_X, ChainedStart_Y, x1, y1, xCenter, yCenter, radius, wantClockWise, isMultiQuadrantArc, this.PointsAddedCount, this.DebugID);
                arcObj.ReverseOnConversionToGCode = false;
            }
            arcObj.DoNotEmitToGCode = DoNotEmitToGCode;
            // return it
            retList.Add(arcObj);
            return retList;
        }

    }
}

