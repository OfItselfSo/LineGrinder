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
    /// A class to encapsulate a GCode move object. This is similar to a line
    /// but we perform a GCODEWORD_MOVERAPID and we pull our tool up off the
    /// work surface. The height is dependent on the MoveMode
    /// </summary>
    /// <history>
    ///    06 Aug 10  Cynic - Started
    /// </history>
    public class GCodeLine_RapidMove : GCodeLine
    {
        // this tells the gcode interpreter how high we move when we move
        public enum GCodeRapidMoveHeightEnum
        {
            GCodeRapidMoveHeight_ZCoordForMove,
            GCodeRapidMoveHeight_ZCoordForClear,
        }
        public const GCodeRapidMoveHeightEnum DEFAULT_RAPIDMOVEHEIGHT = GCodeRapidMoveHeightEnum.GCodeRapidMoveHeight_ZCoordForClear;
        private GCodeRapidMoveHeightEnum rapidMoveHeight = DEFAULT_RAPIDMOVEHEIGHT;

        // these are the endpoints of the line on which we move
        float x0 = -1;
        float y0 = -1;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public GCodeLine_RapidMove(float x0In, float y0In) : base()
        {
            x0 = x0In;
            y0 = y0In;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public GCodeLine_RapidMove(float x0In, float y0In, string commentTextIn) : base(commentTextIn)
        {
            x0 = x0In;
            y0 = y0In;
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
            return "GCodeLine_Move (" + x0.ToString() + "," + y0.ToString() + ")" + " rapidMoveHeight=" + rapidMoveHeight.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current x0 value
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public float Xa0
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
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public float Ya0
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
                return (float)Math.Round((x0 - this.PlotXCoordOriginAdjust),3);
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
                return (float)Math.Round((y0 - this.PlotYCoordOriginAdjust), 3);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current rapid move height
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public GCodeRapidMoveHeightEnum RapidMoveHeight
        {
            get
            {
                return rapidMoveHeight;
            }
            set
            {
                rapidMoveHeight = value;
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
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public override string GetGCodeLine(GCodeFileStateMachine stateMachine)
        {
            StringBuilder sb = new StringBuilder();
            float zCoordForMove = -1;

            if (stateMachine == null)
            {
                LogMessage("GetGCodeLine: stateMachine == null");
                return "M5 ERROR: stateMachine==null";
            }

            // set the height we wish to move at
            if (RapidMoveHeight == GCodeRapidMoveHeightEnum.GCodeRapidMoveHeight_ZCoordForClear)
            {
                zCoordForMove = stateMachine.ZCoordForClear;
            }
            else
            {
                zCoordForMove = stateMachine.ZCoordForMove;
            }

            // Is our Z Axis correctly set?
            if (Math.Round(stateMachine.LastGCodeZCoord, 3) != Math.Round(zCoordForMove, 3))
            {
                // no it is not, send it to the move depth
                if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVERAPID + " " + GCODEWORD_ZAXIS + zCoordForMove.ToString());
                stateMachine.LastGCodeZCoord = zCoordForMove;
                sb.Append(stateMachine.LineTerminator);
            }

            // Now move quickly to the start, we know our Z is now ok and above the pcb
            if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
            sb.Append(GCODEWORD_MOVERAPID + " " + GCODEWORD_XAXIS + gX0OffsetCompensated.ToString() + " " + GCODEWORD_YAXIS + gY0OffsetCompensated.ToString());

            stateMachine.LastGCodeXCoord = gX0OffsetCompensated;
            stateMachine.LastGCodeYCoord = gY0OffsetCompensated;
            sb.Append(stateMachine.LineTerminator);
            return sb.ToString();
        }
    }
}
