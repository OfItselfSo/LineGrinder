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
    /// A class to encapsulate a collection of GerberLine_AMCode objects
    /// </summary>
    public class GerberMacroCollection : OISObjBase, ICollection<GerberLine_AMCode> 
    {
        private List<GerberLine_AMCode> macroList = new List<GerberLine_AMCode>();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberMacroCollection()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the collection
        /// </summary>
        public void Reset()
        {
            (MacroList as ICollection<GerberLine_AMCode>).Clear();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a count of the collection
        /// </summary>
        public int Count()
        {
            return MacroList.Count();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Finds an aperature by Name, we will return null if not found
        /// </summary>
        /// <param name="macroName">the name we are looking for</param>
        public GerberLine_AMCode GetMacroByName(string macroName)
        {
            foreach (GerberLine_AMCode amCode in MacroList)
            {
                if (amCode.MacroName == macroName) return amCode;
            }
            // we did not find it, return a default macro
            LogMessage("Unknown macro name=" + macroName + " returning null");
            return null;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Gerber aperature collection. Will never set or get a null value.
        /// </summary>
        public List<GerberLine_AMCode> MacroList
        {
            get
            {
                if (macroList == null) macroList = new List<GerberLine_AMCode>();
                return macroList;
            }
            set
            {
                macroList = value;
                if (macroList == null) macroList = new List<GerberLine_AMCode>();
            }
        }

        // ####################################################################
        // ##### ICollection<GerberLine_AMCode> Code
        // ####################################################################
        #region ICollection<GerberLine_AMCode> Members

        void ICollection<GerberLine_AMCode>.Add(GerberLine_AMCode item)
        {
            MacroList.Add(item);
        }

        void ICollection<GerberLine_AMCode>.Clear()
        {
            MacroList.Clear();
        }

        bool ICollection<GerberLine_AMCode>.Contains(GerberLine_AMCode item)
        {
            return MacroList.Contains(item);
        }

        void ICollection<GerberLine_AMCode>.CopyTo(GerberLine_AMCode[] array, int arrayIndex)
        {
            MacroList.CopyTo(array, arrayIndex);
        }

        int ICollection<GerberLine_AMCode>.Count
        {
            get 
            {
                return MacroList.Count; 
            }
        }

        bool ICollection<GerberLine_AMCode>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection<GerberLine_AMCode>.Remove(GerberLine_AMCode item)
        {
            return MacroList.Remove(item);
        }

        #endregion

        // ####################################################################
        // ##### IEnumerable<GerberLine_AMCode> Members
        // ####################################################################
        #region IEnumerable<GerberLine_AMCode> Members

        IEnumerator<GerberLine_AMCode> IEnumerable<GerberLine_AMCode>.GetEnumerator()
        {
            return MacroList.GetEnumerator();
        }

        #endregion

        // ####################################################################
        // ##### IEnumerable Members
        // ####################################################################
        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return MacroList.GetEnumerator();
        }

        #endregion
    }
}

