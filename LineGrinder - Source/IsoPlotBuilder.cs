// NOTE: If you are new to this, SAFE and UNSAFE probably does not mean what
//       you think it does in this situation. You might want to look it up.
// 
// NOTE: You must also compile with the "Allow Unsafe Code" option enabled
//       in the Project>Properties menu if you uncomment the line below. 
//
// NOTE: comment the line below out if you want to compile in the safe but  
//       slow bitmap creation mode. The only downside to this is that you
//       will have to wait a bit when you look at the plots for isoSteps
//       1 to 3
#define UNSAFE_BITMAPS_PERMITTED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OISCommon;
using System.ComponentModel;

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
    /// A class to assist with building GCodes from a Gerber file
    /// </summary>
    public class IsoPlotBuilder : OISObjBase
    {
        // isolationPlotArray is the array of ints we use to figure out our isolation 
        // cuts. We refer to the cells in this array as isoCells even though they
        // are never directly visible to the user graphically. It is just a
        // convenient terminology. Each isoCell contains an address of an Overlay
        //
        // The Overlay is the collection of IsoPlotObjects and Flags which are using
        // that isoCell. Note that the overlay distinguishes between a IsoPlotObject
        // used as an Edge of an object to one which is part of the background. In
        // order to save on space we re-use overlays.
        //
        // Thus if an overlay represents a isoCell in use for an edge of obj1 and 
        // a background Obj2 any other isoCell which has that specific use will also
        // use that overlay in its isoCell. In this way only 1 object needs to be
        // created. A isoCell which used edge of obj1 and an edge of Obj2 would 
        // have a different overlay.
        // 
        // empty unused isoCells are inplicitly assigned an overlay with id 0. This 
        // overlay is set up when the isolationPlotArray is initialized. 
        //
        private int[,] isolationPlotArray = null;
        private int isoPlotWidth = ctlPlotViewer.DEFAULT_PLOT_WIDTH;
        private int isoPlotHeight = ctlPlotViewer.DEFAULT_PLOT_HEIGHT;
        // this contains the IsoPlotSegments after isolationPlot Step3
        private List<IsoPlotObjectSegment> isoPlotStep3List = null;

        // this value in the isolation array means no IsoPlotObject
        // is using that isoCell for any purpose
        private const int ISOCELL_NOTUSED = 0;

        // this id is incremented and assigned to every gerber object we draw
        public const int EMPTY_GCODE_BUILDER_OBJ_ID = 0;
        private int currentIsoPlotBuilderObjID = EMPTY_GCODE_BUILDER_OBJ_ID;

        // this is a list of all gCode builder objects we have assigned
        private List<IsoPlotObject> isoPlotObjList = new List<IsoPlotObject>();

        // these are the options we use when creating GCode file output
        private FileManager currentFileManager = new FileManager();

        private GCodeFile currentGCodeFile = new GCodeFile();

        // if multiple isoPlotObjects need to use a isoCell in the isoPlotArray we 
        // compile their isoPlotObjectIDs into an overlay and stuff that list into a dictionary
        // in this manner we can figure out which isoPlotObjects are using any particular 
        // isoCell and for what purpose. The collection below contains all of the allocated
        // Overlay objects
        private OverlayCollection currentOverlayCollection = new OverlayCollection();

        // if we joint two segments together in a chain this is the maximum gap
        private const float DEFAULT_MAX_SEGMENT_GAP_IN_CHAIN_VIA_OVERLAY_NORMAL = 4.9F;
        private const float DEFAULT_MAX_SEGMENT_GAP_IN_CHAIN_VIA_OVERLAY_EDGEMILL = 14.9F;
        // if we join two chains together this is the maximum gap
        private const float DEFAULT_MAX_SEGMENT_GAP_IN_CHAIN_VIA_DISTANCE_NORMAL = 9.9F;
        private const float DEFAULT_MAX_SEGMENT_GAP_IN_CHAIN_VIA_DISTANCE_EDGEMILL = 19.9F;

        // vars to contain our gap distance metrics - used to join chains
        private float overlayMaxSegmentGap_Normal = DEFAULT_MAX_SEGMENT_GAP_IN_CHAIN_VIA_OVERLAY_NORMAL;
        private float overlayMaxSegmentGap_EdgeMill = DEFAULT_MAX_SEGMENT_GAP_IN_CHAIN_VIA_OVERLAY_EDGEMILL;
        private float distanceMaxSegmentGap_Normal = DEFAULT_MAX_SEGMENT_GAP_IN_CHAIN_VIA_DISTANCE_NORMAL;
        private float distanceMaxSegmentGap_EdgeMill = DEFAULT_MAX_SEGMENT_GAP_IN_CHAIN_VIA_DISTANCE_EDGEMILL;

        // a limit of the number of iso segs in chain
        private const int MAX_ISOSEGMENTS_IN_CHAIN = 2500;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="widthIn">The width of the plot</param>
        /// <param name="heightIn">the height of the plot</param>
        public IsoPlotBuilder(int widthIn, int heightIn)
        {
            isoPlotWidth = widthIn;
            isoPlotHeight = heightIn;
            // sanity checks
            if (isoPlotWidth <= 0) isoPlotWidth = ctlPlotViewer.DEFAULT_PLOT_WIDTH;
            if (isoPlotHeight <= 0) isoPlotHeight = ctlPlotViewer.DEFAULT_PLOT_HEIGHT;
            try
            {
                // create the plot now
                isolationPlotArray = new int[isoPlotWidth, isoPlotHeight];
            }
            catch (Exception ex)
            {
                LogMessage("Exception creating array. ex=" + ex.Message + ", isoPlotWidth="+ isoPlotWidth.ToString() + ", isoPlotHeight=" + isoPlotHeight.ToString());
                if ((ex is OutOfMemoryException)==true)
                {
                    OISMessageBox("The isoplot array is too large. Consider reducing the IsoPlotUnitsPerUnit value on the Settings Tab./n/nPlease see the logs");
                }
                // rethrow it
                throw ex;
            }
            // create our IsoPlotBuilder object which represents unused isoCells
            IsoPlotObject_Empty emptyIsoPlotBuilder = new IsoPlotObject_Empty(EMPTY_GCODE_BUILDER_OBJ_ID);
            // add the empty isoplot builder to the list which tracks these things
            isoPlotObjList.Add(emptyIsoPlotBuilder);
            // create our overlay which represents unused isoCells.
            Overlay overlayObj = new Overlay(OverlayCollection.EMPTY_OVERLAY_ID);
            // now add the empty overlay to the overlay collection
            CurrentOverlayCollection.Add(overlayObj);

            // at this point we have one IsoPlotBuilder_Empty object and it has an ID of 0. It has been
            // added to the list that collects these. It has also been placed in an overlay which has an ID 
            // of 0. This overlay has been added to the overlay collection. Since all isoCells in the isolationPlotArray
            // represent overlays and they are also initialized to zero. They implictly point to the newly created
            // overlay which, in turn, references a single IsoPlotBuilder object of ID 0 which does nothing when processed.
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the max segment gap according to the board type. Overlay Version
        /// </summary>
        /// <param name="boardType">the type of board we are processing</param>
        private float GetChainMaxSegmentGap_Overlay(FileManager.OperationModeEnum boardType)
        {
            if (boardType == FileManager.OperationModeEnum.BoardEdgeMill)
            {
                return overlayMaxSegmentGap_EdgeMill;
            }
            else return overlayMaxSegmentGap_Normal;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the max segment gap according to the board type. Distance Version
        /// </summary>
        private float GetChainMaxSegmentGap_Distance(FileManager.OperationModeEnum boardType)
        {
            if (boardType == FileManager.OperationModeEnum.BoardEdgeMill)
            {
                return distanceMaxSegmentGap_EdgeMill;
            }
            else return distanceMaxSegmentGap_Normal;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets next GCode object id. These are assigned sequentially as we draw
        /// the object on the isolationPlot
        /// </summary>
        private int NextIsoPlotObjectID
        {
            get
            {
                currentIsoPlotBuilderObjID++;
                // we cannot cope with greater than 2^24 objects.
                if (currentIsoPlotBuilderObjID >= 16777216)
                {
                    throw new NotImplementedException("NextIsoPlotObjectID >= 2^24. Way too many objects. This should not happen");
                }
                return currentIsoPlotBuilderObjID;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the overlaycollection. Will never return a null
        /// </summary>
        public OverlayCollection CurrentOverlayCollection
        {
            get
            {
                if (currentOverlayCollection == null) currentOverlayCollection = new OverlayCollection();
                return currentOverlayCollection;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the file options to use. Never gets/sets a null value
        /// </summary>
        public FileManager CurrentFileManager
        {
            get
            {
                if (currentFileManager == null) currentFileManager = new FileManager();
                return currentFileManager;
            }
            set
            {
                currentFileManager = value;
                if (currentFileManager == null) currentFileManager = new FileManager();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the isoPlotStep3List. Will never return a null
        /// </summary>
        public List<IsoPlotObjectSegment> IsoPlotStep3List
        {
            get
            {
                if (isoPlotStep3List == null) isoPlotStep3List = new List<IsoPlotObjectSegment>();
                return isoPlotStep3List;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the isolation plot array, will never get a null, This is usually set
        /// to the proper dimensions when the PerformGerberToGCodeStep1 is called
        /// </summary>
        public int[,] IsolationPlotArray
        {
            get
            {
                if (isolationPlotArray == null)
                {
                    isolationPlotArray = new int[ctlPlotViewer.DEFAULT_PLOT_WIDTH, ctlPlotViewer.DEFAULT_PLOT_HEIGHT];
                    // always set this
                    isoPlotWidth = ctlPlotViewer.DEFAULT_PLOT_WIDTH;
                    isoPlotHeight = ctlPlotViewer.DEFAULT_PLOT_HEIGHT;
                }
                return isolationPlotArray;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the Isolation Array as a bitmap so the user can see what is going
        /// on. The objects displayed will be different depending on where in the 
        /// Gerber to GCode processing this is called. For example, if you call this
        /// after the lines running through other lines are removed from the isoplot
        /// then you will not see them. 
        /// </summary>
        /// <remarks>
        /// Credits: this follows one of the examples in here
        ///  http://stackoverflow.com/questions/392324/converting-an-array-of-pixels-to-an-image-in-c
        ///    Most comments in the code below are from the original source - not me
        /// </remarks>
        /// <returns>A bitmap which is the same size as the virtualPlotSize and which
        /// has the isoCells with various attributes (intersection, runover background etc)
        /// colored in useful ways</returns>
        /// <param name="displayModeIn">the display mode, this changes the interpretation</param>
        public Bitmap GetIsolationArrayAsBitmap(DisplayModeEnum displayModeIn)
        {
            // if you are getting compliler errors here, read the comments at the very top of 
            // this file and (possibly) comment out the #define UNSAFE_BITMAPS_PERMITTED line

            #if UNSAFE_BITMAPS_PERMITTED
            // create a bitmap and manipulate it using old school C style pointers (unsafe 
            // according to .NET). This is MUCH faster
            Bitmap bmp = new Bitmap(isoPlotWidth, isoPlotHeight, PixelFormat.Format32bppArgb);
            BitmapData bits = bmp.LockBits(new Rectangle(0, 0, isoPlotWidth, isoPlotHeight), ImageLockMode.ReadWrite, bmp.PixelFormat);
            try
            {
                unsafe
                {
                    for (int y = 0; y < isoPlotHeight; y++)
                    {
                        uint* row = (uint*)((byte*)bits.Scan0 + (y * bits.Stride));
                        for (int x = 0; x < isoPlotWidth; x++)
                        {
                            row[x] = ConvertIsoPlotValueToArgb(displayModeIn, x, y);
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bits);
            }
            #else
            // create a bitmap and manipulate it, using the painfully slow .NET
            // SetPixel method.
            Bitmap bmp = new Bitmap(isoPlotWidth, isoPlotHeight, PixelFormat.Format32bppArgb);
            for (int y = 0; y < isoPlotHeight; y++)
            {
                for (int x = 0; x < isoPlotWidth; x++)
                {
                    Color tmpColor = ConvertIsoPlotValueToColor(displayModeIn, x, y);
                    bmp.SetPixel(x, y, tmpColor);
                }
            }
#endif
            return bmp;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the isolation plot isoCells to colors. This makes it possible
        /// to graphically display the usage of each isoCell in the screen.
        /// 
        /// The color usage is:
        ///   ISOINTERSECTION_COLOR: Used as an edge by two or more isoPlotObjects (an Intersection)
        ///   ISONORMALEDGE_COLOR:         Used by a single isoPlotObject as an edge
        ///   ISOOVERLAY_COLOR:      Used by two or more isoPlotObjects as a background
        ///   ISOINTERIOR_COLOR:     Used by a single isoPlotObject as an background
        ///   ISOBACKGROUND_COLOR:   Used by no isoPlotObjects
        /// 
        /// </summary>
        /// <remarks>
        /// Does NOT check for x,y coordinates being sound. 
        /// Colors here must be synced with the ones in ConvertIsoPlotValueToArgb
        /// </remarks>
        /// <param name="x">x coord on isoplot</param>
        /// <param name="y">y coord on isoplot</param>
        /// <param name="displayModeIn">the display mode, this changes the interpretation</param>
        private Color ConvertIsoPlotValueToColor(DisplayModeEnum displayModeIn, int x, int y)
        {
            // the algorythm is the same. Just call the RGB one and convert it.
            uint rgbVal = ConvertIsoPlotValueToArgb(displayModeIn, x, y);
            return ApplicationColorManager.ConvertIsoBackgroundRGBToColor(rgbVal);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the isolation plot bitmap pixels to colors. This makes it possible
        /// to graphically display the usage of each isoCell in the screen.
        /// 
        /// The color usage is:
        ///   ISOINTERSECTION_COLOR: Used as an edge by two or more isoPlotObjects (an Intersection)
        ///   ISONORMALEDGE_COLOR:         Used by a single isoPlotObject as an edge
        ///   ISOOVERLAY_COLOR:      Used by two or more isoPlotObjects as a background
        ///   ISOINTERIOR_COLOR:     Used by a single isoPlotObject as an background
        ///   ISOBACKGROUND_COLOR:   Used by no isoPlotObjects
        /// 
        /// </summary>
        /// <remarks>
        /// Does NOT check for x,y coordinates being sound. 
        /// Colors here must be synced with the ones in ConvertIsoPlotValueToColor 
        /// </remarks>
        /// <param name="x">x coord on isoplot</param>
        /// <param name="y">y coord on isoplot</param>
        /// <param name="displayModeIn">the display mode, this changes the interpretation</param>
        private uint ConvertIsoPlotValueToArgb(DisplayModeEnum displayModeIn, int x, int y)
        {
            // get the overlay ID in the isoCell
            int overlayIDwithFlags = 0;
            int overlayID = 0;
            Overlay overlayObj = null;
            int overlayCount;
            int backgroundUsageCount;
            int normalEdgeUsageCount;
            int contourEdgeUsageCount;
            int invertEdgeUsageCount;

            // get this now we will need it for lots of tests
            overlayIDwithFlags = isolationPlotArray[x, y];
            overlayID = CleanFlagsFromOverlayID(overlayIDwithFlags);

            // this short circuits the tests below and speeds up the production of the bitmap
            // for most isoplots this will trigger 50%+ of the time (estimated)
            if (overlayID == OverlayCollection.EMPTY_OVERLAY_ID) return ApplicationColorManager.ISOBACKGROUND_COLOR_AS_UINT;

            // we get the overlay object from the isoplot
            overlayObj = CurrentOverlayCollection[isolationPlotArray[x, y]];
            if (overlayObj == null)
            {
                // this should not happen
                LogMessage("ConvertIsoPlotValueToArgb overlayObj==null at x=" + x.ToString() + ", y=" + y.ToString());
                throw new Exception("Overlay object not found.");
            }
            // get our counts from the overlay
            overlayObj.GetAllCounts(out overlayCount, out backgroundUsageCount, out normalEdgeUsageCount, out contourEdgeUsageCount, out invertEdgeUsageCount);

            // we do draw colors differntly in differnt isostep modes
            if (displayModeIn == DisplayModeEnum.DisplayMode_ISOSTEP1)
            {
                // ISOSTEP1 displays intersections and edges and backgrounds

                // do the colors as required
                if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_INVERTEDGE)) != 0) return ApplicationColorManager.ISOINVERTEDGE_COLOR_AS_UINT;
                if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_CONTOUREDGE)) != 0) return ApplicationColorManager.ISOCONTOUREDGE_COLOR_AS_UINT;
                if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_NORMALEDGE)) != 0) return ApplicationColorManager.ISONORMALEDGE_COLOR_AS_UINT;
                // must be a background, but are we an overlap?
                if (overlayCount>1) return ApplicationColorManager.ISOOVERLAY_COLOR_AS_UINT;// more than one overlay in here color it this way
                // must just be a normal background
                return ApplicationColorManager.ISOINTERIOR_COLOR_AS_UINT;
            }
            else if (displayModeIn == DisplayModeEnum.DisplayMode_ISOSTEP2)
            {
                // ISOSTEP2 we are not interested in showing intersections here but we do care about interiors most of all

                if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_BACKGROUND)) != 0) return ApplicationColorManager.ISOINTERIOR_COLOR_AS_UINT;
                // do the colors as required
                if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_INVERTEDGE)) != 0) return ApplicationColorManager.ISOINVERTEDGE_COLOR_AS_UINT;
                if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_CONTOUREDGE)) != 0) return ApplicationColorManager.ISOCONTOUREDGE_COLOR_AS_UINT;
                if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_NORMALEDGE)) != 0) return ApplicationColorManager.ISONORMALEDGE_COLOR_AS_UINT;
                // should be one of the above four colors should never get here
                return ApplicationColorManager.ISOERROR_COLOR_AS_UINT;
            }
            else
            { 
                // must be DisplayModeEnum.DisplayMode_ISOSTEP3, we show just the edge lines in all their glory
                // are we a disregard, we draw these differently
                if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_DISREGARD)) != 0)
                {
                    // we are a disregard
                    if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_INVERTEDGE)) != 0) return ApplicationColorManager.ISOINVERTEDGE_NOTDRAWN_COLOR_AS_UINT;
                    if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_CONTOUREDGE)) != 0) return ApplicationColorManager.ISOCONTOUREDGE_NOTDRAWN_COLOR_AS_UINT;
                    if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_NORMALEDGE)) != 0) return ApplicationColorManager.ISONORMALEDGE_NOTDRAWN_COLOR_AS_UINT;
                    // everything else gets the background
                    return ApplicationColorManager.ISOBACKGROUND_COLOR_AS_UINT;
                }
                else
                {
                    // we are not a disregard, note the precedence order here, edges can be both
                    if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_INVERTEDGE)) != 0) return ApplicationColorManager.ISOINVERTEDGE_COLOR_AS_UINT;
                    if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_CONTOUREDGE)) != 0) return ApplicationColorManager.ISOCONTOUREDGE_COLOR_AS_UINT;
                    if ((overlayIDwithFlags & (int)(OverlayFlagEnum.OverlayFlag_NORMALEDGE)) != 0) return ApplicationColorManager.ISONORMALEDGE_COLOR_AS_UINT;
                    // everything else gets the background
                    return ApplicationColorManager.ISOBACKGROUND_COLOR_AS_UINT;
                } 
            }
         }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs secondary processing on the isolation array to remove
        /// segments which overwrite backgrounds and which are edge superpositions
        /// </summary>
        /// <returns>z succes, nz fail</returns>
        /// <param name="errStr">a helpful error message</param>
        public int PerformSecondaryIsolationArrayProcessing(ref string errStr)
        {
            int retInt;
            retInt = PerformIsolationPlotInteriorIsoCellRemoval(ref errStr);
            if (retInt != 0)
            {
                LogMessage("PerformSecondaryIsolationArrayProcessing call to PerformIsolationPlotInteriorIsoCellRemoval returned " + retInt.ToString());
                return 333;
            }
            retInt = PerformIsoPlotSegmentDiscovery(ref errStr);
            if (retInt != 0)
            {
                LogMessage("PerformSecondaryIsolationArrayProcessing call to PerformIsoPlotSegmentDiscovery returned " + retInt.ToString());
                return 334;
            }
            // lets number all of the isoplot segments. Makes debugging easier
            int count = 0;
            foreach (IsoPlotObjectSegment segObj in IsoPlotStep3List)
            {
                segObj.DebugID = count;
                count++;
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the IsoPlotSegments discovered in PerformIsoPlotSegmentDiscovery
        /// into GCodeCmd objects and populates the supplied GCodeFile object
        /// </summary>
        /// <param name="errStr">a helpful error message</param>
        /// <param name="gcFile">the GCodeFile object we populate</param>
        /// <returns>z success, nz fail</returns>
        public int ConvertIsolationSegmentsToGCode(ref GCodeFile gcFile, ref string errStr)
        {
            //int debugChainCount = 0;
            int duplicateCount = 0;

            errStr = "";

            if (gcFile == null)
            {
                errStr = "Invalid GCode Object";
                LogMessage("ConvertIsolationSegmentsToGCode gcFile == null");
                return 231;

            }
            if (isoPlotStep3List == null)
            {
                errStr = "Isolation Step3 not performed";
                LogMessage("ConvertIsolationSegmentsToGCode isoPlotStep3List == null");
                return 232;

            }
            if (isoPlotStep3List.Count ==0)
            {
                errStr = "Isolation Step3 found now isolation segments";
                LogMessage("ConvertIsolationSegmentsToGCode isoPlotStep3List.Count ==0");
                return 233;
            }

            // yep, a generic list of generic lists. Each chain is a list and we have a list of chains
            List<List<IsoPlotObjectSegment>> listOfIsoPlotSegmentChains = new List<List<IsoPlotObjectSegment>>();

            // we must mark the duplicate segments. We can get these if two endpoint flashes exactly overwrite
            // each other. We do not need to draw exact duplicates and it really messes up the chaining
            duplicateCount = MarkDuplicateSegs();

            //DumpIsoPlotSegmentList(isoPlotStep3List, "isoPlotStep3List", true);

            // Build the chains by looking at the intersections isoplot segs that 
            // share an intersection point and builderID will be chained together

            // Note the loop here is just a counter. We will never have 
            // more chains than isoPlotStep3List.Count as that implies each segment is 
            // its own chain. W
            int xVal = 0;
            int yVal = 0;
            for(int i=0;i< isoPlotStep3List.Count;i++)
            {
                // get the isoplot segment nearest the current xyVal. This is done so that
                // the toolhead does not go zipping from one end of the plot to the other
                // everytime it wants to start a new chain
                IsoPlotObjectSegment segObj = GetUnchainedSegmentNearestPoint(xVal, yVal);
                if (segObj == null) break;

                // find all the isoPlotSegments Chained to this object
                List<IsoPlotObjectSegment> tmpChain = GetIsolationChainForSegment(segObj, CurrentFileManager.OperationMode);
                if (tmpChain == null)
                {
                    errStr = "Isolation Step3 error happened when finding isolation chains";
                    LogMessage("GetIsoChainForPoint: " + errStr);
                    return 243;
                }
                // add the chain list to our list of lists
                listOfIsoPlotSegmentChains.Add(tmpChain);
                // get the last segment in the chain and update our XY
                IsoPlotObjectSegment lastSegObj = tmpChain[tmpChain.Count - 1];
                if(lastSegObj.ReverseOnConversionToGCode==true)
                {
                    // we are reversed set this
                    xVal = lastSegObj.X0;
                    yVal = lastSegObj.Y0;

                }
                else
                {
                    // normal, set this
                    xVal = lastSegObj.X1;
                    yVal = lastSegObj.Y1;
                }

                //debugChainCount++;
                //DebugTODO("remove this");
                //DumpIsoPlotSegmentList(tmpChain, "Chain", true);
                //if (debugChainCount == 2)
                //{
                //    int foo = 1;
                //}
                //    if (debugChainCount >= 3) break;
            }

            // note that segment reversal will have automatically been performed
            // in the above chaining process. All isoPlotSegs will be appropriately 
            // marked

            LogMessage("isoPlotStep3List=" + isoPlotStep3List.Count.ToString());
            LogMessage("isoPlotStep3List DupCount =" + duplicateCount.ToString());
            LogMessage("isoPlotStep3List NonDupCount =" + (isoPlotStep3List.Count- duplicateCount).ToString());
            LogMessage("ChainCount=" + listOfIsoPlotSegmentChains.Count.ToString());
            LogMessage("SegsInAllChains=" + CountSegsInAllChains(listOfIsoPlotSegmentChains).ToString());

            // ok now we deal with orphan arcs. These are single arc chains that are remmnants of the end flash
            // process of lines. Lots of times these are already drawn by the lines and the fact that the 
            // arc remains is just an artifact of the conversion process
            RemoveOrphanArcs(listOfIsoPlotSegmentChains);

            // what mode are we in?
            if (CurrentFileManager.OperationMode == FileManager.OperationModeEnum.BoardEdgeMill)
            {
                // if we are in edge mill mode we have to figure out what chain (or chains) contain
                // the outside segments
                int minX = int.MaxValue;
                int minY = int.MaxValue;
                int maxX = int.MinValue;
                int maxY = int.MinValue;
                List<IsoPlotObjectSegment> outsideChainList = null;
                foreach (List<IsoPlotObjectSegment> chainList in listOfIsoPlotSegmentChains)
                {
                    // does the chain only contain a single segment, if so might just be some reference
                    // pins or other junk
                    if (chainList.Count == 1) continue;
                    if (DoesChainContainALineSegment(chainList) == false) continue;
                    // see if this chain represents the maximum and minimum values we have seen
                    bool retBool = TestChainForMaxAndMin(chainList, ref minX, ref minY, ref maxX, ref maxY);
                    if (retBool == true)
                    {
                        outsideChainList = chainList;
                    }
                }
                // did we find an outsideChainList?
                if (outsideChainList == null)
                {
                    // did not find an outside chain list try again this time let single segments in
                    foreach (List<IsoPlotObjectSegment> chainList in listOfIsoPlotSegmentChains)
                    {
                        // see if this chain represents the maximum and minimum values we have seen
                        bool retBool = TestChainForMaxAndMin(chainList, ref minX, ref minY, ref maxX, ref maxY);
                        if (retBool == true)
                        {
                            outsideChainList = chainList;
                        }
                    }
                    // test again
                    if (outsideChainList == null)
                    {
                        errStr = "No Outside Chain List Could Be Found";
                        LogMessage("ConvertIsolationSegmentsToGCode outsideChainList == null");
                        return 431;
                    }
                }

                // we have an outside chain list

                // do we need to apply tabs
                if ((gcFile.StateMachine.GCodeFileManager.EdgeMillNumTabs > 0) && (gcFile.StateMachine.GCodeFileManager.EdgeMillTabWidth > 0))
                {
                    // yes we need to apply tabs
                    List<IsoPlotObjectSegment_Line> lineList = GetIsoPlotLineSegmentsOrderedBySize(gcFile.StateMachine, outsideChainList, (gcFile.StateMachine.GCodeFileManager.EdgeMillTabWidth * 3));
                    if ((lineList == null) || (lineList.Count == 0))
                    {
                        errStr = "Want to add tabs but no lines found in IsoPlot chain.";
                        LogMessage("ConvertIsolationSegmentsToGCode (lineList == null) || (lineList.Count == 0)");
                        return 451;
                    }
                    // now we have it sorted we apply the tabs to the segments
                    for (int i = 0; i < gcFile.StateMachine.GCodeFileManager.EdgeMillNumTabs; i++)
                    {
                        // get a number between 0 <= index < lineList.Count
                        int index = i % lineList.Count;
                        // add a tab to that segment, because the list is sorted
                        // we add the tabs preferentially to the larger segments
                        lineList[index].NumberOfTabs++;
                        // make sure it knows the length of the tab
                        lineList[index].TabLength = gcFile.StateMachine.GCodeFileManager.EdgeMillTabWidth;
                    }
                    // now make sure that all of the tabs we applied actually stuck
                    int adjTabCount = 0;
                    foreach (IsoPlotObjectSegment_Line segObj in lineList)
                    {
                        adjTabCount+=segObj.CalcAdjustedNumberOfTabs(gcFile.StateMachine);
                    }
                    if(adjTabCount!=gcFile.StateMachine.GCodeFileManager.EdgeMillNumTabs)
                    {
                        // we could not apply some tabs - flag this as an error
                        errStr = "All tabs could not be added. Wanted "+gcFile.StateMachine.GCodeFileManager.EdgeMillNumTabs.ToString()+" got "+adjTabCount.ToString() + " is EdgeMillNumTabs too high?";
                        LogMessage("ConvertIsolationSegmentsToGCode could not add all tabs Wanted "+gcFile.StateMachine.GCodeFileManager.EdgeMillNumTabs.ToString()+" got "+adjTabCount.ToString());
                        return 452;
                    }
                }

                // now for each segObj in the chainList
                foreach (IsoPlotObjectSegment segObj in outsideChainList)
                {
                    // build the GCodeCmds and add it to our GCodeFile 
                    List<GCodeCmd> outList = segObj.GetGCodeCmds(gcFile.StateMachine);
                    if (outList != null)
                    {
                        foreach (GCodeCmd lineObj in outList)
                        {
                            gcFile.AddLine(lineObj);
                        }
                    }
                }
            }
            else
            {
                // now that we have a nice list of segments, convert them to
                // gcodes and add them to the GCodeFile. Since they are chained
                // we should have nice sequential runs and not have to jump
                // around so much lifting the bit up and putting it back down
                foreach (List<IsoPlotObjectSegment> chainList in listOfIsoPlotSegmentChains)
                {
                    // now for each segObj in the chainList
                    foreach (IsoPlotObjectSegment segObj in chainList)
                    {
                        // build the GCodeCmds and add it to our GCodeFile 
                        List<GCodeCmd> outList = segObj.GetGCodeCmds(gcFile.StateMachine);
                        if (outList != null)
                        {
                            foreach (GCodeCmd lineObj in outList)
                            {
                                gcFile.AddLine(lineObj);
                            }
                        }
                    }
                  //  DebugTODO("remove this");
                  //  if(junkCount>1) break;
                  //  junkCount++;
                }
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Orphan arcs are artifacts of endpoint flashes left over from the conversion
        /// process. They are short and are already drawn by line segments. We remove these
        /// because they make the tool head jump around un-necessarily
        /// </summary>
        /// <param name="listOfIsoPlotSegmentChains>the list of chains we check for the orphan arcs in</param>
        private void RemoveOrphanArcs(List<List<IsoPlotObjectSegment>> listOfIsoPlotSegmentChains)
        {
            if (listOfIsoPlotSegmentChains == null) return;

            // run through each chain
            foreach (List<IsoPlotObjectSegment> isoPlotChain in listOfIsoPlotSegmentChains)
            {
                // orphan arcs are always in a chain by themselves
                if (isoPlotChain.Count != 1) continue;
                // they are always arcs
                if ((isoPlotChain[0] is IsoPlotObjectSegment_Arc)==false) continue;
                // are we a duplicate? do not process
                if (isoPlotChain[0].IsDuplicate == true) continue;

                // we have a potential orphan arc, test it
                bool retBool = IsArcAlreadyDrawnInAChain((IsoPlotObjectSegment_Arc)isoPlotChain[0], listOfIsoPlotSegmentChains);
                if(retBool==true)
                {
                    // mark it so it will not draw
                    isoPlotChain[0].DoNotEmitToGCode = true;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if an arc is completely drawn in a chain. This means the start 
        /// point of the arc is at the end of one segment and the end is at the 
        /// beginning of another.
        /// </summary>
        /// <param name="listOfIsoPlotSegmentChains>the list of chains we check for the arc in</param>
        /// <returns>true the arc is contained, false it is not
        private bool IsArcAlreadyDrawnInAChain(IsoPlotObjectSegment_Arc arcToCheck, List<List<IsoPlotObjectSegment>> listOfIsoPlotSegmentChains)
        {
            bool arcStartIsInLine;
            bool arcEndIsInLine;

            if (listOfIsoPlotSegmentChains == null) return false;
            if (arcToCheck == null) return false;

            arcToCheck.GetEffectiveStartAndEndCoords(out int arcStartX, out int arcStartY, out int arcEndX, out int arcEndY);

            // check the arc against each chain
            foreach (List<IsoPlotObjectSegment> isoPlotChain in listOfIsoPlotSegmentChains)
            {
                // go through the chain. build lines
                for (int i = 0; i < isoPlotChain.Count-1; i++)
                {
                    // note there the test is < not <= here

                    if ((isoPlotChain[i] is IsoPlotObjectSegment_Line) == false) continue;
                    isoPlotChain[i].GetEffectiveStartAndEndCoords(out int line1StartX, out int line1StartY, out int line1EndX, out int line1EndY);
                    // quick test to see if it is imbedded totally within this first line
                    arcStartIsInLine = MiscGraphicsUtils.IsPointOnLine(arcStartX, arcStartY, line1StartX, line1StartY, line1EndX, line1EndY, 3);
                    arcEndIsInLine = MiscGraphicsUtils.IsPointOnLine(arcEndX, arcEndY, line1StartX, line1StartY, line1EndX, line1EndY, 3);
                    if ((arcStartIsInLine == true) && (arcEndIsInLine == true)) return true;
                    // we need two lines sequentially
                    if ((isoPlotChain[i+1] is IsoPlotObjectSegment_Line) == false) continue;
                    // we have two lines together, we see if the arc is on the line (3 decimals of rounding
                    isoPlotChain[i+1].GetEffectiveStartAndEndCoords(out int line2StartX, out int line2StartY, out int line2EndX, out int line2EndY);
                    // quick test to see if it is imbedded totally within this second line
                    arcStartIsInLine = MiscGraphicsUtils.IsPointOnLine(arcStartX, arcStartY, line2StartX, line2StartY, line2EndX, line2EndY, 3);
                    arcEndIsInLine = MiscGraphicsUtils.IsPointOnLine(arcEndX, arcEndY, line2StartX, line2StartY, line2EndX, line2EndY, 3);

                    // test if the arc start point is on the line which runs from line1 start to line 2 end.
                    // this does not work for non co-linear lines. Howver it should not return false positives should be very unlikely
                    bool arcStartIsOnLine = MiscGraphicsUtils.IsPointOnLine(arcStartX, arcStartY, line1StartX, line1StartY, line2EndX, line2EndY, 3);
                    // is the start on the line, if not no need to continue
                    if (arcStartIsOnLine==false) continue;
                    // test if the arc end point is on the line 
                    bool arcEndIsOnLine = MiscGraphicsUtils.IsPointOnLine(arcEndX, arcEndY, line1StartX, line1StartY, line2EndX, line2EndY, 3);
                    // is the start on the line, if so the arc is embedded in a part of a chain drawn by two lines
                    if (arcEndIsOnLine == true) return true;
                    
                    // we know the arc the arc is embedded in a part of a chain drawn by two lines
                    return true;
                }

            }

            // off the end, not found
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Finds the chain which starts closest to a point. We do this so we can 
        /// join chains
        /// </summary>
        /// <param name="listArray">the list array we check for the list in</param>
        /// <param name="minDist">the minimum distance between the first item on the list we found and the specified point</param>
        /// <param name="segIndex">the index into the listArray of the list we found. Will be -ve if not found</param>
        /// <param name="xVal">the xValue</param>
        /// <param name="yVal">the yValue</param>
        /// <returns>nearest segment or null for none found</returns>
        private void GetChainWhichStartsClosestToPoint(ref List<IsoPlotObjectSegment>[] listArray, out int segIndex, out float minDist, int xVal, int yVal)
        {
            minDist = float.MaxValue;
            segIndex = -1;

            int effectiveStart_X = 0;
            int effectiveStart_Y = 0;

            if (listArray == null) return;
            
            // loop through the array 
            for (int i = 0; i < listArray.Length; i++)
            {
                // get the current value
                List<IsoPlotObjectSegment> tmpList = listArray[i];
                // if it has already been consumed just try the next
                if (tmpList == null) continue;

                // get the effective start point of the list 
                IsoPlotObjectSegment firstSeg = tmpList[0];
                if (firstSeg.ReverseOnConversionToGCode == true)
                {
                    // we are normal, the end is XY0
                    effectiveStart_X = firstSeg.X0;
                    effectiveStart_Y = firstSeg.Y0;
                }
                else
                {
                    // we are reversed, the end is XY1
                    effectiveStart_X = firstSeg.X1;
                    effectiveStart_Y = firstSeg.Y1;
                }
                // get the distance
                float dist = MiscGraphicsUtils.GetDistanceBetweenTwoPoints(xVal, yVal, effectiveStart_X, effectiveStart_Y);
                // is it the closest one we have seen?
                if (dist < minDist)
                {
                    minDist = dist;
                    segIndex = i;
                }
            }
            // return what we have. It may not be apprpriate
            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Finds the nearest unchanged segment to a specfied point. We only look at
        /// start points.
        /// </summary>
        /// <param name="xVal">the xValue</param>
        /// <param name="yVal">the yValue</param>
        /// <returns>nearest segment or null for none found</returns>
        private IsoPlotObjectSegment GetUnchainedSegmentNearestPoint(int xVal, int yVal)
        {
            float minDist = float.MaxValue;
            IsoPlotObjectSegment outSeg = null;

            // look at each segment. Find the one closest to the specified point
            foreach (IsoPlotObjectSegment segObj in isoPlotStep3List)
            {
                // do we need to consider this one?
                if (segObj.SegmentChained == true)
                {
                    // no we do not, it will have been added
                    continue;
                }
                if (segObj.IsDuplicate == true)
                {
                    // discard, we do not use this one
                    continue;
                }
                // we only calculate the distance to the start point
                float tmpDist = MiscGraphicsUtils.GetDistanceBetweenTwoPoints(xVal, yVal, segObj.X0, segObj.Y0);
                if (tmpDist < minDist)
                {
                    minDist = tmpDist;
                    outSeg = segObj;
                }
            }
            // return what we found
            return outSeg;
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Dumps a isoplot list to the log for diagnostic purposes. 
        /// </summary>
        /// <param name="wantDups">if true we dump the duplicates as well</param>
        private void DumpIsoPlotSegmentList(List<IsoPlotObjectSegment> isoSegList, string headerLine, bool wantDups)
        {
            if (headerLine == null) headerLine = "";
            LogMessage(headerLine);
            foreach (IsoPlotObjectSegment isoSeg in isoSegList)
            {
                if ((wantDups==false) && (isoSeg.IsDuplicate==true)) continue;
                // dump the object
                LogMessage(IsoPlotObjectSegment.GetIsoPlotSegmentDump(isoSeg, "  "));
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Puts all IsoPlotSegment_Line objects in the input list into the output list
        /// and sorts that list by size before returning it.
        /// </summary>
        /// <param name="minSize">the minimum size of the IsoSegmentLength we permit</param>
        /// <param name="outsideChainList">the input list of IsoPlotSegment objects</param>
        /// <param name="stateMachine">the state machine to use for the conversions</param>
        /// <returns>a list of IsoPlotSegment_Line objects sorted by size or empty list for fail</returns>
        private List<IsoPlotObjectSegment_Line> GetIsoPlotLineSegmentsOrderedBySize(GCodeFileStateMachine stateMachine, List<IsoPlotObjectSegment> outsideChainList, float minSize)
        {
            List<IsoPlotObjectSegment_Line> retList = new List<IsoPlotObjectSegment_Line>();

            // sanity check
            if (outsideChainList == null) return retList;

            // add the lines
            foreach (IsoPlotObjectSegment segObj in outsideChainList)
            {
                // has to be a line
                if ((segObj is IsoPlotObjectSegment_Line) == false) continue;
                // we must calculate the segment length
                (segObj as IsoPlotObjectSegment_Line).CalcIsoSegmentLength(stateMachine);
                if (minSize > 0)
                {
                    if ((segObj as IsoPlotObjectSegment_Line).IsoSegmentLength < minSize) continue;
                }
                // we are good to add
                retList.Add((segObj as IsoPlotObjectSegment_Line));
            }
            // now we have to sort it on the IsoSegmentLength
            retList.Sort(IsoPlotObjectSegment_Line.LengthComparison);

            return retList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tests the input chain to see if any of the IsoPlotSegments it contains
        /// exceeds the boundaries of the input parameters
        /// </summary>
        /// <returns>true - at least one dimension exceeded, false none exceeded</returns>
        private bool TestChainForMaxAndMin(List<IsoPlotObjectSegment> chainList, ref int minX, ref int minY, ref int maxX, ref int maxY)
        {
            bool coordUpdated = false;
            // sanity check
            if (chainList == null) return false;

            // run through the list, checking to see if any exceed
            foreach (IsoPlotObjectSegment segObj in chainList)
            {
                if ((segObj is IsoPlotObjectSegment_Line) == true)
                {
                    if ((segObj as IsoPlotObjectSegment_Line).X0 < minX)
                    {
                        minX = (segObj as IsoPlotObjectSegment_Line).X0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotObjectSegment_Line).Y0 < minY)
                    {
                        minY = (segObj as IsoPlotObjectSegment_Line).Y0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotObjectSegment_Line).X0 > maxX)
                    {
                        maxX = (segObj as IsoPlotObjectSegment_Line).X0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotObjectSegment_Line).Y0 > maxY)
                    {
                        maxY = (segObj as IsoPlotObjectSegment_Line).Y0;
                        coordUpdated = true;
                    }
                }
                else if ((segObj is IsoPlotObjectSegment_Arc) == true)
                {
                    if ((segObj as IsoPlotObjectSegment_Arc).X0 < minX)
                    {
                        minX = (segObj as IsoPlotObjectSegment_Arc).X0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotObjectSegment_Arc).Y0 < minY)
                    {
                        minY = (segObj as IsoPlotObjectSegment_Arc).Y0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotObjectSegment_Arc).X0 > maxX)
                    {
                        maxX = (segObj as IsoPlotObjectSegment_Arc).X0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotObjectSegment_Arc).Y0 > maxY)
                    {
                        maxY = (segObj as IsoPlotObjectSegment_Arc).Y0;
                        coordUpdated = true;
                    }
                }
            }
            return coordUpdated;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tests the input chain to see if it contains at least one line
        /// </summary>
        /// <returns>true - at least one dimension exceeded, false none exceeded</returns>
        private bool DoesChainContainALineSegment(List<IsoPlotObjectSegment> chainList)
        {
            // sanity check
            if (chainList == null) return false;

            // run through the list, checking to see if any exceed
            foreach (IsoPlotObjectSegment segObj in chainList)
            {
                if ((segObj is IsoPlotObjectSegment_Line) == true) return true;
            }
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a list of all isoplot segments for a builderIDTag. Will not collect the 
        /// chained ones.
        /// </summary>
        /// <param name="builderIDTag">the tag</param>
        /// <param name="checkEnd">if true we check the end of the segments, false we check the start</param>
        /// <returns>A list of matching segments, or empty list if not found</returns>
        private List<IsoPlotObjectSegment> GetUnchainedIsoSegListByBuilderID(int builderIDTag, bool checkEnd)
        {
            List<IsoPlotObjectSegment> outlist = new List<IsoPlotObjectSegment>();

            foreach (IsoPlotObjectSegment segObj in isoPlotStep3List)
            {
                if (checkEnd == true)
                {
                    // check the start
                    if (segObj.XY1Overlay == null) continue;
                    // do we even contain the tag
                    if (segObj.XY1Overlay.DoesOverlayContainTag(builderIDTag) == false) continue;
                    // ok, we have this tag are we already chained here, do not consider this if it is already used
                    if (segObj.SegmentChained == true) continue;
                    // ok, not chained but are we a duplicate?
                    if (segObj.IsDuplicate == true) continue;
                    // this one is good, remember it
                    outlist.Add(segObj);
                }
                else
                {
                    // check the start
                    if (segObj.XY0Overlay == null) continue;
                    // do we even contain the tag
                    if (segObj.XY0Overlay.DoesOverlayContainTag(builderIDTag) == false) continue;
                    // ok, we have this tag are we already chained here, do not consider this if it is already used
                    if (segObj.SegmentChained == true) continue;
                    // ok, not chained but are we a duplicate?
                    if (segObj.IsDuplicate == true) continue;
                    // this one is good, remember it
                    outlist.Add(segObj);
                }
            }
            // ran off the end, return what we got
            return outlist;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the next best IsoPlot segment from a point and overlay for that point.
        /// We pass in the overlay because we have it. We could just get it from the 
        /// point as well.
        /// </summary>
        /// <param name="xVal">the x coord</param>
        /// <param name="yVal">the y coord</param>
        /// <param name="overlayForPoint">the overlay for the above xy coords</param>
        /// <param name="minDist">the distance is returned here</param>
        /// <param name="minSegNeedsReversal">if true we picked up on an end point rather than a start point</param>
        /// <param name="minSegObj">the isoSeg for the point we found</param>
        /// <returns>z success, nz fail</returns>
        private int GetNextBestIsoSegFromPointByOverlay(int xVal, int yVal, Overlay overlayForPoint, out IsoPlotObjectSegment minSegObj, out bool minSegNeedsReversal, out float minDist)
        {
            List<IsoPlotObjectSegment> segObjList_StartPoints = null;
            List<IsoPlotObjectSegment> segObjList_EndPoints = null;

            minDist = float.MaxValue;
            minSegObj = null;
            minSegNeedsReversal = false;

            if (xVal < 0) return -1;
            if (yVal < 0) return -1;
            if (overlayForPoint == null) return -1;

            // look at each IsoPlotSegment represented in the isoObjForPoint and see if we can 
            // find one with a start or end point close to the input x,y (within reason)
            // the iterator here gets us the values in the overlay dict - ie the builder ID with flags (aka the tag)
            foreach(int builderIDTag in overlayForPoint)
            {
                // is the builderID a background? if so do not consider it
                if (Overlay.TagIsBackgroundUsage(builderIDTag) == true) continue;
                // get a list of all isoplotsegments that start with this tag and are not already used
                segObjList_StartPoints = GetUnchainedIsoSegListByBuilderID(builderIDTag, false);
                // loop through and calc distance to point
                foreach (IsoPlotObjectSegment segObj in segObjList_StartPoints)
                {
                    // get the distance
                    float dist = MiscGraphicsUtils.GetDistanceBetweenTwoPoints(xVal, yVal, segObj.X0, segObj.Y0);
                    // is it the closest one we have seen?
                    if(dist < minDist)
                    {
                        minDist = dist;
                        minSegObj = segObj;
                        minSegNeedsReversal = false;
                    }
                    // if we actually hit one exactly just return now and use that, no point in carrying on
                    if (dist == 0) return 0;
                }
                // get a list of all isoplotsegments that end with this tag and are not already used
                segObjList_EndPoints = GetUnchainedIsoSegListByBuilderID(builderIDTag, true);
                // loop through and calc distance to point
                foreach (IsoPlotObjectSegment segObj in segObjList_EndPoints)
                {
                    // get the distance
                    float dist = MiscGraphicsUtils.GetDistanceBetweenTwoPoints(xVal, yVal, segObj.X1, segObj.Y1);
                    // is it the closest one we have seen?
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minSegObj = segObj;
                        minSegNeedsReversal = true;
                    }
                    // if we actually hit one exactly just return now and use that, no point in carrying on
                    if (dist == 0) return 0;
                }
            }

            //DebugMessage("looking for (" + xVal.ToString() + "," + yVal.ToString() + ")");
            //DumpIsoPlotSegmentList(segObjList_StartPoints, "segObjList_StartPoints");
            //DumpIsoPlotSegmentList(segObjList_EndPoints, "segObjList_EndPoints");

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the next best IsoPlot segment from a point.
        /// </summary>
        /// <param name="xVal">the x coord</param>
        /// <param name="yVal">the y coord</param>
        /// <param name="minDist">the distance is returned here</param>
        /// <param name="minSegNeedsReversal">if true we picked up on an end point rather than a start point</param>
        /// <param name="minSegObj">the isoSeg for the point we found</param>
        /// <returns>z success, nz fail</returns>
        private int GetNextBestIsoSegFromPointByDistance(int xVal, int yVal, out IsoPlotObjectSegment minSegObj, out bool minSegNeedsReversal, out float minDist)
        {
            minDist = float.MaxValue;
            minSegObj = null;
            minSegNeedsReversal = false;
            float tmpDist = 0;

            if (xVal < 0) return -1;
            if (yVal < 0) return -1;

            // look at each segment. Find the one closest to the specified point
            foreach (IsoPlotObjectSegment segObj in isoPlotStep3List)
            {
                // do we need to consider this one?
                if (segObj.SegmentChained == true)
                {
                    // no we do not, it will have been added
                    continue;
                }
                if (segObj.IsDuplicate == true)
                {
                    // discard, we do not use this one
                    continue;
                }
                // we calculate the distance to the start point
                tmpDist = MiscGraphicsUtils.GetDistanceBetweenTwoPoints(xVal, yVal, segObj.X0, segObj.Y0);
                if (tmpDist < minDist)
                {
                    minDist = tmpDist;
                    minSegObj = segObj;
                    minSegNeedsReversal = false;
                }
                // we calculate the distance to the end point
                tmpDist = MiscGraphicsUtils.GetDistanceBetweenTwoPoints(xVal, yVal, segObj.X1, segObj.Y1);
                if (tmpDist < minDist)
                {
                    minDist = tmpDist;
                    minSegObj = segObj;
                    minSegNeedsReversal = true;
                }
            }
            // return what we got
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Marks duplicate isoSegs.
        /// </summary>
        /// <returns>the count of duplicates </returns>
        private int MarkDuplicateSegs()
        {
            int dupCount = 0;

            // do all the segs.
            for(int i=0; i< isoPlotStep3List.Count;i++)
            {
                IsoPlotObjectSegment workingIsoSeg = isoPlotStep3List[i];
                // we only check for duplicates on arcs
                if ((workingIsoSeg is IsoPlotObjectSegment_Arc) == false) continue;

                // compare against the remainder of the array
                for (int j = i+1; j < isoPlotStep3List.Count; j++)
                {
                    IsoPlotObjectSegment tmpIsoSeg = isoPlotStep3List[j];
                    // we only check for duplicates on arcs
                    if ((tmpIsoSeg is IsoPlotObjectSegment_Arc) == false) continue;
                    // are we already a duplicate?
                    if (tmpIsoSeg.IsDuplicate == true) continue;

                    // check the start and end coords
                    if (workingIsoSeg.X0 != tmpIsoSeg.X0) continue;
                    if (workingIsoSeg.Y0 != tmpIsoSeg.Y0) continue;
                    if (workingIsoSeg.X1 != tmpIsoSeg.X1) continue;
                    if (workingIsoSeg.Y1 != tmpIsoSeg.Y1) continue;

                    // we have a duplicate, mark it
                    tmpIsoSeg.IsDuplicate = true;
                    // count it
                    dupCount++;

                    // dump it to the log
                    //string workingStr = IsoPlotObjectSegment.GetIsoPlotSegmentDump(workingIsoSeg, "  ");
                    //string tmpStr = IsoPlotObjectSegment.GetIsoPlotSegmentDump(tmpIsoSeg, "  ");
                    //DebugMessage("Duplicate ISOSEG");
                    //DebugMessage(workingStr);
                    //DebugMessage(tmpStr);
                }
            }
            return dupCount;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Counts all isoplot segments in a list of chains
        /// 
        /// </summary>
        /// <param name="listOfChains">the list of chains</param>
        /// <returns>the count</returns>
        private int CountSegsInAllChains(List<List<IsoPlotObjectSegment>> listOfChains)
        {
            int outCount = 0;
            if (listOfChains == null) return 0;

            foreach(List<IsoPlotObjectSegment> chainObj in listOfChains)
            {
                outCount += chainObj.Count;
            }
            return outCount;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the next IsoPlot segment in a chain from an existing isoPlotSeg.
        /// Note we always chain of the end point of the input segment. This particular
        /// code only fetches the next segment if it shares a builderID in the overlay.
        /// 
        /// Any segment returned here MUST be added to a chain. Its internal 
        /// configuration will be set to support this.
        /// 
        /// </summary>
        /// <param name="startSeg">the start segment</param>
        /// <param name="boardType">the type of board we are processing</param>
        /// <returns>next isoplot segment or null for fail</returns>
        private IsoPlotObjectSegment GetNextIsoSegmentInChainViaOverlayorDistance(IsoPlotObjectSegment startSeg, FileManager.OperationModeEnum boardType)
        {
            IsoPlotObjectSegment nearestSegObj = null;
            bool nearestSegNeedsReversal = false;
            float pointDist = float.MaxValue;
            Overlay overlayForPoint = null;
            int xVal = 0;
            int yVal = 0;

            if (startSeg == null) return null;

            // we chain off the end point but if the segment is 
            // reversed we have touse the start point
            if(startSeg.ReverseOnConversionToGCode==true)
            {
                // feed in the start segment
                xVal = startSeg.X0;
                yVal = startSeg.Y0;
                overlayForPoint = startSeg.XY0Overlay;
            }
            else
            {
                // feed in the end segment
                xVal = startSeg.X1;
                yVal = startSeg.Y1;
                overlayForPoint = startSeg.XY1Overlay;
            }

            //DebugMessage("sstartSeg for " + IsoPlotObjectSegment.GetIsoPlotSegmentDump(startSeg, ""));
            float maxSegmentGap_Overlay = GetChainMaxSegmentGap_Overlay(boardType);
            float maxSegmentGap_Distance = GetChainMaxSegmentGap_Distance(boardType);

            // we have the a start segment, get the next best segment which shares its end point
            GetNextBestIsoSegFromPointByOverlay(xVal, yVal, overlayForPoint, out nearestSegObj, out nearestSegNeedsReversal, out pointDist);
            // is it null or is the actual start of the segment point further away than we can accept?
            // this can happen because we may pick up the segment at some mid point. Usually
            // this is just a few isocells away from the start (due to rounding error)
            // but we cannot assume that.
            if ((nearestSegObj==null) || (pointDist > maxSegmentGap_Overlay))
            {
                // point is unacceptable, try to get it via distance
                GetNextBestIsoSegFromPointByDistance(xVal, yVal, out nearestSegObj, out nearestSegNeedsReversal, out pointDist);
                if ((nearestSegObj == null) || (pointDist > maxSegmentGap_Distance))
                {
                    // no segment, return what we got
                    return null;
                }
                // we are going to use this one set this value now (for debugging)
                nearestSegObj.ChainedViaDistance = true;
            }
 
            // we did find one. Either by overlay (preferred) or by simple distance

            // we are going to use it so set it up
            if (nearestSegNeedsReversal == true)
            {
                nearestSegObj.ReverseOnConversionToGCode = true;
                nearestSegObj.PointIsChainTarget_1 = true;
            }
            else
            {
                nearestSegObj.ReverseOnConversionToGCode = false;
                nearestSegObj.PointIsChainTarget_0 = true;
            }
            // remember the effective GCode start point
            nearestSegObj.ChainedStart_X = xVal;
            nearestSegObj.ChainedStart_Y = yVal;

            // return it, it is set up
            return nearestSegObj;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a complete chain for a starting isoPlotSeg. This segment is assumed
        /// not to be in other chains
        /// 
        /// </summary>
        /// <param name="isoSegToChain">the start segment</param>
        /// <param name="boardType">the type of board we are processing</param>
        /// <returns>list of chained isoplot segs or null for fail</returns>
        private List<IsoPlotObjectSegment> GetIsolationChainForSegment(IsoPlotObjectSegment isoSegToChain, FileManager.OperationModeEnum boardType)
        {
            List<IsoPlotObjectSegment> chainList = new List<IsoPlotObjectSegment>();

            IsoPlotObjectSegment workingSeg = null;

            if (isoSegToChain == null) return chainList;

            // set it up, since this is the top of the chain we are not reversing
            isoSegToChain.ReverseOnConversionToGCode = false;
            // our gcode start point will be the segments start point
            isoSegToChain.ChainedStart_X = isoSegToChain.X0;
            isoSegToChain.ChainedStart_Y = isoSegToChain.Y0;
            // since we are the start of the chain we can assume our start point is a chain target
            // otherwise we could just loop around onto ourselves. Nobody should chain onto us
            isoSegToChain.PointIsChainTarget_0 = true;
            // always add our first segment 
            chainList.Add(isoSegToChain);

            // init this
            workingSeg = isoSegToChain;

            // get the next segments
            for (int i = 0; i < MAX_ISOSEGMENTS_IN_CHAIN; i++)
            {
                // get the next segment in the chain
                IsoPlotObjectSegment nextSegment = GetNextIsoSegmentInChainViaOverlayorDistance(workingSeg, boardType);

                // not found, just return the list we have
                if (nextSegment == null) return chainList;

                // we did find one, add it
                chainList.Add(nextSegment);
                // set up to loop around
                workingSeg = nextSegment;
                // loop around and try to pick up via overlay again
                continue;
            }
            // we should never get here. However if we hit MAX_ISOSEGMENTS_IN_CHAIN it is 
            // not a problem we will just put the remaining segments in another chain
            LogMessage("GetIsolationChainForSegment MAX_ISOSEGMENTS_IN_CHAIN limit hit");

            return chainList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs processing on the isolation array to figure out the isolation segments
        /// from the isoPlotObjects in the overlays. We essentially just run over
        /// the object again and pay attention to those segments which have isoCells 
        /// not in the DISREGARD state.
        /// </summary>
        /// <remarks>The completion of this step is the end of isoStep3</remarks>
        /// <returns>z succes, nz fail</returns>
        /// <param name="errStr">a helpful error message</param>
        public int PerformIsoPlotSegmentDiscovery(ref string errStr)
        {
            Point point1;
            Point point2;
            Point ptUL;
            Point ptLL;
            Point ptLR;
            Point ptUR;
            List<IsoPlotObjectSegment> tmpList = null;
            int retInt;

            LogMessage("PerformIsoPlotSegmentDiscovery called");

            // first we do all the lines.
            foreach (IsoPlotObject isoPlotObject in isoPlotObjList)
            {
                if((isoPlotObject is IsoPlotObject_Line)==false) continue;
                // perform the cast, this makes it easier to read
                IsoPlotObject_Line tmpLine = (IsoPlotObject_Line)isoPlotObject;

                //DebugMessage("Isolation Discovery on: " + isoPlotObject.IsoPlotObjectID.ToString());

                // the input value is actually the center line of a line that has width. Figure
                // out the perpendicular endpoints now
                retInt = MiscGraphicsUtils.GetWideLineEndPoints(tmpLine.X0, tmpLine.Y0, tmpLine.X1, tmpLine.Y1, tmpLine.Width, out ptLL, out ptUL, out ptUR, out ptLR);
                if (retInt != 0)
                {
                    errStr=" call to GetWideLineEndPoints returned " + retInt.ToString();
                    LogMessage("PerformIsoPlotSegmentDiscovery " + errStr);
                    return 0;
                }

                // now we have the endpoints of the rectangle we run over the edges
                // again looking for isoPlot segments

                // SegA
                tmpList = GetIsoPlotSegmentsFromGSLine(isoPlotObject.IsoPlotObjectID, ptUL.X, ptUL.Y, ptLL.X, ptLL.Y);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                }
                foreach (IsoPlotObjectSegment isoSeg in tmpList)
                {
                    if((isoSeg is IsoPlotObjectSegment_Line)==false) continue;
                    //DebugMessage("segA="+(isoLine as IsoPlotSegment_Line).ToString());
                    IsoPlotStep3List.Add(isoSeg);
                }

                // SegB
                tmpList = GetIsoPlotSegmentsFromGSLine(isoPlotObject.IsoPlotObjectID, ptLL.X, ptLL.Y, ptLR.X, ptLR.Y);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                }
                foreach (IsoPlotObjectSegment isoSeg in tmpList)
                {
                    if ((isoSeg is IsoPlotObjectSegment_Line) == false) continue;
                    //DebugMessage("segB=" + (isoLine as IsoPlotSegment_Line).ToString());
                    IsoPlotStep3List.Add(isoSeg);
                }

                // SegC
                tmpList = GetIsoPlotSegmentsFromGSLine(isoPlotObject.IsoPlotObjectID, ptLR.X, ptLR.Y, ptUR.X, ptUR.Y);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                }
                foreach (IsoPlotObjectSegment isoSeg in tmpList)
                {
                    if ((isoSeg is IsoPlotObjectSegment_Line) == false) continue;
                    //DebugMessage("segC=" + (isoLine as IsoPlotSegment_Line).ToString());
                    IsoPlotStep3List.Add(isoSeg);
                }

                // SegD
                tmpList = GetIsoPlotSegmentsFromGSLine(isoPlotObject.IsoPlotObjectID, ptUR.X, ptUR.Y, ptUL.X, ptUL.Y);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                }
                foreach (IsoPlotObjectSegment isoSeg in tmpList)
                {
                    if ((isoSeg is IsoPlotObjectSegment_Line) == false) continue;
                    //DebugMessage("segD=" + (isoLine as IsoPlotSegment_Line).ToString());
                    IsoPlotStep3List.Add(isoSeg);
                }
            } // bottom of  foreach (IsoPlotObject isoPlotObject in isoPlotObjList)

            // next we do all the polygons
            foreach (IsoPlotObject isoPlotObject in isoPlotObjList)
            {
                if ((isoPlotObject is IsoPlotObject_Polygon) == false) continue;
                // perform the cast, this makes it easier to read
                IsoPlotObject_Polygon tmpPoly = (IsoPlotObject_Polygon)isoPlotObject;

                //DebugMessage("Isolation Discovery on: " + isoPlotObject.IsoPlotObjectID.ToString());

                // get the vertexes
                System.Drawing.PointF[] vertexPoints = MiscGraphicsUtils.CalcPolygonVertexPoints((float)tmpPoly.X0, (float)tmpPoly.Y0, tmpPoly.NumSides, (float)tmpPoly.OuterDiameter, (float)tmpPoly.RotationAngleInDegrees);
                // convert it to ints now
                System.Drawing.Point[] lineEndPoints = MiscGraphicsUtils.ConvertPointFArrayToPoint(vertexPoints);

                // now we have the vertexes of the polygon we run over the edges
                // again looking for isoPlot segments

                // process each pair of points in the array. Looping back around when at the end.
                int lastIndex = lineEndPoints.Length - 1;
                for (int i = 0; i < lineEndPoints.Length; i++)
                {
                    // get the two points we draw
                    point1 = lineEndPoints[i];
                    if (i < lastIndex) point2 = lineEndPoints[i + 1];
                    else { point2 = lineEndPoints[0]; }

                    tmpList = GetIsoPlotSegmentsFromGSLine(isoPlotObject.IsoPlotObjectID, point1.X, point1.Y, point2.X, point2.Y);
                    if (tmpList == null)
                    {
                        // this should not happen
                        throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                    }
                    foreach (IsoPlotObjectSegment isoSeg in tmpList)
                    {
                        if ((isoSeg is IsoPlotObjectSegment_Line) == false) continue;
                        //DebugMessage("segA="+(isoLine as IsoPlotSegment_Line).ToString());
                        IsoPlotStep3List.Add(isoSeg);
                    }
                }
            } // bottom of  foreach (IsoPlotObject isoPlotObject in isoPlotObjList)

            // next we do all the outlines
            foreach (IsoPlotObject isoPlotObject in isoPlotObjList)
            {
                if ((isoPlotObject is IsoPlotObject_Outline) == false) continue;
                // perform the cast, this makes it easier to read
                IsoPlotObject_Outline tmpOutline = (IsoPlotObject_Outline)isoPlotObject;

                //DebugMessage("Isolation Discovery on: " + isoPlotObject.IsoPlotObjectID.ToString());

                // get the vertexes, these are already converted to ints when they were stored
                System.Drawing.Point[] lineEndPoints = tmpOutline.PointArray;

                // now we have the vertexes of the outline we run over the edges
                // again looking for isoPlot segments

                // process each pair of points in the array. Looping back around when at the end.
                int lastIndex = lineEndPoints.Length - 1;
                for (int i = 0; i < lineEndPoints.Length; i++)
                {
                    // get the two points we draw
                    point1 = lineEndPoints[i];
                    if (i < lastIndex) point2 = lineEndPoints[i + 1];
                    else { point2 = lineEndPoints[0]; }

                    tmpList = GetIsoPlotSegmentsFromGSLine(isoPlotObject.IsoPlotObjectID, point1.X, point1.Y, point2.X, point2.Y);
                    if (tmpList == null)
                    {
                        // this should not happen
                        throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                    }
                    foreach (IsoPlotObjectSegment isoSeg in tmpList)
                    {
                        if ((isoSeg is IsoPlotObjectSegment_Line) == false) continue;
                        //DebugMessage("segA="+(isoLine as IsoPlotSegment_Line).ToString());
                        IsoPlotStep3List.Add(isoSeg);
                    }
                }
            } // bottom of  foreach (IsoPlotObject isoPlotObject in isoPlotObjList)

            // process the circles
            foreach (IsoPlotObject isoPlotObject in isoPlotObjList)
            {
                if ((isoPlotObject is IsoPlotObject_Circle) == false) continue;
                // perform the cast, this makes it easier to read
                IsoPlotObject_Circle tmpCircle = (IsoPlotObject_Circle)isoPlotObject;

                //DebugMessage("Isolation Discovery on: " + isoPlotObject.IsoPlotObjectID.ToString());

                tmpList = GetIsoPlotSegmentsFromGSCircle(isoPlotObject.IsoPlotObjectID, tmpCircle.X0, tmpCircle.Y0, tmpCircle.Radius, tmpCircle.WantClockWise, tmpCircle.IsMultiQuadrantArc);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSCircle");
                }
                foreach (IsoPlotObjectSegment isoSeg in tmpList)
                {
                    if ((isoSeg is IsoPlotObjectSegment_Arc) == false) continue;
                    //DebugMessage("segE=" + (isoLine as IsoPlotSegment_Arc).ToString());
                    IsoPlotStep3List.Add(isoSeg);
                }
            }

            // process the arcs
            foreach (IsoPlotObject isoPlotObject in isoPlotObjList)
            {
                if ((isoPlotObject is IsoPlotObject_Arc) == false) continue;
                // perform the cast, this makes it easier to read
                IsoPlotObject_Arc tmpArc = (IsoPlotObject_Arc)isoPlotObject;

                //DebugMessage("Isolation Discovery on: " + isoPlotObject.IsoPlotObjectID.ToString());
                // get the segments for the outer arc
                int outerRadius = tmpArc.GetOuterRadius();
                // we know the outer radius is good or the arc object would not be present
                tmpList = GetIsoPlotSegmentsFromGSCircle(isoPlotObject.IsoPlotObjectID, tmpArc.XCenter, tmpArc.YCenter, outerRadius, tmpArc.WantClockWise, tmpArc.IsMultiQuadrantArc);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list (a) returned from call to GetIsoPlotSegmentsFromGSArc");
                }
                foreach (IsoPlotObjectSegment isoArc in tmpList)
                {
                    if ((isoArc is IsoPlotObjectSegment_Arc) == false) continue;
                    //DebugMessage("segE=" + (isoLine as IsoPlotSegment_Arc).ToString());
                    IsoPlotStep3List.Add(isoArc);
                }

                // get the segments for the inner arc
                int innerRadius = tmpArc.GetInnerRadius();
                // is it sensible?
                if (innerRadius >= 0)
                {
                    tmpList = GetIsoPlotSegmentsFromGSCircle(isoPlotObject.IsoPlotObjectID, tmpArc.XCenter, tmpArc.YCenter, innerRadius, tmpArc.WantClockWise, tmpArc.IsMultiQuadrantArc);
                    if (tmpList == null)
                    {
                        // this should not happen
                        throw new Exception(" null GCode list (b) returned from call to GetIsoPlotSegmentsFromGSArc");
                    }
                    foreach (IsoPlotObjectSegment isoArc in tmpList)
                    {
                        if ((isoArc is IsoPlotObjectSegment_Arc) == false) continue;
                        //DebugMessage("segE=" + (isoLine as IsoPlotSegment_Arc).ToString());
                        IsoPlotStep3List.Add(isoArc);
                    }
                    // do the segment between the start points
                    tmpList = GetIsoPlotSegmentsFromGSLine(isoPlotObject.IsoPlotObjectID, tmpArc.InnerFirstX, tmpArc.InnerFirstY, tmpArc.OuterFirstX, tmpArc.OuterFirstY);
                    if (tmpList == null)
                    {
                        // this should not happen
                        throw new Exception(" null GCode list (c) returned from call to GetIsoPlotSegmentsFromGSLine");
                    }
                    foreach (IsoPlotObjectSegment isoSeg in tmpList)
                    {
                        if ((isoSeg is IsoPlotObjectSegment_Line) == false) continue;
                        //DebugMessage("segC=" + (isoLine as IsoPlotSegment_Line).ToString());
                        IsoPlotStep3List.Add(isoSeg);
                    }
                    // do the segment between the end points
                    tmpList = GetIsoPlotSegmentsFromGSLine(isoPlotObject.IsoPlotObjectID, tmpArc.InnerLastX, tmpArc.InnerLastY, tmpArc.OuterLastX, tmpArc.OuterLastY);
                    if (tmpList == null)
                    {
                        // this should not happen
                        throw new Exception(" null GCode list (d) returned from call to GetIsoPlotSegmentsFromGSLine");
                    }
                    foreach (IsoPlotObjectSegment isoSeg in tmpList)
                    {
                        if ((isoSeg is IsoPlotObjectSegment_Line) == false) continue;
                        //DebugMessage("segC=" + (isoLine as IsoPlotSegment_Line).ToString());
                        IsoPlotStep3List.Add(isoSeg);
                    }
                }
            }

            // now we do all the contour lines.
            foreach (IsoPlotObject isoPlotObject in isoPlotObjList)
            {
                if ((isoPlotObject is IsoPlotObject_ContourLine) == false) continue;
                // perform the cast, this makes it easier to read
                IsoPlotObject_ContourLine tmpContourLine = (IsoPlotObject_ContourLine)isoPlotObject;
                //DebugMessage("Isolation Discovery on (ContourLine): " + isoPlotObject.IsoPlotObjectID.ToString());

                // the input value is a line, just get the isoplot segments from it
                tmpList = GetIsoPlotSegmentsFromGSLine(isoPlotObject.IsoPlotObjectID, tmpContourLine.X0, tmpContourLine.Y0, tmpContourLine.X1, tmpContourLine.Y1);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception("ContourLine, null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                }
                foreach (IsoPlotObjectSegment isoSeg in tmpList)
                {
                    if ((isoSeg is IsoPlotObjectSegment_Line) == false) continue;
                    //DebugMessage("ContourLinesegA="+(isoLine as IsoPlotSegment_Line).ToString());
                    IsoPlotStep3List.Add(isoSeg);
                }
            } // bottom of  foreach (IsoPlotObject isoPlotObject in isoPlotObjList)

            // now we do all the contour arcs.
            foreach (IsoPlotObject isoPlotObject in isoPlotObjList)
            {
                if ((isoPlotObject is IsoPlotObject_ContourArc) == false) continue;
                // perform the cast, this makes it easier to read
                IsoPlotObject_ContourArc tmpContourArc = (IsoPlotObject_ContourArc)isoPlotObject;
                //DebugMessage("Isolation Discovery on (ContourArc): " + isoPlotObject.IsoPlotObjectID.ToString());

                // the input value is a arc, just get the isoplot segments from it
                tmpList = GetIsoPlotSegmentsFromGSCircle(isoPlotObject.IsoPlotObjectID, tmpContourArc.XCenter, tmpContourArc.YCenter, tmpContourArc.Radius, tmpContourArc.WantClockWise, tmpContourArc.IsMultiQuadrantArc);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list (q) returned from call to GetIsoPlotSegmentsFromGSCircle");
                }
                foreach (IsoPlotObjectSegment isoArc in tmpList)
                {
                    if ((isoArc is IsoPlotObjectSegment_Arc) == false) continue;
                    //DebugMessage("segE=" + (isoLine as IsoPlotSegment_Arc).ToString());
                    IsoPlotStep3List.Add(isoArc);
                }

            } // bottom of  foreach (IsoPlotObject isoPlotObject in isoPlotObjList)

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs processing on the isolation array to flag isoCells belonging
        /// to a isoPlotObject which are in the interior of other objects and hence 
        /// removes them from consideration during subsequent processing.
        /// </summary>
        /// <remarks>The completion of this step is the end of isoStep2. The
        /// isoPlot is expected to be in isoStep1 when this is called.</remarks>
        /// <returns>z succes, nz fail</returns>
        /// <param name="errStr">a helpful error message</param>
        public int PerformIsolationPlotInteriorIsoCellRemoval(ref string errStr)
        {
            int overlayID = 0;
            errStr = "";
            int overlayCount;
            int backgroundUsageCount;
            int normalEdgeUsageCount;
            int contourEdgeUsageCount;
            int invertEdgeUsageCount;

            for (int y = 0; y < isoPlotHeight; y++)
            {
                for (int x = 0; x < isoPlotWidth; x++)
                {
                    // get the id of the overlay at the isolation plot point
                    overlayID = CleanFlagsFromOverlayID(isolationPlotArray[x, y]);
                    // is it an empty one
                    if(overlayID== OverlayCollection.EMPTY_OVERLAY_ID)
                    {
                        // yes it is, set the clean one and continue
                        isolationPlotArray[x, y] = overlayID;
                        continue;
                    }

                    // Get the overlay 
                    Overlay overlayObj = CurrentOverlayCollection[overlayID];
                    if (overlayObj == null)
                    {
                        // we should always be able to find this
                        LogMessage("PerformSecondaryIsolationArrayProcessing Cannot find overlay object for id=" + overlayID.ToString() + " x=" + x.ToString() + " y=" + y.ToString());
                        throw new Exception("Cannot find overlay object for id");
                    }

                    // get our counts from the overlay
                    overlayObj.GetAllCounts(out overlayCount, out backgroundUsageCount, out normalEdgeUsageCount, out contourEdgeUsageCount, out invertEdgeUsageCount);

                    // are we a background?
                    if(backgroundUsageCount>0)
                    {
                        // is the cell used as a background? most other uses are invalidated but invert edges still draw if they are on top of the background, they 
                        // do not draw if they are under the background since this means some subsequent trace was laid down after the invert got flashed
                        if (invertEdgeUsageCount > 0)
                        {
                            if (overlayObj.InvertEdgeIsOnTopOfBackground() == true)
                            {
                                // mark this 
                                isolationPlotArray[x, y] = overlayID | ((int)OverlayFlagEnum.OverlayFlag_INVERTEDGE) | ((int)OverlayFlagEnum.OverlayFlag_BACKGROUND); // note NO DISREGARD here
                                continue;
                            }
                            else
                            {
                                // at least one background is on top of the invert, this has the effect of being a normal background
                                isolationPlotArray[x, y] = overlayID | ((int)OverlayFlagEnum.OverlayFlag_DISREGARD) | ((int)OverlayFlagEnum.OverlayFlag_BACKGROUND);
                                continue;
                            }
                        }
                        // background usage invalidates any other kind of isoCell, mark all as disregard but flag the edge type
                        if (contourEdgeUsageCount > 0)
                        {
                            // contoure edges take precedence mark this 
                            isolationPlotArray[x, y] = overlayID | ((int)OverlayFlagEnum.OverlayFlag_DISREGARD) | ((int)OverlayFlagEnum.OverlayFlag_CONTOUREDGE) | ((int)OverlayFlagEnum.OverlayFlag_BACKGROUND);
                            continue;
                        }
                        if (normalEdgeUsageCount > 0)
                        {
                            // mark this 
                            isolationPlotArray[x, y] = overlayID | ((int)OverlayFlagEnum.OverlayFlag_DISREGARD) | ((int)OverlayFlagEnum.OverlayFlag_NORMALEDGE) | ((int)OverlayFlagEnum.OverlayFlag_BACKGROUND);
                            continue;
                        }
                        // just a normal background 
                        isolationPlotArray[x, y] = overlayID | ((int)OverlayFlagEnum.OverlayFlag_DISREGARD) | ((int)OverlayFlagEnum.OverlayFlag_BACKGROUND);
                        continue;
                    }

                    // here we know we are not a background

                    // are we an invert edge without a background? These only draw if on an edge. Invert edges need to draw ON something
                    if (invertEdgeUsageCount > 0)
                    {
                        // do we have any other edge usages here, if so we draw the the cell 
                        if (((normalEdgeUsageCount > 0) || (contourEdgeUsageCount > 0)) == true)
                        {
                            // mark this 
                            isolationPlotArray[x, y] = overlayID | ((int)OverlayFlagEnum.OverlayFlag_INVERTEDGE); // note no DISREGARD here
                            continue;
                        }
                        else
                        {
                            // we are an invert edge without a background so remove it
                            isolationPlotArray[x, y] = overlayID | ((int)OverlayFlagEnum.OverlayFlag_DISREGARD) | ((int)OverlayFlagEnum.OverlayFlag_INVERTEDGE);
                        }
                        continue;
                    }

                    // at this point we know the isoCell represents a non invert edge pixel (or pixels) and is not a background

                    if (contourEdgeUsageCount > 0)
                    {
                        // we are a contour, contours take precedence 
                        isolationPlotArray[x, y] = overlayID | ((int)OverlayFlagEnum.OverlayFlag_CONTOUREDGE); // note no DISREGARD here
                        continue;
                    }

                    if (normalEdgeUsageCount > 0)
                    {
                        // we are a normal edge 
                        isolationPlotArray[x, y] = overlayID | ((int)OverlayFlagEnum.OverlayFlag_NORMALEDGE); // note no DISREGARD here
                        continue;
                    }

                    // should never get here

                } // bottom of for (int x = 0; x < isoPlotWidth; x++)
            } // bottom of for (int y = 0; y < isoPlotHeight; y++)
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We get the tag for a specific builderID at an isoCell. This means
        /// looking  into the overlays 
        /// </summary>
        /// <param name="isoPlotObjIDToCheck">object id to check</param>
        /// <param name="x0">x coord</param>
        /// <param name="y0">y coord</param>
        /// <returns> an int which is the id of the IsoPlotObject and any flags (a tag) at that point
        /// or zero for not found</returns>
        private int GetTagForIsoPlotObjectAtIsoCell(int isoPlotObjIDToCheck, int x0, int y0)
        {
            if (x0 <= 0) return 0;
            if (y0 <= 0) return 0;
            if (x0 >= isoPlotWidth) return 0;
            if (y0 >= isoPlotHeight) return 0;

            // clean off the incoming id
            int cleanBuilderID = CleanFlagsFromBuilderID(isoPlotObjIDToCheck);

            // get the id of the overlay at the isolation plot point
            int overlayID = CleanFlagsFromOverlayID(isolationPlotArray[x0, y0]);
            // are we the empty overlay
            if (overlayID == OverlayCollection.EMPTY_OVERLAY_ID)                 
            {
                // we do not have a real overlay here
                return 0;
            }

            // we do have a proper overlay, check this now, just use the indexer
            // to see if it is there. 
            Overlay overlayObj = CurrentOverlayCollection[overlayID];
            if (overlayObj == null) return 0;

            // the indexer on the Overlay will look up the ID
            // and return the id with flags or zero if not found
            return overlayObj[cleanBuilderID];
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We get the tag for a specific builderID's at an isoCell. This means
        /// looking  into the overlays. This one accepts a list. We return on the 
        /// first one we find.
        /// </summary>
        /// <param name="isoPlotObjIDList">a list of builder ids to check</param>
        /// <param name="x0">x coord</param>
        /// <param name="y0">y coord</param>
        /// <returns> an int which is the id of the IsoPlotObject and any flags (the tag) at that point
        /// or zero for not found</returns>
        private int GetTagForIsoPlotObjectAtIsoCell(List<int> isoPlotObjIDList, int x0, int y0)
        {
            if (x0 <= 0) return 0;
            if (y0 <= 0) return 0;
            if (x0 >= isoPlotWidth) return 0;
            if (y0 >= isoPlotHeight) return 0;

            // get the id of the overlay at the isolation plot point
            int overlayID = CleanFlagsFromOverlayID(isolationPlotArray[x0, y0]);
            // are we the empty overlay
            if (overlayID == OverlayCollection.EMPTY_OVERLAY_ID)
            {
                // we do not have a real overlay here
                return 0;
            }

            // we do have a proper overlay, check this now, just use the indexer
            // to see if it is there. 
            Overlay overlayObj = CurrentOverlayCollection[overlayID];
            if (overlayObj == null) return 0;

            // loop through the contents of the list the first one we find we return
            foreach (int builderIDToCheck in isoPlotObjIDList)
            {
                // clean off the incoming id
                int cleanBuilderID = CleanFlagsFromBuilderID(builderIDToCheck);
                // the indexer on the Overlay will look up the ID
                // and return the id with flags or zero if not found
                int outTag = overlayObj[cleanBuilderID];
                // if we found one return it
                if (outTag > 0) return outTag;
            }

            // no tags were found for this list of builderIDs
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We get the tag for a builderID in an isoCell which does not match any in 
        /// a supplied list. This means looking  into the overlays. We return on the 
        /// first one we find.
        /// </summary>
        /// <param name="isoPlotObjIDList">a list of builder ids to check</param>
        /// <param name="altBuilderID">a builder ID in the list we also do not trigger on (-ve if not in use)</param>
        /// <param name="x0">x coord</param>
        /// <param name="y0">y coord</param>
        /// <returns> an int which is the id of the IsoPlotObject and any flags (the tag) at that point
        /// or zero for not found</returns>
        private int GetTagForIsoPlotObjectNotInListAtIsoCell(List<int> isoPlotObjIDList, int altBuilderID, int x0, int y0)
        {
            if (x0 <= 0) return 0;
            if (y0 <= 0) return 0;
            if (x0 >= isoPlotWidth) return 0;
            if (y0 >= isoPlotHeight) return 0;
            if (isoPlotObjIDList == null) return 0;

            // get the id of the overlay at the isolation plot point
            int overlayID = CleanFlagsFromOverlayID(isolationPlotArray[x0, y0]);
            // are we the empty overlay
            if (overlayID == OverlayCollection.EMPTY_OVERLAY_ID)
            {
                // we do not have a real overlay here
                return 0;
            }

            // we do have a proper overlay, check this now, just use the indexer
            // to see if it is there. 
            Overlay overlayObj = CurrentOverlayCollection[overlayID];
            if (overlayObj == null) return 0;

            // this returns everyting we need
            return overlayObj.GetFirstBuilderIDNotInList(isoPlotObjIDList, altBuilderID);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overlay IDs can have flags attached to them during post processing. This
        /// cleans them off and returns just the overlayID
        /// </summary>
        /// <param name="overlayIDIn">the overlay id</param>
        /// <returns>the overlay id with no flags</returns>
        public int CleanFlagsFromOverlayID(int overlayIDIn)
        {
            return overlayIDIn & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Builder IDs can have flags attached to them (BuilderIDTags). This
        /// cleans them off and returns just the builderID
        /// </summary>
        /// <param name="builderIDIn">the builder id</param>
        /// <returns>the builder id with no flags</returns>
        public int CleanFlagsFromBuilderID(int builderIDIn)
        {
            return builderIDIn & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Dumps the IsoPlotArray out to the logfile. Only dumps the non empty overlays
        /// </summary>
        public void DumpIsoPlotArrayToLog()
        {
            LogMessage("DumpIsoPlotArrayToLog Starts isoPlotHeight="+ isoPlotHeight.ToString() + ", isoPlotWidth="+ isoPlotWidth.ToString());
            for (int y = 0; y < isoPlotHeight; y++)
            {
                for (int x = 0; x < isoPlotWidth; x++)
                {
                    // get the id of the overlay at the isolation plot point
                    int isoPlotOverlayID = isolationPlotArray[x, y];
                    if (isoPlotOverlayID == 0) continue;
                    LogMessage("x=" + x.ToString() + ", y=" + y.ToString() + ", ID=" + isoPlotOverlayID.ToString("x8"));
                }
            }
            LogMessage("DumpIsoPlotArrayToLog Ends");
        }

        // ####################################################################
        // ##### GCode Graphical Stigmergy Code
        // ####################################################################
        #region GCode Graphical Stigmergy Code


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clears a builderIDTag from an isoCell in the isolation plot. Observes the graphical stigmergy protocol.
        /// This removes a specific isoPlotObject from having any effect on a point. Basically
        /// we are unflashing a overlay for a builder object tag. 
        /// </summary>
        /// <param name="isoPlotObjectID">the builderID of the IsoPlotObject that we clear (not a Tag)</param>
        /// <param name="ix">X0 coord</param>
        /// <param name="iy">Y coord</param>
        public void ClearBuilderIDFromGSIsoCellByTag(int isoPlotObjectTag, int ix, int iy)
        {
            int existingOverlayID;

            // sanity check the coordinates here
            if (ix < 0) return;
            if (iy < 0) return;
            if (ix >= isoPlotWidth) return;
            if (iy >= isoPlotHeight) return;

            // are we even in use?
            existingOverlayID = CleanFlagsFromOverlayID(IsolationPlotArray[ix, iy]);
            if (existingOverlayID == OverlayCollection.EMPTY_OVERLAY_ID)
            {
                // not in use, just leave now
                return;
            }

            // if we get here we know the cell is in use. We have to replace the overlay with an
            // overlay that contains all of the existing information except the tag belonging
            // to the supplied isoPlotObjectID. This overlay may already exist.
            IsolationPlotArray[ix, iy] = CurrentOverlayCollection.CreateNewOrReturnExistingRemovingSingleTagFromExistingOverlay(isoPlotObjectTag, existingOverlayID);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clears a builderID from an isoCell in the isolation plot. Observes the graphical stigmergy protocol.
        /// This removes a specific isoPlotObject from having any effect on a point. Basically
        /// we are unflashing a overlay for a builder object. 
        /// </summary>
        /// <param name="isoPlotObjectID">the builderID of the IsoPlotObject that we clear (not a Tag)</param>
        /// <param name="ix">X0 coord</param>
        /// <param name="iy">Y coord</param>
        public void ClearBuilderIDFromGSIsoCellByID(int isoPlotObjectID, int ix, int iy)
        {
            int existingOverlayID;

            // sanity check the coordinates here
            if (ix < 0) return;
            if (iy < 0) return;
            if (ix >= isoPlotWidth) return;
            if (iy >= isoPlotHeight) return;

            // are we even in use?
            existingOverlayID = CleanFlagsFromOverlayID(IsolationPlotArray[ix, iy]);
            if (existingOverlayID == OverlayCollection.EMPTY_OVERLAY_ID)
            {
                // not in use, just leave now
                return;
            }

            // if we get here we know the cell is in use. We have to replace the overlay with an
            // overlay that contains all of the existing information except the ID belonging
            // to the supplied isoPlotObjectID. This overlay may already exist.
            IsolationPlotArray[ix, iy] = CurrentOverlayCollection.CreateNewOrReturnExistingRemovingSingleIDFromExistingOverlay(isoPlotObjectID, existingOverlayID);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets a builderID on an IsoCell in the isolation plot. Observes the graphical stigmergy protocol.
        /// This is _not_ fast - but it is necessary. 
        /// </summary>
        /// <param name="usageMode">the usage of this builderID in this cell</param>
        /// <param name="isoPlotObjectID">the id of the IsoPlotObject that ultimately generated this call.</param>
        /// <param name="ix">X0 coord</param>
        /// <param name="iy">Y coord</param>
        public void SetBuilderIDOnGSIsoCell(int isoPlotObjectID, IsoPlotUsageTagFlagEnum usageMode, int ix, int iy)
        {
            // just call this
            SetBuilderIDOnGSIsoCell(isoPlotObjectID, usageMode, ix, iy, false);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets a builderID on an IsoCell in the isolation plot. Observes the graphical stigmergy protocol.
        /// This is _not_ fast - but it is necessary.
        /// </summary>
        /// <param name="usageMode">the usage of this builderID in this cell</param>
        /// <param name="isoPlotObjectID">the id of the GCodeBuidlerObject that ultimately generated this call.</param>
        /// <param name="ix">X0 coord</param>
        /// <param name="iy">Y coord</param>
        /// <param name="makePixelSolo">Removes all builderIDs below it and makes the incoming builderID the only ID in the isoCell</param>
        public void SetBuilderIDOnGSIsoCell(int isoPlotObjectID, IsoPlotUsageTagFlagEnum usageMode, int ix, int iy, bool makePixelSolo)
        {
            int existingOverlayID;
            int newTag;

            //DebugMessage("SetBuilderIDOnGSIsoCell: ID=" + isoPlotObjectID.ToString() + " (" + ix.ToString() + "," + iy.ToString() + ")");

            // sanity check the coordinates here
            if (ix < 0)
            {
                throw new Exception("SetBuilderIDOnGSIsoCell invalid value ix=" + ix.ToString());
            }
            if (iy < 0)
            {
                throw new Exception("SetBuilderIDOnGSIsoCell invalid value iy=" + iy.ToString());
            }
            if (ix >= isoPlotWidth)
            {
                throw new Exception("SetBuilderIDOnGSIsoCell invalid value ix >= isoPlotWidth, ix=" + ix.ToString()+ ", isoPlotWidth=" + isoPlotWidth.ToString());
            }
            if (iy >= isoPlotHeight)
            {
                throw new Exception("SetBuilderIDOnGSIsoCell invalid value iy >= isoPlotHeight, iy=" + iy.ToString() + ", isoPlotHeight=" + isoPlotHeight.ToString());
            }

            if (isoPlotObjectID == 0)
            {
                // zero means the zeroth entry in the builder array, which is always the background builderID
                IsolationPlotArray[ix, iy] = 0;
                return;
            }

            // set the flags on the tag, we assume there is one and only one this will be checked when we set it
            newTag = isoPlotObjectID | (int)usageMode;

            if (makePixelSolo == true)
            {
                // this builderID is the only builderID on the pixel, just force it to do this
                IsolationPlotArray[ix, iy] = CurrentOverlayCollection.CreateNewOrReturnExistingFromSingleUsage(newTag);
            }
            else
            { 
                // we are adding this builderID to the overlays on this isoCell
                // see what overlay is currently in this cell
                existingOverlayID = CleanFlagsFromOverlayID(IsolationPlotArray[ix, iy]);
                if (existingOverlayID == OverlayCollection.EMPTY_OVERLAY_ID)
                {
                    // we have an overlay which represents empty space, create new or reuse an existing and set it
                    IsolationPlotArray[ix, iy] = CurrentOverlayCollection.CreateNewOrReturnExistingFromSingleUsage(newTag);
                    return;
                }

                // not an empty overlay ID. This means one or more other objects are using this cell
                // we have to replace this overlay ID with one that incorporates the existing ones
                // and also our new tag. This may already exist and we can just re-use it.
                IsolationPlotArray[ix, iy] = CurrentOverlayCollection.CreateNewOrReturnExistingFromSingleUsageAndExistingOverlay(newTag, existingOverlayID);
            }
            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a line from two points. Observes the graphical stigmergy protocol. Always
        /// assumes it is drawing an edge isoCell. 
        /// </summary>
        /// <remarks>
        /// Credits: this is based the bresnham line drawing algorythm right out of wikipedia
        ///    http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
        ///    Most comments in the code below are from the original source - not me
        /// </remarks>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <param name="usageFlag">the usage flag we put on the builderID when we write out to the isoPlotCell</param>
        /// <returns>the isoPlotObjID of the line we created or 0 for fail</returns>
        private int DrawGSLine(int isoPlotObjID, IsoPlotUsageTagFlagEnum usageFlag, int x0, int y0, int x1, int y1)
        {
            return DrawGSLine(isoPlotObjID, usageFlag, x0, y0, x1, y1, false);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a line from two points. Observes the graphical stigmergy protocol. Always
        /// assumes it is drawing an edge isoCell
        /// </summary>
        /// <remarks>
        /// Credits: this is based the bresnham line drawing algorythm right out of wikipedia
        ///    http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
        ///    Most comments in the code below are from the original source - not me
        /// </remarks>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <param name="wantSoloIsoCell">if true we do not overlay but rather make the isoPlotObj the only isoPlotObj on the pixel</param>
        /// <param name="usageFlag">the usage flag we put on the builderID when we write out to the isoPlotCell</param>
        /// <returns>the isoPlotObjID of the line we created or 0 for fail</returns>
        private int DrawGSLine(int isoPlotObjID, IsoPlotUsageTagFlagEnum usageFlag, int x0, int y0, int x1, int y1, bool wantSoloIsoCell)
        {
            int ix;
            int iy;

            // sanity check the coordinates here
            if (x0 < 0) return 0;
            if (y0 < 0) return 0;
            if (x1 >= isoPlotWidth) return 0;
            if (y1 >= isoPlotHeight) return 0;

           // DebugMessage("DrawGSLine: ID=" + isoPlotObjID.ToString() + " (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep == true)
            {
                int tmp = x0;
                x0 = y0;
                y0 = tmp;
                tmp = x1;
                x1 = y1;
                y1 = tmp;
            }
            if (x0 > x1)
            {
                int tmp = x0;
                x0 = x1;
                x1 = tmp;
                tmp = y0;
                y0 = y1;
                y1 = tmp;
            }
            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = deltax / 2;
            int ystep;
            int y = y0;
            if (y0 < y1)
            {
                ystep = 1;
            }
            else
            {
                ystep = -1;
            }
            for (int x = x0; x <= x1; x++)
            {
                if (steep == true)
                {
                    ix = y;
                    iy = x;
                }
                else
                {
                    ix = x;
                    iy = y;
                }

                // set the point, obeying the Graphical Stigmergy protocol
                SetBuilderIDOnGSIsoCell(isoPlotObjID, usageFlag, ix, iy, wantSoloIsoCell);

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }

            // return the isoPlotObject ID cleaned of any flags
            return isoPlotObjID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a wide line between two points. Observes the graphical stigmergy protocol.
        /// </summary>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <param name="width">the width of the line</param>
        /// <param name="fillMode">The fill mode</param>
        /// <param name="usageFlag">the usage flag we put on the builderID when we write out to the isoPlotCell</param>
        /// <returns>the isoPlotObjID's of the arc we created or null for fail</returns>
        public int DrawGSLineOutLine(IsoPlotUsageTagFlagEnum usageFlag, int x0, int y0, int x1, int y1, int width, GSFillModeEnum fillMode)
        {
            // these are the endpoints of the rectangle
            Point ptUL;
            Point ptLL;
            Point ptLR;
            Point ptUR;
            Point ptRectLL;
            Point ptRectUR;
            int retInt;
            bool wantSoloIsoCell = false;

            if (width <= 0) return 0;

            // the input value is actually the center line of a line that has width. Figure
            // out the perpendicular endpoints now
            retInt = MiscGraphicsUtils.GetWideLineEndPoints(x0, y0, x1, y1, width, out ptLL, out ptUL, out ptUR, out ptLR);
            if (retInt != 0)
            {
                LogMessage("DrawGSLineOutLine call to GetWideLineEndPoints returned " + retInt.ToString());
                return 0;
            }

            // record this object, with it max and min points 
            IsoPlotObject_Line isoPlotObj = new IsoPlotObject_Line(this.NextIsoPlotObjectID, x0, y0, x1, y1, width);
            isoPlotObjList.Add(isoPlotObj);

            //DebugMessage("DrawGSLineOutline:ID=" + isoPlotObj.IsoPlotObjectID.ToString() + "(" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // draw the lines
            this.DrawGSLine(isoPlotObj.IsoPlotObjectID, usageFlag, ptUL.X, ptUL.Y, ptLL.X, ptLL.Y, wantSoloIsoCell);
            this.DrawGSLine(isoPlotObj.IsoPlotObjectID, usageFlag, ptLL.X, ptLL.Y, ptLR.X, ptLR.Y, wantSoloIsoCell);
            this.DrawGSLine(isoPlotObj.IsoPlotObjectID, usageFlag, ptLR.X, ptLR.Y, ptUR.X, ptUR.Y, wantSoloIsoCell);
            this.DrawGSLine(isoPlotObj.IsoPlotObjectID, usageFlag, ptUR.X, ptUR.Y, ptUL.X, ptUL.Y, wantSoloIsoCell);

            // do we want the box filled?
            if (fillMode == GSFillModeEnum.FillMode_BACKGROUND)
            {
                // yes, we do, figure out the smallest and largest XY for our bounding box
                retInt = MiscGraphicsUtils.GetBoundingRectangleEndPointsFrom4Points(ptLL, ptUL, ptUR, ptLR, out ptRectLL, out ptRectUR);
                if (retInt != 0)
                {
                    LogMessage("DrawGSLineOutLine call to GetBoundingRectangleEndPointsFrom4Points returned " + retInt.ToString());
                    return 0;
                }
                // now fill the bounding box
                BackgroundFillGSByBoundarySimpleHoriz(isoPlotObj.IsoPlotObjectID, ptRectLL.X, ptRectLL.Y, ptRectUR.X, ptRectUR.Y, -1);
            }
            else if (fillMode == GSFillModeEnum.FillMode_ERASE)
            {
                // we erase everything by creating a list of builderIDs which represent the boundary and 0 id for the fill
                List<int> builderIDList = new List<int>();
                builderIDList.Add(isoPlotObj.IsoPlotObjectID);
                retInt = MiscGraphicsUtils.GetBoundingRectangleEndPointsFrom4Points(ptLL, ptUL, ptUR, ptLR, out ptRectLL, out ptRectUR);
                if (retInt != 0)
                {
                    LogMessage("DrawGSLineOutLine call to GetBoundingRectangleEndPointsFrom4Points returned " + retInt.ToString());
                    return 0;
                }
                BackgroundFillGSByBoundaryComplexVert(builderIDList, IsoPlotObject.DEFAULT_ISOPLOTOBJECT_ID, ptRectLL.X, ptRectLL.Y, ptRectUR.X, ptRectUR.Y, -1);
            }

            return isoPlotObj.IsoPlotObjectID&0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws an wide line arc between two points. Observes the graphical stigmergy protocol.
        /// </summary>
        /// <param name="arcCenterXCoord">arcCenterXCoord coord</param>
        /// <param name="arcCenterYCoord">arcCenterYCoord coord</param>
        /// <param name="startAngleInDegrees">startAngleInDegrees coord</param>
        /// <param name="sweepAngleInDegrees">sweepAngleInDegrees coord</param>
        /// <param name="radius">the radius to the center of the line</param>
        /// <param name="width">the width of the line</param>
        /// <param name="fillMode">The fill mode</param>
        /// <param name="wantClockWise">if true, draw the arc clockwise</param>
        /// <param name="isMultiQuadrantArcIn">is an arc specified in multiquadrant mode</param>
        /// <returns> the isoPlotObjID of the arc we created or null for fail</returns>
        public int DrawGSArcOutLine(float arcCenterXCoord, float arcCenterYCoord, float startAngleInDegrees, float sweepAngleInDegrees, float radius, int width, bool wantClockWise, GSFillModeEnum fillMode, bool isMultiQuadrantArc)
        {
            // these are the endpoints of the rectangle
            Point lowerLeft;
            Point upperRight;
            int outerRadius = 0;
            int innerRadius = 0;
            int outerFirstX;
            int outerFirstY;
            int outerLastX;
            int outerLastY;
            int innerFirstX;
            int innerFirstY;
            int innerLastX;
            int innerLastY;
            bool wantSoloIsoCell = false;

            if (width <= 0) return 0;

            // our arc drawing program needs to know how much of each quadrant to fill in we get this information now
            ArcPointMap[] pointMap = MiscGraphicsUtils.GetArcPointMapListFromStartAndSweepAngles(startAngleInDegrees, sweepAngleInDegrees, wantClockWise);

            // record this object
            IsoPlotObject_Arc isoPlotObj = new IsoPlotObject_Arc(this.NextIsoPlotObjectID, (int)arcCenterXCoord, (int)arcCenterYCoord, (int)radius, width, startAngleInDegrees, sweepAngleInDegrees, wantClockWise, isMultiQuadrantArc);

            // the input radius value is actually the center line of a arc that has width. Figure
            // out the two radius values now
            outerRadius = isoPlotObj.GetOuterRadius();
            innerRadius = isoPlotObj.GetInnerRadius();

            if (outerRadius <= 0) return 0;
            // we have at least one good radius - add it
            isoPlotObjList.Add(isoPlotObj);

            // we need to know the maximum chord length our fill can possibly have
            int maxChordLength = (int)MiscGraphicsUtils.CalcMaxInnerChordForTwoConcentricCircles(outerRadius, innerRadius);
            maxChordLength += 1; // round up

            // draw the outer arc
            this.DrawGSArc(isoPlotObj.IsoPlotObjectID, IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, (int)arcCenterXCoord, (int)arcCenterYCoord, outerRadius, pointMap, out outerFirstX, out outerFirstY, out outerLastX, out outerLastY, wantSoloIsoCell);

            // can we add the inner arc and chords?
            if (innerRadius <= 0) return isoPlotObj.IsoPlotObjectID & 0x00ffffff;
            // draw the inner arc
            this.DrawGSArc(isoPlotObj.IsoPlotObjectID, IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, (int)arcCenterXCoord, (int)arcCenterYCoord, innerRadius, pointMap, out innerFirstX, out innerFirstY, out innerLastX, out innerLastY, wantSoloIsoCell);

            // record the inner and outer start and end points
            isoPlotObj.InnerFirstX = innerFirstX;
            isoPlotObj.InnerFirstY = innerFirstY;
            isoPlotObj.InnerLastX = innerLastX;
            isoPlotObj.InnerLastY = innerLastY;
            isoPlotObj.OuterFirstX = outerFirstX;
            isoPlotObj.OuterFirstY = outerFirstY;
            isoPlotObj.OuterLastX = outerLastX;
            isoPlotObj.OuterLastY = outerLastY;

            // draw the beginning line segment
            this.DrawGSLine(isoPlotObj.IsoPlotObjectID, IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, innerFirstX, innerFirstY, outerFirstX, outerFirstY, wantSoloIsoCell);
            // draw the ending line segment
            this.DrawGSLine(isoPlotObj.IsoPlotObjectID, IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, innerLastX, innerLastY, outerLastX, outerLastY, wantSoloIsoCell);

            // do we want the arc filled?
            if (fillMode == GSFillModeEnum.FillMode_BACKGROUND)
            {
                // yes, we do, figure out the smallest and largest XY for our bounding box
                bool retBool = isoPlotObj.GetBoundingPoints(out lowerLeft, out upperRight);
                if (retBool == true)
                {
                    // now fill the bounding box
                    BackgroundFillGSByBoundarySimpleHoriz(isoPlotObj.IsoPlotObjectID, lowerLeft.X, lowerLeft.Y, upperRight.X, upperRight.Y, maxChordLength);
                }
            }
            else if (fillMode == GSFillModeEnum.FillMode_ERASE)
            {
                // we erase everything by creating a list of builderIDs which represent the boundary and 0 id for the fill
                List<int> builderIDList = new List<int>();
                builderIDList.Add(isoPlotObj.IsoPlotObjectID);
                // figure out the smallest and largest XY for our bounding box
                bool retBool = isoPlotObj.GetBoundingPoints(out lowerLeft, out upperRight);
                if (retBool == true)
                {
                    // now fill the bounding box
                    BackgroundFillGSByBoundaryComplexHoriz(builderIDList, IsoPlotObject.DEFAULT_ISOPLOTOBJECT_ID, lowerLeft.X, lowerLeft.Y, upperRight.X, upperRight.Y, maxChordLength);
                }
            }

            return isoPlotObj.IsoPlotObjectID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a contour arc between two points. Observes the graphical stigmergy protocol.
        /// Does not fill
        /// </summary>
        /// <param name="arcCenterXCoord">arcCenterXCoord coord</param>
        /// <param name="arcCenterYCoord">arcCenterYCoord coord</param>
        /// <param name="startAngleInDegrees">startAngleInDegrees coord</param>
        /// <param name="sweepAngleInDegrees">sweepAngleInDegrees coord</param>
        /// <param name="radius">the radius to the center of the line</param>
        /// <param name="width">the width of the line</param>
        /// <param name="wantClockWise">if true, draw the arc clockwise</param>
        /// <param name="isMultiQuadrantArcIn">is an arc specified in multiquadrant mode</param>
        /// <param name="makePixelSolo">Removes all builderIDs below it and makes the incoming builderID the only ID in the isoCell</param>
        /// <param name="usageFlag">the usage flag we put on the builderID when we write out to the isoPlotCell</param>
        /// <returns> the isoPlotObjID of the arc we created or null for fail</returns>
        public int DrawGSContourArc(IsoPlotUsageTagFlagEnum usageFlag, float arcCenterXCoord, float arcCenterYCoord, float startAngleInDegrees, float sweepAngleInDegrees, float radius, bool wantClockWise, bool isMultiQuadrantArc, bool makePixelSolo)
        {
            int firstX;
            int firstY;
            int lastX;
            int lastY;

            if (radius <= 0) return 0;

            // our arc drawing program needs to know how much of each quadrant to fill in we get this information now
            ArcPointMap[] pointMap = MiscGraphicsUtils.GetArcPointMapListFromStartAndSweepAngles(startAngleInDegrees, sweepAngleInDegrees, wantClockWise);

            // record this object
            IsoPlotObject_ContourArc isoPlotObj = new IsoPlotObject_ContourArc(this.NextIsoPlotObjectID, (int)arcCenterXCoord, (int)arcCenterYCoord, (int)radius, startAngleInDegrees, sweepAngleInDegrees, wantClockWise, isMultiQuadrantArc);
            isoPlotObjList.Add(isoPlotObj);
            
            // draw the arc
            this.DrawGSArc(isoPlotObj.IsoPlotObjectID, usageFlag, (int)arcCenterXCoord, (int)arcCenterYCoord, (int)radius, pointMap, out firstX, out firstY, out lastX, out lastY, makePixelSolo);

            // record the inner and outer start and end points
            isoPlotObj.FirstX = firstX;
            isoPlotObj.FirstY = firstY;
            isoPlotObj.LastX = lastX;
            isoPlotObj.LastY = lastY;

            return isoPlotObj.IsoPlotObjectID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a outline. Observes the graphical stigmergy protocol. The input 
        /// params should all be in plot coords.
        /// </summary>
        /// <param name="vertexPoints">the vertex points</param>
        /// <param name="fillmode">The fill mode</param>
        /// <param name="usageFlag">the usage flag we put on the builderID when we write out to the isoPlotCell</param>
        /// <returns>a list of the isoPlotObjIDs of the lines created or null for fail</returns>
        public int DrawGSOutLine(IsoPlotUsageTagFlagEnum usageFlag, PointF[] vertexPoints, GSFillModeEnum fillmode)
        {
            Point point1;
            Point point2;

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            // convert vertexes to ints now
            System.Drawing.Point[] lineEndPoints = MiscGraphicsUtils.ConvertPointFArrayToPoint(vertexPoints);

            // record this object 
            IsoPlotObject_Outline isoPlotObj = new IsoPlotObject_Outline(this.NextIsoPlotObjectID, lineEndPoints);
            isoPlotObjList.Add(isoPlotObj);

            //DebugMessage("DrawGSPolygonOutLine:ID=" + isoPlotObj.IsoPlotObjectID.ToString() + "(" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // process each pair of points in the array. Looping back around when at the end.
            int lastIndex = lineEndPoints.Length - 1;
            for (int i=0; i< lineEndPoints.Length;i++)
            {
                // get the two points we draw
                point1 = lineEndPoints[i];
                if (i < lastIndex) point2 = lineEndPoints[i + 1];
                else { point2 = lineEndPoints[0]; }

                // collect the max and min endpoints as we go
                if (point1.X > maxX) maxX = point1.X;
                if (point1.X < minX) minX = point1.X;
                if (point2.X > maxX) maxX = point2.X;
                if (point2.X < minX) minX = point2.X;

                if (point1.Y > maxY) maxY = point1.Y;
                if (point1.Y < minY) minY = point1.Y;
                if (point2.Y > maxY) maxY = point2.Y;
                if (point2.Y < minY) minY = point2.Y;

                // draw the line
                this.DrawGSLine(isoPlotObj.IsoPlotObjectID, usageFlag, (int)point1.X, (int)point1.Y, (int)point2.X, (int)point2.Y);
            }

            // do we want the box filled?
            if (fillmode == GSFillModeEnum.FillMode_BACKGROUND)
            {
                // now fill the bounding box
                BackgroundFillGSByBoundarySimpleHoriz(isoPlotObj.IsoPlotObjectID, minX, minY, maxX, maxY, -1);
            }

            return isoPlotObj.IsoPlotObjectID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a polygon. Observes the graphical stigmergy protocol. The input 
        /// params should all be in plot coords.
        /// </summary>
        /// <param name="xCenter">the x centerpoint</param>
        /// <param name="yCenter">the x centerpoint</param>
        /// <param name="numSides">the number of sides</param>
        /// <param name="diameter">the diameter of the circle enclosing the polygon</param>
        /// <param name="rotationAngleInDegrees">the rotation angle of the polygon in degrees</param>
        /// <param name="fillmode">The fill mode</param>
        /// <param name="usageFlag">the usage flag we put on the builderID when we write out to the isoPlotCell</param>
        /// <returns>the isoPlotObjID of the line created or 0 for fail</returns>
        public int DrawGSPolygonOutLine(IsoPlotUsageTagFlagEnum usageFlag, float xCenter, float yCenter, int numSides, float diameter, float rotationAngleInDegrees, GSFillModeEnum fillmode)
        {
            Point point1;
            Point point2;

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            if (numSides < 3) return 0;
            if (diameter <= 0) return 0;

            // get the vertexes
            System.Drawing.PointF[] vertexPoints = MiscGraphicsUtils.CalcPolygonVertexPoints(xCenter, yCenter, numSides, diameter, rotationAngleInDegrees);
            // convert it to ints now
            System.Drawing.Point[] lineEndPoints = MiscGraphicsUtils.ConvertPointFArrayToPoint(vertexPoints);

            // record this object 
            IsoPlotObject_Polygon isoPlotObj = new IsoPlotObject_Polygon(this.NextIsoPlotObjectID, (int)xCenter, (int)yCenter, (int)diameter, numSides, (int)rotationAngleInDegrees);
            isoPlotObjList.Add(isoPlotObj);

            //DebugMessage("DrawGSPolygonOutLine:ID=" + isoPlotObj.IsoPlotObjectID.ToString() + "(" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // process each pair of points in the array. Looping back around when at the end.
            int lastIndex = lineEndPoints.Length - 1;
            for (int i = 0; i < lineEndPoints.Length; i++)
            {
                // get the two points we draw
                point1 = lineEndPoints[i];
                if (i < lastIndex) point2 = lineEndPoints[i + 1];
                else { point2 = lineEndPoints[0]; }

                // collect the max and min endpoints as we go
                if (point1.X > maxX) maxX = point1.X;
                if (point1.X < minX) minX = point1.X;
                if (point2.X > maxX) maxX = point2.X;
                if (point2.X < minX) minX = point2.X;

                if (point1.Y > maxY) maxY = point1.Y;
                if (point1.Y < minY) minY = point1.Y;
                if (point2.Y > maxY) maxY = point2.Y;
                if (point2.Y < minY) minY = point2.Y;

                // draw the line
                this.DrawGSLine(isoPlotObj.IsoPlotObjectID, usageFlag, (int)point1.X, (int)point1.Y, (int)point2.X, (int)point2.Y);
            }

            // do we want the box filled?
            if (fillmode == GSFillModeEnum.FillMode_BACKGROUND)
            {
                // now fill the bounding box
                BackgroundFillGSByBoundarySimpleHoriz(isoPlotObj.IsoPlotObjectID, minX, minY, maxX, maxY, -1);
            }

            return isoPlotObj.IsoPlotObjectID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a line between two points. Observes the graphical stigmergy protocol.
        /// </summary>
        /// <remarks>
        /// This just takes the two end points and draws a line between them. This line is
        /// assumed to be an edge line. This will run the tool over that line when the PCB
        /// is routed. This gives the effect of contour a single track.
        /// </remarks>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <param name="usageFlag">the usage flag we put on the builderID when we write out to the isoPlotCell</param>
        /// <returns>the isoPlotObjID of the line created or 0 for fail</returns>
        public int DrawGSLineContourLine(IsoPlotUsageTagFlagEnum usageFlag, int x0, int y0, int x1, int y1)
        {
            // the inputs are the endpoints of the line

            // record this object, with it max and min points 
            IsoPlotObject_ContourLine isoPlotObj = new IsoPlotObject_ContourLine(this.NextIsoPlotObjectID, x0, y0, x1, y1);
            isoPlotObjList.Add(isoPlotObj);

            //DebugMessage("DrawGSLineContourLine:ID=" + isoPlotObj.IsoPlotObjectID.ToString() + "(" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // draw the lines
            this.DrawGSLine(isoPlotObj.IsoPlotObjectID, usageFlag, x0, y0, x1, y1);

            return isoPlotObj.IsoPlotObjectID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a filled arc of a defined radius, startangle and sweepAngle at a point. Observes the 
        /// graphical stigmergy protocol. Always does overlays never solo isoCells
        /// </summary>
        /// <remarks>
        /// Credits: much of algorythm came from 
        ///    http://stackoverflow.com/questions/1201200/fast-algorithm-for-drawing-filled-circles
        ///    However in this particular application we have some specific requirements. When
        ///    we later go through and figure out the IsoPlotSegment_Arc objects (these are the 
        ///    one we convert to GCodes) we must follow the circle linearly cell-by-cell. The
        ///    usual code for this is optimized to place cells in all eight quadrants in one pass.
        ///    This, while faster, cannot be used here. We need to be able to follow the arc around
        ///    checking for intersections and overlays on the background.
        ///    
        ///    Consequently, the algorithm has been unrolled into a number of for loops. It is 
        ///    NOT efficient. I recognise this - but then we are not using it for graphical display
        ///    we are calculating a plot and the drawing time, as long as its not outrageous, is not
        ///    super relevant.
        /// </remarks>
        /// <param name="cx">X center</param>
        /// <param name="cy">Y center</param>
        /// <param name="radius">the radius</param>
        /// <param name="pointMap">a point map of the percentages of the quadrants we fill in</param>
        /// <param name="isoPlotObjID">the builderID</param>
        /// <param name="firstXDrawn">the first x point written</param>
        /// <param name="firstYDrawn">the first y point written</param>
        /// <param name="lastXDrawn">the last x point written</param>
        /// <param name="lastYDrawn">the last y point written</param>
        /// <param name="usageFlag">the usage flag we put on the builderID when we write out to the isoPlotCell</param>
        /// <returns>the isoPlotObjID or 0 for fail</returns>
        private int DrawGSArc(int isoPlotObjID, IsoPlotUsageTagFlagEnum usageFlag, int cx, int cy, int radius, ArcPointMap[] pointMap, out int firstXDrawn, out int firstYDrawn, out int lastXDrawn, out int lastYDrawn)
        {
            return DrawGSArc(isoPlotObjID, usageFlag, cx, cy, radius, pointMap, out firstXDrawn, out firstYDrawn, out lastXDrawn, out lastYDrawn, false);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a filled arc of a defined radius, startangle and sweepAngle at a point. Observes the 
        /// graphical stigmergy protocol.
        /// </summary>
        /// <remarks>
        /// Credits: much of algorythm came from 
        ///    http://stackoverflow.com/questions/1201200/fast-algorithm-for-drawing-filled-circles
        ///    However in this particular application we have some specific requirements. When
        ///    we later go through and figure out the IsoPlotSegment_Arc objects (these are the 
        ///    one we convert to GCodes) we must follow the circle linearly cell-by-cell. The
        ///    usual code for this is optimized to place cells in all eight quadrants in one pass.
        ///    This, while faster, cannot be used here. We need to be able to follow the arc around
        ///    checking for intersections and overlays on the background.
        ///    
        ///    Consequently, the algorithm has been unrolled into a number of for loops. It is 
        ///    NOT efficient. I recognise this - but then we are not using it for graphical display
        ///    we are calculating a plot and the drawing time, as long as its not outrageous, is not
        ///    super relevant.
        /// </remarks>
        /// <param name="cx">X center</param>
        /// <param name="cy">Y center</param>
        /// <param name="radius">the radius</param>
        /// <param name="pointMap">a point map of the percentages of the quadrants we fill in</param>
        /// <param name="isoPlotObjID">the builderID</param>
        /// <param name="firstXDrawn">the first x point written</param>
        /// <param name="firstYDrawn">the first y point written</param>
        /// <param name="lastXDrawn">the last x point written</param>
        /// <param name="lastYDrawn">the last y point written</param>
        /// <param name="wantSoloIsoCell">if true we do not overlay but rather make the builderID the only builderID on the isoCell</param>
        /// <param name="usageFlag">the usage flag we put on the builderID when we write out to the isoPlotCell</param>
        /// <returns>the isoPlotObjID or 0 for fail</returns>
        private int DrawGSArc(int isoPlotObjID, IsoPlotUsageTagFlagEnum usageFlag, int cx, int cy, int radius, ArcPointMap[] pointMap, out int firstXDrawn, out int firstYDrawn, out int lastXDrawn, out int lastYDrawn, bool wantSoloIsoCell)
        {
            int error = -radius;
            int x = radius;
            int y = 0;
            int ptCount = 0;
            Point[] ptArray = null;
            int ptIndex = 0;
            int workingX = int.MinValue;
            int workingY = int.MinValue;
            int cellsDrawnInQuadrant = 0;
            ArcPointMap workingPointMapObj = null;

            // set our vars now
            int lastX = int.MinValue;
            int lastY = int.MinValue;
            int firstX = int.MinValue;
            int firstY = int.MinValue;

            // set our out vars now
            firstXDrawn = int.MinValue;
            firstYDrawn = int.MinValue;
            lastXDrawn = int.MinValue;
            lastYDrawn = int.MinValue;

            //DebugMessage("DrawGSArc: ID=" + isoPlotObjID.ToString() + " (" + cx.ToString() + "," + cy.ToString() + ") radius=" + radius.ToString());

            // we have to have eight pointMaps here
            if (pointMap == null) return 0;
            if (pointMap.Length != 8) return 0;


            // first we get a count of the points in one section of the arc
            while (x >= y)
            {
                // count this
                ptCount++;
                // calc error
                error += y;
                ++y;
                error += y;
                if (error >= 0)
                {
                    --x;
                    error -= x;
                    error -= x;
                }
            }
            // this should not happen
            if (ptCount == 0)
            {
                LogMessage("DrawGSCircleFilled: ID=" + isoPlotObjID.ToString() + " ptCount==0");
                return 0;
            }

            // create the array in which we store the points
            ptArray = new Point[ptCount];

            // now put the points in place in the array
            error = -radius;
            x = radius;
            y = 0;
            while (x >= y)
            {
                // create a new point for our x,y
                ptArray[ptIndex] = new Point(x, y);
                ptIndex++;
                // calc error
                error += y;
                ++y;
                error += y;
                if (error >= 0)
                {
                    --x;
                    error -= x;
                    error -= x;
                }
            }

            // convert our point map percentages to actual cell counts
            for (int i = 0; i < pointMap.Length; i++)
            {
                pointMap[i].SetCellCounts(ptCount);
            }


            // ###
            // now go through the quadrants adding points sequentially. at this point the
            // point map has the number of cells to skip at the start of each quadrant 
            // and the number to draw from that point. This does not necessarily 
            // completely draw the rest of the quadrant
            // ###

            // UR quadrant CCW from +X axis up to 45 degree line
            cellsDrawnInQuadrant = 0;
            workingPointMapObj = pointMap[0]; // note hard coded here
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx + ptArray[i].X;
                workingY = cy + ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                if (firstX < 0)
                {
                    firstX = workingX;
                    firstY = workingY;
                }
                // DebugMessage("++1 x="+ workingX.ToString() + " y=" + workingY.ToString());
                if (i < workingPointMapObj.CellsToSkip) { } // skip this cell
                else
                {
                    // not skipping, should we draw it?
                    if (cellsDrawnInQuadrant < workingPointMapObj.CellsToDraw)
                    {
                        // yes we should, draw it and count it
                        this.SetBuilderIDOnGSIsoCell(isoPlotObjID, usageFlag, workingX, workingY, wantSoloIsoCell);
                        // record our out vars
                        if ((workingPointMapObj.ThisIsTheStartOctant) && (firstXDrawn < 0))
                        {
                            firstXDrawn = workingX;
                            firstYDrawn = workingY;
                        }
                        if(workingPointMapObj.ThisIsTheStopOctant)
                        {
                            lastXDrawn = workingX;
                            lastYDrawn = workingY;
                        }
                        //DebugMessage("SGSP ID="+ isoPlotObjID.ToString() + ", Draw x=" + workingX.ToString() + ", y=" + workingY.ToString());
                        cellsDrawnInQuadrant++;
                    }
                }                
                lastX = workingX;
                lastY = workingY;
            }

            // peek into the next quadrant and see if we need to force update the first drawn
            if ((pointMap[1].CellsToDraw==0) && (pointMap[1].ThisIsTheStartOctant==true))
            {
                firstXDrawn = workingX;
                firstYDrawn = workingY;
            }

            // UR quadrant CCW from 45 degree line to +Y axis
            cellsDrawnInQuadrant = 0;
            workingPointMapObj = pointMap[1]; // note hard coded here
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx + ptArray[i].Y;
                workingY = cy + ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                // DebugMessage("++2 x="+ workingX.ToString() + " y=" + workingY.ToString());
                if (i < workingPointMapObj.CellsToSkip) { } // skip this cell
                else
                {
                    // not skipping, should we draw it?
                    if (cellsDrawnInQuadrant < workingPointMapObj.CellsToDraw)
                    {
                        // yes we should, draw it and count it
                        this.SetBuilderIDOnGSIsoCell(isoPlotObjID, usageFlag, workingX, workingY, wantSoloIsoCell);
                        // record our out vars
                        if ((workingPointMapObj.ThisIsTheStartOctant) && (firstXDrawn < 0))
                        {
                            firstXDrawn = workingX;
                            firstYDrawn = workingY;
                        }
                        if (workingPointMapObj.ThisIsTheStopOctant)
                        {
                            lastXDrawn = workingX;
                            lastYDrawn = workingY;
                        }
                        cellsDrawnInQuadrant++;
                    }
                }

                lastX = workingX;
                lastY = workingY;
            }

            // UL quadrant CCW from +Y axis to 45 degree line
            cellsDrawnInQuadrant = 0;
            workingPointMapObj = pointMap[2]; // note hard coded here
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx - ptArray[i].Y;
                workingY = cy + ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                // DebugMessage("++3 x="+ workingX.ToString() + " y=" + workingY.ToString());
                if (i < workingPointMapObj.CellsToSkip) { } // skip this cell
                else
                {
                    // not skipping, should we draw it?
                    if (cellsDrawnInQuadrant < workingPointMapObj.CellsToDraw)
                    {
                        // yes we should, draw it and count it
                        this.SetBuilderIDOnGSIsoCell(isoPlotObjID, usageFlag, workingX, workingY, wantSoloIsoCell);
                        // record our out vars
                        if ((workingPointMapObj.ThisIsTheStartOctant) && (firstXDrawn < 0))
                        {
                            firstXDrawn = workingX;
                            firstYDrawn = workingY;
                        }
                        if (workingPointMapObj.ThisIsTheStopOctant)
                        {
                            lastXDrawn = workingX;
                            lastYDrawn = workingY;
                        }
                        cellsDrawnInQuadrant++;
                    }
                }

                lastX = workingX;
                lastY = workingY;
            }

            // UL quadrant CCW from 45 degree line to -X axis
            cellsDrawnInQuadrant = 0;
            workingPointMapObj = pointMap[3]; // note hard coded here
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx - ptArray[i].X;
                workingY = cy + ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                // DebugMessage("++4 x="+ workingX.ToString() + " y=" + workingY.ToString());
                if (i < workingPointMapObj.CellsToSkip) { } // skip this cell
                else
                {
                    // not skipping, should we draw it?
                    if (cellsDrawnInQuadrant < workingPointMapObj.CellsToDraw)
                    {
                        // yes we should, draw it and count it
                        this.SetBuilderIDOnGSIsoCell(isoPlotObjID, usageFlag, workingX, workingY, wantSoloIsoCell);
                        // record our out vars
                        if ((workingPointMapObj.ThisIsTheStartOctant) && (firstXDrawn < 0))
                        {
                            firstXDrawn = workingX;
                            firstYDrawn = workingY;
                        }
                        if (workingPointMapObj.ThisIsTheStopOctant)
                        {
                            lastXDrawn = workingX;
                            lastYDrawn = workingY;
                        }
                        cellsDrawnInQuadrant++;
                    }
                }

                lastX = workingX;
                lastY = workingY;
            }

            // LL quadrant CCW from -X axis to 45 degree line
            cellsDrawnInQuadrant = 0;
            workingPointMapObj = pointMap[4]; // note hard coded here
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx - ptArray[i].X;
                workingY = cy - ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                // DebugMessage("++5 x="+ workingX.ToString() + " y=" + workingY.ToString());
                if (i < workingPointMapObj.CellsToSkip) { } // skip this cell
                else
                {
                    // not skipping, should we draw it?
                    if (cellsDrawnInQuadrant < workingPointMapObj.CellsToDraw)
                    {
                        // yes we should, draw it and count it
                        this.SetBuilderIDOnGSIsoCell(isoPlotObjID, usageFlag, workingX, workingY, wantSoloIsoCell);
                        // record our out vars
                        if ((workingPointMapObj.ThisIsTheStartOctant) && (firstXDrawn < 0))
                        {
                            firstXDrawn = workingX;
                            firstYDrawn = workingY;
                        }
                        if (workingPointMapObj.ThisIsTheStopOctant)
                        {
                            lastXDrawn = workingX;
                            lastYDrawn = workingY;
                        }
                        cellsDrawnInQuadrant++;
                    }
                }

                lastX = workingX;
                lastY = workingY;
            }

            // LL quadrant CCW from 45 degree line to -Y axis
            cellsDrawnInQuadrant = 0;
            workingPointMapObj = pointMap[5]; // note hard coded here
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx - ptArray[i].Y;
                workingY = cy - ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                // DebugMessage("++6 x="+ workingX.ToString() + " y=" + workingY.ToString());
                if (i < workingPointMapObj.CellsToSkip) { } // skip this cell
                else
                {
                    // not skipping, should we draw it?
                    if (cellsDrawnInQuadrant < workingPointMapObj.CellsToDraw)
                    {
                        // yes we should, draw it and count it
                        this.SetBuilderIDOnGSIsoCell(isoPlotObjID, usageFlag, workingX, workingY, wantSoloIsoCell);
                        // record our out vars
                        if ((workingPointMapObj.ThisIsTheStartOctant) && (firstXDrawn < 0))
                        {
                            firstXDrawn = workingX;
                            firstYDrawn = workingY;
                        }
                        if (workingPointMapObj.ThisIsTheStopOctant)
                        {
                            lastXDrawn = workingX;
                            lastYDrawn = workingY;
                        }
                        cellsDrawnInQuadrant++;
                    }
                }
                lastX = workingX;
                lastY = workingY;
            }

            // LR quadrant CCW from -Y axis to 45 degree line
            cellsDrawnInQuadrant = 0;
            workingPointMapObj = pointMap[6]; // note hard coded here
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx + ptArray[i].Y;
                workingY = cy - ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                //  DebugMessage("++7 x="+ workingX.ToString() + " y=" + workingY.ToString());
                if (i < workingPointMapObj.CellsToSkip) { } // skip this cell
                else
                {
                    // not skipping, should we draw it?
                    if (cellsDrawnInQuadrant < workingPointMapObj.CellsToDraw)
                    {
                        // yes we should, draw it and count it
                        this.SetBuilderIDOnGSIsoCell(isoPlotObjID, usageFlag, workingX, workingY, wantSoloIsoCell);
                        // record our out vars
                        if ((workingPointMapObj.ThisIsTheStartOctant) && (firstXDrawn < 0))
                        {
                            firstXDrawn = workingX;
                            firstYDrawn = workingY;
                        }
                        if (workingPointMapObj.ThisIsTheStopOctant)
                        {
                            lastXDrawn = workingX;
                            lastYDrawn = workingY;
                        }
                        cellsDrawnInQuadrant++;
                    }
                }

                lastX = workingX;
                lastY = workingY;
            }

            // LR quadrant CCW from 45 degree line to +X axis
            cellsDrawnInQuadrant = 0;
            workingPointMapObj = pointMap[7]; // note hard coded here
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx + ptArray[i].X;
                workingY = cy - ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                if ((workingX == firstX) && (workingY == firstY)) continue;
                //  DebugMessage("++8 x=" + workingX.ToString() + " y=" + workingY.ToString());
                if (i < workingPointMapObj.CellsToSkip) { } // skip this cell
                else
                {
                    // not skipping, should we draw it?
                    if (cellsDrawnInQuadrant < workingPointMapObj.CellsToDraw)
                    {
                        // yes we should, draw it and count it
                        this.SetBuilderIDOnGSIsoCell(isoPlotObjID, usageFlag, workingX, workingY, wantSoloIsoCell);
                        // record our out vars
                        if ((workingPointMapObj.ThisIsTheStartOctant) && (firstXDrawn < 0))
                        {
                            firstXDrawn = workingX;
                            firstYDrawn = workingY;
                        }
                        if (workingPointMapObj.ThisIsTheStopOctant)
                        {
                            lastXDrawn = workingX;
                            lastYDrawn = workingY;
                        }
                        cellsDrawnInQuadrant++;
                    }
                }
                lastX = workingX;
                lastY = workingY;
            }

            // return the object id with no flags
            return isoPlotObjID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a filled circle of a defined radius at a point. Observes the 
        /// graphical stigmergy protocol.
        /// </summary>
        /// <remarks>
        /// Credits: much of algorythm came from 
        ///    http://stackoverflow.com/questions/1201200/fast-algorithm-for-drawing-filled-circles
        ///    However in this particular application we have some specific requirements. When
        ///    we later go through and figure out the IsoPlotSegment_Arc objects (these are the 
        ///    one we convert to GCodes) we must follow the circle linearly cell-by-cell. The
        ///    usual code for this is optimized to place cells in all eight quadrants in one pass.
        ///    This, while faster, cannot be used here. We need to be able to follow the arc around
        ///    checking for intersections and overlays on the background.
        ///    
        ///    Consequently, the algorithm has been unrolled into a number of for loops. It is 
        ///    NOT efficient. I recognise this - but then we are not using it for graphical display
        ///    we are calculating a plot and the drawing time, as long as its not outrageous, is not
        ///    super relevant.
        /// </remarks>
        /// <param name="cx">X center</param>
        /// <param name="cy">Y center</param>
        /// <param name="radius">the radius</param>
        /// <param name="fillMode">the fill mode</param>
        /// <param name="wantClockWise">the wantClockWise value</param>
        /// <param name="isMultiQuadrantArcIn">is an arc specified in multiquadrant mode</param>
        /// <param name="usageFlag">the usage flag we put on the builderID when we write out to the isoPlotCell</param>
        /// <returns>the isoPlotObjID of the circle we created or 0 for fail</returns>
        public int DrawGSCircle(IsoPlotUsageTagFlagEnum usageFlag, int cx, int cy, int radius, GSFillModeEnum fillMode, bool wantClockWise)
        {
            int error = -radius;
            int x = radius;
            int y = 0;
            int ptCount = 0;
            Point[] ptArray = null;
            int ptIndex = 0;
            int lastX = int.MinValue;
            int lastY = int.MinValue;
            int firstX = int.MinValue;
            int firstY = int.MinValue;
            int workingX = int.MinValue;
            int workingY = int.MinValue;
            bool wantSoloIsoCell = false;

            // record this object 
            IsoPlotObject_Circle isoPlotObj = new IsoPlotObject_Circle(this.NextIsoPlotObjectID, cx, cy, radius, wantClockWise);
            isoPlotObjList.Add(isoPlotObj);
            //DebugMessage("DrawGSCircleFilled: ID=" + isoPlotObj.IsoPlotObjectID.ToString() + " (" + cx.ToString() + "," + cy.ToString() + ") radius=" + radius.ToString());
 
            // first we get a count of the points in one section of the arc
            while (x >= y)
            {
                // count this
                ptCount++;
                // calc error
                error += y;
                ++y;
                error += y;
                if (error >= 0)
                {
                    --x;
                    error -= x;
                    error -= x;
                }
            }
            // this should not happen
            if (ptCount == 0)
            {
                LogMessage("DrawGSCircleFilled: ID=" + isoPlotObj.IsoPlotObjectID.ToString() + " ptCount==0");
                return 0;
            }

            // create the array in which we store the points
            ptArray = new Point[ptCount];

            // now put the points in place in the array
            error = -radius;
            x = radius;
            y = 0;
            while (x >= y)
            {
                // create a new point for our x,y
                ptArray[ptIndex] = new Point(x, y);
                ptIndex++;
                // calc error
                error += y;
                ++y;
                error += y;
                if (error >= 0)
                {
                    --x;
                    error -= x;
                    error -= x;
                }
            }
            // ###
            // now go through the quadrants adding points sequentially
            // ###

            // UR quadrant CCW from +X axis up to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx + ptArray[i].X;
                workingY = cy + ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                if(firstX<0)
                {
                    firstX = workingX;
                    firstY = workingY;
                }
               // DebugMessage("++1 x="+ workingX.ToString() + " y=" + workingY.ToString());
                this.SetBuilderIDOnGSIsoCell(isoPlotObj.IsoPlotObjectID, usageFlag, workingX, workingY, wantSoloIsoCell);
                lastX = workingX;
                lastY = workingY;
            }
            // UR quadrant CCW from 45 degree line to +Y axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx + ptArray[i].Y;
                workingY = cy + ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
               // DebugMessage("++2 x="+ workingX.ToString() + " y=" + workingY.ToString());
                this.SetBuilderIDOnGSIsoCell(isoPlotObj.IsoPlotObjectID, usageFlag, workingX, workingY, wantSoloIsoCell);
                lastX = workingX;
                lastY = workingY;
            }
            // UL quadrant CCW from +Y axis to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx - ptArray[i].Y;
                workingY = cy + ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
               // DebugMessage("++3 x="+ workingX.ToString() + " y=" + workingY.ToString());
                this.SetBuilderIDOnGSIsoCell(isoPlotObj.IsoPlotObjectID, usageFlag, workingX, workingY, wantSoloIsoCell);
                lastX = workingX;
                lastY = workingY;
            }
            // UL quadrant CCW from 45 degree line to -X axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx - ptArray[i].X;
                workingY = cy + ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
               // DebugMessage("++4 x="+ workingX.ToString() + " y=" + workingY.ToString());
                this.SetBuilderIDOnGSIsoCell(isoPlotObj.IsoPlotObjectID, usageFlag, workingX, workingY, wantSoloIsoCell);
                lastX = workingX;
                lastY = workingY;
            }
            // LL quadrant CCW from -X axis to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx - ptArray[i].X;
                workingY = cy - ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
               // DebugMessage("++5 x="+ workingX.ToString() + " y=" + workingY.ToString());
                this.SetBuilderIDOnGSIsoCell(isoPlotObj.IsoPlotObjectID, usageFlag, workingX, workingY, wantSoloIsoCell);
                lastX = workingX;
                lastY = workingY;
            }
            // LL quadrant CCW from 45 degree line to -Y axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx - ptArray[i].Y;
                workingY = cy - ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
               // DebugMessage("++6 x="+ workingX.ToString() + " y=" + workingY.ToString());
                this.SetBuilderIDOnGSIsoCell(isoPlotObj.IsoPlotObjectID, usageFlag, workingX, workingY, wantSoloIsoCell);
                lastX = workingX;
                lastY = workingY;
            }
            // LR quadrant CCW from -Y axis to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx + ptArray[i].Y;
                workingY = cy - ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
              //  DebugMessage("++7 x="+ workingX.ToString() + " y=" + workingY.ToString());
                this.SetBuilderIDOnGSIsoCell(isoPlotObj.IsoPlotObjectID, usageFlag, workingX, workingY, wantSoloIsoCell);
                lastX = workingX;
                lastY = workingY;
            }
            // LR quadrant CCW from 45 degree line to +X axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx + ptArray[i].X;
                workingY = cy - ptArray[i].Y;
              //  DebugMessage("++8 x=" + workingX.ToString() + " y=" + workingY.ToString());
                if ((workingX == lastX) && (workingY == lastY)) continue;
                if ((workingX == firstX) && (workingY == firstY)) continue;
                this.SetBuilderIDOnGSIsoCell(isoPlotObj.IsoPlotObjectID, usageFlag, workingX, workingY, wantSoloIsoCell);
                lastX = workingX;
                lastY = workingY;
            }

            // do we want to fill it?
            if (fillMode == GSFillModeEnum.FillMode_BACKGROUND)
            {
                // yes, we do
                BackgroundFillGSByBoundarySimpleHoriz(isoPlotObj.IsoPlotObjectID, cx - radius - 1, cy - radius - 1, cx + radius + 1, cy + radius + 1, -1);
            }
            else if (fillMode == GSFillModeEnum.FillMode_ERASE)
            {
                // we erase everything by creating a list of builderIDs which represent the boundary and 0 id for the fill
                List<int> builderIDList = new List<int>();
                builderIDList.Add(isoPlotObj.IsoPlotObjectID);
                BackgroundFillGSByBoundaryComplexVert(builderIDList, IsoPlotObject.DEFAULT_ISOPLOTOBJECT_ID, cx - radius - 1, cy - radius - 1, cx + radius + 1, cy + radius + 1, -1);
            }


            // return the object id with no flags
            return isoPlotObj.IsoPlotObjectID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to fill in the background of the interior of a region bounded
        /// by a list of isoPlotObjIDs. This function observes the GS protocol and 
        /// will dig around in the overlay arrays to check for edge boundarys.
        /// </summary>
        /// <remarks>
        ///     The way this works is we treat the incoming coords as a rectangle
        ///     then we raster for each y0->y1 we draw a line of background cells
        ///     on the isoPlot from x0 to x1. However we do not set a cell unless we
        ///     have are on or already seen a edge cell. If we see another edge cell
        ///     we consider ourselves to be out of the object and stop writing until
        ///     we see another non-consecutive edge cell belonging to a builder id in
        ///     the list. This permits this function to work for any arbitrarily drawn 
        ///     object as long as it has encoded its isoPlotObjID in the overlays
        /// </remarks>
        /// <param name="builderIDList">a list of the ID's of the isoPlotObjects which form the object boundary</param>
        /// <param name="backgroundBuilderID">the builderID to use for the background</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <param name="maxChordLength">an upper bound on the max fill len. If -ve we ignore</param>
        public void BackgroundFillGSRegionComplex(List<int> builderIDList, int backgroundBuilderID, int x0, int y0, int x1, int y1, int maxChordLength)
        {
            int lx;
            int ly;
            int hx;
            int hy;
            int ptFlags;
            bool thisIsoCellIsRegionEdgeIsoCell = false;
            bool thisIsoCellBelongsToRegion = false;

            // some sanity checks
            if (backgroundBuilderID < 0) return;
            if (builderIDList == null) return;
            if (builderIDList.Count == 0) return;

            if (x0 < 0) return;
            if (x1 < 0) return;
            if (y0 < 0) return;
            if (y1 < 0) return;
            // we have to have a bounding rectangle or there is no point doing this.
            if (x0 == x1) return;
            if (y0 == y1) return;

            // sort our our low and high values
            if (x0 > x1)
            {
                lx = x1;
                hx = x0;
            }
            else
            {
                lx = x0;
                hx = x1;
            }
            if (y0 > y1)
            {
                ly = y1;
                hy = y0;
            }
            else
            {
                ly = y0;
                hy = y1;
            }

            // we start at the low x margin
            for (int xVal = lx; xVal <= hx; xVal++)
            {
                bool regionFillIsActive = false;
                bool lastIsoCellWasEdge = false;
                List<FillLineContainer> fillList = new List<FillLineContainer>();
                FillLineContainer currentFillLine = new FillLineContainer();

                // for each y at this xVal
                for (int yVal = ly; yVal <= hy; yVal++)
                {
                    // get the cell
                    ptFlags = GetTagForIsoPlotObjectAtIsoCell(builderIDList, xVal, yVal);
                    if(ptFlags<=0)
                    {
                        // the cell does not belong to the region of interest
                        thisIsoCellBelongsToRegion = false;
                        // logically it cannot be an edge cell for the region either
                        thisIsoCellIsRegionEdgeIsoCell = false;
                    }
                    else
                    {
                        // the cell does belong to the region of interest
                        thisIsoCellBelongsToRegion = true;
                        // detect if we are an edge cell for this region and set flags
                        if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_CONTOUREDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else thisIsoCellIsRegionEdgeIsoCell = false;
                    }

                    // we have 4 possibilities
                    if ((thisIsoCellBelongsToRegion == false) && (thisIsoCellIsRegionEdgeIsoCell == false))
                    {
                        // not in region, not edge cell, we have to decide if we are filling here
                        // we can only turn this on if we have previously seen an edge cell
                        if (lastIsoCellWasEdge==true)
                        {
                            // last cell was an edge cell this always means toggle the fill state
                            if (regionFillIsActive == true) regionFillIsActive = false;
                            else regionFillIsActive = true;
                        }
                        // do we need to fill this? It may have already been active or it may be newly activated
                        if(regionFillIsActive==true) currentFillLine.SetStartOrEnd(yVal);
                        // mark this now
                        lastIsoCellWasEdge = false;
                    }
                    else if ((thisIsoCellBelongsToRegion == true) && (thisIsoCellIsRegionEdgeIsoCell == false))
                    {
                        // in region but just a background cell of the region, set it as a background cell for the new builder id now
                        currentFillLine.SetStartOrEnd(yVal);
                        lastIsoCellWasEdge = false;
                    }
                    else if ((thisIsoCellBelongsToRegion == true) && (thisIsoCellIsRegionEdgeIsoCell == true))
                    {
                        // in region and is an edge cell. We need to mark this as edge cell for the new new builder id now.
                        if (currentFillLine.IsFullyPopulated() == true)
                        {
                            // we also add the existing fill line to our list
                            fillList.Add(currentFillLine);
                            // create a new one 
                            currentFillLine = new FillLineContainer();
                        }
                        // mark this, we can have multiple edge cells in a sequence
                        lastIsoCellWasEdge = true;
                    }
                    else
                    {
                        // it can never be this 
                        // ((thisIsoCellBelongsToRegion == false) && (thisIsoCellIsRegionEdgeIsoCell == true))
                        lastIsoCellWasEdge = true;
                    }
                } // bottom of for (int yVal = ly; yVal <= hy; yVal++)

                // at this point we have a fillList. Note that if we turned on and did not run
                // accross an an edge cell to finish it we will not have added that fill container
                // to the list. This is exactly the behaviour we want. We do not want a single edge
                // cell to turn on fill if there is no further boundary to turn it off. We need
                // to run across two boundarys to have a valid fill line
                foreach (FillLineContainer fillObj in fillList)
                {
                    // these might not be fully populated
                    if (fillObj.IsFullyPopulated() == false) continue;
                    // do we need to check the chord length?
                    if (maxChordLength > 0)
                    {
                        // we have to check, if our fill is too long just bail out
                        if (Math.Abs(fillObj.EndCoord - fillObj.StartCoord) > maxChordLength) continue;
                    }
                    // we have a good one, populate from start to end inclusive
                    for (int y= fillObj.StartCoord; y<=fillObj.EndCoord; y++)
                    {
                        // set the background cell for the region here
                        SetBuilderIDOnGSIsoCell(backgroundBuilderID, IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_BACKGROUND, xVal, y);
                    }
                }

            } // bottom of for (int xVal = lx; xVal <= hx; xVal++)
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to fill in the background of the interior of a region bounded
        /// by a list of isoPlotObjIDs. This function observes the GS protocol and 
        /// will dig around in the overlay arrays to check for edge boundarys. This
        /// code ignores the fill values and strictly triggers on the edges. It does
        /// cope with multiple edge isoCells in a sequence and will not fill if we are
        /// outside the boundary.
        /// </summary>
        /// <param name="builderIDList">a list of the object id of the cells which form the object boundary</param>
        /// <param name="backgroundBuilderID">the builderID to use for the background</param>
        /// <param name="maxChordLength">an upper bound on the max fill len. If -ve we ignore</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        public void BackgroundFillGSByBoundaryComplexVert(List<int> builderIDList, int backgroundBuilderID, int x0, int y0, int x1, int y1, int maxChordLength)
        {
            int lx;
            int ly;
            int hx;
            int hy;
            int ptFlags;
            bool thisIsoCellIsRegionEdgeIsoCell = false;

            // some sanity checks
            if (backgroundBuilderID < 0) return;
            if (builderIDList == null) return;
            if (builderIDList.Count == 0) return;

            if (x0 < 0) return;
            if (x1 < 0) return;
            if (y0 < 0) return;
            if (y1 < 0) return;
            // we have to have a bounding rectangle or there is no point doing this.
            if (x0 == x1) return;
            if (y0 == y1) return;

            // sort our our low and high values
            if (x0 > x1)
            {
                lx = x1;
                hx = x0;
            }
            else
            {
                lx = x0;
                hx = x1;
            }
            if (y0 > y1)
            {
                ly = y1;
                hy = y0;
            }
            else
            {
                ly = y0;
                hy = y1;
            }

            // we start at the low x margin
            for (int xVal = lx; xVal <= hx; xVal++)
            {
                bool regionFillIsActive = false;
                bool lastIsoCellWasEdge = false;
                List<FillLineContainer> fillList = new List<FillLineContainer>();
                FillLineContainer currentFillLine = new FillLineContainer();

                // for each y at this xVal
                for (int yVal = ly; yVal <= hy; yVal++)
                {
                    // get the cell 
                    ptFlags = GetTagForIsoPlotObjectAtIsoCell(builderIDList, xVal, yVal);
                    if (ptFlags <= 0)
                    {
                        // logically it cannot be an edge cell for the region either
                        thisIsoCellIsRegionEdgeIsoCell = false;
                    }
                    else
                    {
                        // detect if we are an edge cell for this region and set flags
                        if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_CONTOUREDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else thisIsoCellIsRegionEdgeIsoCell = false;
                    }

                    // we have 2 possibilities
                    if (thisIsoCellIsRegionEdgeIsoCell == false)
                    {
                        // not edge cell, we have to decide if we are filling here
                        // we can only turn this on if we have previously, immediately, seen an edge cell
                        if (lastIsoCellWasEdge == true)
                        {
                            // last cell was an edge cell this always means toggle the fill state
                            if (regionFillIsActive == true) regionFillIsActive = false;
                            else regionFillIsActive = true;
                        }
                        // do we need to fill this? It may have already been active or it may be newly activated
                        if (regionFillIsActive == true)
                        {
                            currentFillLine.SetStartOrEnd(yVal);
                        }
                        // mark this now
                        lastIsoCellWasEdge = false;
                    }
                    else // if (thisIsoCellIsRegionEdgeIsoCell == true)
                    {
                        // is an edge cell. Were we filling?
                        if (currentFillLine.IsFullyPopulated()==true)
                        {
                            // yes we were filling, we add the existing fill line to our list
                            fillList.Add(currentFillLine);
                            // create a new one 
                            currentFillLine = new FillLineContainer();
                        }
                        // mark this, we can have multiple edge cells in a sequence
                        lastIsoCellWasEdge = true;
                       // regionFillIsActive = false;
                    }
                } // bottom of for (int yVal = ly; yVal <= hy; yVal++)

                // at this point we have a fillList. Note that if we turned on and did not run
                // accross an an edge isoCell to finish it we will not have added that fill container
                // to the list. This is exactly the behaviour we want. We do not want a single edge
                // cell to turn on fill if there is no further boundary to turn it off. We need
                // to run across two boundarys to have a valid fill line
                foreach (FillLineContainer fillObj in fillList)
                {
                    // these might not be fully populated
                    if (fillObj.IsFullyPopulated() == false) continue;
                    // do we need to check the chord length?
                    if (maxChordLength > 0)
                    {
                        // we have to check, if our fill is too long just bail out
                        if (Math.Abs(fillObj.EndCoord - fillObj.StartCoord) > maxChordLength) continue;
                    }
                    // we have a good one, populate from start to end inclusive
                    for (int y = fillObj.StartCoord; y <= fillObj.EndCoord; y++)
                    {
                        // set the background cell for the region here
                        SetBuilderIDOnGSIsoCell(backgroundBuilderID, IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_BACKGROUND, xVal, y);
                    }
                }

            } // bottom of for (int xVal = lx; xVal <= hx; xVal++)
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to fill in the background of the interior of a region bounded
        /// by a list of isoPlotObjIDs. This function observes the GS protocol and 
        /// will dig around in the overlay arrays to check for edge boundarys. This
        /// code ignores the fill values and strictly triggers on the edges. It does
        /// cope with multiple edge isoCells in a sequence and will not fill if we are
        /// outside the boundary.
        /// </summary>
        /// <param name="builderIDList">a list of the IDs of the isoPlotObjects which form the object boundary</param>
        /// <param name="backgroundBuilderID">the builderID to use for the background</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <param name="maxChordLength">an upper bound on the max fill len. If -ve we ignore</param>
        public void BackgroundFillGSByBoundaryComplexHoriz(List<int> builderIDList, int backgroundBuilderID, int x0, int y0, int x1, int y1, int maxChordLength)
        {
            int lx;
            int ly;
            int hx;
            int hy;
            int ptFlags;
            bool thisIsoCellIsRegionEdgeIsoCell = false;

            // some sanity checks
            if (backgroundBuilderID < 0) return;
            if (builderIDList == null) return;
            if (builderIDList.Count == 0) return;

            if (x0 < 0) return;
            if (x1 < 0) return;
            if (y0 < 0) return;
            if (y1 < 0) return;
            // we have to have a bounding rectangle or there is no point doing this.
            if (x0 == x1) return;
            if (y0 == y1) return;

            // sort our our low and high values
            if (x0 > x1)
            {
                lx = x1;
                hx = x0;
            }
            else
            {
                lx = x0;
                hx = x1;
            }
            if (y0 > y1)
            {
                ly = y1;
                hy = y0;
            }
            else
            {
                ly = y0;
                hy = y1;
            }

            // we start at the low y margin
            for (int yVal = ly; yVal <= hy; yVal++)
            {
                bool regionFillIsActive = false;
                bool lastIsoCellWasEdge = false;
                List<FillLineContainer> fillList = new List<FillLineContainer>();
                FillLineContainer currentFillLine = new FillLineContainer();


                // for each x at this yVal
                for (int xVal = lx; xVal <= hx; xVal++)
                {
                    // get the cell 
                    ptFlags = GetTagForIsoPlotObjectAtIsoCell(builderIDList, xVal, yVal);
                    if (ptFlags <= 0)
                    {
                        // logically it cannot be an edge cell for the region either
                        thisIsoCellIsRegionEdgeIsoCell = false;
                    }
                    else
                    {
                        // detect if we are an edge isoCell for this region and set flags
                        if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_CONTOUREDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else thisIsoCellIsRegionEdgeIsoCell = false;
                    }

                    // we have 2 possibilities
                    if (thisIsoCellIsRegionEdgeIsoCell == false)
                    {
                        // not edge isoCell, we have to decide if we are filling here
                        // we can only turn this on if we have previously, immediately, seen an edge cell
                        if (lastIsoCellWasEdge == true)
                        {
                            // last cell was an edge cell this always means toggle the fill state
                            if (regionFillIsActive == true) regionFillIsActive = false;
                            else regionFillIsActive = true;
                        }
                        // do we need to fill this? It may have already been active or it may be newly activated
                        if (regionFillIsActive == true)
                        {
                            currentFillLine.SetStartOrEnd(xVal);
                        }
                        // mark this now
                        lastIsoCellWasEdge = false;
                    }
                    else // if (thisIsoCellIsRegionEdgeIsoCell == true)
                    {
                        // is an edge cell. Were we filling?
                        if (currentFillLine.IsFullyPopulated() == true)
                        {
                            // yes we were filling, we add the existing fill line to our list
                            fillList.Add(currentFillLine);
                            // create a new one 
                            currentFillLine = new FillLineContainer();
                        }
                        // mark this, we can have multiple edge cells in a sequence
                        lastIsoCellWasEdge = true;
                        // regionFillIsActive = false;
                    }
                } // bottom of for (int xVal = lx; xVal <= hx; xVal++)

                // at this point we have a fillList. Note that if we turned on and did not run
                // across an an edge cell to finish it we will not have added that fill container
                // to the list. This is exactly the behaviour we want. We do not want a single edge
                // cell to turn on fill if there is no further boundary to turn it off. We need
                // to run across two boundarys to have a valid fill line
                foreach (FillLineContainer fillObj in fillList)
                {
                    // these might not be fully populated
                    if (fillObj.IsFullyPopulated() == false) continue;
                    // do we need to check the chord length?
                    if(maxChordLength>0)
                    {
                        // we have to check, if our fill is too long just bail out
                        if (Math.Abs(fillObj.EndCoord - fillObj.StartCoord) > maxChordLength) continue;
                    }
                    // do we need to check the chord length?
                    if(maxChordLength>0)
                    {
                        // we have to check, if our fill is too long just bail out
                        if (Math.Abs(fillObj.EndCoord - fillObj.StartCoord) > maxChordLength) continue;
                    }

                    // we have a good one, populate from start to end inclusive
                    for (int x = fillObj.StartCoord; x <= fillObj.EndCoord; x++)
                    {
                        // set the background cell for the region here
                        SetBuilderIDOnGSIsoCell(backgroundBuilderID, IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_BACKGROUND, x, yVal);
                    }
                }

            } // bottom of for (int yVal = ly; yVal <= hy; yVal++)
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to fill in the background of the interior of a region bounded
        /// by the specified isoPlotObjID. This function observes the GS protocol and 
        /// will dig around in the overlay arrays to check for edge boundarys.
        /// </summary>
        /// <param name="isoPlotObjID">the ID of the isoPlotObjects which form the object boundary</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <param name="maxChordLength">an upper bound on the max fill len. If -ve we ignore</param>
        public void BackgroundFillGSByBoundarySimpleHoriz(int isoPlotObjID, int x0, int y0, int x1, int y1, int maxChordLength)
        {
            int lx;
            int ly;
            int hx;
            int hy;
            int ptFlags;
            bool thisIsoCellIsRegionEdgeIsoCell = false;

            // some sanity checks
            if (isoPlotObjID < 0) return;

            if (x0 < 0) return;
            if (x1 < 0) return;
            if (y0 < 0) return;
            if (y1 < 0) return;
            // we have to have a bounding rectangle or there is no point doing this.
            if (x0 == x1) return;
            if (y0 == y1) return;

            // sort our our low and high values
            if (x0 > x1)
            {
                lx = x1;
                hx = x0;
            }
            else
            {
                lx = x0;
                hx = x1;
            }
            if (y0 > y1)
            {
                ly = y1;
                hy = y0;
            }
            else
            {
                ly = y0;
                hy = y1;
            }

            // we start at the low y margin
            for (int yVal = ly; yVal <= hy; yVal++)
            {
                bool regionFillIsActive = false;
                bool lastIsoCellWasEdge = false;
                List<FillLineContainer> fillList = new List<FillLineContainer>();
                FillLineContainer currentFillLine = new FillLineContainer();

                // for each x at this yVal
                for (int xVal = lx; xVal <= hx; xVal++)
                {
                    // get the tag in the cell 
                    ptFlags = GetTagForIsoPlotObjectAtIsoCell(isoPlotObjID, xVal, yVal);
                    if (ptFlags <= 0)
                    {
                        // logically it cannot be an edge cell for the region either
                        thisIsoCellIsRegionEdgeIsoCell = false;
                    }
                    else
                    {
                        // detect if we are an edge isoCell for this region and set flags
                        if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_CONTOUREDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else thisIsoCellIsRegionEdgeIsoCell = false;
                    }

                    // we have 2 possibilities
                    if (thisIsoCellIsRegionEdgeIsoCell == false)
                    {
                        // not edge isoCell, we have to decide if we are filling here
                        // we can only turn this on if we have previously, immediately, seen an edge cell
                        if (lastIsoCellWasEdge == true)
                        {
                            // last cell was an edge cell this always means toggle the fill state
                            if (regionFillIsActive == true) regionFillIsActive = false;
                            else regionFillIsActive = true;
                        }
                        // do we need to fill this? It may have already been active or it may be newly activated
                        if (regionFillIsActive == true)
                        {
                            currentFillLine.SetStartOrEnd(xVal);
                        }
                        // mark this now
                        lastIsoCellWasEdge = false;
                    }
                    else // if (thisIsoCellIsRegionEdgeIsoCell == true)
                    {
                        // is an edge cell. Were we filling?
                        if (currentFillLine.IsFullyPopulated() == true)
                        {
                            // yes we were filling, we add the existing fill line to our list
                            fillList.Add(currentFillLine);
                            // create a new one 
                            currentFillLine = new FillLineContainer();
                        }
                        // mark this, we can have multiple edge cells in a sequence
                        lastIsoCellWasEdge = true;
                        // regionFillIsActive = false;
                    }
                } // bottom of for (int xVal = lx; xVal <= hx; xVal++)

                // at this point we have a fillList. Note that if we turned on and did not run
                // across an an edge cell to finish it we will not have added that fill container
                // to the list. This is exactly the behaviour we want. We do not want a single edge
                // cell to turn on fill if there is no further boundary to turn it off. We need
                // to run across two boundarys to have a valid fill line
                foreach (FillLineContainer fillObj in fillList)
                {
                    // these might not be fully populated
                    if (fillObj.IsFullyPopulated() == false) continue;
                    // do we need to check the chord length?
                    if (maxChordLength > 0)
                    {
                        // we have to check, if our fill is too long just bail out
                        if (Math.Abs(fillObj.EndCoord - fillObj.StartCoord) > maxChordLength) continue;
                    }
                    // we have a good one, populate from start to end inclusive
                    for (int x = fillObj.StartCoord; x <= fillObj.EndCoord; x++)
                    {
                        // set the background cell for the region here
                        SetBuilderIDOnGSIsoCell(isoPlotObjID, IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_BACKGROUND, x, yVal);
                    }
                }

            } // bottom of for (int yVal = ly; yVal <= hy; yVal++)        
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to erase all isoCells belonging to a builder ID in a region and which
        /// are also in use by another builder ID. 
        /// 
        /// This function observes the GS protocol and will adjust the overlay
        /// to accomplish its function.
        /// </summary>
        /// <param name="isoPlotObjID">the ID of the isoPlotObject which forms the object boundary</param>
        /// <param name="isoPlotObjToErase">the id of the isoPlotObjects we erase in the object boundary. This can be different than the object itself</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        public void EraseABuilderIDFromRegionUsingABuilderID(int isoPlotObjID, int isoPlotObjToErase, int x0, int y0, int x1, int y1)
        {
            // we accept either opposite corners of the bounding rectangle 
            // and sort out the (l) Low and the (h) high values of each;
            int lx;
            int ly;
            int hx;
            int hy;
            int bObj1Tag;
            int bObj2Tag;

            //DebugMessage("EraseABuilderIDFromRegionUsingABuilderID: ID=" + isoPlotObjID.ToString() + " (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // some sanity checks
            if (isoPlotObjID <= 0) return;
            if (isoPlotObjToErase <= 0) return;
            if (x0 < 0) return;
            if (x1 < 0) return;
            if (y0 < 0) return;
            if (y1 < 0) return;
            // we have to have a bounding rectangle or there is no point doing this.
            if (x0 == x1) return;
            if (y0 == y1) return;

            // sort our our low and high values
            if (x0 > x1)
            {
                lx = x1;
                hx = x0;
            }
            else
            {
                lx = x0;
                hx = x1;
            }
            if (y0 > y1)
            {
                ly = y1;
                hy = y0;
            }
            else
            {
                ly = y0;
                hy = y1;
            }

            // now run vertically from ly to hy
            for (int yVal = ly; yVal <= hy; yVal++)
            {
                //DebugMessage("");
                //DebugMessage("Y set to " + yVal.ToString());
                //DebugMessage("");

                // and now run horzontally from lx to hx
                for (int xVal = lx; xVal <= hx; xVal++)
                {
                    // check this point, is it in use by the builder object in any way
                    bObj1Tag = GetTagForIsoPlotObjectAtIsoCell(isoPlotObjID, xVal, yVal);
                    // check this point, is it in use in any way by the builder object to erase
                    bObj2Tag = GetTagForIsoPlotObjectAtIsoCell(isoPlotObjToErase, xVal, yVal);
                    // if both must be active for us to remove isoPlotObjToErase from it
                    if ((bObj1Tag == 0) || (bObj2Tag == 0)) continue;
                    // reset the point for the builder object we are to erase Note we pass the TAG in here
                    ClearBuilderIDFromGSIsoCellByTag(bObj2Tag, xVal, yVal);
                } // bottom of for (int xVal = lx; xVal < hx; xVal++)
            } // bottom of for (int yVal = ly; yVal <= hy; yVal++)
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to erase all isoCells belonging to a builder ID in a region which
        /// are NOT also in use by any one of a list of builder IDs. 
        /// 
        /// This function observes the GS protocol and will adjust the overlay
        /// to accomplish its function.
        /// </summary>
        /// <param name="testBuilderIDList">the ID of the isoPlotObjects which forms the test.</param>
        /// <param name="builderIDToErase">the builderID to erase.</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        public void EraseBuilderIDIfNotOnCellWithIDsInList(List<int> testBuilderIDList, int builderIDToErase, int x0, int y0, int x1, int y1)
        {
            // we accept either opposite corners of the bounding rectangle 
            // and sort out the (l) Low and the (h) high values of each;
            int lx;
            int ly;
            int hx;
            int hy;
            int bObj1Tag;
            int bObj2Tag;

            //DebugMessage("EraseABuilderIDFromRegionUsingABuilderID: ID=" + isoPlotObjID.ToString() + " (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // some sanity checks
            if (testBuilderIDList == null) return;
            if (testBuilderIDList.Count <= 0) return;
            if (builderIDToErase <= 0) return;

            if (x0 < 0) return;
            if (x1 < 0) return;
            if (y0 < 0) return;
            if (y1 < 0) return;
            // we have to have a bounding rectangle or there is no point doing this.
            if (x0 == x1) return;
            if (y0 == y1) return;

            // sort our our low and high values
            if (x0 > x1)
            {
                lx = x1;
                hx = x0;
            }
            else
            {
                lx = x0;
                hx = x1;
            }
            if (y0 > y1)
            {
                ly = y1;
                hy = y0;
            }
            else
            {
                ly = y0;
                hy = y1;
            }

            // now run vertically from ly to hy
            for (int yVal = ly; yVal <= hy; yVal++)
            {
                //DebugMessage("");
                //DebugMessage("Y set to " + yVal.ToString());
                //DebugMessage("");

                // and now run horzontally from lx to hx
                for (int xVal = lx; xVal <= hx; xVal++)
                {
                    // check this point, is it in use by the builder object to erase in any way
                    bObj1Tag = GetTagForIsoPlotObjectAtIsoCell(builderIDToErase, xVal, yVal);
                    // if we not in use just carry on
                    if (bObj1Tag == 0) continue;

                    // ok it is in use, is it in use by the any builder other object other than the ones in the list in any way
                    bObj2Tag = GetTagForIsoPlotObjectNotInListAtIsoCell(testBuilderIDList, builderIDToErase, xVal, yVal);
                    // this will be nz if any other obj in the list is using it
                    if (bObj2Tag <= 0) continue;

                    // at this point we know we should remove the builderIDToErase from having any 
                    // influence on the cell because it is on something other than a builder ID in the list

                    // reset the point for the builder object we are to erase Note we pass the ID in here
                    ClearBuilderIDFromGSIsoCellByID(builderIDToErase, xVal, yVal);

                } // bottom of for (int xVal = lx; xVal < hx; xVal++)
            } // bottom of for (int yVal = ly; yVal <= hy; yVal++)
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to fill in the background of the interior of a region bounded
        /// by a list of isoPlotObjIDs. This function observes the GS protocol and 
        /// will dig around in the overlay arrays to check for edge boundarys. This
        /// code ignores the fill values and strictly triggers on the edges. It does
        /// cope with multiple edge isoCells in a sequence and will not fill if we are
        /// outside the boundary.
        /// </summary>
        /// <param name="eraseBuilderIDList">a list of the IDs of the isoPlotObjects which form the object boundary</param>
        /// <param name="boundaryBuilderID">the builderID which forms the boundary</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <param name="maxChordLength">an upper bound on the max fill len. If -ve we ignore</param>
        public void EraseBuilderIDListFromRegionUsingABuilderIDHoriz(List<int> eraseBuilderIDList, int boundaryBuilderID, int x0, int y0, int x1, int y1, int maxChordLength)
        {
            int lx;
            int ly;
            int hx;
            int hy;
            int ptFlags;
            bool thisIsoCellIsRegionEdgeIsoCell = false;

            // some sanity checks
            if (boundaryBuilderID < 0) return;
            if (eraseBuilderIDList == null) return;
            if (eraseBuilderIDList.Count == 0) return;

            if (x0 < 0) return;
            if (x1 < 0) return;
            if (y0 < 0) return;
            if (y1 < 0) return;
            // we have to have a bounding rectangle or there is no point doing this.
            if (x0 == x1) return;
            if (y0 == y1) return;

            // sort our our low and high values
            if (x0 > x1)
            {
                lx = x1;
                hx = x0;
            }
            else
            {
                lx = x0;
                hx = x1;
            }
            if (y0 > y1)
            {
                ly = y1;
                hy = y0;
            }
            else
            {
                ly = y0;
                hy = y1;
            }

            // we start at the low y margin
            for (int yVal = ly; yVal <= hy; yVal++)
            {
                bool regionFillIsActive = false;
                bool lastIsoCellWasEdge = false;
                List<FillLineContainer> fillList = new List<FillLineContainer>();
                FillLineContainer currentFillLine = new FillLineContainer();


                // for each x at this yVal
                for (int xVal = lx; xVal <= hx; xVal++)
                {
                    // get the cell 
                    ptFlags = GetTagForIsoPlotObjectAtIsoCell(boundaryBuilderID, xVal, yVal);
                    if (ptFlags <= 0)
                    {
                        // logically it cannot be an edge cell for the region either
                        thisIsoCellIsRegionEdgeIsoCell = false;
                    }
                    else
                    {
                        // detect if we are an edge isoCell for this region and set flags
                        if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_CONTOUREDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else if ((ptFlags & ((int)IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE)) != 0) thisIsoCellIsRegionEdgeIsoCell = true;
                        else thisIsoCellIsRegionEdgeIsoCell = false;
                    }

                    // we have 2 possibilities
                    if (thisIsoCellIsRegionEdgeIsoCell == false)
                    {
                        // not edge isoCell, we have to decide if we are filling here
                        // we can only turn this on if we have previously, immediately, seen an edge cell
                        if (lastIsoCellWasEdge == true)
                        {
                            // last cell was an edge cell this always means toggle the fill state
                            if (regionFillIsActive == true) regionFillIsActive = false;
                            else regionFillIsActive = true;
                        }
                        // do we need to fill this? It may have already been active or it may be newly activated
                        if (regionFillIsActive == true)
                        {
                            currentFillLine.SetStartOrEnd(xVal);
                        }
                        // mark this now
                        lastIsoCellWasEdge = false;
                    }
                    else // if (thisIsoCellIsRegionEdgeIsoCell == true)
                    {
                        // is an edge cell. Were we filling?
                        if (currentFillLine.IsFullyPopulated() == true)
                        {
                            // yes we were filling, we add the existing fill line to our list
                            fillList.Add(currentFillLine);
                            // create a new one 
                            currentFillLine = new FillLineContainer();
                        }
                        // mark this, we can have multiple edge cells in a sequence
                        lastIsoCellWasEdge = true;
                        // regionFillIsActive = false;
                    }
                } // bottom of for (int xVal = lx; xVal <= hx; xVal++)

                // at this point we have a fillList. Note that if we turned on and did not run
                // across an an edge cell to finish it we will not have added that fill container
                // to the list. This is exactly the behaviour we want. We do not want a single edge
                // cell to turn on fill if there is no further boundary to turn it off. We need
                // to run across two boundarys to have a valid fill line
                foreach (FillLineContainer fillObj in fillList)
                {
                    // these might not be fully populated
                    if (fillObj.IsFullyPopulated() == false) continue;
                    // do we need to check the chord length?
                    if (maxChordLength > 0)
                    {
                        // we have to check, if our fill is too long just bail out
                        if (Math.Abs(fillObj.EndCoord - fillObj.StartCoord) > maxChordLength) continue;
                    }
                    // do we need to check the chord length?
                    if (maxChordLength > 0)
                    {
                        // we have to check, if our fill is too long just bail out
                        if (Math.Abs(fillObj.EndCoord - fillObj.StartCoord) > maxChordLength) continue;
                    }

                    // we have a good one, populate from start to end inclusive
                    for (int x = fillObj.StartCoord; x <= fillObj.EndCoord; x++)
                    {
                        // erase for every ID (note not Tag) in the list we have
                        foreach (int builderIDToErase in eraseBuilderIDList)
                        {
                            // reset the point for the builder object we are to erase. Note we pass the ID in here
                            ClearBuilderIDFromGSIsoCellByID(builderIDToErase, x, yVal);
                        }
                    }
                }

            } // bottom of for (int yVal = ly; yVal <= hy; yVal++)
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to erase all background isoCells belonging to a builder ID in a region.
        /// Does not erase isoCells which are also edges.
        /// 
        /// This function observes the GS protocol and will adjust the overlay
        /// to accomplish its function.
        /// </summary>
        /// <param name="isoPlotObjID">the ID of the isoPlotObject which forms the object boundary</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        public void EraseBackgroundOnlyIsoCellsByBuilderID(int isoPlotObjID, int x0, int y0, int x1, int y1)
        {
            // we accept either opposite corners of the bounding rectangle 
            // and sort out the (l) Low and the (h) high values of each;
            int lx;
            int ly;
            int hx;
            int hy;
            int bObj1Tag;

            //DebugMessage("EraseBackgroundOnlyIsoCellsByBuilderID: ID=" + isoPlotObjID.ToString() + " (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // some sanity checks
            if (isoPlotObjID <= 0) return;
            if (x0 < 0) return;
            if (x1 < 0) return;
            if (y0 < 0) return;
            if (y1 < 0) return;
            // we have to have a bounding rectangle or there is no point doing this.
            if (x0 == x1) return;
            if (y0 == y1) return;

            // sort our our low and high values
            if (x0 > x1)
            {
                lx = x1;
                hx = x0;
            }
            else
            {
                lx = x0;
                hx = x1;
            }
            if (y0 > y1)
            {
                ly = y1;
                hy = y0;
            }
            else
            {
                ly = y0;
                hy = y1;
            }

            // now run vertically from ly to hy
            for (int yVal = ly; yVal <= hy; yVal++)
            {
                //DebugMessage("");
                //DebugMessage("Y set to " + yVal.ToString());
                //DebugMessage("");

                // and now run horzontally from lx to hx
                for (int xVal = lx; xVal <= hx; xVal++)
                {
                    // check this point, is it in use by the builder object in any way
                    bObj1Tag = GetTagForIsoPlotObjectAtIsoCell(isoPlotObjID, xVal, yVal);
                    // if both must be active for us to remove isoPlotObjToErase from it
                    if (Overlay.TagIsNormalEdgeUsage(bObj1Tag) == true) continue; // skip edge pixels
                    // reset the point for the builder object we are to erase
                    ClearBuilderIDFromGSIsoCellByTag(bObj1Tag, xVal, yVal);  // NOTE we use the Tag here
                } // bottom of for (int xVal = lx; xVal < hx; xVal++)
            } // bottom of for (int yVal = ly; yVal <= hy; yVal++)
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GCode Line segments from a GS line drawn on the iso plot. What we
        /// do is essentially redraw the line but instead of writing out values
        /// we look for sequences of isoCells which are not in the DISREGARD
        /// state. 
        /// </summary>
        /// <remarks>
        /// Credits: this is based the bresnham line drawing algorythm right out of wikipedia
        ///    http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
        ///    Most comments in the code below are from the original source - not me
        /// </remarks>
        /// <param name="isoPlotObjID">the id of the builder object we are testing</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <returns>A List of IsoPlotSegment_Line objects representing the sequences of 
        /// non DISREGARD isoCells on this line or null for fail. Can return an empty list</returns>
        public List<IsoPlotObjectSegment> GetIsoPlotSegmentsFromGSLine(int isoPlotObjID, int x0, int y0, int x1, int y1)
        {
            int ix = int.MinValue;
            int iy = int.MinValue;
            bool retBool;
            IsoPlotObjectSegment isoSegObj = null;

            List<IsoPlotObjectSegment> retList = new List<IsoPlotObjectSegment>();
            //DebugMessage("GetIsoPlotSegmentsFromGSLine: ID=" + isoPlotObjID.ToString() + " (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // sanity check the coordinates here
            if (x0 < 0) return null;
            if (y0 < 0) return null;
            if (x1 >= isoPlotWidth) return null;
            if (y1 >= isoPlotHeight) return null;

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep == true)
            {
                int tmp = x0;
                x0 = y0;
                y0 = tmp;
                tmp = x1;
                x1 = y1;
                y1 = tmp;
            }
            if (x0 > x1)
            {
                int tmp = x0;
                x0 = x1;
                x1 = tmp;
                tmp = y0;
                y0 = y1;
                y1 = tmp;
            }
            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = deltax / 2;
            int ystep;
            int y = y0;
            if (y0 < y1)
            {
                ystep = 1;
            }
            else
            {
                ystep = -1;
            }

            // init this now
            isoSegObj = new IsoPlotObjectSegment_Line(isoPlotObjID, -1, -1, -1, -1);

            for (int x = x0; x <= x1; x++)
            {
                if (steep == true)
                {
                    ix = y;
                    iy = x;
                }
                else
                {
                    ix = x;
                    iy = y;
                }

                // the code which checks each isoCell and builds the segments has been factored
                // out here. This is because we also use it for the arcs and want to maintain
                // one common code base
                retBool = IsoPlotSegmentCollector(isoSegObj, isoPlotObjID, ix,  iy, false);
                // did we add the cell on this call?
                if(retBool==false)
                {
                    // no we did not, is our segment in use?
                    if (isoSegObj.SegmentIsUsed == true)
                    {
                        // yes it is add it to the list, if it is not just a single point (those are artifacts of the aperture join)
                        if (isoSegObj.SegmentIsPoint == false) retList.Add(isoSegObj);
                        // create a new one now
                        isoSegObj = new IsoPlotObjectSegment_Line(isoPlotObjID, -1, -1, -1, -1);
                    }
                    else { } // the existing segment is ok, just use that
                }

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }

            // if we get here we may still have an activeLineSeg. The thing we look for 
            // is wether we have an unused isoCell and that it is not a single point.
            if (isoSegObj.SegmentIsUsed == true)
            {
                // yes it is add it to the list
                if (isoSegObj.SegmentIsPoint == false) retList.Add(isoSegObj);
                // create a new one now
                isoSegObj = null;
            }

            // return the isoPlotObject ID cleaned of any flags
            return retList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Follows a circle around and generates the isoplot segments that we wish to generate
        /// gcode arcs for.
        /// </summary>
        /// <remarks>
        /// Credits: much of algorythm came from 
        ///    http://stackoverflow.com/questions/1201200/fast-algorithm-for-drawing-filled-circles
        ///    However in this particular application we have some specific requirements. When
        ///    we later go through and figure out the IsoPlotSegment_Arc objects (these are the 
        ///    one we convert to GCodes) we must follow the circle linearly cell-by-cell. The
        ///    usual code for this is optimized to place cells in all eight quarants in one pass.
        ///    This, while faster, cannot be used here. We need to be able to follow the arc around
        ///    checking for intersections and overlays on the background.
        ///    
        ///    Consequently, the algorithm has been unrolled into a number of for loops. It is 
        ///    NOT efficient. I recognise this - but then we are not using it for graphical display
        ///    we are calculating a plot and the drawing time, as long as its not outrageous, is not
        ///    super relevant.
        /// </remarks>
        /// <param name="isoPlotObjID">the id of the builder object we are drawing with</param>
        /// <param name="cx">center X coord</param>
        /// <param name="cy">center Y coord</param>
        /// <param name="radius">the radius</param>
        /// <param name="wantClockWise">the wantClockWise value</param>
        /// <returns>A List of IsoPlotSegment_Line objects representing the sequences of 
        /// non DISREGARD cells on this circle or null for fail. Can return an empty list</returns>
        public List<IsoPlotObjectSegment> GetIsoPlotSegmentsFromGSCircle(int isoPlotObjID, int cx, int cy, int radius, bool wantClockWise, bool isMultiQuadrantArc)
        {
            int error = -radius;
            int x = radius;
            int y = 0;
            int ptCount = 0;
            Point[] ptArray = null;
            int ptIndex = 0;
            int workingX = int.MinValue;
            int workingY = int.MinValue;
            int lastX = int.MinValue;
            int lastY = int.MinValue;
            int firstX = int.MinValue;
            int firstY = int.MinValue;

            // used by the IsoPlotSegmentCollector
            bool retBool;
            IsoPlotObjectSegment isoSegObj = null;

            List<IsoPlotObjectSegment> retList = new List<IsoPlotObjectSegment>();

            //DebugMessage("GetIsoPlotSegmentsFromGSCircle: ID=" + isoPlotObjID.ToString() + " (" + cx.ToString() + "," + cy.ToString() + ") radius=" + radius.ToString());

            // first we get a count of the points in one section of the arc
            while (x >= y)
            {
                // count this
                ptCount++;
                // calc error
                error += y;
                ++y;
                error += y;
                if (error >= 0)
                {
                    --x;
                    error -= x;
                    error -= x;
                }
            }
            // this should not happen
            if (ptCount == 0)
            {
                LogMessage("GetIsoPlotSegmentsFromGSCircle: ID=" + isoPlotObjID.ToString() + " ptCount==0");
                return null;
            }

            // create the array in which we store the points
            ptArray = new Point[ptCount];

            // now put the points in place in the array
            error = -radius;
            x = radius;
            y = 0;
            while (x >= y)
            {
                // create a new point for our x,y
                ptArray[ptIndex] = new Point(x, y);
                ptIndex++;
                // calc error
                error += y;
                ++y;
                error += y;
                if (error >= 0)
                {
                    --x;
                    error -= x;
                    error -= x;
                }
            }
            // ###
            // now go through the quadrants checking points sequentially
            // ###

            //if((cx==9536) && (cy==7976) && (radius==49))
            //{
            //    int foo = 1;
            //}
            //else
            //{
            //    return new List<IsoPlotObjectSegment>();
            //}

            // init this now
            isoSegObj = new IsoPlotObjectSegment_Arc(isoPlotObjID, -1, -1, -1, -1, cx, cy, radius, wantClockWise, isMultiQuadrantArc);

            //DebugMessage("pts = " + isoSegObj.PointsAddedCount.ToString());
            // UR quadrant CCW from +X axis up to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx + ptArray[i].X;
                workingY = cy + ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                if(firstX<0)
                {
                    firstX = workingX;
                    firstY = workingY;
                }
                // call the segment calculator with the point, it will add to the retList as required
                retBool = IsoPlotSegmentCollector(isoSegObj, isoPlotObjID, workingX, workingY, true);
                // did we add the cell on this call?
                if (retBool == false)
                {
                    // no we did not, is our segment in use?
                    if (isoSegObj.SegmentIsUsed == true)
                    {
                        // yes it is, add it to the list if it is not just a single point (those are artifacts of the aperture join)
                        if (isoSegObj.PointsAddedCount != 1) retList.Add(isoSegObj);
                        // create a new one now
                        isoSegObj = new IsoPlotObjectSegment_Arc(isoPlotObjID, -1, -1, -1, -1, cx, cy, radius, wantClockWise, isMultiQuadrantArc);
                    }
                    else { } // the existing segment is ok, just use that
                }
                //DebugMessage("IPSC ID=" + isoPlotObjID.ToString() + ", x=" + workingX.ToString() + ", y=" + workingY.ToString());
                lastX = workingX;
                lastY = workingY;
            }
            
            //DebugMessage("pts = " + isoSegObj.PointsAddedCount.ToString());
            // UR quadrant CCW from 45 degree line to +Y axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx + ptArray[i].Y;
                workingY = cy + ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                // DebugMessage("**2 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                retBool = IsoPlotSegmentCollector(isoSegObj, isoPlotObjID, workingX, workingY, true);
                // did we add the cell on this call?
                if (retBool == false)
                {
                    // no we did not, is our segment in use?
                    if (isoSegObj.SegmentIsUsed == true)
                    {
                        // yes it is, add it to the list if it is not just a single point (those are artifacts of the aperture join)
                        if (isoSegObj.PointsAddedCount != 1) retList.Add(isoSegObj);
                        // create a new one now
                        isoSegObj = new IsoPlotObjectSegment_Arc(isoPlotObjID, -1, -1, -1, -1, cx, cy, radius, wantClockWise, isMultiQuadrantArc);
                    }
                    else { } // the existing segment is ok, just use that
                }
                lastX = workingX;
                lastY = workingY;
            }
            
            //DebugMessage("pts = " + isoSegObj.PointsAddedCount.ToString());
            // UL quadrant CCW from +Y axis to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx - ptArray[i].Y;
                workingY = cy + ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                // DebugMessage("**3 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                retBool = IsoPlotSegmentCollector(isoSegObj, isoPlotObjID, workingX, workingY, true);
                // did we add the cell on this call?
                if (retBool == false)
                {
                    // no we did not, is our segment in use?
                    if (isoSegObj.SegmentIsUsed == true)
                    {
                        // yes it is, add it to the list if it is not just a single point (those are artifacts of the aperture join)
                        if (isoSegObj.PointsAddedCount != 1) retList.Add(isoSegObj);
                        // create a new one now
                        isoSegObj = new IsoPlotObjectSegment_Arc(isoPlotObjID, -1, -1, -1, -1, cx, cy, radius, wantClockWise, isMultiQuadrantArc);
                    }
                    else { } // the existing segment is ok, just use that
                }
                lastX = workingX;
                lastY = workingY;
            }
            
            //DebugMessage("pts = " + isoSegObj.PointsAddedCount.ToString());
            // UL quadrant CCW from 45 degree line to -X axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx - ptArray[i].X;
                workingY = cy + ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                //DebugMessage("**4 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                retBool = IsoPlotSegmentCollector(isoSegObj, isoPlotObjID, workingX, workingY, true);
                // did we add the cell on this call?
                if (retBool == false)
                {
                    // no we did not, is our segment in use?
                    if (isoSegObj.SegmentIsUsed == true)
                    {
                        // yes it is, add it to the list if it is not just a single point (those are artifacts of the aperture join)
                        if (isoSegObj.PointsAddedCount != 1) retList.Add(isoSegObj);
                        // create a new one now
                        isoSegObj = new IsoPlotObjectSegment_Arc(isoPlotObjID, -1, -1, -1, -1, cx, cy, radius, wantClockWise, isMultiQuadrantArc);
                    }
                    else { } // the existing segment is ok, just use that
                }
                lastX = workingX;
                lastY = workingY;
            }
            
            //DebugMessage("pts = " + isoSegObj.PointsAddedCount.ToString());
            // LL quadrant CCW from -X axis to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx - ptArray[i].X;
                workingY = cy - ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                // DebugMessage("**5 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                retBool = IsoPlotSegmentCollector(isoSegObj, isoPlotObjID, workingX, workingY, true);
                // did we add the cell on this call?
                if (retBool == false)
                {
                    // no we did not, is our segment in use?
                    if (isoSegObj.SegmentIsUsed == true)
                    {
                        // yes it is, add it to the list if it is not just a single point (those are artifacts of the aperture join)
                        if (isoSegObj.PointsAddedCount != 1) retList.Add(isoSegObj);
                        // create a new one now
                        isoSegObj = new IsoPlotObjectSegment_Arc(isoPlotObjID, -1, -1, -1, -1, cx, cy, radius, wantClockWise, isMultiQuadrantArc);
                    }
                    else { } // the existing segment is ok, just use that
                }
                lastX = workingX;
                lastY = workingY;
            }
            
            //DebugMessage("pts = " + isoSegObj.PointsAddedCount.ToString());
            // LL quadrant CCW from 45 degree line to -Y axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx - ptArray[i].Y;
                workingY = cy - ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                //DebugMessage("**6 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                retBool = IsoPlotSegmentCollector(isoSegObj, isoPlotObjID, workingX, workingY, true);
                // did we add the cell on this call?
                if (retBool == false)
                {
                    // no we did not, is our segment in use?
                    if (isoSegObj.SegmentIsUsed == true)
                    {
                        // yes it is, add it to the list if it is not just a single point (those are artifacts of the aperture join)
                        if (isoSegObj.PointsAddedCount != 1) retList.Add(isoSegObj);
                        // create a new one now
                        isoSegObj = new IsoPlotObjectSegment_Arc(isoPlotObjID, -1, -1, -1, -1, cx, cy, radius, wantClockWise, isMultiQuadrantArc);
                    }
                    else { } // the existing segment is ok, just use that
                }
                lastX = workingX;
                lastY = workingY;
            }
            
            //DebugMessage("pts = " + isoSegObj.PointsAddedCount.ToString());
            // LR quadrant CCW from -Y axis to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx + ptArray[i].Y;
                workingY = cy - ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                //DebugMessage("**7 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                retBool = IsoPlotSegmentCollector(isoSegObj, isoPlotObjID, workingX, workingY, true);
                // did we add the cell on this call?
                if (retBool == false)
                {
                    // no we did not, is our segment in use?
                    if (isoSegObj.SegmentIsUsed == true)
                    {
                        // yes it is, add it to the list if it is not just a single point (those are artifacts of the aperture join)
                        if (isoSegObj.PointsAddedCount != 1) retList.Add(isoSegObj);
                        // create a new one now
                        isoSegObj = new IsoPlotObjectSegment_Arc(isoPlotObjID, -1, -1, -1, -1, cx, cy, radius, wantClockWise, isMultiQuadrantArc);
                    }
                    else { } // the existing segment is ok, just use that
                }
                lastX = workingX;
                lastY = workingY;
            }
            
            //DebugMessage("pts = " + isoSegObj.PointsAddedCount.ToString());
            // LR quadrant CCW from 45 degree line to +X axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx + ptArray[i].X;
                workingY = cy - ptArray[i].Y;
                //DebugMessage("**8 x=" + workingX.ToString() + " y=" + workingY.ToString());
                if ((workingX == lastX) && (workingY == lastY)) continue;
                // NOTE: we do NOT do this check here. We need to have a look at
                //       the first cell we did so we can join up circles etc
                // *** if ((workingX == firstX) && (workingY == firstY)) continue;
                // call the segment calculator with the point, it will add to the retList as required
                retBool = IsoPlotSegmentCollector(isoSegObj, isoPlotObjID, workingX, workingY, true);
                // did we add the cell on this call?
                if (retBool == false)
                {
                    // no we did not, is our segment in use?
                    if (isoSegObj.SegmentIsUsed == true)
                    {
                        // yes it is, add it to the list if it is not just a single point (those are artifacts of the aperture join)
                        if (isoSegObj.PointsAddedCount != 1) retList.Add(isoSegObj);
                        // create a new one now
                        isoSegObj = new IsoPlotObjectSegment_Arc(isoPlotObjID, -1, -1, -1, -1, cx, cy, radius, wantClockWise, isMultiQuadrantArc);
                    }
                    else { } // the existing segment is ok, just use that
                }
                lastX = workingX;
                lastY = workingY;
            }

            // if we get here we may still have an activeLineSeg. The thing we look for 
            // is wether we have an unused isoCell and that it is not a single point.
            if (isoSegObj.SegmentIsUsed == true)
            {
                // yes it is add it to the list
                if (isoSegObj.PointsAddedCount != 1) retList.Add(isoSegObj);
                // create a new one now
                isoSegObj = null;
            }

            // just return it
            return retList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts a isoCell coord in the isoplot and keeps track of which isoCells form
        /// contiguous isoPlotSegments. If it finds a valid segment it will add it to 
        /// the supplied isoSegObj.
        /// </summary>
        /// <remarks>Call this for every cell in a line or circle on the isoPlot. If it
        /// returns false create a new isoSegObj object before calling it again. Note
        /// you MUST proceed sequentially along the entity.
        /// </remarks>
        /// <param name="ix">x cell cooord</param>
        /// <param name="iy">y cell cooord</param>
        /// <param name="isoPlotObjID">the id of the owner isoPlot, only usedif mustCheckOwner is true</param>
        /// <param name="isoSegObj">the segment object we populate</param>
        /// <param name="mustCheckOwner">if true we use the isoPlotObjID to check ownership of the cell</param>
        /// <returns>true we populated, false we failed also the isoSegObj is populated</returns>
        private bool IsoPlotSegmentCollector(IsoPlotObjectSegment isoSegObj, int isoPlotObjID, int ix, int iy, bool mustCheckOwner)
        {
            // get the isocell
            int overlayID = 0;
            Overlay overlayObj = null;

            try
            {
                // get the overlay id from our isocell
                overlayID = IsolationPlotArray[ix, iy];
                int disregardFlag = (overlayID & ((int)OverlayFlagEnum.OverlayFlag_DISREGARD));
                int unusedFlag = this.CleanFlagsFromOverlayID(overlayID);

                // are we a disregard cell or are we unused?
                if ((disregardFlag != 0) || (unusedFlag == OverlayCollection.EMPTY_OVERLAY_ID)) return false;

                // always get the overlay object
                overlayObj = CurrentOverlayCollection[overlayID];

                // some segment types (such as arcs) need a check for ownership of the isoplot cell because
                // they do not write to all of the ones we run over in our processing. Others such as lines
                // and circles only ever process the ones they own.
                if (mustCheckOwner == true)
                {
                    // now we have done the quick checks, test the cell actually belongs to the isoPlotObjID
                    // we assume not null here for speed
                    if (overlayObj.DoesOverlayContainIsoPlotID(isoPlotObjID) == false) return false;
                }

                //DebugMessage("[ix, iy] =(" + ix.ToString() + "," + iy.ToString() + "), disregardFlag =" + disregardFlag.ToString("x8") + ", unusedFlag =" + unusedFlag.ToString());

                // not an invalid cell, and it belongs to our isoPlotObjID so we just record it
                isoSegObj.AddNewXAndYToSegment(ix, iy, overlayObj);

                // let the caller know this worked
                return true;
            }
            catch (Exception ex)
            {
                LogMessage("IsoPlotSegmentCollector Error at point (" + ix.ToString() + "," + iy.ToString() + ")");
                throw ex;
            }


        }

        #endregion



    }
}

