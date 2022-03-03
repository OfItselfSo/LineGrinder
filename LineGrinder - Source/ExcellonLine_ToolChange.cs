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
    /// A class to encapsulate a excellon Tool Change T Code
    /// </summary>
    public class ExcellonLine_ToolChange : ExcellonLine
    {

        private int toolNumber = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        public ExcellonLine_ToolChange(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current D Code value
        /// </summary>
        public int ToolNumber
        {
            get
            {
                return toolNumber;
            }
            set
            {
                toolNumber = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the action the plot requires based on the current context
        /// </summary>
        /// <param name="graphicsObj">a graphics object on which to plot</param>
        /// <param name="stateMachine">the excellon plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <returns>an enum value indicating what next action to take</returns>
        public override PlotActionEnum PerformPlotExcellonAction(Graphics graphicsObj, ExcellonFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {
            ExcellonLine_ToolTable toolTabObj = null;
            // see if we can find the tool table object for this change
            toolTabObj = stateMachine.GetToolTableObjectByToolNumber(toolNumber);
            if (toolTabObj != null) stateMachine.LastDrillWidth = toolTabObj.DrillDiameter;
            errorValue = 0;
            errorString = "Successful End";
            return PlotActionEnum.PlotAction_Continue;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Parses out the line and gets the required information from it
        /// </summary>
        /// <param name="processedLineStr">a line string without block terminator or format parameters</param>
        /// <param name="stateMachine">The state machine containing the implied modal values</param>
        /// <returns>z success, nz fail</returns>
        public override int ParseLine(string processedLineStr, ExcellonFileStateMachine stateMachine)
        {
            int outInt = -1;
            int nextStartPos = 0;
            bool retBool;

            //DebugMessage("Excellon ParseLine(TChange) started");

            if (processedLineStr == null) return 100;
            if (processedLineStr.StartsWith("T") == false)
            {
                return 200;
            }

            // now the TCode line will have an T tag

            // LOOK FOR THE T TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'T', nextStartPos);
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
                    LogMessage("Excellon ParseLine(T) failed on call to ParseNumberFromString_TillNonDigit_RetInteger");
                    return 333;
                }
                else
                {
                    // set the value now
                    toolNumber = outInt;
                }
            }

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the excellon line into a GCode line and returns it
        /// </summary>
        /// <param name="stateMachine">the state machine with the configuration</param>
        /// <param name="gcLineList">a list of the equivalent gcode line object. This can be 
        /// empty if there is no direct conversion</param>
        /// <returns>z success, nz fail</returns>
        public override int GetGCodeCmd(ExcellonFileStateMachine stateMachine, out List<GCodeCmd> gcLineList)
        {
            GCodeCmd_ToolChange tcLine;
            ExcellonLine_ToolTable toolTabObj = null;
            string commentStr = null;

            // see if we can find the tool table object for this change
            toolTabObj = stateMachine.GetToolTableObjectByToolNumber(toolNumber);
            if (toolTabObj != null)
            {
                commentStr = "ToolChange, ToolNum: "+toolNumber.ToString()+ ", Drill Dia:"+toolTabObj.DrillDiameter.ToString();
                // also remember this
                stateMachine.LastDrillWidth = toolTabObj.DrillDiameter;
            }
            gcLineList = new List<GCodeCmd>();
            tcLine = new GCodeCmd_ToolChange(toolNumber);
            if (commentStr != null) tcLine.CommentText = commentStr;
            gcLineList.Add(tcLine);
            return 0;
        }

    }
}

