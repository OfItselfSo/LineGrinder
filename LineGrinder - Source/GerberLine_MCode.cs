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
    /// A class to encapsulate a gerber M Code
    /// </summary>
    /// <history>
    ///    07 Jul 10  Cynic - Started
    /// </history>
    public class GerberLine_MCode : GerberLine
    {

        private int currentMCode = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public GerberLine_MCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current D Code value
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
        public int CurrentMCode
        {
            get
            {
                return currentMCode;
            }
            set
            {
                currentMCode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the action the plot requires based on the current context
        /// </summary>
        /// <param name="graphicsObj">a graphics object on which to plot</param>
        /// <param name="stateMachine">the gerber plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <returns>an enum value indicating what next action to take</returns>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public override GerberLine.PlotActionEnum PerformPlotGerberAction(Graphics graphicsObj, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {
            // all of the M codes are just plot terminations
            errorValue = 0;
            errorString = "Successful End";
            return GerberLine.PlotActionEnum.PlotAction_End;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Parses out the line and gets the required information from it
        /// </summary>
        /// <param name="processedLineStr">a line string without block terminator or format parameters</param>
        /// <param name="stateMachine">The state machine containing the implied modal values</param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public override int ParseLine(string processedLineStr, GerberFileStateMachine stateMachine)
        {
            int outInt = -1;
            int nextStartPos = 0;
            bool retBool;

            //LogMessage("ParseLine(M) started");

            if (processedLineStr == null) return 100;
            if (processedLineStr.StartsWith("M") == false)
            {
                return 200;
            }

            // now the MCode line will have an M tag

            // LOOK FOR THE M TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'M', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // this is not an error - just means we did not find one
            }
            else
            {
                // this will have a float number
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetInteger(processedLineStr, nextStartPos, ref outInt, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("ParseLine(M) failed on call to ParseNumberFromString_TillNonDigit_RetInteger");
                    return 333;
                }
                else
                {
                    // set the value now
                    currentMCode = outInt;
                }
            }

            return 0;
        }

    }
}
