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
    /// A class to encapsulate a excellon XY Code
    /// </summary>
    public class ExcellonLine_XYCode : ExcellonLine
    {

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        /// These values are the decimal compensated values from the DCode itself. They
        /// are not yet scaled to plot coordinates.
        private float xCoord = 0;
        private float yCoord = 0;
        private float lastDrillWidth = 0;
        // these are the last plot coords used
        private int lastPlotXCoordEnd = 0;
        private int lastPlotYCoordEnd = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        public ExcellonLine_XYCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last used drill width value
        /// </summary>
        public float LastDrillWidth
        {
            get
            {
                return lastDrillWidth;
            }
            set
            {
                lastDrillWidth = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current X Coordinate value
        /// </summary>
        public float XCoord
        {
            get
            {
                return xCoord;
            }
            set
            {
                xCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Y Coordinate value
        /// </summary>
        public float YCoord
        {
            get
            {
                return yCoord;
            }
            set
            {
                yCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last plotted X Coordinate value
        /// </summary>
        public float LastPlotXCoordEnd
        {
            get
            {
                return lastPlotXCoordEnd;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last plotted Y Coordinate value
        /// </summary>
        public float LastPlotYCoordEnd
        {
            get
            {
                return lastPlotYCoordEnd;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current gx0 value, but applies offset compensation
        /// </summary>
        public float gX0OffsetCompensated
        {
            get
            {
                return (float)Math.Round((xCoord - this.PlotXCoordOriginAdjust), 3);
                //return xCoord - this.PlotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current gy0 value, but applies offset compensation
        /// </summary>
        public float gY0OffsetCompensated
        {
            get
            {
                return (float)Math.Round((yCoord - this.PlotYCoordOriginAdjust), 3);
                //return yCoord - this.PlotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current X Coordinate value with origin compensation
        /// </summary>
        public float CoordOriginCompensated_X
        {
            get
            {
                return XCoord + PlotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current Y Coordinate value with origin compensation
        /// </summary>
        public float CoordOriginCompensated_Y
        {
            get
            {
                return YCoord + PlotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current X Coordinate value with origin compensation
        /// and flipping applied (if necessary)
        /// </summary>
        public int GetIsoPlotCoordOriginCompensated_X(ExcellonFileStateMachine stateMachine)
        {
            // Just return this
            return (int)Math.Round((CoordOriginCompensated_X * stateMachine.IsoPlotPointsPerAppUnit));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current Y Coordinate value with origin compensation
        /// and flipping applied (if necessary)
        /// </summary>
        public int GetIsoPlotCoordOriginCompensated_Y(ExcellonFileStateMachine stateMachine)
        {
            // Just return this
            return (int)Math.Round((CoordOriginCompensated_Y * stateMachine.IsoPlotPointsPerAppUnit));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets for a new plot
        /// </summary>
        public override void ResetForPlot()
        {
            lastPlotXCoordEnd = 0;
            lastPlotYCoordEnd = 0;
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
            float outFloat = 0;
            int nextStartPos = 0;
            bool retBool;

            //LogMessage("ParseLine(XY) started");

            if (processedLineStr == null) return 100;
            if (((processedLineStr.StartsWith("X") == true) || (processedLineStr.StartsWith("Y") == true)) == false)
            {
                return 200;
            }

            // now the line will have some combination of X and Y tags in some order

            // LOOK FOR THE X TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'X', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // we have to have this
                LogMessage("ParseLine(XY) lineNumber="+LineNumber.ToString()+" failed. No X coordinate found.");
                return 1331;
            }
            else
            {
                // this will have a float number
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(processedLineStr, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("ParseLine(DX) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 1332;
                }
                else
                {
                    // set the value now
                    xCoord = DecimalScaleNumber(outFloat, stateMachine.ExcellonFileManager.DrillingNumberOfDecimalPlaces, stateMachine.ExcellonFileManager.DrillingCoordinateZerosMode);
                }
            }

            // LOOK FOR THE Y TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'Y', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // we have to have this
                LogMessage("ParseLine(XY) lineNumber=" + LineNumber.ToString() + " failed. No Y coordinate found.");
                return 2331;
            }
            else
            {
                // this will have a float number
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(processedLineStr, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("ParseLine(DY) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 2332;
                }
                else
                {
                    // set the value now
                    yCoord = DecimalScaleNumber(outFloat, stateMachine.ExcellonFileManager.DrillingNumberOfDecimalPlaces, stateMachine.ExcellonFileManager.DrillingCoordinateZerosMode);
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
            gcLineList = null;

            GCodeCmd_ZMove zLine = null;
            GCodeCmd_RapidMove rmLine = null;
            gcLineList = new List<GCodeCmd>();

            if (stateMachine.CurrentTool != null)
            {
                if (stateMachine.CurrentTool.SkipThisTool==true)
                {
                    gcLineList = new List<GCodeCmd>();
                    return 0;
                }
            }


            int x0 = GetIsoPlotCoordOriginCompensated_X(stateMachine);
            int y0 = GetIsoPlotCoordOriginCompensated_Y(stateMachine);

            // G00 rapid move tool head to the xCoord, yCoord
            rmLine = new GCodeCmd_RapidMove(x0, y0);
            gcLineList.Add(rmLine);
            stateMachine.LastXCoord = XCoord;
            stateMachine.LastYCoord = YCoord;
            stateMachine.LastPlotXCoord = x0;
            stateMachine.LastPlotYCoord = y0;
            // record locally
            lastPlotXCoordEnd = x0;
            lastPlotYCoordEnd = y0;

            // set the drill width
            float workingDrillWidth = stateMachine.LastDrillWidth * stateMachine.IsoPlotPointsPerAppUnit;
            // remember this
            LastDrillWidth = stateMachine.LastDrillWidth;

            // G00 - put the bit into the work piece
            zLine = new GCodeCmd_ZMove(GCodeCmd_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForCut);
            zLine.SetGCodePlotDrillValues(x0, y0, workingDrillWidth);
            zLine.WantLinearMove = true;
            gcLineList.Add(zLine);

            // G00 - pull the bit out of the work piece
            zLine = new GCodeCmd_ZMove(GCodeCmd_ZMove.GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForClear);
            zLine.SetGCodePlotDrillValues(x0, y0, workingDrillWidth);
            gcLineList.Add(zLine);

            return 0;
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

            int x0 = GetIsoPlotCoordOriginCompensated_X(stateMachine);
            int y0 = GetIsoPlotCoordOriginCompensated_Y(stateMachine);

            // G00 rapid move tool head to the xCoord, yCoord
            stateMachine.LastXCoord = XCoord;
            stateMachine.LastYCoord = YCoord;
            stateMachine.LastPlotXCoord = x0;
            stateMachine.LastPlotYCoord = y0;
            // record locally
            lastPlotXCoordEnd = x0;
            lastPlotYCoordEnd = y0;

            // set the drill width
            float workingDrillWidth = stateMachine.LastDrillWidth * stateMachine.IsoPlotPointsPerAppUnit;
            // remember this
            LastDrillWidth = stateMachine.LastDrillWidth;

            MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, stateMachine.ExcellonHoleBrush, x0, y0, workingDrillWidth, workingDrillWidth);

            errorValue = 0;
            errorString = "Successful End";
            return PlotActionEnum.PlotAction_Continue;
        }

    }
}

