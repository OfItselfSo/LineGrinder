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
    /// A class to contain and manipulate a collection of GCodeBuilderObjectOverlay objects
    /// pixel. 
    /// </summary>
    /// <history>
    ///    26 Jul 10  Cynic - Started
    /// </history>
    public class GCodeBuilderObjectOverlayCollection : OISObjBase
    {
        // this id is incremented and assigned to every gerber object we draw
        private int currentBuilderOverlayID = 0;

        // this is a list of all GCodeBuilderObjectOverlay for the isolation Plot
        private Dictionary<int, GCodeBuilderObjectOverlay> overlayObjects = new Dictionary<int, GCodeBuilderObjectOverlay>();
        private Dictionary<string, int> idToIdentKeyMap = new Dictionary<string, int>();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="builderOverlayIDIn">the builderOverlayID for this object</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public GCodeBuilderObjectOverlayCollection()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets next builderOverlay object id. These are assigned sequentially as we build
        /// distinct overlay objects for the isolation plot
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public int NextBuilderOverlayID
        {
            get
            {
                currentBuilderOverlayID++;
                // we cannot cope with greater than 2^24 objects.
                if (currentBuilderOverlayID >= 16777216)
                {
                    throw new NotImplementedException("NextGcOverlayID >= 2^24. Way too many intersections. This should not happen");
                }
                return currentBuilderOverlayID;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Searches the list of GCodeBuilderObjectOverlay objects and finds one matching
        /// the current ident string. This uses the secondary dictionary index.
        /// </summary>
        /// <param name="identKeyIn">the ident key to look for</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public GCodeBuilderObjectOverlay GetGCodeBuilderObjectOverlayByIdentKey(string identKeyIn)
        {
            int tmpID = int.MinValue;
            if (identKeyIn == null) return null;
            if (identKeyIn == GCodeBuilderObjectOverlay.DEFAULT_IDENT_KEY) return null;

            // look up the id in our ident key to id index
            idToIdentKeyMap.TryGetValue(identKeyIn, out tmpID);
            // did we find it?
            if (tmpID <= 0)
            {
                // no, we did not
                return null;
            }
            // yes we did find it - just use this code
            return this[tmpID];
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// An indexer for the overlay collection. NOTE: this will only use the 
        /// lower 24 bits when looking for a key.
        /// </summary>
        /// <param name="builderOverlayID">the builderOverlayID to return</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public GCodeBuilderObjectOverlay this[int builderOverlayID]
        {
            get
            {
                GCodeBuilderObjectOverlay tmpObj = null;
                overlayObjects.TryGetValue((builderOverlayID&0x00ffffff),out tmpObj);
                return tmpObj;
            }
         }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds an overlay object to our collection
        /// </summary>
        /// <param name="item">the overlay object to add</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public void Add(GCodeBuilderObjectOverlay item)
        {
            if (item == null) return;
            if (item.BuilderOverlayID == GCodeBuilderObjectOverlay.DEFAULT_BUILDEROVERLAY_ID)
            {
                throw new Exception("Bad overlay ID presented to collection");
            }
            // does it already exist? - if so leave
            if (overlayObjects.Keys.Contains(item.BuilderOverlayID) == true)
            {
                return;
            }
            // add it now
            overlayObjects.Add(item.BuilderOverlayID, item);
            // also update this secondary index for speed
            idToIdentKeyMap.Add(item.IdentKey,(item.BuilderOverlayID& 0x00ffffff));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the count of the objects in the overlay collection
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public int Count
        {
            get { return overlayObjects.Count; }
        }

     }
}
