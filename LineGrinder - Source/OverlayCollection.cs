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
    /// A class to contain and manipulate a collection of Overlay objects 
    /// </summary>
    public class OverlayCollection : OISObjBase
    {
        public const int EMPTY_OVERLAY_ID = 0;
        // this id is incremented and assigned to every gerber object we draw
        private int currentBuilderOverlayID = EMPTY_OVERLAY_ID;

        // this is a list of all Overlay for the isolation Plot
        private Dictionary<int, Overlay> overlayObjects = new Dictionary<int, Overlay>();
        private Dictionary<string, int> idToIdentKeyMap = new Dictionary<string, int>();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public OverlayCollection()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets next builderOverlay object id. These are assigned sequentially
        /// </summary>
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
        /// Accepts a single builderIDTag. Checks to see if we have an overlay matching that.
        /// If we do, we return it. If not, we create one and return that.
        /// </summary>
        /// <param name="tagIn">the tag to look for</param>
        /// <returns>the overlay id of the existing or new overlay</returns>
        public int CreateNewOrReturnExistingFromSingleUsage(int tagIn)
        {
            Overlay outOverlay = null;

            // create the ident key an overlay with just this single usage would have
            string identKey = Overlay.CalcIndentKeyFromSingleUsage(tagIn);
            // get the overlay
            outOverlay = GetOverlayByIdentKey(identKey);
            // do we have one? if so use that
            if (outOverlay != null) return outOverlay.OverlayID;
            // we do not. We have to create one
            outOverlay = new Overlay(NextBuilderOverlayID, tagIn);
            // now add it
            this.Add(outOverlay);
            // return its ID
            return outOverlay.OverlayID;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts a single tag and an overlay ID. Checks to see if we have an 
        /// overlay matching that existing one plus the new tag.
        /// If we do, we return it. If not, we create one and return that.
        /// </summary>
        /// <param name="newUsage">the usage to add</param>
        /// <param name="existingOverlayIDIn">the overlay ID </param>
        /// <returns>the overlay id of the existing or new overlay</returns>
        public int CreateNewOrReturnExistingFromSingleUsageAndExistingOverlay(int newTag, int existingOverlayIDIn)
        {
            Overlay outOverlay = null;
            Overlay existingOverlayObj = null;

            // make sure we are dealing with a clean ID
            int existingOverlayID = OverlayCollection.CleanFlagsFromOverlayID(existingOverlayIDIn);

            if (existingOverlayID == Overlay.DEFAULT_OVERLAY_ID)
            {
                throw new Exception("Bad overlay ID presented to collection");
            }
            // get the existing overlay object
            existingOverlayObj = this[existingOverlayID];
            if(existingOverlayObj==null)
            {
                // this should not happen, the caller should always have a valid overlay id
                throw new Exception("Unknown base overlay ID presented to collection");
            }
            // does this overlay already contain the existing usage. In theory this should 
            // never happen. But in reality sometimes the circle and arc algorythms write to the 
            // same isopoint twice
            bool retBool = existingOverlayObj.DoesOverlayContainTag(newTag);
            if(retBool == true)
            {
                // yes it does, no need to create another one with a duplicate usage added, just use this one
                return existingOverlayObj.OverlayID;
            }

            // we need to see if we already have on based on the existing one with the new usage added
            string identKey = existingOverlayObj.CalcIndentKeyPlusUsage(newTag);
            // get the overlay the same as the existing one but with the new usage
            outOverlay = GetOverlayByIdentKey(identKey);
            // do we have one? if so use that
            if (outOverlay != null) return outOverlay.OverlayID;
            // we do not. We have to create one
            outOverlay = existingOverlayObj.BuildNewOverlayWithAddedUsage(NextBuilderOverlayID, newTag);
            // now add it
            this.Add(outOverlay);
            // return its ID
            return outOverlay.OverlayID;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts a single tag and an overlay ID. Checks to see if we have an overlay
        /// matching the existing overlay without the tag.
        /// If we do, we return it. If not, we create one and return that.
        /// </summary>
        /// <param name="tagIn">the tag to look for</param>
        /// <param name="existingOverlayIDIn">the overlay ID </param>
        /// <returns>the overlay id of the existing or new overlay</returns>
        public int CreateNewOrReturnExistingRemovingSingleTagFromExistingOverlay(int tagIn, int existingOverlayIDIn)
        {
            Overlay outOverlay = null;
            Overlay existingOverlayObj = null;
            string identKey = "";

            // make sure we are dealing with a clean ID
            int existingOverlayID = OverlayCollection.CleanFlagsFromOverlayID(existingOverlayIDIn);

            if (existingOverlayID == Overlay.DEFAULT_OVERLAY_ID)
            {
                throw new Exception("Bad overlay ID presented to collection");
            }
            // get the existing overlay object
            existingOverlayObj = this[existingOverlayID];
            if (existingOverlayObj == null)
            {
                // this should not happen, the caller should always have a valid overlay id
                throw new Exception("Unknown base overlay ID presented to collection");
            }

            // does this overlay already not contain the existing tag. In theory this should 
            // never happen. But in reality sometimes the circle and arc algorythms write to the 
            // same isopoint twice
            bool retBool = existingOverlayObj.DoesOverlayContainIsoPlotID(tagIn);
            if (retBool == false)
            {
                // no it does not, no need to create another one with a duplicate usage added, just use this one
                return existingOverlayObj.OverlayID;
            }

            identKey = existingOverlayObj.CalcIndentKeyMinusTag(tagIn);
            // get the overlay
            outOverlay = GetOverlayByIdentKey(identKey);
            // do we have one? if so use that
            if (outOverlay != null) return outOverlay.OverlayID;
            // we do not. We have to create one
            outOverlay = existingOverlayObj.BuildNewOverlayWithMissingTag(NextBuilderOverlayID, tagIn);
            // now add it
            this.Add(outOverlay);
            // return its ID
            return outOverlay.OverlayID;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts a single builder ID and an overlay ID. Checks to see if we have an overlay
        /// matching the existing overlay without that ID.
        /// If we do, we return it. If not, we create one and return that.
        /// </summary>
        /// <param name="idIn">the id to look for</param>
        /// <param name="existingOverlayIDIn">the overlay ID </param>
        /// <returns>the overlay id of the existing or id of the new overlay</returns>
        public int CreateNewOrReturnExistingRemovingSingleIDFromExistingOverlay(int idIn, int existingOverlayIDIn)
        {
            Overlay outOverlay = null;
            Overlay existingOverlayObj = null;
            string identKey = "";

            // make sure we are dealing with a clean ID
            int existingOverlayID = OverlayCollection.CleanFlagsFromOverlayID(existingOverlayIDIn);

            if (existingOverlayID == Overlay.DEFAULT_OVERLAY_ID)
            {
                throw new Exception("Bad overlay ID presented to collection");
            }
            // get the existing overlay object
            existingOverlayObj = this[existingOverlayID];
            if (existingOverlayObj == null)
            {
                // this should not happen, the caller should always have a valid overlay id
                throw new Exception("Unknown base overlay ID presented to collection");
            }

            // does this overlay already not contain the existing id. This can happen
            bool retBool = existingOverlayObj.DoesOverlayContainIsoPlotID(idIn);
            if (retBool == false)
            {
                // no it does not, no need to create another one with a duplicate usage added, just use this one
                return existingOverlayObj.OverlayID;
            }

            identKey = existingOverlayObj.CalcIndentKeyMinusID(idIn);
            // get the overlay
            outOverlay = GetOverlayByIdentKey(identKey);
            // do we have one? if so use that
            if (outOverlay != null) return outOverlay.OverlayID;
            // we do not. We have to create one
            outOverlay = existingOverlayObj.BuildNewOverlayWithMissingID(NextBuilderOverlayID, idIn);
            // now add it
            this.Add(outOverlay);
            // return its ID
            return outOverlay.OverlayID;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if an overlay already contains a tag.
        /// </summary>
        /// <param name="overlayIDIn">the overlay to look for</param>
        /// <param name="tagIn">the tag to check</param>
        /// <returns>
        /// True - the overlay object represented by the id contains the usage, false - it does not
        /// </returns>
        public bool DoesOverlayContainTag(int overlayIDIn, int tagIn)
        {
            Overlay overlayObj = null;

            // make sure we are dealing with a clean ID
            int overlayID = OverlayCollection.CleanFlagsFromOverlayID(overlayIDIn);

            if (overlayID == Overlay.DEFAULT_OVERLAY_ID)
            {
                throw new Exception("Bad overlay ID presented to collection");
            }
            // get the existing overlay object
            overlayObj = this[overlayID];
            // does it even exist?
            if (overlayObj == null) return false;
            // it exists, lets ask the overlay itself
            return overlayObj.DoesOverlayContainTag(tagIn);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Searches the list of Overlay objects and finds one matching
        /// the current ident string. This uses the secondary dictionary index.
        /// </summary>
        /// <param name="identKeyIn">the ident key to look for</param>
        /// <returns>the overlay object or null for fail</returns>
        public Overlay GetOverlayByIdentKey(string identKeyIn)
        {
            int tmpID = int.MinValue;
            // note this  can be the default ident key signalling an empty object
            // we can find this ok in the idToIdentKeyMap
            if (identKeyIn == null) return null;

            // look up the id in our ident key to id index
            bool retBool = idToIdentKeyMap.TryGetValue(identKeyIn, out tmpID);
            // did we find it?
            if (retBool == false)  
            {
                // no, we did not
                return null;
            }

            // yes we did find it - just use this code
            // note: zero is ok here, just means default overlay
            return this[tmpID];
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// An indexer for the overlay collection. NOTE: this will only use the 
        /// lower 24 bits when looking for a key.
        /// </summary>
        /// <param name="overlayIDIn">the overlayID to find</param>
        public Overlay this[int overlayIDIn]
        {
            get
            {
                // make sure we are dealing with a clean ID
                int overlayID = OverlayCollection.CleanFlagsFromOverlayID(overlayIDIn);

                Overlay tmpObj = null;
                overlayObjects.TryGetValue(overlayID,out tmpObj);
                return tmpObj;
            }
         }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds an overlay object to our collection
        /// </summary>
        /// <param name="item">the overlay object to add</param>
        public void Add(Overlay item)
        {
            if (item == null) return;
            if (item.OverlayID == Overlay.DEFAULT_OVERLAY_ID)
            {
                throw new Exception("Bad overlay ID presented to collection");
            }
            // does it already exist? - if so leave
            if (overlayObjects.Keys.Contains(item.OverlayID) == true)
            {
                return;
            }
            // add it now
            overlayObjects.Add(item.OverlayID, item);
            // also update this secondary index for speed
            idToIdentKeyMap.Add(item.IdentKey, OverlayCollection.CleanFlagsFromOverlayID(item.OverlayID));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overlay IDs can have flags attached to them during post processing. This
        /// cleans them off and returns just the overlayID
        /// </summary>
        /// <param name="overlayIDIn">the overlay id</param>
        /// <returns>the overlay id with no flags</returns>
        public static int CleanFlagsFromOverlayID(int overlayIDIn)
        {
            return overlayIDIn & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the count of the objects in the overlay collection
        /// </summary>
        public int Count
        {
            get { return overlayObjects.Count; }
        }

     }
}

