using System;
using System.Collections;
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
    /// A class to contain the isoPlotObjects for the isolation plot. Essentially
    /// this class keeps track of which IsoPlotObjects are using a particular
    /// cell. 
    /// </summary>
    public class Overlay : OISObjBase, IEnumerable<int>
    {
        // this is the id of this overlay
        public const int DEFAULT_OVERLAY_ID = -1;
        private int overlayID = DEFAULT_OVERLAY_ID;

        // this is a list of all isoPlotObjects contained this object
        private Dictionary<int, int> isoPlotObjsInOverlay = new Dictionary<int, int>();

        // this is a count of the background usages in our overlay list
        private int backgroundUsageCount = 0;

        // this is a count of the normal edge usages in our overlay list
        private int normalEdgeUsageCount = 0;

        // this is a count of the contour edge usages in our overlay list
        private int contourEdgeUsageCount = 0;

        // this is a count of the invert edge usages in our overlay list
        private int invertEdgeUsageCount = 0;

        public const string OVERLAY_IDENTKEY_SEPARATOR = "|";
        public const string DEFAULT_IDENT_KEY = "<NoPix>" + OVERLAY_IDENTKEY_SEPARATOR;
        private string identKey = DEFAULT_IDENT_KEY;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overlayIDIn">the overlayID for this object</param>
        public Overlay(int overlayIDIn)
        {
            overlayID = CleanAllFlagsFromOverlayID(overlayIDIn);

            // we leave the identkey value at the defaults
            //SetIndentKey();
            // set the other counts
            SetNormalEdgeCount();
            SetContourEdgeCount();
            SetInvertEdgeCount();
            SetBackgroundCount();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor for one tag
        /// </summary>
        /// <param name="overlayIDIn">the overlayID for this object</param>
        /// <param name="isoPlotObjectTag">the tag (id + flags) of the IsoPlotObject to add</param>
        public Overlay(int overlayIDIn, int isoPlotObjectTag)
        {
            overlayID = CleanAllFlagsFromTag(overlayIDIn);

            // check the tag
            if (TagHasOneAndOnlyOneFlag(isoPlotObjectTag) == false) throw new Exception("Multiple Flags on Tag");

            // add isoPlotObjectTag value here, 
            isoPlotObjsInOverlay.Add(CleanAllFlagsFromTag(isoPlotObjectTag), isoPlotObjectTag);

            // now calc the IdentKey value. 
            SetIndentKey();
            // set the other counts
            SetNormalEdgeCount();
            SetContourEdgeCount();
            SetInvertEdgeCount();
            SetBackgroundCount();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Builds a new overlay with all of the existing tags plus a new one. Must be 
        /// supplied with a new ID
        /// </summary>
        /// <param name="newBuilderOverlayId">the overlay id for the new object</param>
        /// <param name="isoPlotObjectTag">the tag (id + flags) of the IsoPlotObject to add</param>
        public Overlay BuildNewOverlayWithAddedUsage(int newBuilderOverlayId, int isoPlotObjectTag)
        {
            Overlay newObj = new Overlay(newBuilderOverlayId);

            // check the tag
            if (TagHasOneAndOnlyOneFlag(isoPlotObjectTag) == false) throw new Exception("Multiple Flags on Tag");

            // this ought to clone the contents nicely
            foreach (int usageValue in isoPlotObjsInOverlay.Values)
            {
                newObj.Add(usageValue);
            }
            // add on our additional tag
            newObj.Add(isoPlotObjectTag);

            // set the ident key
            newObj.SetIndentKey();
            // set the other counts
            newObj.SetNormalEdgeCount();
            newObj.SetBackgroundCount();
            newObj.SetContourEdgeCount();
            newObj.SetInvertEdgeCount();

            return newObj;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Builds a new overlay with all of the existing tags minus the specified one. Must be 
        /// supplied with a new ID. Basically the same as the one below which removes by ID 
        /// but specifically tests to see if we are dealing with a tag
        /// </summary>
        /// <param name="newBuilderOverlayId">the overlay id for the new object</param>
        /// <param name="isoPlotObjectTag">the usage Tag to skip</param>
        public Overlay BuildNewOverlayWithMissingTag(int newBuilderOverlayId, int isoPlotObjectTag)
        {
            Overlay newObj = new Overlay(newBuilderOverlayId);

            // check the tag
            if (TagHasOneAndOnlyOneFlag(isoPlotObjectTag) == false) throw new Exception("Multiple Flags on Tag");

            int builderID = CleanAllFlagsFromTag(isoPlotObjectTag);

            // just call this with the tag cleaned up to be a builder ID
            return BuildNewOverlayWithMissingID(newBuilderOverlayId, builderID);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Builds a new overlay with all of the existing tags minus the 
        /// any belonging to the specified ID. Must be 
        /// supplied with a new ID
        /// </summary>
        /// <param name="newBuilderOverlayId">the overlay id for the new object</param>
        /// <param name="isoPlotObjectID">the builder ID to skip</param>
        public Overlay BuildNewOverlayWithMissingID(int newBuilderOverlayId, int isoPlotObjectID)
        {
            Overlay newObj = new Overlay(newBuilderOverlayId);

            int builderID = CleanAllFlagsFromTag(isoPlotObjectID);

            // this ought to clone the contents nicely
            foreach (int usageValue in isoPlotObjsInOverlay.Values)
            {
                int objID = CleanAllFlagsFromTag(usageValue);
                if (builderID == objID) continue;
                // not one we need to skip so add it
                newObj.Add(usageValue);
            }

            // set the ident key
            newObj.SetIndentKey();
            // set the other counts
            newObj.SetNormalEdgeCount();
            newObj.SetBackgroundCount();
            newObj.SetContourEdgeCount();
            newObj.SetInvertEdgeCount();

            return newObj;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a new tag to the list.
        /// </summary>
        /// <param name="builderIDTagIn">the tag (ID+flags) of the builderID</param>
        public void Add(int builderIDTagIn)
        {

            // check the tag
            if (TagHasOneAndOnlyOneFlag(builderIDTagIn) == false) throw new Exception("Multiple Flags on Tag");
            
            // add tag value here, 
            isoPlotObjsInOverlay.Add(CleanAllFlagsFromTag(builderIDTagIn), builderIDTagIn);

            // now calc the IdentKey value. 
            SetIndentKey();
            // set the other counts
            SetNormalEdgeCount();
            SetBackgroundCount();
            SetContourEdgeCount();
            SetInvertEdgeCount();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clean all of the flags off a Tag, This just leaves the builderID
        /// </summary>
        /// <param name="tagIn">the tag to clean</param>
        public static int CleanAllFlagsFromTag(int tagIn)
        {
            return tagIn & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clean all of the flags off an overlayID
        /// </summary>
        /// <param name="overlayIDIn">the overlayIDIn to clean</param>
        public static int CleanAllFlagsFromOverlayID(int overlayIDIn)
        {
            return overlayIDIn & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a tag is a background usage
        /// </summary>
        /// <param name="tagIn">the tag to check</param>
        /// <returns>True - is a background, false - is not</returns>
        public static bool TagIsBackgroundUsage(int tagIn)
        {
            if ((tagIn & (int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_BACKGROUND) != 0) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a tag is a normal edge usage
        /// </summary>
        /// <param name="tagIn">the tag to check</param>
        /// <returns>True - is a edge, false - is not</returns>
        public static bool TagIsNormalEdgeUsage(int tagIn)
        {
            if ((tagIn & (int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE) != 0) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a tag is a contour edge usage
        /// </summary>
        /// <param name="tagIn">the tag to check</param>
        /// <returns>True - is a edge, false - is not</returns>
        public static bool TagIsContourEdgeUsage(int tagIn)
        {
            if ((tagIn & (int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_CONTOUREDGE) != 0) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a tag is a invert edge usage
        /// </summary>
        /// <param name="tagIn">the tag to check</param>
        /// <returns>True - is a edge, false - is not</returns>
        public static bool TagIsInvertEdgeUsage(int tagIn)
        {
            if ((tagIn & (int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE) != 0) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the background count from the existing state.
        /// </summary>
        public void SetBackgroundCount()
        {
            // reset
            backgroundUsageCount = 0;

            // loop through the contents
            foreach (int tagValue in isoPlotObjsInOverlay.Values)
            {
                if (Overlay.TagIsBackgroundUsage(tagValue) == true) backgroundUsageCount++;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the normal edge count from the existing state.
        /// </summary>
        public void SetNormalEdgeCount()
        {
            // reset
            normalEdgeUsageCount = 0;

            // loop through the contents
            foreach (int tagValue in isoPlotObjsInOverlay.Values)
            {
                if (Overlay.TagIsNormalEdgeUsage(tagValue) == true) normalEdgeUsageCount++;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the contour edge count from the existing state.
        /// </summary>
        public void SetContourEdgeCount()
        {
            // reset
            contourEdgeUsageCount = 0;

            // loop through the contents
            foreach (int tagValue in isoPlotObjsInOverlay.Values)
            {
                if (Overlay.TagIsContourEdgeUsage(tagValue) == true) contourEdgeUsageCount++;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the invert edge count from the existing state.
        /// </summary>
        public void SetInvertEdgeCount()
        {
            // reset
            invertEdgeUsageCount = 0;

            // loop through the contents
            foreach (int tagValue in isoPlotObjsInOverlay.Values)
            {
                if (Overlay.TagIsInvertEdgeUsage(tagValue) == true) invertEdgeUsageCount++;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the ident key from the existing state.
        /// </summary>
        public void SetIndentKey()
        {
            string outIdent = "";

            // loop through the contents
            foreach (int tagValue in isoPlotObjsInOverlay.Values)
            {
                outIdent += tagValue.ToString("x8") + OVERLAY_IDENTKEY_SEPARATOR;
            }
            if (identKey == "") identKey = DEFAULT_IDENT_KEY;
            identKey = outIdent;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates an ident key from the existing state plus a tag
        /// </summary>
        public string CalcIndentKeyPlusUsage(int tagIn)
        {

            // just bang it on the end
            string outIdent = IdentKey + tagIn.ToString("x8") + OVERLAY_IDENTKEY_SEPARATOR;
            if (outIdent == "") outIdent = DEFAULT_IDENT_KEY;
            return outIdent;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates an ident key from the existing state minus a builder object
        /// Tag. A tag contains flag information as well as the id. We ignore all that
        /// and just skip every tag which matches the builderID.
        /// </summary>
        /// <param name="tagIn">the tag we look for</param>
        public string CalcIndentKeyMinusTag(int tagIn)
        {
            // check the tag
            if (TagHasOneAndOnlyOneFlag(tagIn) == false) throw new Exception("Multiple Flags on Tag");

            int builderID = CleanAllFlagsFromTag(tagIn);
            // just return this
            return CalcIndentKeyMinusID(builderID);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates an ident key from the existing state minus a builder object
        /// id. We ignore all tag flags that may be there
        /// and just skip every tag which matches the builderID.
        /// </summary>
        /// <param name="idIn">the id we look for</param>
        /// <returns>the ident key without the specified overlay in it</returns>
        public string CalcIndentKeyMinusID(int idIn)
        {
            string outIdent = "";

            int builderID = CleanAllFlagsFromTag(idIn);

            // loop through the contents
            foreach (int usageValue in isoPlotObjsInOverlay.Values)
            {
                // do we want to skip this one?
                int currentID = CleanAllFlagsFromTag(usageValue);
                if (currentID == builderID) continue;

                outIdent += usageValue.ToString("x8") + OVERLAY_IDENTKEY_SEPARATOR;
            }
            if (outIdent == "") outIdent = DEFAULT_IDENT_KEY;
            return outIdent;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We get the first builderID in the overlay that does not exist in a 
        /// supplied list. We ignore all tag flags that may be there
        /// and just skip every tag which matches the builderID.
        /// </summary>
        /// <param name="builderIDList">a list of builder IDs to check</param>
        /// <param name="altBuilderID">an alternative builder id we test agains</param>
        /// <returns>the builderID not in the list or -ve for fail</returns>
        public int GetFirstBuilderIDNotInList(List<int> builderIDList, int altBuilderID)
        {

            // loop through the every builderID in our overlay
            foreach (int builderID in isoPlotObjsInOverlay.Keys)
            {
                // we never trigger on the alt
                if (altBuilderID == builderID) continue;
                // test against the list
                if (builderIDList.Contains(builderID) == false)
                {
                    // we found one not in the list, return it.
                    return builderID;
                }
            }            
            return 0;
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates an ident key from a single tag.
        /// </summary>
        public static string CalcIndentKeyFromSingleUsage(int tagIn)
        {
            return tagIn.ToString("x8") + OVERLAY_IDENTKEY_SEPARATOR;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// gets the unique ident key
        /// </summary>
        public string IdentKey
        {
            get
            {
                return identKey;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides the generic ToString()
        /// </summary>
        public override string ToString()
        {
            if (IdentKey == null) return "<no ident key>";
            return IdentKey;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// gets/sets the overlayID
        /// </summary>
        public int OverlayID
        {
            get
            {
                return overlayID;
            }
            set
            {
                overlayID = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the backgroundUsageCount. This is set when isoPlotObject tags are added
        /// </summary>
        public int BackgroundUsageCount
        {
            get
            {
                return backgroundUsageCount;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets all counts. This is combined into one call for efficiency
        /// </summary>
        public void GetAllCounts(out int overlayCountOut, out int backgroundUsageCountOut, out int normalEdgeUsageCountOut, out int contourEdgeUsageCountOut, out int invertEdgeUsageCountOut)
        {
            overlayCountOut = isoPlotObjsInOverlay.Count;
            backgroundUsageCountOut = backgroundUsageCount;
            normalEdgeUsageCountOut = normalEdgeUsageCount;
            contourEdgeUsageCountOut = contourEdgeUsageCount;
            invertEdgeUsageCountOut = invertEdgeUsageCount;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the total edge usage count. Is sum of all of edge counts.
        /// </summary>
        public int TotalEdgeUsageCount
        {
            get
            {
                return normalEdgeUsageCount+ contourEdgeUsageCount + invertEdgeUsageCount;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the normalEdgeUsageCount. This is set when isoPlotObject tags are added
        /// </summary>
        public int NormalEdgeUsageCount
        {
            get
            {
                return normalEdgeUsageCount;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the contourEdgeUsageCount. This is set when isoPlotObject tags are added
        /// </summary>
        public int ContourEdgeUsageCount
        {
            get
            {
                return contourEdgeUsageCount;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the invertEdgeUsageCount. This is set when isoPlotObject tags are added
        /// </summary>
        public int InvertEdgeUsageCount
        {
            get
            {
                return invertEdgeUsageCount;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the overlayCount. This is the number of builderIDTags which have been added
        /// </summary>
        public int OverlayCount
        {
            get
            {
                return isoPlotObjsInOverlay.Count;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// An indexer for the isoPlotObjectIDs in the collection. Given a builder
        /// Object ID it will return the tag it uses.
        /// </summary>
        /// <param name="isoPlotObjectID">the isoPlotObjectID to look for</param>
        /// <returns>
        /// The usage or -1 for fail
        /// </returns>
        public int this[int isoPlotObjID]
        {
            get
            {
                int isoPlotObjIDtmp = 0;
                isoPlotObjsInOverlay.TryGetValue(CleanAllFlagsFromTag(isoPlotObjID), out isoPlotObjIDtmp);
                return isoPlotObjIDtmp;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if this overlay already contains a tag.
        /// </summary>
        /// <param name=" tagIn">the tag to check</param>
        /// <returns>
        /// True - this overlay contains the tag, false - it does not
        /// </returns>
        public bool DoesOverlayContainTag(int tagIn)
        {
            // loop through the contents, a bit slow but we don't have that
            // many in here
            foreach (int usageValue in isoPlotObjsInOverlay.Values)
            {
                if (usageValue == tagIn) return true;
            }
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if this overlay already contains any tag belonging to a builderID.
        /// </summary>
        /// <param name="isoPlotObjID">the isoPlotObjID to check</param>
        /// <returns>
        /// True - this overlay contains a usage referencing this ID, false - it does not
        /// </returns>
        public bool DoesOverlayContainIsoPlotID(int isoPlotObjID)
        {
            int isoPlotObjIDtmp = 0;
            // this returns true or false depending on whether the builder id is there or not
            return isoPlotObjsInOverlay.TryGetValue((int)Overlay.CleanAllFlagsFromTag(isoPlotObjID), out isoPlotObjIDtmp);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Checks to see that there is one and only one flag on a tag
        /// </summary>
        /// <param name="builderIDTag">the builderIDTag to check</param>
        /// <returns>
        /// True - one and only one tag set, false - no tags or more than one
        /// </returns>
        public static bool TagHasOneAndOnlyOneFlag(int builderIDTag)
        {
            int flags = builderIDTag & 0x7f000000;
            if (flags == 0) return false;
            return (flags & (flags - 1)) == 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a background was applied after an invert edge
        /// </summary>
        /// <returns>true - background has come after invert edge, false - it did not</returns>
        public bool InvertEdgeIsOnTopOfBackground()
        {
            bool invertEdgeSeen = false;
            bool backgroundSeen = false;
            bool lastWasInvertEdge = false;

            // this ought to clone the contents nicely
            foreach (int usageValue in isoPlotObjsInOverlay.Values)
            {
                if (TagIsInvertEdgeUsage(usageValue) == true)
                {
                    invertEdgeSeen = true;
                    lastWasInvertEdge = true;
                }
                else if (TagIsBackgroundUsage(usageValue) == true)
                {
                    backgroundSeen = true;
                    lastWasInvertEdge = false;
                }
            }

            // ok by the time we get here we have an idea of the order of things
            if ((invertEdgeSeen == true) && (backgroundSeen == true) && (lastWasInvertEdge == true)) return true;
            // anything else is false
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a list of builder id tags (essentially the values from the dict)
        /// </summary>
        public List<int> ListOfTags
        {
            get
            {
                List<int> outList = new List<int>();
                foreach (int usageValue in isoPlotObjsInOverlay.Values)
                {
                    outList.Add(usageValue);
                }
                return outList;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Supports the IEnumerable, returns an enumerator on the dictionary values
        /// </summary>
        public IEnumerator<int> GetEnumerator()
        {
            return isoPlotObjsInOverlay.Values.GetEnumerator();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Supports the IEnumerable, returns an enumerator on the dictionary values
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            //forces use of the non-generic implementation on the Values collection
            return ((IEnumerable)isoPlotObjsInOverlay.Values).GetEnumerator();
        }

    }
}

