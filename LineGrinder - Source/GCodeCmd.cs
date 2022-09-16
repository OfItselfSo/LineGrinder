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
    /// A abstract base class for GCode File command lines
    /// </summary>
    public abstract class GCodeCmd : OISObjBase
    {

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
        public const string GCODEWORD_SETPOSITION = "G92";

        // all GCodeCmds can also include comments on the line 
        private const string DEFAULT_COMMENT_TEXT = "";
        protected string commentText = DEFAULT_COMMENT_TEXT;

        private const float INCHTOMM_CONVERSION_FACTOR = 25.4f;

        private int isoPlotCellsInObject = 0;

        // sometimes we need to draw the gcodes backwards to get the chaining
        // to be nice and consistent
        private bool reverseOnConversionToGCode = false;
        private bool doNotEmitToGCode = false;

        private int debugID = -1;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GCodeCmd()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isoPlotCellsInObjectIn">the number of isoplot cells used to build this object</param>
        /// <param name="debugIDIn">the debug id</param>
        public GCodeCmd(int isoPlotCellsInObjectIn, int debugIDIn)
        {
            isoPlotCellsInObject = isoPlotCellsInObjectIn;
            debugID = debugIDIn;
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
        /// Constructor
        /// </summary>
        /// <param name="commentTextIn">the comment text</param>
        public GCodeCmd(string commentTextIn)
        {
            commentText = commentTextIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides ToString()
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("GCodeCmd: ");
            if (CommentIsPresent == true)
            {
                sb.Append(" " + GCODEWORD_COMMENTOPEN + CommentText + GCODEWORD_COMMENTCLOSE);
            }
            return sb.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the flag that indicates if we are reversing the gcode 
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
        /// Gets the number of isoplot cells used to build this object
        /// </summary>
        public int IsoPlotCellsInObject
        {
            get
            {
                return isoPlotCellsInObject;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The coords in the GCodeCmds are stored in isoPlot format (ints) this
        /// makes it simple to plot it. The output GCode file needs real world 
        /// coordinates such as that used in the Gerber file. This proc performs
        /// the conversion.
        /// 
        /// This one is for the X coordinate since the X offset can differ from Y
        /// 
        /// </summary>
        /// <param name="stateMachine">the statemachine</param>
        /// <param name="xCoordToConvert">the coord to convert</param>
        public float ConvertIsoPlotCoordToGCodeOutputCoord_X(GCodeFileStateMachine stateMachine, int xCoordToConvert)
        {
            // basically we divide by the isoPlot scaling to get plot values. Note that the xCoordToConvert
            // will have been shifed so that all values are non negative. This puts the origin in the lower left
            // hand corner. If the user wanted the GCode output to be defined relative to the center of the plot
            // we must SUBTRACT it from the value here
            float interimX = (xCoordToConvert / stateMachine.IsoPlotPointsPerAppUnit) - stateMachine.GCodeOutputPlotOriginAdjust_X + (stateMachine.AbsoluteOffset_X / stateMachine.IsoPlotPointsPerAppUnit);

            // are we mirroring around a veritical axis?
            if(stateMachine.MirrorOnConversionToGCode == FlipModeEnum.X_Flip)
            {
                interimX = (interimX * -1);
                // if we are not offsetting the X origin this means the origin must be at 0,0. After mirroring our 
                // origin will be at 0,0 but all the coords will be negative from that. This effectively makes the gcode
                // start point in the lower right corner. If we at 2x the X center point adjust we can put it all back
                // properly
                if(stateMachine.GCodeOutputPlotOriginAdjust_X==0)
                {
                    // note stateMachine.GCodeOutputPlotOriginAdjust_X and stateMachine.GCodeMirrorAxisPlotCoord_X are both 
                    // the mid points of the gcode plot
                    interimX = interimX + (2 * stateMachine.GCodeMirrorAxisPlotCoord_X);
                }
            }

            // return the val converted to the proper coords
            interimX = ConvertCoordToDesiredUnitSystem(interimX, stateMachine.SourceUnits, stateMachine.OutputUnits);

            // apply the rezero factor if needed and return
            if (stateMachine.ApplyRezeroFactor == true) return interimX - stateMachine.RezeroFactor_X;
            else return interimX;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The coords in the GCodeCmds are stored in isoPlot format (ints) this
        /// makes it simple to plot it. The output GCode file needs real world 
        /// coordinates such as that used in the Gerber file. This proc performs
        /// the conversion.
        /// 
        /// This one is for the Y coordinate since the X offset can differ from Y
        /// 
        /// </summary>
        /// <param name="stateMachine">the statemachine</param>
        /// <param name="yCoordToConvert">the coord to convert</param>
        public float ConvertIsoPlotCoordToGCodeOutputCoord_Y(GCodeFileStateMachine stateMachine, int yCoordToConvert)
        {
            // basically we divide by the isoPlot scaling to get plot values. Note that the xCoordToConvert
            // will have been shifed so that all values are non negative. This puts the origin in the lower left
            // hand corner. If the user wanted the GCode output to be defined relative to the center of the plot
            // we must SUBTRACT it from the value here
            float interimY = (yCoordToConvert / stateMachine.IsoPlotPointsPerAppUnit) - stateMachine.GCodeOutputPlotOriginAdjust_Y + (stateMachine.AbsoluteOffset_Y / stateMachine.IsoPlotPointsPerAppUnit);

            // return the val converted to the proper coords
            interimY = ConvertCoordToDesiredUnitSystem(interimY, stateMachine.SourceUnits, stateMachine.OutputUnits);

            // apply the rezero factor if needed and return
            if (stateMachine.ApplyRezeroFactor == true) return interimY - stateMachine.RezeroFactor_Y;
            else return interimY;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the coordinates to a desired system of units (inch or mm). If the 
        /// coord is already in that system of units it does nothing.
        /// 
        /// </summary>
        /// <param name="coordVal">the coordinate to convert</param>
        /// <param name="unitsInUse">the units in use by the coordinate</param>
        /// <param name="unitsWanted">the units wanted</param>
        protected float ConvertCoordToDesiredUnitSystem(float coordVal, ApplicationUnitsEnum unitsInUse, ApplicationUnitsEnum unitsWanted)
        {
            // are we already good?
            if(unitsInUse == unitsWanted) return coordVal;
            if (unitsInUse == ApplicationUnitsEnum.INCHES) return coordVal * INCHTOMM_CONVERSION_FACTOR;
            if (unitsInUse == ApplicationUnitsEnum.MILLIMETERS) return coordVal / INCHTOMM_CONVERSION_FACTOR;
            throw new NotImplementedException("Unknown coordinate units");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the comment text. Will never get or set NULL
        /// </summary>
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
        /// <param name="stateMachine">the statemachine string</param>
        public abstract string GetGCodeCmd(GCodeFileStateMachine stateMachine);

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
        public virtual PlotActionEnum PerformPlotGCodeAction(Graphics graphicsObj, GCodeFileStateMachine stateMachine, bool wantEndPointMarkers, ref int errorValue, ref string errorString)
        {
            // ignore this
            errorValue = 0;
            errorString = "";
            return PlotActionEnum.PlotAction_Continue;
        }

    }
}

