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
    /// <history>
    ///    07 Jul 10  Cynic - Started
    /// </history>
    public class GerberLine_ADCode : GerberLine
    {
        // defines the states, makes them more readable
        public enum ApertureTypeEnum
        {
            APERTURETYPE_CIRCLE,
            APERTURETYPE_RECTANGLE,
            APERTURETYPE_OVAL,
            APERTURETYPE_POLYGON,
        };

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        public const ApertureTypeEnum DEFAULT_APERTURETYPE = ApertureTypeEnum.APERTURETYPE_CIRCLE;
        public const int DEFAULT_APERTURE_DNUMBER = -1;

        private ApertureTypeEnum apertureType = DEFAULT_APERTURETYPE;
        private int dNumber = DEFAULT_APERTURE_DNUMBER;

        // aperture defintitions not all are used by every aperture type
        /// These values are the decimal scaled values from the DCode itself. They
        /// are not yet scaled to plot coordinates.
        private float outerDiameter = 0;
        private float xAxisHoleDimension = 0;
        private float yAxisHoleDimension = 0;
        private float xAxisDimension = 0;
        private float yAxisDimension = 0;
        private float numSides = 0;
        private float degreesRotation = 0;
        private float outsideDimension = 0;

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
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public GerberLine_ADCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates maximum distance in any direction this aperture will draw
        /// from a point. 
        /// </summary>
        /// <history>
        ///    08 Aug 10  Cynic - Started
        /// </history>
        public float GetApertureIncrementalDimension()
        {
            // it is just half of this
            return GetApertureDimension()/2;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates maximum width of the aperture in any direction
        /// from a point. 
        /// </summary>
        /// <history>
        ///    09 Sep 10  Cynic - Started
        /// </history>
        public float GetApertureDimension()
        {
            switch (apertureType)
            {
                case ApertureTypeEnum.APERTURETYPE_OVAL:
                    // the incremental dimension for an ellipse is the larger of the
                    // two axis dimensions 
                    if (xAxisDimension > yAxisDimension) return xAxisDimension;
                    else return yAxisDimension;
                case ApertureTypeEnum.APERTURETYPE_POLYGON:
                    // the incremental dimension for an polygon is outsideDimension
                    return outsideDimension;
                case ApertureTypeEnum.APERTURETYPE_RECTANGLE:
                    // the incremental dimension for a rectangle is the larger of the
                    // two axis dimensions
                    if (xAxisDimension > yAxisDimension) return xAxisDimension;
                    else return yAxisDimension;
                case ApertureTypeEnum.APERTURETYPE_CIRCLE:
                    // the incremental dimension for a circle is the radius
                    return this.outerDiameter;
                default:
                    return 0;
            }
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
        /// <history>
        ///    13 Jul 10  Cynic - Started
        /// </history>
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
                    hPen = new Pen(stateMachine.PlotLineColor, penWidth);
                    hPen.Alignment = PenAlignment.Center;

                }
                return hPen;
            }
            else if (workingAngle == 90)
            {
                if (vPen == null)
                {
                    penWidth = GetPenWidthForAngle(stateMachine, workingAngle);
                    vPen = new Pen(stateMachine.PlotLineColor, penWidth);
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
                    gPen = new Pen(stateMachine.PlotLineColor, penWidth);
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
                        gPen = new Pen(stateMachine.PlotLineColor, penWidth);
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
        /// <history>
        ///    28 Jul 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    14 Jul 10  Cynic - Started
        /// </history>
        public int GetPenWidthForAngle(GerberFileStateMachine stateMachine, int workingAngle)
        {
            float retVal;
            float workingAngleInRadians;

            switch(apertureType)
            {
                case ApertureTypeEnum.APERTURETYPE_OVAL:
                    // the formula for this is 1/SQRT(((Cos(theta)**2)/(a^2)) + ((sin(theta)**2)/(b^2)) 
                    // a is the length of the major axis in the xdirection, b is the axis in the y direction
                    float a2 = xAxisDimension * xAxisDimension;
                    float b2 = yAxisDimension * yAxisDimension;
                    workingAngleInRadians = (float)(((float)workingAngle) * Math.PI / 180);
                    float cosTheta = (float)Math.Cos(workingAngleInRadians);
                    float sinTheta = (float)Math.Sin(workingAngleInRadians);
                    retVal = (float)(1/Math.Sqrt((cosTheta * cosTheta)/b2 + (sinTheta * sinTheta)/a2));
                    if (retVal <= 0) return 1;
                    else return (int)Math.Round((retVal * stateMachine.IsoPlotPointsPerAppUnit));
                case ApertureTypeEnum.APERTURETYPE_POLYGON:
                    throw new NotImplementedException("Polygonal aperture on line " + LineNumber.ToString() + " is not supported.");
                case ApertureTypeEnum.APERTURETYPE_RECTANGLE:
                    // the formula for this is abs(h*(cos(theta - arctan(y/x)) where 
                    // h is the length of the hypotenuse and x,y are half the width+height
                    float x = xAxisDimension/2;
                    float y = yAxisDimension/2;
                    workingAngleInRadians = (float)(((float)workingAngle)*Math.PI/180);
                    float h = (float)Math.Sqrt((xAxisDimension * xAxisDimension) + (yAxisDimension * yAxisDimension));
                    retVal = (float)(Math.Abs(h * (float)Math.Cos(workingAngleInRadians - Math.Atan(x / y))));
                    if (retVal == 0) return 1;
                    else return (int)Math.Round((retVal * stateMachine.IsoPlotPointsPerAppUnit));
                case ApertureTypeEnum.APERTURETYPE_CIRCLE:
                    // the aperture of a circle for any angle is just the diameter
                    return (int)Math.Round((this.outerDiameter * stateMachine.IsoPlotPointsPerAppUnit));
                default:
                    return 5;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the X and Y compensation for a line based on the isolation routing
        /// distance and the angle of the line. this is the amount we need to adjust
        /// +/- the endpoints by to account for the fact that we are routing around
        /// the end of it
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
        /// <param name="isolationCenterDistance">The distance to the center of the isolation cut. 
        /// ie half the isolation width scaled to plot coordinates</param>
        /// <param name="xComp">+/- compensation in the x direction. This is returned in screen coordinates</param>
        /// <param name="yComp">+/- compensation in the y direction. This is returned in screen coordinates</param>
        /// <param name="xyComp">+/- compensation in the direction of the line. This will always just be 
        /// the isolationCenterDistance scaled to screen coordinates</param>
        /// <history>
        ///    29 Jul 10  Cynic - Started
        /// </history>
        public bool GetIsolationLineXYCompensation(GerberFileStateMachine stateMachine, int left, int bottom, int right, int top, int isolationCenterDistance, out int xComp, out int yComp, out int xyComp)
        {
            xComp = -1;
            yComp = -1;
            xyComp = -1;
            if (isolationCenterDistance <= 0) return false;
            // get the angle
            int angle = CalcPenAngle(left, bottom, right, top);
            // figure these out
            xComp = (int)Math.Round((((double)isolationCenterDistance) * Math.Cos(angle)));
            yComp = (int)Math.Round((((double)isolationCenterDistance) * Math.Sin(angle)));
            // this is always this
            xyComp = isolationCenterDistance;
            return true;
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
        /// <history>
        ///    14 Jul 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    13 Jul 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    06 Jul 10  Cynic - Started
        /// </history>
        public int DNumber
        {
            get
            {
                return dNumber;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current coordinate mode. There is no set accessor
        /// as this is derived during the population with the parameter.
        /// </summary>
        /// <history>
        ///    06 Jul 10  Cynic - Started
        /// </history>
        public ApertureTypeEnum ApertureType
        {
            get
            {
                return apertureType;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Populate the object from an Aperture Definition prarameter
        /// </summary>
        /// <param name="adParamBlock">The AD string param block, no param, or block
        /// delimiters. Just the AD block</param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    06 Jul 10  Cynic - Started
        /// </history>
        public int PopulateFromParameter(string adParamBlock)
        {
            //DebugMessage("PopulateFromParameter() started");
            int outInt = -1;
            float outFloat = 0;
            int nextStartPos = -1;
            bool retBool;

            if (adParamBlock == null) return 100;
            if (adParamBlock.StartsWith(GerberFile.RS274APDEFPARAM) == false) return 200;

            // convert to a character array
            char[] adChars = adParamBlock.ToCharArray();
            if (adChars == null) return 300;
            if (adChars.Length < 5) return 400;
            // we have to have a 'D' here to define the D number
            if (adChars[2] != 'D') return 200;
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

            if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
            {
                LogMessage("PopulateFromParameter malformed adParamBlock. No data after Dnumber");
                return 422;
            }

            // now set the aperture type - it will be next in the sequence
            retBool = SetApertureTypeFromCharacter(adChars[nextStartPos]);
            nextStartPos++;
            if (retBool != true)
            {
                LogMessage("PopulateFromParameter failed on call to SetApertureTypeFromCharacter");
                return 522;
            }
            if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
            {
                LogMessage("PopulateFromParameter malformed adParamBlock. No data after ApType");
                return 622;
            }

            // re-sync and position just after the ',' char in the aperture parameter definition
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, ',', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
            {
                LogMessage("PopulateFromParameter malformed adParamBlock. No data after comma");
                return 722;
            }

            // now process the definitions specific to the aperture type
            if (apertureType == ApertureTypeEnum.APERTURETYPE_CIRCLE)
            {
                // od x <XAxisHoleDimension> x <YAxisHoleDimension>
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Ca) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 133;
                }
                else
                {
                    // set the value now
                    outerDiameter = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // we are done
                    return 0;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                // ###
                // These are optional
                // ###
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Ce) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 135;
                }
                else
                {
                    // set the value now
                    xAxisHoleDimension = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                else
                {
                    // set the value now
                    yAxisHoleDimension = outFloat;
                }
                return 0;
            }
            else if (apertureType == ApertureTypeEnum.APERTURETYPE_RECTANGLE)
            {
                // XAxisDimension x YAxisDimension <XAxisHoleDimension> x <YAxisHoleDimension>
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Ra) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 143;
                }
                else
                {
                    // set the value now
                    xAxisDimension = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    LogMessage("PopulateFromParameter (Rb) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 1431;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    LogMessage("PopulateFromParameter (Rc) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 1431;
                }
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Rd) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 144;
                }
                else
                {
                    // set the value now
                    yAxisDimension = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // we are done
                    return 0;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                // ###
                // These are optional
                // ###
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Re) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 145;
                }
                else
                {
                    // set the value now
                    xAxisHoleDimension = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                else
                {
                    // set the value now
                    yAxisHoleDimension = outFloat;
                }
                return 0;
            }
            else if (apertureType == ApertureTypeEnum.APERTURETYPE_OVAL)
            {
                // XAxisDimension x YAxisDimension <XAxisHoleDimension> x <YAxisHoleDimension>
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Oa) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 163;
                }
                else
                {
                    // set the value now
                    xAxisDimension = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    LogMessage("PopulateFromParameter (Ob) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 1631;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    LogMessage("PopulateFromParameter (Oc) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 1631;
                }
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Od) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 164;
                }
                else
                {
                    // set the value now
                    yAxisDimension = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // we are done
                    return 0;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                // ###
                // These are optional
                // ###
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Oe) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 165;
                }
                else
                {
                    // set the value now
                    xAxisHoleDimension = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                else
                {
                    // set the value now
                    yAxisHoleDimension = outFloat;
                }
                return 0;
            }
            else if (apertureType == ApertureTypeEnum.APERTURETYPE_POLYGON)
            {
                // OutsideDimension x NumSides X <degreesRotation> x <XAxisHoleDimension> x <YAxisHoleDimension>
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Pa) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 163;
                }
                else
                {
                    // set the value now
                    outsideDimension = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    LogMessage("PopulateFromParameter (Pb) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 1631;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    LogMessage("PopulateFromParameter (Pc) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 1631;
                }
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Pd) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 164;
                }
                else
                {
                    // set the value now
                    numSides = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // we are done
                    return 0;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                // ###
                // These are optional
                // ###
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    LogMessage("PopulateFromParameter (Pe) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 165;
                }
                else
                {
                    // set the value now
                    degreesRotation = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                else
                {
                    // set the value now
                    xAxisHoleDimension = outFloat;
                }
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
                if ((nextStartPos < 0) || (nextStartPos > adChars.Length))
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
                if (retBool != true)
                {
                    // this is not an error - just means we are all done
                    return 0;
                }
                else
                {
                    // set the value now
                    yAxisHoleDimension = outFloat;
                }
                return 0;
            }
            else
            {
                LogMessage("PopulateFromParameter malformed adParamBlock. invalid aperture type");
                return 173;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Set the aperature type from a character input
        /// </summary>
        /// <param name="apChar">A value of C, R, O, P for the aperture type</param>
        /// <returns>true - succes, false fail</returns>
        /// <history>
        ///    06 Jul 10  Cynic - Started
        /// </history>
        public bool SetApertureTypeFromCharacter(char apChar)
        {
            if (apChar == 'C')
            {
                apertureType = ApertureTypeEnum.APERTURETYPE_CIRCLE;
                return true;
            }
            else if (apChar == 'R')
            {
                apertureType = ApertureTypeEnum.APERTURETYPE_RECTANGLE;
                return true;
            }
            else if (apChar == 'O')
            {
                apertureType = ApertureTypeEnum.APERTURETYPE_OVAL;
                return true;
            }
            else if (apChar == 'P')
            {
                throw new NotImplementedException("Polygonal aperture on line "+ LineNumber.ToString() + " is not supported.");
               // apertureType = ApertureTypeEnum.APERTURETYPE_POLYGON;
               // return true;
            }
            else
            {
                return false;
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
        /// <history>
        ///    15 Jul 10  Cynic - Started
        /// </history>
        public void FixupLineEndpointsForGerberPlot(GerberFileStateMachine stateMachine, Graphics graphicsObj, Brush workingBrush, Pen workingPen, float x1, float y1, float x2, float y2)
        {
            int xlen;
            int ylen;
            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            if (workingPen == null) return;

            switch (apertureType)
            {
                case ApertureTypeEnum.APERTURETYPE_OVAL:
                    // fix up the two end points
                    MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, workingBrush, x1, y1, workingPen.Width, workingPen.Width);
                    MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, workingBrush, x2, y2, workingPen.Width, workingPen.Width);
                    return;
                case ApertureTypeEnum.APERTURETYPE_POLYGON:
                    throw new NotImplementedException("Polygonal aperture on line " + LineNumber.ToString() + " is not supported.");
                case ApertureTypeEnum.APERTURETYPE_RECTANGLE:
                    xlen = (int)Math.Round((this.xAxisDimension * stateMachine.IsoPlotPointsPerAppUnit));
                    ylen = (int)Math.Round((this.yAxisDimension * stateMachine.IsoPlotPointsPerAppUnit));
                    MiscGraphicsUtils.FillRectangleCenteredOnPoint(graphicsObj, workingBrush, x1, y1, xlen, ylen);
                    MiscGraphicsUtils.FillRectangleCenteredOnPoint(graphicsObj, workingBrush, x2, y2, xlen, ylen);
                    return;
                case ApertureTypeEnum.APERTURETYPE_CIRCLE:
                    // fix up the two end points
                    MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, workingBrush, x1, y1, workingPen.Width, workingPen.Width);
                    MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, workingBrush, x2, y2, workingPen.Width, workingPen.Width);
                    return;
                default:
                    return;
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
        /// <history>
        ///    15 Jul 10  Cynic - Started
        /// </history>
        public void FlashApertureForGerberPlot(GerberFileStateMachine stateMachine, Graphics graphicsObj, Brush workingBrush, float x1, float y1)
        {
            int xlen;
            int ylen;
            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            if (stateMachine == null) return;

            switch (apertureType)
            {
                case ApertureTypeEnum.APERTURETYPE_OVAL:
                    xlen = (int)Math.Round((this.xAxisDimension * stateMachine.IsoPlotPointsPerAppUnit));
                    ylen = (int)Math.Round((this.yAxisDimension * stateMachine.IsoPlotPointsPerAppUnit));
                    MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, workingBrush, x1, y1, xlen, ylen);
                    // do we need to draw in the hole?
                    if ((xAxisHoleDimension > 0) && (yAxisHoleDimension > 0))
                    {
                        // yes we do
                        xlen = (int)Math.Round((this.xAxisHoleDimension * stateMachine.IsoPlotPointsPerAppUnit));
                        ylen = (int)Math.Round((this.yAxisHoleDimension * stateMachine.IsoPlotPointsPerAppUnit));
                        MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, stateMachine.PlotBackgroundBrush, x1, y1, xlen, ylen);
                    }
                    return;
                case ApertureTypeEnum.APERTURETYPE_POLYGON:
                    throw new NotImplementedException("Polygonal aperture on line " + LineNumber.ToString() + " is not supported.");
                case ApertureTypeEnum.APERTURETYPE_RECTANGLE:
                    xlen = (int)Math.Round((this.xAxisDimension * stateMachine.IsoPlotPointsPerAppUnit));
                    ylen = (int)Math.Round((this.yAxisDimension * stateMachine.IsoPlotPointsPerAppUnit));
                    MiscGraphicsUtils.FillRectangleCenteredOnPoint(graphicsObj, workingBrush, x1, y1, xlen, ylen);
                    // do we need to draw in the hole?
                    if ((xAxisHoleDimension > 0) && (yAxisHoleDimension > 0))
                    {
                        // yes we do
                        xlen = (int)Math.Round((this.xAxisHoleDimension * stateMachine.IsoPlotPointsPerAppUnit));
                        ylen = (int)Math.Round((this.yAxisHoleDimension * stateMachine.IsoPlotPointsPerAppUnit));
                        MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, stateMachine.PlotBackgroundBrush, x1, y1, xlen, ylen);
                    }
                    return;
                case ApertureTypeEnum.APERTURETYPE_CIRCLE:
                    int circleDiameter = (int)Math.Round((this.outerDiameter * stateMachine.IsoPlotPointsPerAppUnit));
                    MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, workingBrush, x1, y1, circleDiameter, circleDiameter);
                    // do we need to draw in the hole?
                    if ((xAxisHoleDimension > 0) && (yAxisHoleDimension > 0))
                    {
                        // yes we do
                        xlen = (int)Math.Round((this.xAxisHoleDimension * stateMachine.IsoPlotPointsPerAppUnit));
                        ylen = (int)Math.Round((this.yAxisHoleDimension * stateMachine.IsoPlotPointsPerAppUnit));
                        MiscGraphicsUtils.FillElipseCenteredOnPoint(graphicsObj, stateMachine.PlotBackgroundBrush, x1, y1, xlen, ylen);
                    }
                    return;
                default:
                    return;
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
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public void FixupLineEndpointsForGCodePlot(GCodeBuilder gCodeBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int x2, int y2, int radius, int xyComp)
        {
            if (gCodeBuilder == null) return;
            if (stateMachine == null) return;

            switch (apertureType)
            {
                case ApertureTypeEnum.APERTURETYPE_OVAL:
                    // fix up the two end points
                    gCodeBuilder.DrawGSCircle(x1, y1, radius, true);
                    gCodeBuilder.DrawGSCircle(x2, y2, radius, true);
                    return;
                case ApertureTypeEnum.APERTURETYPE_POLYGON:
                    throw new NotImplementedException("Polygonal aperture on line " + LineNumber.ToString() + " is not supported.");
                case ApertureTypeEnum.APERTURETYPE_RECTANGLE:
                    int xComp = (int)Math.Round(((xAxisDimension*stateMachine.IsoPlotPointsPerAppUnit)));
                    int yComp = (int)Math.Round(((yAxisDimension*stateMachine.IsoPlotPointsPerAppUnit)/2));
                    gCodeBuilder.DrawGSLineOutLine(x1, y1 - yComp - xyComp, x1, y1 + yComp + xyComp, xComp + (2 * xyComp), true);
                    gCodeBuilder.DrawGSLineOutLine(x2, y2 - yComp - xyComp, x2, y2 + yComp + xyComp, xComp + (2 * xyComp), true);
                    return;
                case ApertureTypeEnum.APERTURETYPE_CIRCLE:
                    // fix up the two end points
                    gCodeBuilder.DrawGSCircle(x1, y1, radius, true);
                    gCodeBuilder.DrawGSCircle(x2, y2, radius, true);
                    return;
                default:
                    return;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If we draw a line with an aperture it will not always have linear ends
        /// for example if the aperture is a circle it will have half circle ends. Each
        /// line that is drawn calls this to make it look right visually
        /// </summary>
        /// <param name="gCodeBuilder">the builder opbject</param>
        /// <param name="stateMachine">the statemachine</param>
        /// <param name="xyComp">the xy compensation factor</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    20 Aug 10  Cynic - Started
        ///    30 Jan 11  Cynic - Added MaikF's bugfix to the APERTURETYPE_RECTANGLE section
        /// </history>
        public void FlashApertureForGCodePlot(GCodeBuilder gCodeBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int xyComp)
        {
            bool ellipseIsVertical = false;
            int workingOuterDiameter = int.MinValue;
            float majorEllipseAxis = float.NegativeInfinity;
            float minorEllipseAxis = float.NegativeInfinity;
            float majorEllipseAxisAdjustedForCircle = float.NegativeInfinity;
            int x1EllipseCompensatedEndpoint;
            int y1EllipseCompensatedEndpoint;
            int x2EllipseCompensatedEndpoint;
            int y2EllipseCompensatedEndpoint;
            int majorEllipseAxisEndpointCompensation = 0;
            if (gCodeBuilder == null) return;
            if (stateMachine == null) return;

            switch (apertureType)
            {
                case ApertureTypeEnum.APERTURETYPE_OVAL:

                    // NOTE: it is not possible to encode an Oval or Ellipse in GCode, you have to
                    //       represent this with a line segment which has two circles at the end
                    //       the total length should be equal the longest dimension. The shortest
                    //       dimension will be used as the diameter of the circles.

                    // NOTE: we need not deal with ellipses in here which do not have major and minor
                    //       axis parallel to the X and Y axis. There is no way to represent this in
                    //       Gerber code and they are usually encoded as an AM macro which is a 
                    //       combination of two circles and a line - just as we are doing (see above)
                    //       and for exactly the same reasons.

                    // find the shortest and longest dimensions
                    if (xAxisDimension > yAxisDimension)
                    {
                        majorEllipseAxis = xAxisDimension;
                        minorEllipseAxis = yAxisDimension;
                        ellipseIsVertical = false;
                    }
                    else
                    {
                        majorEllipseAxis = yAxisDimension;
                        minorEllipseAxis = xAxisDimension;
                        ellipseIsVertical = true;
                    }
                    // now figure out the length of the line between the circles
                    majorEllipseAxisAdjustedForCircle = majorEllipseAxis - minorEllipseAxis;
                    // sanity checks
                    if (majorEllipseAxisAdjustedForCircle < 0)
                    {
                        // what the hell? we got problems
                        throw new Exception("Cannot draw ellipse on line " + LineNumber.ToString() + " adjusted axis less than zero.");
                    }
                    else if (majorEllipseAxisAdjustedForCircle == 0)
                    {
                        // we must be dealing with a circle, just draw one
                        workingOuterDiameter = (int)Math.Round((majorEllipseAxis * stateMachine.IsoPlotPointsPerAppUnit));
                        gCodeBuilder.DrawGSCircle(x1, y1, ((workingOuterDiameter + xyComp) / 2), true);
                        return;
                    }
                    // now figure out the diameter of the endpoint circles
                    workingOuterDiameter = (int)Math.Round((minorEllipseAxis * stateMachine.IsoPlotPointsPerAppUnit));
                    // calc the amount we have to add/subtract on the center point to get to the 
                    // endpoints
                    majorEllipseAxisEndpointCompensation = (int)Math.Round(((majorEllipseAxisAdjustedForCircle * stateMachine.IsoPlotPointsPerAppUnit) / 2));
                    majorEllipseAxisEndpointCompensation += (xyComp/2);
                    // compensate the endpoints, the way we do this is dependent on whether
                    // the ellipse is horizontal or vertical
                    if (ellipseIsVertical == false)
                    {
                        x1EllipseCompensatedEndpoint = x1 - majorEllipseAxisEndpointCompensation;
                        x2EllipseCompensatedEndpoint = x1 + majorEllipseAxisEndpointCompensation;
                        y1EllipseCompensatedEndpoint = y1;
                        y2EllipseCompensatedEndpoint = y1;
                    }
                    else
                    {
                        x1EllipseCompensatedEndpoint = x1;
                        x2EllipseCompensatedEndpoint = x1;
                        y1EllipseCompensatedEndpoint = y1 - majorEllipseAxisEndpointCompensation;
                        y2EllipseCompensatedEndpoint = y1 + majorEllipseAxisEndpointCompensation;
                    }
                    // draw the first circle endpoint
                    gCodeBuilder.DrawGSCircle(x1EllipseCompensatedEndpoint, y1EllipseCompensatedEndpoint, ((workingOuterDiameter + xyComp) / 2), true);
                    // draw the second circle endpoint
                    gCodeBuilder.DrawGSCircle(x2EllipseCompensatedEndpoint, y2EllipseCompensatedEndpoint, ((workingOuterDiameter + xyComp) / 2), true);
                    // draw the connecting line between the circle centers
                    gCodeBuilder.DrawGSLineOutLine(x1EllipseCompensatedEndpoint, y1EllipseCompensatedEndpoint, x2EllipseCompensatedEndpoint, y2EllipseCompensatedEndpoint, (workingOuterDiameter + xyComp), true);

/*
                    DebugMessage("");
                    DebugMessage("majorEllipseAxis=" + (majorEllipseAxis * stateMachine.IsoPlotPointsPerAppUnit));
                    DebugMessage("minorEllipseAxis =" + (minorEllipseAxis * stateMachine.IsoPlotPointsPerAppUnit).ToString());
                    DebugMessage("y1EllipseCompensatedEndpoint =" + (y1EllipseCompensatedEndpoint).ToString());
                    DebugMessage("y2EllipseCompensatedEndpoint =" + (y2EllipseCompensatedEndpoint).ToString());
                    DebugMessage("y2EllipseCompensatedEndpoint-y1EllipseCompensatedEndpoint =" + (y2EllipseCompensatedEndpoint - y1EllipseCompensatedEndpoint).ToString());
                    DebugMessage("xyComp =" + xyComp.ToString());
                    DebugMessage("workingOuterDiameter =" + workingOuterDiameter.ToString());
                    DebugMessage("");
*/
                    return;
                case ApertureTypeEnum.APERTURETYPE_POLYGON:
                    throw new NotImplementedException("Polygonal aperture on line " + LineNumber.ToString() + " is not supported.");
                case ApertureTypeEnum.APERTURETYPE_RECTANGLE:
                    int xComp = (int)Math.Round(((xAxisDimension * stateMachine.IsoPlotPointsPerAppUnit)));
                    int yComp = (int)Math.Round(((yAxisDimension * stateMachine.IsoPlotPointsPerAppUnit) / 2));
                    // original code - which was in error 
                    // gCodeBuilder.DrawGSLineOutLine(x1, y1 - yComp - xyComp    , x1, y1 + yComp + xyComp    , xComp + (2 * xyComp), true);
                    // corrected, courtesy of MaikF
                    gCodeBuilder.DrawGSLineOutLine(x1, y1 - yComp - xyComp / 2, x1, y1 + yComp + xyComp / 2, xComp + (1 * xyComp), true);
                    return;
                case ApertureTypeEnum.APERTURETYPE_CIRCLE:
                    // draw the circle
                    workingOuterDiameter = (int)Math.Round((outerDiameter * stateMachine.IsoPlotPointsPerAppUnit));
                    gCodeBuilder.DrawGSCircle(x1, y1, ((workingOuterDiameter + xyComp) / 2), true);
                    return;
                default:
                    return;
            }
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
        public override int ParseLine(string processedLineStr, GerberFileStateMachine stateMachine)
        {
            //DebugMessage("ParseParameter(AD) started");

            if (processedLineStr == null) return 100;
            if (processedLineStr.StartsWith(GerberFile.RS274APDEFPARAM) == false) return 200;

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

    }
}
