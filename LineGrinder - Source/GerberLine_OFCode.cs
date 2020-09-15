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
    /// A class to encapsulate a gerber OF Code
    /// </summary>
    /// <history>
    ///    22 Sep 10  Cynic - Started
    /// </history>
    public class GerberLine_OFCode : GerberLine
    {
        public const float DEFAULT_XOFFSET = 0f;
        public const float DEFAULT_YOFFSET = 0f;

        private float xOffset = DEFAULT_XOFFSET;
        private float yOffset = DEFAULT_YOFFSET;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        /// <history>
        ///    22 Sep 10  Cynic - Started
        /// </history>
        public GerberLine_OFCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current xOffset. There is no set accessor
        /// as this is derived from the OFCode Line.
        /// </summary>
        /// <history>
        ///    22 Sep 10  Cynic - Started
        /// </history>
        public float XOffset
        {
            get
            {
                return xOffset;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current yOffset. There is no set accessor
        /// as this is derived from the OFCode Line.
        /// </summary>
        /// <history>
        ///    22 Sep 10  Cynic - Started
        /// </history>
        public float YOffset
        {
            get
            {
                return yOffset;
            }
        }

 
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Parses out the line and gets the required information from it
        /// </summary>
        /// <param name="processedLineStr">a line string without block terminator or format parameters</param>
        /// <param name="stateMachine">The state machine containing the implied modal values</param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    22 Sep 10  Cynic - Started
        /// </history>
        public override int ParseLine(string processedLineStr, GerberFileStateMachine stateMachine)
        {
            float outFloat = 0;
            int nextStartPos = 0;
            bool retBool;

            LogMessage("ParseLine(OF) started");

            if (processedLineStr == null) return 100;
            if (processedLineStr.StartsWith(GerberFile.RS274OFPARAM) == false) 
            {
                return 200;
            }

            // now the line will have some combination of A and B tags in some order
            // LOOK FOR THE A TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'A', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // just means not found
            }
            else
            {
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(processedLineStr, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                else
                {
                    // set the value now
                    xOffset = outFloat;
                }

            }

            // LOOK FOR THE B TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'B', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // just means not found
            }
            else
            {
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(processedLineStr, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                else
                {
                    // set the value now
                    yOffset = outFloat;
                }

            }
            return 0;
        }
    }
}
