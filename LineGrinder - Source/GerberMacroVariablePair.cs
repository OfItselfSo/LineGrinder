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
    /// A class to contain and handle a pair of Gerber Macro Variables
    /// </summary>
    public class GerberMacroVariablePair : OISObjBase
    {
        // these are the two variables of our pair
        private GerberMacroVariable xVar = new GerberMacroVariable();
        private GerberMacroVariable yVar = new GerberMacroVariable();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xVarStringIn">the xVar value</param>
        /// <param name="yVarStringIn">the yVar value</param>
        public GerberMacroVariablePair(string xVarStringIn, string yVarStringIn)
        {
            xVar = new GerberMacroVariable(xVarStringIn);
            yVar = new GerberMacroVariable(yVarStringIn);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the xVar. No Set accesor, is set by the 
        /// constructor
        /// </summary>
        public GerberMacroVariable XVar
        {
            get
            {
                if (xVar == null) xVar = new GerberMacroVariable();
                return xVar;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the yVar. No Set accesor, is set by the 
        /// constructor
        /// </summary>
        public GerberMacroVariable YVar
        {
            get
            {
                if (yVar == null) yVar = new GerberMacroVariable();
                return yVar;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Processes the variable pair into a numeric value. It may need the numbered
        /// variables to do this
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public PointF ProcessVariablePairToFloat(GerberMacroVariableArray varArray)
        {
            if (varArray == null) return new PointF(0,0);

            // these objects know how to translate themselves
            float xVarlAsFloat = xVar.ProcessVariableStringToFloat(varArray);
            float yVarlAsFloat = yVar.ProcessVariableStringToFloat(varArray);

            return new PointF(xVarlAsFloat, yVarlAsFloat);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Replaces the ToString()
        /// </summary>
        public override string ToString()
        {
            // just return the variable string 
            return "(" + XVar.ToString() + "," + YVar.ToString() + ")";
        }
    }
}

