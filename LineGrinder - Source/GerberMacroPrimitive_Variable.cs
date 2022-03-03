using System;
using System.Collections.Generic;
using System.Drawing;

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
    /// A class for an aperture macro VARIABLE primitive. This is a pseudo primitive
    /// which actually redefines a variable.
    /// </summary>
    public class GerberMacroPrimitive_Variable : GerberMacroPrimitive_Base
    {

        private GerberMacroVariable variableOperation = new GerberMacroVariable();
        private int variableNumber = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberMacroPrimitive_Variable() : base(MacroPrimitiveTypeEnum.MACROPRIMITIVE_VARIABLE)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the variable number
        /// </summary>
        public int VariableNumber
        {
            get
            {
                return variableNumber;
            }
            set
            {
                variableNumber = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the variable. Will never get/set null
        /// </summary>
        public GerberMacroVariable VariableOperation
        {
            get
            {
                if (variableOperation == null) variableOperation = new GerberMacroVariable();
                return variableOperation;
            }
            set
            {
                variableOperation = value;
                if (variableOperation == null) variableOperation = new GerberMacroVariable();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the parameters for this primitive from the supplied string
        /// </summary>
        /// <param name="primStr">the string with the primitives</param>
        /// <returns>z success, nz fail</returns>
        public override int ParsePrimitiveString(string primStr)
        {
            // these come in the form "$1=$2x0.8" and are used to re-assign a variable value on the fly

            char[] splitChars = {'='};
            string[] paramArray = null;

            if ((primStr==null) || (primStr.Length==0))
            {
                // should never happen
                LogMessage("ParsePrimitiveString(Variable), (primStr==null) || (primStr.Length==0)");
                return 10;
            }
            if (primStr.StartsWith(GerberLine_AMCode.AM_VARIABLE_PRIMITIVE) == false)
            {
                // should never happen
                LogMessage("ParsePrimitiveString(Variable), start marker not present");
                return 20;
            }

            // record this
            PrimitiveString = primStr;

            // split the parameters out
            paramArray = primStr.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            // this should never be null
            if (paramArray == null)
            {
                LogMessage("ParsePrimitiveString(Variable), blockArray == null");
                return 30;
            }
            // we only two parameters. The first is the variable number and the second is the operation to apply
            if (paramArray.Length != 2)
            {
                // should never happen
                LogMessage("ParsePrimitiveString(Variable), blockArray.Length != 2 is " + paramArray.Length.ToString());
                return 40;
            }
            try
            {
                // param 0 is the variable target, this must convert, remove the $ sign first
                string varNumOnly = paramArray[0].Replace("$", "");
                VariableNumber = Convert.ToInt32(varNumOnly.Trim());

                // param 1 is the operation
                VariableOperation = new GerberMacroVariable(paramArray[1].Trim());
            }
            catch (Exception ex)
            {
                LogMessage("ParsePrimitiveString(Circle), param conversion failed, msg="+ex.Message);
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

            // note this type of (pseudo) primitive does not draw. It is essentially a variable assignment.
            float outVar = VariableOperation.ProcessVariableStringToFloat(varArray);
            // now we have it, set it
            varArray[this.VariableNumber] = new GerberMacroVariable(this.VariableNumber, outVar.ToString());
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
            // note this type of (pseudo) primitive does not draw. It is essentially a variable assignment.
            float outVar = VariableOperation.ProcessVariableStringToFloat(varArray);
            // now we have it, set it
            varArray[this.VariableNumber] = new GerberMacroVariable(this.VariableNumber, outVar.ToString());

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We get a bounding box of the primitive. This is in non-scaled macro coords
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>a rectangle which will bound the primitives drawing</returns>
        public override RectangleF GetMacroPrimitiveBoundingBox(GerberMacroVariableArray varArray)
        {
            // should never happen
            throw new Exception("GetMacroPrimitiveBoundingBox called on variable primitive");
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
            // should never happen
            throw new Exception("GetMaxMinXAndYValuesForPrimitive called on variable primitive");
        }

    }
}

