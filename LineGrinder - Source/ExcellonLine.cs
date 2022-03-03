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
    /// A abstract base class for Excellon file lines
    /// </summary>
    public abstract class ExcellonLine : OISObjBase
    {

        // this is returned by the PerformPlotExcellonAction to 
        // indicate if the plotting can continue
        public enum PlotActionEnum
        {
            PlotAction_Continue,
            PlotAction_FailWithError,
            PlotAction_End,
        }

        // These are the values we ADD to the existing X and Y coords from the DCodes
        // in order to set the origin approximately at zero
        private float plotXCoordOriginAdjust = 0;
        private float plotYCoordOriginAdjust = 0;

        // this is the current file source
        private string rawLineStr = "";
        private string processedLineStr = "";
        private int lineNumber = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        /// <param name="lineNumberIn">the line number</param>
        public ExcellonLine(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
        {
            rawLineStr = rawLineStrIn;
            processedLineStr = processedLineStrIn;
            lineNumber = lineNumberIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the line number. There is no set accessor. This value is set in the 
        /// constructor
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
        /// Gets/Sets the current X Coord Origin Adjust value. These are compensating
        /// values we apply to the current coordinates in order to make the
        /// smallest X coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// </summary>
        public float PlotXCoordOriginAdjust
        {
            get
            {
                return plotXCoordOriginAdjust;
            }
            set
            {
                plotXCoordOriginAdjust = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Y Coord Origin Adjust value. These are compensating
        /// values we apply to the current coordinates in order to make the
        /// smallest Y coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// </summary>
        public float PlotYCoordOriginAdjust
        {
            get
            {
                return plotYCoordOriginAdjust;
            }
            set
            {
                plotYCoordOriginAdjust = value;
            }
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets or Sets the LineStr. Will never get or set a null value
        /// </summary>
        public string RawLineStr
        {
            get
            {
                if (rawLineStr == null) rawLineStr = "";
                return rawLineStr;
            }
            set
            {
                rawLineStr = value;
                if (rawLineStr == null) rawLineStr = "";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Inserts decimal places into the XY coordinate values as appropriate
        /// </summary>
        /// <param name="numToScale">the number we need to scale</param>
        /// <param name="integerPlaces">the number of integer places</param>
        /// <param name="decimalPlaces">the number of decimal places</param>
        /// <param name="leadingZeroMode">a flag to indicate if leading or traling zeros are discarded</param>
        /// <returns>z success, nz fail</returns>
        protected float DecimalScaleNumber(float numToScale, int decimalPlaces, FileManager.ExcellonDrillingCoordinateZerosModeEnum leadingZeroMode)
        {
            switch (leadingZeroMode)
            {
                case FileManager.ExcellonDrillingCoordinateZerosModeEnum.DecimalNumber:
                {
                    // in this mode the number is the number. It is already scaled
                    return numToScale;
                }
                case FileManager.ExcellonDrillingCoordinateZerosModeEnum.FixedDecimalPoint:
                case FileManager.ExcellonDrillingCoordinateZerosModeEnum.OmitLeadingZeros:
                {
                    // all we have to do is divide the number by the 10^decimalPlaces
                    // for example if decimalPlaces is three and numToScale is 1503
                    // then the real number should be 01.503
                    if (decimalPlaces == 0) return numToScale;
                    float tmpFloat = numToScale / (float)Math.Pow(10, decimalPlaces);
                    return tmpFloat;
                }
                default: // probably omit trailing zeros mode
                {
                    // this is a lot more tricky since we have to have the original text
                    // value in order to figure this out. This blows chunks, and I have
                    // a hard time believing anybody uses it. I will leave it as not
                    // implemented at the moment because I have better things to do.
                    throw new NotImplementedException("Excellon Files in Omit Trailing Zeros Mode are not Supported");
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets for a new plot
        /// </summary>
        public virtual void ResetForPlot()
        {
            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the action the plot excellon code action required based on the current context
        /// </summary>
        /// <param name="graphicsObj">a graphics object on which to plot</param>
        /// <param name="stateMachine">the excellon plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <returns>z success, nz fail</returns>
        public virtual PlotActionEnum PerformPlotExcellonAction(Graphics graphicsObj, ExcellonFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {
            // ignore this
            errorValue = 0;
            errorString = "";
            return PlotActionEnum.PlotAction_Continue;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Parses out the line and gets the required information from it
        /// </summary>
        /// <param name="processedLineStr">a line string without block terminator or format parameters</param>
        /// <param name="stateMachine">The state machine containing the implied modal values</param>
        /// <returns>z success, nz fail</returns>
        public abstract int ParseLine(string processedLineStr, ExcellonFileStateMachine stateMachine);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the excellon line into a GCode line and returns it
        /// </summary>
        /// <param name="stateMachine">the state machine with the configuration</param>
        /// <param name="gcLineList">a list of the equivalent gcode line object. This can be 
        /// empty if there is no direct conversion</param>
        /// <returns>z success, nz fail</returns>
        public abstract int GetGCodeCmd(ExcellonFileStateMachine stateMachine, out List<GCodeCmd> gcLineList);


    }
}

