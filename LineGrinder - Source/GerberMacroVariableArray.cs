using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// A class to encapsulate an array of GerberMacroVariable objects
    /// </summary>
    public class GerberMacroVariableArray : OISObjBase 
    {
        public const int MAX_VARIABLE_ITERATION_COUNT = 10;
        public const int MAX_MACRO_VARIABLES = 25;
        // this is the array that holds our variables. 
        private GerberMacroVariable[] variableArray = new GerberMacroVariable[MAX_MACRO_VARIABLES];

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberMacroVariableArray()
        {
            Reset();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Reset/Init the array
        /// </summary>
        private void Reset()
        {
            for(int i =0; i < MAX_MACRO_VARIABLES; i++)
            {
                variableArray[i] = new GerberMacroVariable(i);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a count of the array
        /// </summary>
        public int Count()
        {
            return variableArray.Count();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Gerber aperature collection. Will never set or get a null value.
        /// </summary>
        public GerberMacroVariable[] VariableArray
        {
            get
            {
                if (variableArray == null) variableArray = new GerberMacroVariable[MAX_MACRO_VARIABLES];
                return variableArray;
            }
            set
            {
                variableArray = value;
                if (variableArray == null) variableArray = new GerberMacroVariable[MAX_MACRO_VARIABLES];
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// An indexer for the array, will never get or set a null variable
        /// </summary>
        public GerberMacroVariable this[int index]
        {
            get
            {
                if (VariableArray[index] == null) VariableArray[index] = new GerberMacroVariable(index);
                return VariableArray[index];
            }
            set
            {
                VariableArray[index] = value;
                if (VariableArray[index] == null) VariableArray[index] = new GerberMacroVariable(index);
            }
        }
    }
}

