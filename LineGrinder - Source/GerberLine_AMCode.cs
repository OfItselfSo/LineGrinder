using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
    /// A class to encapsulate a gerber AM Code (aperture macro)
    /// </summary>
    public class GerberLine_AMCode : GerberLine
    {
        private GerberCommandSourceLines srcLineObj = null;
        private string macroName = "";

        private List<GerberMacroPrimitive_Base> macroPrimitives = new List<GerberMacroPrimitive_Base>();

        public const string AM_VARIABLE_PRIMITIVE = "$";// $ means we are dealing with a variable redefine
        public const string AM_COMMENT_PRIMITIVE = "0"; // 1 is a comment primitive
        public const string AM_CIRCLE_PRIMITIVE = "1";  // 1 is a circle primitive
        public const string AM_VLINE_PRIMITIVE = "20";  // 1 is a vector line primitive
        public const string AM_CLINE_PRIMITIVE = "21";  // 1 is a center line primitive
        public const string AM_OUTLINE_PRIMITIVE = "4";   // 1 is a outline primitive
        public const string AM_POLYGON_PRIMITIVE = "5"; // 1 is a polygon primitive
        public const string AM_MOIRE_PRIMITIVE = "6";   // 1 is a moire primitive
        public const string AM_THERMAL_PRIMITIVE = "7"; // 1 is a thermal primitive

        public const string AM_PRIMITIVE_DELIM = ",";   // delimiter between components of macro primitive
        public const char AM_PRIMITIVE_DELIM_CHAR = ','; // delimiter between components of macro primitive

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        public GerberLine_AMCode(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
            // should never happen
            throw new NotImplementedException("Cannot use standard constructor for Aperture Macros");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor, this one processes a GerberCommandSourceLines object since
        /// AM commands can be multi-line
        /// </summary>
        public GerberLine_AMCode(GerberCommandSourceLines srcLineObjIn)
            : base(srcLineObjIn.GetFirstLine(), srcLineObjIn.GetFirstLine().Trim(GerberFile.RS274_CMD_DELIMITER_CHAR), srcLineObjIn.LineNumber)
        {
            // set our source line object now
            srcLineObj = srcLineObjIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the source line object. There is no set accessor - this is done in the 
        /// constructor
        /// </summary>
        public GerberCommandSourceLines SrcLineObj
        {
            get
            {
                return srcLineObj;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the macro primitives for this macro. The set is done during init.
        /// Will never return null;
        /// </summary>
        public List<GerberMacroPrimitive_Base> MacroPrimitives
        {
            get
            {
                if (macroPrimitives == null) macroPrimitives = new List<GerberMacroPrimitive_Base>();
                return macroPrimitives;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the macro name. There is no set accessor - this is done in the 
        /// during population. Will never return null, will return empty.
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
        /// Calculates the maximum and minimum coords for a macro. 
        /// </summary>
        /// <param name="ptLL">lower left point</param>
        /// <param name="ptUR">upper right point</param>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>true the coords are set with values, false they are not</returns>
        public bool GetMaxMinXAndYValuesForMacro(GerberMacroVariableArray varArray, out PointF ptUR, out PointF ptLL)
        {
            // collect the largest, smallest here
            float largestX = float.MinValue;
            float smallestX = float.MaxValue;
            float largestY = float.MinValue;
            float smallestY = float.MaxValue;

            // loop through every primitive. These will also cope with rotations
            foreach (GerberMacroPrimitive_Base primObj in macroPrimitives)
            {
                // variable prims do not draw and hence have no bounding box
                if ((primObj is GerberMacroPrimitive_Variable) == true) continue;

                PointF macroUR;
                PointF macroLL;
                // get the max and min from the primitive
                bool retBool = primObj.GetMaxMinXAndYValuesForPrimitive(varArray, out macroUR, out macroLL);
                if (retBool == false) continue;

                // now check the max min
                if (macroUR.X > largestX) largestX = macroUR.X;
                if (macroLL.X < smallestX) smallestX = macroLL.X;
                if (macroUR.Y > largestY) largestY = macroUR.Y;
                if (macroLL.Y < smallestY) smallestY = macroLL.Y;

                // check it this way too
                if (macroLL.X > largestX) largestX = macroLL.X;
                if (macroUR.X < smallestX) smallestX = macroUR.X;
                if (macroLL.Y > largestY) largestY = macroLL.Y;
                if (macroUR.Y < smallestY) smallestY = macroUR.Y;
            }

            // return the points
            ptLL = new PointF(smallestX, smallestY);
            ptUR = new PointF(largestX, largestY);
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the largest bounding rectangle for the primitives in the macro.
        /// NOTE this does NOT necessarily include the macro origin. It is the box which
        /// will contain every primitive, the origin can be outside of this.
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        /// <returns>a retangle with the bounds</returns>
        public RectangleF GetMacroPrimitivesBoundingBox(GerberMacroVariableArray varArray)
        {
            float maxX;
            float minX;
            float maxY;
            float minY;

            // collect the largest, smallest here
            float largestX = float.MinValue;
            float smallestX = float.MaxValue;
            float largestY = float.MinValue;
            float smallestY = float.MaxValue; ;

            // loop through every primitive. These will also cope with rotations
            foreach (GerberMacroPrimitive_Base primObj in macroPrimitives)
            {
                // variable prims do not draw and hence have no bounding box
                if ((primObj is GerberMacroPrimitive_Variable) == true) continue;

                // get the max and min from the primitive
                RectangleF rectF = primObj.GetMacroPrimitiveBoundingBox(varArray);

                // check a few things. We need to know if this ever happens
                if (rectF.Width < 0) throw new Exception("Negative Width of " + rectF.Width + " on macro primitive " + primObj.PrimitiveString);
                if (rectF.Height < 0) throw new Exception("Negative Height of " + rectF.Height + " on macro primitive " + primObj.PrimitiveString);

                // we write this out to make it real clear
                minX = rectF.X;
                minY = rectF.Y;
                maxX = rectF.X + rectF.Width;
                maxY = rectF.Y + rectF.Height;

                // now check the max min
                if (maxX > largestX) largestX = maxX;
                if (minX < smallestX) smallestX = minX;
                if (maxY > largestY) largestY = maxY;
                if (minY < smallestY) smallestY = minY;

                // check it this way too
                if (minX > largestX) largestX = minX;
                if (maxX < smallestX) smallestX = maxX;
                if (minY > largestY) largestY = minY;
                if (maxY < smallestY) smallestY = maxY;
            }

            float boundingWidth = largestX - smallestX;
            float boundingHeight = largestY - smallestY;

            // check a few things. We need to know if this ever happens
            if (boundingWidth < 0) throw new Exception("Negative Width of " + boundingWidth + " on macro " + this.MacroName);
            if (boundingHeight < 0) throw new Exception("Negative Height of " + boundingHeight + " on macro " + this.MacroName);

            // return the rectangle
            return new RectangleF(smallestX, smallestY, boundingWidth, boundingHeight);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets all lines in the command concatenated as one string with leading/trailing 
        /// spaces and leading/trailing command delimiters removed. The block delimiters are still
        /// in place.
        /// </summary><returns>the full command string never returns null</returns>
        public string GetFullCommandString()
        {
            string outString = "";
            char[] cmdCharsToTrim = { GerberFile.RS274_CMD_DELIMITER_CHAR, ' ' };

            // loop through every command object we have
            foreach (string srcLine in srcLineObj.CommandList)
            {
                // concatenate.
                outString = outString + srcLine.Trim(cmdCharsToTrim);
            }

            return outString;
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
            // should never happen
            throw new NotImplementedException("Cannot use standard ParseLine for Aperture Macros");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Processes the SrcLineObj commands. This is a replacement for ParseLine for multiline
        /// Gerber Commands. It expects the command object to have been set
        /// </summary>
        /// <returns>z success, nz fail</returns>
        public int ProcessCommand()
        {
            string[] blockArray = null;
            char[] blockSplitChars = { GerberFile.RS274_BLOCK_TERMINATOR_CHAR };
            string fullCommandStr;
            int retInt = 0;

            if (SrcLineObj==null)
            {
                LogMessage("ProcessCommand(AM), SrcLineObj==null");
                return 10;
            }

            // the Aperture macro can be on multiple lines or it can be only on one. We have to
            // split out each macro primitive. We do this by concatenating all lines then splitting on the 
            // block terminator char
            fullCommandStr = GetFullCommandString();

            // now split the parts of the command back up
            blockArray = fullCommandStr.Split(blockSplitChars, StringSplitOptions.RemoveEmptyEntries);
            // this should never be null
            if (blockArray == null)
            {
                LogMessage("ProcessCommand(AM), blockArray == null");
                return -20;
            }
            if (blockArray.Length <= 0)
            {
                // should never happen
                LogMessage("ProcessCommand(AM), blockArray.Length <= 0");
                return -30;
            }

            // now get the name of the Macro
            if (blockArray[0].StartsWith(GerberFile.RS274_AM_CMD) == false)
            {
                // should never happen
                LogMessage("ProcessCommand(AM), command does not start with aperture macro AM cmd");
                return -40;
            }
            string nameStr = blockArray[0].Remove(0, GerberFile.RS274_AM_CMD.Length);
            if (nameStr.Length ==0)
            {
                // should never happen
                LogMessage("ProcessCommand(AM), No macro name supplied by the command");
                return -50;
            }
            // set it
            macroName = nameStr;

            // now process the macro primitives
            for(int i=1; i< blockArray.Length; i++)
            {
                GerberMacroPrimitive_Base primObj = null;

                string primStr = blockArray[i];
                // figure out what type of primitive we have
                if (primStr.StartsWith(AM_VARIABLE_PRIMITIVE) == true)
                {
                    // variable pseudo primitive
                    primObj = new GerberMacroPrimitive_Variable();
                }
                else if (primStr.StartsWith(AM_COMMENT_PRIMITIVE) == true)
                {
                    // comment primitive, we just ignore these. No point in adding them
                    continue;
                }
                else if (primStr.StartsWith(AM_CIRCLE_PRIMITIVE + AM_PRIMITIVE_DELIM) == true)
                {
                    // circle primitive
                    primObj = new GerberMacroPrimitive_Circle();
                }
                else if (primStr.StartsWith(AM_OUTLINE_PRIMITIVE + AM_PRIMITIVE_DELIM) == true)
                {
                    // outline primitive
                    primObj = new GerberMacroPrimitive_Outline();
                }
                else if (primStr.StartsWith(AM_POLYGON_PRIMITIVE + AM_PRIMITIVE_DELIM) == true)
                {
                    // polygon primitive
                    primObj = new GerberMacroPrimitive_Polygon();
                }
                else if (primStr.StartsWith(AM_VLINE_PRIMITIVE + AM_PRIMITIVE_DELIM) == true)
                {
                    // vector line primitive
                    primObj = new GerberMacroPrimitive_VectorLine();
                }
                else if (primStr.StartsWith(AM_CLINE_PRIMITIVE + AM_PRIMITIVE_DELIM) == true)
                {
                    // center line primitive
                    primObj = new GerberMacroPrimitive_CenterLine();
                }
                else
                {
                    // should never happen
                    LogMessage("ProcessCommand(AM), Unknown primitive macro of:"+ primStr);
                    return -60;
                }
                retInt = primObj.ParsePrimitiveString(primStr);
                if (retInt != 0)
                {
                    // should never happen
                    LogMessage("ProcessCommand(AM), Error " + retInt.ToString() + " found when processing macro primitive");
                    return -70;
                }
                // we are good, add this to the collection of primitives for this macro
                MacroPrimitives.Add(primObj);
            }

            // at this point we have all of the primitives added to the macro

            return 0;
        }

    }
}

