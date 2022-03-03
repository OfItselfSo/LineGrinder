using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// A class for an aperture macro Vector Line primitive
    /// </summary>
    public class GerberMacroPrimitive_VectorLine : GerberMacroPrimitive_Base
    {

        // the width of the line
        private GerberMacroVariable width = new GerberMacroVariable();
        private GerberMacroVariable xStartCoord = new GerberMacroVariable();
        private GerberMacroVariable yStartCoord = new GerberMacroVariable();
        private GerberMacroVariable xEndCoord = new GerberMacroVariable();
        private GerberMacroVariable yEndCoord = new GerberMacroVariable();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberMacroPrimitive_VectorLine() : base(MacroPrimitiveTypeEnum.MACROPRIMITIVE_VECTORLINE)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the width 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetWidth(GerberMacroVariableArray varArray)
        {
            if (width == null) width = new GerberMacroVariable();
            return width.ProcessVariableStringToFloat(varArray);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the width 
        /// </summary>
        /// <param name="widthStr">a string defining the width</param>
        public void SetWidth(string widthStr)
        {
            if ((widthStr == null) || (widthStr == "")) width = new GerberMacroVariable();
            else width = new GerberMacroVariable(widthStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the xStartCoord. 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetXStartCoord(GerberMacroVariableArray varArray)
        {

            if (xStartCoord == null) xStartCoord = new GerberMacroVariable();
            float workingXStartCoord = xStartCoord.ProcessVariableStringToFloat(varArray);
            float workingDegreesRotation = GetDegreesRotation(varArray);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                // yes, return the converted value
                if (yStartCoord == null) yStartCoord = new GerberMacroVariable();
                float workingYStartCoord = yStartCoord.ProcessVariableStringToFloat(varArray);
                double angleInRadians = MiscGraphicsUtils.DegreesToRadians(workingDegreesRotation);
                return (float)((workingXStartCoord * Math.Cos(angleInRadians)) - workingYStartCoord * Math.Sin(angleInRadians));
            }
            else
            {
                // no just return the existing value
                return workingXStartCoord;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the xStartCoord. 
        /// </summary>
        /// <param name="startXCoordStr">a string defining the startXCoord</param>
        public void SetXStartCoord(string startXCoordStr)
        {
            if ((startXCoordStr == null) || (startXCoordStr == "")) xStartCoord = new GerberMacroVariable();
            else xStartCoord = new GerberMacroVariable(startXCoordStr);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the yStartCoord. 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetYStartCoord(GerberMacroVariableArray varArray)
        {

            if (yStartCoord == null) yStartCoord = new GerberMacroVariable();
            float workingYStartCoord = yStartCoord.ProcessVariableStringToFloat(varArray);
            float workingDegreesRotation = GetDegreesRotation(varArray);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                // yes, return the converted value
                if (xStartCoord == null) xStartCoord = new GerberMacroVariable();
                float workingXStartCoord = xStartCoord.ProcessVariableStringToFloat(varArray);
                double angleInRadians = MiscGraphicsUtils.DegreesToRadians(workingDegreesRotation);
                return (float)((workingYStartCoord * Math.Cos(angleInRadians)) + workingXStartCoord * Math.Sin(angleInRadians));
            }
            else
            {
                // no just return the eyisting value
                return workingYStartCoord;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the yStartCoord. 
        /// </summary>
        /// <param name="startYCoordStr">a string defining the startYCoord</param>
        public void SetYStartCoord(string startYCoordStr)
        {
            if ((startYCoordStr == null) || (startYCoordStr == "")) yStartCoord = new GerberMacroVariable();
            else yStartCoord = new GerberMacroVariable(startYCoordStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the xEndCoord. 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetXEndCoord(GerberMacroVariableArray varArray)
        {

            if (xEndCoord == null) xEndCoord = new GerberMacroVariable();
            float workingXEndCoord = xEndCoord.ProcessVariableStringToFloat(varArray);
            float workingDegreesRotation = GetDegreesRotation(varArray);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                // yes, return the converted value
                if (yEndCoord == null) yEndCoord = new GerberMacroVariable();
                float workingYEndCoord = yEndCoord.ProcessVariableStringToFloat(varArray);
                double angleInRadians = MiscGraphicsUtils.DegreesToRadians(workingDegreesRotation);
                return (float)((workingXEndCoord * Math.Cos(angleInRadians)) - workingYEndCoord * Math.Sin(angleInRadians));
            }
            else
            {
                // no just return the existing value
                return workingXEndCoord;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the xEndCoord. 
        /// </summary>
        /// <param name="endXCoordStr">a string defining the endXCoord</param>
        public void SetXEndCoord(string endXCoordStr)
        {
            if ((endXCoordStr == null) || (endXCoordStr == "")) xEndCoord = new GerberMacroVariable();
            else xEndCoord = new GerberMacroVariable(endXCoordStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the yEndCoord. 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetYEndCoord(GerberMacroVariableArray varArray)
        {

            if (yEndCoord == null) yEndCoord = new GerberMacroVariable();
            float workingYEndCoord = yEndCoord.ProcessVariableStringToFloat(varArray);
            float workingDegreesRotation = GetDegreesRotation(varArray);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                // yes, return the converted value
                if (xEndCoord == null) xEndCoord = new GerberMacroVariable();
                float workingXEndCoord = xEndCoord.ProcessVariableStringToFloat(varArray);
                double angleInRadians = MiscGraphicsUtils.DegreesToRadians(workingDegreesRotation);
                return (float)((workingYEndCoord * Math.Cos(angleInRadians)) + workingXEndCoord * Math.Sin(angleInRadians));
            }
            else
            {
                // no just return the eyisting value
                return workingYEndCoord;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the yEndCoord. 
        /// </summary>
        /// <param name="endYCoordStr">a string defining the endYCoord</param>
        public void SetYEndCoord(string endYCoordStr)
        {
            if ((endYCoordStr == null) || (endYCoordStr == "")) yEndCoord = new GerberMacroVariable();
            else yEndCoord = new GerberMacroVariable(endYCoordStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the line length of the rectangle. 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetLineLength(GerberMacroVariableArray varArray)
        {
            float start_x = GetXStartCoord(varArray);
            float start_y = GetYStartCoord(varArray);
            float end_x = GetXEndCoord(varArray);
            float end_y = GetYEndCoord(varArray);

            float xLen = end_x - start_x;
            float yLen = end_y - start_y;

            // just do the math
            return (float)Math.Sqrt((xLen * xLen) + (yLen * yLen));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the center point of the rectangle. 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public PointF GetCenterPoint(GerberMacroVariableArray varArray)
        {

            // get the start coords
            float start_x = GetXStartCoord(varArray);
            float start_y = GetYStartCoord(varArray);
            float end_x = GetXEndCoord(varArray);
            float end_y = GetYEndCoord(varArray);

            // add them up and divide by two
            return new PointF((float)((start_x+end_x)/2), (float)((start_y + end_y) / 2));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the parameters for this primitive from the supplied string
        /// </summary>
        /// <param name="primStr">the string with the primitives</param>
        /// <returns>z success, nz fail</returns>
        public override int ParsePrimitiveString(string primStr)
        {
            char[] splitChars = { GerberLine_AMCode.AM_PRIMITIVE_DELIM_CHAR };
            string[] paramArray = null;

            if ((primStr==null) || (primStr.Length==0))
            {
                // should never happen
                LogMessage("ParsePrimitiveString(VLine), (primStr==null) || (primStr.Length==0)");
                return 10;
            }
            if (primStr.StartsWith(GerberLine_AMCode.AM_VLINE_PRIMITIVE + GerberLine_AMCode.AM_PRIMITIVE_DELIM) == false)
            {
                // should never happen
                LogMessage("ParsePrimitiveString(VLine), start marker not present");
                return 20;
            }

            // record this
            PrimitiveString = primStr;

            // split the parameters out
            paramArray = primStr.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            // this should never be null
            if (paramArray == null)
            {
                LogMessage("ParsePrimitiveString(VLine), blockArray == null");
                return 30;
            }
            // we expect eight parameters including the marker for the GerberLine_AMCode.AM_CLINE_PRIMITIVE value
            if ((paramArray.Length == 8) == false)
            {
                // should never happen
                LogMessage("ParsePrimitiveString(VLine), blockArray.Length != 6 is " + paramArray.Length.ToString());
                return 40;
            }
            try
            {
                // paramArray[0] is the AM_VLINE_PRIMITIVE value

                // param 1 is the aperture state
                if (paramArray[1] == "1") SetApertureIsOn("1");
                else SetApertureIsOn("0");

                // param 2 is the width
                SetWidth(paramArray[2]);

                // param 3 is the Start X Coord
                SetXStartCoord(paramArray[3]);

                // param 4 is the Start Y Coord
                SetYStartCoord(paramArray[4]);

                // param 5 is the End X Coord
                SetXEndCoord(paramArray[5]);

                // param 6 is the End Y Coord
                SetYEndCoord(paramArray[6]);

                // param 7 is the rotation. It is not optional on this prim
                SetDegreesRotation(paramArray[7]);
            }
            catch (Exception ex)
            {
                LogMessage("ParsePrimitiveString(VLine), param conversion failed, msg="+ex.Message);
                return 100;
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We fill a shape with the macro primitive at the current coordinates. 
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="bottomOfGraphicsObjInMacroScreenCoords_X">the macro screen coord which represents the bottom 0 coord of the graphics object</param>
        /// <param name="bottomOfGraphicsObjInMacroScreenCoords_Y">the macro screen coord which represents the bottom 0 coord of the graphics object</param>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>z success, nz fail</returns>
        public override void FlashMacroPrimitiveForGerberPlot(GerberFileStateMachine stateMachine, GerberMacroVariableArray varArray, Graphics graphicsObj, Brush workingBrush, float bottomOfGraphicsObjInMacroScreenCoords_X, float bottomOfGraphicsObjInMacroScreenCoords_Y)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            if (stateMachine == null) return;

            // note that the graphicsObj here is not the main screen. It is a smaller temporary graphics object
            // designed only to receive the macro. The underlying bitmap of this graphics object gets overlaid on the screen later

            // note the bottomOfGraphicsObjInMacroScreenCoords_? vars are not the bottom of this macro primitives space. It is the
            // screen coords of the bottom of the smallest rectangle which contains all of the macro primitives

            float workingWidth = GetWidth(varArray);
            int lineWidthInScreenCoords = (int)Math.Round(workingWidth * stateMachine.IsoPlotPointsPerAppUnit);

            // we will need the start and end point coords 
            float primStartPointInScreenCoords_X = GetXStartCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit;
            float primStartPointInScreenCoords_Y = GetYStartCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit;
            float primEndPointInScreenCoords_X = GetXEndCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit;
            float primEndPointInScreenCoords_Y = GetYEndCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit;

            // now calc the effective draw points
            int effectiveLeftCenterPoint_X = (int)(primStartPointInScreenCoords_X - bottomOfGraphicsObjInMacroScreenCoords_X);
            int effectiveLeftCenterPoint_Y = (int)(primStartPointInScreenCoords_Y - bottomOfGraphicsObjInMacroScreenCoords_Y);
            int effectiveRightCenterPoint_X = (int)(primEndPointInScreenCoords_X - bottomOfGraphicsObjInMacroScreenCoords_X);
            int effectiveRightCenterPoint_Y = (int)(primEndPointInScreenCoords_Y - bottomOfGraphicsObjInMacroScreenCoords_Y);

            //// Draw gerber line according to exposure
            Pen workingPen = GetDrawPen(stateMachine, varArray, lineWidthInScreenCoords); 
            // simulate sweeping the aperture from one end of the line to the other
            graphicsObj.DrawLine(workingPen, effectiveLeftCenterPoint_X, effectiveLeftCenterPoint_Y, effectiveRightCenterPoint_X, effectiveRightCenterPoint_Y);
            if (workingPen != null) workingPen.Dispose();

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flash a macro primitive for a CLine on a GCode Plot
        /// </summary>
        /// <param name="isoPlotBuilder">the builder opbject</param>
        /// <param name="stateMachine">the statemachine</param>
        /// <param name="macroBuilderIDs">a list of builder IDs which have drawn in POSITIVE on this macro</param>
        /// <param name="xyComp">the xy compensation factor</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>z success, nz fail</returns>
        public override int FlashMacroPrimitiveForGCodePlot(GerberMacroVariableArray varArray, List<int> macroBuilderIDs, IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int xyComp)
        {
            // now do the flash
            int builderID = 0;

            if (isoPlotBuilder == null) return -1;
            if (stateMachine == null) return -1;
            if (macroBuilderIDs == null) return -1;

            // figure out width and height
            float workingWidth = GetWidth(varArray);
            int lineWidthInScreenCoords = (int)Math.Round(workingWidth * stateMachine.IsoPlotPointsPerAppUnit);
            float workingHeight = GetLineLength(varArray);
            int lineHeightInScreenCoords = (int)Math.Round(workingHeight * stateMachine.IsoPlotPointsPerAppUnit);

            //DebugMessage("efc_X=" + effectiveCenter_X.ToString() + ", " + "efc_Y=" + effectiveCenter_Y.ToString());

            // figure out the xyComp in plot coords, it comes in as screen coords here
            // we need it to feed into the rotation calculations
            float xyCompInPlotCoords = ((float)xyComp) / stateMachine.IsoPlotPointsPerAppUnit;
            xyCompInPlotCoords /= 2;

            // draw the line outline, is the polarity on?
            if (GetApertureIsOn(varArray) == false)
            {
                // we will need the center point coords 
                PointF centerPoint = GetCenterPoint(varArray);
                int primCenterPointInScreenCoords_X = (int)(centerPoint.X * stateMachine.IsoPlotPointsPerAppUnit);
                int primCenterPointInScreenCoords_Y = (int)(centerPoint.Y * stateMachine.IsoPlotPointsPerAppUnit);

                int effectiveCenter_X = x1 + primCenterPointInScreenCoords_X;
                int effectiveCenter_Y = y1 + primCenterPointInScreenCoords_Y;

                // get the center points 
                PointF centerPointLeft = this.GetLeftCenterCoord(varArray, xyCompInPlotCoords, true);
                PointF centerPointRight = this.GetRightCenterCoord(varArray, xyCompInPlotCoords, true);
                // convert them to screen coords
                int leftCenterPointInScreenCoords_X = (int)(centerPointLeft.X * stateMachine.IsoPlotPointsPerAppUnit);
                int leftCenterPointInScreenCoords_Y = (int)(centerPointLeft.Y * stateMachine.IsoPlotPointsPerAppUnit);
                int rightCenterPointInScreenCoords_X = (int)(centerPointRight.X * stateMachine.IsoPlotPointsPerAppUnit);
                int rightCenterPointInScreenCoords_Y = (int)(centerPointRight.Y * stateMachine.IsoPlotPointsPerAppUnit);
                // now calc the effective draw points
                int effectiveLeftCenterPoint_X = x1 + leftCenterPointInScreenCoords_X;
                int effectiveLeftCenterPoint_Y = y1 + leftCenterPointInScreenCoords_Y;
                int effectiveRightCenterPoint_X = x1 + rightCenterPointInScreenCoords_X;
                int effectiveRightCenterPoint_Y = y1 + rightCenterPointInScreenCoords_Y;

                // adjust the line height for the XY comp, the width was compensated earlier
                int lineHeightInScreenCoordsXYCompensated = lineHeightInScreenCoords - (xyComp);

                // clear polarity
                builderID =  isoPlotBuilder.DrawGSLineOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, effectiveLeftCenterPoint_X, effectiveLeftCenterPoint_Y, effectiveRightCenterPoint_X, effectiveRightCenterPoint_Y, lineHeightInScreenCoordsXYCompensated, stateMachine.BackgroundFillModeAccordingToPolarity);

                // get the bounding box of this primitive
                RectangleF primBoundingBox = GetMacroPrimitiveBoundingBox(varArray);
                // convert to screen values, this is still the primitive bounding rect though
                RectangleF primBoundingBoxInScreenCoords = MiscGraphicsUtils.ConvertRectFToScreenCoordinates(primBoundingBox, stateMachine);
                // the bounding box is relative to the center coords we adjust to absolute
                int newX0 = (int)(effectiveCenter_X - (primBoundingBoxInScreenCoords.Width/2));
                int newY0 = (int)(effectiveCenter_Y - (primBoundingBoxInScreenCoords.Height/2));
                int newX1 = (int)(newX0 + primBoundingBoxInScreenCoords.Width); 
                int newY1 = (int)(newY0 + primBoundingBoxInScreenCoords.Height);

                // now remove everything belonging to the macro at this point which is under our transparent flash
                isoPlotBuilder.EraseBuilderIDListFromRegionUsingABuilderIDHoriz(macroBuilderIDs, builderID, newX0, newY0, newX1, newY1, -1);

                // now remove the transparent flash INVERT_EDGE if it is not on something belonging to the macro. This prevents us 
                // cutting through non macro material.
                isoPlotBuilder.EraseBuilderIDIfNotOnCellWithIDsInList(macroBuilderIDs, builderID, newX0, newY0, newX1, newY1);

                // we do NOT return the builder ID here only the ones which draw in positive get returned
                return 0;
            }
            else
            {
                // get the center points 
                PointF centerPointLeft = this.GetLeftCenterCoord(varArray, xyCompInPlotCoords, false);
                PointF centerPointRight = this.GetRightCenterCoord(varArray, xyCompInPlotCoords, false);
                // convert them to screen coords
                int leftCenterPointInScreenCoords_X = (int)(centerPointLeft.X * stateMachine.IsoPlotPointsPerAppUnit);
                int leftCenterPointInScreenCoords_Y = (int)(centerPointLeft.Y * stateMachine.IsoPlotPointsPerAppUnit);
                int rightCenterPointInScreenCoords_X = (int)(centerPointRight.X * stateMachine.IsoPlotPointsPerAppUnit);
                int rightCenterPointInScreenCoords_Y = (int)(centerPointRight.Y * stateMachine.IsoPlotPointsPerAppUnit);
                // now calc the effective draw points
                int effectiveLeftCenterPoint_X = x1 + leftCenterPointInScreenCoords_X;
                int effectiveLeftCenterPoint_Y = y1 + leftCenterPointInScreenCoords_Y;
                int effectiveRightCenterPoint_X = x1 + rightCenterPointInScreenCoords_X;
                int effectiveRightCenterPoint_Y = y1 + rightCenterPointInScreenCoords_Y;

                // adjust the line height for the XY comp, the width was compensated earlier
                int lineHeightInScreenCoordsXYCompensated = lineHeightInScreenCoords + (xyComp);

                // dark polarity
                builderID = isoPlotBuilder.DrawGSLineOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, effectiveLeftCenterPoint_X, effectiveLeftCenterPoint_Y, effectiveRightCenterPoint_X, effectiveRightCenterPoint_Y, lineHeightInScreenCoordsXYCompensated, stateMachine.BackgroundFillModeAccordingToPolarity);
                return builderID;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We get a bounding box of the primitive. This is in non-scaled macro coords
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>a rectangle which will bound the primitives drawing</returns>
        public override RectangleF GetMacroPrimitiveBoundingBox(GerberMacroVariableArray varArray)
        {
            PointF ptUR;
            PointF ptLL;

            // get the max and min vals
            GetMaxMinXAndYValuesForPrimitive(varArray, out ptUR, out ptLL);

            // return the bounding box, note the width and height must always be positive
            return new RectangleF(ptLL.X, ptLL.Y, ptUR.X-ptLL.X, ptUR.Y-ptLL.Y);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the maximum and minimum coords for a primitive. 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <param name="ptLL">lower left point</param>
        /// <param name="ptUR">upper right point</param>
        /// <returns>true the coords are set with values, false they are not</returns>
        public override bool GetMaxMinXAndYValuesForPrimitive(GerberMacroVariableArray varArray, out PointF ptUR, out PointF ptLL)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            // get our points, note the UR, UL etc are just for notation. 
            // Rotations inherent in the Get.. call can move these around
            PointF tmpUR = GetURCoord(varArray);
            PointF tmpUL = GetULCoord(varArray);
            PointF tmpLR = GetLRCoord(varArray);
            PointF tmpLL = GetLLCoord(varArray);

            // minX
            if (minX > tmpUR.X) minX = tmpUR.X;
            if (minX > tmpUL.X) minX = tmpUL.X;
            if (minX > tmpLR.X) minX = tmpLR.X;
            if (minX > tmpLL.X) minX = tmpLL.X;
            // minY
            if (minY > tmpUR.Y) minY = tmpUR.Y;
            if (minY > tmpUL.Y) minY = tmpUL.Y;
            if (minY > tmpLR.Y) minY = tmpLR.Y;
            if (minY > tmpLL.Y) minY = tmpLL.Y;
            // maxX
            if (maxX < tmpUR.X) maxX = tmpUR.X;
            if (maxX < tmpUL.X) maxX = tmpUL.X;
            if (maxX < tmpLR.X) maxX = tmpLR.X;
            if (maxX < tmpLL.X) maxX = tmpLL.X;
            // maxY
            if (maxY < tmpUR.Y) maxY = tmpUR.Y;
            if (maxY < tmpUL.Y) maxY = tmpUL.Y;
            if (maxY < tmpLR.Y) maxY = tmpLR.Y;
            if (maxY < tmpLL.Y) maxY = tmpLL.Y;

            ptUR = new PointF(maxX, maxY);
            ptLL = new PointF(minX, minY);
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the UR point. Note due to rotations this point may not be 
        /// the actual UR point - you have to check
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public PointF GetURCoord(GerberMacroVariableArray varArray)
        {
            PointF centerPoint = GetCenterPoint(varArray);

            float workingXCenterCoord = centerPoint.X;
            float workingYCenterCoord = centerPoint.Y; ;
            float workingDegreesRotation = GetDegreesRotation(varArray);
            float workingWidth = GetWidth(varArray);
            float workingHeight = GetLineLength(varArray);

            // calc this now
            float xCoord = workingXCenterCoord + (workingWidth / 2);
            float yCoord = workingYCenterCoord + (workingHeight / 2);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                PointF pointToRotate = new PointF(xCoord, yCoord);
                // do the math and return
                return MiscGraphicsUtils.RotatePointAboutPointByDegrees(centerPoint, workingDegreesRotation, pointToRotate);
            }
            else
            {
                // no just return the existing value
                return new PointF(xCoord, yCoord);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the UL point. Note due to rotations this point may not be 
        /// the actual UL point - you have to check
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public PointF GetULCoord(GerberMacroVariableArray varArray)
        {
            PointF centerPoint = GetCenterPoint(varArray);

            float workingXCenterCoord = centerPoint.X;
            float workingYCenterCoord = centerPoint.Y; ;
            float workingDegreesRotation = GetDegreesRotation(varArray);
            float workingWidth = GetWidth(varArray);
            float workingHeight = GetLineLength(varArray);

            // calc this now
            float xCoord = workingXCenterCoord - (workingWidth / 2);
            float yCoord = workingYCenterCoord + (workingHeight / 2);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                PointF pointToRotate = new PointF(xCoord, yCoord);
                // do the math and return
                return MiscGraphicsUtils.RotatePointAboutPointByDegrees(centerPoint, workingDegreesRotation, pointToRotate);
            }
            else
            {
                // no just return the existing value
                return new PointF(xCoord, yCoord);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the LL point. Note due to rotations this point may not be 
        /// the actual LL point - you have to check
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public PointF GetLLCoord(GerberMacroVariableArray varArray)
        {
            PointF centerPoint = GetCenterPoint(varArray);

            float workingXCenterCoord = centerPoint.X;
            float workingYCenterCoord = centerPoint.Y; ;
            float workingDegreesRotation = GetDegreesRotation(varArray);
            float workingWidth = GetWidth(varArray);
            float workingHeight = GetLineLength(varArray);

            // calc this now
            float xCoord = workingXCenterCoord - (workingWidth / 2);
            float yCoord = workingYCenterCoord - (workingHeight / 2);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                PointF pointToRotate = new PointF(xCoord, yCoord);
                // do the math and return
                return MiscGraphicsUtils.RotatePointAboutPointByDegrees(centerPoint, workingDegreesRotation, pointToRotate);
            }
            else
            {
                // no just return the existing value
                return new PointF(xCoord, yCoord);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the LR point. Note due to rotations this point may not be 
        /// the actual LR point - you have to check
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public PointF GetLRCoord(GerberMacroVariableArray varArray)
        {
            PointF centerPoint = GetCenterPoint(varArray);

            float workingXCenterCoord = centerPoint.X;
            float workingYCenterCoord = centerPoint.Y; ;
            float workingDegreesRotation = GetDegreesRotation(varArray);
            float workingWidth = GetWidth(varArray);
            float workingHeight = GetLineLength(varArray);

            // calc this now
            float xCoord = workingXCenterCoord + (workingWidth / 2);
            float yCoord = workingYCenterCoord - (workingHeight / 2);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                PointF pointToRotate = new PointF(xCoord, yCoord);
                // do the math and return
                return MiscGraphicsUtils.RotatePointAboutPointByDegrees(centerPoint, workingDegreesRotation, pointToRotate);
            }
            else
            {
                // no just return the existing value
                return new PointF(xCoord, yCoord);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the horizontal Left Center point. Note due to rotations this point may not be 
        /// the actual Left Center point (or horizontal) - you have to check
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <param name="xyCompInPlotCoords">the xy compensation in plot coordinates (Gerber not screen coords)</param>
        /// <param name="wantInverseXYComp">if true we apply the xyComp in inverse</param>
        public PointF GetLeftCenterCoord(GerberMacroVariableArray varArray, float xyCompInPlotCoords, bool wantInverseXYComp)
        {

            PointF centerPoint = GetCenterPoint(varArray);

            float workingXCenterCoord = centerPoint.X;
            float workingYCenterCoord = centerPoint.Y; ;
            float workingDegreesRotation = GetDegreesRotation(varArray);
            float workingWidth = GetWidth(varArray);
            //float workingHeight = GetHeight(varArray);

            // calc this now
            float xCoord = 0;
            if(wantInverseXYComp==true) xCoord = workingXCenterCoord - (workingWidth / 2) + xyCompInPlotCoords;
            else xCoord = workingXCenterCoord - (workingWidth / 2) - xyCompInPlotCoords;
            float yCoord = workingYCenterCoord;

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                PointF pointToRotate = new PointF(xCoord, yCoord);
                // do the math and return
                return MiscGraphicsUtils.RotatePointAboutPointByDegrees(centerPoint, workingDegreesRotation, pointToRotate);
            }
            else
            {
                // no just return the existing value
                return new PointF(xCoord, yCoord);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the horizontal Right Center point. Note due to rotations this point may not be 
        /// the actual Right Center point (or horizontal) - you have to check
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <param name="xyCompInPlotCoords">the xy compensation in plot coordinates (Gerber not screen coords)</param>
        /// <param name="wantInverseXYComp">if true we apply the xyComp in inverse</param>
        public PointF GetRightCenterCoord(GerberMacroVariableArray varArray, float xyCompInPlotCoords, bool wantInverseXYComp)
        {
            PointF centerPoint = GetCenterPoint(varArray);

            float workingXCenterCoord = centerPoint.X;
            float workingYCenterCoord = centerPoint.Y; ;
            float workingDegreesRotation = GetDegreesRotation(varArray);
            float workingWidth = GetWidth(varArray);
            //float workingHeight = GetHeight(varArray);

            // calc this now
            float xCoord = 0;
            if (wantInverseXYComp == true) xCoord = workingXCenterCoord + (workingWidth / 2) - xyCompInPlotCoords;
            else xCoord = workingXCenterCoord + (workingWidth / 2) + xyCompInPlotCoords;
            float yCoord = workingYCenterCoord;

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                PointF pointToRotate = new PointF(xCoord, yCoord);
                // do the math and return
                return MiscGraphicsUtils.RotatePointAboutPointByDegrees(centerPoint, workingDegreesRotation, pointToRotate);
            }
            else
            {
                // no just return the existing value
                return new PointF(xCoord, yCoord);
            }
        }
    }
}

