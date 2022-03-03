using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
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
    /// Certain commands in a Gerber file can span multiple source lines. This
    /// class encapsulates the lines for a single Gerber Command.
    /// </summary>
    public class GerberCommandSourceLines : OISObjBase
    {
        // list of the strings in the command
        private List<string> commandList = new List<string>();
        private int lineNumber = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialLine">the first line of the file. Most of the time 
        /// this is all we need</param>
        /// <param name="lineNumberIn">the line number. For multiline commands
        /// the linenumber is the first line we see</param>
        public GerberCommandSourceLines(string initialLine, int lineNumberIn)
        {
            AddLine(initialLine);
            lineNumber = lineNumberIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if we are a single line command
        /// </summary>
        public bool IsSingleLine()
        {
            if (CommandList.Count() > 1) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the command list. There is no set accessor. We set the lines 
        /// individually
        /// </summary>
        public List<string> CommandList
        {
            get
            {
                if (commandList == null) commandList = new List<string>();
                return commandList;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the line number. There is no set accessor. We set this
        /// this when  we set the line
        /// </summary>
        public int LineNumber
        {
            get
            {
                return lineNumber;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a line. 
        /// </summary>
        public int AddLine(string lineToAdd)
        {
            if (lineToAdd == null) return 100;
            CommandList.Add(lineToAdd);
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Appends a line. 
        /// </summary>
        public int AppendLine(string lineToAppend)
        {
            if (lineToAppend == null) return 100;
            if (lineToAppend.Length == 0) return 0;
            // should not have this
            if (CommandList.Count == 0) return 200;
            // append it
            CommandList[CommandList.Count-1]= CommandList[CommandList.Count - 1]+ lineToAppend;
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the first line. Will never return null will return empty
        /// </summary>
        public string GetFirstLine()
        {
            if (CommandList.Count == 0) return "<no command data>";
            return CommandList[0];
        }
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Override the to string value
        /// </summary>
        public override string ToString()
        {
            return GetFirstLine();
        }
    }
}

