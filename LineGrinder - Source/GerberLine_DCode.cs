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
    /// A class to encapsulate a gerber D Code
    /// </summary>
    public class GerberLine_DCode : GerberLine
    {
        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        // the number of this DCode
        private int dCode = 0;

        /// These values are the decimal compensated values from the DCode itself. They
        /// are not yet scaled to plot coordinates.
        private float dCodeCoord_X = 0;
        private float dCodeCoord_Y = 0;
        bool xCoordFound = false;
        bool yCoordFound = false;

        // used for circular interpolation
        private float arcCenterXDistance = 0;
        private float arcCenterYDistance = 0;

        // these are the dCode?Coords in plot coords. They are only available
        // after the plot has been performed. Basically these are the coords
        // which have been drawn by this DCode, could be line could be an arc
        private int lastPlotXCoordStart = 0;
        private int lastPlotYCoordStart = 0;
        private int lastPlotXCoordEnd = 0;
        private int lastPlotYCoordEnd = 0;
        private int lastRadiusPlotCoord = 0;
        private float lastSweepAngle = 0;
        private float lastStartAngle = 0;
        private int lastPlotCenterXCoord = 0;
        private int lastPlotCenterYCoord = 0;
        private bool lastSweepAngleWasClockwise = false;


        // if this is true we are drawing area fill (G36 - G37) or some sort of text label
        // layer. These lines have no width do not get wrapped with an isolation cut. rather
        // the tool bit just runs down the center of those lines
        private const bool DEFAULT_CURRENTLINE_ISNOT_FORISOLATION = false;
        private bool isCoincidentLineInFillRegion = DEFAULT_CURRENTLINE_ISNOT_FORISOLATION;

        private int gCodeIsoPlotObjectID = -1;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        public GerberLine_DCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets for a new plot
        /// </summary>
        public override void ResetForPlot()
        {

            // do not do these, they get set on the parse and first run and 
            // stay that way
            //xCoordFound = false;
            //yCoordFound = false;

            lastPlotXCoordStart = 0;
            lastPlotYCoordStart = 0;
            lastPlotXCoordEnd = 0;
            lastPlotYCoordEnd = 0;
            lastRadiusPlotCoord = 0;
            lastSweepAngle = 0;
            lastStartAngle = 0;
            lastPlotCenterXCoord = 0;
            lastPlotCenterYCoord = 0;
            lastSweepAngleWasClockwise = false;

            // we do not reset this. It gets calculated once and stays there
            // isCoincidentLineInFillRegion = DEFAULT_CURRENTLINE_ISNOT_FORISOLATION;

            gCodeIsoPlotObjectID = -1;
    }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets /Sets the gCodeIsoPlotObjectID value. is set internally
        /// </summary>
        public int IsoPlotObjectID
        {
            get
            {
                return gCodeIsoPlotObjectID;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets /Sets the isCoincidentLineInFillRegion value
        /// </summary>
        public bool IsCoincidentLineInFillRegion
        {
            get
            {
                return isCoincidentLineInFillRegion;
            }
            set
            {
                isCoincidentLineInFillRegion = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current X Coordinate value
        /// </summary>
        public float DCodeCoord_X
        {
            get
            {
                return dCodeCoord_X;
            }
            set
            {
                dCodeCoord_X = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Y Coordinate value
        /// </summary>
        public float DCodeCoord_Y
        {
            get
            {
                return dCodeCoord_Y;
            }
            set
            {
                dCodeCoord_Y = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the radius if we are drawing an arc. Otherwise returns 0
        /// </summary>
        /// <param name="startXCoord">the endXCoord of the previous DCode</param>
        private float CalcRadiusIfArc()
        {
            // only D01 in circular interpolation mode can have arcs
            if (dCode != 1) return 0; 
            // if we don't have a center offset we don't have a radius
            if ((arcCenterXDistance==0) && (arcCenterYDistance==0)) return 0;
            // do the math
            return (float)Math.Sqrt((arcCenterXDistance * arcCenterXDistance) + (arcCenterYDistance * arcCenterYDistance));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the maximum and minimum coords for a DCode. This supports
        /// lines, flashes, arcs and macros. We expect the stateMachine to have been set appropriately
        /// </summary>
        /// <param name="startDCodeXCoord">the previous Dcode X coord, the current one is the end coord</param>
        /// <param name="startDCodeYCoord">the previous Dcode Y coord, the current one is the end coord</param>
        /// <param name="ptLL">lower left point</param>
        /// <param name="ptUR">upper right point</param>
        /// <param name="stateMachine">the gerber plot state machine</param>
        /// <returns>true the coords are set with values, false they are not</returns>
        public bool GetMaxMinXAndYValues(GerberFileStateMachine stateMachine, float startDCodeXCoord, float startDCodeYCoord, out PointF ptUR, out PointF ptLL)
        {

            ptLL = new PointF(0, 0);
            ptUR = new PointF(0, 0);

            // get the current aperture here
            GerberLine_ADCode workingAperture = stateMachine.CurrentAperture;
            float apertureDimension = workingAperture.GetApertureDimension();

            if (dCode == 1)
            {
                // we are a draw, this can be circular or linear

                // what mode are we in
                if (stateMachine.GerberFileInterpolationMode== GerberInterpolationModeEnum.INTERPOLATIONMODE_CIRCULAR)
                {
                    // we are doing circular interpolation. We could be drawing a complete circle running through the
                    // two points or we could be doing a partial arc. We need the radius of the circle here
                    float radius = CalcRadiusIfArc();
                    
                    // we do not bother to check if we are actually drawing a complete circle, we just set the max and min as if we are
                    // note that we base the center on the start coords. That is what the spec says
                    ptLL.X = startDCodeXCoord + arcCenterXDistance - radius;
                    ptLL.Y = startDCodeYCoord + arcCenterYDistance - radius;
                    ptUR.X = startDCodeXCoord + arcCenterXDistance + radius;
                    ptUR.Y = startDCodeYCoord + arcCenterYDistance + radius;
                    // compensate for the aperture dimensions on each end, should only be half but we give it a bit extra
                    ptLL.X = ptLL.X - apertureDimension;
                    ptLL.Y = ptLL.Y - apertureDimension;
                    ptUR.X = ptUR.X + apertureDimension;
                    ptUR.Y = ptUR.Y + apertureDimension;
                    return true;
                }
                else
                {
                    // assume we are doing linear interpolation, we get the bounding box of the
                    // line joining the points +/- half the max dimension of the aperture for the 
                    // endpoint flashes.

                    // get the LL and UR points of the bounding rectangle for the line
                    MiscGraphicsUtils.GetBoundingRectangleEndPointsFrom2Points(new PointF(startDCodeXCoord, startDCodeYCoord), new PointF(this.DCodeCoord_X, this.DCodeCoord_Y), out ptLL, out ptUR);
                    if (ptLL == null) return false;
                    if (ptUR == null) return false;
                    // compensate for the aperture dimensions on each end, should only be half but we give it a bit extra
                    ptLL.X = ptLL.X - apertureDimension;
                    ptLL.Y = ptLL.Y - apertureDimension;
                    ptUR.X = ptUR.X + apertureDimension;
                    ptUR.Y = ptUR.Y + apertureDimension;
                    return true;
                }
            }
            else if (dCode == 2)
            {
                // We are a move to a point, set the max, min as if we flashed here
                ptLL.X = this.DCodeCoord_X - apertureDimension / 2;
                ptLL.Y = this.DCodeCoord_Y - apertureDimension / 2;
                ptUR.X = this.DCodeCoord_X + apertureDimension / 2;
                ptUR.Y = this.DCodeCoord_Y + apertureDimension / 2;
                return true;
            }
            else if (dCode == 3)
            {
                if (workingAperture.ApertureType == ApertureTypeEnum.APERTURETYPE_MACRO)
                {
                    PointF macroPT_UR;
                    PointF macroPT_LL;
                    GerberAperture_Base apertureObj = workingAperture.ADCodeAperture;

                    // macros need to reset
                    apertureObj.ResetForFlash();

                    if (apertureObj == null) return false;
                    if ((apertureObj is GerberAperture_Macro) == false) return false;
                    GerberLine_AMCode macroObj = (apertureObj as GerberAperture_Macro).GetMacroObject(stateMachine);
                    if (macroObj == null) return false;
                    // get the max and min coords here
                    macroObj.GetMaxMinXAndYValuesForMacro((apertureObj as GerberAperture_Macro).VariableArray, out macroPT_UR, out macroPT_LL);
                    // set this now
                    ptLL.X = this.DCodeCoord_X + macroPT_LL.X;
                    ptLL.Y = this.DCodeCoord_Y + macroPT_LL.Y;
                    ptUR.X = this.DCodeCoord_X + macroPT_UR.X;
                    ptUR.Y = this.DCodeCoord_Y + macroPT_UR.Y;
                    return true;
                }
                else
                {
                    // we are a flash at a point, calc as if for a flash
                    ptLL.X = this.DCodeCoord_X - apertureDimension / 2;
                    ptLL.Y = this.DCodeCoord_Y - apertureDimension / 2;
                    ptUR.X = this.DCodeCoord_X + apertureDimension / 2;
                    ptUR.Y = this.DCodeCoord_Y + apertureDimension / 2;
                }
                return true;
            }
            else
            {
                // this is just an aperture define 
                return false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current X Coordinate value with origin compensation
        /// </summary>
        public float DCodeCoordOriginCompensated_X
        {
            get
            {
                return dCodeCoord_X + PlotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current Y Coordinate value with origin compensation
        /// </summary>
        public float DCodeCoordOriginCompensated_Y
        {
            get
            {
                return dCodeCoord_Y + PlotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current X Coordinate value with origin compensation
        /// and flipping applied (if necessary)
        /// </summary>
        public int GetIsoPlotCoordOriginCompensated_X(GerberFileStateMachine stateMachine)
        {
            // Just return this
            return (int)Math.Round((DCodeCoordOriginCompensated_X * stateMachine.IsoPlotPointsPerAppUnit));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current Y Coordinate value with origin compensation
        /// and flipping applied (if necessary)
        /// </summary>
        public int GetIsoPlotCoordOriginCompensated_Y(GerberFileStateMachine stateMachine)
        {
            // Just return this
            return (int)Math.Round((DCodeCoordOriginCompensated_Y * stateMachine.IsoPlotPointsPerAppUnit));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current origin X  distance to the arc center
        /// </summary>
        public float ArcCenterXDistance
        {
            get
            {
                return arcCenterXDistance;
            }
            set
            {
                arcCenterXDistance = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Y distance to the arc center
        /// </summary>
        public float ArcCenterYDistance
        {
            get
            {
                return arcCenterYDistance;
            }
            set
            {
                arcCenterYDistance = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the arc center Coordinate X value with origin compensation
        /// </summary>
        public float ArcCenterXCoordOriginCompensated(GerberFileStateMachine stateMachine)
        {
            // note this is calculated off the start coord not the end
            return stateMachine.LastDCodeXCoord + this.ArcCenterXDistance + PlotXCoordOriginAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the arc center Y Coordinate value with origin compensation
        /// </summary>
        public float ArcCenterYCoordOriginCompensated(GerberFileStateMachine stateMachine)
        {
            // note this is calculated off the start coord not the end
            return stateMachine.LastDCodeYCoord + this.ArcCenterYDistance + PlotYCoordOriginAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current arc Center X Coordinate value with origin compensation
        /// and flipping applied (if necessary)
        /// </summary>
        public float GetIsoPlotArcCenterXCoordOriginCompensatedAndFlipped(GerberFileStateMachine stateMachine)
        {
            // not an X flip. Just return this
            return (float)Math.Round((ArcCenterXCoordOriginCompensated(stateMachine) * stateMachine.IsoPlotPointsPerAppUnit));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current arc center Y Coordinate value with origin compensation
        /// and flipping applied (if necessary)
        /// </summary>
        public float GetIsoPlotArcCenterYCoordOriginCompensatedAndFlipped(GerberFileStateMachine stateMachine)
        {
            // Just return this
            return (float)Math.Round((ArcCenterYCoordOriginCompensated(stateMachine) * stateMachine.IsoPlotPointsPerAppUnit));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current D Code value. There is no get accessor this is 
        /// set when the line is parsed
        /// </summary>
        public int DCode
        {
            get
            {
                return dCode;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the lastPlotXCoordStart value
        /// </summary>
        public int LastPlotXCoordStart
        {
            get { return lastPlotXCoordStart; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the lastPlotYCoordStart value
        /// </summary>
        public int LastPlotYCoordStart
        {
            get { return lastPlotYCoordStart; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the lastPlotXCoordEnd value
        /// </summary>
        public int LastPlotXCoordEnd
        {
            get { return lastPlotXCoordEnd; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the lastPlotYCoordEnd value
        /// </summary>
        public int LastPlotYCoordEnd
        {
            get { return lastPlotYCoordEnd; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the lastRadiusPlotCoord value
        /// </summary>
        public int LastRadiusPlotCoord
        {
            get { return lastRadiusPlotCoord; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the lastSweepAngle value
        /// </summary>
        public float LastSweepAngle
        {
            get { return lastSweepAngle; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the lastStartAngle value
        /// </summary>
        public float LastStartAngle
        {
            get { return lastStartAngle; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the lastPlotCenterXCoord value
        /// </summary>
        public int LastPlotCenterXCoord
        {
            get { return lastPlotCenterXCoord; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the lastPlotCenterYCoord value
        /// </summary>
        public int LastPlotCenterYCoord
        {
            get { return lastPlotCenterYCoord; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the lastSweepAngleWasClockwise flag
        /// </summary>
        public bool LastSweepAngleWasClockwise
        {
            get { return lastSweepAngleWasClockwise; }
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
        /// Sets default coordinates if required. Older gerber files can sometimes 
        /// omit and X or Y code from a D code if they are 
        /// the same as the previous one, We fill in the previous one here 
        /// (as if it was specified) - keeps the algorythm simple
        /// </summary>
        /// <param name="stateMachine">the gerber plot state machine</param>
        private void SetDefaultCoordsIfRequired(GerberFileStateMachine stateMachine)
        {
            //heck that we found an X and Y coord. 
            if (xCoordFound == false)
            {
                // we did not get an x coord, assume one
                dCodeCoord_X = stateMachine.LastDCodeXCoord;
                xCoordFound = true;
            }
            if (yCoordFound == false)
            {
                // we did not get an y coord, assume one
                dCodeCoord_Y = stateMachine.LastDCodeYCoord;
                yCoordFound = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets default coordinates if required. Older gerber files can sometimes 
        /// omit and X or Y code from a D code if they are 
        /// the same as the previous one, We fill in the previous one here 
        /// (as if it was specified) - keeps the algorythm simple
        /// </summary>
        /// <param name="lastXCoord">the lastXCoord to use if not set</param>
        /// <param name="lastYCoord">the lastYCoord to use if not set</param>
        public void SetDefaultCoordsIfRequired(float lastXCoord, float lastYCoord)
        {
            //heck that we found an X and Y coord. 
            if (xCoordFound == false)
            {
                // we did not get an x coord, assume one
                dCodeCoord_X = lastXCoord;
                xCoordFound = true;
            }
            if (yCoordFound == false)
            {
                // we did not get an y coord, assume one
                dCodeCoord_Y = lastYCoord;
                yCoordFound = true;
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
            int workingXCoord = 0;
            int workingYCoord = 0;
            float startAngleInDegrees;
            float sweepAngleInDegrees;
            Pen workingPen = null;

            if (stateMachine == null)
            {
                errorValue = 999;
                errorString = "PerformPlotGerberAction (D) stateMachine == null";
                return GerberLine.PlotActionEnum.PlotAction_FailWithError;
            }

            if (DCode == 1)
            {
                // we cannot draw with an aperture macro. Check this now
                if(stateMachine.CurrentAperture.ApertureType == ApertureTypeEnum.APERTURETYPE_MACRO)
                {
                    throw new Exception("Cannot draw with an APERTURETYPE_MACRO");
                }

                // exposure on, move to point via line or arc thus drawing
                SetDefaultCoordsIfRequired(stateMachine);

                if (stateMachine.GerberFileInterpolationMode == GerberInterpolationModeEnum.INTERPOLATIONMODE_CIRCULAR)
                {
                    // get the current X,Y coordinates, these will already be scaled, origin compensated and flipped
                    float startXCoord = stateMachine.LastPlotXCoord;
                    float startYCoord = stateMachine.LastPlotYCoord;

                    // get the end X,Y coordinate
                    float endXCoord = GetIsoPlotCoordOriginCompensated_X(stateMachine);
                    float endYCoord = GetIsoPlotCoordOriginCompensated_Y(stateMachine);

                    // get the arc center point, by adding on the specified offsets. Note we do not involve
                    // the end coords here. These are calculated off the start coords
                    float arcCenterXCoord = GetIsoPlotArcCenterXCoordOriginCompensatedAndFlipped(stateMachine);
                    float arcCenterYCoord = GetIsoPlotArcCenterYCoordOriginCompensatedAndFlipped(stateMachine);

                    // get our points
                    PointF startPoint = new PointF(startXCoord, startYCoord);
                    PointF endPoint = new PointF(endXCoord, endYCoord);
                    PointF centerPoint = new PointF(arcCenterXCoord, arcCenterYCoord);

                    // get the radius now
                    float radius = MiscGraphicsUtils.GetDistanceBetweenTwoPoints(endPoint, centerPoint);

                    //DebugMessage("GD1 startPoint=" + startPoint.ToString() + ", endPoint=" + endPoint.ToString() + ", centerPoint=" + centerPoint.ToString() + ", radius=" + radius);

                    // A NOTE on the direction of angles here. Gerber can specifiy clockwise or counter clockwise. .NET only wants its angles specified counter 
                    // clockwise. So CounterClockwise just works fine. For Clockwise we keep the start and endpoints the same (those do not change) and
                    // then adjust the calculated sweep angle so it sweeps the other way. Thus the call to ConvertCounterClockwiseWSweepAngleToClockwise
                    // really gives us the counter clockwise sweep angle which gives us the same effect as a clockwise one would do if we could specify 
                    // it that way.

                    // get our start and sweep angles NOTE these are ALWAYS calculated CCW from X axis. We only adjust the sweep angle for Clockwise
                    MiscGraphicsUtils.GetCounterClockwiseStartAndSweepAnglesFrom2PointsAndCenter(startPoint, endPoint, centerPoint, out startAngleInDegrees, out sweepAngleInDegrees);

                    if (stateMachine.GerberFileInterpolationCircularDirectionMode == GerberInterpolationCircularDirectionModeEnum.DIRECTIONMODE_COUNTERCLOCKWISE)
                    {
                        // fall through
                    }
                    else if (stateMachine.GerberFileInterpolationCircularDirectionMode == GerberInterpolationCircularDirectionModeEnum.DIRECTIONMODE_CLOCKWISE)
                    {
                        // adjust the sweep angle
                        sweepAngleInDegrees = MiscGraphicsUtils.ConvertCounterClockwiseWSweepAngleToClockwise(sweepAngleInDegrees);
                    }
                    else
                    {
                        // We MUST have a setting for clockwise or counter clockwise. We 
                        // cannot assume this. The spec says this explicitly.
                        errorValue = 800;
                        errorString = "PerformPlotGerberAction (D) GerberInterpolationCircularDirectionMode == DIRECTIONMODE_UNKNOWN";
                        return GerberLine.PlotActionEnum.PlotAction_FailWithError;
                    }

                    // are we dealing with a sweep angle of 0 degrees?
                    if (sweepAngleInDegrees == 0)
                    {
                        // yes, this actually may be a full circle or it may just be a tiny little arc segment with an angle so small
                        // that it cannot be drawn. We can tell by looking at the quadrant mode
                        if (stateMachine.IsInMultiQuadrantMode==true) sweepAngleInDegrees = 360;
                    }

                    //DebugMessage("  GD1a startAngleInDegrees=" + startAngleInDegrees.ToString() + ", sweepAngleInDegrees=" + sweepAngleInDegrees.ToString());

                    // create an enclosing rectangle 
                    RectangleF outerRect = new RectangleF(arcCenterXCoord - radius , arcCenterYCoord - radius , 2 * radius, 2 * radius);
                    //DebugMessage("  GD1a outerRect=" + outerRect.ToString());

                    if (stateMachine.ContourDrawingModeEnabled == false)
                    {
                        // Draw normal arc, exposure on
                        workingPen = stateMachine.CurrentAperture.GetWorkingPen(stateMachine, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, (int)endXCoord, (int)endYCoord);
                        // simulate sweeping the aperture from one end of the arc to the other
                        graphicsObj.DrawArc(workingPen, outerRect, startAngleInDegrees, sweepAngleInDegrees);
                        // fix the line endpoints
                        stateMachine.CurrentAperture.FixupLineEndpointsForGerberPlot(stateMachine, graphicsObj, stateMachine.GerberApertureBrush, workingPen, startXCoord, startYCoord, endXCoord, endYCoord);
                        if (stateMachine.ShowGerberCenterLines == true)
                        {
                            graphicsObj.DrawArc(ApplicationColorManager.DEFAULT_GERBERPLOT_CENTERLINE_PEN, outerRect, startAngleInDegrees, sweepAngleInDegrees);
                        }
                    }
                    else
                    {
                        // we are a contour arc. Note that arcs are never coincident lines, we can draw but do not, the fill takes care of this nicely
                        //graphicsObj.DrawArc(ApplicationColorManager.DEFAULT_GERBERPLOT_CONTOURLINE_PEN, outerRect, startAngleInDegrees, sweepAngleInDegrees);
                        stateMachine.ComplexObjectList_DCode.Add(this);
                    }

                    // remember these for internal processing
                    lastPlotXCoordStart = (int)startXCoord;
                    lastPlotYCoordStart = (int)startYCoord;
                    lastPlotXCoordEnd = (int)endXCoord;
                    lastPlotYCoordEnd = (int)endYCoord;
                    lastRadiusPlotCoord = (int)radius;
                    lastStartAngle = startAngleInDegrees;
                    lastSweepAngle = sweepAngleInDegrees;
                    lastPlotCenterXCoord = (int)arcCenterXCoord;
                    lastPlotCenterYCoord = (int)arcCenterYCoord;

                    // need to set these for the next code
                    stateMachine.LastDCodeXCoord = DCodeCoord_X;
                    stateMachine.LastDCodeYCoord = DCodeCoord_Y;
                    stateMachine.LastPlotXCoord = (int)endXCoord;
                    stateMachine.LastPlotYCoord = (int)endYCoord;
                }
                else
                {
                    // assume linear interpolation. Technically we should throw an error if this is not explicitly specified 
                    // but for backwards compatibility we assume linear by default

                    // get the current X,Y coordinates and scale up
                    workingXCoord = GetIsoPlotCoordOriginCompensated_X(stateMachine);
                    workingYCoord = GetIsoPlotCoordOriginCompensated_Y(stateMachine);

                    if(stateMachine.ContourDrawingModeEnabled == false)
                    {
                        // Draw gerber line, exposure on
                        workingPen = stateMachine.CurrentAperture.GetWorkingPen(stateMachine, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);
                        // simpulate sweeping the aperture from one end of the line to the other
                        graphicsObj.DrawLine(workingPen, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);

                        // now fixup the endpoints to compensate for non-rectangular apertures
                        stateMachine.CurrentAperture.FixupLineEndpointsForGerberPlot(stateMachine, graphicsObj, stateMachine.GerberApertureBrush, workingPen, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);
                        if (stateMachine.ShowGerberCenterLines == true)
                        {
                            graphicsObj.DrawLine(ApplicationColorManager.DEFAULT_GERBERPLOT_CENTERLINE_PEN, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);
                        }
                    }
                    else
                    {
                        // we are on a contour line. These have no width but we do not draw coincident lines, those are just internal gerber region define mechanisms
                        if (isCoincidentLineInFillRegion == false)
                        {
                            // not a coincident line - we can draw it, but do not the fill takes care of this nicely
                            //graphicsObj.DrawLine(ApplicationColorManager.DEFAULT_GERBERPLOT_CONTOURLINE_PEN, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);
                            //DebugMessage("workingXCoord=" + workingXCoord.ToString() + ", workingYCoord=" + workingYCoord.ToString());
                            stateMachine.ComplexObjectList_DCode.Add(this);
                        }
                        else
                        {
                            // we are a coincident line, we still add this, our path needs these to figure out a Clipping region
                            stateMachine.ComplexObjectList_DCode.Add(this);
                        }
                    }
                    // remember these for internal processing
                    lastPlotXCoordStart = stateMachine.LastPlotXCoord;
                    lastPlotYCoordStart = stateMachine.LastPlotYCoord;
                    lastPlotXCoordEnd = workingXCoord;
                    lastPlotYCoordEnd = workingYCoord;
                    lastRadiusPlotCoord = 0;
                    lastStartAngle = 0;
                    lastSweepAngle = 0;
                    lastPlotCenterXCoord = 0;
                    lastPlotCenterYCoord = 0;

                    // remember these for the next DCode
                    stateMachine.LastPlotXCoord = workingXCoord;
                    stateMachine.LastPlotYCoord = workingYCoord;
                    stateMachine.LastDCodeXCoord = DCodeCoord_X;
                    stateMachine.LastDCodeYCoord = DCodeCoord_Y;
                }
            }
            else if (DCode == 2)
            {
                // Exposure off, move to point
                SetDefaultCoordsIfRequired(stateMachine);

                // a DCode of 2 with contouring enabled actually means fill the existing contour
                // and go to the next.
                if ((stateMachine.ContourDrawingModeEnabled == true) && (stateMachine.ComplexObjectList_DCode.Count != 0))
                {
                    // Create solid brush.
                    SolidBrush fillBrush = (SolidBrush)stateMachine.GerberContourFillBrush; 
                    // Get the bounding box
                    Rectangle boundingRect = stateMachine.GetBoundingRectangleFromComplexObjectList_DCode();
                    // get a graphics path defining this object
                    GraphicsPath gPath = stateMachine.GetGraphicsPathFromComplexObjectList_DCode();
                    MiscGraphicsUtils.FillRectangleUsingGraphicsPath(graphicsObj, gPath, boundingRect, fillBrush);
                    // reset the complex object list
                    stateMachine.ComplexObjectList_DCode = new List<GerberLine_DCode>();
                }

                // get the current X,Y coordinates and scale up
                workingXCoord = GetIsoPlotCoordOriginCompensated_X(stateMachine);
                workingYCoord = GetIsoPlotCoordOriginCompensated_Y(stateMachine);

                // remember these for internal processing
                lastPlotXCoordStart = stateMachine.LastPlotXCoord;
                lastPlotYCoordStart = stateMachine.LastPlotYCoord;
                lastPlotXCoordEnd = workingXCoord;
                lastPlotYCoordEnd = workingYCoord;
                lastRadiusPlotCoord = 0;
                lastStartAngle = 0;
                lastSweepAngle = 0;
                lastPlotCenterXCoord = 0;
                lastPlotCenterYCoord = 0;

                // remember these for the next DCode
                stateMachine.LastPlotXCoord = workingXCoord;
                stateMachine.LastPlotYCoord = workingYCoord;
                stateMachine.LastDCodeXCoord = DCodeCoord_X;
                stateMachine.LastDCodeYCoord = DCodeCoord_Y;

            }
            else if (DCode == 3)
            {

                // move to point and flash
                SetDefaultCoordsIfRequired(stateMachine);

                // get the current X,Y coordinates and scale up
                workingXCoord = GetIsoPlotCoordOriginCompensated_X(stateMachine);
                workingYCoord = GetIsoPlotCoordOriginCompensated_Y(stateMachine);

                // move to point and Flash aperture
                stateMachine.CurrentAperture.FlashApertureForGerberPlot(stateMachine, graphicsObj, stateMachine.GerberApertureBrush, workingXCoord, workingYCoord);

                // remember these for internal processing
                lastPlotXCoordStart = stateMachine.LastPlotXCoord;
                lastPlotYCoordStart = stateMachine.LastPlotYCoord;
                lastPlotXCoordEnd = workingXCoord;
                lastPlotYCoordEnd = workingYCoord;
                lastRadiusPlotCoord = 0;
                lastStartAngle = 0;
                lastSweepAngle = 0;
                lastPlotCenterXCoord = 0;
                lastPlotCenterYCoord = 0;

                // remember these for the next DCode
                stateMachine.LastPlotXCoord = workingXCoord;
                stateMachine.LastPlotYCoord = workingYCoord;
                stateMachine.LastDCodeXCoord = DCodeCoord_X;
                stateMachine.LastDCodeYCoord = DCodeCoord_Y;
            }
            else
            {
                try
                {
                    // must be an aperture select
                    stateMachine.CurrentAperture = stateMachine.ApertureCollection.GetApertureByID(DCode);
                    // we have no X, Y coords on this type of DCode
                }
                catch (Exception ex)
                {
                    LogMessage("PerformPlotGerberAction error: " + ex.Message);
                    errorValue = 900;
                    errorString = "Plot failed with error: "+ex.Message;
                    return GerberLine.PlotActionEnum.PlotAction_FailWithError;
                }
            }

            //DebugMessage("aDCode=" + dCode.ToString() + " (" + lastPlotXCoordStart.ToString() + "," + lastPlotYCoordStart.ToString() + ") (" + lastPlotXCoordEnd.ToString() + "," + lastPlotYCoordEnd.ToString() + ")");
            //DebugMessage("bDCode=" + dCode.ToString() + " (" + stateMachine.LastPlotXCoord.ToString() + "," + stateMachine.LastPlotYCoord.ToString() + ")");
            //DebugMessage("cDCode=" + dCode.ToString() + " (" + stateMachine.LastDCodeXCoord.ToString() + "," + stateMachine.LastDCodeYCoord.ToString() + ")");
            //DebugMessage("");

            return GerberLine.PlotActionEnum.PlotAction_Continue;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the plot Isolation Object actions required based on the current context
        /// </summary>
        /// <param name="isoPlotBuilder">A GCode Builder object</param>
        /// <param name="stateMachine">the gerber plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <returns>an enum value indicating what next action to take</returns>
        public override GerberLine.PlotActionEnum PerformPlotIsoStep1Action(IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {
            int workingXCoord = 0;
            int workingYCoord = 0;
            float startAngleInDegrees;
            float sweepAngleInDegrees;

            if (isoPlotBuilder == null)
            {
                errorValue = 998;
                errorString = "PerformPlotIsoStep1Action (D) isoPlotBuilder == null";
                return GerberLine.PlotActionEnum.PlotAction_FailWithError;
            }
            if (stateMachine == null)
            {
                errorValue = 999;
                errorString = "PerformPlotIsoStep1Action (D) stateMachine == null";
                return GerberLine.PlotActionEnum.PlotAction_FailWithError;
            }

            if (DCode == 1)
            {
                // we cannot draw with an aperture macro. Check this now
                if (stateMachine.CurrentAperture.ApertureType == ApertureTypeEnum.APERTURETYPE_MACRO)
                {
                    throw new Exception("Cannot draw with an APERTURETYPE_MACRO");
                }

                // exposure on, move to point via line or arc thus drawing

                if (stateMachine.GerberFileInterpolationMode == GerberInterpolationModeEnum.INTERPOLATIONMODE_CIRCULAR)
                {
                    bool wantClockWise = false;

                    // get the current X,Y coordinates, these will already be scaled, origin compensated and flipped
                    float startXCoord = stateMachine.LastPlotXCoord;
                    float startYCoord = stateMachine.LastPlotYCoord;

                    // get the end X,Y coordinate
                    float endXCoord = GetIsoPlotCoordOriginCompensated_X(stateMachine);
                    float endYCoord = GetIsoPlotCoordOriginCompensated_Y(stateMachine);

                    // get the arc center point, by adding on the specified offsets. Note we do not involve
                    // the end coords here. These are calculated off the start coords
                    float arcCenterXCoord = GetIsoPlotArcCenterXCoordOriginCompensatedAndFlipped(stateMachine);
                    float arcCenterYCoord = GetIsoPlotArcCenterYCoordOriginCompensatedAndFlipped(stateMachine);

                    // get our points
                    PointF startPoint = new PointF(startXCoord, startYCoord);
                    PointF endPoint = new PointF(endXCoord, endYCoord);
                    PointF centerPoint = new PointF(arcCenterXCoord, arcCenterYCoord);

                    // get the radius now
                    float radius = MiscGraphicsUtils.GetDistanceBetweenTwoPoints(endPoint, centerPoint);

                    //DebugMessage("ISD1 startPoint=" + startPoint.ToString() + ", endPoint=" + endPoint.ToString() + ", centerPoint=" + centerPoint.ToString() + ", radius=" + radius);

                    // A NOTE on the direction of angles here. Gerber can specify clockwise or counter clockwise. .NET only wants its angles specified counter 
                    // clockwise. So CounterClockwise just works fine. For Clockwise we keep the start and endpoints the same (those do not change) and
                    // then adjust the calculated sweep angle so it sweeps the other way. Thus the call to ConvertCounterClockwiseWSweepAngleToClockwise
                    // really gives us the counter clockwise sweep angle which gives us the same effect as a clockwise one would do if we could specify 
                    // it that way.

                    // get our start and sweep angles NOTE these are ALWAYS calculated CCW from X axis. We only adjust the sweep angle for Clockwise
                    MiscGraphicsUtils.GetCounterClockwiseStartAndSweepAnglesFrom2PointsAndCenter(startPoint, endPoint, centerPoint, out startAngleInDegrees, out sweepAngleInDegrees);

                    if (stateMachine.GerberFileInterpolationCircularDirectionMode == GerberInterpolationCircularDirectionModeEnum.DIRECTIONMODE_COUNTERCLOCKWISE)
                    {
                        // fall through
                        wantClockWise = false;
                    }
                    else if (stateMachine.GerberFileInterpolationCircularDirectionMode == GerberInterpolationCircularDirectionModeEnum.DIRECTIONMODE_CLOCKWISE)
                    {
                        // adjust the sweep angle
                        sweepAngleInDegrees = MiscGraphicsUtils.ConvertCounterClockwiseWSweepAngleToClockwise(sweepAngleInDegrees);
                        wantClockWise = true;
                    }
                    else
                    {
                        // We MUST have a setting for clockwise or counter clockwise. We 
                        // cannot assume this. The spec says this explicitly.
                        errorValue = 800;
                        errorString = "PerformPlotIsoStep1Action (D) GerberInterpolationCircularDirectionMode == DIRECTIONMODE_UNKNOWN";
                        return GerberLine.PlotActionEnum.PlotAction_FailWithError;
                    }
                    //DebugMessage("  ISD1a startAngleInDegrees=" + startAngleInDegrees.ToString() + ", sweepAngleInDegrees=" + sweepAngleInDegrees.ToString());

                    // are we dealing with a sweep angle of 0 degrees?
                    if (sweepAngleInDegrees == 0)
                    {
                        // yes, this actually may be a full circle or it may just be a tiny little arc segment with an angle so small
                        // that it cannot be drawn. We can tell by looking at the quadrant mode
                        if (stateMachine.IsInMultiQuadrantMode == true) sweepAngleInDegrees = 360;
                    }

                    // get our xyComp. this is the offset around the gerber plot  
                    int xyComp = stateMachine.CurrentAperture.GetIsolationLineXYCompensation(stateMachine);

                    // Get Line width here
                    int width = stateMachine.CurrentAperture.GetPenWidthForLine(stateMachine, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);
                    // Note The aperture fixup banged on the end takes care of the isolation routing round the
                    // end points of the arc
                    width += (xyComp * 2);

                    // are we a contour arc?
                    if (stateMachine.ContourDrawingModeEnabled == false)
                    {
                        // no, regular arc, draw the arc outline
                        isoPlotBuilder.DrawGSArcOutLine(arcCenterXCoord, arcCenterYCoord, startAngleInDegrees, sweepAngleInDegrees, radius, width, wantClockWise, stateMachine.BackgroundFillModeAccordingToPolarity, stateMachine.IsInMultiQuadrantMode);
                        // now fixup the endpoints to compensate for non-rectangular apertures
                        stateMachine.CurrentAperture.FixupLineEndpointsForGCodePlot(isoPlotBuilder, stateMachine, (int)startXCoord, (int)startYCoord, (int)endXCoord, (int)endYCoord, (width / 2), xyComp);
                    }
                    else
                    {
                        int builderID = -1;
                        // yes we are a contour arc. Note arcs are never coincident lines, depending on the usage mode this is either a contour edge or an invert edge
                        if (stateMachine.GerberFileLayerPolarity== GerberLayerPolarityEnum.CLEAR)
                        {
                            // clear polarity
                            builderID = isoPlotBuilder.DrawGSContourArc(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, arcCenterXCoord, arcCenterYCoord, startAngleInDegrees, sweepAngleInDegrees, radius, wantClockWise, stateMachine.IsInMultiQuadrantMode, false);
                        }
                        else
                        {
                            // dark polarity
                            builderID = isoPlotBuilder.DrawGSContourArc(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_CONTOUREDGE, arcCenterXCoord, arcCenterYCoord, startAngleInDegrees, sweepAngleInDegrees, radius, wantClockWise, stateMachine.IsInMultiQuadrantMode, false);
                        }
                        // contours form complex objects so we add this one to the list
                        stateMachine.ComplexObjectList_DCode.Add(this);
                        // record this
                        gCodeIsoPlotObjectID = builderID;
                    }

                    // remember these for internal processing
                    lastPlotXCoordStart = (int)startXCoord;
                    lastPlotYCoordStart = (int)startYCoord;
                    lastPlotXCoordEnd = (int)endXCoord;
                    lastPlotYCoordEnd = (int)endYCoord;
                    lastRadiusPlotCoord = (int)radius;
                    lastStartAngle = startAngleInDegrees;
                    lastSweepAngle = sweepAngleInDegrees;
                    lastPlotCenterXCoord = (int)arcCenterXCoord;
                    lastPlotCenterYCoord = (int)arcCenterYCoord;

                    // need to set these for the next code
                    stateMachine.LastDCodeXCoord = DCodeCoord_X;
                    stateMachine.LastDCodeYCoord = DCodeCoord_Y;
                    stateMachine.LastPlotXCoord = (int)endXCoord;
                    stateMachine.LastPlotYCoord = (int)endYCoord;
                }
                else
                {
                    // assume linear interpolation. Technically we should throw an error if this is not explicitly specified 
                    // but for backwards compatibility we assume linear by default

                    // get the current X,Y coordinates and scale up
                    workingXCoord = GetIsoPlotCoordOriginCompensated_X(stateMachine);
                    workingYCoord = GetIsoPlotCoordOriginCompensated_Y(stateMachine);

                    // get our xyComp. this is the offset around the gerber plot  
                    int xyComp = stateMachine.CurrentAperture.GetIsolationLineXYCompensation(stateMachine);

                    // Get Line width here
                    int width = stateMachine.CurrentAperture.GetPenWidthForLine(stateMachine, stateMachine.LastPlotXCoord, stateMachine.LastPlotYCoord, workingXCoord, workingYCoord);
                    // Note The aperture fixup banged on the end takes care of the isolation routing round the
                    // end points of the line
                    int x0 = stateMachine.LastPlotXCoord;
                    int y0 = stateMachine.LastPlotYCoord;
                    int x1 = workingXCoord;
                    int y1 = workingYCoord;
                    width += (xyComp * 2);

                    // are we a contour line?
                    if (stateMachine.ContourDrawingModeEnabled == false)
                    {
                        // no, draw the line outline, what about polarity?
                        if (stateMachine.GerberFileLayerPolarity == GerberLayerPolarityEnum.CLEAR)
                        {
                            // clear polarity
                            isoPlotBuilder.DrawGSLineOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x0, y0, x1, y1, width, stateMachine.BackgroundFillModeAccordingToPolarity);
                        }
                        else
                        {
                            // dark polarity
                            isoPlotBuilder.DrawGSLineOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, x0, y0, x1, y1, width, stateMachine.BackgroundFillModeAccordingToPolarity);
                        }
                        // now fixup the endpoints to compensate for non-rectangular apertures
                        stateMachine.CurrentAperture.FixupLineEndpointsForGCodePlot(isoPlotBuilder, stateMachine, x0, y0, x1, y1, (width / 2), xyComp);
                    }
                    else
                    {
                        // yes, we are on a contour line. These have no width but often describe copper pour regions
                        if (isCoincidentLineInFillRegion == false)
                        {
                            // not a coincident line - we can draw it
                            int builderID = isoPlotBuilder.DrawGSLineContourLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_CONTOUREDGE, x0, y0, x1, y1);
                            // contours form complex objects so we add this one to the list
                            stateMachine.ComplexObjectList_DCode.Add(this);
                            // record this
                            gCodeIsoPlotObjectID = builderID;
                        }
                        else
                        {
                            // we are a coincident line, we do not add this, in ISOPlot1 mode we do not want to bother with coincident lines
                            // they are just a complex gerber mechanism to describe a fill region
                            // stateMachine.ComplexObjectList_DCode.Add(this);
                            //int foo = 1;
                        }
                    }
                    // remember these for internal processing
                    lastPlotXCoordStart = stateMachine.LastPlotXCoord;
                    lastPlotYCoordStart = stateMachine.LastPlotYCoord;
                    lastPlotXCoordEnd = workingXCoord;
                    lastPlotYCoordEnd = workingYCoord;
                    lastRadiusPlotCoord = 0;
                    lastStartAngle = 0;
                    lastSweepAngle = 0;
                    lastPlotCenterXCoord = 0;
                    lastPlotCenterYCoord = 0;

                    // remember these for the next DCode
                    stateMachine.LastPlotXCoord = workingXCoord;
                    stateMachine.LastPlotYCoord = workingYCoord;
                    stateMachine.LastDCodeXCoord = DCodeCoord_X;
                    stateMachine.LastDCodeYCoord = DCodeCoord_Y;
                }
            }
            else if (DCode == 2)
            {

                // a DCode of 2 with contouring enabled actually means fill the existing contour
                // and go to the next.
                if ((stateMachine.ContourDrawingModeEnabled == true) && (stateMachine.ComplexObjectList_DCode.Count != 0))
                {
                    // process the complex object by drawing in the background 

                    // Get the bounding box
                    Rectangle boundingRect = stateMachine.GetBoundingRectangleFromComplexObjectList_DCode();
                    // get a list of builderIDs in the complex object list
                    List<int> builderIDList = stateMachine.GetListOfIsoPlotBuilderIDsFromComplexObjectList_DCode();
                    if (builderIDList.Count > 0)
                    {
                        if (stateMachine.BackgroundFillModeAccordingToPolarity == GSFillModeEnum.FillMode_BACKGROUND)
                        {
                            // Normal Dark polarity. Just fill it in with a backgroundpixel
                            isoPlotBuilder.BackgroundFillGSRegionComplex(builderIDList, builderIDList[0], boundingRect.X, boundingRect.Y, boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height, -1);
                        }
                        else if (stateMachine.BackgroundFillModeAccordingToPolarity == GSFillModeEnum.FillMode_ERASE)
                        {
                            // we have reverse polarity. The object we just drew must be cleaned out and everything underneath it removed
                            isoPlotBuilder.BackgroundFillGSByBoundaryComplexVert(builderIDList, IsoPlotObject.DEFAULT_ISOPLOTOBJECT_ID, boundingRect.X, boundingRect.Y, boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height, -1);
                        }
                    }
                    // reset, we have processed the complex list
                    stateMachine.ComplexObjectList_DCode = new List<GerberLine_DCode>();
                    // we do NOT disable contour drawing mode, we are STILL contour drawing
                    //stateMachine.ContourDrawingModeEnabled = false;
                }

                // get the current X,Y coordinates and scale up
                workingXCoord = GetIsoPlotCoordOriginCompensated_X(stateMachine);
                workingYCoord = GetIsoPlotCoordOriginCompensated_Y(stateMachine);

                // Exposure off, move to point

                // remember these for internal processing
                lastPlotXCoordStart = stateMachine.LastPlotXCoord;
                lastPlotYCoordStart = stateMachine.LastPlotYCoord;
                lastPlotXCoordEnd = workingXCoord;
                lastPlotYCoordEnd = workingYCoord;
                lastRadiusPlotCoord = 0;
                lastStartAngle = 0;
                lastSweepAngle = 0;
                lastPlotCenterXCoord = 0;
                lastPlotCenterYCoord = 0;

                // remember these for the next DCode
                stateMachine.LastPlotXCoord = workingXCoord;
                stateMachine.LastPlotYCoord = workingYCoord;
                stateMachine.LastDCodeXCoord = DCodeCoord_X;
                stateMachine.LastDCodeYCoord = DCodeCoord_Y;
            }
            else if (DCode == 3)
            {
                // get the current X,Y coordinates and scale up
                workingXCoord = GetIsoPlotCoordOriginCompensated_X(stateMachine);
                workingYCoord = GetIsoPlotCoordOriginCompensated_Y(stateMachine);
                // move to point 
                // the amount of extra X and Y we add for the gcode line
                int xyComp = stateMachine.CurrentAperture.GetIsolationLineXYCompensation(stateMachine);

                int x0 = workingXCoord;
                int y0 = workingYCoord;
                int x1 = workingXCoord;
                int y1 = workingYCoord;
                // are we on one of the refpin pads? - we do not isocut these
                if (stateMachine.IsThisARefPinPad(DCodeCoord_X, DCodeCoord_Y, stateMachine.CurrentAperture.GetApertureDimension()) == true)
                {
                    // skip it, we do not isolation route refpin pads
                }
                else
                {
                    // we only flash if not contour drawing, should never see flashes in a contour 
                    if (stateMachine.ContourDrawingModeEnabled == false)
                    {
                        // flash aperture
                        stateMachine.CurrentAperture.FlashApertureForGCodePlot(isoPlotBuilder, stateMachine, x0, y0, xyComp * 2);
                    }
                }

                // remember these for internal processing
                lastPlotXCoordStart = stateMachine.LastPlotXCoord;
                lastPlotYCoordStart = stateMachine.LastPlotYCoord;
                lastPlotXCoordEnd = workingXCoord;
                lastPlotYCoordEnd = workingYCoord;
                lastRadiusPlotCoord = 0;
                lastStartAngle = 0;
                lastSweepAngle = 0;
                lastPlotCenterXCoord = 0;
                lastPlotCenterYCoord = 0;

                // remember these for the next DCode
                stateMachine.LastPlotXCoord = workingXCoord;
                stateMachine.LastPlotYCoord = workingYCoord;
                stateMachine.LastDCodeXCoord = DCodeCoord_X;
                stateMachine.LastDCodeYCoord = DCodeCoord_Y;
            }
            else
            {
                // must be an aperture select
                stateMachine.CurrentAperture = stateMachine.ApertureCollection.GetApertureByID(DCode);
                // we do not save the X,Y coords for this DCode
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

            // now the DCode line will have some combination of D, X, Y and possibly I and J tags in some order
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
                    dCodeCoord_X = DecimalScaleNumber(outFloat, stateMachine.FormatParameter.XIntegerPlaces, stateMachine.FormatParameter.XDecimalPlaces, stateMachine.FormatParameter.LeadingZeroMode);
                    xCoordFound = true;
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
                    dCodeCoord_Y = DecimalScaleNumber(outFloat, stateMachine.FormatParameter.YIntegerPlaces, stateMachine.FormatParameter.YDecimalPlaces, stateMachine.FormatParameter.LeadingZeroMode);
                    yCoordFound = true;
                }
            }

            // LOOK FOR THE I TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'I', nextStartPos);
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
                    LogMessage("ParseLine(DI) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 133;
                }
                else
                {
                    // set the value now
                    arcCenterXDistance = DecimalScaleNumber(outFloat, stateMachine.FormatParameter.XIntegerPlaces, stateMachine.FormatParameter.XDecimalPlaces, stateMachine.FormatParameter.LeadingZeroMode);
                }
            }

            // LOOK FOR THE J TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'J', nextStartPos);
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
                    LogMessage("ParseLine(DJ) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                    return 133;
                }
                else
                {
                    // set the value now
                    arcCenterYDistance = DecimalScaleNumber(outFloat, stateMachine.FormatParameter.YIntegerPlaces, stateMachine.FormatParameter.YDecimalPlaces, stateMachine.FormatParameter.LeadingZeroMode);
                }
            }

            // LOOK FOR THE D TAG
            nextStartPos = 0;
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(processedLineStr, 'D', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > processedLineStr.Length))
            {
                // this is not an error - just means we did not find one, we assume a D01 as per section
                // 7.2 Coordinate Data without Operation Code of the specification
                dCode = 1;
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
                    dCode = outInt;
                }
            }

            return 0;
        }

    }
}

