using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LineGrinder
{

    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A base class describing a Gerber Aperture: Macro
    /// </summary>
    public class GerberAperture_Macro : GerberAperture_Base
    {

        private string macroName = "";
        // 
        private GerberMacroVariableArray variableArray = new GerberMacroVariableArray();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="macroNameIn">the name of the macro</param>
        public GerberAperture_Macro(string macroNameIn) : base(ApertureTypeEnum.APERTURETYPE_MACRO)
        {
            macroName = macroNameIn;
            if (macroName == null) macroName = "";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the macro (AM Code object) for this object. This contains
        /// the definition of the macro in the form of primitives. 
        /// </summary>
        /// <returns>the ADCode object or null for fail</returns>
        /// <param name="stateMachine">the statemachine</param>
        public GerberLine_AMCode GetMacroObject(GerberFileStateMachine stateMachine)
        {
            return stateMachine.MacroCollection.GetMacroByName(MacroName);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the variable array - will never return null
        /// </summary>
        public GerberMacroVariableArray VariableArray
        {
            get
            { 
                if (variableArray == null) variableArray = new GerberMacroVariableArray();
                return variableArray;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates maximum width of the aperture in any direction
        /// from a point. 
        /// </summary>
        public override float GetApertureDimension()
        {
            // the incremental dimension for a circle is the radius
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the name of the macro. There is no set accessor. This is 
        /// done in construction. Will never return null
        /// </summary>
        public string MacroName
        {
            get
            {
                if (macroName == null) macroName = "";
                return macroName;
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
        public override void FlashApertureForGerberPlot(GerberFileStateMachine stateMachine, Graphics graphicsObj, Brush workingBrush, float x1, float y1)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            if (stateMachine == null) return;

            // get the macro AM code object for this aperture
            GerberLine_AMCode workingMacro = GetMacroObject(stateMachine);
            if(workingMacro==null)
            {
                throw new Exception("No macro definition found for name " + MacroName);
            }

            // get the largest bounding box for all primitives in the macro
            RectangleF boundingRectF = workingMacro.GetMacroPrimitivesBoundingBox(VariableArray);

            // note the bottom of this box is not the (0,0) point of any one primitive. It is also not
            // the (0,0) point of the macro.

            // convert to screen values, this is still the macro bounding rect though
            RectangleF boundingRectFInScreenCoord = MiscGraphicsUtils.ConvertRectFToScreenCoordinates(boundingRectF, stateMachine);

            // set up to create a bitmap we draw our primitives on, we convert our floats to int
            // the +1 here is to ensure we round up. The bounding box can be bigger, but smaller is bad
            // we can assume the width and height here are positive. This should have been checked much earlier
            int boundingWidth = (int)(boundingRectFInScreenCoord.Width) + 1;
            int boundingHeight = (int)(boundingRectFInScreenCoord.Height) + 1;
           
            // create an appropriately sized bitmap
            Bitmap macroBitmap = new Bitmap(boundingWidth, boundingHeight);
            // get a graphics object from the bitmap. We use this to draw the macro
            Graphics macroGraphicsObj = Graphics.FromImage(macroBitmap);
            // fill the rectangle with a background color not normally used on Gerber plots
            macroGraphicsObj.FillRectangle(new SolidBrush(ApplicationColorManager.DEFAULT_MACRO_TRANSPARENT_COLOR), 0,0, boundingWidth, boundingHeight);

            // for diagnostics
            //DebugTODO("remove this");
            //macroGraphicsObj.DrawEllipse(Pens.Maroon, 0, 0, 25, 25);

            //// we run through our primitive list in order and and flash 
            foreach (GerberMacroPrimitive_Base primObj in workingMacro.MacroPrimitives)
            {
                primObj.FlashMacroPrimitiveForGerberPlot(stateMachine, VariableArray, macroGraphicsObj, workingBrush, boundingRectFInScreenCoord.X, boundingRectFInScreenCoord.Y);
            }

            // now we place the newly drawn macroBitmap on the screen, first create an int based rectangle
            // which describes the screen location in Gerber coord onto which we place the macro bitmap
            // x1 and y1 are already converted to Gerber coords and are the effective (0,0) point of the macro
            // however they are not the (0,0) ppoint of the place we draw. The aperture origin does not have to 
            // be in the macro Bitmap

            // we make this obvious
            int effectiveDrawPoint_X0 = (int)(x1 + boundingRectFInScreenCoord.X);
            int effectiveDrawPoint_Y0 = (int)(y1 + boundingRectFInScreenCoord.Y);
            int effectiveWidth = boundingWidth; // this does not change 
            int effectiveHeight = boundingHeight; // this does not change 

            // create a defining rectangle
            Rectangle boundingRect = new Rectangle((int)effectiveDrawPoint_X0, effectiveDrawPoint_Y0, effectiveWidth, effectiveHeight);
            // make the parts that did not get a macro primitive drawn on them transparent
            macroBitmap.MakeTransparent(ApplicationColorManager.DEFAULT_MACRO_TRANSPARENT_COLOR);
            // overlay the macro bitmap on our main gerber display
            graphicsObj.DrawImage(macroBitmap, boundingRect);

            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flash an aperture for a Circle on a GCode Plot
        /// </summary>
        /// <param name="isoPlotBuilder">the builder opbject</param>
        /// <param name="stateMachine">the statemachine</param>
        /// <param name="xyComp">the xy compensation factor</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <returns>z success, nz fail</returns>
        public override void FlashApertureForGCodePlot(IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int xyComp)
        {
            List<int> macroBuilderIDs = new List<int>();

            if (isoPlotBuilder == null) return;
            if (stateMachine == null) return;

            // get the macro AM code object for this aperture
            GerberLine_AMCode workingMacro = GetMacroObject(stateMachine);
            if (workingMacro == null)
            {
                throw new Exception("No macro definition found for name " + MacroName);
            }

            //// we run through our primitive list in order and and draw for GCode 
            foreach (GerberMacroPrimitive_Base primObj in workingMacro.MacroPrimitives)
            {
                int builderID=primObj.FlashMacroPrimitiveForGCodePlot(this.VariableArray, macroBuilderIDs, isoPlotBuilder, stateMachine, x1, y1, xyComp);
                if (builderID <= 0) continue;
                macroBuilderIDs.Add(builderID);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the width a pen should be for a specific angle.  The width
        /// is returned in pixel units which means all dimensions are scaled up
        /// by 
        /// </summary>
        /// <param name="stateMachine">the state machine with gerber file details</param>
        /// <param name="workingAngle">the angle the aperture will move at in degrees</param>
        public override int GetPenWidthForAngle(GerberFileStateMachine stateMachine, int workingAngle)
        {
            throw new Exception("Not available in this aperture type");
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
        public override void FixupLineEndpointsForGerberPlot(GerberFileStateMachine stateMachine, Graphics graphicsObj, Brush workingBrush, Pen workingPen, float x1, float y1, float x2, float y2)
        {
            throw new Exception("Not available in this aperture type");
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
        public override void FixupLineEndpointsForGCodePlot(IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int x2, int y2, int radius, int xyComp)
        {
            throw new Exception("Not available in this aperture type");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Populate the object from an Aperture Definition prarameter
        /// </summary>
        /// <param name="adParamBlock">The AD string param block, no param, or block
        /// delimiters. Just the AD block</param>
        /// <param name="nextStartPos">the next start position after all of the ADX leading text</param>
        /// <returns>z success, nz fail</returns>
        public override int PopulateFromParameter(string adParamBlock, int nextStartPos)
        {

            if (adParamBlock == null) return 100;
            if (adParamBlock.StartsWith(GerberFile.RS274_AD_CMD) == false) return 200;


            if ((nextStartPos < 0) || (nextStartPos >= adParamBlock.Length))
            {
                // this is perfectly ok for a macro
                return 0;
            }

            // we get the data past the comma into a string
            string variableStr = adParamBlock.Substring(nextStartPos);

            if (variableStr == null) return -1;
            if (variableStr.Length == 0) return -1;

            // set this now
            DefinitionBlock = variableStr;
            // set the variables
            return SetApertureVariablesFromString(variableStr);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the aperture variables from a string. 
        /// </summary>
        /// <param name="variableStr">the variable string</param>
        /// <returns>z succes, nz fail</returns>
        public int SetApertureVariablesFromString( string variableStr)
        {
            char[] splitArr = new char[] { 'x', 'X' };

            if (variableStr == null) return -1;
            if (variableStr.Length == 0) return -1;

            // now split it on the X delimiters as per spec
            string[] outParams = variableStr.Split(splitArr, StringSplitOptions.RemoveEmptyEntries);

            // populate the variable arrays
            int varIndex = 1;
            foreach(string paramStr in outParams)
            {
                VariableArray[varIndex] = new GerberMacroVariable(varIndex, paramStr);
                varIndex++;
                if(varIndex>GerberMacroVariableArray.MAX_MACRO_VARIABLES)
                {
                    throw new Exception("Maximum number of Macro Variables Exceeded");
                }
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the aperture for a new flash
        /// </summary>
        public override void ResetForFlash()
        {
            // macros need to do this, The macro itself can mess with the variables
            SetApertureVariablesFromString(DefinitionBlock);
        }
    }
}

