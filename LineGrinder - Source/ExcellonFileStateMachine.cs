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
    /// A base class to keep track of the state of the excellon file. This is usually
    /// some modal state implied by the commands which have executed previously
    /// </summary>
    /// <history>
    ///    01 Sep 10  Cynic - Started
    /// </history>
    public class ExcellonFileStateMachine : OISObjBase
    {

        public const ApplicationUnitsEnum DEFAULT_EXCELLONFILE_UNITS = ApplicationUnitsEnum.INCHES;
        private ApplicationUnitsEnum excellonFileUnits = DEFAULT_EXCELLONFILE_UNITS;

        // this is the linenumber which indicates we are at the end of the header
        private int headerEndLine = int.MaxValue;

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the excellon file or gCode file

        // these are the options we use when creating GCode file output
        private FileManager excellonFileManager = new FileManager();

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

        public const float DEFAULT_DRILL_WIDTH = 0.020f;
        private float lastDrillWidth = DEFAULT_DRILL_WIDTH;

        // these values are maintained by the plot control and filled in prior to drawing
        private float isoPlotPointsPerAppUnit = ApplicationImplicitSettings.DEFAULT_VIRTURALCOORD_PER_INCH;

        // our collection of tool definitions
        List<ExcellonLine_ToolTable> toolCollection = new List<ExcellonLine_ToolTable>();

        // this is the color of the Excellon Plot lines
        private Color plotLineColor = ApplicationColorManager.DEFAULT_EXCELLONPLOT_LINE_COLOR;

        // this plotBrush is the background color of the plot
        public Brush plotBackgroundBrush = ApplicationColorManager.DEFAULT_EXCELLONPLOT_BACKGROUND_BRUSH;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public ExcellonFileStateMachine()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the line number at which the header ends
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public int HeaderEndLine
        {
            get
            {
                return headerEndLine;
            }
            set
            {
                headerEndLine = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last drill width used
        /// </summary>
        /// <history>
        ///    05 Sep 10  Cynic - Started
        /// </history>
        public float LastDrillWidth
        {
            get
            {
                return lastDrillWidth;
            }
            set
            {
                lastDrillWidth = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the excellon file options to use. Never ges/sets a null value
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public FileManager ExcellonFileManager
        {
            get
            {
                if (excellonFileManager == null) excellonFileManager = new FileManager();
                return excellonFileManager;
            }
            set
            {
                excellonFileManager = value;
                if (excellonFileManager == null) excellonFileManager = new FileManager();
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
        /// Gets the current units mode. 
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public ApplicationUnitsEnum ExcellonFileUnits
        {
            get
            {
                return excellonFileUnits;
            }
            set
            {
                excellonFileUnits = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the working plot line color.
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public Brush PlotApertureBrush
        {
            get
            {
                return ApplicationColorManager.DEFAULT_EXCELLONPLOT_BRUSH_NOSHOW_APERTURES;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the working plot brush. Will never get/set null values
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public Brush PlotBackgroundBrush
        {
            get
            {
                if (plotBackgroundBrush == null) plotBackgroundBrush = ApplicationColorManager.DEFAULT_EXCELLONPLOT_BACKGROUND_BRUSH;
                return plotBackgroundBrush;
            }
            set
            {
                plotBackgroundBrush = value;
                if (plotBackgroundBrush == null) plotBackgroundBrush = ApplicationColorManager.DEFAULT_EXCELLONPLOT_BACKGROUND_BRUSH;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the aperture collection. Will never set or get a null value.
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public List<ExcellonLine_ToolTable> ToolCollection
        {
            get
            {
                if (toolCollection == null) toolCollection = new List<ExcellonLine_ToolTable>();
                return toolCollection;
            }
            set
            {
                toolCollection = value;
                if (toolCollection == null) toolCollection = new List<ExcellonLine_ToolTable>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the toolTable object based on the table id
        /// </summary>
        /// <history>
        ///    05 Sep 10  Cynic - Started
        /// </history>
        public ExcellonLine_ToolTable GetToolTableObjectByToolNumber(int toolNumber)
        {
            foreach (ExcellonLine_ToolTable toolTabObj in ToolCollection)
            {
                if(toolTabObj.ToolNumber==toolNumber) return toolTabObj;
            }
            // not found
            return null;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the toolTable largest diameter drill dimension
        /// </summary>
        /// <history>
        ///    05 Sep 10  Cynic - Started
        /// </history>
        public float GetMaxToolCollectionDrillDiameter()
        {
            float maxDiameter=0;
            foreach (ExcellonLine_ToolTable toolTabObj in ToolCollection)
            {
                if(toolTabObj.DrillDiameter>maxDiameter) maxDiameter = toolTabObj.DrillDiameter;
            }
            // not found
            return maxDiameter;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the state machine values necessary for plotting to the defaults
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public void ResetForPlot()
        {
            lastPlotXCoord = 0;
            lastPlotYCoord = 0;
            lastDCodeXCoord = 0;
            lastDCodeYCoord = 0;
            lastDrillWidth = 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Virtual Coords Per Unit
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
        /// </history>
        public FileManager.OperationModeEnum OperationMode
        {
            get
            {
                return ExcellonFileManager.OperationMode;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last DCode X Coordinate value
        /// </summary>
        /// <history>
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
        ///    01 Sep 10  Cynic - Started
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
