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
    /// A class to encapsulate a GCode G04 Dwell command. 
    /// </summary>
    public class GCodeCmd_Dwell : GCodeCmd
    {
        // the dwell time is measured in seconds
        public const float DEFAULT_DWELL_TIME = 1.0f;
        private float dwellTime = DEFAULT_DWELL_TIME;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GCodeCmd_Dwell() : base()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dwellTimeIn">The dwell time</param>
        public GCodeCmd_Dwell(float dwellTimeIn)
            : base()
        {
            dwellTime = dwellTimeIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dwellTimeIn">The dwell time</param>
        /// <param name="commentTextIn">The comment text</param>
        public GCodeCmd_Dwell(float dwellTimeIn, string commentTextIn)
            : base()
        {
            dwellTime = dwellTimeIn;
            commentText = commentTextIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the dwell time. This is measured in seconds, never gets/sets
        /// a value less than zero
        /// </summary>
        public float DwellTime
        {
            get
            {
                if (dwellTime < 0f) dwellTime = DEFAULT_DWELL_TIME ;
                return dwellTime;
            }
            set
            {
                dwellTime = value;
                if (dwellTime < 0f) dwellTime = DEFAULT_DWELL_TIME;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides ToString()
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("GCodeCmd_Dwell: " + dwellTime.ToString());
            if (CommentIsPresent == true)
            {
                sb.Append(" " + GCODEWORD_COMMENTOPEN + CommentText + GCODEWORD_COMMENTCLOSE);
            }
            return sb.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GCode line as it would be written to the text file. Will 
        /// never return a null value
        /// </summary>
        /// <param name="stateMachine">the stateMachine</param>
        public override string GetGCodeCmd(GCodeFileStateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                LogMessage("GetGCodeCmd: stateMachine == null");
                return "M5 ERROR: stateMachine==null";
            }

            StringBuilder sb = new StringBuilder();
            if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
            sb.Append(GCODEWORD_DWELL+" P"+dwellTime.ToString());
            if (CommentIsPresent == true)
            {
                sb.Append(" " + GCODEWORD_COMMENTOPEN + CommentText + GCODEWORD_COMMENTCLOSE);
            }
            sb.Append(stateMachine.LineTerminator);
            return sb.ToString();
        }
    }
}

