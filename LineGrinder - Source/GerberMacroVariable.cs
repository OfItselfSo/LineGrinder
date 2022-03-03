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
    /// A class to contain and handle Gerber Macro Variables
    /// </summary>
    public class GerberMacroVariable : OISObjBase
    {

        int variableNumber = 0;
        // this is the definitition of the string for the variable
        public const string DEFAULT_VARIABLE_STRING = "0";
        private string variableString = DEFAULT_VARIABLE_STRING;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor for an un-numbered variable
        /// </summary>
        public GerberMacroVariable()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor, used if we have a variable number only
        /// </summary>
        /// <param name="variableNumberIn">the variable number</param>
        public GerberMacroVariable(int variableNumberIn)
        {
            // set this
            variableNumber = variableNumberIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor, used if we have an un-numbered variable and a variable string
        /// </summary>
        /// <param name="variableStringIn">the string to calc the variable</param>
        public GerberMacroVariable(string variableStringIn)
        {
            // set this
            VariableString = variableStringIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor, used if we have a variable number and a variable string
        /// </summary>
        /// <param name="variableNumberIn">the variable number</param>
        /// <param name="variableStringIn">the string to calc the variable</param>
        public GerberMacroVariable(int variableNumberIn, string variableStringIn)
        {
            // set this
            variableNumber = variableNumberIn;
            VariableString = variableStringIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the variable number. No Set accesor, is set by the 
        /// constructor
        /// </summary>
        public int VariableNumber
        {
            get
            {
                if (variableNumber < 0) variableNumber = 0;
                return variableNumber;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the variable string, will never get null. If empty will use the 
        /// DEFAULT_VARIABLE_STRING
        /// </summary>
        public string VariableString
        {
            get
            {
                if (variableString == null) variableString = DEFAULT_VARIABLE_STRING;
                if (variableString == "") variableString = DEFAULT_VARIABLE_STRING;
                return variableString;
            }
            set
            {
                variableString = value;
                if (variableString == null) variableString = DEFAULT_VARIABLE_STRING;
                if (variableString == "") variableString = DEFAULT_VARIABLE_STRING;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Processes the variable string into a numeric value. It may need the numbered
        /// variables to do this
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public float ProcessVariableStringToFloat(GerberMacroVariableArray varArray)
        {
            if (varArray == null) return 0;

            string filledString = VariableString;
            // sanity checks
            if (filledString == null) variableString = DEFAULT_VARIABLE_STRING;
            if (filledString == "") variableString = DEFAULT_VARIABLE_STRING;

            // now iteratively replace all of the $vars in the string
            filledString = FillVariableString(varArray, filledString);
            // sanity checks
            if (filledString == null) variableString = DEFAULT_VARIABLE_STRING;
            if (filledString == "") variableString = DEFAULT_VARIABLE_STRING;

            // create a math parser object
            MathParser parser = new MathParser();
            // our parser uses '*' for multiplication
            filledString = filledString.Replace("x", "*");
            filledString = filledString.Replace("X", "*");

            // parse it
            return (float)parser.Parse(filledString);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills the variable string by removing all $vars and replacing them with values
        /// </summary>
        /// <param name="varArray">the array to get the numbered variables from</param>
        public string FillVariableString(GerberMacroVariableArray varArray, string stringToFill)
        {
            // we do not do this indefinitely 
            for (int iterationCount = 0; iterationCount < GerberMacroVariableArray.MAX_VARIABLE_ITERATION_COUNT; iterationCount++)
            {
                // do we need to process further?
                if (stringToFill.Contains("$") == false) return stringToFill;
                // yes we do, we could figure out which ones are in there but this is easier
                for(int index=0; index<varArray.Count(); index++)
                {
                    // do the replace for every variable
                    // for example $4 will get replaced with "(<variableString>)" in the stringToFill. If the VariableString also 
                    // contains further $vars we will pick them up on the next iteration. We put brackets around them because if there is a
                    // negative value in there our math parser cannot handle "3+-2" but it can handle "3+(-2)"
                    stringToFill = stringToFill.Replace("$" + index.ToString(), "(" + varArray.VariableArray[index].VariableString + ")");
                }
            }
            // if we get here we did not replace them all
            throw new Exception("Self referential macro variables found");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Replaces the ToString()
        /// </summary>
        public override string ToString()
        {
            // just return the variable string 
            return this.VariableString;
        }
    }
}

