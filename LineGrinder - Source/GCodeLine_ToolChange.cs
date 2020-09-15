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
    /// A class to encapsulate a GCode T ToolChange command. 
    /// </summary>
    /// <history>
    ///    05 Sep 10  Cynic - Started
    /// </history>
    public class GCodeLine_ToolChange : GCodeLine
    {
        // the dwell time is measured in seconds
        public const int DEFAULT_TOOLNUMBER = 0;
        private int toolNumber = DEFAULT_TOOLNUMBER;

        private float drillDiameter = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    05 Sep 10  Cynic - Started
        /// </history>
        public GCodeLine_ToolChange() : base()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="toolNumberIn">The dwell time</param>
        /// <history>
        ///    05 Sep 10  Cynic - Started
        /// </history>
        public GCodeLine_ToolChange(int toolNumberIn)
            : base()
        {
            toolNumber = toolNumberIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="toolNumberIn">The dwell time</param>
        /// <param name="commentTextIn">The comment text</param>
        /// <history>
        ///    05 Sep 10  Cynic - Started
        /// </history>
        public GCodeLine_ToolChange(int toolNumberIn, string commentTextIn)
            : base()
        {
            toolNumber = toolNumberIn;
            commentText = commentTextIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the dwell time. This is measured in seconds, never gets/sets
        /// a value less than zero
        /// </summary>
        /// <history>
        ///    05 Sep 10  Cynic - Started
        /// </history>
        public int ToolNumber
        {
            get
            {
                return toolNumber;
            }
            set
            {
                toolNumber = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Drill diameter
        /// </summary>
        /// <history>
        ///    05 Sep 10  Cynic - Started
        /// </history>
        public float DrillDiameter
        {
            get
            {
                return drillDiameter;
            }
            set
            {
                drillDiameter = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides ToString()
        /// </summary>
        /// <history>
        ///    05 Sep 10  Cynic - Started
        /// </history>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("GCodeLine_ToolChange: " + toolNumber.ToString());
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
        /// <history>
        ///    05 Sep 10  Cynic - Started
        /// </history>
        public override string GetGCodeLine(GCodeFileStateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                LogMessage("GetGCodeLine: stateMachine == null");
                return "T ERROR: stateMachine==null";
            }

            StringBuilder sb = new StringBuilder();
            // for a tool change we must stop the spindle or EMC emits a really 
            // odd "cannot change tools with cutter radius compensation" error
            if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
            sb.Append(GCODEWORD_SPINDLESTOP);
            sb.Append(stateMachine.LineTerminator);

            if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
            sb.Append(GCODEWORD_TOOLSELECT + toolNumber.ToString());
            if (CommentIsPresent == true)
            {
                sb.Append(" " + GCODEWORD_COMMENTOPEN + CommentText + GCODEWORD_COMMENTCLOSE);
            }
            sb.Append(stateMachine.LineTerminator);

            // now force the tool change
            if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
            sb.Append(GCODEWORD_TOOLCHANGE);
            sb.Append(stateMachine.LineTerminator);

            if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
            sb.Append(GCODEWORD_SPINDLESTART_CW);
            sb.Append(stateMachine.LineTerminator);

            return sb.ToString();
        }
    }
}
