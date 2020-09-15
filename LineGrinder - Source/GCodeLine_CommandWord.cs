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
    /// A class to encapsulate a GCode single word command. Lots of GCode commands
    /// are single words "G20", "G90", "M05" etc. Rather than make a separate class
    /// for each of these we just use this utility class
    /// </summary>
    /// <history>
    ///    06 Aug 10  Cynic - Started
    /// </history>
    public class GCodeLine_CommandWord : GCodeLine
    {
        private const string DEFAULT_COMMAND_WORD = GCODEWORD_PROGRAMEND;
        private string commandWord = DEFAULT_COMMAND_WORD;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public GCodeLine_CommandWord() : base()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commandWordIn">The command word</param>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public GCodeLine_CommandWord(string commandWordIn)
            : base()
        {
            commandWord = commandWordIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commandWordIn">The command word</param>
        /// <param name="commentTextIn">The comment text</param>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public GCodeLine_CommandWord(string commandWordIn, string commentTextIn)
            : base()
        {
            commandWord = commandWordIn;
            commentText = commentTextIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the command Word. Will never get or set NULL
        /// </summary>
        /// <history>
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public string CommandWord
        {
            get
            {
                if (commandWord == null) commandWord = DEFAULT_COMMAND_WORD;
                return commandWord;
            }
            set
            {
                commandWord = value;
                if (commandWord == null) commandWord = DEFAULT_COMMAND_WORD;
            }
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
            sb.Append("GCodeLine_CommandWord: " + CommandWord);
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
        ///    06 Aug 10  Cynic - Started
        /// </history>
        public override string GetGCodeLine(GCodeFileStateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                LogMessage("GetGCodeLine: stateMachine == null");
                return "M5 ERROR: stateMachine==null";
            }

            StringBuilder sb = new StringBuilder();
            if (stateMachine.GCodeFileManager.ShowGCodeLineNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
            sb.Append(CommandWord);
            if (CommentIsPresent == true)
            {
                sb.Append(" " + GCODEWORD_COMMENTOPEN + CommentText + GCODEWORD_COMMENTCLOSE);
            }
            sb.Append(stateMachine.LineTerminator);
            return sb.ToString();
        }
    }
}
