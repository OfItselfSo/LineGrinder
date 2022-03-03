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
    public class GCodeCmd_RapidMove : GCodeCmd
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
        private int x0 = -1;
        private int y0 = -1;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        public GCodeCmd_RapidMove(int x0In, int y0In) : base()
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
        public GCodeCmd_RapidMove(int x0In, int y0In, string commentTextIn) : base(commentTextIn)
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
            return "GCodeCmd_Move (" + x0.ToString() + "," + y0.ToString() + ")" + " rapidMoveHeight=" + rapidMoveHeight.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current rapid move height
        /// </summary>
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
        public override string GetGCodeCmd(GCodeFileStateMachine stateMachine)
        {
            StringBuilder sb = new StringBuilder();

            float zCoordForMove = -1;

            if (stateMachine == null)
            {
                LogMessage("GetGCodeCmd: stateMachine == null");
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
                if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
                sb.Append(GCODEWORD_MOVERAPID + " " + GCODEWORD_ZAXIS + zCoordForMove.ToString());
                stateMachine.LastGCodeZCoord = zCoordForMove;
                sb.Append(stateMachine.LineTerminator);
            }

            float gX0OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_X(stateMachine, x0), 3);
            float gY0OffsetCompensated_Raw = (float)Math.Round(ConvertIsoPlotCoordToGCodeOutputCoord_Y(stateMachine, y0), 3);

            float gX0OffsetCompensated_Rounded = (float)Math.Round(gX0OffsetCompensated_Raw, 3);
            float gY0OffsetCompensated_Rounded = (float)Math.Round(gY0OffsetCompensated_Raw, 3);


            // Now move quickly to the start, we know our Z is now ok and above the pcb
            if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
            sb.Append(GCODEWORD_MOVERAPID + " " + GCODEWORD_XAXIS + gX0OffsetCompensated_Rounded.ToString() + " " + GCODEWORD_YAXIS + gY0OffsetCompensated_Rounded.ToString());

            stateMachine.LastGCodeXCoord = gX0OffsetCompensated_Rounded;
            stateMachine.LastGCodeYCoord = gY0OffsetCompensated_Rounded;
            sb.Append(stateMachine.LineTerminator);

            return sb.ToString();
        }
    }
}

