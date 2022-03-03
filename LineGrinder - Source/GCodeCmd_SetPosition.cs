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
    /// A class to encapsulate a GCode move object. This is the set position
    /// </summary>
    public class GCodeCmd_SetPosition : GCodeCmd
    {

        // these are the specified points
        private int x0 = 0;
        private int y0 = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0In">X0 coord</param>
        /// <param name="y0In">Y0 coord</param>
        public GCodeCmd_SetPosition(int x0In, int y0In) : base()
        {
            x0 = x0In;
            y0 = y0In;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0In">X0 coord</param>
        /// <param name="y0In">Y0 coord</param>
        public GCodeCmd_SetPosition(int x0In, int y0In, string commentTextIn) : base(commentTextIn)
        {
            x0 = x0In;
            y0 = y0In;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides ToString()
        /// </summary>
        public override string ToString()
        {
            return "GCodeCmd_Move (" + X0.ToString() + "," + Y0.ToString() + ")";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current x0
        /// </summary>
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
        /// Gets/Sets the current y0
        /// </summary>
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
        /// Gets the GCode lines for this object as they would be written to the text file. Will 
        /// never return a null value. Can often return more than one line if the 
        /// current context in the stateMachine requires it
        /// </summary>
        /// <param name="stateMachine">the stateMachine</param>
        public override string GetGCodeCmd(GCodeFileStateMachine stateMachine)
        {
            StringBuilder sb = new StringBuilder();

            if (stateMachine == null)
            {
                LogMessage("GetGCodeCmd: stateMachine == null");
                return "M5 ERROR: stateMachine==null";
            }

            // calc some values
            float gX0OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_X(stateMachine, X0), 3);
            float gX0OffsetCompensated_Rounded = (float)Math.Round(gX0OffsetCompensated_Raw, 3);

            float gY0OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_Y(stateMachine, Y0), 3);
            float gY0OffsetCompensated_Rounded = (float)Math.Round(gY0OffsetCompensated_Raw, 3);

            // do we want line numbers
            if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
            // Now set the start position
            sb.Append(GCODEWORD_SETPOSITION + " ");
            sb.Append(GCODEWORD_XAXIS + gX0OffsetCompensated_Rounded.ToString() + " ");
            sb.Append(GCODEWORD_YAXIS + gY0OffsetCompensated_Rounded.ToString());
            if (CommentIsPresent == true)
            {
                sb.Append(" " + GCODEWORD_COMMENTOPEN + CommentText + GCODEWORD_COMMENTCLOSE);
            }
            sb.Append(stateMachine.LineTerminator);

            stateMachine.LastGCodeXCoord = gX0OffsetCompensated_Rounded;
            stateMachine.LastGCodeYCoord = gY0OffsetCompensated_Rounded;

            return sb.ToString();
        }
    }
}

