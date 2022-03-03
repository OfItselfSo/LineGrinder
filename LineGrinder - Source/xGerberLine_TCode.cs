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
    /// A class to encapsulate a gerber T Code
    /// </summary>
    public class xGerberLine_TCode : GerberLine
    {

        private string currentTCode = "";
        private string currentTCodeAttribute = "";

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        public xGerberLine_TCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current T Code value. Never gets/sets null
        /// </summary>
        public string CurrentTCode
        {
            get
            {
                if (currentTCode == null) currentTCode = "";
                return currentTCode;
            }
            set
            {
                currentTCode = value;
                if (currentTCode == null) currentTCode = "";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current T Code attribute value. Never gets/sets null
        /// </summary>
        public string CurrentTCodeAttribute
        {
            get
            {
                if (currentTCodeAttribute == null) currentTCodeAttribute = "";
                return currentTCodeAttribute;
            }
            set
            {
                currentTCodeAttribute = value;
                if (currentTCodeAttribute == null) currentTCodeAttribute = "";
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
        public override GerberLine.PlotActionEnum PerformPlotGerberAction(Graphics graphicsObj, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {
            errorValue = 0;
            errorString = "";
            switch (currentTCode)
            {
                case "A":
                    {
                        // This is just a linear interpolation
                        return GerberLine.PlotActionEnum.PlotAction_Continue;
                    }
                case "D":
                    {
                        // This is just an Ignore data block we can just ignore its contents
                        // set the value now
                        return GerberLine.PlotActionEnum.PlotAction_Continue;
                    }
                case "F":
                    {
                        // This is INCHES mode
                        return GerberLine.PlotActionEnum.PlotAction_Continue;
                    }
                case "O":
                    {
                        // This is Millimeters mode
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
            //LogMessage("ParseLine(T) started");

            if (processedLineStr == null) return 100;
            else if (processedLineStr.StartsWith("TA.") == true)
            {
                // this is an apperture attribute
                CurrentTCode = "A";
                // set it
                CurrentTCodeAttribute = processedLineStr.Replace("TA.", "").Trim('%');
            }
            else if (processedLineStr.StartsWith("TD") == true)
            {
                // this is an attribute delete
                CurrentTCode = "D";
                // set it
                CurrentTCodeAttribute = processedLineStr.Replace("TD", "").Trim('%');
            }
            else if (processedLineStr.StartsWith("TF.") == true)
            {
                // this is an file attribute
                CurrentTCode = "F";
                // set it
                CurrentTCodeAttribute = processedLineStr.Replace("TF.", "").Trim('%');
            }
            else if (processedLineStr.StartsWith("TO.") == true)
            {
                // this is an object attribute
                CurrentTCode = "O";
                // set it
                CurrentTCodeAttribute = processedLineStr.Replace("TO.", "").Trim('%');
            }
            else
            {
                // we do not know this one
                LogMessage("ParseLine(T) failed. Unknown TCode ");
                return 200;
            }

            // we are good
            return 0;
        }

    }
}

