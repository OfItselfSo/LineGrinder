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
    /// A class to encapsulate a gerber G37 Code (Turn off Polygon Area Fill)
    /// </summary>
    /// <history>
    ///    15 Jan 11  Cynic - Started
    /// </history>
    public class GerberLine_G37Code : GerberLine
    {

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        /// <history>
        ///    15 Jan 11  Cynic - Started
        /// </history>
        public GerberLine_G37Code(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Parses out the line and gets the required information from it
        /// </summary>
        /// <param name="processedLineStr">a line string without block terminator or format parameters</param>
        /// <param name="stateMachine">The state machine containing the implied modal values</param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    15 Jan 11  Cynic - Started
        /// </history>
        public override int ParseLine(string processedLineStr, GerberFileStateMachine stateMachine)
        {
            //LogMessage("ParseLine(G37) started");

            if (processedLineStr == null) return 100;
            if (processedLineStr.StartsWith("G37") == false)
            {
                return 200;
            }

            // the G37 line should not have any parameters or follow on text, 
            // we assume this to be true. Just set the state machine so it knows
            // that the CurrentLinesAreNotForIsolation and more (turn it off)
            stateMachine.CurrentLinesAreNotForIsolation = false;
 
            return 0;
        }

    }
}