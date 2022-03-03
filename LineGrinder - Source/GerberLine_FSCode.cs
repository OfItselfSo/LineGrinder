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
    /// A class to encapsulate a gerber FS Code
    /// </summary>
    public class GerberLine_FSCode : GerberLine
    {

        // defines the states, makes them more readable
        public enum LeadingZeroModeEnum
        {
            OMIT_LEADING_ZEROS,
            OMIT_TRAILING_ZEROS
        };

        // this helps with the block parsing
        public enum ParsingHelperEnum
        {
            NEXT_IS_NONE,
            NEXT_IS_XINTEGER,
            NEXT_IS_XDECIMAL,
            NEXT_IS_YINTEGER,
            NEXT_IS_YDECIMAL,
        };

        public const LeadingZeroModeEnum DEFAULT_LEADING_ZERO_MODE = LeadingZeroModeEnum.OMIT_LEADING_ZEROS;
        private LeadingZeroModeEnum leadingZeroMode = DEFAULT_LEADING_ZERO_MODE;
        private GerberCoordinateModeEnum coordinateMode = GerberFileStateMachine.DEFAULT_COORDINATE_MODE;

        public const int DEFAULT_XINTEGER_PLACES = 2;
        public const int DEFAULT_XDECIMAL_PLACES = 3;
        public const int DEFAULT_YINTEGER_PLACES = 2;
        public const int DEFAULT_YDECIMAL_PLACES = 3;
        public const int DEFAULT_N_CODE_LENGTHS = 2;
        public const int DEFAULT_G_CODEL_LENGTHS = 2;
        public const int DEFAULT_D_CODE_LENGTHS = 2;
        public const int DEFAULT_M_CODE_LENGTHS = 2;

        private int xIntegerPlaces = DEFAULT_XINTEGER_PLACES;
        private int xDecimalPlaces = DEFAULT_XDECIMAL_PLACES;
        private int yIntegerPlaces = DEFAULT_YINTEGER_PLACES;
        private int yDecimalPlaces = DEFAULT_YDECIMAL_PLACES;
        private int nCodeLengths = DEFAULT_N_CODE_LENGTHS;
        private int gCodeLengths = DEFAULT_G_CODEL_LENGTHS;
        private int dCodeLengths = DEFAULT_D_CODE_LENGTHS;
        private int mCodeLengths = DEFAULT_M_CODE_LENGTHS;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        public GerberLine_FSCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current xIntegerPlaces. There is no set accessor
        /// as this is derived from the FSCode Line.
        /// </summary>
        public int XIntegerPlaces
        {
            get
            {
                return xIntegerPlaces;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current yIntegerPlaces. There is no set accessor
        /// as this is derived from the FSCode Line.
        /// </summary>
        public int YIntegerPlaces
        {
            get
            {
                return yIntegerPlaces;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current xDecimalPlaces. There is no set accessor
        /// as this is derived from the FSCode Line.
        /// </summary>
        public int XDecimalPlaces
        {
            get
            {
                return xDecimalPlaces;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current yDecimalPlaces. There is no set accessor
        /// as this is derived from the FSCode Line.
        /// </summary>
        public int YDecimalPlaces
        {
            get
            {
                return yDecimalPlaces;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current leading zero mode. There is no set accessor
        /// as this is derived from the header processing done after SourceLines is set.
        /// </summary>
        public LeadingZeroModeEnum LeadingZeroMode
        {
            get
            {
                return leadingZeroMode;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current coordinate mode. There is no set accessor
        /// as this is derived from the header processing done after SourceLines is set.
        /// </summary>
        public GerberCoordinateModeEnum CoordinateMode
        {
            get
            {
                return coordinateMode;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Parses out the line and gets the required information from it
        /// </summary>
        /// <param name="processedLineStr">a line string without block terminator or format parameters</param>
        /// <param name="stateMachine">The state machine containing the implied modal values</param>
        /// <returns>z success, nz fail</returns>
        public override int ParseLine(string processedLineStr, GerberFileStateMachine stateMachine)
        {
            ParsingHelperEnum parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;

            LogMessage("ParseFormatParameter() started");

            if (processedLineStr == null) return 100;
            if (processedLineStr.StartsWith(GerberFile.RS274_FS_CMD) == false) return 200;

            // convert to a character array
            char[] fsChars = processedLineStr.ToCharArray();
            if (fsChars == null) return 300;
            if (fsChars.Length < 2) return 400;
            for (int i = 2; i < fsChars.Length; i++)
            {
                switch (fsChars[i])
                {
                    case 'L':
                        leadingZeroMode = LeadingZeroModeEnum.OMIT_LEADING_ZEROS;
                        parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        continue;
                    case 'T':
                        leadingZeroMode = LeadingZeroModeEnum.OMIT_LEADING_ZEROS;
                        parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        continue;
                    case 'A':
                        coordinateMode = GerberCoordinateModeEnum.COORDINATE_ABSOLUTE;
                        parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        // set this as well
                        stateMachine.GerberFileCoordinateMode = GerberCoordinateModeEnum.COORDINATE_ABSOLUTE;
                        continue;
                    case 'I':
                        coordinateMode = GerberCoordinateModeEnum.COORDINATE_INCREMENTAL;
                        parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        stateMachine.GerberFileCoordinateMode = GerberCoordinateModeEnum.COORDINATE_INCREMENTAL;
                        continue;
                    case 'X':
                        parsingHelper = ParsingHelperEnum.NEXT_IS_XINTEGER;
                        continue;
                    case 'Y':
                        parsingHelper = ParsingHelperEnum.NEXT_IS_YINTEGER;
                        continue;
                    case 'N':
                        try { nCodeLengths = (int)char.GetNumericValue(fsChars[i + 1]); }
                        catch { };
                        parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        continue;
                    case 'G':
                        try { gCodeLengths = (int)char.GetNumericValue(fsChars[i + 1]); }
                        catch { };
                        parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        continue;
                    case 'D':
                        try { dCodeLengths = (int)char.GetNumericValue(fsChars[i + 1]); }
                        catch { };
                        parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        continue;
                    case 'M':
                        try { mCodeLengths = (int)char.GetNumericValue(fsChars[i + 1]); }
                        catch { };
                        parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        continue;
                    default:
                        if (parsingHelper == ParsingHelperEnum.NEXT_IS_XINTEGER)
                        {
                            try { xIntegerPlaces = (int)char.GetNumericValue(fsChars[i]); }
                            catch { };
                            parsingHelper = ParsingHelperEnum.NEXT_IS_XDECIMAL;
                        }
                        else if (parsingHelper == ParsingHelperEnum.NEXT_IS_XDECIMAL)
                        {
                            try { xDecimalPlaces = (int)char.GetNumericValue(fsChars[i]); }
                            catch { };
                            parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        }
                        else if (parsingHelper == ParsingHelperEnum.NEXT_IS_YINTEGER)
                        {
                            try { yIntegerPlaces = (int)char.GetNumericValue(fsChars[i]); }
                            catch { };
                            parsingHelper = ParsingHelperEnum.NEXT_IS_YDECIMAL;
                        }
                        else if (parsingHelper == ParsingHelperEnum.NEXT_IS_YDECIMAL)
                        {
                            try { yDecimalPlaces = (int)char.GetNumericValue(fsChars[i]); }
                            catch { };
                            parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        }
                        else
                        {
                            // completely unknown value
                            parsingHelper = ParsingHelperEnum.NEXT_IS_NONE;
                        }
                        continue;
                }

            }
            return 0;
        }

    }
}

