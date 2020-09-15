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
    /// A abstract base class for Gerber file lines
    /// </summary>
    /// <history>
    ///    07 Jul 10  Cynic - Started
    ///    15 Jan 11  Cynic - Added doNotDisplay code
    /// </history>
    public abstract class GerberLine : OISObjBase
    {

        // this is returned by the PerformPlotGerberAction to 
        // indicate if the plotting can continue
        public enum PlotActionEnum
        {
            PlotAction_Continue,
            PlotAction_FailWithError,
            PlotAction_End,
        }

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        // These are the values we ADD to the existing X and Y coords from the DCodes
        // in order to set the origin approximately at zero
        private float plotXCoordOriginAdjust = 0;
        private float plotYCoordOriginAdjust = 0;

        // this is the current file source
        private string rawLineStr = "";
        private string processedLineStr = "";
        private int lineNumber = 0;

        // sometimes in the interests of adjusting the gerber codes so that they
        // translate better from a exposure based plotter concept to a isolation
        // routing concept we do not want to display the item. The functionality
        // will have been fixed up and added in later.
        private const bool DEFAULT_DONOTDISPLAY = false;
        private bool doNotDisplay = DEFAULT_DONOTDISPLAY;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        /// <param name="lineNumberIn">the line number</param>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public GerberLine(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
        {
            rawLineStr = rawLineStrIn;
            processedLineStr = processedLineStrIn;
            lineNumber = lineNumberIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets /Sets the doNotDisplay value
        /// </summary>
        /// <history>
        ///    15 Jan 11  Cynic - Started
        /// </history>
        public bool DoNotDisplay
        {
            get
            {
                return doNotDisplay;
            }
            set
            {
                doNotDisplay = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the line number. There is no set accessor. This value is set in the 
        /// constructor
        /// </summary>
        /// <history>
        ///    09 Aug 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    08 Aug 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    08 Aug 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
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
        /// Performs the action the plot gerber code action required based on the current context
        /// </summary>
        /// <param name="graphicsObj">a graphics object on which to plot</param>
        /// <param name="stateMachine">the gerber plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public virtual GerberLine.PlotActionEnum PerformPlotGerberAction(Graphics graphicsObj, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
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
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public abstract int ParseLine(string processedLineStr, GerberFileStateMachine stateMachine);

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
        ///    24 Jul 10  Cynic - Started
        /// </history>
        public virtual GerberLine.PlotActionEnum PerformPlotIsoStep1Action(GCodeBuilder gCodeBuilder, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {
            errorValue = 0;
            errorString = "";
            return PlotActionEnum.PlotAction_Continue;
        }

    }
}
