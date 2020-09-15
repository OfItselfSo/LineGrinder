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
    ///    05 Aug 10  Cynic - Started
    ///    26 Feb 11  Cynic - Added the UserYCoordOriginAdjust codes
    /// </history>
    public class GCodeFileStateMachine : OISObjBase
    {

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        // ####################################################################
        // ##### Variables needed in order to output the gcode file or draw it
        // ####################################################################

        // these are the options we use when creating GCode file output
        private FileManager gcodeFileManager = new FileManager();

        // these are the toolhead feed rates (etc) currently in operation
        private ToolHeadParameters toolHeadSetup = new ToolHeadParameters();

        private ApplicationUnitsEnum gcodeUnits = ApplicationImplicitSettings.DEFAULT_APPLICATION_UNITS;

        /// GCode commands (such as G1) are often relative to the current position of the 
        /// tool head. The values below are the current absolute position of the toolhead
        /// derived from the previous lines of the GCode. 
        private float lastGCodeXCoord = 0;
        private float lastGCodeYCoord = 0;
        private float lastGCodeZCoord = 0;

        // this is the last specified feedrate in the gcode file. This is 
        private float lastFeedRate = 0;

        // number of digits in the line numbers - we prepend leading zeros
        private const int LINENUMBER_DIGITS = 5;
        private const int START_LINE_NUMBER = 0;
        private int lineNumber = START_LINE_NUMBER;

        public const string DEFAULT_LINE_TERMINATOR = "\r\n";
        public string lineTerminator = DEFAULT_LINE_TERMINATOR;

        // these are the centers of all the pads, we might need these
        // to generate pad touchdown code. The diameters here are the
        // diameters of the gerber aperture that flashed them. There is no 
        // isolation cut information associated with them
        private List<GerberPad> padCenterPointList = new List<GerberPad>();

        // These are the values ADDed to the X and Y coords of the GCODE lines
        // in order to move the origin to a place of the users taste. These
        // are purely cosmetic (in Line Grinder) and are applied only at the very
        // end of the processing - just before the output GCode is generated.
        // the units are application units (inches or mm)
        private float userXCoordOriginAdjust = 100;
        private float userYCoordOriginAdjust = 100;

        // ####################################################################
        // ##### Variables only needed to draw the gcode on the screen
        // ####################################################################

        // this is the color we use
        private Color plotLineColor = ApplicationColorManager.DEFAULT_GCODEPLOT_LINE_COLOR;

        // this is the pen we use to plot the gcode lines
        private Pen plotBorderPen = ApplicationColorManager.DEFAULT_GCODEPLOT_BORDER_PEN;

        // this is the pen we use to plot the gcode line centercuts
        private Pen plotCutLinePen = ApplicationColorManager.DEFAULT_GCODEPLOT_CUTLINE_PEN;

        // this plotBrush should be the same colour as the plotLineColor
        public Brush plotBorderBrush = ApplicationColorManager.DEFAULT_GCODEPLOTBORDER_BRUSH;

        // this value is are maintained by the plot control and filled in prior to drawing
        private float isoPlotPointsPerAppUnit = ApplicationImplicitSettings.DEFAULT_VIRTURALCOORD_PER_INCH;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public GCodeFileStateMachine()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the gcode file to the defaults necessary for plotting
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public void ResetForPlot()
        {
            lastGCodeXCoord = 0;
            lastGCodeYCoord = 0;
            lastGCodeZCoord = 0;
            DisposeAllPens();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the user X Coord Origin Adjust value. 
        /// These are the values ADDed to the X and Y coords of the GCODE lines
        /// in order to move the origin to a place of the users taste. These
        /// are purely cosmetic (in Line Grinder) and are applied only at the very
        /// end of the processing - just before the output GCode is generated.
        /// the units are application units (inches or mm)
        /// </summary>
        /// <history>
        ///    26 Feb 11  Cynic - Started
        /// </history>
        public float UserXCoordOriginAdjust
        {
            get
            {
                return userXCoordOriginAdjust;
            }
            set
            {
                userXCoordOriginAdjust = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the user Y Coord Origin Adjust value. 
        /// These are the values ADDed to the X and Y coords of the GCODE lines
        /// in order to move the origin to a place of the users taste. These
        /// are purely cosmetic (in Line Grinder) and are applied only at the very
        /// end of the processing - just before the output GCode is generated.
        /// the units are application units (inches or mm)
        /// </summary>
        /// <history>
        ///    26 Feb 11  Cynic - Started
        /// </history>
        public float UserYCoordOriginAdjust
        {
            get
            {
                return userYCoordOriginAdjust;
            }
            set
            {
                userYCoordOriginAdjust = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the tool head setup. This determines which set of toolhead specs 
        /// (eg: zDepth, xySpeed, etc) we use for the GCode Generation, These can be 
        ///  different for iso cuts, refPins, edgeMill etc. Will never get or set null.
        /// </summary>
        /// <history>
        ///    03 Sep 10  Cynic - Started
        /// </history>
        public ToolHeadParameters ToolHeadSetup
        {
            get
            {
                if(toolHeadSetup == null) toolHeadSetup =  new ToolHeadParameters();
                return toolHeadSetup;
            }
            set
            {
                toolHeadSetup = value;
                if (toolHeadSetup == null) toolHeadSetup = new ToolHeadParameters();
            }
        }

/*
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pad center point list. Never get/sets a null value
        /// </summary>
        /// <history>
        ///    30 Aug 10  Cynic - Started
        /// </history>
        public List<GCodePad> PadCenterPointList
        {
            get
            {
                if (padCenterPointList == null) padCenterPointList = new List<GCodePad>();
                return padCenterPointList;
            }
            set
            {
                padCenterPointList = value;
                if (padCenterPointList == null) padCenterPointList = new List<GCodePad>();
            }
        }
*/
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the gcode file to the defaults necessary for emitting the GCode file
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public void ResetForEmitFile()
        {
            lastGCodeXCoord = 0;
            lastGCodeYCoord = 0;
            lastGCodeZCoord = 0;
            lineNumber = START_LINE_NUMBER;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gcode file options to use. Never ges/sets a null value
        /// </summary>
        /// <history>
        ///    10 Aug 10  Cynic - Started
        /// </history>
        public FileManager GCodeFileManager
        {
            get
            {
                if (gcodeFileManager == null) gcodeFileManager = new FileManager();
                return gcodeFileManager;
            }
            set
            {
                gcodeFileManager = value;
                if (gcodeFileManager == null) gcodeFileManager = new FileManager();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the next available line number
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public int GetNextLineNumber()
        {
            lineNumber++;
            return lineNumber;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the next available line number in a string
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public string BuildNextLineNumberString()
        {
            return GCodeLine.GCODEWORD_LINENUMBER + string.Format("{0:d" + LINENUMBER_DIGITS.ToString() + "}", GetNextLineNumber());
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/sets the current units in use. 
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public ApplicationUnitsEnum GCodeUnits
        {
            get
            {
                return gcodeUnits;
            }
            set
            {
                gcodeUnits = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the X Scale Factor
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
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
        /// Gets/Sets the working plot line color.
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
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
        /// Gets/Sets the working line pen.
        /// </summary>
        /// <history>
        ///    09 Aug 10  Cynic - Started
        /// </history>
        public Pen PlotBorderPen
        {
            get
            {
                return plotBorderPen;
            }
            set
            {
                plotBorderPen = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the cut line pen.
        /// </summary>
        /// <history>
        ///    11 Aug 10  Cynic - Started
        /// </history>
        public Pen PlotCutLinePen
        {
            get
            {
                return plotCutLinePen;
            }
            set
            {
                plotCutLinePen = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the working plot border brush. Will never get/set null values
        /// </summary>
        /// <history>
        ///    09 Aug 10  Cynic - Started
        /// </history>
        public Brush PlotBorderBrush
        {
            get
            {
                if (plotBorderBrush == null) plotBorderBrush = ApplicationColorManager.DEFAULT_GCODEPLOTBORDER_BRUSH;
                return plotBorderBrush;
            }
            set
            {
                plotBorderBrush = value;
                if (plotBorderBrush == null) plotBorderBrush = ApplicationColorManager.DEFAULT_GCODEPLOTBORDER_BRUSH;
            }
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
            if ((plotBorderPen != null) && (plotBorderPen != ApplicationColorManager.DEFAULT_GCODEPLOT_BORDER_PEN))
            {
                  plotBorderPen.Dispose();
            }
            plotBorderPen = ApplicationColorManager.DEFAULT_GCODEPLOT_BORDER_PEN;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets or Sets the lineTerminator. Will never get or set a null value
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public string LineTerminator
        {
            get
            {
                if (lineTerminator == null) lineTerminator = "";
                return lineTerminator;
            }
            set
            {
                lineTerminator = value;
                if (lineTerminator == null) lineTerminator = "";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last used GCode X Coordinate value
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public float LastGCodeXCoord
        {
            get
            {
                return lastGCodeXCoord;
            }
            set
            {
                lastGCodeXCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last used GCode Y Coordinate value
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public float LastGCodeYCoord
        {
            get
            {
                return lastGCodeYCoord;
            }
            set
            {
                lastGCodeYCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last used GCode Z Coordinate value
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public float LastGCodeZCoord
        {
            get
            {
                return lastGCodeZCoord;
            }
            set
            {
                lastGCodeZCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last used feedrate
        /// </summary>
        /// <history>
        ///    11 Sep 10  Cynic - Converted
        /// </history>
        public float LastFeedRate
        {
            get
            {
                return lastFeedRate;
            }
            set
            {
                lastFeedRate = value;
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
                return GCodeFileManager.OperationMode;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Isolation width
        /// <history>
        ///    05 Aug 10  Cynic - Started
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
        /// Gets/Sets the current XY Feedrate
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        ///    24 Aug 10  Cynic - Updated to use file manager
        ///    03 Sep 10  Cynic - Update to use toolhead parameter obj
        /// </history>
        public float CurrentXYFeedrate
        {
            get
            {
                return ToolHeadSetup.XYFeedRate;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Z Feedrate
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        ///    24 Aug 10  Cynic - Updated to use file manager
        ///    03 Sep 10  Cynic - Update to use toolhead parameter obj
        /// </history>
        public float CurrentZFeedrate
        {
            get
            {
                return ToolHeadSetup.ZFeedRate;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Z Coord For Cut
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        ///    24 Aug 10  Cynic - Updated to use file manager
        ///    03 Sep 10  Cynic - Update to use toolhead parameter obj
        /// </history>
        public float ZCoordForCut
        {
            get
            {
                return ToolHeadSetup.ZCutLevel;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Z Coord For Alt1 cut levels
        /// </summary>
        /// <history>
        ///    31 Aug 10  Cynic - Started
        ///    05 Sep 10  Cynic - Converted to Alt1 cuts
        /// </history>
        public float ZCoordForAlt1Cut
        {
            get
            {
                return ToolHeadSetup.ZAlt1CutLevel;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Z Coord For Move
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        ///    24 Aug 10  Cynic - Updated to use file manager
        ///    03 Sep 10  Cynic - Update to use toolhead parameter obj
        /// </history>
        public float ZCoordForMove
        {
            get
            {
                return ToolHeadSetup.ZMoveLevel;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Z Coord For Clear
        /// </summary>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        ///    24 Aug 10  Cynic - Updated to use file manager
        ///    03 Sep 10  Cynic - Update to use toolhead parameter obj
        /// </history>
        public float ZCoordForClear
        {
            get
            {
                return ToolHeadSetup.ZClearLevel;
            }
        }

    }
}
