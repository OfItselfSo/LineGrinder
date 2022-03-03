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
    /// A abstract base class for isolation plot segments
    /// </summary>
    public abstract class IsoPlotObjectSegment : OISObjBase
    {
        private Pen linePen = Pens.White;
        private Pen endPointPen = Pens.Red;


        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in isoplot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        // these are the endpoints of the segment
        protected int x0 = -1;
        protected int y0 = -1;
        protected int x1 = -1;
        protected int y1 = -1;

        // these are the overlay objects underneath the x0,y0 and x1,y1 points. We could easily get these 
        // from the isoplot array but for efficiency they are stored here. We check these constantly 
        // when it comes time to create the chains
        protected Overlay xy0Overlay = null;
        protected Overlay xy1Overlay = null;

        // these are flags which let us know if anybody has chained to us on the xy0 or xy1 points
        // if so that point xy0 or xy1 cannot be added to another chain
        private bool pointIsChainTarget_0 = false;
        private bool pointIsChainTarget_1 = false;
        private bool reverseOnConversionToGCode = false;

        // when constructing the chains for gcode generation the start point can be a bit off
        // of the real start point (due to rounding) this is where we should start from if
        // creating gcode. Note this is subject to reversal as usual
        private int chainedStart_X = -1;
        private int chainedStart_Y = -1;

        private int pointsAddedCount = 0;

        private int isoPlotObjID = -1;
        private int debugID = -1;
        // we can be added to a chain by overlay or by distance
        // this is for diagnostics
        private bool chainedViaDistance = false;
        private bool isDuplicate = false;
        private bool doNotEmitToGCode = false;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isoPlotObjIDIn">the isoPlot builderID this segment represents</param>
        public IsoPlotObjectSegment(int isoPlotObjIDIn)
        {
            isoPlotObjID = isoPlotObjIDIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the object for chaining calculations
        /// </summary>
        public void ResetForChaining()
        {
            pointIsChainTarget_0 = false;
            pointIsChainTarget_1 = false;
            reverseOnConversionToGCode = false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the debugID
        /// </summary>
        public int DebugID
        {
            get
            {
                return debugID;
            }
            set
            {
                debugID = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the chainedViaDistance
        /// </summary>
        public bool ChainedViaDistance
        {
            get
            {
                return chainedViaDistance;
            }
            set
            {
                chainedViaDistance = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isDuplicate
        /// </summary>
        public bool IsDuplicate
        {
            get
            {
                return isDuplicate;
            }
            set
            {
                isDuplicate = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the doNotEmitToGCode flag
        /// </summary>
        public bool DoNotEmitToGCode
        {
            get
            {
                return doNotEmitToGCode;
            }
            set
            {
                doNotEmitToGCode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether the segment needs to flip X0,Y0 and X1,Y1 when
        /// we convert to GCode
        /// </summary>
        public bool ReverseOnConversionToGCode
        {
            get
            {
                return reverseOnConversionToGCode;
            }
            set
            {
                reverseOnConversionToGCode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether the segment has been chained at either the star or the end
        /// </summary>
        public bool SegmentChained
        {
            get
            {
                if (pointIsChainTarget_0 == true) return true;
                if (pointIsChainTarget_1 == true) return true;
                return false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We add a new X and Y to the segment. If we have not yet seen a start val
        /// we update that. We always update the end value.
        /// </summary>
        /// <param name="xVal">the x coord in the isoplot array</param>
        /// <param name="yVal">the y coord in the isoplot array</param>
        /// <param name="overlayObjIn">the overlay object</param>
        public void AddNewXAndYToSegment(int xVal, int yVal, Overlay overlayObjIn)
        {
            // do we need to add to the start?
            if ((x0 < 0) || (y0 < 0))
            {
                x0 = xVal;
                y0 = yVal;
                xy0Overlay = overlayObjIn;
            }
            // always do this
            x1 = xVal;
            y1 = yVal;
            xy1Overlay = overlayObjIn;
            // count it
            pointsAddedCount++;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Counts the number of points added
        /// </summary>
        public int PointsAddedCount
        {
            get
            {
                return pointsAddedCount;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the segment is a point (start and end are equal)
        /// </summary>
        public bool SegmentIsPoint
        {
            get
            {
                if (x0 != x1) return false;
                if (y0 != y1) return false;
                return true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the segment is used (has valid start and stop points)
        /// </summary>
        public bool SegmentIsUsed
        {
            get
            {
                if (x0 < 0) return false;
                if (y0 < 0) return false;
                if (x1 < 0) return false;
                if (y1 < 0) return false;
                return true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current chainedStart_X value
        /// </summary>
        public int ChainedStart_X
        {
            get
            {
                return chainedStart_X;
            }
            set
            {
                chainedStart_X = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current chainedStart_Y value
        /// </summary>
        public int ChainedStart_Y
        {
            get
            {
                return chainedStart_Y;
            }
            set
            {
                chainedStart_Y = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current pointIsChainTarget_0 value
        /// </summary>
        public bool PointIsChainTarget_0
        {
            get
            {
                return pointIsChainTarget_0;
            }
            set
            {
                pointIsChainTarget_0 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current pointIsChainTarget_1 value
        /// </summary>
        public bool PointIsChainTarget_1
        {
            get
            {
                return pointIsChainTarget_1;
            }
            set
            {
                pointIsChainTarget_1 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current xy0Overlay. 
        /// </summary>
        public Overlay XY0Overlay
        {
            get
            {
                return xy0Overlay;
            }
        }
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current xy1Overlay. 
        /// </summary>
        public Overlay XY1Overlay
        {
            get
            {
                return xy1Overlay;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current x0 value. Use AddNewXAndYToSegment to set
        /// </summary>
        public int X0
        {
            get
            {
                return x0;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current y0 value. Use AddNewXAndYToSegment to set
        /// </summary>
        public int Y0
        {
            get
            {
                return y0;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current x1 value. Use AddNewXAndYToSegment to set
        /// </summary>
        public int X1
        {
            get
            {
                return x1;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current y1 value. Use AddNewXAndYToSegment to set
        /// </summary>
        public int Y1
        {
            get
            {
                return y1;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pen we use for lines
        /// </summary>
        public Pen LinePen
        {
            get
            {
                return linePen;
            }
            set
            {
                linePen = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pen we use for endpoints
        /// </summary>
        public Pen EndPointPen
        {
            get
            {
                return endPointPen;
            }
            set
            {
                endPointPen = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the contents of the isoPlotSegment to an appropriate GCodeCmd object
        /// </summary>
        /// <returns>list of GCodeCmd objects or null for fail</returns>
        public abstract List<GCodeCmd> GetGCodeCmds(GCodeFileStateMachine stateMachine);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Dumps an isoplot segment
        /// </summary>
        /// <param name="isoSeg">the isoSegment</param>
        /// <param name="prefixStr">a prefix string</param>
        public static string GetIsoPlotSegmentDump(IsoPlotObjectSegment isoSeg, string prefixStr)
        {
            StringBuilder sb = new StringBuilder();
            int x0;
            int y0;
            int x1;
            int y1;
            int cx;
            int cy;
            string segType = "";
            string arcDir = "";
            string revStr = "N";
            string startOverlayStr = "";
            string endOverlayStr = "";
            int originalStartX = 0;
            int originalStartY = 0;
            string chainedVia = "O";
            string dupStr = " ";

            if (prefixStr == null) prefixStr = "";

            if ((isoSeg is IsoPlotObjectSegment_Arc) == true)
            {
                segType = "Arc";
                if ((isoSeg as IsoPlotObjectSegment_Arc).WantClockWise == true) arcDir = "CW";
                else arcDir = "CC";
                cx = (isoSeg as IsoPlotObjectSegment_Arc).XCenter;
                cy = (isoSeg as IsoPlotObjectSegment_Arc).YCenter;
            }
            else
            {
                segType = "Line";
                arcDir = "";
                cx = 0;
                cy = 0;
            }

            if (isoSeg.ChainedViaDistance == true) chainedVia = "D";
            else chainedVia = "O";
            if (isoSeg.IsDuplicate == true) dupStr = "$";
            else dupStr = " ";

            if (isoSeg.ReverseOnConversionToGCode == true)
            {
                // we are reversed
                x0 = isoSeg.X1;
                y0 = isoSeg.Y1;
                originalStartX = isoSeg.X1;
                originalStartY = isoSeg.Y1;
                if (isoSeg.ChainedStart_X >= 0) x0 = isoSeg.ChainedStart_X;
                if (isoSeg.ChainedStart_Y >= 0) y0 = isoSeg.ChainedStart_Y;
                x1 = isoSeg.X0;
                y1 = isoSeg.Y0;
                revStr = "R";
                startOverlayStr = isoSeg.XY1Overlay.ToString();
                endOverlayStr = isoSeg.XY0Overlay.ToString();
            }
            else
            {
                // normal
                x0 = isoSeg.X0;
                y0 = isoSeg.Y0;
                originalStartX = isoSeg.X0;
                originalStartY = isoSeg.Y0;
                if (isoSeg.ChainedStart_X >= 0) x0 = isoSeg.ChainedStart_X;
                if (isoSeg.ChainedStart_Y >= 0) y0 = isoSeg.ChainedStart_Y;
                x1 = isoSeg.X1;
                y1 = isoSeg.Y1;
                revStr = "N";
                startOverlayStr = isoSeg.XY0Overlay.ToString();
                endOverlayStr = isoSeg.XY1Overlay.ToString();
            }

            // dump it out
            sb.Append(prefixStr + segType + " " + isoSeg.DebugID.ToString() + " " + revStr + chainedVia + dupStr);
            sb.Append(" (" + x0.ToString() + "," + y0.ToString() + ")  (" + x1.ToString() + "," + y1.ToString() + ")");
            sb.Append(" **[" + originalStartX.ToString() + "," + originalStartY.ToString() + "]  ");
            if ((isoSeg is IsoPlotObjectSegment_Arc) == true)
            {
                sb.Append(" "+ arcDir+ "   ^^[" + cx.ToString() + "," + cy.ToString() + "] " );
            }
            sb.Append(", startOverlay=" + startOverlayStr + ", endOverlay="+ endOverlayStr);
            return sb.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the effective start and end coords of a segment, this takes into 
        /// account reversals and ChainedStart modifications
        /// </summary>
        /// <param name="startX">the start x coord</param>
        /// <param name="startY">the start y coord</param>
        /// <param name="endX">the end x coord</param>
        /// <param name="endY">the end y coord</param>
        public void GetEffectiveStartAndEndCoords(out int startX, out int startY, out int endX, out int endY)
        {
            if (ReverseOnConversionToGCode == true)
            {
                // we are reversed
                startX = this.X1;
                startY = this.Y1;
                if (this.ChainedStart_X >= 0) startX = this.ChainedStart_X;
                if (this.ChainedStart_Y >= 0) startY = this.ChainedStart_Y;
                endX = this.X0;
                endY = this.Y0;
            }
            else
            {
                // normal
                startX = this.X0;
                startY = this.Y0;
                if (this.ChainedStart_X >= 0) startX = this.ChainedStart_X;
                if (this.ChainedStart_Y >= 0) startY = this.ChainedStart_Y;
                endX = this.X1;
                endY = this.Y1;
            }
        }

    }
}

