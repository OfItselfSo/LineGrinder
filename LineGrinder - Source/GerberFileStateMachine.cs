using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// A base class to keep track of the state of the gerber file. This is usually
    /// some modal state implied by the commands which have executed previously
    /// </summary>
    public class GerberFileStateMachine : OISObjBase
    {
        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        // these are the options we use when creating GCode file output
        private FileManager gerberFileManager = new FileManager();

        // these are the toolhead feed rates (etc) currently in operation
        private ToolHeadParameters toolHeadSetup = new ToolHeadParameters(36);

        /// These values are the coordinate values from the DCode itself. They
        /// are not yet scaled to plot coordinates. If you set these you must
        /// also set the lastPlot?Coord which is the scaled and offset version
        private float lastDCodeXCoord = 0;
        private float lastDCodeYCoord = 0;

        /// These values are the coordinate values from the DCode. Essentially they
        /// are the lastDCode?Coord which have been scaled and origin shifted. They
        /// are both set at the same time when a DCode uses them
        private int lastPlotXCoord = 0;
        private int lastPlotYCoord = 0;

        // a copy of the currently active format parameter
        private GerberLine_FSCode formatParameter = new GerberLine_FSCode("", "", 0);

        // the current application units.
        private ApplicationUnitsEnum gerberFileUnits = ApplicationImplicitSettings.DEFAULT_APPLICATION_UNITS;

        // the current file coordinate mode
        public const GerberCoordinateModeEnum DEFAULT_COORDINATE_MODE = GerberCoordinateModeEnum.COORDINATE_ABSOLUTE;
        private GerberCoordinateModeEnum gerberFileCoordinateMode = DEFAULT_COORDINATE_MODE;

        // the current file interpolation mode. Actually the default here should be UNKNOWN as per spec but we assume linear for backwards compatibility
        public const GerberInterpolationModeEnum DEFAULT_INTERPOLATION_MODE = GerberInterpolationModeEnum.INTERPOLATIONMODE_LINEAR;
        private GerberInterpolationModeEnum gerberFileInterpolationMode = DEFAULT_INTERPOLATION_MODE;

        // the current file interpolation circular direction mode, the default here is unknown. The spec is quite clear that this must be specified
        public const GerberInterpolationCircularDirectionModeEnum DEFAULT_INTERPOLATIONCIRCULARDIRECTION_MODE = GerberInterpolationCircularDirectionModeEnum.DIRECTIONMODE_UNKNOWN;
        private GerberInterpolationCircularDirectionModeEnum gerberFileInterpolationCircularDirectionMode = DEFAULT_INTERPOLATIONCIRCULARDIRECTION_MODE;

        // the current file interpolation circular quadrant mode, the default here is unknown. The spec is quite clear that this must be specified
        public const GerberInterpolationCircularQuadrantModeEnum DEFAULT_INTERPOLATIONCIRCULARQUADRANT_MODE = GerberInterpolationCircularQuadrantModeEnum.QUADRANTMODE_UNKNOWN;
        private GerberInterpolationCircularQuadrantModeEnum gerberFileInterpolationCircularQuadrantMode = DEFAULT_INTERPOLATIONCIRCULARQUADRANT_MODE;

        public const GerberLayerPolarityEnum DEFAULT_LAYERPOLARITY = GerberLayerPolarityEnum.DARK;
        private GerberLayerPolarityEnum gerberFileLayerPolarity = DEFAULT_LAYERPOLARITY;

        // these values are maintained by the plot control and filled in prior to drawing
        private float isoPlotPointsPerAppUnit = ApplicationImplicitSettings.DEFAULT_VIRTURALCOORD_PER_INCH;

        // these are the centers of all the pads, we might need these
        // to generate pad touchdown code.
        private List<GerberPad> padCenterPointList = new List<GerberPad>();

        // some gerber objects form complex objects. This keeps a list of the ones in use
        // until we do something with them. (Usually this just means drawing in a background etc)
        private List<GerberLine_DCode> complexObjectList_DCode = new List<GerberLine_DCode>();

        // this indicates if we have found reference pins
        private bool referencePinsFound = false;
        // this indicates that the user specified reference pins in the manager but we could not
        // find them and the user is ok with this
        private bool ignoreReferencePinsIfNotFound = false;

        // if this is true we are drawing area fill (G36 - G37) or some sort of text label
        // layer. These lines have no width do not get wrapped with an isolation cut. rather
        // the tool bit just runs down the center of those lines
        private const bool DEFAULT_CONTOURMODE = false;
        private bool contourDrawingModeEnabled = DEFAULT_CONTOURMODE;

        // our collection of apertures
        GerberApertureCollection apertureCollection = new GerberApertureCollection();
        // the currently selected aperture
        GerberLine_ADCode currentAperture = new GerberLine_ADCode("", "", 0);

        // our collection of macros
        GerberMacroCollection macroCollection = new GerberMacroCollection();

        public const bool DEFAULT_SHOW_GERBER_CENTERLINES = false;
        private bool showGerberCenterLines = DEFAULT_SHOW_GERBER_CENTERLINES;

        public const bool DEFAULT_SHOW_GERBER_APERTURES = false;
        private bool showGerberApertures = DEFAULT_SHOW_GERBER_APERTURES;


        public int diagnosticVar = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberFileStateMachine()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the state machine values necessary for plotting to the defaults
        /// </summary>
        public void ResetForPlot()
        {
            lastPlotXCoord = 0;
            lastPlotYCoord = 0;
            lastDCodeXCoord = 0;
            lastDCodeYCoord = 0;
            if (ApertureCollection.Count()>0) CurrentAperture = ApertureCollection.ApertureList[0];
            else { currentAperture = new GerberLine_ADCode("", "", 0); }

            ApertureCollection.DisposeAllPens();
            //padCenterPointList = new List<GerberPad>();

            gerberFileInterpolationMode = DEFAULT_INTERPOLATION_MODE;
            gerberFileInterpolationCircularDirectionMode = DEFAULT_INTERPOLATIONCIRCULARDIRECTION_MODE;
            gerberFileInterpolationCircularQuadrantMode = DEFAULT_INTERPOLATIONCIRCULARQUADRANT_MODE;
            gerberFileLayerPolarity = DEFAULT_LAYERPOLARITY;

            contourDrawingModeEnabled = DEFAULT_CONTOURMODE;
            complexObjectList_DCode = new List<GerberLine_DCode>();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gerber file options to use. Never ges/sets a null value
        /// </summary>
        public FileManager GerberFileManager
        {
            get
            {
                if (gerberFileManager == null) gerberFileManager = new FileManager();
                return gerberFileManager;
            }
            set
            {
                gerberFileManager = value;
                if (gerberFileManager == null) gerberFileManager = new FileManager();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the tool head setup. This determines which set of toolhead specs 
        /// (eg: zDepth, xySpeed, etc) we use for the GCode Generation, These can be 
        ///  different for iso cuts, refPins, edgeMill etc. Will never get or set null.
        /// </summary>
        public ToolHeadParameters ToolHeadSetup
        {
            get
            {
                if (toolHeadSetup == null) toolHeadSetup = new ToolHeadParameters(39);
                return toolHeadSetup;
            }
            set
            {
                toolHeadSetup = value;
                if (toolHeadSetup == null) toolHeadSetup = new ToolHeadParameters(50);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pad center point list. Never get/sets a null value
        /// </summary>
        public List<GerberPad> PadCenterPointList
        {
            get
            {
                if (padCenterPointList == null) padCenterPointList = new List<GerberPad>();
                return padCenterPointList;
            }
            set
            {
                padCenterPointList = value;
                if (padCenterPointList == null) padCenterPointList = new List<GerberPad>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the ignoreReferencePinsIfNotFound flag
        /// </summary>
        public bool IgnoreReferencePinsIfNotFound
        {
            get
            {
                return ignoreReferencePinsIfNotFound;
            }
            set
            {
                ignoreReferencePinsIfNotFound = value;
            }
        }
        
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the referencePinsFound flag
        /// </summary>
        public bool ReferencePinsFound
        {
            get
            {
                return referencePinsFound;
            }
            set
            {
                referencePinsFound = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets /Sets the currentLineIsNotForIsolation value
        /// </summary>
        public bool ContourDrawingModeEnabled
        {
            get
            {
                return contourDrawingModeEnabled;
            }
            set
            {
                contourDrawingModeEnabled = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Determines if the settings are such that we are drawing with clear polarity
        /// </summary>
        public bool IsUsingClearPolarity
        {
            get
            {
                if(GerberFileLayerPolarity== GerberLayerPolarityEnum.CLEAR) return true;
                return false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets /Sets the current complex object list for DCodes
        /// </summary>
        public List<GerberLine_DCode> ComplexObjectList_DCode
        {
            get
            {
                if(complexObjectList_DCode==null) complexObjectList_DCode = new List<GerberLine_DCode>();
                return complexObjectList_DCode;
            }
            set
            {
                complexObjectList_DCode = value;
                if (complexObjectList_DCode == null) complexObjectList_DCode = new List<GerberLine_DCode>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a list of builderIDs from a complextObjectList
        /// </summary>
        /// <returns>list of ints which are the builderIDs, never returns invalid or 0 ones</returns>
        public List<int> GetListOfIsoPlotBuilderIDsFromComplexObjectList_DCode()
        {
            List<int> outList = new List<int>();
            foreach (GerberLine_DCode dcodeObj in ComplexObjectList_DCode)
            {
                // reject any invalid ones
                if(dcodeObj.IsoPlotObjectID<=0) continue;
                // add it
                outList.Add(dcodeObj.IsoPlotObjectID);
            }

            return outList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the bounding rectangle from the contents of the complex object list
        /// </summary>
        /// <returns>a bounding rectangle or one with 0 length and height if null</returns>
        public Rectangle GetBoundingRectangleFromComplexObjectList_DCode()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            // return this by default
            if (ComplexObjectList_DCode.Count == 0) return new Rectangle(0,0,0,0);

            // loop through every D code in the complexObjectList_DCode and try to find the absolute
            // max and min X and Y coord. Including compensation for arcs which may be drawn
            // outside the max and min start points of the DCode itself.
            foreach (GerberLine_DCode dcodeObj in ComplexObjectList_DCode)
            {
                int tmpVal = 0;

                // are we on a line draw or a arc draw?
                if(dcodeObj.LastRadiusPlotCoord<=0)
                {
                    // we are on a line draw
                    tmpVal = dcodeObj.LastPlotXCoordStart ;
                    if (tmpVal < minX) minX = tmpVal;
                    if (tmpVal > maxX) maxX = tmpVal;

                    tmpVal = dcodeObj.LastPlotYCoordStart;
                    if (tmpVal < minY) minY = tmpVal;
                    if (tmpVal > maxY) maxY = tmpVal;

                    // do the end coord
                    tmpVal = dcodeObj.LastPlotXCoordEnd;
                    if (tmpVal < minX) minX = tmpVal;
                    if (tmpVal > maxX) maxX = tmpVal;

                    tmpVal = dcodeObj.LastPlotYCoordEnd ;
                    if (tmpVal < minY) minY = tmpVal;
                    if (tmpVal > maxY) maxY = tmpVal;
                }
                else
                {
                    // we are on an arc draw, compensate for the radius, use the center coords
                    tmpVal = dcodeObj.LastPlotCenterXCoord - dcodeObj.LastRadiusPlotCoord;
                    if (tmpVal < minX) minX = tmpVal;
                    if (tmpVal > maxX) maxX = tmpVal;

                    tmpVal = dcodeObj.LastPlotCenterXCoord + dcodeObj.LastRadiusPlotCoord;
                    if (tmpVal < minX) minX = tmpVal;
                    if (tmpVal > maxX) maxX = tmpVal;

                    tmpVal = dcodeObj.LastPlotCenterYCoord - dcodeObj.LastRadiusPlotCoord;
                    if (tmpVal < minY) minY = tmpVal;
                    if (tmpVal > maxY) maxY = tmpVal;

                    tmpVal = dcodeObj.LastPlotCenterYCoord + dcodeObj.LastRadiusPlotCoord;
                    if (tmpVal < minY) minY = tmpVal;
                    if (tmpVal > maxY) maxY = tmpVal;

                }
            }

            if (minX == int.MaxValue) return new Rectangle(0, 0, 0, 0); 
            if (minY == int.MaxValue) return new Rectangle(0, 0, 0, 0);
            if (maxX == int.MinValue) return new Rectangle(0, 0, 0, 0);
            if (maxY == int.MinValue) return new Rectangle(0, 0, 0, 0);

            // create the rectangle now
            Rectangle outRect = new Rectangle(minX, minY, maxX - minX, maxY - minY);

            return outRect;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Builds a graphics path from the complex object list
        /// </summary>
        /// <returns>the graphicsPath object</returns>
        public GraphicsPath GetGraphicsPathFromComplexObjectList_DCode()
        {
            GraphicsPath gPath = new GraphicsPath();

            // go through the complex object list and add the lines 
            // and arcs to make a graphics path 

            // first we pickup everything that bounds the region
            foreach (GerberLine_DCode dcodeObj in ComplexObjectList_DCode)
            {
                // we are only intersted in things that actually draw a line or an arc
                if (dcodeObj.DCode != 1) continue;
 
                // ok at this point we know we are not bothering with the stuff that the coincident lines 
                // describe. This means that we are looking at the boundary of the fill.
                if (dcodeObj.LastRadiusPlotCoord != 0)
                {
                    // non zero last radius assume we were an arc, get the enclosing rectangle
                    Rectangle outerRect = new Rectangle(dcodeObj.LastPlotCenterXCoord - dcodeObj.LastRadiusPlotCoord, dcodeObj.LastPlotCenterYCoord - dcodeObj.LastRadiusPlotCoord, 2 * dcodeObj.LastRadiusPlotCoord, 2 * dcodeObj.LastRadiusPlotCoord);
                    // add the arc, note the start angle the sweep angle must always be counterclockwise here, the start angle should already be adjusted so that +ve X axis is the zero
                    float clockwiseSweepAngle = MiscGraphicsUtils.ConvertSweepAngleToCounterClockwise(dcodeObj.LastSweepAngleWasClockwise, dcodeObj.LastSweepAngle);
                    gPath.AddArc(outerRect, dcodeObj.LastStartAngle, clockwiseSweepAngle);
                }
                else
                {
                    // assume we are a line
                    gPath.AddLine(dcodeObj.LastPlotXCoordStart, dcodeObj.LastPlotYCoordStart, dcodeObj.LastPlotXCoordEnd, dcodeObj.LastPlotYCoordEnd);
                }
            } // bottom of foreach (GerberLine_DCode dcodeObj in ComplexObjectList_DCode)

            return gPath;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tests to see if the specfied values match a refpin pad
        /// </summary>
        /// <param name="x0">the xcoord</param>
        /// <param name="y0">the ycoord</param>
        /// <param name="padWidth">the padwidth</param>
        public bool IsThisARefPinPad(float x0, float y0, float padWidth)
        {
            // just run through and test, not very efficient
            foreach (GerberPad padObj in PadCenterPointList)
            {
                if (padObj.IsRefPin == false) continue;
                if (padObj.X0 != x0) continue;
                if (padObj.Y0 != y0) continue;
                if (padObj.PadDiameter != padWidth) continue;
                // this is a match - say so
                return true;
            }
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tests to see if the specfied values match an Ignore pad
        /// </summary>
        /// <param name="x0">the xcoord</param>
        /// <param name="y0">the ycoord</param>
        /// <param name="padWidth">the padwidth</param>
        public bool IsThisAnIgnorePad(float x0, float y0, float padWidth)
        {
            // just run through and test, not very efficient
            foreach (GerberPad padObj in PadCenterPointList)
            {
                if (padObj.IgnoreDueToSize == false) continue;
                if (padObj.X0 != x0) continue;
                if (padObj.Y0 != y0) continue;
                if (padObj.PadDiameter != padWidth) continue;
                // this is a match - say so
                return true;
            }
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the  gerber plot line/foreground color.
        /// </summary>
        public Color GerberForegroundColor
        {
            get
            {
                if (IsUsingClearPolarity == true) return ApplicationColorManager.DEFAULT_GERBERPLOT_BACKGROUND_COLOR;
                else return ApplicationColorManager.DEFAULT_GERBERPLOT_FOREGROUND_COLOR;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the brush for apertures. 
        /// </summary>
        public Brush GerberApertureBrush
        {
            get
            {
                // we are showing apertures use a distinct brush otherwise just the foreground
                if (showGerberApertures == true) return GerberDistinctApertureBrush;
                else return GerberForegroundBrush;
             }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the gerber plot brush we use when showing distinct apertures
        /// </summary>
        public Brush GerberDistinctApertureBrush
        {
            get
            {
                return ApplicationColorManager.DEFAULT_GERBERPLOT_SHOWAPERTURE_BRUSH;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gerber plot foreground  brush. 
        /// </summary>
        public Brush GerberForegroundBrush
        {
            get
            {
                if (IsUsingClearPolarity == true) return ApplicationColorManager.DEFAULT_GERBERPLOT_BACKGROUND_BRUSH;
                else return ApplicationColorManager.DEFAULT_GERBERPLOT_FOREGROUND_BRUSH;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gerber plot background  brush. 
        /// </summary>
        public Brush GerberBackgroundBrush
        {
            get
            {
                return ApplicationColorManager.DEFAULT_GERBERPLOT_BACKGROUND_BRUSH;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gerber plot contour region fill  brush. 
        /// </summary>
        public Brush GerberContourFillBrush
        {
            get
            {
                return GerberForegroundBrush;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the appropriate background fill mode according to polarity. 
        /// NOTE: we assume we want a background fill just what kind is based on the 
        /// polarity.
        /// </summary>
        public GSFillModeEnum BackgroundFillModeAccordingToPolarity
        {
            get
            {
                if (IsUsingClearPolarity == true) return GSFillModeEnum.FillMode_ERASE;
                else return GSFillModeEnum.FillMode_BACKGROUND;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the aperture collection. Will never set or get a null value.
        /// </summary>
        public GerberLine_ADCode CurrentAperture
        {
            get
            {
                if (currentAperture == null) currentAperture = new GerberLine_ADCode("", "", 0);
                return currentAperture;
            }
            set
            {
                currentAperture = value;
                if (apertureCollection == null) currentAperture = new GerberLine_ADCode("", "", 0);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the aperture collection. Will never set or get a null value.
        /// </summary>
        public GerberApertureCollection ApertureCollection
        {
            get
            {
                if (apertureCollection == null) apertureCollection = new GerberApertureCollection();
                return apertureCollection;
            }
            set
            {
                apertureCollection = value;
                if (apertureCollection == null) apertureCollection = new GerberApertureCollection();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the macro collection. Will never set or get a null value.
        /// </summary>
        public GerberMacroCollection MacroCollection
        {
            get
            {
                if (macroCollection == null) macroCollection = new GerberMacroCollection();
                return macroCollection;
            }
            set
            {
                macroCollection = value;
                if (macroCollection == null) macroCollection = new GerberMacroCollection();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether we show the Gerber Apertures when plotting
        /// </summary>
        public bool ShowGerberApertures
        {
            get
            {
                return showGerberApertures;
            }
            set
            {
                showGerberApertures = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether we show the Gerber Centerlines when plotting
        /// </summary>
        public bool ShowGerberCenterLines
        {
            get
            {
                return showGerberCenterLines;
            }
            set
            {
                showGerberCenterLines = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the format parameter, will never get or set null
        /// </summary>
        public GerberLine_FSCode FormatParameter
        {
            get
            {
                if (formatParameter == null) formatParameter = new GerberLine_FSCode("", "", 0);
                return formatParameter;
            }
            set
            {
                formatParameter = value;
                if (formatParameter == null) formatParameter = new GerberLine_FSCode("", "", 0);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current file units. There is no set accessor
        /// as this is derived from the header processing in the file itself.
        /// </summary>
        public ApplicationUnitsEnum GerberFileUnits
        {
            get
            {
                return gerberFileUnits;
            }
            set
            {
                gerberFileUnits = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current dimension mode. 
        /// </summary>
        public GerberCoordinateModeEnum GerberFileCoordinateMode
        {
            get
            {
                return gerberFileCoordinateMode;
            }
            set
            {
                gerberFileCoordinateMode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current interpolation mode. 
        /// </summary>
        public GerberInterpolationModeEnum GerberFileInterpolationMode
        {
            get
            {
                return gerberFileInterpolationMode;
            }
            set
            {
                gerberFileInterpolationMode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether the segment needs to pay attention to multi quadrant mode
        /// </summary>
        public bool IsInMultiQuadrantMode
        {
            get
            {
                if (gerberFileInterpolationCircularQuadrantMode == GerberInterpolationCircularQuadrantModeEnum.QUADRANTMODE_MULTI) return true;
                return false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current interpolation mode. 
        /// </summary>
        public GerberInterpolationCircularQuadrantModeEnum GerberFileInterpolationCircularQuadrantMode
        {
            get
            {
                return gerberFileInterpolationCircularQuadrantMode;
            }
            set
            {
                gerberFileInterpolationCircularQuadrantMode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current layer polarity setting 
        /// </summary>
        public GerberLayerPolarityEnum GerberFileLayerPolarity
        {
            get
            {
                return gerberFileLayerPolarity;
            }
            set
            {
                gerberFileLayerPolarity = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current interpolation circular direction mode. 
        /// </summary>
        public GerberInterpolationCircularDirectionModeEnum GerberFileInterpolationCircularDirectionMode
        {
            get
            {
                return gerberFileInterpolationCircularDirectionMode;
            }
            set
            {
                gerberFileInterpolationCircularDirectionMode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current wantClockWise value. This is just a bool based off
        /// of the GerberFileInterpolationCircularDirectionMode
        /// </summary>
        public bool WantClockWise
        {
            get
            {
                if (gerberFileInterpolationCircularDirectionMode == GerberInterpolationCircularDirectionModeEnum.DIRECTIONMODE_CLOCKWISE) return true;
                return false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Virtual Coords Per Unit
        /// </summary>
        public float IsoPlotPointsPerAppUnit
        {
            get
            {
                return isoPlotPointsPerAppUnit;
            }
            set
            {
                isoPlotPointsPerAppUnit = value;
            }
        }
 
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the operation mode from the filemanager. Just a shortcut
        /// </summary>
        public FileManager.OperationModeEnum OperationMode
        {
            get
            {
                return GerberFileManager.OperationMode;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Isolation width
        public float IsolationWidth
        {
            get
            {
                return ToolHeadSetup.ToolWidth;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last DCode X Coordinate value
        /// </summary>
        public float LastDCodeXCoord
        {
            get
            {
                return lastDCodeXCoord;
            }
            set
            {
                lastDCodeXCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last DCode Y Coordinate value
        /// </summary>
        public float LastDCodeYCoord
        {
            get
            {
                return lastDCodeYCoord;
            }
            set
            {
                lastDCodeYCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last plot X Coordinate value
        /// </summary>
        public int LastPlotXCoord
        {
            get
            {
                return lastPlotXCoord;
            }
            set
            {
                lastPlotXCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last plot Y Coordinate value
        /// </summary>
        public int LastPlotYCoord
        {
            get
            {
                return lastPlotYCoord;
            }
            set
            {
                lastPlotYCoord = value;
            }
        }

    }
}

