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
    /// A class to encapsulate a collection of GerberLine_ADCode objects
    /// </summary>
    /// <history>
    ///    07 Jul 10  Cynic - Started
    /// </history>
    public class GerberApertureCollection : OISObjBase, ICollection<GerberLine_ADCode> 
    {
        private List<GerberLine_ADCode> apertureCollection = new List<GerberLine_ADCode>();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public GerberApertureCollection()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the collection
        /// </summary>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public void Reset()
        {
            DisposeAllPens();
            (ApertureCollection as ICollection<GerberLine_ADCode>).Clear();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Finds an aperature by D Code, we never return null
        /// </summary>
        /// <param name="idValue">the D code value we are looking for</param>
        /// <history>
        ///    13 Jul 10  Cynic - Started
        /// </history>
        public GerberLine_ADCode GetApertureByID(int idValue)
        {
            foreach (GerberLine_ADCode adCode in ApertureCollection)
            {
                if (adCode.DNumber == idValue) return adCode;
            }
            // we did not find it, return a default aperture
            LogMessage("Unknown aperture id=" + idValue.ToString() + " returning default aperture");
            return new GerberLine_ADCode("", "", 0);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the maximum aperture dimensions for all apertures in any direction
        /// from the center point. This assumes the aperture is centered on the point
        /// </summary>
        /// <param name="idValue">the D code value we are looking for</param>
        /// <history>
        ///    08 Aug 10  Cynic - Started
        /// </history>
        public float GetMaxApertureIncrementalDimension()
        {
            return GetMaxApertureDimension() / 2;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the maximum aperture dimension for all apertures. This is 
        /// essentially the biggest possible "extra bit" added onto a line
        /// because of the use of the apertures
        /// </summary>
        /// <param name="idValue">the D code value we are looking for</param>
        /// <history>
        ///    10 Sep 10  Cynic - Started
        /// </history>
        public float GetMaxApertureDimension()
        {
            float maxApertureDimension = 0;
            foreach (GerberLine_ADCode adCode in ApertureCollection)
            {
                if (maxApertureDimension < adCode.GetApertureDimension()) maxApertureDimension = adCode.GetApertureDimension();
            }
            return maxApertureDimension;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Dispose of all pens
        /// </summary>
        /// <history>
        ///    13 Jul 10  Cynic - Started
        /// </history>
        public void DisposeAllPens()
        {
            foreach (GerberLine_ADCode adCode in ApertureCollection)
            {
                adCode.DisposeAllPens();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Gerber aperature collection. Will never set or get a null value.
        /// </summary>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public List<GerberLine_ADCode> ApertureCollection
        {
            get
            {
                if (apertureCollection == null) apertureCollection = new List<GerberLine_ADCode>();
                return apertureCollection;
            }
            set
            {
                apertureCollection = value;
                if (apertureCollection == null) apertureCollection = new List<GerberLine_ADCode>();
            }
        }

        // ####################################################################
        // ##### ICollection<GerberLine_ADCode> Code
        // ####################################################################
        #region ICollection<GerberLine_ADCode> Members

        void ICollection<GerberLine_ADCode>.Add(GerberLine_ADCode item)
        {
            ApertureCollection.Add(item);
        }

        void ICollection<GerberLine_ADCode>.Clear()
        {
            ApertureCollection.Clear();
        }

        bool ICollection<GerberLine_ADCode>.Contains(GerberLine_ADCode item)
        {
            return ApertureCollection.Contains(item);
        }

        void ICollection<GerberLine_ADCode>.CopyTo(GerberLine_ADCode[] array, int arrayIndex)
        {
            ApertureCollection.CopyTo(array, arrayIndex);
        }

        int ICollection<GerberLine_ADCode>.Count
        {
            get 
            {
                return ApertureCollection.Count; 
            }
        }

        bool ICollection<GerberLine_ADCode>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection<GerberLine_ADCode>.Remove(GerberLine_ADCode item)
        {
            return ApertureCollection.Remove(item);
        }

        #endregion

        // ####################################################################
        // ##### IEnumerable<GerberLine_ADCode> Members
        // ####################################################################
        #region IEnumerable<GerberLine_ADCode> Members

        IEnumerator<GerberLine_ADCode> IEnumerable<GerberLine_ADCode>.GetEnumerator()
        {
            return ApertureCollection.GetEnumerator();
        }

        #endregion

        // ####################################################################
        // ##### IEnumerable Members
        // ####################################################################
        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ApertureCollection.GetEnumerator();
        }

        #endregion
    }
}
