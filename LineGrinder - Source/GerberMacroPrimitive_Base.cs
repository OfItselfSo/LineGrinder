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
    /// A base class for all Gerber Aperture Macro Primitives
    /// </summary>
    public abstract class GerberMacroPrimitive_Base : OISObjBase
    {
        // the type of primitive we are
        private MacroPrimitiveTypeEnum macroPrimitiveType = MacroPrimitiveTypeEnum.MACROPRIMITIVE_UNKNOWN;

        // some dimensions common to all macro primitives
        private GerberMacroVariable xCenterCoord = new GerberMacroVariable();
        private GerberMacroVariable yCenterCoord = new GerberMacroVariable();
        private GerberMacroVariable degreesRotation = new GerberMacroVariable();
        private const string DEFAULT_APERTURE_STATE = "1";
        private GerberMacroVariable apertureIsOn = new GerberMacroVariable(DEFAULT_APERTURE_STATE);
        private string primitiveString = "";

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberMacroPrimitive_Base(MacroPrimitiveTypeEnum macroPrimitiveTypeIn)
        {
            // set this
            macroPrimitiveType = macroPrimitiveTypeIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the macro primitive type. There is no set accessor. This is set during 
        /// construction
        /// </summary>
        public MacroPrimitiveTypeEnum MacroPrimitiveType
        {
            get
            {
                return macroPrimitiveType;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the primitiveString.  Never gets/sets null.
        /// </summary>
        public string PrimitiveString
        {
            get
            {
                if(primitiveString==null) primitiveString = "";
                return primitiveString;
            }
            set
            {
                primitiveString = value;
                if (primitiveString == null) primitiveString = "";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the aperture on/off state. This may require using numbered variables
        /// to resolve this
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public bool GetApertureIsOn(GerberMacroVariableArray varArray)
        {
            if (apertureIsOn == null) apertureIsOn = new GerberMacroVariable(DEFAULT_APERTURE_STATE);
            return Convert.ToBoolean(apertureIsOn.ProcessVariableStringToFloat(varArray));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the degreesRotation in degrees counter clockwise
        /// </summary>
        /// <param name="apertureStateStr">a string defining the aperture state</param>
        public void SetApertureIsOn(string apertureStateStr)
        {
            if ((apertureStateStr == null) || (apertureStateStr == "")) apertureIsOn = new GerberMacroVariable(DEFAULT_APERTURE_STATE);
            else apertureIsOn = new GerberMacroVariable(apertureStateStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the brush we use to draw the macro primitive. This depends on the 
        /// aperture on/off state
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public Brush GetDrawBrush(GerberFileStateMachine stateMachine, GerberMacroVariableArray varArray)
        {
            // gets the draw brush according to the aperture on/off state
            if (GetApertureIsOn(varArray) == false) return ApplicationColorManager.DEFAULT_MACRO_TRANSPARENT_COLOR_Brush;
            else return stateMachine.GerberApertureBrush;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the pen we use to draw the macro primitive. This depends on the 
        /// aperture on/off state. Remember to dispose of this pen
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public Pen GetDrawPen(GerberFileStateMachine stateMachine, GerberMacroVariableArray varArray, int penWidth)
        {
            // gets the draw pen according to the aperture on/off state
            if (GetApertureIsOn(varArray) == false) return new Pen(ApplicationColorManager.DEFAULT_MACRO_TRANSPARENT_COLOR, penWidth);
            else return new Pen(stateMachine.GerberForegroundColor, penWidth);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the degreesRotation in degrees counter clockwise
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetDegreesRotation(GerberMacroVariableArray varArray)
        {
            if (degreesRotation == null) degreesRotation = new GerberMacroVariable();
            return degreesRotation.ProcessVariableStringToFloat(varArray);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the degreesRotation in degrees counter clockwise
        /// </summary>
        /// <param name="degreesRotationStr">a string defining the degrees rotation</param>
        public void SetDegreesRotation(string degreesRotationStr)
        {
            if((degreesRotationStr==null) || (degreesRotationStr=="")) degreesRotation = new GerberMacroVariable();
            else degreesRotation = new GerberMacroVariable(degreesRotationStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the xCenterCoord. 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetXCenterCoord(GerberMacroVariableArray varArray)
        {

            if (xCenterCoord == null) xCenterCoord = new GerberMacroVariable();
            float workingXCenterCoord = xCenterCoord.ProcessVariableStringToFloat(varArray);
            float workingDegreesRotation = GetDegreesRotation(varArray);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                // yes, return the converted value
                if (yCenterCoord == null) yCenterCoord = new GerberMacroVariable();
                float workingYCenterCoord = yCenterCoord.ProcessVariableStringToFloat(varArray);
                double angleInRadians = MiscGraphicsUtils.DegreesToRadians(workingDegreesRotation);
                return (float)((workingXCenterCoord * Math.Cos(angleInRadians)) - workingYCenterCoord * Math.Sin(angleInRadians));
            }
            else
            {
                // no just return the existing value
                return workingXCenterCoord;
            }

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the xCenterCoord. 
        /// </summary>
        /// <param name="centerXCoordStr">a string defining the centerXCoord</param>
        public void SetXCenterCoord(string centerXCoordStr)
        {
            if ((centerXCoordStr == null) || (centerXCoordStr == "")) xCenterCoord = new GerberMacroVariable();
            else xCenterCoord = new GerberMacroVariable(centerXCoordStr);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the yCenterCoord. 
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetYCenterCoord(GerberMacroVariableArray varArray)
        {

            if (yCenterCoord == null) yCenterCoord = new GerberMacroVariable();
            float workingYCenterCoord = yCenterCoord.ProcessVariableStringToFloat(varArray);
            float workingDegreesRotation = GetDegreesRotation(varArray);

            // do we have a rotation
            if (workingDegreesRotation != 0)
            {
                // yes, return the converted value
                if (xCenterCoord == null) xCenterCoord = new GerberMacroVariable();
                float workingXCenterCoord = xCenterCoord.ProcessVariableStringToFloat(varArray);
                double angleInRadians = MiscGraphicsUtils.DegreesToRadians(workingDegreesRotation);
                return (float)((workingYCenterCoord * Math.Cos(angleInRadians)) + workingXCenterCoord * Math.Sin(angleInRadians));
            }
            else
            {
                // no just return the eyisting value
                return workingYCenterCoord;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the yCenterCoord. 
        /// </summary>
        /// <param name="centerYCoordStr">a string defining the centerYCoord</param>
        public void SetYCenterCoord(string centerYCoordStr)
        {
            if ((centerYCoordStr == null) || (centerYCoordStr == "")) yCenterCoord = new GerberMacroVariable();
            else yCenterCoord = new GerberMacroVariable(centerYCoordStr);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the distance from the center to the origin
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float GetCenterDistance(GerberMacroVariableArray varArray)
        {
            float xCenter = GetXCenterCoord(varArray);
            float yCenter = GetYCenterCoord(varArray);
            // get the distance
            return MiscGraphicsUtils.GetDistanceBetweenTwoPoints(new PointF(0, 0), new PointF(xCenter, yCenter));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Must be implemented by the derived class to process the macro primitive
        /// string to get the parameters of the primitive
        /// </summary>
        /// <param name="primStr">the string with the primitives</param>
        /// <returns>z success, nz fail</returns>
        public abstract int ParsePrimitiveString(string primStr);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If we fill a shape with the macro at the current coordinates. This 
        /// simulates the aperture flash of a Gerber plotter
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="bottomOfGraphicsObjInMacroScreenCoords_X">the macro screen coord which represents the bottom 0 coord of the graphics object</param>
        /// <param name="bottomOfGraphicsObjInMacroScreenCoords_Y">the macro screen coord which represents the bottom 0 coord of the graphics object</param>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>z success, nz fail</returns>
        public abstract void FlashMacroPrimitiveForGerberPlot(GerberFileStateMachine stateMachine, GerberMacroVariableArray varArray, Graphics graphicsObj, Brush workingBrush, float bottomOfGraphicsObjInMacroScreenCoords_X, float bottomOfGraphicsObjInMacroScreenCoords_Y);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flash a macro primitive for a Circle on a GCode Plot
        /// </summary>
        /// <param name="isoPlotBuilder">the builder opbject</param>
        /// <param name="stateMachine">the statemachine</param>
        /// <param name="xyComp">the xy compensation factor</param>
        /// <param name="macroBuilderIDs">a list of builder IDs which have drawn in POSITIVE on this macro</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>z success, nz fail</returns>
        public abstract int FlashMacroPrimitiveForGCodePlot(GerberMacroVariableArray varArray, List<int> macroBuilderIDs, IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int xyComp);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We get a bounding box of the primitive. This is in non-scaled macro coords
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>a rectangle which will bound the primitives drawing</returns>
        public abstract RectangleF GetMacroPrimitiveBoundingBox(GerberMacroVariableArray varArray);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the maximum and minimum coords for a primitive. 
        /// </summary>
        /// <param name="ptLL">lower left point</param>
        /// <param name="ptUR">upper right point</param>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>true the coords are set with values, false they are not</returns>
        public abstract bool GetMaxMinXAndYValuesForPrimitive(GerberMacroVariableArray varArray, out PointF ptUR, out PointF ptLL);

    }
}

