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
    /// A class to encapsulate a gerber G54 Code
    /// </summary>
    /// <history>
    ///    23 Sep 10  Cynic - Started
    /// </history>
    public class GerberLine_G54Code : GerberLine
    {

        private int currentDCode = 0;


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        /// <history>
        ///    23 Sep 10  Cynic - Started
        /// </history>
        public GerberLine_G54Code(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current D Code value
        /// </summary>
        /// <history>
        ///    23 Sep 10  Cynic - Started
        /// </history>
        public int CurrentDCode
        {
            get
            {
                return currentDCode;
            }
            set
            {
                currentDCode = value;
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
        ///    23 Sep 10  Cynic - Started
        /// </history>
        public override GerberLine.PlotActionEnum PerformPlotGerberAction(Graphics graphicsObj, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {

            if (stateMachine == null)
            {
                errorValue = 999;
                errorString = "PerformPlotGerberAction (G54) stateMachine == null";
                return GerberLine.PlotActionEnum.PlotAction_FailWithError;
            }

            // must be an aperture select
            stateMachine.CurrentAperture = stateMachine.ApertureCollection.GetApertureByID(CurrentDCode);
            return GerberLine.PlotActionEnum.PlotAction_Continue;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the plot Isolation Object actions required based on the current context
        /// </summary>
        /// <param name="gCodeBuilder">A GCode Builder object</param>
        /// <param name="stateMachine">the gerber plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <returns>an enum value indicating what next action to take</returns>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public override GerberLine.PlotActionEnum PerformPlotIsoStep1Action(GCodeBuilder gCodeBuilder, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {

            if (gCodeBuilder == null)
            {
                errorValue = 998;
                errorString = "PerformPlotIsoStep1Action (G54) gCodeBuilder == null";
                return GerberLine.PlotActionEnum.PlotAction_FailWithError;
            }
            if (stateMachine == null)
            {
                errorValue = 999;
                errorString = "PerformPlotIsoStep1Action (G54) stateMachine == null";
                return GerberLine.PlotActionEnum.PlotAction_FailWithError;
            }

            // must be an aperture select
            stateMachine.CurrentAperture = stateMachine.ApertureCollection.GetApertureByID(CurrentDCode);
            return GerberLine.PlotActionEnum.PlotAction_Continue;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Parses out the line and gets the required information from it
        /// </summary>
        /// <param name="processedLineStr">a line string without block terminator or format parameters</param>
        /// <param name="stateMachine">The state machine containing the implied modal values</param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    23 Sep 10  Cynic - Started
        /// </history>
        public override int ParseLine(string processedLineStr, GerberFileStateMachine stateMachine)
        {
            int outInt = -1;
            int nextStartPos = 0;
            bool retBool;

            //LogMessage("ParseLine(G54) started");

            if (processedLineStr == null) return 100;
            if (processedLineStr.StartsWith("G54") == false)
            {
                return 200;
            }

            // assume defaults from the state machine
            if (stateMachine != null)
            {
                currentDCode = stateMachine.LastDCode;
            }

            // now the G54 line should have a D tag
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'D', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // this is not an error - just means we did not find one
            }
            else
            {
                // this will have a integer number
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetInteger(processedLineStr, nextStartPos, ref outInt, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("ParseLine(G54) failed on call to ParseNumberFromString_TillNonDigit_RetInteger");
                    return 333;
                }
                else
                {
                    // set the value now
                    currentDCode = outInt;
                }
            }

            return 0;
        }

    }
}