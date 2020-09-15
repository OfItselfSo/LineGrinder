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
    /// A base class to keep track of the state of the gerber file. This is usually
    /// some modal state implied by the commands which have executed previously
    /// </summary>
    /// <history>
    ///    08 Jul 10  Cynic - Started
    ///    15 Jan 11  Cynic - Added currentLinesAreNotForIsolation code
    /// </history>
    public class GerberFileStateMachine : OISObjBase
    {
        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        // these are the options we use when creating GCode file output
        private FileManager gerberFileManager = new FileManager();

        // these are the toolhead feed rates (etc) currently in operation
        private ToolHeadParameters toolHeadSetup = new ToolHeadParameters();

        /// These values are used if we are flipping in the x and y directions
        private float xFlipMax = 0;
        private float yFlipMax = 0;

        private int lastPlotXCoord = 0;
        private int lastPlotYCoord = 0;

        /// These values are the decimal scaled values from the DCode itself. They
        /// are not yet scaled to plot coordinates.
        private float lastDCodeXCoord = 0;
        private float lastDCodeYCoord = 0;

        private int lastDCode = 0;

        // a copy of the currently active format parameter
        private GerberLine_FSCode formatParameter = new GerberLine_FSCode("", "", 0);

        // the current application units.
        private ApplicationUnitsEnum gerberFileUnits = ApplicationImplicitSettings.DEFAULT_APPLICATION_UNITS;

        // the current file coordinate mode
        public const GerberCoordinateModeEnum DEFAULT_COORDINATE_MODE = GerberCoordinateModeEnum.COORDINATE_ABSOLUTE;
        private GerberCoordinateModeEnum gerberFileCoordinateMode = DEFAULT_COORDINATE_MODE;

        // these values are maintained by the plot control and filled in prior to drawing
        private float isoPlotPointsPerAppUnit = ApplicationImplicitSettings.DEFAULT_VIRTURALCOORD_PER_INCH;

        // these are the centers of all the pads, we might need these
        // to generate pad touchdown code.
        private List<GerberPad> padCenterPointList = new List<GerberPad>();

        private bool referencePinsFound = false;

        // if this is true we are drawing area fill (G36 - G37) or some sort of text label
        // layer. These lines have no width do not get wrapped with an isolation cut. rather
        // the tool bit just runs down the center of those lines
        private const bool DEFAULT_CURRENTLINES_ARENOT_FORISOLATION = false;
        private bool currentLinesAreNotForIsolation = DEFAULT_CURRENTLINES_ARENOT_FORISOLATION;
        // this is a marker which indicates if the currentLinesAreNotForIsolation flag
        // has ever gone true. This enables us to check and see if there is any
        // post processing fixups which need to be performed
        private const bool DEFAULT_CURRENTLINES_ARENOT_FORISOLATION_HASBEENENABLED = false;
        private bool currentLinesAreNotForIsolationHasBeenEnabled = DEFAULT_CURRENTLINES_ARENOT_FORISOLATION_HASBEENENABLED;

        // our collection of apertures
        GerberApertureCollection apertureCollection = new GerberApertureCollection();
        // the currently selected aperture
        GerberLine_ADCode currentAperture = new GerberLine_ADCode("", "", 0);

        public const bool DEFAULT_SHOW_GERBER_CENTERLINES = false;
        private bool showGerberCenterLines = DEFAULT_SHOW_GERBER_CENTERLINES;

        public const bool DEFAULT_SHOW_GERBER_APERTURES = false;
        private bool showGerberApertures = DEFAULT_SHOW_GERBER_APERTURES;

        // this is the color of the Gerber Plot lines
        private Color plotLineColor = ApplicationColorManager.DEFAULT_GERBERPLOT_LINE_COLOR;

        // this plotBrush is the background color of the plot
        public Brush plotBackgroundBrush = ApplicationColorManager.DEFAULT_GERBERPLOT_BACKGROUND_BRUSH;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
        public GerberFileStateMachine()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gerber file options to use. Never ges/sets a null value
        /// </summary>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    06 Sep 10  Cynic - Started
        /// </history>
        public ToolHeadParameters ToolHeadSetup
        {
            get
            {
                if (toolHeadSetup == null) toolHeadSetup = new ToolHeadParameters();
                return toolHeadSetup;
            }
            set
            {
                toolHeadSetup = value;
                if (toolHeadSetup == null) toolHeadSetup = new ToolHeadParameters();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pad center point list. Never get/sets a null value
        /// </summary>
        /// <history>
        ///    30 Aug 10  Cynic - Started
        /// </history>
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
        /// Gets/Sets the referencePinsFound flag
        /// </summary>
        /// <history>
        ///    07 Oct 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    15 Jan 11  Cynic - Started
        /// </history>
        public bool CurrentLinesAreNotForIsolation
        {
            get
            {
                return currentLinesAreNotForIsolation;
            }
            set
            {
                currentLinesAreNotForIsolation = value;
                // if this ever goes true, set the currentLinesAreNotForIsolationHasBeenEnabled flag now
                if (currentLinesAreNotForIsolation == true) currentLinesAreNotForIsolationHasBeenEnabled = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the currentLineIsNotForIsolationHasBeenEnabled value. There is no 
        /// set accessor this is done in the CurrentLinesAreNotForIsolation property
        /// </summary>
        /// <history>
        ///    15 Jan 11  Cynic - Started
        /// </history>
        public bool CurrentLinesAreNotForIsolationHasBeenEnabled
        {
            get
            {
                return currentLinesAreNotForIsolationHasBeenEnabled;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tests to see if the specfied values match a refpin pad
        /// </summary>
        /// <param name="x0">the xcoord</param>
        /// <param name="y0">the ycoord</param>
        /// <param name="padWidth">the padwidth</param>
        /// <history>
        ///    10 Sep 10  Cynic - Started
        /// </history>
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
        /// Gets/Sets the working plot line color.
        /// </summary>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
        public Color PlotLineColor
        {
            get
            {
                return plotLineColor;
            }
            set
            {
                plotLineColor = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the brush for apertures. Will never get null values
        /// </summary>
        /// <history>
        ///    15 Jul 10  Cynic - Started
        /// </history>
        public Brush PlotApertureBrush
        {
            get
            {
                if (showGerberApertures == true) return ApplicationColorManager.DEFAULT_GERBERPLOT_BRUSH_SHOW_APERTURES;
                else return ApplicationColorManager.DEFAULT_GERBERPLOT_BRUSH_NOSHOW_APERTURES;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the working plot brush. Will never get/set null values
        /// </summary>
        /// <history>
        ///    15 Jul 10  Cynic - Started
        /// </history>
        public Brush PlotBackgroundBrush
        {
            get
            {
                if (plotBackgroundBrush == null) plotBackgroundBrush = ApplicationColorManager.DEFAULT_GERBERPLOT_BACKGROUND_BRUSH;
                return plotBackgroundBrush;
            }
            set
            {
                plotBackgroundBrush = value;
                if (plotBackgroundBrush == null) plotBackgroundBrush = ApplicationColorManager.DEFAULT_GERBERPLOT_BACKGROUND_BRUSH;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the aperture collection. Will never set or get a null value.
        /// </summary>
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    07 Jul 10  Cynic - Started
        /// </history>
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
        /// Gets/Sets whether we show the Gerber Apertures when plotting
        /// </summary>
        /// <history>
        ///    12 Jul 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    12 Jul 10  Cynic - Started
        /// </history>
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
        /// Resets the state machine values necessary for plotting to the defaults
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
        public void ResetForPlot()
        {
            lastPlotXCoord = 0;
            lastPlotYCoord = 0;
            lastDCodeXCoord = 0;
            lastDCodeYCoord = 0;
            CurrentAperture = ApertureCollection.ApertureCollection[0];
            ApertureCollection.DisposeAllPens();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the format parameter, will never get or set null
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
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
        /// Gets the current dimension mode. There is no set accessor
        /// as this is derived from the header processing.
        /// </summary>
        /// <history>
        ///    12 Jul 10  Cynic - Started
        /// </history>
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
        /// Gets the current dimension mode. There is no set accessor
        /// as this is derived from the header processing.
        /// </summary>
        /// <history>
        ///    12 Jul 10  Cynic - Started
        /// </history>
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
        /// Gets/Sets the Virtual Coords Per Unit
        /// </summary>
        /// <history>
        ///    13 Jul 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    24 Aug 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    26 Jul 10  Cynic - Started
        ///    24 Aug 10  Cynic - Updated to use file manager
        ///    03 Sep 10  Cynic - Update to use toolhead parameter obj
        /// </history>
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
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
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
        /// Gets/Sets the value we use when flipping in the X direction
        /// </summary>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
        public float XFlipMax
        {
            get
            {
                return xFlipMax;
            }
            set
            {
                xFlipMax = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the value we use when flipping in the Y direction
        /// </summary>
        /// <history>
        ///    21 Aug 10  Cynic - Started
        /// </history>
        public float YFlipMax
        {
            get
            {
                return yFlipMax;
            }
            set
            {
                yFlipMax = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last plot X Coordinate value
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
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
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
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

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last D Code value
        /// </summary>
        /// <history>
        ///    08 Jul 10  Cynic - Started
        /// </history>
        public int LastDCode
        {
            get
            {
                return lastDCode;
            }
            set
            {
                lastDCode = value;
            }
        }

    }
}
