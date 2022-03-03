using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    /// A class to encapsulate a gerber AD Code
    /// </summary>
    public class GerberLine_ADCode : GerberLine
    {
        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        // this is our aperture
        private GerberAperture_Base adCodeAperture = null;

        public const int DEFAULT_APERTURE_DNUMBER = -1;
        private int dNumber = DEFAULT_APERTURE_DNUMBER;

        // aperture defintitions not all are used by every aperture type
        /// These values are the decimal scaled values from the DCode itself. They
        /// are not yet scaled to plot coordinates.
        //private float outerDiameter = 0;
        //private float xAxisHoleDimension = 0;
        //private float yAxisHoleDimension = 0;
        //private float xAxisDimension = 0;
        //private float yAxisDimension = 0;
        //private float numSides = 0;
        //private float degreesRotation = 0;
        //private float outsideDimension = 0;

        // pens are only valid for a specific angle
        private Pen hPen = null;  // pen for horizontal lines
        private Pen vPen = null;  // pen for vertical lines
        private Pen gPen = null;  // general pen
        private float gPenAngle = 0;  // angle current general pen is set for

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        public GerberLine_ADCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current aperture object for this code. This can be null. There
        /// is no set accessor. The aperture is created at construction time.
        /// </summary>
        public GerberAperture_Base ADCodeAperture
        {
            get
            {
                return adCodeAperture;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates maximum width of the aperture in any direction
        /// from a point. 
        /// </summary>
        public float GetApertureDimension()
        {
            if (ADCodeAperture == null) return 0;
            return ADCodeAperture.GetApertureDimension();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a pen with a width suitable for the aperture and the angle it is
        /// drawing at. We have to do it this way because non symmetrical apertures
        /// (an elipse for example) can have different widths depending on the 
        /// angle it is being drawn at. In order to avoid to much creation and 
        /// destruction of pens we try to save them for re-use if we can.
        /// have dif
        /// </summary>
        /// <remarks>In general, if a coordinate is an int it has been scaled and it represents
        ///       a value in plot coordinates. If it is a float it represents an unscaled
        ///       value from the gerber file or gCode file
        /// </remarks>
        /// <param name="bottom">point on line pen will draw we use this to get the angle</param>
        /// <param name="top">point on line pen will draw we use this to get the angle</param>
        /// <param name="left">point on line pen will draw we use this to get the angle</param>
        /// <param name="right">point on line pen will draw we use this to get the angle</param>
        public Pen GetWorkingPen(GerberFileStateMachine stateMachine, int left, int bottom, int right, int top)
        {
            if (stateMachine == null) return null;

            int workingAngle = CalcPenAngle(left, bottom, right, top);
        //    LogMessage("GetWorkingPen angle =" + workingAngle.ToString());

            int penWidth = 1;
            if (workingAngle == 0)
            {
                if (hPen == null)
                {
                    penWidth = GetPenWidthForAngle(stateMachine, workingAngle);
                    hPen = new Pen(stateMachine.GerberForegroundColor, penWidth);
                    hPen.Alignment = PenAlignment.Center;

                }
                return hPen;
            }
            else if (workingAngle == 90)
            {
                if (vPen == null)
                {
                    penWidth = GetPenWidthForAngle(stateMachine, workingAngle);
                    vPen = new Pen(stateMachine.GerberForegroundColor, penWidth);
                    vPen.Alignment = PenAlignment.Center;
                }
                return vPen;
            }
            else
            {
                // general angle
                if (gPen == null)
                {
                    penWidth = GetPenWidthForAngle(stateMachine, workingAngle);
                    gPen = new Pen(stateMachine.GerberForegroundColor, penWidth);
                    gPen.Alignment = PenAlignment.Center;
                    gPenAngle = workingAngle;
                }
                else
                {
                    // we have a pen but is it suitable for the current angle
                    if (workingAngle == gPenAngle)
                    {
                        // yes it is, just return it
                        return gPen;
                    }
                    else
                    {
                        gPen.Dispose();
                        gPen = null;
                        penWidth = GetPenWidthForAngle(stateMachine, workingAngle);
                        gPen = new Pen(stateMachine.GerberForegroundColor, penWidth);
                        gPenAngle = workingAngle;
                    }
                }
                return gPen;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a pen width for a line based on the current aperture
        /// </summary>
        /// <remarks>In general, if a coordinate is an int it has been scaled and it represents
        ///       a value in plot coordinates. If it is a float it represents an unscaled
        ///       value from the gerber file or gCode file
        /// </remarks>
        /// <param name="bottom">point on line pen will draw we use this to get the angle</param>
        /// <param name="top">point on line pen will draw we use this to get the angle</param>
        /// <param name="left">point on line pen will draw we use this to get the angle</param>
        /// <param name="right">point on line pen will draw we use this to get the angle</param>
        /// <param name="stateMachine">the state machine with gerber file details</param>
        public int GetPenWidthForLine(GerberFileStateMachine stateMachine, int left, int bottom, int right, int top)
        {
            if (stateMachine == null) return 1;

            int workingAngle = CalcPenAngle(left, bottom, right, top);
            //    DebugMessage("GetPenWidthForLine angle =" + workingAngle.ToString());
            return GetPenWidthForAngle(stateMachine, workingAngle);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the width a pen should be for a specific angle.  The width
        /// is returned in pixel units which means all dimensions are scaled up
        /// by 
        /// </summary>
        /// <param name="stateMachine">the state machine with gerber file details</param>
        /// <param name="workingAngle">the angle the aperture will move at in degrees</param>
        public int GetPenWidthForAngle(GerberFileStateMachine stateMachine, int workingAngle)
        {
            if (ADCodeAperture == null) return 0;
            try
            {
                return ADCodeAperture.GetPenWidthForAngle(stateMachine, workingAngle);
            }
            catch (Exception ex)
            {
                // rethrow with line number
                throw new Exception("Line Number:" + LineNumber.ToString() + ", " + ex.Message);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the X and Y compensation for a line based on the isolation routing
        /// distance and the angle of the line. this is the amount we need to adjust
        /// +/- the endpoints by to account for the fact that we are routing around
        /// the end of it
        /// </summary>
        /// <param name="stateMachine">the state machine with gerber file details</param>
        /// <returns>
        /// the xyCompensation or -ve for fail
        /// </returns>
        public int GetIsolationLineXYCompensation(GerberFileStateMachine stateMachine)
        {
            if(stateMachine==null) return -1;
            // calculate it this way
            return (int)Math.Round(((stateMachine.IsolationWidth * stateMachine.IsoPlotPointsPerAppUnit) / 2));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates a pen angle. we only do this to the nearest 10 degrees. All
        /// values are returned between 0 and 90 degrees
        /// </summary>
        /// <remarks>In general, if a coordinate is an int it has been scaled and it represents
        ///       a value in plot coordinates. If it is a float it represents an unscaled
        ///       value from the gerber file or gCode file
        /// </remarks>
        /// <returns>angle between line and X axis from 0 to 90 deg</returns>
        private int CalcPenAngle(int left, int bottom, int right, int top)
        {
            int numerator = top - bottom;
            int denominator = right - left;

            if (denominator == 0) return 90;

            double outValRadians = Math.Atan(((double)numerator / (double)denominator));
            // use 18 instead of 180 - so we can round
            double outValDegrees = (18.0 / Math.PI) * outValRadians;

            return (int)Math.Abs(Math.Round(outValDegrees, 0) * 10);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Dispose of all pens
        /// </summary>
        public void DisposeAllPens()
        {
            if (hPen != null)
            {
                hPen.Dispose();
                hPen = null;
            }
            if (vPen != null)
            {
                vPen.Dispose();
                vPen = null;
            }
            if (gPen != null)
            {
                gPen.Dispose();
                gPen = null;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current D Number. There is no set accessor
        /// as this is derived during the population with the parameter.
        /// </summary>
        public int DNumber
        {
            get
            {
                return dNumber;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the aperture type. There is no set accessor, this is set in 
        /// the constructor
        /// </summary>
        public ApertureTypeEnum ApertureType
        {
            get
            {
                if (ADCodeAperture == null) return ApertureTypeEnum.APERTURETYPE_UNKNOWN;
                return ADCodeAperture.ApertureType;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Set the aperature object from a character input
        /// </summary>
        /// <param name="apertureString">A value of C, R, O, P or named aperture for the aperture type</param>
        /// <returns>true - succes, false fail</returns>
        public GerberAperture_Base GetApertureObjFromString(string apertureString)
        {
            string apStr = "";

            if (apertureString == null) return null;
            if (apertureString == "") return null;

            // figure out what we are dealing with. Everything from the start to 
            // the first ',' will be the thing we trigger on
            int index = apertureString.IndexOf(',');
            if (index <= 0) apStr = apertureString; // probably a macro which does not have any parameters
            else apStr = apertureString.Substring(0, index);

            if (apStr == null) return null;
            if (apStr == "") return null;
            if (apStr.Length == 0) return null;


            if (apStr == "C")
            {
                return new GerberAperture_CCode();
            }
            else if (apStr == "R")
            {
                return new GerberAperture_RCode();
            }
            else if (apStr == "O")
            {
                return new GerberAperture_OCode();
            }
            else if (apStr == "P")
            {
                return new GerberAperture_PCode();
            }
            else
            {
                // could be a macro at this point, the apStr will be the name
                return new GerberAperture_Macro(apStr);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If we draw a line with an aperture it will not always have linear ends
        /// for example if the aperture is a circle it will have half circle ends. Each
        /// line that is drawn calls this to make it look right visually
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="workingPen">the pen used for the line, we get the width from this</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <param name="x2">the second x value</param>
        /// <param name="y2">the second y value</param>
        /// <returns>z success, nz fail</returns>
        public void FixupLineEndpointsForGerberPlot(GerberFileStateMachine stateMachine, Graphics graphicsObj, Brush workingBrush, Pen workingPen, float x1, float y1, float x2, float y2)
        {
            if (ADCodeAperture == null) return;
            try
            {
                ADCodeAperture.FixupLineEndpointsForGerberPlot(stateMachine, graphicsObj, workingBrush,workingPen, x1, y1, x2, y2);
            }
            catch (Exception ex)
            {
                // rethrow with line number
                throw new Exception("Line Number:" + LineNumber.ToString() + ", " + ex.Message);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If we fill a shape with the aperture at the current coordinates. This 
        /// simulates the aperture flash of a Gerber plotter
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="x1">the x center value</param>
        /// <param name="y1">the y center value</param>
        /// <returns>z success, nz fail</returns>
        public void FlashApertureForGerberPlot(GerberFileStateMachine stateMachine, Graphics graphicsObj, Brush workingBrush, float x1, float y1)
        {
            if (ADCodeAperture == null) return;
            try
            {
                // macros need to reset
                ADCodeAperture.ResetForFlash();
                // flash it
                ADCodeAperture.FlashApertureForGerberPlot(stateMachine, graphicsObj, workingBrush, x1, y1);
            }
            catch (Exception ex)
            {
                // rethrow with line number
                throw new Exception("Line Number:" + LineNumber.ToString() + ", " + ex.Message);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If we draw a line with an aperture it will not always have linear ends
        /// for example if the aperture is a circle it will have half circle ends. Each
        /// line that is drawn calls this to make it look right visually
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="workingPen">the pen used for the line, we get the width from this</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <param name="x2">the second x value</param>
        /// <param name="y2">the second y value</param>
        /// <param name="xyComp">the xy compensation factor</param>
        /// <returns>z success, nz fail</returns>
        public void FixupLineEndpointsForGCodePlot(IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int x2, int y2, int radius, int xyComp)
        {
            if (isoPlotBuilder == null) return;
            if (stateMachine == null) return;

            if (ADCodeAperture == null) return;
            try
            {
                ADCodeAperture.FixupLineEndpointsForGCodePlot(isoPlotBuilder, stateMachine, x1, y1, x2, y2, radius, xyComp);
            }
            catch (Exception ex)
            {
                // rethrow with line number
                throw new Exception("Line Number:" + LineNumber.ToString() + ", " + ex.Message);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flash the aperture
        /// </summary>
        /// <param name="isoPlotBuilder">the builder opbject</param>
        /// <param name="stateMachine">the statemachine</param>
        /// <param name="xyComp">the xy compensation factor</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <returns>z success, nz fail</returns>
        public void FlashApertureForGCodePlot(IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int xyComp)
        {
            if (ADCodeAperture == null) return;
            try
            {
                // macros need to reset
                ADCodeAperture.ResetForFlash();
                // flash it
                ADCodeAperture.FlashApertureForGCodePlot(isoPlotBuilder, stateMachine, x1, y1, xyComp);
            }
            catch (Exception ex)
            {
                // rethrow with line number
                throw new Exception("Line Number:" + LineNumber.ToString() + ", " + ex.Message);
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
            //DebugMessage("ParseParameter(AD) started");

            if (processedLineStr == null) return 100;
            if (processedLineStr.StartsWith(GerberFile.RS274_AD_CMD) == false) return 200;

            // convert to a character array
            char[] adChars = processedLineStr.ToCharArray();
            if (adChars == null) return 300;
            if (adChars.Length < 5) return 400;
            // we have to have a 'D' here to define the D number
            if (adChars[2] != 'D') return 200;
            int retInt = PopulateFromParameter(processedLineStr);
            if (retInt != 0)
            {
                LogMessage("ParseParameter(AD) failed on call to apObj.PopulateFromParameter, processedLineStr is:" + processedLineStr + " retValue is " + retInt.ToString());
                return 101;
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Populate the object from an Aperture Definition prarameter
        /// </summary>
        /// <param name="adParamBlock">The AD string param block, no param, or block
        /// delimiters. Just the AD block</param>
        /// <returns>z success, nz fail</returns>
        public int PopulateFromParameter(string adParamBlock)
        {
            //DebugMessage("PopulateFromParameter() started");
            int outInt = -1;
            int nextStartPos = -1;
            bool retBool;

            if (adParamBlock == null) return 100;
            if (adParamBlock.StartsWith(GerberFile.RS274_AD_CMD) == false) return 200;

            // convert to a character array
            char[] adChars = adParamBlock.ToCharArray();
            if (adChars == null) return 300;
            if (adChars.Length < 5) return 400;
            // we have to have a 'D' here to define the D number
            if (adChars[2] != 'D') return 200;

            // get the D number
            retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetInteger(adParamBlock, 3, ref outInt, ref nextStartPos);
            if (retBool != true)
            {
                LogMessage("PopulateFromParameter failed on call to ParseNumberFromString_TillNonDigit_RetInteger");
                return 322;
            }
            else
            {
                // set the dNumber now
                dNumber = outInt;
            }

            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                LogMessage("PopulateFromParameter malformed adParamBlock. No data after Dnumber");
                return 422;
            }

            // at this point we have to take care. if the aperature definitition contains one of a
            // specific set of chars then we are dealing with a normal aperture. If it does not it must 
            // be a named aperture referring to a macro

            // set the aperture object
            adCodeAperture = GetApertureObjFromString(adParamBlock.Substring(nextStartPos));
            nextStartPos++;
            if (adCodeAperture == null)
            {
                LogMessage("PopulateFromParameter failed on call to GetApertureObjFromCharacter");
                return 522;
            }
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                LogMessage("PopulateFromParameter malformed adParamBlock. No data after ADType");
                return 622;
            }

            // re-sync and position just after the ',' char in the aperture parameter definition
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, ',', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                if ((adCodeAperture is GerberAperture_Macro) == false)
                {
                    // have to have this for normal apertures
                    LogMessage("PopulateFromParameter malformed adParamBlock. No data after comma");
                    return 722;
                }
                else
                {
                    // we are a macro aperture with no params
                    nextStartPos = adParamBlock.Length;
                }
            }

            // now let the aperture object populate itself
            int retInt = adCodeAperture.PopulateFromParameter(adParamBlock, nextStartPos);
            if (retInt != 0)
            {
                LogMessage("PopulateFromParameter call to adCodeAperture.PopulateFromParameter returned " + retInt.ToString());
                return 723;
            }

            return 0;
        }

    }
}

