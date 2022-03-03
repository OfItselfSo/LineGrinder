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
    /// A class to encapsulate a gerber G Code
    /// </summary>
    public class xGerberLine_GCode : GerberLine
    {

        private int currentGCode = 0;
        private static int[] supportedGCodes = { 4 };

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        public xGerberLine_GCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current G Code value
        /// </summary>
        public int CurrentGCode
        {
            get
            {
                return currentGCode;
            }
            set
            {
                currentGCode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Not every GCode is supported. This tells us if the specified code is supported
        /// </summary>
        /// <param name="gCodeIn">the gcode to test</param>
        /// <returns>true is supported, false is not</returns>
        public static bool IsThisGCodeSupported(int gCodeIn)
        {
            foreach (int supportedCode in supportedGCodes)
            {
                if (supportedCode == gCodeIn) return true;
            }
            return false;
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
        public override GerberLine.PlotActionEnum PerformPlotGerberAction(Graphics graphicsObj, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {
            errorValue = 0;
            errorString = "";
            switch (currentGCode)
            {
                case 1:
                    {
                        // This is just a linear interpolation
                        return GerberLine.PlotActionEnum.PlotAction_Continue;
                    }
                case 4:
                    {
                        // This is just an Ignore data block we can just ignore its contents
                        // set the value now
                        return GerberLine.PlotActionEnum.PlotAction_Continue;
                    }
                case 70:
                    {
                        // This is INCHES mode
                        return GerberLine.PlotActionEnum.PlotAction_Continue;
                    }
                case 71:
                    {
                        // This is Millimeters mode
                        return GerberLine.PlotActionEnum.PlotAction_Continue;
                    }
                case 75:
                    {
                        // This is just circular interpolation enabled
                        // we ignore it
                        return GerberLine.PlotActionEnum.PlotAction_Continue;
                    }
                case 90:
                    {
                        // This is absolute coordinates enabled
                        // we ignore it
                        return GerberLine.PlotActionEnum.PlotAction_Continue;
                    }
                case 91:
                    {
                        // This is incremental coordinates enabled
                        return GerberLine.PlotActionEnum.PlotAction_Continue;
                    }
                default:
                    {
                        // we do not know what we have - fail now
                        errorValue = 0;
                        errorString = "";
                        return GerberLine.PlotActionEnum.PlotAction_FailWithError;
                    }
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
            int outInt = -1;
            int nextStartPos = 0;
            bool retBool;

            //LogMessage("ParseLine(G) started");

            if (processedLineStr == null) return 100;
            if (processedLineStr.StartsWith("G") == false)
            {
                return 200;
            }

            // now the GCode line will have an G tag

            // LOOK FOR THE G TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'G', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // this is an error - we have to find one
                LogMessage("ParseLine(G) failed on call to FindCharacterReturnNextPos");
                return 332;
            }
            // this will have a float number
            retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetInteger(processedLineStr, nextStartPos, ref outInt, ref nextStartPos);
            if (retBool != true)
            {
                LogMessage("ParseLine(G) failed on call to ParseNumberFromString_TillNonDigit_RetInteger");
                return 333;
            }

            switch(outInt)
            {
                case 1:
                    {
                        // This is just an linear interpolation 
                        currentGCode = outInt;
                        break;
                    }
                case 4:
                    {
                        // This is just an Ignore data block we can just ignore its contents
                        // set the value now
                        currentGCode = outInt;
                        break;
                    }
                case 70:
                {
                    // This is INCHES mode
                    currentGCode = outInt;
                    stateMachine.GerberFileUnits = ApplicationUnitsEnum.INCHES;
                    break;
                }
                case 71:
                {
                    // This is Millimeters mode
                    currentGCode = outInt;
                    stateMachine.GerberFileUnits = ApplicationUnitsEnum.MILLIMETERS;
                    break;
                }
                case 75:
                {
                    // This is just circular interpolation enabled
                    // we ignore it
                    currentGCode = outInt;
                    break;
                }
                case 90:
                {
                    // This is absolute coordinates enabled
                    currentGCode = outInt;
                    stateMachine.GerberFileCoordinateMode = GerberCoordinateModeEnum.COORDINATE_ABSOLUTE;
                    break;
                }
                case 91:
                {
                    // This is incremental coordinates enabled
                    currentGCode = outInt;
                    stateMachine.GerberFileCoordinateMode = GerberCoordinateModeEnum.COORDINATE_INCREMENTAL;
                    break;
                }
                default:
                {
                    LogMessage("ParseLine(G) failed. Unknown GCode of"+outInt.ToString());
                    return 334;
                }
            }
            return 0;
        }

    }
}

