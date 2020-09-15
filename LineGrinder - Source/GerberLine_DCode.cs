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
    /// A class to encapsulate a gerber D Code
    /// </summary>
    /// <history>
    ///    07 Jul 10  Cynic - Started
    ///    15 Jan 11  Cynic - Added currentLineIsNotForIsolation code
    /// </history>
    public class GerberLine_DCode : GerberLine
    {
        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        private int currentDCode = 0;

        /// These values are the decimal compensated values from the DCode itself. They
        /// are not yet scaled to plot coordinates.
        private float dCodeXCoord = 0;
        private float dCodeYCoord = 0;

        // if this is true we are drawing area fill (G36 - G37) or some sort of text label
        // layer. These lines have no width do not get wrapped with an isolation cut. rather
        // the tool bit just runs down the center of those lines
        private const bool DEFAULT_CURRENTLINE_ISNOT_FORISOLATION = false;
        private bool currentLineIsNotForIsolation = DEFAULT_CURRENTLINE_ISNOT_FORISOLATION;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public GerberLine_DCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets /Sets the currentLineIsNotForIsolation value
        /// </summary>
        /// <history>
        ///    15 Jan 11  Cynic - Started
        /// </history>
        public bool CurrentLineIsNotForIsolation
        {
            get
            {
                return currentLineIsNotForIsolation;
            }
            set
            {
                currentLineIsNotForIsolation = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current X Coordinate value
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
        public float DCodeXCoord
        {
            get
            {
                return dCodeXCoord;
            }
            set
            {
                dCodeXCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Y Coordinate value
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
        public float DCodeYCoord
        {
            get
            {
                return dCodeYCoord;
            }
            set
            {
                dCodeYCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current X Coordinate value with origin compensation
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
        public float DCodeXCoordOriginCompensated
        {
            get
            {
                return dCodeXCoord + PlotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current Y Coordinate value with origin compensation
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
        public float DCodeYCoordOriginCompensated
        {
            get
            {
                return dCodeYCoord + PlotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current X Coordinate value with origin compensation
        /// and flipping applied (if necessary)
        /// </summary>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
        public int GetIsoPlotXCoordOriginCompensatedAndFlipped(GerberFileStateMachine stateMachine)
        {
            if (stateMachine.GerberFileManager.IsoFlipMode == FileManager.IsoFlipModeEnum.X_Flip)
            {
                return (int)Math.Round(((stateMachine.XFlipMax - DCodeXCoordOriginCompensated) * stateMachine.IsoPlotPointsPerAppUnit));
            }
            // not an X flip. Just return this
            return (int)Math.Round((DCodeXCoordOriginCompensated * stateMachine.IsoPlotPointsPerAppUnit));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current Y Coordinate value with origin compensation
        /// and flipping applied (if necessary)
        /// </summary>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
        public int GetIsoPlotYCoordOriginCompensatedAndFlipped(GerberFileStateMachine stateMachine)
        {
            if (stateMachine.GerberFileManager.IsoFlipMode == FileManager.IsoFlipModeEnum.Y_Flip)
            {
                return (int)Math.Round(((stateMachine.YFlipMax - DCodeYCoordOriginCompensated) * stateMachine.IsoPlotPointsPerAppUnit));
            }
            // not a Y flip. Just return this
            return (int)Math.Round((DCodeYCoordOriginCompensated * stateMachine.IsoPlotPointsPerAppUnit));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current D Code value
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
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
        /// Inserts decimal places into the DCode XY values as appropriate
        /// </summary>
        /// <param name="numToScale">the number we need to scale</param>
        /// <param name="integerPlaces">the number of integer places</param>
        /// <param name="decimalPlaces">the number of decimal places</param>
        /// <param name="leadingZeroMode">a flag to indicate if leading or traling zeros are discarded</param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        private float DecimalScaleNumber(float numToScale, int integerPlaces, int decimalPlaces, GerberLine_FSCode.LeadingZeroModeEnum leadingZeroMode)
        {
            if (leadingZeroMode == GerberLine_FSCode.LeadingZeroModeEnum.OMIT_TRAILING_ZEROS)
            {
                // this is a lot more tricky since we have to have the original text
                // value in order to figure this out. This blows chunks, and I have
                // a hard time believing anybody uses it. I will leave it as not
                // implemented at the moment because I have better things to do.
                throw new NotImplementedException("Gerber Files in Omit Trailing Zeros Mode are not Supported");
            }
            else //GerberLine_FSCode.LeadingZeroModeEnum.OMIT_LEADING_ZEROS
            {
                // all we have to do is divide the number by the 10^decimalPlaces
                // for example if decimalPlaces is three and numToScale is 1503
                // then the real number should be 01.503
                if (decimalPlaces == 0) return numToScale;
                float tmpFloat = numToScale / (float)Math.Pow(10, decimalPlaces);
                return tmpFloat;
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
        ///    07 Jul 10  Cynic - Started
        ///    15 Jan 11  Cynic - added in the currentLineIsNotForIsolation code
        /// </history>
        public override GerberLine.PlotActionEnum PerformPlotGerberAction(Graphics graphicsObj, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {
            int workingXCoord = 0;
            int workingYCoord = 0;
            Pen workingPen = null;

            if (stateMachine == null)
            {
                errorValue = 999;
                errorString = "PerformPlotGerberAction (D) stateMachine == null";
                return GerberLine.PlotActionEnum.PlotAction_FailWithError;
            }

            if (CurrentDCode == 1)
            {
                // get the current X,Y coordinates and scale up
                workingXCoord = GetIsoPlotXCoordOriginCompensatedAndFlipped(stateMachine);
                workingYCoord = GetIsoPlotYCoordOriginCompensatedAndFlipped(stateMachine);
                // Draw line, exposure on
                workingPen = stateMachine.CurrentAperture.GetWorkingPen(stateMachine, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);
                // simpulate sweeping the aperture from one end of the line to the other
                graphicsObj.DrawLine(workingPen, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);

          //      DebugMessage("workingXCoord=" + workingXCoord.ToString());

                /*
                DebugMessage("");
                DebugMessage("stateMachine.XFlipMax=" + stateMachine.XFlipMax.ToString());
                DebugMessage("DCodeXCoordOriginCompensated =" + DCodeXCoordOriginCompensated.ToString());
                DebugMessage("stateMachine.LastPlotXCoord=" + stateMachine.LastPlotXCoord.ToString());
                DebugMessage("workingXCoord=" + workingXCoord.ToString());
                DebugMessage("dCodeXCoord=" + dCodeXCoord.ToString());
                DebugMessage("PlotXCoordOriginAdjust=" + PlotXCoordOriginAdjust.ToString());
                DebugMessage("");
                */

                // now fixup the endpoints to compensate for non-rectangular apertures
                stateMachine.CurrentAperture.FixupLineEndpointsForGerberPlot(stateMachine, graphicsObj, stateMachine.PlotApertureBrush, workingPen, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);
                if (stateMachine.ShowGerberCenterLines == true)
                {
                    graphicsObj.DrawLine(ApplicationColorManager.DEFAULT_GERBERPLOT_CENTERLINE_PEN, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);
                }
                // remember these for the next DCode
                stateMachine.LastPlotXCoord = workingXCoord;
                stateMachine.LastPlotYCoord = workingYCoord;
            }
            else if (CurrentDCode == 2)
            {
                // get the current X,Y coordinates and scale up
                workingXCoord = GetIsoPlotXCoordOriginCompensatedAndFlipped(stateMachine);
                workingYCoord = GetIsoPlotYCoordOriginCompensatedAndFlipped(stateMachine);
                // Exposure off, move to point
                // remember these for the next DCode
                stateMachine.LastPlotXCoord = workingXCoord;
                stateMachine.LastPlotYCoord = workingYCoord;
            }
            else if (CurrentDCode == 3)
            {
                // get the current X,Y coordinates and scale up
                workingXCoord = GetIsoPlotXCoordOriginCompensatedAndFlipped(stateMachine);
                workingYCoord = GetIsoPlotYCoordOriginCompensatedAndFlipped(stateMachine);
                // move to point and Flash aperture
                stateMachine.CurrentAperture.FlashApertureForGerberPlot(stateMachine, graphicsObj, stateMachine.PlotApertureBrush, workingXCoord, workingYCoord);
                // remember these for the next DCode
                stateMachine.LastPlotXCoord = workingXCoord;
                stateMachine.LastPlotYCoord = workingYCoord;
            }
            else
            {
                // must be an aperture select
                stateMachine.CurrentAperture = stateMachine.ApertureCollection.GetApertureByID(CurrentDCode);
            }
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
        ///    15 Jan 11  Cynic - added in the currentLineIsNotForIsolation code
        /// </history>
        public override GerberLine.PlotActionEnum PerformPlotIsoStep1Action(GCodeBuilder gCodeBuilder, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {
            int workingXCoord = 0;
            int workingYCoord = 0;

            if (gCodeBuilder == null)
            {
                errorValue = 998;
                errorString = "PerformPlotIsoStep1Action (D) gCodeBuilder == null";
                return GerberLine.PlotActionEnum.PlotAction_FailWithError;
            }
            if (stateMachine == null)
            {
                errorValue = 999;
                errorString = "PerformPlotIsoStep1Action (D) stateMachine == null";
                return GerberLine.PlotActionEnum.PlotAction_FailWithError;
            }

            if (CurrentDCode == 1)
            {
                // get the current X,Y coordinates and scale up
                workingXCoord = GetIsoPlotXCoordOriginCompensatedAndFlipped(stateMachine);
                workingYCoord = GetIsoPlotYCoordOriginCompensatedAndFlipped(stateMachine);

                // the amount of extra X and Y we add depends on the angle. 
                int xComp = -1;
                int yComp = -1;
                int xyComp = -1;
                int width = -1;
                stateMachine.CurrentAperture.GetIsolationLineXYCompensation(stateMachine, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord, (int)Math.Round(((stateMachine.IsolationWidth * stateMachine.IsoPlotPointsPerAppUnit) / 2)), out xComp, out yComp, out xyComp);
                // Get Line width here
                width = stateMachine.CurrentAperture.GetPenWidthForLine(stateMachine, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);
                // note we do not need to use the xComp and yComp here. The aperture
                // fixup banged on the end takes care of the isolation routing round the
                // end points of the line
                int x0 = stateMachine.LastPlotXCoord ;
                int y0 = stateMachine.LastPlotYCoord;
                int x1 = workingXCoord ;
                int y1 = workingYCoord; 
                width += (xyComp * 2);

                // we do not isolation route if currentLineIsNotForIsolation == true. These 
                // are usually simple engraving lines and will get picked up later and the 
                // GCodes for them added
                if (this.currentLineIsNotForIsolation == false)
                {
                    // draw the line outline
                    gCodeBuilder.DrawGSLineOutLine(x0, y0, x1, y1, width, true);
                    // now fixup the endpoints to compensate for non-rectangular apertures
                    stateMachine.CurrentAperture.FixupLineEndpointsForGCodePlot(gCodeBuilder, stateMachine, x0, y0, x1, y1, (width / 2), xyComp);
                }
                else
                {
                    gCodeBuilder.DrawGSLineEngravingLine(x0, y0, x1, y1);
                }
                // remember these for the next DCode
                stateMachine.LastPlotXCoord = workingXCoord;
                stateMachine.LastPlotYCoord = workingYCoord;
            }
            else if (CurrentDCode == 2)
            {
                // get the current X,Y coordinates and scale up
                workingXCoord = GetIsoPlotXCoordOriginCompensatedAndFlipped(stateMachine);
                workingYCoord = GetIsoPlotYCoordOriginCompensatedAndFlipped(stateMachine);

                // Exposure off, move to point
                // remember these for the next DCode
                stateMachine.LastPlotXCoord = workingXCoord;
                stateMachine.LastPlotYCoord = workingYCoord;
            }
            else if (CurrentDCode == 3)
            {
                // get the current X,Y coordinates and scale up
                workingXCoord = GetIsoPlotXCoordOriginCompensatedAndFlipped(stateMachine);
                workingYCoord = GetIsoPlotYCoordOriginCompensatedAndFlipped(stateMachine);
                // move to point 
                // the amount of extra X and Y we add depends on the angle. 
                int xComp = -1;
                int yComp = -1;
                int xyComp = -1;
                stateMachine.CurrentAperture.GetIsolationLineXYCompensation(stateMachine, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord, (int)Math.Round(((stateMachine.IsolationWidth * stateMachine.IsoPlotPointsPerAppUnit) / 2)), out xComp, out yComp, out xyComp);
                // note we do not need to use the xComp and yComp here. The aperture
                // fixup banged on the end takes care of the isolation routing round the
                // end points of the line
                int x0 = workingXCoord;
                int y0 = workingYCoord;
                int x1 = workingXCoord;
                int y1 = workingYCoord;
                // are we on one of the refpin pads? - we do not isocut these
                if (stateMachine.IsThisARefPinPad(DCodeXCoord, DCodeYCoord, stateMachine.CurrentAperture.GetApertureDimension()) == true)
                {
                    // skip it, we do not isolation route refpin pads
                }
                else
                {
                    // we do not isolation route if currentLineIsNotForIsolation == true. These 
                    // are usually simple engraving lines and will get picked up later and the 
                    // GCodes for them added
                    if (this.currentLineIsNotForIsolation == false)
                    {
                        // flash aperture
                        stateMachine.CurrentAperture.FlashApertureForGCodePlot(gCodeBuilder, stateMachine, x0, y0, xyComp * 2);
                    }
                }
                // remember these for the next DCode
                stateMachine.LastPlotXCoord = workingXCoord;
                stateMachine.LastPlotYCoord = workingYCoord;
            }
            else
            {
                // must be an aperture select
                stateMachine.CurrentAperture = stateMachine.ApertureCollection.GetApertureByID(CurrentDCode);
            }
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
        ///    07 Jul 10  Cynic - Started
        ///    15 Jan 11  Cynic - added in the currentLineIsNotForIsolation code
        /// </history>
        public override int ParseLine(string processedLineStr, GerberFileStateMachine stateMachine)
        {
            int outInt = -1;
            float outFloat = 0;
            int nextStartPos = 0;
            bool retBool;

            //LogMessage("ParseLine(D) started");

            if (processedLineStr == null) return 100;
            if (((processedLineStr.StartsWith("D") == true) || (processedLineStr.StartsWith("X") == true) || (processedLineStr.StartsWith("Y") == true)) == false)
            {
                return 200;
            }

            // assume defaults from the state machine
            if (stateMachine != null)
            {
                dCodeXCoord = stateMachine.LastDCodeXCoord;
                dCodeYCoord = stateMachine.LastDCodeYCoord;
                currentDCode = stateMachine.LastDCode;
                currentLineIsNotForIsolation = stateMachine.CurrentLinesAreNotForIsolation;
             //   if (currentLineIsNotForIsolation == true)
             //   {
             //       int foo = 1;
             //   }
            }

            // now the DCode line will have some combination of D, X and Y tags in some order
            // some may be missing. We look for each in turn

            // LOOK FOR THE X TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'X', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // this is not an error - just means we did not find one
            }
            else
            {
                // this will have a float number
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(processedLineStr, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("ParseLine(DX) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 133;
                }
                else
                {
                    // set the value now
                    dCodeXCoord = DecimalScaleNumber(outFloat, stateMachine.FormatParameter.XIntegerPlaces, stateMachine.FormatParameter.XDecimalPlaces, stateMachine.FormatParameter.LeadingZeroMode);
                }
            }

            // LOOK FOR THE Y TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'Y', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // this is not an error - just means we did not find one
            }
            else
            {
                // this will have a float number
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(processedLineStr, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("ParseLine(DY) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 233;
                }
                else
                {
                    // set the value now
                    dCodeYCoord = DecimalScaleNumber(outFloat, stateMachine.FormatParameter.YIntegerPlaces, stateMachine.FormatParameter.YDecimalPlaces, stateMachine.FormatParameter.LeadingZeroMode);
                }
            }

            // LOOK FOR THE D TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'D', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // this is not an error - just means we did not find one
            }
            else
            {
                // this will have a float number
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetInteger(processedLineStr, nextStartPos, ref outInt, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("ParseLine(DD) failed on call to ParseNumberFromString_TillNonDigit_RetInteger");
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