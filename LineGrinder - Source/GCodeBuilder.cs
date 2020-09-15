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
    /// <history>
    ///    22 Jul 10  Cynic - Started
    /// </history>
    public class GCodeBuilder : OISObjBase
    {
        // this is the array of ints we use to figure out our isolation cuts.
        // We refer to the cells in this array as pixels even though they
        // are never directly visible to the user graphically. It is just a
        // convenient terminology
        private int[,] isolationPlotArray = null;
        private int isoPlotWidth = ctlPlotViewer.DEFAULT_PLOT_WIDTH;
        private int isoPlotHeight = ctlPlotViewer.DEFAULT_PLOT_HEIGHT;
        // this contains the IsoPlotSegments after isolationPlot Step3
        private List<IsoPlotSegment> isoPlotStep3List = null;

        // this value in the isolation array means no GCodeBuilderObject
        // is using that pixel for any purpose
        private const int PIXEL_NOTUSED = 0;

        // this id is incremented and assigned to every gerber object we draw
        private int currentGerberObjID = 0;
        // this is a list of all gerber objects we have assigned
        private List<GCodeBuilderObject> builderObjList = new List<GCodeBuilderObject>();

        // these are the options we use when creating GCode file output
        private FileManager gcodeBuilderFileManager = new FileManager();

        private GCodeFile currentGCodeFile = new GCodeFile();

        // if multiple builderObjects need to use a pixel in the isoPlotArray we 
        // compile their builderObjectIDs into a list and stuff that list into a dictionary
        // in this manner we can figure out which builderObjects are using any particular 
        // pixel and for what purpose. The collection below contains all of the allocated
        // GCodeBuilderObjectOverlay objects
        private GCodeBuilderObjectOverlayCollection overlayCollection = new GCodeBuilderObjectOverlayCollection();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="widthIn">The width of the plot</param>
        /// <param name="heightIn">the height of the plot</param>
        /// <history>
        ///    22 Jul 10  Cynic - Started
        /// </history>
        public GCodeBuilder(int widthIn, int heightIn)
        {
            isoPlotWidth = widthIn;
            isoPlotHeight = heightIn;
            // sanity checks
            if (isoPlotWidth <= 0) isoPlotWidth = ctlPlotViewer.DEFAULT_PLOT_WIDTH;
            if (isoPlotHeight <= 0) isoPlotHeight = ctlPlotViewer.DEFAULT_PLOT_HEIGHT;
            // create the plot now
            isolationPlotArray = new int[isoPlotWidth, isoPlotHeight];
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets next Gerber object id. These are assigned sequentially as we draw
        /// the object on the isolationPlot
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        private int NextGerberObjectID
        {
            get
            {
                currentGerberObjID++;
                // we cannot cope with greater than 2^24 objects.
                if (currentGerberObjID >= 16777216)
                {
                    throw new NotImplementedException("NextGerberObjectID >= 2^24. Way too many objects. This should not happen");
                }
                return currentGerberObjID;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the overlaycollection. Will never return a null
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public GCodeBuilderObjectOverlayCollection OverlayCollection
        {
            get
            {
                if (overlayCollection == null)
                {
                    overlayCollection = new GCodeBuilderObjectOverlayCollection();
                }
                return overlayCollection;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the file options to use. Never gets/sets a null value
        /// </summary>
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
        public FileManager GCodeBuilderFileManager
        {
            get
            {
                if (gcodeBuilderFileManager == null) gcodeBuilderFileManager = new FileManager();
                return gcodeBuilderFileManager;
            }
            set
            {
                gcodeBuilderFileManager = value;
                if (gcodeBuilderFileManager == null) gcodeBuilderFileManager = new FileManager();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the isoPlotStep3List. Will never return a null
        /// </summary>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public List<IsoPlotSegment> IsoPlotStep3List
        {
            get
            {
                if (isoPlotStep3List == null) isoPlotStep3List = new List<IsoPlotSegment>();
                return isoPlotStep3List;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the isolation plot array, will never get a null, This is usually set
        /// to the proper dimensions when the PerformGerberToGCodeStep1 is called
        /// </summary>
        /// <history>
        ///    24 Jul 10  Cynic - Started
        /// </history>
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
        /// then youw will not see them. 
        /// </summary>
        /// <remarks>
        /// Credits: this follows one of the examples in here
        ///  http://stackoverflow.com/questions/392324/converting-an-array-of-pixels-to-an-image-in-c
        ///    Most comments in the code below are from the original source - not me
        /// </remarks>
        /// <returns>A bitmap which is the same size as the virtualPlotSize and which
        /// has the pixels with various attributes (intersection, runover background etc)
        /// colored in useful ways</returns>
        /// <param name="displayModeIn">the display mode, this changes the interpretation</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public Bitmap GetIsolationArrayAsBitmap(ctlPlotViewer.DisplayModeEnum displayModeIn)
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
                            //Color tmpColor = ConvertIsoPlotValueToColor(displayModeIn, x, y);
                            //row[x] = tmpColor.ToArgb();
                            row[x] = ConvertIsoPlotValueToArgb(displayModeIn, x, y);
                            //row[x] = ColorManager.ISOBACKGROUND_COLOR_AS_UINT;
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
        /// Converts the isolation plot pixels to colors. This makes it possible
        /// to graphically display the usage of each pixel in the screen.
        /// 
        /// The color usage is:
        ///   ISOINTERSECTION_COLOR: Used as an edge by two or more builderObjects (an Intersection)
        ///   ISOEDGE_COLOR:         Used by a single builderObject as an edge
        ///   ISOOVERLAY_COLOR:      Used by two or more builderObjects as a background
        ///   ISOINTERIOR_COLOR:     Used by a single builderObject as an background
        ///   ISOBACKGROUND_COLOR:   Used by no builderObjects
        /// 
        /// </summary>
        /// <remarks>
        /// Does NOT check for x,y coordinats being sound. 
        /// Colors here must be synced with the ones in ConvertIsoPlotValueToArgb
        /// </remarks>
        /// <param name="x">x coord on isoplot</param>
        /// <param name="y">y coord on isoplot</param>
        /// <param name="displayModeIn">the display mode, this changes the interpretation</param>
        /// <history>
        ///    28 Jul 10  Cynic - Started
        ///    21 Aug 10  Cynic - Optimized for speed
        /// </history>
        private Color ConvertIsoPlotValueToColor(ctlPlotViewer.DisplayModeEnum displayModeIn, int x, int y)
        {
            // this short circuits the tests below and speeds up the production of the bitmap
            // for most isoplots this will trigger 50%+ of the time (estimated)
            if (isolationPlotArray[x, y] == 0) return ApplicationColorManager.ISOBACKGROUND_COLOR;

            if (displayModeIn == ctlPlotViewer.DisplayModeEnum.DisplayMode_ISOSTEP1)
            {
                // are we an edge?
                if (IsoPlotPointIsEdge(x, y) == true)
                {
                    // yes we are, perform these tests 
                    int tmpCount = IsoPlotPointEdgeUsageCount(x, y);
                    if (tmpCount >= 2)
                    {
                        // we have an intersection of two edge pixels
                        return ApplicationColorManager.ISOINTERSECTION_COLOR;
                    }
                    else if (tmpCount != 0)
                    {
                        // we have a single edge pixel
                        return ApplicationColorManager.ISOEDGE_COLOR;
                    }
                    else
                    {
                        // should not happen
                        return ApplicationColorManager.ISOBACKGROUND_COLOR;
                    }
                }
                // are we an overlay
                if (IsoPlotPointIsOverlay(x, y) == true)
                {
                    // we have an overlay background pixel
                    return ApplicationColorManager.ISOOVERLAY_COLOR;
                }

                if (IsoPlotPointIsBackground(x, y) == true)
                {
                    // we have an single background usage pixel
                    return ApplicationColorManager.ISOINTERIOR_COLOR;
                }

                // not used, return this
                return ApplicationColorManager.ISOBACKGROUND_COLOR;

            } // bottom of if (displayModeIn == ctlPlotViewer.DisplayModeEnum.DisplayMode_ISOSTEP1)
            else
            {
                if (IsoPlotPointIsDisregard(x, y) == true)
                {
                    // in this mode we draw it as a background pixel
                    return ApplicationColorManager.ISOINTERIOR_COLOR;
                }
                int tmpCount = IsoPlotPointEdgeUsageCount(x, y);
                if (tmpCount >= 2)
                {
                    // we have an intersection of two edge pixels
                    return ApplicationColorManager.ISOINTERSECTION_COLOR;
                }
                if (tmpCount != 0)
                {
                    // we have a single edge pixel
                    return ApplicationColorManager.ISOEDGE_COLOR;
                }
                if (IsoPlotPointIsOverlay(x, y) == true)
                {
                    // we have an overlay background pixel
                    return ApplicationColorManager.ISOOVERLAY_COLOR;
                }
                if (IsoPlotPointIsBackground(x, y) == true)
                {
                    // we have an single background usage pixel
                    return ApplicationColorManager.ISOINTERIOR_COLOR;
                }
                // not used, return this
                return ApplicationColorManager.ISOBACKGROUND_COLOR;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the isolation plot pixels to colors. This makes it possible
        /// to graphically display the usage of each pixel in the screen.
        /// 
        /// The color usage is:
        ///   ISOINTERSECTION_COLOR: Used as an edge by two or more builderObjects (an Intersection)
        ///   ISOEDGE_COLOR:         Used by a single builderObject as an edge
        ///   ISOOVERLAY_COLOR:      Used by two or more builderObjects as a background
        ///   ISOINTERIOR_COLOR:     Used by a single builderObject as an background
        ///   ISOBACKGROUND_COLOR:   Used by no builderObjects
        /// 
        /// </summary>
        /// <remarks>
        /// Does NOT check for x,y coordinats being sound. 
        /// Colors here must be synced with the ones in ConvertIsoPlotValueToColor 
        /// </remarks>
        /// <param name="x">x coord on isoplot</param>
        /// <param name="y">y coord on isoplot</param>
        /// <param name="displayModeIn">the display mode, this changes the interpretation</param>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
        private uint ConvertIsoPlotValueToArgb(ctlPlotViewer.DisplayModeEnum displayModeIn, int x, int y)
        {
            // this short circuits the tests below and speeds up the production of the bitmap
            // for most isoplots this will trigger 50%+ of the time (estimated)
            if (isolationPlotArray[x, y] == 0) return ApplicationColorManager.ISOBACKGROUND_COLOR_AS_UINT;

            if (displayModeIn == ctlPlotViewer.DisplayModeEnum.DisplayMode_ISOSTEP1)
            {
                // are we an edge?
                if (IsoPlotPointIsEdge(x, y) == true)
                {
                    // yes we are, perform these tests 
                    int tmpCount = IsoPlotPointEdgeUsageCount(x, y);
                    if (tmpCount >= 2)
                    {
                        // we have an intersection of two edge pixels
                        return ApplicationColorManager.ISOINTERSECTION_COLOR_AS_UINT;
                    }
                    else if (tmpCount != 0)
                    {
                        // we have a single edge pixel
                        return ApplicationColorManager.ISOEDGE_COLOR_AS_UINT;
                    }
                    else
                    {
                        // should not happen
                        return ApplicationColorManager.ISOBACKGROUND_COLOR_AS_UINT;
                    }
                }
                // are we an overlay
                if (IsoPlotPointIsOverlay(x, y) == true)
                {
                    // we have an overlay background pixel
                    return ApplicationColorManager.ISOOVERLAY_COLOR_AS_UINT;
                }

                if (IsoPlotPointIsBackground(x, y) == true)
                {
                    // we have an single background usage pixel
                    return ApplicationColorManager.ISOINTERIOR_COLOR_AS_UINT;
                }

                // not used, return this
                return ApplicationColorManager.ISOBACKGROUND_COLOR_AS_UINT;

            } // bottom of if (displayModeIn == ctlPlotViewer.DisplayModeEnum.DisplayMode_ISOSTEP1)
            else
            {
                if (IsoPlotPointIsDisregard(x, y) == true)
                {
                    // in this mode we draw it as a background pixel
                    return ApplicationColorManager.ISOINTERIOR_COLOR_AS_UINT;
                }
                int tmpCount = IsoPlotPointEdgeUsageCount(x, y);
                if (tmpCount >= 2)
                {
                    // we have an intersection of two edge pixels
                    return ApplicationColorManager.ISOINTERSECTION_COLOR_AS_UINT;
                }
                if (tmpCount != 0)
                {
                    // we have a single edge pixel
                    return ApplicationColorManager.ISOEDGE_COLOR_AS_UINT;
                }
                if (IsoPlotPointIsOverlay(x, y) == true)
                {
                    // we have an overlay background pixel
                    return ApplicationColorManager.ISOOVERLAY_COLOR_AS_UINT;
                }
                if (IsoPlotPointIsBackground(x, y) == true)
                {
                    // we have an single background usage pixel
                    return ApplicationColorManager.ISOINTERIOR_COLOR_AS_UINT;
                }
                // not used, return this
                return ApplicationColorManager.ISOBACKGROUND_COLOR_AS_UINT;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We check if a plot in the isolation array is an overlay or not
        /// </summary>
        /// <remarks>Does NOT check for x,y coordinats being sound</remarks>
        /// <history>
        ///    28 Jul 10  Cynic - Started
        /// </history>
        private bool IsoPlotPointIsOverlay(int x, int y)
        {
            int plotPixelVal = IsolationPlotArray[x, y];
            if ((plotPixelVal & ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY)) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We check if a plot in the isolation array is an edge or not
        /// </summary>
        /// <remarks>Does NOT check for x,y coordinats being sound</remarks>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
        private bool IsoPlotPointIsEdge(int x, int y)
        {
            int plotPixelVal = IsolationPlotArray[x, y];
            if ((plotPixelVal & ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_EDGE)) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We check if a plot in the isolation array is an background or not
        /// </summary>
        /// <remarks>Does NOT check for x,y coordinats being sound</remarks>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
        private bool IsoPlotPointIsBackground(int x, int y)
        {
            int plotPixelVal = IsolationPlotArray[x, y];
            if ((plotPixelVal & ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_BACKGROUND)) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We check if a plot in the isolation array is an disregard or not
        /// </summary>
        /// <remarks>Does NOT check for x,y coordinats being sound</remarks>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        private bool IsoPlotPointIsDisregard(int x, int y)
        {
            int plotPixelVal = IsolationPlotArray[x, y];
            if ((plotPixelVal & ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_DISREGARD)) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We get the number of GCodeBuilderObjects which are using that point
        /// as a background.
        /// </summary>
        /// <remarks>Does NOT check for x,y coordinats being sound</remarks>
        /// <history>
        ///    28 Jul 10  Cynic - Started
        /// </history>
        private int IsoPlotPointBackgroundUsageCount(int x, int y)
        {
            int plotPixelVal = IsolationPlotArray[x, y];
            if ((plotPixelVal & ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY)) != 0)
            {
                // we do have an overlay, check this now, just use the indexer
                // to see if it is there. This will only use the lower 24 bits
                // for the search
                GCodeBuilderObjectOverlay tmpObj = OverlayCollection[plotPixelVal];
                if (tmpObj == null) return 0;
                return tmpObj.BackgroundPixelCount;
            }
            else
            {
                // we are not an overlay, conduct single pixel test
                if ((plotPixelVal & ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_BACKGROUND)) != 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We get the number of GCodeBuilderObjects which are using that point
        /// as a edge.
        /// </summary>
        /// <remarks>Does NOT check for x,y coordinats being sound</remarks>
        /// <history>
        ///    28 Jul 10  Cynic - Started
        /// </history>
        private int IsoPlotPointEdgeUsageCount(int x, int y)
        {
            int plotPixelVal = IsolationPlotArray[x, y];
            if ((plotPixelVal & ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY)) != 0)
            {
                // we do have an overlay, check this now, just use the indexer
                // to see if it is there. This will only use the lower 24 bits
                // for the search
                GCodeBuilderObjectOverlay tmpObj = OverlayCollection[plotPixelVal];
                if (tmpObj == null) return 0;
                return tmpObj.EdgePixelCount;
            }
            else
            {
                // we are not an overlay, conduct single pixel test
                if ((plotPixelVal & ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_EDGE)) != 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Paints the input graphic object with the contents of the gerber file
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <history>
        ///    02 Aug 10  Cynic - Started
        /// </history>
        public void PlotIsoStep3(Graphics graphicsObj, float isoPlotPointsPerAppUnit)
        {
            int errInt = 0;
            string errStr = "";
            IsoPlotSegment.PlotActionEnum errAction = IsoPlotSegment.PlotActionEnum.PlotAction_End;

            foreach (IsoPlotSegment segObj in IsoPlotStep3List)
            {
                errAction = segObj.PerformPlotIsoStep3Action(graphicsObj, ref errInt, ref errStr);
                if (errAction == IsoPlotSegment.PlotActionEnum.PlotAction_Continue)
                {
                    // all is well
                    continue;
                }
                if (errAction == IsoPlotSegment.PlotActionEnum.PlotAction_End)
                {
                    // we are all done
                    return;
                }
                else if (errAction == IsoPlotSegment.PlotActionEnum.PlotAction_FailWithError)
                {
                    // handle this error
                    LogMessage("PlotIsoStep3 Failed on obj:" + segObj.ToString());
                    return;
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
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public int PerformSecondaryIsolationArrayProcessing(ref string errStr)
        {
            int retInt;
            retInt = PerformIsolationPlotInteriorPixelRemoval(ref errStr);
            if (retInt != 0)
            {
                LogMessage("PerformSecondaryIsolationArrayProcessing call to PerformIsolationPlotInteriorPixelRemoval returned "+retInt.ToString());
                return 333;
            }
            retInt = PerformIsoPlotSegmentDiscovery(ref errStr);
            if (retInt != 0)
            {
                LogMessage("PerformSecondaryIsolationArrayProcessing call to PerformIsoPlotSegmentDiscovery returned " + retInt.ToString());
                return 334;
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the IsoPlotSegments discovered in PerformIsoPlotSegmentDiscovery
        /// into GCodeLine objects and populates the supplied GCodeFile object
        /// </summary>
        /// <param name="errStr">a helpful error message</param>
        /// <param name="gcFile">the GCodeFile object we populate</param>
        /// <returns>z success, nz fail</returns>
        /// <history>
        ///    05 Jul 10  Cynic - Started
        ///    23 Sep 10  Cynic - Added better code to find the outside chain for edge milling
        ///    03 Oct 10  Cynic - Now fixed to set the TabLength in the isosegment
        /// </history>
        public int ConvertIsolationSegmentsToGCode(ref GCodeFile gcFile, ref string errStr)
        {
            int retInt;
            List<IsoPlotSegment> tmpList = null;

            errStr="";

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

            // a generic list of generic lists - ooooohhh yummy! One does not get the 
            // opportunity to do that very often
            List<List<IsoPlotSegment>> listOfIsoPlotSegmentLists = new List<List<IsoPlotSegment>>();

            // for each isoPlotSegment, try to build a chain for it. This 
            // gets less efficient as we go down the list since we will be
            // testing objects which are higly probable to have been already
            // chained up but the recursive GetIsoChainForPoint calls
            foreach(IsoPlotSegment segObj in isoPlotStep3List)
            {
                // create a new list 
                tmpList = new List<IsoPlotSegment>();
                // do we need to consider this one?
                if (segObj.SegmentChained == true)
                {
                    // now we do not, it will have been added
                    continue;
                }
                // find all the isoPlotSegments Chained to this object
                retInt = GetIsoChainForPoint(ref tmpList, segObj.X0, segObj.Y0);
                if (retInt < 0)
                {
                    errStr = "Isolation Step3 error "+retInt.ToString() + " happened when finding isolation chains";
                    LogMessage("GetIsoChainForPoint: " + errStr);
                    return 243;
                }
                // add the chain list to our list of lists
                listOfIsoPlotSegmentLists.Add(tmpList);
            }

            // ok now we have a list of chains we still have a problem. The
            // segments might be sequential but they might still be 
            // reversed relative to each other. This
            // would necessarily mean that we have to lift the bit up when
            // we cut them. For example, we could have two line segments
            // (0,0) to (1,1) and (4,2) to (1,1) these do not flow together
            // we need flip one of them around on conversion. In that case 
            // the above example would be (0,0) to (1,1) and (1,1) to (4,2) 
            // that way we flow nicely from the end of one segment into the
            // start of the next.
            int curX;
            int curY;
            foreach (List<IsoPlotSegment> chainList in listOfIsoPlotSegmentLists)
            {
                curX = -1;
                curY = -1;
                // now run over each segObj in the chainList
                foreach (IsoPlotSegment segObj in chainList)
                {
                    // are we just starting the chain?
                    if ((curX < 0) || (curY < 0))
                    {
                        // yes we are, record the tail of the current segObj
                        curX = segObj.X1;
                        curY = segObj.Y1;
                        segObj.NeedFlipOnConversionToGCode = false;
                        continue;
                    }
                    // no, we are not just starting, do curX and curY 
                    // match the X0,Y0 pair?
                    if ((curX == segObj.X0) && (curY == segObj.Y0))
                    {
                        // yes, they do, object is in correct orientation
                        segObj.NeedFlipOnConversionToGCode = false;
                        curX = segObj.X1;
                        curY = segObj.Y1;
                    }
                    else
                    {
                        // no, they do not, we need to flip when we convert
                        // we assume here that X1 and Y1 match curX and curY
                        // we are in a chain after all so something has to match
                        segObj.NeedFlipOnConversionToGCode = true;
                        curX = segObj.X0;
                        curY = segObj.Y0;
                    }
                }
            }

/*
            DebugMessage("listOfIsoPlotSegmentLists.Count=" + listOfIsoPlotSegmentLists.Count.ToString());
            foreach (List<IsoPlotSegment> chainList in listOfIsoPlotSegmentLists)
            {
                DebugMessage("  chainList.Count=" + chainList.Count.ToString());
                foreach (IsoPlotSegment segObj in chainList)
                {
                    DebugMessage("  SegObj=" + segObj.ToString());
                }
            }
*/
            // what mode are we in?
            if (GCodeBuilderFileManager.OperationMode == FileManager.OperationModeEnum.BoardEdgeMill)
            {
                // if we are in edge mill mode we have to figure out what chain (or chains) contain
                // the outside segments
                int minX = int.MaxValue;
                int minY = int.MaxValue;
                int maxX = int.MinValue;
                int maxY = int.MinValue;
                List<IsoPlotSegment> outsideChainList = null;
                foreach (List<IsoPlotSegment> chainList in listOfIsoPlotSegmentLists)
                {
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
                    errStr = "No Outside Chain List Could Be Found";
                    LogMessage("ConvertIsolationSegmentsToGCode outsideChainList == null");
                    return 431;
                }

                // do we need to apply tabs
                if ((gcFile.StateMachine.GCodeFileManager.EdgeMillNumTabs > 0) && (gcFile.StateMachine.GCodeFileManager.EdgeMillTabWidth > 0))
                {
                    // yes we need to apply tabs
                    List<IsoPlotSegment_Line> lineList = GetIsoPlotLineSegmentsOrderedBySize(gcFile.StateMachine, outsideChainList, (gcFile.StateMachine.GCodeFileManager.EdgeMillTabWidth * 3));
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
                    foreach (IsoPlotSegment_Line segObj in lineList)
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
                foreach (IsoPlotSegment segObj in outsideChainList)
                {
                    // build the GCodeLines and add it to our GCodeFile 
                    List<GCodeLine> outList = segObj.GetGCodeLines(gcFile.StateMachine);
                    if (outList != null)
                    {
                        foreach (GCodeLine lineObj in outList)
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
                foreach (List<IsoPlotSegment> chainList in listOfIsoPlotSegmentLists)
                {
                    // now for each segObj in the chainList
                    foreach (IsoPlotSegment segObj in chainList)
                    {
                        // build the GCodeLines and add it to our GCodeFile 
                        List<GCodeLine> outList = segObj.GetGCodeLines(gcFile.StateMachine);
                        if (outList != null)
                        {
                            foreach (GCodeLine lineObj in outList)
                            {
                                gcFile.AddLine(lineObj);
                            }
                        }
                    }
                }
            }
            return 0;
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
        /// <history>
        ///    28 Sep 10  Cynic - Started
        /// </history>
        private List<IsoPlotSegment_Line> GetIsoPlotLineSegmentsOrderedBySize(GCodeFileStateMachine stateMachine, List<IsoPlotSegment> outsideChainList, float minSize)
        {
            List<IsoPlotSegment_Line> retList = new List<IsoPlotSegment_Line>();

            // sanity check
            if (outsideChainList == null) return retList;

            // add the lines
            foreach (IsoPlotSegment segObj in outsideChainList)
            {
                // has to be a line
                if ((segObj is IsoPlotSegment_Line) == false) continue;
                // we must calculate the segment length
                (segObj as IsoPlotSegment_Line).CalcIsoSegmentLength(stateMachine);
                if (minSize > 0)
                {
                    if ((segObj as IsoPlotSegment_Line).IsoSegmentLength < minSize) continue;
                }
                // we are good to add
                retList.Add((segObj as IsoPlotSegment_Line));
            }
            // now we have to sort it on the IsoSegmentLength
            retList.Sort(IsoPlotSegment_Line.LengthComparison);

            return retList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tests the input chain to see if any of the IsoPlotSegments it contains
        /// exceeds the boundaries of the input parameters
        /// </summary>
        /// <returns>true - at least one dimension exceeded, false none exceeded</returns>
        /// <history>
        ///    23 Sep 10  Cynic - Started
        /// </history>
        private bool TestChainForMaxAndMin(List<IsoPlotSegment> chainList, ref int minX, ref int minY, ref int maxX, ref int maxY)
        {
            bool coordUpdated = false;
            // sanity check
            if (chainList == null) return false;

            // run through the list, checking to see if any exceed
            foreach (IsoPlotSegment segObj in chainList)
            {
                if ((segObj is IsoPlotSegment_Line) == true)
                {
                    if ((segObj as IsoPlotSegment_Line).X0 < minX)
                    {
                        minX = (segObj as IsoPlotSegment_Line).X0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotSegment_Line).Y0 < minY)
                    {
                        minY = (segObj as IsoPlotSegment_Line).Y0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotSegment_Line).X0 > maxX)
                    {
                        maxX = (segObj as IsoPlotSegment_Line).X0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotSegment_Line).Y0 > maxY)
                    {
                        maxY = (segObj as IsoPlotSegment_Line).Y0;
                        coordUpdated = true;
                    }
                }
                else if ((segObj is IsoPlotSegment_Arc) == true)
                {
                    if ((segObj as IsoPlotSegment_Arc).X0 < minX)
                    {
                        minX = (segObj as IsoPlotSegment_Arc).X0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotSegment_Arc).Y0 < minY)
                    {
                        minY = (segObj as IsoPlotSegment_Arc).Y0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotSegment_Arc).X0 > maxX)
                    {
                        maxX = (segObj as IsoPlotSegment_Arc).X0;
                        coordUpdated = true;
                    }
                    if ((segObj as IsoPlotSegment_Arc).Y0 > maxY)
                    {
                        maxY = (segObj as IsoPlotSegment_Arc).Y0;
                        coordUpdated = true;
                    }
                }
            }
            return coordUpdated;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The isoPlotStep3List contains all of the specific cuts we make. Theoretically
        /// we could just make GCodes out of them and everything would work. However
        /// it is much nicer and more efficient to have the end of one GCode cut lead
        /// right into the next. This eliminates the need to lift the bit off the work
        /// surface and also helps elimiate small little "bridges" between the ends of the 
        /// cuts caused by backlash in the milling machine. 
        /// 
        /// The sequence of consecutive isolation segments are called "IsoChains" here.
        /// 
        /// IsoStep3 must have been performed or nothing is going to work. 
        /// </summary>
        /// <remarks>
        /// Highly RECURSIVE!!!!!
        /// </remarks>
        /// <param name="chainList">a reference to the chainlist we add to</param>
        /// <param name="xIn">xcoord of isoSegPoint</param>
        /// <param name="yIn">ycoord of isoSegPoint</param>
        /// <returns>z success, +ve pt not found, -ve fail</returns>
        /// <history>
        ///    06 August 10  Cynic - Started
        /// </history>
        private int GetIsoChainForPoint(ref List<IsoPlotSegment> chainList, int xIn, int yIn)
        {

            // TODO, every IsoPlotSegment should form a closed loop. If we find a segment
            // which we cannot use to form a complete loop then something is very wrong.

            if (chainList == null) return -100;

            // take the object and look at its X,Y and find the next object which matches
            // for each isoPlotSegment
            foreach (IsoPlotSegment segObj in isoPlotStep3List)
            {
                // is this segment in use? If so we are not interested
                if (segObj.SegmentChained == true) continue;
                // no, it is not in use, does it match x0,y0?
                if ((xIn == segObj.X0) && (yIn == segObj.Y0))
                {
                    // yes it does, add this segment to the chainlist
                    chainList.Add(segObj);
                    // record the fact that this segment is in use in a chain
                    segObj.SegmentChained = true;
                    // since we just matched x0,y0 , follow this chain further down x1,y1
                    GetIsoChainForPoint(ref chainList, segObj.X1, segObj.Y1);
                    // never proceed further after we have found one
                    return 0;
                }
                // does it match x1,y1?
                else if ((xIn == segObj.X1) && (yIn == segObj.Y1))
                {
                    // yes it does, add this segment to the chainlist
                    chainList.Add(segObj);
                    // record the fact that this segment is in use in a chain
                    segObj.SegmentChained = true;
                    // since we just matched x1,y1 , follow this chain further down x0,y0
                    GetIsoChainForPoint(ref chainList, segObj.X0, segObj.Y0);
                    // never proceed further after we have found one
                    return 0;
                }
            }
            // not found 
            return 1;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs processing on the isolation array to figure out the isolation segments
        /// from the builder the gcodeBuilderObjects. We essentially just run over
        /// the object again and pay attention to those segments which have pixels 
        /// not in the DISREGARD state.
        /// </summary>
        /// <remarks>The completion of this step is the end of isoStep3</remarks>
        /// <returns>z succes, nz fail</returns>
        /// <param name="errStr">a helpful error message</param>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        ///    15 Jan 11  Cynic - Added the engraving line code
        /// </history>
        public int PerformIsoPlotSegmentDiscovery(ref string errStr)
        {
            Point ptUL;
            Point ptLL;
            Point ptLR;
            Point ptUR;
            List<IsoPlotSegment> tmpList = null;
            GCodeBuilderObject_Line tmpLine = null;
            GCodeBuilderObject_EngravingLine tmpEngravingLine = null;
            GCodeBuilderObject_Circle tmpCircle = null;
            int retInt;

            LogMessage("PerformIsoPlotSegmentDiscovery called");

            // first we do all the lines.
            foreach (GCodeBuilderObject builderObject in builderObjList)
            {
                if((builderObject is GCodeBuilderObject_Line)==false) continue;
                // perform the cast, this makes it easier to read
                tmpLine = (GCodeBuilderObject_Line)builderObject;

                //DebugMessage("Isolation Discovery on: " + builderObject.BuilderObjectID.ToString());

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
                tmpList = GetIsoPlotSegmentsFromGSLine(builderObject.BuilderObjectID, ptUL.X, ptUL.Y, ptLL.X, ptLL.Y);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                }
                foreach (IsoPlotSegment isoLine in tmpList)
                {
                    if((isoLine is IsoPlotSegment_Line)==false) continue;
                    //DebugMessage("segA="+(isoLine as IsoPlotSegment_Line).ToString());
                    IsoPlotStep3List.Add(isoLine);
                }

                // SegB
                tmpList = GetIsoPlotSegmentsFromGSLine(builderObject.BuilderObjectID, ptLL.X, ptLL.Y, ptLR.X, ptLR.Y);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                }
                foreach (IsoPlotSegment isoLine in tmpList)
                {
                    if ((isoLine is IsoPlotSegment_Line) == false) continue;
                    //DebugMessage("segB=" + (isoLine as IsoPlotSegment_Line).ToString());
                    IsoPlotStep3List.Add(isoLine);
                }

                // SegC
                tmpList = GetIsoPlotSegmentsFromGSLine(builderObject.BuilderObjectID, ptLR.X, ptLR.Y, ptUR.X, ptUR.Y);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                }
                foreach (IsoPlotSegment isoLine in tmpList)
                {
                    if ((isoLine is IsoPlotSegment_Line) == false) continue;
                    //DebugMessage("segC=" + (isoLine as IsoPlotSegment_Line).ToString());
                    IsoPlotStep3List.Add(isoLine);
                }

                // SegD
                tmpList = GetIsoPlotSegmentsFromGSLine(builderObject.BuilderObjectID, ptUR.X, ptUR.Y, ptUL.X, ptUL.Y);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                }
                foreach (IsoPlotSegment isoLine in tmpList)
                {
                    if ((isoLine is IsoPlotSegment_Line) == false) continue;
                    //DebugMessage("segD=" + (isoLine as IsoPlotSegment_Line).ToString());
                    IsoPlotStep3List.Add(isoLine);
                }
            } // bottom of  foreach (GCodeBuilderObject builderObject in builderObjList)

            // now we have all of the line segments we need to get the remaining arcs
            // and circles
            foreach (GCodeBuilderObject builderObject in builderObjList)
            {
                if ((builderObject is GCodeBuilderObject_Circle) == false) continue;
                // perform the cast, this makes it easier to read
                tmpCircle = (GCodeBuilderObject_Circle)builderObject;

                //DebugMessage("Isolation Discovery on: " + builderObject.BuilderObjectID.ToString());

                tmpList = GetIsoPlotSegmentsFromGSCircle(builderObject.BuilderObjectID, tmpCircle.X0, tmpCircle.Y0, tmpCircle.Radius);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception(" null GCode list returned from call to GetIsoPlotSegmentsFromGSCircle");
                }
                foreach (IsoPlotSegment isoLine in tmpList)
                {
                    if ((isoLine is IsoPlotSegment_Arc) == false) continue;
                    //DebugMessage("segE=" + (isoLine as IsoPlotSegment_Arc).ToString());
                    IsoPlotStep3List.Add(isoLine);
                }

            }

            // now we do all the engraving lines.
            foreach (GCodeBuilderObject builderObject in builderObjList)
            {
                if ((builderObject is GCodeBuilderObject_EngravingLine) == false) continue;
                // perform the cast, this makes it easier to read
                tmpEngravingLine = (GCodeBuilderObject_EngravingLine)builderObject;
                //DebugMessage("Isolation Discovery on (EngravingLine): " + builderObject.BuilderObjectID.ToString());

                // the input value is a line, just get the isoplot segments from it
                tmpList = GetIsoPlotSegmentsFromGSLine(builderObject.BuilderObjectID, tmpEngravingLine.X0, tmpEngravingLine.Y0, tmpEngravingLine.X1, tmpEngravingLine.Y1);
                if (tmpList == null)
                {
                    // this should not happen
                    throw new Exception("EngravingLine, null GCode list returned from call to GetIsoPlotSegmentsFromGSLine");
                }
                foreach (IsoPlotSegment isoLine in tmpList)
                {
                    if ((isoLine is IsoPlotSegment_Line) == false) continue;
                    //DebugMessage("EngravingLinesegA="+(isoLine as IsoPlotSegment_Line).ToString());
                    IsoPlotStep3List.Add(isoLine);
                }
            } // bottom of  foreach (GCodeBuilderObject builderObject in builderObjList)

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs processing on the isolation array to flag pixels belonging
        /// to a builderObject which are in the interior of other objects and hence 
        /// removes them from consideration during subsequent processing.
        /// </summary>
        /// <remarks>The completion of this step is the end of isoStep2. The
        /// isoPlot is expected to be in isoStep1 when this is called.</remarks>
        /// <returns>z succes, nz fail</returns>
        /// <param name="errStr">a helpful error message</param>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public int PerformIsolationPlotInteriorPixelRemoval(ref string errStr)
        {
            int isoPlotID = 0;
            errStr = "";

            for (int y = 0; y < isoPlotHeight; y++)
            {
                for (int x = 0; x < isoPlotWidth; x++)
                {
                    // get the id at the isolation plot point, this can be either
                    // a builderObjID or an overlay ID depending on the flags set
                    isoPlotID = isolationPlotArray[x, y];

                    // if the pixel contains an edge over somebody elses background
                    // we set the disregard flag, the OVERLAY flag must necessarily
                    // have been set if this situation is true
                    if ((isoPlotID & (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY) == 0)
                    {
                        // if there is no overlay then there is no need to do anything. Only 
                        // one object is using that pixel
                        continue;
                    }

                    // we do have an overlay, get the overlay object, just use the indexer
                    // to fetche it. This will only use the lower 24 bits
                    // for the search
                    GCodeBuilderObjectOverlay tmpObj = OverlayCollection[isoPlotID];
                    if (tmpObj == null)
                    {
                        // we should always be able to find this
                        LogMessage("PerformSecondaryIsolationArrayProcessing Cannot find overlay object for id=" + (isoPlotID & 0x00ffffff).ToString() + " x=" + x.ToString() + " y=" + y.ToString());
                        throw new Exception("Cannot find overlay object for id");
                    }
                    // if this is true we are just two or more backgrounds layered on one another
                    if (tmpObj.EdgePixelCount == 0)
                    {
                            // mark it as disregard
                            isolationPlotArray[x, y] = isoPlotID | ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_DISREGARD);
                    }

                    // do we have more objects using this as a background than an edge?
                    // if so we know this edge pixel is in the interior of an object
                    // and can be disregarded
                    if (tmpObj.BackgroundPixelCount > tmpObj.EdgePixelCount)
                    {
                        // mark it as disregard
                        isolationPlotArray[x, y] = isoPlotID | ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_DISREGARD);
                        continue;
                    }
                 }
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We test a specified point on the isolation plot to see if it is in use
        /// by the specified builderObjId. Will look into the overlays to see if the 
        /// builderObjID is there - if that is required.
        /// </summary>
        /// <param name="builderObjIDToCheck">object id to check</param>
        /// <param name="x0">x coord</param>
        /// <param name="y0">y coord</param>
        /// <returns> an int consisting of the builderObjIDToCheck and any flags at that point
        /// or zero for not found</returns>
        /// <history>
        ///    28 Jul 10  Cynic - Started
        /// </history>
        private int DoesBuilderObjectUseThisIsoPlotPoint(int builderObjIDToCheck, int x0, int y0)
        {
            if (x0 <= 0) return 0;
            if (y0 <= 0) return 0;
            if (x0 >= isoPlotWidth) return 0;
            if (y0 >= isoPlotHeight) return 0;

            // clean off the incoming id
            int cleanIDIn=builderObjIDToCheck&0x00ffffff;
            // get the id at the isolation plot point
            int isoPlotID = isolationPlotArray[x0, y0];
            if ((isoPlotID & (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY) == 0)
            {
                // we do not have an overlay here, just a normal check will do
                if (cleanIDIn == (isoPlotID & 0x00ffffff)) return isoPlotID;
                else return 0;
            }
            else
            {
                // we do have an overlay, check this now, just use the indexer
                // to see if it is there. This will only use the lower 24 bits
                // for the search
                GCodeBuilderObjectOverlay tmpObj = OverlayCollection[isoPlotID];
                if (tmpObj == null) return 0;
                else
                {
                    // the indexer on the GCodeBuilderObjectOverlay will look up the ID
                    // and return the id with flags or zero if not found
                    return tmpObj[cleanIDIn];
                }
            }
        }

        // ####################################################################
        // ##### GCode Graphical Stigmergy Code
        // ####################################################################
        #region GCode Graphical Stigmergy Code

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets a point in the isolation plot. Observes the graphical stigmergy protocol.
        /// This is _not_ fast - but it is necessary.
        /// </summary>
        /// <remarks>
        /// Credits: this is based the bresnham line drawing algorythm right out of wikipedia
        ///    http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
        ///    Most comments in the code below are from the original source - not me
        /// </remarks>
        /// <param name="isEdge">if this is true we are writing an edge pixel
        /// otherwise it is assumed we are writing an edge pixel</param>
        /// <param name="builderObjectID">the id of the GCodeBuidlerObject that ultimately generated
        /// this call.</param>
        /// <param name="ix">X0 coord</param>
        /// <param name="iy">Y coord</param>
        /// <param name="ignoreOverwrites">if true we ignore it if we are overwriting a pixel which
        /// already belongs to us. This can happen for some composite shapes (rectangle etc)</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public void SetGSPoint(int builderObjectID, bool isEdge, int ix, int iy, bool ignoreOverwrites)
        {
            int existingPixel;
            int newPixel;
            GCodeBuilderObjectOverlay builderOverlayNew = null;
            GCodeBuilderObjectOverlay builderOverlayExisting = null;
            string tmpIdent;

//            DebugMessage("SetGSPoint: ID=" + builderObjectID.ToString() + " (" + ix.ToString() + "," + iy.ToString() + ")");

            // sanity check the coordinates here
            if (ix < 0) return;
            if (iy < 0) return;
            if (ix >= isoPlotWidth) return;
            if (iy >= isoPlotHeight) return;

            // figure this out now, we will use it later
            if (isEdge == false)
            {
                newPixel = builderObjectID | (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_BACKGROUND;
            }
            else
            {
                newPixel = builderObjectID | (int)(GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_EDGE | GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_BACKGROUND);
            }
            
            // now do the read and write, the pixel is always 0 if no object
            // has used it, otherwise it will be nonzero and we have to 
            // take some special steps
            existingPixel = IsolationPlotArray[ix, iy];
            if (existingPixel == PIXEL_NOTUSED)
            {
                // nothing has used this pixel yet. We just write out the ID value to the 
                // lower 24 bits and leave the OVERLAY bit blank
                IsolationPlotArray[ix, iy] = newPixel;
            }
            // if we get here we know someone other object is using the pixel. Is the OVERLAY flag
            // already set? If so this means there is already an overlay object there and this
            // object is number 3 (or more)
            else if ((existingPixel & (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY) != 0)
            {
                // we already have an overlay object there, clearly this pixel is in use by two
                // or more other objects. Get the overlay object 
                builderOverlayExisting = overlayCollection[(existingPixel & 0x00ffffff)];
                if (builderOverlayExisting == null)
                {
                    // what? it has to be there. something very wrong
                    throw new Exception("Overlay object " + (existingPixel & 0x00ffffff).ToString() + " not found in collection");
                }
                // Now a sanity check to see if this ID is already in use within that overlay
                if (builderOverlayExisting[builderObjectID]!=0)
                {
                    if (ignoreOverwrites == false)
                    {
                        // For non composite objects we are not allowed to re-write
                        // over an existing pixel of our own.
                        LogMessage("Error overwriting own isolation pixel in overlay, builderObjID=" + builderObjectID.ToString() + " x=" + ix.ToString() + " y=" + iy.ToString());
                        throw new Exception("Error drawing GCode object, overwriting own isolation pixel in overlay");
                    }
                    else
                    {
                        // just ignore
                        return;
                    }
                }
                // The ID is not currently in the overlay. We now see if we have an existing overlay object which matches
                // the existing overlay + this new pixel. if we have one then use it, otherwise
                // create a new one. We do this to conserve resources
                tmpIdent = builderOverlayExisting.CalcIndentKeyPlusPixel(newPixel);
                builderOverlayNew = OverlayCollection.GetGCodeBuilderObjectOverlayByIdentKey(tmpIdent);
                if (builderOverlayNew == null)
                {
                    // we do not have a match, clone the existing one and then
                    // add the newPixel to make one
                    builderOverlayNew = builderOverlayExisting.DeepClone(OverlayCollection.NextBuilderOverlayID);
                    builderOverlayNew.Add(newPixel);
                    // add it to the collection now
                    OverlayCollection.Add(builderOverlayNew);
                    // set the pixel in the isoplot too
                    IsolationPlotArray[ix, iy] = builderOverlayNew.BuilderOverlayID | (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY;
                }
                else
                {
                    // we found an existing one which will do the job nicely, just set the pixel
                    IsolationPlotArray[ix, iy] = builderOverlayNew.BuilderOverlayID | (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY;
                }
            }
            else
            {
                // The pixel is in use but not by an overlay. We now need one. First a sanity check to see if this ID is already
                // in use on that pixel
                if (builderObjectID == GCodeBuilderObject.ReturnIDFromPixel(existingPixel))
                {
                    if (ignoreOverwrites == false)
                    {
                        // For non composite objects we are not allowed to re-write
                        // over an existing pixel of our own.
                        LogMessage("Error overwriting own isolation pixel, builderObjID=" + builderObjectID.ToString() + " x=" + ix.ToString() + " y=" + iy.ToString());
                        throw new Exception("Error drawing GCode object, overwriting own isolation pixel");
                    }
                    else
                    {
                        // just ignore
                        return;
                    }
                }
                // builderObjectID is not in use, we make a new overlay. In order to conserve resources we will
                // see if we can find an existing overlay we can use which represents the usage at this pixel. We 
                // do this by creating the identKey of the hypothetical GCodeBuilderObjectOverlay we wish to create and
                // see if it already exists. if it does we use that. if it doesn't we install it
                tmpIdent = GCodeBuilderObjectOverlay.CalcIndentKeyFromTwoPixels(existingPixel, newPixel);
                builderOverlayNew = OverlayCollection.GetGCodeBuilderObjectOverlayByIdentKey(tmpIdent);
                if (builderOverlayNew == null)
                {
                    // create one and replace the 
                    // existing pixel with its ID and the overlay flag
                    builderOverlayNew = new GCodeBuilderObjectOverlay(OverlayCollection.NextBuilderOverlayID, existingPixel, newPixel);
                    // add it now
                    OverlayCollection.Add(builderOverlayNew);
                    // set the pixel too
                    IsolationPlotArray[ix, iy] = builderOverlayNew.BuilderOverlayID | (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY;
                    //DebugMessage("intersection pixel created");
                }
                else
                {
                    // we found an existing one which will do the job nicely, just set the pixel
                    IsolationPlotArray[ix, iy] = builderOverlayNew.BuilderOverlayID | (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY;
                    //DebugMessage("existing intersection pixel used");
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a line from two points. Observes the graphical stigmergy protocol. Always
        /// assumes it is drawing an edge pixel
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
        /// <param name="ignoreOverwrites">if true we ignore it if we are overwriting a pixel which
        /// already belongs to us. This can happen for some composite shapes (rectangle etc)</param>
        /// <returns>the builderObjID of the line we created or 0 for fail</returns>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        private int DrawGSLine(int builderObjID, int x0, int y0, int x1, int y1, bool ignoreOverwrites)
        {
            int ix;
            int iy;

            // sanity check the coordinates here
            if (x0 < 0) return 0;
            if (y0 < 0) return 0;
            if (x1 >= isoPlotWidth) return 0;
            if (y1 >= isoPlotHeight) return 0;

           // DebugMessage("DrawGSLine: ID=" + builderObjID.ToString() + " (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

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
                SetGSPoint(builderObjID, true, ix, iy, ignoreOverwrites);

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }

            // return the builderObject ID cleaned of any flags
            return builderObjID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a wide line between two points. Observes the graphical stigmergy protocol.
        /// </summary>
        /// <remarks>
        /// This just takes the line angle, figures out the angle of the line perpendicular and
        /// then figures out the XY coordinates of the perpendicular lines at the endpoints of 
        /// the specified line such that the sum of the length of two perpendiculars starting at 
        /// the end points is the frikken width. This gave me a lot more trouble than you'd think
        /// it should for such a simple problem. sigh.
        /// </remarks>
        /// <param name="isEdge">if this is true we are writing an edge pixel
        /// otherwise it is assumed we are writing an background pixel</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <returns>a list of the builderObjID's of the lines we created or null for fail</returns>
        /// <history>
        ///    28 Jul 10  Cynic - Started
        /// </history>
        public int DrawGSLineOutLine(int x0, int y0, int x1, int y1, int width, bool wantFilled)
        {
            // these are the endpoints of the rectangle
            Point ptUL;
            Point ptLL;
            Point ptLR;
            Point ptUR;
            Point ptRectLL;
            Point ptRectUR;
            int retInt;

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
            GCodeBuilderObject_Line builderObj = new GCodeBuilderObject_Line(this.NextGerberObjectID, x0, y0, x1, y1, width);
            builderObjList.Add(builderObj);

            //DebugMessage("DrawGSLineOutline:ID=" + builderObj.BuilderObjectID.ToString() + "(" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // draw the lines
            this.DrawGSLine(builderObj.BuilderObjectID, ptUL.X, ptUL.Y, ptLL.X, ptLL.Y, true);
            this.DrawGSLine(builderObj.BuilderObjectID, ptLL.X, ptLL.Y, ptLR.X, ptLR.Y, true);
            this.DrawGSLine(builderObj.BuilderObjectID, ptLR.X, ptLR.Y, ptUR.X, ptUR.Y, true);
            this.DrawGSLine(builderObj.BuilderObjectID, ptUR.X, ptUR.Y, ptUL.X, ptUL.Y, true);

            // do we want the box filled?
            if (wantFilled == true)
            {
                // yes, we do, figure out the smallest and largest XY for our bounding box
                retInt = MiscGraphicsUtils.GetBoundingRectangleEndPointsFrom4Points(ptLL, ptUL, ptUR, ptLR, out ptRectLL, out ptRectUR);
                if (retInt != 0)
                {
                    LogMessage("DrawGSLineOutLine call to GetBoundingRectangleEndPointsFrom4Points returned " + retInt.ToString());
                    return 0;
                }
                // now fill the bounding box
                BackgroundFillGSRegionSimple(builderObj.BuilderObjectID, ptRectLL.X, ptRectLL.Y, ptRectUR.X, ptRectUR.Y);
            }

            return builderObj.BuilderObjectID&0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a line between two points. Observes the graphical stigmergy protocol.
        /// </summary>
        /// <remarks>
        /// This just takes the two end points and draws a line between them. This line is
        /// assumed to be an edge line. This will run the tool over that line when the PCB
        /// is routed. This gives the effect of engraving a single track.
        /// </remarks>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <returns>a list of the builderObjID's of the lines we created or null for fail</returns>
        /// <history>
        ///    15 Jan 11  Cynic - Started
        /// </history>
        public int DrawGSLineEngravingLine(int x0, int y0, int x1, int y1)
        {
            // the inputs are the endpoints of the line

            // record this object, with it max and min points 
            GCodeBuilderObject_EngravingLine builderObj = new GCodeBuilderObject_EngravingLine(this.NextGerberObjectID, x0, y0, x1, y1);
            builderObjList.Add(builderObj);

            //DebugMessage("DrawGSLineEngravingLine:ID=" + builderObj.BuilderObjectID.ToString() + "(" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // draw the lines
            this.DrawGSLine(builderObj.BuilderObjectID, x0, y0, x1, y1, true);

            return builderObj.BuilderObjectID & 0x00ffffff;
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
        ///    one we convert to GCodes) we must follow the circle linearly pixel-by-pixel. The
        ///    usual code for this is optimized to place pixels in all eight quarants in one pass.
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
        /// <param name="wantFilled">if true we fill it with GS background pixels</param>
        /// <returns>the builderObjID of the circle we created or 0 for fail</returns>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        ///    04 Aug 10  Cynic - rewired and unrolled as in comments above
        /// </history>
        public int DrawGSCircle(int cx, int cy, int radius, bool wantFilled)
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

            // record this object 
            GCodeBuilderObject_Circle builderObj = new GCodeBuilderObject_Circle(this.NextGerberObjectID, cx, cy, radius);
            builderObjList.Add(builderObj);
            //DebugMessage("DrawGSCircleFilled: ID=" + builderObj.BuilderObjectID.ToString() + " (" + cx.ToString() + "," + cy.ToString() + ") radius=" + radius.ToString());

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
                LogMessage("DrawGSCircleFilled: ID=" + builderObj.BuilderObjectID.ToString() + " ptCount==0");
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
                this.SetGSPoint(builderObj.BuilderObjectID, true, workingX, workingY, false);
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
                this.SetGSPoint(builderObj.BuilderObjectID, true, workingX, workingY, false);
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
                this.SetGSPoint(builderObj.BuilderObjectID, true, workingX, workingY, false);
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
                this.SetGSPoint(builderObj.BuilderObjectID, true, workingX, workingY, false);
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
                this.SetGSPoint(builderObj.BuilderObjectID, true, workingX, workingY, false);
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
                this.SetGSPoint(builderObj.BuilderObjectID, true, workingX, workingY, false);
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
                this.SetGSPoint(builderObj.BuilderObjectID, true, workingX, workingY, false);
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
                this.SetGSPoint(builderObj.BuilderObjectID, true, workingX, workingY, false);
                lastX = workingX;
                lastY = workingY;
            }

            // do we want to fill it?
            if (wantFilled == true)
            {
                // yes, we do
                BackgroundFillGSRegionSimple(builderObj.BuilderObjectID, cx - radius - 1, cy - radius - 1, cx + radius + 1, cy + radius + 1);
            }

            // return the object id with no flags
            return builderObj.BuilderObjectID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to fill in the background of the interior of a region bounded
        /// by the specified builderObjID. This function observes the GS protocol and 
        /// will dig around in the overlay arrays to check for edge boundarys.
        /// </summary>
        /// <remarks>
        ///     The way this works is we treat the incoming coords as a rectangle
        ///     then we raster for each y0->y1 we draw a line of background pixels
        ///     on the isoPlot from x0 to x1. However we do not set a pixel unless we
        ///     have are on or already seen a edge pixel. If we see another edge pixel
        ///     we consider ourselves to be out of the object and stop writing. This
        ///     permits this function to work for any arbitrarily drawn object as long
        ///     as it has encoded its builderObjID either directly on the isolation plot or
        ///     in the overlays.
        ///     
        ///     We assume the line is simple. This means as soon as we run across the 
        ///     terminating edge we can stop drawing a line. This makes it more efficient 
        ///     for things like rectangles or circles (when past the far side of the object 
        ///     there is no more to do).  This code will not work if something like a W shape 
        ///     was being filled in as it would stop after the first V in the VV shape
        /// </remarks>
        /// <param name="builderObjID">the object id of the pixels which form the object boundary</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <history>
        ///    28 Jul 10  Cynic - Started
        /// </history>
        private void BackgroundFillGSRegionSimple(int builderObjID, int x0, int y0, int x1, int y1)
        {
            // we accept either opposite corners of the bounding rectangle 
            // and sort out the (l) Low and the (h) high values of each;
            int lx;
            int ly;
            int hx;
            int hy;
            bool startEdgeFound = false;
            int lastStartEdgeX = int.MinValue;
            int firstXOfLine = int.MinValue;
            int lastXOfLine = int.MinValue;
            int ptFlags;

            //DebugMessage("BackgroundFillGSRegion: ID=" + builderObjID.ToString() + " (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

            // some sanity checks
            if (builderObjID <= 0) return;
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

            // now go and fill in, we run vertically from ly to hy
            for (int yVal = ly; yVal <= hy; yVal++)
            {
                //DebugMessage("");
                //DebugMessage("Y set to " + yVal.ToString());
                //DebugMessage("");

                // reset this
                startEdgeFound = false;
                lastStartEdgeX = int.MinValue;
                firstXOfLine = int.MinValue;
                lastXOfLine = int.MinValue;
                // and now run horzontally from lx to hx
                for (int xVal = lx; xVal <= hx; xVal++)
                {
                    ptFlags = DoesBuilderObjectUseThisIsoPlotPoint(builderObjID, xVal, yVal);
                    //DebugMessage("ptFlags=" + ptFlags.ToString()+ " (" + xVal.ToString() + "," + yVal.ToString() +")");
                    // are we on an edge pixel?
                    if ((ptFlags & ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_EDGE)) != 0)
                    {
                        //DebugMessage("OnEdge");
                        // yes we are on an edge, have we already seen a start edge pixel
                        if (startEdgeFound == true)
                        {
                            //DebugMessage("  startEdgeFound == true");
                            // yes we have, but is it adjacent to the last one?
                            if ((lastStartEdgeX + 1) == xVal)
                            {
                                //DebugMessage("   adjacent:(lastStartEdgeX + 1) == xVal, xVal="+xVal.ToString());
                                // yes it is, this is just a consecutive start pixel 
                                // it is not really a edge stop, just record and continue
                                lastStartEdgeX = xVal;
                                //DebugMessage("   lastStartEdgeX now " + lastStartEdgeX.ToString());
                                continue;
                            }
                            else
                            {
                                // no it is not, this must be a true end edge pixel
                                //DebugMessage("   not adjacent:(lastStartEdgeX + 1) != xVal, xVal=" + xVal.ToString());
                                startEdgeFound = false;
                                lastStartEdgeX = int.MinValue;
                                //DebugMessage("    startEdgeFound==true, setting to false");
                                //DebugMessage("    lastStartEdgeX reset to -1");
                                // we will never see another start pixel in a simple
                                // object - just go the the next line now
                                break;
                            }
                        }
                        else
                        {
                            //DebugMessage("  startEdgeFound==false, setting to true");
                            // we have not, record this
                            startEdgeFound = true;
                            // and record the current X position so we can
                            // track consecutives, we consider these all to be
                            // the same start edge flag
                            lastStartEdgeX = xVal;
                            //DebugMessage("   lastStartEdgeX now " + lastStartEdgeX.ToString());

                        }
                    }
                    else
                    {
                        //DebugMessage("OnFill");
                        // no we are not on an edge pixel, have we seen an edge?
                        if (startEdgeFound == true)
                        {
                            //DebugMessage("startEdgeFound == true, firstXOfLine=" + firstXOfLine.ToString() + " lastXOfLine=" + lastXOfLine.ToString());
                            if (firstXOfLine < 0)
                            {
                                firstXOfLine = xVal;
                               // DebugMessage("    firstXOfLine now " + firstXOfLine.ToString());
                            }
                            else
                            {
                                lastXOfLine = xVal;
                               // DebugMessage("    lastXOfLine now " + lastXOfLine.ToString());
                            }
                        }
                        else
                        {
                            //DebugMessage("startEdgeFound == false, ignoring pixel");
                        }
                    }
                } // bottom of for (int xVal = lx; xVal < hx; xVal++)

                // at this point we should have found the firstXOfLine and lastXOfLine
                // we draw the line if we found them, both have to be set! 
                if ((firstXOfLine >= 0) && (lastXOfLine >= 0))
                {
                    // at this point we should have found the firstXOfLine and lastXOfLine
                    // we draw the line if we found them, both have to be set! 
                    if (firstXOfLine >= 0)
                    {
                        // did we find and run through endpoint? 
                        if ((lastXOfLine >= 0) && (startEdgeFound==false))
                        {
                            // yes we did there are more than one point involved here
                            for (int xVal = firstXOfLine; xVal <= lastXOfLine; xVal++)
                            {
                                // set all of the points on the line
                                SetGSPoint(builderObjID, false, xVal, yVal, false);
                            }
                        }
                    }
                }
            } // bottom of for (int xVal = lx; xVal < hx; xVal++)
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GCode Line segments from a GS line drawn on the iso plot. What we
        /// do is essentially redraw the line but instead of writing out values
        /// we look for sequences of pixels which are not in the DISREGARD
        /// state. If we find any pixels which are used by more than one edge
        /// we check the EDGEINUSE flag. This means some other edge is currently
        /// routing it and we do not need to do so. We find the start and end point
        /// of each line segment and add it to the list. Every pixel we build a line
        /// for we mark as EDGEINUSE.
        /// </summary>
        /// <remarks>
        /// Credits: this is based the bresnham line drawing algorythm right out of wikipedia
        ///    http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
        ///    Most comments in the code below are from the original source - not me
        /// </remarks>
        /// <param name="builderObjID">the id of the builder object we are testing</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <returns>A List of IsoPlotSegment_Line objects representing the sequences of 
        /// non DISREGARD pixels on this line or null for fail. Can return an empty list</returns>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public List<IsoPlotSegment> GetIsoPlotSegmentsFromGSLine(int builderObjID, int x0, int y0, int x1, int y1)
        {
            int ix = int.MinValue;
            int iy = int.MinValue;

            List<IsoPlotSegment> retList = new List<IsoPlotSegment>();
            //DebugMessage("GetIsoPlotSegmentsFromGSLine: ID=" + builderObjID.ToString() + " (" + x0.ToString() + "," + y0.ToString() + ") (" + x1.ToString() + "," + y1.ToString() + ")");

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

            int lineSegX0 = int.MinValue;
            int lineSegY0 = int.MinValue;
            int lineSegX1 = int.MinValue;
            int lineSegY1 = int.MinValue;
            bool unusedPixelFound = false;

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

                // the code which checks each pixel and builds the segments has been factored
                // out here. This is because we also use it for the arcs and want to maintain
                // one common code base
                IsoPlotSegmentCalculator(IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Line, ix, iy, ref retList, ref lineSegX0, ref lineSegY0, ref lineSegX1, ref lineSegY1, ref unusedPixelFound);

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }

            // if we get here we may still have an activeLineSeg. The thing we look for 
            // is wether we have an unused pixel.
            if ((unusedPixelFound == true) && (((lineSegX0 == lineSegX1) && (lineSegY0 == lineSegY1)) == false))
            {
                // we have an unused pixel, we record the line
                IsoPlotSegment_Line tmpGCLine = new IsoPlotSegment_Line(lineSegX0, lineSegY0, lineSegX1, lineSegY1);
                retList.Add(tmpGCLine);
                // reset everything
                lineSegX0 = int.MinValue;
                lineSegY0 = int.MinValue;
                lineSegX1 = int.MinValue;
                lineSegY1 = int.MinValue;
                unusedPixelFound = false;
            }

            // return the builderObject ID cleaned of any flags
            return retList;
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
        ///    one we convert to GCodes) we must follow the circle linearly pixel-by-pixel. The
        ///    usual code for this is optimized to place pixels in all eight quarants in one pass.
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
        /// <param name="wantFilled">if true we fill it with GS background pixels</param>
        /// <returns>the builderObjID of the circle we created or 0 for fail</returns>
        /// <history>
        ///    04 Aug 10  Cynic - Started
        /// </history>
        public List<IsoPlotSegment> GetIsoPlotSegmentsFromGSCircle(int builderObjID, int cx, int cy, int radius)
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

            // used by the IsoPlotSegmentCalculator
            int lineSegX0 = int.MinValue;
            int lineSegY0 = int.MinValue;
            int lineSegX1 = int.MinValue;
            int lineSegY1 = int.MinValue;
            bool unusedPixelFound = false;

            List<IsoPlotSegment> retList = new List<IsoPlotSegment>();

            // DebugMessage("GetIsoPlotSegmentsFromGSCircle: ID=" + builderObjID.ToString() + " (" + cx.ToString() + "," + cy.ToString() + ") radius=" + radius.ToString());

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
                LogMessage("GetIsoPlotSegmentsFromGSCircle: ID=" + builderObjID.ToString() + " ptCount==0");
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
              //  DebugMessage("**1 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                IsoPlotSegmentCalculator(IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Arc, workingX, workingY, ref retList, ref lineSegX0, ref lineSegY0, ref lineSegX1, ref lineSegY1, ref unusedPixelFound);
                lastX = workingX;
                lastY = workingY;
            }
            // UR quadrant CCW from 45 degree line to +Y axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx + ptArray[i].Y;
                workingY = cy + ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
               // DebugMessage("**2 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                IsoPlotSegmentCalculator(IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Arc, workingX, workingY, ref retList, ref lineSegX0, ref lineSegY0, ref lineSegX1, ref lineSegY1, ref unusedPixelFound);
                lastX = workingX;
                lastY = workingY;
            }
            // UL quadrant CCW from +Y axis to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx - ptArray[i].Y;
                workingY = cy + ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
               // DebugMessage("**3 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                IsoPlotSegmentCalculator(IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Arc, workingX, workingY, ref retList, ref lineSegX0, ref lineSegY0, ref lineSegX1, ref lineSegY1, ref unusedPixelFound);
                lastX = workingX;
                lastY = workingY;
            }
            // UL quadrant CCW from 45 degree line to -X axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx - ptArray[i].X;
                workingY = cy + ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                //DebugMessage("**4 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                IsoPlotSegmentCalculator(IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Arc, workingX, workingY, ref retList, ref lineSegX0, ref lineSegY0, ref lineSegX1, ref lineSegY1, ref unusedPixelFound);
                lastX = workingX;
                lastY = workingY;
            }
            // LL quadrant CCW from -X axis to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx - ptArray[i].X;
                workingY = cy - ptArray[i].Y;
                if ((workingX == lastX) && (workingY == lastY)) continue;
               // DebugMessage("**5 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                IsoPlotSegmentCalculator(IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Arc, workingX, workingY, ref retList, ref lineSegX0, ref lineSegY0, ref lineSegX1, ref lineSegY1, ref unusedPixelFound);
                lastX = workingX;
                lastY = workingY;
            }
            // LL quadrant CCW from 45 degree line to -Y axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx - ptArray[i].Y;
                workingY = cy - ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                //DebugMessage("**6 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                IsoPlotSegmentCalculator(IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Arc, workingX, workingY, ref retList, ref lineSegX0, ref lineSegY0, ref lineSegX1, ref lineSegY1, ref unusedPixelFound);
                lastX = workingX;
                lastY = workingY;
            }
            // LR quadrant CCW from -Y axis to 45 degree line
            for (int i = 0; i < ptCount; i++)
            {
                workingX = cx + ptArray[i].Y;
                workingY = cy - ptArray[i].X;
                if ((workingX == lastX) && (workingY == lastY)) continue;
                //DebugMessage("**7 x="+ workingX.ToString() + " y=" + workingY.ToString());
                // call the segment calculator with the point, it will add to the retList as required
                IsoPlotSegmentCalculator(IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Arc, workingX, workingY, ref retList, ref lineSegX0, ref lineSegY0, ref lineSegX1, ref lineSegY1, ref unusedPixelFound);
                lastX = workingX;
                lastY = workingY;
            }
            // LR quadrant CCW from 45 degree line to +X axis
            for (int i = ptCount - 1; i >= 0; i--)
            {
                workingX = cx + ptArray[i].X;
                workingY = cy - ptArray[i].Y;
                //DebugMessage("**8 x=" + workingX.ToString() + " y=" + workingY.ToString());
                if ((workingX == lastX) && (workingY == lastY)) continue;
                // NOTE: we do NOT do this check here. We need to have a look at
                //       the first pixel we did so we can join up circles etc
                // *** if ((workingX == firstX) && (workingY == firstY)) continue;
                // call the segment calculator with the point, it will add to the retList as required
                IsoPlotSegmentCalculator(IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Arc, workingX, workingY, ref retList, ref lineSegX0, ref lineSegY0, ref lineSegX1, ref lineSegY1, ref unusedPixelFound);
                lastX = workingX;
                lastY = workingY;
            }

            // if we get here we may still have an activeArcSeg. The thing we look for 
            // is whether we have an unused pixel.
            if ((unusedPixelFound == true) && (((lineSegX0 == lineSegX1) && (lineSegY0 == lineSegY1)) == false))
            {
                // we have an unused pixel, we record the arc
                IsoPlotSegment_Arc tmpGCArc = new IsoPlotSegment_Arc(lineSegX0, lineSegY0, lineSegX1, lineSegY1);
                retList.Add(tmpGCArc);
                // reset everything
                lineSegX0 = int.MinValue;
                lineSegY0 = int.MinValue;
                lineSegX1 = int.MinValue;
                lineSegY1 = int.MinValue;
                unusedPixelFound = false;
            }

            // we have to populate the center and radius values in our segments
            // the IsoPlotSegmentCalculator does not know how to do this
            foreach (IsoPlotSegment_Arc arcSeg in retList)
            {
                arcSeg.XCenter = cx;
                arcSeg.YCenter = cy;
                arcSeg.Radius = radius;
            }

            return retList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts a pixel coord in the isoplot and keeps track of which pixels form
        /// contiguous isoPlotSegments. After repeated calls it populates
        /// the retList with the valid IsoPlotSegments it finds. This code is 
        /// common to the GetIsoPlotSegmentsFromGSLine and GetIsoPlotSegmentsFromGSCircle
        /// </summary>
        /// <remarks>Call this for every pixel in a line or circle on the isoPlot and
        /// the retList will have the valid IsoPlotSegments in that entity. Note
        /// you MUST proceed sequentially along the entity.
        /// </remarks>
        /// <param name="ix">x pixel cooord</param>
        /// <param name="iy">y pixel cooord</param>
        /// <param name="lineSegX0">used internally, intialise to int.MinValue</param>
        /// <param name="lineSegY0">used internally, intialise to int.MinValue</param>
        /// <param name="lineSegX1">used internally, intialise to int.MinValue</param>
        /// <param name="lineSegY1">used internally, intialise to int.MinValue</param>
        /// <param name="unusedPixelFound">used internally, intialize to false</param>
        /// <param name="retList">a list of the IsoPlotSegments found</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        ///    04 Aug 10  Cynic - Abstracted out to here
        /// </history>
        private void IsoPlotSegmentCalculator(IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum segType, int ix, int iy, ref List<IsoPlotSegment> retList, ref int lineSegX0, ref int lineSegY0, ref int lineSegX1, ref int lineSegY1, ref bool unusedPixelFound)
        {
            // get the point
            int plotPixelVal = 0;

            try
            {
                plotPixelVal = IsolationPlotArray[ix, iy];
            }
            catch (Exception ex)
            {
                // Exception happened Record it
                LogMessage("IsoPlotSegmentCalculator Exception at [ix, iy] =(" + ix.ToString() + "," + iy.ToString() + ") Ex=" + ex.Message);
                // send it on
                throw ex;
            }
            if (IsoPlotPointIsDisregard(ix, iy) == true)
            {
                // this point is a disregard. If we have a start and stop
                // and a valid set of internal points we record the line otherwise 
                // we just disregard
                if ((unusedPixelFound == true) && (((lineSegX0 == lineSegX1) && (lineSegY0 == lineSegY1)) == false))
                {
                    // the seg is good and worth recording 
                    if (segType == IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Line)
                    {
                        // we want a line here
                        IsoPlotSegment_Line tmpGCLine = new IsoPlotSegment_Line(lineSegX0, lineSegY0, lineSegX1, lineSegY1);
                        retList.Add(tmpGCLine);
                    }
                    else
                    {
                        // we want an arc here
                        IsoPlotSegment_Arc tmpGCArc = new IsoPlotSegment_Arc(lineSegX0, lineSegY0, lineSegX1, lineSegY1);
                        retList.Add(tmpGCArc);
                    }
                    // reset everything
                    lineSegX0 = int.MinValue;
                    lineSegY0 = int.MinValue;
                    lineSegX1 = int.MinValue;
                    lineSegY1 = int.MinValue;
                    unusedPixelFound = false;
                }
                else
                {
                    // line is not good and not worth recording
                    // reset everything
                    lineSegX0 = int.MinValue;
                    lineSegY0 = int.MinValue;
                    lineSegX1 = int.MinValue;
                    lineSegY1 = int.MinValue;
                    unusedPixelFound = false;
                }
            }
            else
            {
                // ok it is not a disregard, but is it in use?
                if ((plotPixelVal & ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_EDGEINUSE)) == 0)
                {
                    // this edge pixel is not in use, we can collect it, have
                    // we got a start point already?
                    if (lineSegX0 < 0)
                    {
                        // no we have not - this pixel represents a start of a segment
                        lineSegX0 = ix;
                        lineSegY0 = iy;
                        // we also record this so that we have an endpoint
                        lineSegX1 = ix;
                        lineSegY1 = iy;
                        // we need to know this
                        unusedPixelFound = true;
                    }
                    else
                    {
                        // we do have a start point, just adjust the end points
                        lineSegX1 = ix;
                        lineSegY1 = iy;
                        // we need to know this
                        unusedPixelFound = true;
                    }
                    // mark it as in use now
                    IsolationPlotArray[ix, iy] = plotPixelVal | ((int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_EDGEINUSE);
                }
                else
                {
                    // the point is in use, there are several scenarios which can 
                    // play out here, first - do we have a start point?
                    if (lineSegX0 < 0)
                    {
                        // no we do not, this means that this could be an intersection
                        // already used by somebody else but we have to use it 
                        // as a start point as well for continuity
                        lineSegX0 = ix;
                        lineSegY0 = iy;
                        // we also record this so that we have an endpoint
                        lineSegX1 = ix;
                        lineSegY1 = iy;
                        // we set this, we just set the start point so we know this is false
                        unusedPixelFound = false;
                    }
                    // ok, we have a start point, have we seen a not in use pixel
                    else if (unusedPixelFound == false)
                    {
                        // we have not. this is just the next pixel in the 
                        // sequence - ignore the previous one and treat
                        // this one as the new start pixel
                        lineSegX0 = ix;
                        lineSegY0 = iy;
                        // we also record this so that we have an endpoint
                        lineSegX1 = ix;
                        lineSegY1 = iy;
                        // we set this, we just set the start point so we know this is false
                        unusedPixelFound = false;
                    }
                    else
                    {
                        // ok we have a start point and have see at least one unused pixel
                        // we consider this new EDGEINUSE pixel to be the terminator of our
                        // segment. We make the segment into a line and add it to the array
                        lineSegX1 = ix;
                        lineSegY1 = iy;
                        // the seg is good and worth recording 
                        if (segType == IsoPlotSegment.IsoPlotSegmentCalculatorTypeEnum.IsoPlotSegmentCalculator_Line)
                        {
                            // we want a line here
                            IsoPlotSegment_Line tmpGCLine = new IsoPlotSegment_Line(lineSegX0, lineSegY0, lineSegX1, lineSegY1);
                            retList.Add(tmpGCLine);
                        }
                        else
                        {
                            // we want an arc here
                            IsoPlotSegment_Arc tmpGCArc = new IsoPlotSegment_Arc(lineSegX0, lineSegY0, lineSegX1, lineSegY1);
                            retList.Add(tmpGCArc);
                        }
                        // reset everything
                        lineSegX0 = int.MinValue;
                        lineSegY0 = int.MinValue;
                        lineSegX1 = int.MinValue;
                        lineSegY1 = int.MinValue;
                        unusedPixelFound = false;
                    }
                }
            } // bottom of if (IsoPlotPointIsDisregard ... else {
        }

        #endregion

        // ####################################################################
        // ##### Old code which might come in useful again one day
        // ####################################################################
        #region Old code which might come in useful again one day

        /*
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a filled circle of a defined radius at a point. Observes the 
        /// graphical stigmergy protocol.
        /// </summary>
        /// <remarks>
        /// Credits: much of algorythm from 
        ///    http://stackoverflow.com/questions/1201200/fast-algorithm-for-drawing-filled-circles
        ///    Most comments in the code below are from the original source - not me
        /// </remarks>
        /// <param name="cx">X center</param>
        /// <param name="cy">Y center</param>
        /// <param name="radius">the radius</param>
        /// <param name="wantFilled">if true we fill it with GS background pixels</param>
        /// <returns>the builderObjID of the circle we created or 0 for fail</returns>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public int DrawGSCircleX(int cx, int cy, int radius, bool wantFilled)
        {
            int error = -radius;
            int x = radius;
            int y = 0;


            // record this object 
            GCodeBuilderObject_Circle builderObj = new GCodeBuilderObject_Circle(this.NextGerberObjectID, cx, cy, radius);
            builderObjList.Add(builderObj);
            DebugMessage("DrawGSCircleFilled: ID="+builderObj.BuilderObjectID.ToString()+" (" + cx.ToString() + "," + cy.ToString() + ") radius="+ radius.ToString());

            // The following while loop may altered to 'while (x > y)' for a
            // performance benefit, as long as a call to 'Plot4GSCirclePoints' follows
            // the body of the loop. This allows for the elimination of the
            // '(x != y') test in 'Plot8GSCirclePoints', providing a further benefit.
            while (x >= y)
            {
                Plot8GSCirclePoints(builderObj.BuilderObjectID, cx, cy, x, y, false);

                error += y;
                ++y;
                error += y;

                // The following test may be implemented in assembly language in
                // most machines by testing the carry flag after adding 'y' to
                // the value of 'error' in the previous step, since 'error'
                // nominally has a negative value.
                if (error >= 0)
                {
                    --x;
                    error -= x;
                    error -= x;
                }
            }
            // do we want to fill it?
            if (wantFilled == true)
            {
                // yes, we do
                BackgroundFillGSRegionSimple(builderObj.BuilderObjectID, cx - radius - 1, cy - radius - 1, cx + radius + 1, cy + radius + 1);
            }
            // return the object id with no flags
            return builderObj.BuilderObjectID & 0x00ffffff;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Utility proc used by DrawGSCircleFilled
        /// </summary>
        /// <remarks>
        /// Credits: much of algorythm from 
        ///    http://stackoverflow.com/questions/1201200/fast-algorithm-for-drawing-filled-circles
        ///    Most comments in the code below are from the original source - not me
        /// </remarks>
        /// <param name="cx">X center</param>
        /// <param name="cy">Y center</param>
        /// <param name="radius">the radius</param>
        /// <param name="ignoreOverwrites">if true we ignore it if we are overwriting a pixel which
        /// already belongs to us. This can happen for some composite shapes (rectangle etc)</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        private void Plot8GSCirclePoints(int builderObjectID, int cx, int cy, int x, int y, bool ignoreOverwrites)
        {
            Plot4GSCirclePoints(builderObjectID, cx, cy, x, y, ignoreOverwrites);
            if (x != y) Plot4GSCirclePoints(builderObjectID, cx, cy, y, x, ignoreOverwrites);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Utility proc used by DrawFilledCircle
        /// </summary>
        /// <remarks>
        /// Credits: much of algorythm from 
        ///    http://stackoverflow.com/questions/1201200/fast-algorithm-for-drawing-filled-circles
        ///    Most comments in the code below are from the original source - not me
        /// </remarks>
        /// <param name="cx">X center</param>
        /// <param name="cy">Y center</param>
        /// <param name="radius">the radius</param>
        /// <param name="ignoreOverwrites">if true we ignore it if we are overwriting a pixel which
        /// already belongs to us. This can happen for some composite shapes (rectangle etc)</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        private void Plot4GSCirclePoints(int builderObjectID, int cx, int cy, int x, int y, bool ignoreOverwrites)
        {
            // The '(x != 0 && y != 0)' test in the last line of this function
            // may be omitted for a performance benefit if the radius of the
            // circle is known to be non-zero.
            this.SetGSPoint(builderObjectID, true, cx + x, cy + y, ignoreOverwrites);
            if (x != 0) this.SetGSPoint(builderObjectID, true, cx - x, cy + y, ignoreOverwrites);
            if (y != 0) this.SetGSPoint(builderObjectID, true, cx + x, cy - y, ignoreOverwrites);
            if (x != 0 && y != 0) this.SetGSPoint(builderObjectID, true, cx - x, cy - y, ignoreOverwrites);
        }
*/
        #endregion

    }
}
