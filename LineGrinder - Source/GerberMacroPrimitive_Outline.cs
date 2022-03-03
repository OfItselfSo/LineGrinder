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
    public class GerberMacroPrimitive_Outline : GerberMacroPrimitive_Base
    {

        public const int MIN_VERTEX_COUNT = 3;
        public const int MAX_VERTEX_COUNT = 5000;

        // the number of the vertexes in the outline. NOTE the first and last point
        // must be the same and there must be at least 3 and less than 5000
        private GerberMacroVariable numVertexes = new GerberMacroVariable();

        // this is our array of vertex points
        private GerberMacroVariablePair[] vertexPointArray = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberMacroPrimitive_Outline() : base(MacroPrimitiveTypeEnum.MACROPRIMITIVE_OUTLINE)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the numVertexes 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetNumVertexes(GerberMacroVariableArray varArray)
        {
            if (numVertexes == null) numVertexes = new GerberMacroVariable();
            return numVertexes.ProcessVariableStringToFloat(varArray);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the numVertexes 
        /// </summary>
        /// <param name="numVertexesStr">a string defining the degrees rotation</param>
        public void SetNumVertexes(string numVertexesStr)
        {
            if ((numVertexesStr == null) || (numVertexesStr == "")) numVertexes = new GerberMacroVariable();
            else numVertexes = new GerberMacroVariable(numVertexesStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Initializes the vertex point array
        /// </summary>
        /// <param name="numVertexPoints">the number of vertex points</param>
        private void InitVertexPointArray(int numVertexPoints)
        {
            vertexPointArray = new GerberMacroVariablePair[numVertexPoints];
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets a vertex point in the vertexPointArray
        /// </summary>
        /// <param name="index">the index into the array</param>
        /// <param name="xVal">the X value to set</param>
        /// <param name="yVal">the Y value to set</param>
        private void SetVertexPoint(int index, string xVal, string yVal)
        {
            // just set it. We do not check for bounds - just let it blow
            vertexPointArray[index] = new GerberMacroVariablePair(xVal, yVal);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a vertex point from the vertexPointArray, as a converted and 
        /// rotated PointF
        /// </summary>
        /// <param name="index">the index into the array</param>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>the vertex point</returns>
        private PointF GetVertexPoint(GerberMacroVariableArray varArray, int index)
        {
            // get the raw point
            PointF pointToRotate = vertexPointArray[index].ProcessVariablePairToFloat(varArray);

            // we willneed the rotation
            float workingDegreesRotation = GetDegreesRotation(varArray);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                // yes, return the converted value
                double angleInRadians = MiscGraphicsUtils.DegreesToRadians(workingDegreesRotation);
                float newXVal = (float)((pointToRotate.X * Math.Cos(angleInRadians)) - pointToRotate.Y * Math.Sin(angleInRadians));
                float newYVal = (float)((pointToRotate.Y * Math.Cos(angleInRadians)) + pointToRotate.X * Math.Sin(angleInRadians));
                return new PointF(newXVal, newYVal);
            }
            else
            {
                // no just return the existing value
                return pointToRotate;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a vertexPointArray, as an equivalent array of converted and 
        /// rotated PointFs
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>the vertex point</returns>
        private PointF[] GetPointArrayAsPointFArray(GerberMacroVariableArray varArray)
        {
            PointF[] outArray = new PointF[vertexPointArray.Length];

            // loop through the array
            for (int i = 0; i < vertexPointArray.Length; i++)
            {
                // convert and process each one
                outArray[i] = GetVertexPoint(varArray, i);
            }
            // return it
            return outArray;
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
                LogMessage("ParsePrimitiveString(Outline), (primStr==null) || (primStr.Length==0)");
                return 10;
            }
            if (primStr.StartsWith(GerberLine_AMCode.AM_OUTLINE_PRIMITIVE + GerberLine_AMCode.AM_PRIMITIVE_DELIM) == false)
            {
                // should never happen
                LogMessage("ParsePrimitiveString(Outline), start marker not present");
                return 20;
            }

            // record this
            PrimitiveString = primStr;

            // split the parameters out
            paramArray = primStr.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            // this should never be null
            if (paramArray == null)
            {
                LogMessage("ParsePrimitiveString(Outline), blockArray == null");
                return 30;
            }
            // we expect seven parameters including the marker for the GerberLine_AMCode.AM_OUTLINE_PRIMITIVE value
            if ((paramArray.Length < 10) == true)
            {
                // should never happen
                LogMessage("ParsePrimitiveString(Outline), blockArray.Length < 10 is " + paramArray.Length.ToString());
                return 40;
            }
            try
            {
                // paramArray[0] is the AM_OUTLINE_PRIMITIVE value

                // param 1 is the aperture state
                if (paramArray[1] == "1") SetApertureIsOn("1");
                else SetApertureIsOn("0");

                // param 2 is the numVertexes. 
                // We require this to be a number cannot be a variable. The spec is unclear on this. 
                // But we cannot process with it as a $variable. Hopefully not something people are 
                // going to want to do
                SetNumVertexes(paramArray[2]);

                // get the number of vertexes here. Pass in a dummy variable array since we 
                // do not have one yet. This MUST parse or we cannot process
                int workingNumVertexes = (int)GetNumVertexes(new GerberMacroVariableArray());
                if (workingNumVertexes < MIN_VERTEX_COUNT)
                {
                    // should never happen
                    LogMessage("ParsePrimitiveString(Outline), numVertexes < " + MIN_VERTEX_COUNT.ToString() + "  is " + workingNumVertexes.ToString());
                    return 50;
                }
                if (workingNumVertexes > MAX_VERTEX_COUNT)
                {
                    // should never happen
                    LogMessage("ParsePrimitiveString(Outline), numVertexes > " + MAX_VERTEX_COUNT.ToString() + "  is " + workingNumVertexes.ToString());
                    return 51;
                }
                // we add 1 to this. The spec repeats the last point so a 3 vertex triangle actually has four points where the first and last
                // are identical.
                workingNumVertexes += 1;
                // initialize the vertex point array
                InitVertexPointArray(workingNumVertexes);

                // the next n pairs are the X,Y coords of the vertexes. We start at 3 according to spec
                for (int i=0;i< workingNumVertexes;i++)
                {
                    int xCoordIndex = 3 + (2*i);         // the index of this X coord in the paramArray
                    int yCoordIndex = (3 + 1) + (2*i);   // the index of this Y coord in the paramArray
                    // sanity checks
                    if (xCoordIndex >= paramArray.Length)
                    {
                        // should never happen
                        LogMessage("ParsePrimitiveString(Outline), xCoordIndex value of " + xCoordIndex.ToString() + " is too large for parameter array of length " + paramArray.Length.ToString());
                        return 52;
                    }
                    if (yCoordIndex >= paramArray.Length)
                    {
                        // should never happen
                        LogMessage("ParsePrimitiveString(Outline), yCoordIndex value of " + yCoordIndex.ToString() + " is too large for parameter array of length " + paramArray.Length.ToString());
                        return 53;
                    }
                    SetVertexPoint(i, paramArray[xCoordIndex], paramArray[yCoordIndex]);
                }

                // param (3+workingNumVertexes) is the rotation
                SetDegreesRotation(paramArray[(3 + (workingNumVertexes*2))]);
            }
            catch (Exception ex)
            {
                LogMessage("ParsePrimitiveString(Outline), param conversion failed, msg="+ex.Message);
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

            // get the VertexPointArray converted and rotated
            PointF[] tmpPointArray = GetPointArrayAsPointFArray(varArray);
            // collect the vertexPoints we operate on here
            PointF[] vertexPoints = new PointF[vertexPointArray.Length];

            // loop through the tmpPointArray
            for (int i = 0; i < tmpPointArray.Length; i++)
            {
                //DebugMessage("rawPoint=" + tmpPointArray[i].ToString());

                // we will need them as screen coords 
                float primPointInScreenCoords_X = tmpPointArray[i].X * stateMachine.IsoPlotPointsPerAppUnit;
                float primPointInScreenCoords_Y = tmpPointArray[i].Y * stateMachine.IsoPlotPointsPerAppUnit;

                // we can calc the effective draw point of the current primitive, we do it out here to make this obvious
                float effectiveDrawPoint_X = primPointInScreenCoords_X - bottomOfGraphicsObjInMacroScreenCoords_X;
                float effectiveDrawPoint_Y = primPointInScreenCoords_Y - bottomOfGraphicsObjInMacroScreenCoords_Y;

                // create a new point and stuff it in the array
                vertexPoints[i] = new PointF(effectiveDrawPoint_X, effectiveDrawPoint_Y);
                //DebugMessage("  vertexPoint=" + vertexPoints[i].ToString());
            }


            // figure out the brush. We do not use the one passed in, we can flash in in reverse here
            Brush tmpBrush = GetDrawBrush(stateMachine, varArray);

            // Now flash
            MiscGraphicsUtils.FillOutlineCenteredOnPoint(graphicsObj, tmpBrush, vertexPoints);
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

            if (isoPlotBuilder == null) return -1;
            if (stateMachine == null) return -1;
            if (macroBuilderIDs == null) return -1;

            // get the VertexPointArray converted and rotated
            PointF[] tmpPointArray = GetPointArrayAsPointFArray(varArray);
            // collect the vertexPoints we operate on here
            PointF[] vertexPoints = new PointF[vertexPointArray.Length];

            // loop through the tmpPointArray
            for (int i = 0; i < tmpPointArray.Length; i++)
            {
                //DebugMessage("rawPoint=" + tmpPointArray[i].ToString());

                // we will need them as screen coords 
                float primPointInScreenCoords_X = tmpPointArray[i].X * stateMachine.IsoPlotPointsPerAppUnit;
                float primPointInScreenCoords_Y = tmpPointArray[i].Y * stateMachine.IsoPlotPointsPerAppUnit;

                // we can calc the effective draw point of the current primitive, we do it out here to make this obvious
                float effectiveDrawPoint_X = x1 + primPointInScreenCoords_X;
                float effectiveDrawPoint_Y = y1 + primPointInScreenCoords_Y;

                // create a new point and stuff it in the array
                vertexPoints[i] = new PointF(effectiveDrawPoint_X, effectiveDrawPoint_Y);
                //DebugMessage("  vertexPoint=" + vertexPoints[i].ToString());
            }

            if (GetApertureIsOn(varArray)==true)
            {
                // draw the circle

                builderID = isoPlotBuilder.DrawGSOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, vertexPoints, stateMachine.BackgroundFillModeAccordingToPolarity);
                // return this, the caller keeps track of these
                return builderID;
            }
            else
            {
                // draw the circle, we use invert edges here so we draw the circle only if it is on some other background, use no fill. We will erase between it and the hole later
                builderID = isoPlotBuilder.DrawGSOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, vertexPoints, GSFillModeEnum.FillMode_NONE);

                // now we have to remove every builderID in the macro in every isoCell under the above object. This is not fast.
                // We know which ones might be there because we have a list

                // we will need the center point coords 
                int primCenterPointInScreenCoords_X = (int)(GetXCenterCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit);
                int primCenterPointInScreenCoords_Y = (int)(GetYCenterCoord(varArray) * stateMachine.IsoPlotPointsPerAppUnit);

                int effectiveCenter_X = x1 + primCenterPointInScreenCoords_X;
                int effectiveCenter_Y = y1 + primCenterPointInScreenCoords_Y;

                // get the bounding box of this primitive
                RectangleF primBoundingBox = GetMacroPrimitiveBoundingBox(varArray);
                // convert to screen values, this is still the primitive bounding rect though
                RectangleF primBoundingBoxInScreenCoords = MiscGraphicsUtils.ConvertRectFToScreenCoordinates(primBoundingBox, stateMachine);
                // the bounding box is relative to the center coords we adjust to absolute
                int newX0 = (int)(effectiveCenter_X - (primBoundingBoxInScreenCoords.Width / 2));
                int newY0 = (int)(effectiveCenter_Y - (primBoundingBoxInScreenCoords.Height / 2));
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
            return new RectangleF(ptLL.X, ptLL.Y, ptUR.X - ptLL.X, ptUR.Y - ptLL.Y);
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


            // get the VertexPointArray converted and rotated
            PointF[] tmpPointArray = GetPointArrayAsPointFArray(varArray);
            // collect the vertexPoints we operate on here
            PointF[] vertexPoints = new PointF[vertexPointArray.Length];

            // loop through the tmpPointArray
            for (int i = 0; i < tmpPointArray.Length; i++)
            {
                PointF tmpPT = tmpPointArray[i];
                if (minX > tmpPT.X) minX = tmpPT.X;
                if (minY > tmpPT.Y) minY = tmpPT.Y;
                if (maxX < tmpPT.X) maxX = tmpPT.X;
                if (maxY < tmpPT.Y) maxY = tmpPT.Y;
            }

            // return this
            ptUR = new PointF(maxX, maxY);
            ptLL = new PointF(minX, minY);
            return true;
        }


    }
}

