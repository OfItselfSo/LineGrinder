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
    /// A abstract base class for GCode File lines
    /// </summary>
    /// <history>
    ///    02 Aug 10  Cynic - Started
    /// </history>
    public abstract class GCodeLine : OISObjBase
    {

        // this is returned by the PerformPlotIsoStep1Action to 
        // indicate if the plotting can continue
        public enum PlotActionEnum
        {
            PlotAction_Continue,
            PlotAction_FailWithError,
            PlotAction_End,
        }

        // these are the GCode words we emit in our GCode files.
        public const string GCODEWORD_COMMENTOPEN = "(";
        public const string GCODEWORD_COMMENTCLOSE = ")";
        public const string GCODEWORD_LINENUMBER = "N";
        public const string GCODEWORD_FEEDRATE = "F";
        public const string GCODEWORD_XAXIS = "X";
        public const string GCODEWORD_YAXIS = "Y";
        public const string GCODEWORD_ZAXIS = "Z";
        public const string GCODEWORD_XARCCENTER = "I";
        public const string GCODEWORD_YARCCENTER = "J";
        public const string GCODEWORD_ARCRADIUS = "R";
        public const string GCODEWORD_MOVERAPID = "G00";
        public const string GCODEWORD_MOVEINLINE = "G01";
        public const string GCODEWORD_MOVEINCIRCLECW = "G02";
        public const string GCODEWORD_MOVEINCIRCLECCW = "G03";
        public const string GCODEWORD_DWELL = "G04";
        public const string GCODEWORD_TOOLCHANGE = "M6";
        public const string GCODEWORD_TOOLSELECT = "T";
        public const string GCODEWORD_UNIT_IN = "G20";
        public const string GCODEWORD_UNIT_MM = "G21";
        public const string GCODEWORD_XYPLANE = "G17";
        public const string GCODEWORD_COORDMODE_ABSOLUTE = "G90";
        public const string GCODEWORD_SPINDLESTART_CW = "M03";
        public const string GCODEWORD_SPINDLESTOP = "M05";
        public const string GCODEWORD_PROGRAMEND = "M02";

        // all GCodeLines can also include comments on the line 
        private const string DEFAULT_COMMENT_TEXT = "";
        protected string commentText = DEFAULT_COMMENT_TEXT;

        // These are the values ADDed to the existing X and Y coords from the Gerber
        // file DCodes in order to set the origin approximately at zero. We SUBTRACT
        // them from the X and Y coordinates in order to set the origin back to 
        // the original position in the output GCode file
        private float plotXCoordOriginAdjust = 0;
        private float plotYCoordOriginAdjust = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public GCodeLine()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commentTextIn">the comment text</param>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public GCodeLine(string commentTextIn)
        {
            commentText = commentTextIn;
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
            StringBuilder sb = new StringBuilder();
            sb.Append("GCodeLine: ");
            if (CommentIsPresent == true)
            {
                sb.Append(" " + GCODEWORD_COMMENTOPEN + CommentText + GCODEWORD_COMMENTCLOSE);
            }
            return sb.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current X Coord Origin Adjust value. These are compensating
        /// values ADDED to the gerber coordinates in order to make the
        /// smallest X coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// We SUBTRACT this value from the GCode parameters in order to put them 
        /// back to where they were.
        /// </summary>
        /// <history>
        ///    11 Aug 10  Cynic - Started
        /// </history>
        public float PlotXCoordOriginAdjust
        {
            get
            {
                return plotXCoordOriginAdjust;
            }
            set
            {
                plotXCoordOriginAdjust = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Y Coord Origin Adjust value. These are compensating
        /// values ADDED to the gerber coordinates in order to make the
        /// smallest Y coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// We SUBTRACT this value from the GCode parameters in order to put them 
        /// back to where they were.
        /// </summary>
        /// <history>
        ///    11 Aug 10  Cynic - Started
        /// </history>
        public float PlotYCoordOriginAdjust
        {
            get
            {
                return plotYCoordOriginAdjust;
            }
            set
            {
                plotYCoordOriginAdjust = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the comment text. Will never get or set NULL
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public string CommentText
        {
            get
            {
                if (commentText == null) commentText = DEFAULT_COMMENT_TEXT;
                return commentText;
            }
            set
            {
                commentText = value;
                if (commentText == null) commentText = DEFAULT_COMMENT_TEXT;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if we actually have some comment text
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public bool CommentIsPresent
        {
            get
            {
                if ((commentText != null) && (commentText.Length > 0)) return true;
                else return false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GCode line as it would be written to the text file. Will 
        /// never return a null value
        /// </summary>
        /// <param name="lineTerminatorIn">the line termination string</param>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public abstract string GetGCodeLine(GCodeFileStateMachine stateMachine);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the action the plot GCode code action required based on the current context
        /// </summary>
        /// <param name="graphicsObj">a graphics object on which to plot</param>
        /// <param name="stateMachine">the ggcode plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <param name="wantEndPointMarkers">if true we draw the endpoints of the gcodes
        /// in a different color</param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public virtual GCodeLine.PlotActionEnum PerformPlotGCodeAction(Graphics graphicsObj, GCodeFileStateMachine stateMachine, bool wantEndPointMarkers, ref int errorValue, ref string errorString)
        {
            // ignore this
            errorValue = 0;
            errorString = "";
            return PlotActionEnum.PlotAction_Continue;
        }

    }
}
