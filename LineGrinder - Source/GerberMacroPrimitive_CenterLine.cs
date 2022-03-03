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
    /// A class for an aperture macro Center Line primitive
    /// </summary>
    public class GerberMacroPrimitive_CenterLine : GerberMacroPrimitive_Base
    {

        // the width of the line
        private GerberMacroVariable width = new GerberMacroVariable();

        // the height of the line
        private GerberMacroVariable height = new GerberMacroVariable();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberMacroPrimitive_CenterLine() : base(MacroPrimitiveTypeEnum.MACROPRIMITIVE_CENTERLINE)
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
        /// Gets the height 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetHeight(GerberMacroVariableArray varArray)
        {
            if (height == null) height = new GerberMacroVariable();
            return height.ProcessVariableStringToFloat(varArray);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the height 
        /// </summary>
        /// <param name="heightStr">a string defining the height</param>
        public void SetHeight(string heightStr)
        {
            if ((heightStr == null) || (heightStr == "")) height = new GerberMacroVariable();
            else height = new GerberMacroVariable(heightStr);
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
                LogMessage("ParsePrimitiveString(CLine), (primStr==null) || (primStr.Length==0)");
                return 10;
            }
            if (primStr.StartsWith(GerberLine_AMCode.AM_CLINE_PRIMITIVE + GerberLine_AMCode.AM_PRIMITIVE_DELIM) == false)
            {
                // should never happen
                LogMessage("ParsePrimitiveString(CLine), start marker not present");
                return 20;
            }

            // record this
            PrimitiveString = primStr;

            // split the parameters out
            paramArray = primStr.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            // this should never be null
            if (paramArray == null)
            {
                LogMessage("ParsePrimitiveString(CLine), blockArray == null");
                return 30;
            }
            // we expect seven parameters including the marker for the GerberLine_AMCode.AM_CLINE_PRIMITIVE value
            if ((paramArray.Length == 7) == false)
            {
                // should never happen
                LogMessage("ParsePrimitiveString(CLine), blockArray.Length != 6 is " + paramArray.Length.ToString());
                return 40;
            }
            try
            {
                // paramArray[0] is the AM_CLINE_PRIMITIVE value

                // param 1 is the aperture state
                if (paramArray[1] == "1") SetApertureIsOn("1");
                else SetApertureIsOn("0");

                // param 2 is the width
                SetWidth(paramArray[2]);

                // param 3 is the height
                SetHeight(paramArray[3]);

                // param 4 is the Center X Coord
                SetXCenterCoord(paramArray[4]);
                
                // param 5 is the Center Y Coord
                SetYCenterCoord(paramArray[5]);

                // param 6 is the rotation. It is not optional on this prim
                SetDegreesRotation(paramArray[6]);
            }
            catch (Exception ex)
            {
                LogMessage("ParsePrimitiveString(CLine), param conversion failed, msg="+ex.Message);
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

            // get the bounding box of this primitive
            RectangleF primBoundingBox = GetMacroPrimitiveBoundingBox(varArray);
            // convert to screen values, this is still the primitive bounding rect though
            RectangleF primBoundingBoxInScreenCoords = MiscGraphicsUtils.ConvertRectFToScreenCoordinates(primBoundingBox, stateMachine);

            float workingWidth = GetWidth(varArray);
            int lineWidthInScreenCoords = (int)Math.Round(workingWidth * stateMachine.IsoPlotPointsPerAppUnit);
            float workingHeight = GetHeight(varArray);
            int lineHeightInScreenCoords = (int)Math.Round(workingHeight * stateMachine.IsoPlotPointsPerAppUnit);

            // we will need the center point coords 
            int primCenterPointInScreenCoords_X = (int)(GetXCenterCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit);
            int primCenterPointInScreenCoords_Y = (int)(GetYCenterCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit);

            //DebugMessage("  BX=(" + bottomOfGraphicsObjInMacroScreenCoords_X.ToString() + "," + bottomOfGraphicsObjInMacroScreenCoords_Y.ToString() + ")");
            //DebugMessage("  primBoundingBox=" + primBoundingBox.ToString());
            //DebugMessage("  primBoundingBoxInScreenCoords=" + primBoundingBoxInScreenCoords.ToString());
            //DebugMessage("  CPX,Y=(" + primCenterPointInScreenCoords_X.ToString() + "," + primCenterPointInScreenCoords_Y.ToString() + ")");

            // we can calc the effective center point of the current primitive, we do it out here to make this obvious
            int effectiveDrawPoint_X0 = (int)(primCenterPointInScreenCoords_X - bottomOfGraphicsObjInMacroScreenCoords_X);
            int effectiveDrawPoint_Y0 = (int)(primCenterPointInScreenCoords_Y - bottomOfGraphicsObjInMacroScreenCoords_Y);

            // get the center points 
            PointF centerPointLeft = this.GetLeftCenterCoord(varArray, 0, false);
            PointF centerPointRight = this.GetRightCenterCoord(varArray, 0, false);
            // convert them to screen coords
            int leftCenterPointInScreenCoords_X = (int)(centerPointLeft.X * stateMachine.IsoPlotPointsPerAppUnit);
            int leftCenterPointInScreenCoords_Y = (int)(centerPointLeft.Y * stateMachine.IsoPlotPointsPerAppUnit);
            int rightCenterPointInScreenCoords_X = (int)(centerPointRight.X * stateMachine.IsoPlotPointsPerAppUnit);
            int rightCenterPointInScreenCoords_Y = (int)(centerPointRight.Y * stateMachine.IsoPlotPointsPerAppUnit);
            // now calc the effective draw points
            int effectiveLeftCenterPoint_X = (int)(leftCenterPointInScreenCoords_X - bottomOfGraphicsObjInMacroScreenCoords_X);
            int effectiveLeftCenterPoint_Y = (int)(leftCenterPointInScreenCoords_Y - bottomOfGraphicsObjInMacroScreenCoords_Y);
            int effectiveRightCenterPoint_X = (int)(rightCenterPointInScreenCoords_X - bottomOfGraphicsObjInMacroScreenCoords_X);
            int effectiveRightCenterPoint_Y = (int)(rightCenterPointInScreenCoords_Y - bottomOfGraphicsObjInMacroScreenCoords_Y);

            //// Draw gerber line according to exposure
            Pen workingPen = GetDrawPen(stateMachine, varArray, lineHeightInScreenCoords); 
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
            float workingHeight = GetHeight(varArray);
            int lineHeightInScreenCoords = (int)Math.Round(workingHeight * stateMachine.IsoPlotPointsPerAppUnit);

            // we will need the center point coords 
            int primCenterPointInScreenCoords_X = (int)(GetXCenterCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit);
            int primCenterPointInScreenCoords_Y = (int)(GetYCenterCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit);

            int effectiveCenter_X = x1 + primCenterPointInScreenCoords_X;
            int effectiveCenter_Y = y1 + primCenterPointInScreenCoords_Y;

            //DebugMessage("efc_X=" + effectiveCenter_X.ToString() + ", " + "efc_Y=" + effectiveCenter_Y.ToString());

            // figure out the xyComp in plot coords, it comes in as screen coords here
            // we need it to feed into the rotation calculations
            float xyCompInPlotCoords = ((float)xyComp) / stateMachine.IsoPlotPointsPerAppUnit;
            xyCompInPlotCoords /= 2;


            // draw the line outline, is the polarity on?
            if (GetApertureIsOn(varArray) == false)
            {
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

            float workingXCenterCoord = GetXCenterCoord(varArray);
            float workingYCenterCoord = GetYCenterCoord(varArray);
            float workingDegreesRotation = GetDegreesRotation(varArray);
            float workingWidth = GetWidth(varArray);
            float workingHeight = GetHeight(varArray);

            // calc this now
            float xCoord = workingXCenterCoord + (workingWidth / 2);
            float yCoord = workingYCenterCoord + (workingHeight / 2);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                PointF pointToRotate = new PointF(xCoord, yCoord);
                PointF centerPoint = new PointF(workingXCenterCoord, workingYCenterCoord);
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

            float workingXCenterCoord = GetXCenterCoord(varArray);
            float workingYCenterCoord = GetYCenterCoord(varArray);
            float workingDegreesRotation = GetDegreesRotation(varArray);
            float workingWidth = GetWidth(varArray);
            float workingHeight = GetHeight(varArray);

            // calc this now
            float xCoord = workingXCenterCoord - (workingWidth / 2);
            float yCoord = workingYCenterCoord + (workingHeight / 2);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                PointF pointToRotate = new PointF(xCoord, yCoord);
                PointF centerPoint = new PointF(workingXCenterCoord, workingYCenterCoord);
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

            float workingXCenterCoord = GetXCenterCoord(varArray);
            float workingYCenterCoord = GetYCenterCoord(varArray);
            float workingDegreesRotation = GetDegreesRotation(varArray);
            float workingWidth = GetWidth(varArray);
            float workingHeight = GetHeight(varArray);

            // calc this now
            float xCoord = workingXCenterCoord - (workingWidth / 2);
            float yCoord = workingYCenterCoord - (workingHeight / 2);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                PointF pointToRotate = new PointF(xCoord, yCoord);
                PointF centerPoint = new PointF(workingXCenterCoord, workingYCenterCoord);
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

            float workingXCenterCoord = GetXCenterCoord(varArray);
            float workingYCenterCoord = GetYCenterCoord(varArray);
            float workingDegreesRotation = GetDegreesRotation(varArray);
            float workingWidth = GetWidth(varArray);
            float workingHeight = GetHeight(varArray);

            // calc this now
            float xCoord = workingXCenterCoord + (workingWidth / 2);
            float yCoord = workingYCenterCoord - (workingHeight / 2);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                PointF pointToRotate = new PointF(xCoord, yCoord);
                PointF centerPoint = new PointF(workingXCenterCoord, workingYCenterCoord);
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

            float workingXCenterCoord = GetXCenterCoord(varArray);
            float workingYCenterCoord = GetYCenterCoord(varArray);
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
                PointF centerPoint = new PointF(workingXCenterCoord, workingYCenterCoord);
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

            float workingXCenterCoord = GetXCenterCoord(varArray);
            float workingYCenterCoord = GetYCenterCoord(varArray);
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
                PointF centerPoint = new PointF(workingXCenterCoord, workingYCenterCoord);
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

