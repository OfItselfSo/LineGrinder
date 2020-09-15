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
    /// A class to contain the pixel overlays for the isolation plot. Essentially
    /// this class keeps track of which GCodeBuilderObject are using a particular
    /// pixel. 
    /// </summary>
    /// <history>
    ///    26 Jul 10  Cynic - Started
    /// </history>
    public class GCodeBuilderObjectOverlay : OISObjBase
    {
        public const int DEFAULT_BUILDEROVERLAY_ID = 0;
        protected int builderOverlayID = DEFAULT_BUILDEROVERLAY_ID;

        // this is a list of all builderObjects which use this pixel
        private Dictionary<int,int> builderObjsOverlaidOnPixel = new Dictionary<int,int>();

        // this is a de-normalized count of the background pixels in our overlay list
        private int backgroundPixelCount=0;

        // this is a de-normalized count of the edge pixels in our overlay list
        private int edgePixelCount = 0;

        public const string DEFAULT_IDENT_KEY = "";
        private string identKey = DEFAULT_IDENT_KEY;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="builderOverlayIDIn">the builderOverlayID for this object</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public GCodeBuilderObjectOverlay(int builderOverlayIDIn)
        {
            builderOverlayID = builderOverlayIDIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="builderOverlayIDIn">the builderOverlayID for this object</param>
        /// <param name="builderObject1Pixel">the isoPlotPixel value for object 1</param>
        /// <param name="builderObject1Pixel">the isoPlotPixel value for object 2</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public GCodeBuilderObjectOverlay(int builderOverlayIDIn, int builderObject1Pixel, int builderObject2Pixel)
        {
            builderOverlayID = builderOverlayIDIn;
            // be sure to turn off the OVERLAY bit if set
            int pixel1 = builderObject1Pixel & (~(int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY);
            int pixel2 = builderObject2Pixel & (~(int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY);

            //DebugMessage("Create Overlay ID=" + this.BuilderOverlayID.ToString() + " pixel1=" + pixel1.ToString() + " pixel2=" + pixel2.ToString());

            // add pixel1 value here, 
            builderObjsOverlaidOnPixel.Add((pixel1&0x00ffffff),pixel1);
            // add pixel2 value here, 
            builderObjsOverlaidOnPixel.Add((pixel2&0x00ffffff),pixel2);

            // now test to see of the background pixels are set
            if ((pixel1 & (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_BACKGROUND) != 0)
            {
                backgroundPixelCount++;
            }
            if ((pixel2 & (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_BACKGROUND) != 0)
            {
                backgroundPixelCount++;
            }
            // now test to see of the edge pixels are set
            if ((pixel1 & (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_EDGE) != 0)
            {
                edgePixelCount++;
            }
            if ((pixel2 & (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_EDGE) != 0)
            {
                edgePixelCount++;
            }

            // now calc the IdentKey value. Note we do not have to sort these values
            identKey = CalcIndentKeyFromTwoPixels(pixel1, pixel2);
            
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a new pixel value to the list
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public void Add(int builderObject1Pixel)
        {
            // be sure to turn off the OVERLAY bit if set
            int pixel1 = builderObject1Pixel & (~(int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY);

            //DebugMessage("AddPixelToOverlay ID="+this.BuilderOverlayID.ToString()+" pixel=" + pixel1.ToString());

            // add pixel1 value here, 
            builderObjsOverlaidOnPixel.Add((pixel1 & 0x00ffffff), pixel1);

            // now test to see of the background pixels are set
            if ((pixel1 & (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_BACKGROUND) != 0)
            {
                backgroundPixelCount++;
            }
            // now test to see of the edge pixels are set
            if ((pixel1 & (int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_EDGE) != 0)
            {
                edgePixelCount++;
            }

            // now calc the IdentKey value. Note we do not have to sort these values
            identKey = CalcIndentKeyPlusPixel(pixel1);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates an ident key from two pixels
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public static string CalcIndentKeyFromTwoPixels(int builderObject1Pixel, int builderObject2Pixel)
        {
            // be sure to turn off the OVERLAY bit if set
            int pixel1 = builderObject1Pixel & (~(int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY);
            int pixel2 = builderObject2Pixel & (~(int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY);

            return pixel1.ToString() + "|" + pixel2.ToString() + "|";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates an ident key from the existing state plus a pixel
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public string CalcIndentKeyPlusPixel(int builderObject1Pixel)
        {
            // be sure to turn off the OVERLAY bit if set
            int pixel1 = builderObject1Pixel & (~(int)GCodeBuilderPixelFlagEnum.GCodeBuilderPixelFlag_OVERLAY);

            return IdentKey + pixel1.ToString() + "|";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// gets the unique ident key
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public string IdentKey
        {
            get
            {
                return identKey;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// gets/sets the builderOverlayID
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public int BuilderOverlayID
        {
            get
            {
                return builderOverlayID;
            }
            set
            {
                builderOverlayID = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the backgroundPixelCount. This is set when builderObject pixels are added
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public int BackgroundPixelCount
        {
            get
            {
                return backgroundPixelCount;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the edgePixelCount. This is set when builderObject pixels are added
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public int EdgePixelCount
        {
            get
            {
                return edgePixelCount;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the pixelCount. This is set when builderObject pixels are added
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public int PixelCount
        {
            get
            {
                if (builderObjsOverlaidOnPixel == null) return 0;
                return builderObjsOverlaidOnPixel.Count;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// An indexer for the builderObjectIDs in the collection. 
        /// </summary>
        /// <param name="builderObjectID">the builderObjectID to look for</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public int this[int builderObjID]
        {
            get
            {
                int builderObjIDtmp = 0;
                builderObjsOverlaidOnPixel.TryGetValue((int)(builderObjID & 0x00ffffff), out builderObjIDtmp);
                return builderObjIDtmp;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// provides deep clone functionality
        /// </summary>
        /// <param name="newBuilderOverlayId">the overlay id for the new object</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public GCodeBuilderObjectOverlay DeepClone(int newBuilderOverlayId)
        {
            GCodeBuilderObjectOverlay newObj = new GCodeBuilderObjectOverlay(newBuilderOverlayId);

            // this ought to clone the contents nicely
            foreach (int pixelValue in builderObjsOverlaidOnPixel.Values)
            {
                newObj.Add(pixelValue);
            }
            return newObj;
        }
    }
}
