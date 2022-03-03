using System;
using System.Collections.Generic;
using System.Linq;
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
    /// A class to contain some useful gerber file parsing routines
    /// </summary>
    public static class GerberParseUtils 
    {

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Parse a number from a string when given a specified start point. Stop
        /// at the first non-digit number and return that position as well as the 
        /// integer value of the number specified.
        /// </summary>
        /// <param name="lineStr">the line string to parse on</param>
        /// <param name="nextStartPos">an out parameter, this is the position of the first non digit number after the start position</param>
        /// <param name="outInt">the integer output value, parsed out and converted</param>
        /// <param name="startPos">the postion in the string to start at</param>
        /// <returns>true success, nz fail</returns>
        static public bool ParseNumberFromString_TillNonDigit_RetInteger(string lineStr, int startPos, ref int outInt, ref int nextStartPos)
        {
            StringBuilder sb = new StringBuilder();
            int posIndex = 0;

            if (startPos < 0) goto FAIL_OUT;
            if (lineStr == null) goto FAIL_OUT;
            if (lineStr.Length ==0 ) goto FAIL_OUT;

            // convert to a character array
            char[] lineChars = lineStr.ToCharArray();
            if (lineChars == null) goto FAIL_OUT;
            if (lineChars.Length < startPos) goto FAIL_OUT;

            for (posIndex = startPos; posIndex < lineChars.Length; posIndex++)
            {
                if (char.IsDigit(lineChars[posIndex]) == true)
                {
                    sb.Append(lineChars[posIndex]);
                }
                else if (lineChars[posIndex] == '-')
                {
                    // we also accept a minus sign
                    sb.Append(lineChars[posIndex]);
                }
                else if (lineChars[posIndex] == '+')
                {
                    // we also accept a plus sign
                    sb.Append(lineChars[posIndex]);
                }
                else
                {
                    // we hit a letter or are all done
                    break;
                }
            }
            try
            {
                nextStartPos = posIndex;
                outInt = Convert.ToInt32(sb.ToString());
                return true;
            }
            catch
            {
                nextStartPos = -1;
                outInt = -1;
                return false;
            }

            FAIL_OUT:
            nextStartPos = -1;
            outInt = -1;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Parse a number from a string when given a specified start point. Stop
        /// at the first non-digit number and return that position as well as the 
        /// float value of the number specified.
        /// </summary>
        /// <param name="lineStr">the line string to parse on</param>
        /// <param name="nextStartPos">an out parameter, this is the position of the first non digit number after the start position</param>
        /// <param name="outInt">the integer output value, parsed out and converted</param>
        /// <param name="startPos">the postion in the string to start at</param>
        /// <returns>true success, nz fail</returns>
        static public bool ParseNumberFromString_TillNonDigit_RetFloat(string lineStr, int startPos, ref float outFloat, ref int nextStartPos)
        {
            StringBuilder sb = new StringBuilder();
            int posIndex = 0;

            if (startPos < 0) goto FAIL_OUT;
            if (lineStr == null) goto FAIL_OUT;
            if (lineStr.Length == 0) goto FAIL_OUT;

            // convert to a character array
            char[] lineChars = lineStr.ToCharArray();
            if (lineChars == null) goto FAIL_OUT;
            if (lineChars.Length < startPos) goto FAIL_OUT;

            for (posIndex = startPos; posIndex < lineChars.Length; posIndex++)
            {
                if (char.IsDigit(lineChars[posIndex]) == true)
                {
                    sb.Append(lineChars[posIndex]);
                }
                else if (lineChars[posIndex] == '-')
                {
                    // we also accept a minus sign
                    sb.Append(lineChars[posIndex]);
                }
                else if (lineChars[posIndex] == '+')
                {
                    // we also accept a plus sign
                    sb.Append(lineChars[posIndex]);
                }
                else if (lineChars[posIndex] == '.')
                {
                    // we also accept a decimal point
                    sb.Append(lineChars[posIndex]);
                }
                else
                {
                    // we hit a letter or are all done
                    break;
                }
            }
            try
            {
                nextStartPos = posIndex;
                outFloat = Convert.ToSingle(sb.ToString());
                return true;
            }
            catch
            {
                nextStartPos = -1;
                outFloat = 0;
                return false;
            }

        FAIL_OUT:
            nextStartPos = -1;
            outFloat = 0;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Trundle forward through the string looking for a character. Then return the 
        /// next position. Yes I know there are  much more efficient ways of doing this
        /// I have my reasons for doing it this way.
        /// </summary>
        /// <param name="lineStr">the line string to parse on</param>
        /// <param name="searchChar">the search character</param>
        /// <param name="startPos">the start position</param>
        /// <returns>true success, nz fail</returns>
        static public int FindCharacterReturnNextPos(string lineStr, char searchChar, int startPos)
        {
            int posIndex = 0;

            if (startPos < 0) return -1;
            if (lineStr == null) return -1;
            if (lineStr.Length == 0) return -1;

            // convert to a character array
            char[] lineChars = lineStr.ToCharArray();
            if (lineChars == null) return -1;
            if (lineChars.Length < startPos) return -1;

            for (posIndex = startPos; posIndex < lineChars.Length; posIndex++)
            {
                if (lineChars[posIndex] == searchChar)
                {
                    if((posIndex+1) >= lineChars.Length) return -1;
                    return posIndex + 1;
                }
            }
            return -1;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Strips a trailing style comment from a string
        /// </summary>
        /// <param name="lineStr">the line string to parse on</param>
        /// <param name="commentDelimiter">the comment delimiter</param>
        /// <returns>string stripped of comments</returns>
        static public string RemoveTrailingCommentsFromString(string lineStr, string commentDelimiter)
        {
            if ((commentDelimiter == null) || (commentDelimiter.Length == 0)) return lineStr;
            if ((lineStr == null) || (lineStr.Length == 0)) return lineStr;

            // have we got a comment delimiter
            int commentPos = lineStr.IndexOf(commentDelimiter);
            // did we find a comment delimiter
            if (commentPos < 0) return lineStr;
            return lineStr.Substring(0, commentPos);
        }
    }
}

