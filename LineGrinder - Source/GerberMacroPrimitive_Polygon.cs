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
    /// A class for an aperture macro Polygon primitive
    /// </summary>
    public class GerberMacroPrimitive_Polygon : GerberMacroPrimitive_Base
    {

        // the diameter of the circle around the polygon
        private GerberMacroVariable diameter = new GerberMacroVariable();
        // the number of the sides in the polygon
        private GerberMacroVariable numSides = new GerberMacroVariable();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberMacroPrimitive_Polygon() : base(MacroPrimitiveTypeEnum.MACROPRIMITIVE_POLYGON)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the diameter 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetDiameter(GerberMacroVariableArray varArray)
        {
            if (diameter == null) diameter = new GerberMacroVariable();
            return diameter.ProcessVariableStringToFloat(varArray);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the diameter 
        /// </summary>
        /// <param name="diameterStr">a string defining the degrees rotation</param>
        public void SetDiameter(string diameterStr)
        {
            if ((diameterStr == null) || (diameterStr == "")) diameter = new GerberMacroVariable();
            else diameter = new GerberMacroVariable(diameterStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the numSides 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetNumSides(GerberMacroVariableArray varArray)
        {
            if (numSides == null) numSides = new GerberMacroVariable();
            return numSides.ProcessVariableStringToFloat(varArray);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the numSides 
        /// </summary>
        /// <param name="numSidesStr">a string defining the degrees rotation</param>
        public void SetNumSides(string numSidesStr)
        {
            if ((numSidesStr == null) || (numSidesStr == "")) numSides = new GerberMacroVariable();
            else numSides = new GerberMacroVariable(numSidesStr);
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
                LogMessage("ParsePrimitiveString(Polygon), (primStr==null) || (primStr.Length==0)");
                return 10;
            }
            if (primStr.StartsWith(GerberLine_AMCode.AM_POLYGON_PRIMITIVE + GerberLine_AMCode.AM_PRIMITIVE_DELIM) == false)
            {
                // should never happen
                LogMessage("ParsePrimitiveString(Polygon), start marker not present");
                return 20;
            }

            // record this
            PrimitiveString = primStr;

            // split the parameters out
            paramArray = primStr.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            // this should never be null
            if (paramArray == null)
            {
                LogMessage("ParsePrimitiveString(Polygon), blockArray == null");
                return 30;
            }
            // we expect seven parameters including the marker for the GerberLine_AMCode.AM_POLYGON_PRIMITIVE value
            if ((paramArray.Length == 7) == false)
            {
                // should never happen
                LogMessage("ParsePrimitiveString(Polygon), blockArray.Length != 7 is " + paramArray.Length.ToString());
                return 40;
            }
            try
            {
                // paramArray[0] is the AM_POLYGON_PRIMITIVE value

                // param 1 is the aperture state
                if (paramArray[1] == "1") SetApertureIsOn("1");
                else SetApertureIsOn("0");

                // param 2 is the numSides
                SetNumSides(paramArray[2]);

                // param 3 is the Center X Coord
                SetXCenterCoord(paramArray[3]);
                
                // param 4 is the Center Y Coord
                SetYCenterCoord(paramArray[4]);

                // param 5 is the diameter
                SetDiameter(paramArray[5]);

                // param 6 is the rotation. It is optional
                SetDegreesRotation(paramArray[6]);
            }
            catch (Exception ex)
            {
                LogMessage("ParsePrimitiveString(Polygon), param conversion failed, msg="+ex.Message);
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

            float workingDiameter = GetDiameter(varArray);
            int circleDiameter = (int)Math.Round(workingDiameter * stateMachine.IsoPlotPointsPerAppUnit);
            int workingNumSides = (int)GetNumSides(varArray);
            float rotationAngle = GetDegreesRotation(varArray);

            // we will need the center point coords 
            float primCenterPointInScreenCoords_X = GetXCenterCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit;
            float primCenterPointInScreenCoords_Y = GetYCenterCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit;

            // we can calc the effective center point of the current primitive, we do it out here to make this obvious
            float effectiveDrawPoint_X0 = primCenterPointInScreenCoords_X - bottomOfGraphicsObjInMacroScreenCoords_X;
            float effectiveDrawPoint_Y0 = primCenterPointInScreenCoords_Y - bottomOfGraphicsObjInMacroScreenCoords_Y;

            //DebugMessage("  effectiveDrawPoint=(" + effectiveDrawPoint_X0.ToString() + "," + effectiveDrawPoint_Y0.ToString() + ")");

            // figure out the brush. We do not use the one passed in, we can flash in in reverse here
            Brush tmpBrush = GetDrawBrush(stateMachine, varArray);

            // Now flash
            MiscGraphicsUtils.FillPolygonCenteredOnPoint(graphicsObj, tmpBrush, effectiveDrawPoint_X0, effectiveDrawPoint_Y0, workingNumSides, circleDiameter, rotationAngle);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flash a macro primitive for a Circle on a GCode Plot
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
            int workingOuterDiameter = int.MinValue;

            if (isoPlotBuilder == null) return -1;
            if (stateMachine == null) return -1;
            if (macroBuilderIDs == null) return -1;

            // figure out the diameter
            float workingDiameter = GetDiameter(varArray);
            workingOuterDiameter = (int)Math.Round((workingDiameter * stateMachine.IsoPlotPointsPerAppUnit));
            int workingNumSides = (int)GetNumSides(varArray);
            float rotationAngle = GetDegreesRotation(varArray);

            // we will need the center point coords 
            int primCenterPointInScreenCoords_X = (int)(GetXCenterCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit);
            int primCenterPointInScreenCoords_Y = (int)(GetYCenterCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit);

            int effectiveCenter_X = x1 + primCenterPointInScreenCoords_X;
            int effectiveCenter_Y = y1 + primCenterPointInScreenCoords_Y;

            //DebugMessage("efc_X=" + effectiveCenter_X.ToString() + ", " + "efc_Y=" + effectiveCenter_Y.ToString());

            if (GetApertureIsOn(varArray)==true)
            {
                // draw the circle
                int radius = (workingOuterDiameter + xyComp) / 2;
                builderID = isoPlotBuilder.DrawGSPolygonOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, effectiveCenter_X, effectiveCenter_Y, workingNumSides, workingOuterDiameter + xyComp, rotationAngle, stateMachine.BackgroundFillModeAccordingToPolarity);
                // return this, the caller keeps track of these
                return builderID;
            }
            else
            {
                // draw the circle, we use invert edges here so we draw the circle only if it is on some other background, use no fill. We will erase between it and the hole later
                int radius = (workingOuterDiameter - xyComp) / 2;
                if (radius <= 0) return -1;
                builderID = isoPlotBuilder.DrawGSPolygonOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, effectiveCenter_X, effectiveCenter_Y, workingNumSides, workingOuterDiameter - xyComp, rotationAngle, GSFillModeEnum.FillMode_NONE);

                // now we have to remove every builderID in the macro in every isoCell under the above object. This is not fast.
                // We know which ones might be there because we have a list

                int newX0 = effectiveCenter_X - radius;
                int newY0 = effectiveCenter_Y - radius;
                int newX1 = effectiveCenter_X + radius;
                int newY1 = effectiveCenter_Y + radius;

                // now remove everything belonging to the macro at this point which is under our transparent flash
                isoPlotBuilder.EraseBuilderIDListFromRegionUsingABuilderIDHoriz(macroBuilderIDs, builderID, newX0, newY0, newX1, newY1, -1);

                // now remove the transparent flash INVERT_EDGE if it is not on something belonging to the macro. This prevents us 
                // cutting through non macro material.
                isoPlotBuilder.EraseBuilderIDIfNotOnCellWithIDsInList(macroBuilderIDs, builderID, newX0, newY0, newX1, newY1);

                // we do NOT return the builder ID here only the ones which draw in positive get returned
                return 0;
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
            float workingDiameter = GetDiameter(varArray);

            float radius = workingDiameter / 2;

            float minX = GetXCenterCoord(varArray) - radius;
            float minY = GetYCenterCoord(varArray) - radius;

            // return the bounding box, note the width and height must always be positive
            return new RectangleF(minX, minY, workingDiameter, workingDiameter);
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
            float radius = GetDiameter(varArray) / 2;
            float minX = GetXCenterCoord(varArray) - radius;
            float minY = GetYCenterCoord(varArray) - radius;
            float maxX = GetXCenterCoord(varArray) + radius;
            float maxY = GetYCenterCoord(varArray) + radius;

            ptUR = new PointF(maxX, maxY);
            ptLL = new PointF(minX, minY);
            return true;
        }


    }
}

