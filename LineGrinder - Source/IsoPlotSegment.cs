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
    /// <history>
    ///    30 Jul 10  Cynic - Started
    /// </history>
    public abstract class IsoPlotSegment : OISObjBase
    {
        private Pen linePen = Pens.White;
        private Pen endPointPen = Pens.Red;

        private bool segmentChained = false;
        private bool needFlipOnConversionToGCode = false;

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        // these are the endpoints of the segment
        protected int x0 = -1;
        protected int y0 = -1;
        protected int x1 = -1;
        protected int y1 = -1;

        // this is returned by the plotting actions to
        // indicate if the plotting can continue
        public enum PlotActionEnum
        {
            PlotAction_Continue,
            PlotAction_FailWithError,
            PlotAction_End,
        }

        // this tells the IsoPlotSegmentCalculator
        // which kind of IsoPlotSegment to create
        public enum IsoPlotSegmentCalculatorTypeEnum
        {
            IsoPlotSegmentCalculator_Line,
            IsoPlotSegmentCalculator_Arc,
        }
        
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public IsoPlotSegment()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the object for chaining calculations
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public void ResetForChaining()
        {
            segmentChained = false;
            needFlipOnConversionToGCode = false;
        }
        
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether the segment needs to flip X0,Y0 and X1,Y1 when
        /// we convert to GCode
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public bool NeedFlipOnConversionToGCode
        {
            get
            {
                return needFlipOnConversionToGCode;
            }
            set
            {
                needFlipOnConversionToGCode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether the segment has been chained
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public bool SegmentChained
        {
            get
            {
                return segmentChained;
            }
            set
            {
                segmentChained = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current x0 value
        /// </summary>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public int X0
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
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public int Y0
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
        public int X1
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
        public int Y1
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
        /// Gets/Sets the pen we use for lines
        /// </summary>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
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
        /// Converts the contents of the isoPlotSegment to an appropriate GCodeLine object
        /// </summary>
        /// <returns>list of GCodeLine objects or null for fail</returns>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        ///    27 Sep 10  Cynic - Converted to return a list of GCodeLine objects
        /// </history>
        public abstract List<GCodeLine> GetGCodeLines(GCodeFileStateMachine stateMachine);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the action the plot GCode code action required based on the current context
        /// </summary>
        /// <param name="graphicsObj">a graphics object on which to plot</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public virtual IsoPlotSegment.PlotActionEnum PerformPlotIsoStep3Action(Graphics graphicsObj, ref int errorValue, ref string errorString)
        {
            // ignore this
            errorValue = 0;
            errorString = "";
            return PlotActionEnum.PlotAction_Continue;
        }

    }
}
