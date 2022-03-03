using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
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
    /// A control to graphically display Gerber and G-code files
    /// </summary>
    public partial class ctlPlotViewer : ctlOISBase
    {

        // this determines the view we use to display the plot
        private DisplayModeEnum displayMode = DisplayModeEnum.DisplayMode_GERBERONLY;

        // this is the gerber file we display
        private GerberFile gerberFileToDisplay = new GerberFile();
        // this is the excellon file we display
        private ExcellonFile excellonFileToDisplay = new ExcellonFile();
        // this is the display IsoPlotBuilder it should have been built from the gerberFileToDisplay
        private IsoPlotBuilder isoplotBuilderToDisplay = null;
        // this is the display gcode file it should have been built from the isoplotBuilderToDisplay
        private GCodeFile gcodeFileToDisplay = null;

        // this, if false, disables all plot displays
        public const bool DEFAULT_SHOW_PLOT = true;
        private bool showPlot = DEFAULT_SHOW_PLOT;

        public const bool DEFAULT_SHOW_GERBERONGCODE = false;
        private bool showGerberOnGCode = DEFAULT_SHOW_GERBERONGCODE;

        public const bool DEFAULT_SHOW_ORIGIN = false;
        private bool showOrigin = DEFAULT_SHOW_ORIGIN;

        public const bool DEFAULT_SHOW_GCODE_ORIGIN = false;
        private bool showGCodeOrigin = DEFAULT_SHOW_GCODE_ORIGIN;

        public const bool DEFAULT_SHOW_FLIP_AXIS = false;
        private bool showFlipAxis = DEFAULT_SHOW_FLIP_AXIS;

        public const bool DEFAULT_GCODE_AXIS_IS_IN_CENTER = true;
        private bool gcodeOriginAtCenter = DEFAULT_GCODE_AXIS_IS_IN_CENTER;

        // these are the values we add to the plot origin in order
        // to find the true origin of the plot display. This is the 
        // origin actually in the Gerber or Excellon file
        private float plotXOriginLocation = 0;
        private float plotYOriginLocation = 0;

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        private float minPlotXCoord = 0;
        private float minPlotYCoord = 0;
        private float maxPlotXCoord = 0;
        private float maxPlotYCoord = 0;
        private float midPlotXCoord = 0;
        private float midPlotYCoord = 0;

        private PointF workingOrigin = new PointF(0, 0);
        private const int DEFAULT_PADDING_LEFT = 10;
        private const int DEFAULT_PADDING_TOP = 10;
        private const int DEFAULT_PADDING_RIGHT = 10;
        private const int DEFAULT_PADDING_BOTTOM = 10;
        Padding plotPadding = new Padding(DEFAULT_PADDING_LEFT, DEFAULT_PADDING_TOP, DEFAULT_PADDING_TOP, DEFAULT_PADDING_BOTTOM);
        public const int DEFAULT_PLOT_WIDTH = 1000;
        public const int DEFAULT_PLOT_HEIGHT = 850;

        private const int DEFAULT_PLOT_PADDING_TOP = 20;
        private const int DEFAULT_PLOT_PADDING_RIGHT = 20;

        // this is the size of the virtual GerberPlot
        Size virtualPlotSize = new Size(DEFAULT_PLOT_WIDTH, DEFAULT_PLOT_HEIGHT);
        // this is the size of the virtual GerberPlot including padding
        Size virtualScreenSize = new Size(DEFAULT_PLOT_WIDTH + DEFAULT_PADDING_LEFT + DEFAULT_PADDING_RIGHT, DEFAULT_PLOT_HEIGHT + DEFAULT_PADDING_RIGHT + DEFAULT_PADDING_BOTTOM);

        private float isoPlotPointsPerAppUnit = ApplicationImplicitSettings.DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_IN;
        private ApplicationUnitsEnum screenUnits = ApplicationImplicitSettings.DEFAULT_APPLICATION_UNITS;

        // this bitmap is used to display temp iso plot steps
        Bitmap backgroundBitmap = null;
        // this is the displayMode the background bitmap is appropriate for
        private DisplayModeEnum bitmapMode = DisplayModeEnum.DisplayMode_GERBERONLY;

        // this is the default magnification level we return to whenever we open a new file
        // these are percents values *1.00 is 100%)
        public const float DEFAULT_MAGNIFICATION_LEVEL = 1.00f;
        // this is the zero based index in the DEFAULT_MAGNIFICATION_LEVELS of
        // the DEFAULT_MAGNIFICATION_LEVEL
        public const int DEFAULT_MAGNICATION_LEVEL_INDEX = 6;
        // these are the possible default scale levels, the user can specify values between these manually
        public static float[] DEFAULT_MAGNIFICATION_LEVELS = {0.25f, 0.33f, 0.50f, 0.66f, 0.75f, 0.87f, 1.00f, 1.25f, 1.50f, 2.00f, 3.00f, 4.00f, 5.00f, 6.00f, 7.00f, 8.00f, 10.00f, 12.00f, 16.00f };
        // this is the currently operational level of magnification
        private float magnificationLevel = DEFAULT_MAGNIFICATION_LEVEL;

        // these are the dots per inch on the screen. We get them once in the first paint
        // in order to avoid things that don't need a graphics object having to obtain one
        // these are never used directly since the application units may be MM. Always
        // access these values through DotsPerAppUnitX and DotsPerAppUnitY which does the conversion.
        private bool _dpiHasBeenSet = false;
        private float _dpiX = 96f;
        private float _dpiY = 96f;

        // there are 25.4 mm to the inch
        private const int INCHTOMMSCALERx10 = 254;

        //TODO when using the scroll wheel to scale, adjust the x and  offset so the 
        // pixel under the mouse stays in roughly the same place

        private Point lastMouseDownPosition;
        private PointF workingOriginAtMouseDown = new PointF(0, 0);
        private bool panningActive = false;

        private TextBox mouseCursorDisplayControl = null;
        //private Matrix lastTransformMatrix = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public ctlPlotViewer()
        {
            InitializeComponent();            
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the control we use to display the mouse cursor position
        /// </summary>
        public TextBox MouseCursorDisplayControl
        {
            get
            {
                return mouseCursorDisplayControl;
            }
            set
            {
                mouseCursorDisplayControl = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// An invalidate routine for this control
        /// </summary>
        public new void Invalidate()
        {
            base.Invalidate();
            // invalidate every control we possess
            foreach (Control conObj in this.Controls)
            {
                conObj.Invalidate();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether we show the GCode cut lines when plotting GCodes
        /// </summary>
        public bool ShowGerberOnGCode
        {
            get
            {
                return showGerberOnGCode;
            }
            set
            {
                showGerberOnGCode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether we show the (0,0) origin on the display
        /// </summary>
        public bool ShowOrigin
        {
            get
            {
                return showOrigin;
            }
            set
            {
                showOrigin = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether we show the GCode origin on the display
        /// </summary>
        public bool ShowGCodeOrigin
        {
            get
            {
                return showGCodeOrigin;
            }
            set
            {
                showGCodeOrigin = value;
            }
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether we show the GCode origin in the center of the plot
        /// </summary>
        public bool GcodeOriginAtCenter
        {
            get
            {
                return gcodeOriginAtCenter;
            }
            set
            {
                gcodeOriginAtCenter = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets whether we show the FlipAxis on the display
        /// </summary>
        public bool ShowFlipAxis
        {
            get
            {
                return showFlipAxis;
            }
            set
            {
                showFlipAxis = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the GerberFile to display. Will never set or get a null value.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public GerberFile GerberFileToDisplay
        {
            set
            {
                gerberFileToDisplay = value;
                if (gerberFileToDisplay == null) gerberFileToDisplay = new GerberFile();
            }
            get
            {
                if (gerberFileToDisplay == null) gerberFileToDisplay = new GerberFile();
                return gerberFileToDisplay;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the ExcellonFile to display. Will never set or get a null value.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public ExcellonFile ExcellonFileToDisplay
        {
            set
            {
                excellonFileToDisplay = value;
                if (excellonFileToDisplay == null) excellonFileToDisplay = new ExcellonFile();
            }
            get
            {
                if (excellonFileToDisplay == null) excellonFileToDisplay = new ExcellonFile();
                return excellonFileToDisplay;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isoplotbuilder to display. Can set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public IsoPlotBuilder IsoPlotBuilderToDisplay
        {
            get
            {
                return isoplotBuilderToDisplay;
            }
            set
            {
                isoplotBuilderToDisplay = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the GCodeFile to display. Can set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public GCodeFile GCodeFileToDisplay
        {
            get
            {
                return gcodeFileToDisplay;
            }
            set
            {
                gcodeFileToDisplay = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the background bitmap which represents the interim GCode calculation stages
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public Bitmap BackgroundBitmap
        {
            get
            {
                return backgroundBitmap;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the background bitmap which represents the interim GCode calculation stages
        /// </summary>
        public void SetBackgroundBitmap(Bitmap bitmapIn, DisplayModeEnum displayModeIn)
        {
            backgroundBitmap = bitmapIn;
            bitmapMode = displayModeIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the display mode the current background bitmap is appropriate for
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public DisplayModeEnum BitmapMode
        {
            get
            {
                return bitmapMode;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the which plot of the display we show
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public DisplayModeEnum DisplayMode
        {
            get
            {
                return displayMode;
            }
            set
            {
                displayMode = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the display
        /// </summary>
        public void Reset()
        {
            showPlot = DEFAULT_SHOW_PLOT;
            plotXOriginLocation = 0;
            plotYOriginLocation = 0;
            minPlotXCoord = 0;
            minPlotYCoord = 0;
            maxPlotXCoord = 0;
            maxPlotYCoord = 0;
            workingOrigin = new PointF(0, 0);
            magnificationLevel = DEFAULT_MAGNIFICATION_LEVEL;
            plotPadding = new Padding(DEFAULT_PADDING_LEFT, DEFAULT_PADDING_TOP, DEFAULT_PADDING_RIGHT, DEFAULT_PADDING_BOTTOM);
            virtualPlotSize = new Size(DEFAULT_PLOT_WIDTH, DEFAULT_PLOT_HEIGHT);
            virtualScreenSize = new Size(DEFAULT_PLOT_WIDTH + DEFAULT_PADDING_LEFT + DEFAULT_PADDING_RIGHT, DEFAULT_PLOT_HEIGHT + DEFAULT_PADDING_RIGHT + DEFAULT_PADDING_BOTTOM);
            // we do not have these
            IsoPlotBuilderToDisplay = null;
            GCodeFileToDisplay = null;
            workingOrigin.X = 0;
            workingOrigin.Y = 0;
            Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the virtual plot size. There is no set accessor - this value is 
        /// set when the gerber file is plotted. Never returns a value with a
        /// height or width of zero
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public Size VirtualPlotSize
        {
            get
            {
                if ((virtualPlotSize.Width <= 0) || (virtualPlotSize.Height <= 0))
                {
                    virtualPlotSize = new Size(DEFAULT_PLOT_WIDTH, DEFAULT_PLOT_HEIGHT);
                }
                return virtualPlotSize;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the currently set Screen Units as an enum
        /// </summary>
        [Browsable(false)]
        public ApplicationUnitsEnum ScreenUnits
        {
            get
            {
                return screenUnits;
            }
            set
            {
                screenUnits = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the virtual plot size.  Never gets/sets a value less than
        /// or equal to zero
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        [ReadOnlyAttribute(true)]
        public float IsoPlotPointsPerAppUnit
        {
            get
            {
                if (isoPlotPointsPerAppUnit <= 0)
                {
                    if (ScreenUnits == ApplicationUnitsEnum.INCHES)
                    {
                        isoPlotPointsPerAppUnit = ApplicationImplicitSettings.DEFAULT_VIRTURALCOORD_PER_INCH;
                    }
                    else
                    {
                        isoPlotPointsPerAppUnit = ApplicationImplicitSettings.DEFAULT_VIRTURALCOORD_PER_MM;
                    }
                }
                return isoPlotPointsPerAppUnit;
            }
            set
            {
                isoPlotPointsPerAppUnit = value;
                if (isoPlotPointsPerAppUnit <= 0)
                {
                    if (ScreenUnits == ApplicationUnitsEnum.INCHES)
                    {
                        isoPlotPointsPerAppUnit = ApplicationImplicitSettings.DEFAULT_VIRTURALCOORD_PER_INCH;
                    }
                    else
                    {
                        isoPlotPointsPerAppUnit = ApplicationImplicitSettings.DEFAULT_VIRTURALCOORD_PER_MM;
                    }
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Shows whatever objects we have configured on the plot
        /// </summary>
        public void ShowPlot()
        {
             // a reset is assumed to have been done prior to this call
            SetVirtualPlotSize();
            SetScrollBarMaxMinLimits();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/sets the current magnification level. Will never get/set a value less
        /// than zero.
        /// </summary>
        public float MagnificationLevel
        {
            get
            {
                if (magnificationLevel <= 0) magnificationLevel = DEFAULT_MAGNIFICATION_LEVEL;
                return magnificationLevel;
            }
            set
            {
                magnificationLevel = value;
                if (magnificationLevel <= 0) magnificationLevel = DEFAULT_MAGNIFICATION_LEVEL;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the plotXOriginLocation.  these are the values we add to the 
        /// plot origin in order to find the true origin of the plot display. This 
        /// is the (0,0) origin actually used in the Gerber or Excellon file
        /// </summary>
        public float PlotXOriginLocation
        {
            get
            {
                return plotXOriginLocation;
            }
            set
            {
                plotXOriginLocation = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the plotYOriginLocation.  these are the values we add to the 
        /// plot origin in order to find the true origin of the plot display. This 
        /// is the (0,0) origin actually used in the Gerber or Excellon file
        /// </summary>
        public float PlotYOriginLocation
        {
            get
            {
                return plotYOriginLocation;
            }
            set
            {
                plotYOriginLocation = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Figures out the virtual plot dimensions using the largest sizes we have
        /// and the scaling factor
        /// </summary>
        public void SetVirtualPlotSize()
        {
            // the virtual plot size must be large enough to take the full size of the GerberPlot to display
            // the minPlotXYCoords should be origin compensated to zero. We just use the maxXY here

            float xSize = maxPlotXCoord;
            float ySize = maxPlotYCoord;

            xSize *= (float)isoPlotPointsPerAppUnit;
            ySize *= (float)isoPlotPointsPerAppUnit;

            // add on a bit of extra space so the objects with the biggest X or Y 
            // we just add on the DEFAULT_PLOT_PADDING      
            ySize+= (float)DEFAULT_PLOT_PADDING_TOP;
            xSize+= (float)DEFAULT_PLOT_PADDING_RIGHT;

            if (xSize <= 0) xSize = DEFAULT_PLOT_WIDTH;
            if (ySize <= 0) ySize = DEFAULT_PLOT_HEIGHT;

            virtualPlotSize = new Size((int)xSize, (int)ySize);
            virtualScreenSize = new Size((int)xSize + plotPadding.Left + plotPadding.Right, (int)ySize + plotPadding.Top + plotPadding.Bottom);

            LogMessage("SetVirtualPlotSize: virtualPlotSize: (0,0) " + virtualPlotSize.ToString());
            LogMessage("SetVirtualPlotSize: virtualScreenSize: (0,0) " + virtualScreenSize.ToString());

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the limits of the plot. This is the limits of the plot used by the 
        /// objects on display. The units will be as determined by
        /// SetPlotUnits
        /// </summary>
        /// <param name="maxXCoordIn">maxX</param>
        /// <param name="maxYCoordIn">maxY</param>
        /// <param name="minXCoordIn">minX</param>
        /// <param name="minYCoordIn">minY</param>
        /// <param name="midXCoordIn">minX</param>
        /// <param name="midYCoordIn">minY</param>
        public int SetPlotObjectLimits(float minXCoordIn, float minYCoordIn, float maxXCoordIn, float maxYCoordIn, float midXCoordIn, float midYCoordIn)
        {
            minPlotXCoord = minXCoordIn;
            minPlotYCoord = minYCoordIn;
            maxPlotXCoord = maxXCoordIn;
            maxPlotYCoord = maxYCoordIn;
            midPlotXCoord = midXCoordIn;
            midPlotYCoord = midYCoordIn;

            //DebugMessage("SetPlotObjectLimits minX=" + minPlotXCoord.ToString() + ", maxX=" + maxPlotXCoord.ToString() + ", minY=" + minPlotYCoord.ToString() + ", maxY=" + maxPlotYCoord.ToString() + ", midX=" + midPlotXCoord.ToString() + ", midY=" + midPlotYCoord.ToString());

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a size change event
        /// </summary>
        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            SetScrollBarMaxMinLimits();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a mouse wheel event
        /// </summary>
        public void HandleMouseWheelEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int currIndex = 0;
            // Update the drawing based upon the mouse wheel scrolling.
            int numberOfMagLevels = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            // get our position in the array now
            currIndex = GetCurrentMagLevelsIndexIntoDefaultMagLevelArray();
            // add on where we want to go based on the mouse wheel, we should just add
            // numberOfScaleLevels on to currIndex here but that does not seem to work well
            // so we do it increment by increment
            if (numberOfMagLevels > 0)
            {
                currIndex += 1;
            }
            else if (numberOfMagLevels < 0)
            {
                currIndex -= 1;
            }

            // sanity check
            if (currIndex < 0) currIndex = 0;
            if (currIndex >= DEFAULT_MAGNIFICATION_LEVELS.Count()) currIndex = DEFAULT_MAGNIFICATION_LEVELS.Count() - 1;
            // set the magnification level
            MagnificationLevel = DEFAULT_MAGNIFICATION_LEVELS[currIndex];
            // set the scroll bar
            SetScrollBarMaxMinLimits();
            panel1.Invalidate();

    //        DebugMessage("MouseWheel, delta=" + e.Delta.ToString() + " MagnificationLevel=" + MagnificationLevel.ToString());
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// returns the index of the entry in the DEFAULT_MAGNIFICATION_LEVELS just
        /// equal to greater than the current magnification level
        /// </summary>
        public int GetCurrentMagLevelsIndexIntoDefaultMagLevelArray()
        {
            for (int index = 0; index < DEFAULT_MAGNIFICATION_LEVELS.Count(); index++ )
            {
                if (DEFAULT_MAGNIFICATION_LEVELS[index] >= magnificationLevel) return index;
            }
            // not found - just return the last one
            return DEFAULT_MAGNIFICATION_LEVELS.Count()-1;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Paints the control according to the currently loaded gerber file
        /// </summary>
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // this is the translation and scaling matrix we use to draw on the panel
            Matrix R1 = null;

            // Note that flipX means that we 
            // flip about the center vertical axis such that the Y values remain constant but 
            // the X values are mirrored around the center between the minimum and maximum

            // this is 0 in normal mode or set to a value to shift the X axis offset if we are
            // X flipping
            int flipXCompensator = 0;  
            // normally 1, this gets set to -1 to initiate a flip about the Y axis
            int matrixXFlipInitiator = 1;

            // get the graphics object
            System.Drawing.Graphics graphicsObj = panel1.CreateGraphics();
            
            //DebugMessage("Paint called");

            if(_dpiHasBeenSet == false)
            {
                // set this now
                _dpiX = graphicsObj.DpiX;
                _dpiY = graphicsObj.DpiY;
                _dpiHasBeenSet = true;
            }

            try
            {
                if (showPlot == false)
                {
                    // just clear the screen
                    graphicsObj.Clear(ApplicationColorManager.DEFAULT_PLOT_PANEL_COLOR);
                }
                else
                {
                    // if this is true, we are done, screen is automatically cleared
                    if (DisplayMode == DisplayModeEnum.DisplayMode_NONE) return;


                    if ((GerberFileToDisplay.IsPopulated==true) && (GerberFileToDisplay.FlipMode == IsoFlipModeEnum.X_Flip))
                    {
                        // yes, we do want to flip about the Y axis. We will need to adjust some things on the display
                        flipXCompensator = (int)(gerberFileToDisplay.MaxPlotXCoord * isoPlotPointsPerAppUnit);
                        matrixXFlipInitiator = -1;
                    }
                    else { } // leave everyting at defaults

                    const int INVERSION_COMPENSATOR_OFFSET = 3;
                    // set up the matrix to invert on the Y axis. This
                    // puts the origin 0,0 in the lower left hand corner
                    // the INVERSION_COMPENSATOR_OFFSET is necessary because
                    // the reflection and translation are off slightly. I think
                    // this is due to the borders or something, anyhoo this makes
                    // it come out right
                    R1 = new Matrix(1* matrixXFlipInitiator, 0, 0, -1, 0, 0);
                    R1.Translate(0, panel1.Height - INVERSION_COMPENSATOR_OFFSET, MatrixOrder.Append);
                    // R1.Translate(workingOrigin.X, workingOrigin.Y);
                    R1.Translate(workingOrigin.X * matrixXFlipInitiator, workingOrigin.Y);
                    // now compensate for the left, and top padding
                    R1.Translate(plotPadding.Left, plotPadding.Top);
                    // now compensate for the scaling
                    float xScreenScale = ConvertMagnificationLevelToXScreenScaleFactor(MagnificationLevel);
                    float yScreenScale = ConvertMagnificationLevelToYScreenScaleFactor(MagnificationLevel);
                    R1.Scale(xScreenScale, yScreenScale);
                    // now translate appropriately. Normally this will be 0,0 but if we are flip X axis 
                    // it will have other values. Note this MUST come after the above scaling!!! Note that
                    // it goes in negative. The matrix math seems to require this
                    R1.Translate(flipXCompensator * matrixXFlipInitiator, 0);

                    //DebugMessage("workingOrigin=" + workingOrigin.ToString());


                   // DebugMessage("");
                   // DebugMessage("MagnificationLevel=" + MagnificationLevel.ToString());
                   // DebugMessage("xScreenScale=" + xScreenScale.ToString());
                   // DebugMessage("xScreenScale*virtualScreenSize.Width=" + (xScreenScale * virtualScreenSize.Width).ToString());
                   // DebugMessage("yScreenScale=" + yScreenScale.ToString());
                   // DebugMessage("yScreenScale*virtualScreenSize.Height=" + (yScreenScale * virtualScreenSize.Height).ToString());
                   // DebugMessage("");
                    // apply it to the graphics object. This means the rest of the code
                    // does not need to know about it
                    graphicsObj.Transform = R1;
                    // draw the background and the border
                    DrawBackground(graphicsObj, ApplicationColorManager.DEFAULT_PLOT_BACKGROUND_BRUSH);
                    //DebugTODO("make the border and corners options");
                    //DrawBorder(graphicsObj, ApplicationColorManager.DEFAULT_PLOT_BORDER_PEN);
                    //DrawDiagnosticCornerBoxes(graphicsObj);

                    if (DisplayMode == DisplayModeEnum.DisplayMode_GERBERONLY)
                    {
                        // Draw the Gerber File
                        if ((GerberFileToDisplay != null) && (GerberFileToDisplay.IsPopulated == true))
                        {
                            GerberFileToDisplay.PlotGerberFile(graphicsObj);
                        }
                        else if ((ExcellonFileToDisplay != null) && (ExcellonFileToDisplay.IsPopulated == true))
                        {
                            ExcellonFileToDisplay.PlotExcellonFile(graphicsObj);
                        }
                    }
                    else if (DisplayMode == DisplayModeEnum.DisplayMode_ISOSTEP1)
                    {
                        // the bitmap will have been set up differently
                        if (backgroundBitmap != null) graphicsObj.DrawImage(backgroundBitmap, 0, 0);
                        // do we want to show the gerber plot anyways?
                        if (ShowGerberOnGCode == true)
                        {
                            // Draw the Gerber File
                            if ((GerberFileToDisplay != null) && (GerberFileToDisplay.IsPopulated == true))
                            {
                                GerberFileToDisplay.PlotGerberFile(graphicsObj);
                            }
                        }
                    }
                    else if (DisplayMode == DisplayModeEnum.DisplayMode_ISOSTEP2)
                    {
                        if (backgroundBitmap != null) graphicsObj.DrawImage(backgroundBitmap, 0, 0);
                        // do we want to show the gerber plot anyways?
                        if (ShowGerberOnGCode == true)
                        {
                            // Draw the Gerber File
                            if ((GerberFileToDisplay != null) && (GerberFileToDisplay.IsPopulated == true))
                            {
                                GerberFileToDisplay.PlotGerberFile(graphicsObj);
                            }
                        }
                    }
                    else if (DisplayMode == DisplayModeEnum.DisplayMode_ISOSTEP3)
                    {
                        if (backgroundBitmap != null) graphicsObj.DrawImage(backgroundBitmap, 0, 0);
                        // do we want to show the gerber plot anyways?
                        if (ShowGerberOnGCode == true)
                        {
                            // Draw the Gerber File
                            if ((GerberFileToDisplay != null) && (GerberFileToDisplay.IsPopulated == true))
                            {
                                GerberFileToDisplay.PlotGerberFile(graphicsObj);
                            }
                        }
                    }
                    else if (DisplayMode == DisplayModeEnum.DisplayMode_GCODEONLY)
                    {
                        // show the GCode File
                        if (GCodeFileToDisplay != null)
                        {
                            GCodeFileToDisplay.PlotGCodeFile(graphicsObj, false);
                        }
                        // do we want to show the gerber plot anyways?
                        if (ShowGerberOnGCode == true)
                        {
                            // Draw the Gerber File
                            if ((GerberFileToDisplay != null) && (GerberFileToDisplay.IsPopulated == true))
                            {
                                GerberFileToDisplay.PlotGerberFile(graphicsObj);
                            }
                            else if ((ExcellonFileToDisplay != null) && (ExcellonFileToDisplay.IsPopulated == true))
                            {
                                ExcellonFileToDisplay.PlotExcellonFile(graphicsObj);
                            }
                        }
                    }
                    // draw the Flip Axis origin, if enabled
                    DrawFlipAxis(graphicsObj, ApplicationColorManager.DEFAULT_PLOT_FLIPAXIS_PEN);
                    // draw the GCode origin, if enabled
                    DrawGCodeOrigin(graphicsObj, ApplicationColorManager.DEFAULT_PLOT_GCODE_ORIGIN_PEN);
                    // draw the origin, if enabled, this always goes over top the GCode origin
                    DrawOrigin(graphicsObj, ApplicationColorManager.DEFAULT_PLOT_ORIGIN_PEN);

                    // actually this returns a clone it will need to be disposed
                    //Matrix R2 = graphicsObj.Transform;
                    //R2.Invert();
                    //lastTransformMatrix = R2;
                }
            }
            finally
            {
                if (graphicsObj != null) graphicsObj.Dispose();
            } 
        }

        private bool rightButtonDown = false;
        private bool leftButtonDown = false;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// This handles a mouse down event for the panel
        /// </summary>
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            // test the buttons, we only care about left and right at the moment
            if (e.Button == MouseButtons.Left) leftButtonDown = true;
            else if (e.Button == MouseButtons.Right) rightButtonDown = true;
            else return;

            // we require both the left and right buttons to be down
            // at the same time in order to pan
            if (leftButtonDown == false || rightButtonDown == false) return;

            // enable panning
            panningActive = true;
            lastMouseDownPosition.X = e.X;
            lastMouseDownPosition.Y = e.Y;
            workingOriginAtMouseDown = workingOrigin;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// This handles a mouse up event for the panel
        /// </summary>
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                leftButtonDown = false;
            }
            if (e.Button == MouseButtons.Right)
            {
                rightButtonDown = false;
            }
            // any upclick on any button turns off panning
            panningActive = false;

        }

        Point MouseToWorld(Point location)
        {
            return new Point(0, 0);
            // this was an attempt to have the actual gerber/gcode coords display in the 
            // bottom right of the main form. Never could get it to work and so backed it out
            //if(lastTransformMatrix!=null)
            //{
            //    Point[] ptArray = new Point[3];
            //    ptArray[0] = location;
            //    //ptArray[0] = new Point(location.X * (int)IsoPlotPointsPerAppUnit, location.Y* (int)IsoPlotPointsPerAppUnit);
            //    ptArray[1] = new Point(0, 0);
            //    ptArray[2] = new Point(100,100);
            //    lastTransformMatrix.TransformPoints(ptArray);

            //    //return new Point(ptArray[1].X, ptArray[1].Y);


            //    total crap in here
            //    // the zero in here just re-inforces that we are offsetting from the (0,0) plot position
            //    //double xOrigin = (ptArray[0].X * IsoPlotPointsPerAppUnit) - ptArray[1].X;
            //    //double yOrigin = (ptArray[0].Y * IsoPlotPointsPerAppUnit) - ptArray[1].Y;
            //    //double xOrigin = (ptArray[0].X ) - ptArray[1].X;
            //    // double yOrigin = (ptArray[0].Y ) - ptArray[1].Y;

            //    DebugMessage(ptArray[0].X.ToString() + " " + ptArray[1].X + " " + ptArray[2].X);
            //    int real_X = ptArray[0].X - ptArray[1].X;
            //    int scale_X = ptArray[2].X - ptArray[1].X;
            //    int xOrigin = (real_X * 100) / scale_X;
            //    int yOrigin = ptArray[0].Y;

            //    return new Point((int)xOrigin, (int)yOrigin);
            //}
            //else return new Point(location.X, location.Y);

            ////return new Point((int)((float)(location.X) / pageScale - xformPoint.X + 0.5F),
            ////        (int)((float)(location.Y) / pageScale - xformPoint.Y + 0.5F));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// This handles a mouse move event for the panel
        /// </summary>
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //if(mouseCursorDisplayControl != null)
            //{
            //    Point convertedPoint = MouseToWorld(Cursor.Position);

            //    mouseCursorDisplayControl.Text = string.Format("X: {0} , Y: {1}", convertedPoint.X, convertedPoint.Y);
            //}

            PointF tmpOrigin = new PointF();
            if (panningActive == true)
            {
                // record what we started with
                PointF tempOrigin = workingOrigin;

                // ####
                // #### X axis panning
                // ####
                // is it possible to pan in the X direction?
                if (hScrollBar1.Maximum > hScrollBar1.LargeChange)
                {
                    // yes, it is possible to pan in the x direction
                    try
                    {
                        tmpOrigin.X = workingOriginAtMouseDown.X - (lastMouseDownPosition.X - e.X);
                      //  DebugMessage("tmpOrigin.X=" + tmpOrigin.X.ToString() + " workingOrigin.X=" + workingOrigin.X.ToString() + " Value=" + hScrollBar1.Value.ToString() + " hScrollBar1.Minimum=" + hScrollBar1.Minimum.ToString() + " hScrollBar1.Maximum=" + hScrollBar1.Maximum.ToString() + " hScrollBar1.LargeChange=" + hScrollBar1.LargeChange.ToString());

                        if (tmpOrigin.X < ((hScrollBar1.Maximum - hScrollBar1.LargeChange) * -1))
                        {
                            workingOrigin.X = (float)((hScrollBar1.Maximum - hScrollBar1.LargeChange) * -1);
                            //DebugMessage("a" + workingOrigin.X.ToString());
                            hScrollBar1.Value = hScrollBar1.Maximum;
                        }
                        else if (tmpOrigin.X > hScrollBar1.Minimum)
                        {
                            workingOrigin.X = (float)(hScrollBar1.Minimum * -1);
                            //DebugMessage("b" + workingOrigin.X.ToString());
                            hScrollBar1.Value = hScrollBar1.Minimum;
                        }
                        else
                        {
                            // DebugMessage("c" + workingOrigin.X.ToString());
                            workingOrigin.X = tmpOrigin.X;
                            hScrollBar1.Value = (int)(tmpOrigin.X * -1);
                        }
                    }
                    catch
                    {
                        // do not throw exceptions here
                    }
                } // bottom of if (hScrollBar1.Maximum > hScrollBar1.LargeChange)

                // ####
                // #### Y axis panning
                // ####
                // is it possible to pan in the Y direction?
                if (vScrollBar1.Maximum > vScrollBar1.LargeChange)
                {
                    // yes, it is possible to pan in the Y direction
                    try
                    {
                        tmpOrigin.Y = workingOriginAtMouseDown.Y + (lastMouseDownPosition.Y - e.Y);
                      //    DebugMessage("tmpOrigin.Y=" + tmpOrigin.Y.ToString() + " workingOrigin.Y=" + workingOrigin.Y.ToString() + " Value=" + vScrollBar1.Value.ToString() + " vScrollBar1.Minimum=" + vScrollBar1.Minimum.ToString() + " vScrollBar1.Maximum=" + vScrollBar1.Maximum.ToString() + " vScrollBar1.LargeChange=" + vScrollBar1.LargeChange.ToString());

                        if (tmpOrigin.Y < ((vScrollBar1.Maximum - vScrollBar1.LargeChange) * -1))
                        {
                        //    DebugMessage("a " + tmpOrigin.Y.ToString());
                            workingOrigin.Y = (float)((vScrollBar1.Maximum - vScrollBar1.LargeChange) * -1);
                            vScrollBar1.Value = vScrollBar1.Minimum;
                        }
                        else if (tmpOrigin.Y > vScrollBar1.Minimum)
                        {
                         //   DebugMessage("b " + tmpOrigin.Y.ToString());
                            workingOrigin.Y = (float)(vScrollBar1.Minimum * -1);
                            vScrollBar1.Value = (vScrollBar1.Maximum - vScrollBar1.LargeChange);
                        }
                        else
                        {
                         //   DebugMessage("c " + tmpOrigin.Y.ToString());
                            workingOrigin.Y = tmpOrigin.Y;
                            vScrollBar1.Value = (int)((vScrollBar1.Maximum - vScrollBar1.LargeChange)+(tmpOrigin.Y));
                        }
                    }
                    catch
                    {
                        // do not throw exceptions here
                    }
                } // bottom of if (vScrollBar1.Maximum > vScrollBar1.LargeChange)

                // did we actually change the origin? do not redraw the screen if it did not change
                // this saves on flashing
                if(((tempOrigin.X == workingOrigin.X) && (tempOrigin.Y == workingOrigin.Y))==false)
                {
                    // we did change invalidate the screen
                    panel1.Invalidate();
                    hScrollBar1.Invalidate();
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// This returns the number of dots per app unit on the screen. If the 
        /// app units are inches this is the screen resolution in dpi. Otherwise
        /// this is the dpmm
        /// </summary>
        private float DotsPerAppUnitX
        {
            get
            {
                if (ScreenUnits == ApplicationUnitsEnum.INCHES)
                {
                    return _dpiX;
                }
                else
                {
                    return ((_dpiX * 10) / INCHTOMMSCALERx10);
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// This returns the number of dots per app unit on the screen. If the 
        /// app units are inches this is the screen resolution in dpi. Otherwise
        /// this is the dpmm
        /// </summary>
        private float DotsPerAppUnitY
        {
            get
            {
                if (ScreenUnits == ApplicationUnitsEnum.INCHES)
                {
                    return _dpiY;
                }
                else
                {
                    return ((_dpiY * 10) / INCHTOMMSCALERx10);
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We have to take the DPI of the screen into account when presenting
        /// the user with a particular magnification level. This does that
        /// </summary>
        ///                       also made it support mm app units
        private float ConvertMagnificationLevelToXScreenScaleFactor(float magLevel)
        {
            if (magLevel >= 1)
            {
                return (DotsPerAppUnitX * magLevel) / isoPlotPointsPerAppUnit;
            }
            else
            {
                return (DotsPerAppUnitX / isoPlotPointsPerAppUnit) * magLevel;
            }

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We have to take the DPI of the screen into account when presenting
        /// the user with a particular magnification level. This does that
        /// </summary>
        ///                       also made it support mm app units
        private float ConvertMagnificationLevelToYScreenScaleFactor(float magLevel)
        {
            if (magLevel >= 1)
            {
                return (DotsPerAppUnitY * magLevel) / isoPlotPointsPerAppUnit;
            }
            else
            {
                return (DotsPerAppUnitY / isoPlotPointsPerAppUnit) * magLevel;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a scroll event on the horizontal scroll bar
        /// </summary>
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            // the algorythm in the SetScrollBar values makes this 
            // work out really simply
            workingOrigin.X = e.NewValue*-1;
         //   DebugMessage("workingOrigin.X=" + workingOrigin.X.ToString() + " Value=" + e.NewValue.ToString() + " hScrollBar1.Maximum=" + hScrollBar1.Maximum.ToString() + " hScrollBar1.LargeChange=" + hScrollBar1.LargeChange.ToString() + " value+LargeChange+e.NewValue=" + (hScrollBar1.LargeChange + e.NewValue).ToString());
            panel1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a scroll event on the vertical scroll bar
        /// </summary>
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            // the algorythm in the SetScrollBar values makes this 
            // work out really simply
            workingOrigin.Y = e.NewValue - (vScrollBar1.Maximum - vScrollBar1.LargeChange);
          //  DebugMessage("workingOrigin.Y=" + workingOrigin.Y.ToString() + " Value=" + e.NewValue.ToString() + " vScrollBar1.Maximum=" + vScrollBar1.Maximum.ToString() + " vScrollBar1.LargeChange=" + vScrollBar1.LargeChange.ToString() + " value+LargeChange=" + (vScrollBar1.LargeChange + e.NewValue).ToString());
            panel1.Invalidate();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the scroll bar slider positions to the current origin
        /// </summary>
        public void SyncCurrentOriginToScrollBarSliderPositions()
        {
            try
            {
                if (hScrollBar1.LargeChange > hScrollBar1.Maximum)
                {
                    // our virtual screen does not occupy the full screen
                    // the best centering offset is in the tag
                    int tmpOffset = (int)hScrollBar1.Tag;
                    if(tmpOffset>=0) workingOrigin.X = (int)hScrollBar1.Tag;
                    else workingOrigin.X = 0;
                }
                else
                {
                    // we can use this to scroll
                    workingOrigin.X = hScrollBar1.Value * -1;
                }
/*
                DebugMessage("");
                DebugMessage("SyncCurrentOriginToScrollBarSliderPositions");
                DebugMessage("workingOrigin.X=" + workingOrigin.X.ToString());
                DebugMessage("hScrollBar1.Value=" + hScrollBar1.Value.ToString());
                DebugMessage("hScrollBar1.Tag=" + ((int)(hScrollBar1.Tag)).ToString());
                DebugMessage("");
 */
            }
            catch { }

            try
            {
                if (vScrollBar1.LargeChange > vScrollBar1.Maximum)
                {
                    // our virtual screen does not occupy the full screen
                    // the best centering offset is in the tag
                    int tmpOffset = (int)vScrollBar1.Tag;
                    if (tmpOffset >= 0) workingOrigin.Y = (int)vScrollBar1.Tag;
                    else workingOrigin.Y = 0;
                }
                else
                {
                    // we can use this to scroll
                    workingOrigin.Y = vScrollBar1.Value - (vScrollBar1.Maximum - vScrollBar1.LargeChange);
                }
/*
                DebugMessage("");
                DebugMessage("SyncCurrentOriginToScrollBarSliderPositions");
                DebugMessage("workingOrigin.Y=" + workingOrigin.Y.ToString());
                DebugMessage("vScrollBar1.Value=" + vScrollBar1.Value.ToString());
                DebugMessage("vScrollBar1.Tag=" + ((int)(vScrollBar1.Tag)).ToString());
                DebugMessage("");
 */
            }
            catch { }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Set the scroll bar values, code derived from sample at
        /// http://msdn.microsoft.com/en-us/library/system.windows.forms.scrollbar.maximum.aspx
        /// </summary>
        public void SetScrollBarMaxMinLimits()
        {
            int workingSmallChange = 0;
            int workingLargeChange = 0;

            //SmallChange and LargeChange: Per UI guidelines, these must be set
            //    relative to the size of the view that the user sees, not to
            //    the total size including the unseen part.  In this example,
            //    these must be set relative to the panel box, not to the image.

            //Configure the horizontal scrollbar
            if (this.hScrollBar1.Visible)
            {
                // calc the number of pixels of our virtual plot would use
                // on the screen if it were possible to make it fully visible
                int virtualScreenWidthInScreenPixels = (int)((((float)this.virtualScreenSize.Width) * DotsPerAppUnitX * MagnificationLevel) / isoPlotPointsPerAppUnit);
                
                // set these. The Large Change will be the width of our scroll thumb
                workingSmallChange = virtualScreenWidthInScreenPixels / 20;
                workingLargeChange = virtualScreenWidthInScreenPixels / 10;

/*
                DebugMessage("");
                DebugMessage("SetScrollBarMaxMinLimits()");
                DebugMessage("xvirtualScreenSize.Width=" + virtualScreenSize.Width.ToString());
                DebugMessage("xdpiX=" + dpiX.ToString());
                DebugMessage("xMagnificationLevel=" + MagnificationLevel.ToString());
                DebugMessage("xisoPlotPointsPerAppUnit=" + isoPlotPointsPerAppUnit.ToString());
                DebugMessage("xworkingLargeChange=" + workingLargeChange.ToString());
                DebugMessage("xpanel1.ClientSize.Width=" + panel1.ClientSize.Width.ToString());
                DebugMessage("xvScrollBar1.Width=" + vScrollBar1.Width.ToString());
*/
                // calculate the working scrollbar maximum
                int workingMaximum = virtualScreenWidthInScreenPixels + workingLargeChange - (panel1.ClientSize.Width - vScrollBar1.Width);
//                DebugMessage("workingMaximum=" + workingMaximum.ToString());
                // is our current virtualScreenWidthInScreenPixels less than the 
                // actual panel+compensation factors? It is if the working maximum is less than zero
                if (workingMaximum <= workingLargeChange)
                {
//                    DebugMessage("noscroll mode");
                    // the virtualScreenWidthInScreenPixels is less than the screen size, we set defaults
                    // the call to SyncCurrentOriginToScrollBarSliderPositions() should notice that
                    // LargeChange>Maximum and adjust the offset to center the virtual screen
                    hScrollBar1.Minimum = 0;
                    hScrollBar1.Maximum = 100;
                    this.hScrollBar1.SmallChange = 100;
                    this.hScrollBar1.LargeChange = hScrollBar1.Maximum+1;
                    hScrollBar1.Value = 0;
                    hScrollBar1.Tag = (panel1.ClientSize.Width - vScrollBar1.Width - virtualScreenWidthInScreenPixels)/2;
                }
                else
                {
    //                DebugMessage("scroll mode");
                    hScrollBar1.Minimum = 0;
                    hScrollBar1.Maximum = workingMaximum;
                    hScrollBar1.SmallChange = workingSmallChange;
                    hScrollBar1.LargeChange = workingLargeChange;
                    //TODO set the value now
                }
/*                
                DebugMessage(" virtualScreenWidthInScreenPixels=" + virtualScreenWidthInScreenPixels.ToString());
                DebugMessage("vScrollBar1.Width=" + vScrollBar1.Width.ToString());
                DebugMessage("MagnificationLevel=" + MagnificationLevel.ToString());
                DebugMessage("panel1.ClientSize.Width=" + panel1.ClientSize.Width.ToString());
                DebugMessage("virtualScreenSize.Width=" + virtualScreenSize.Width.ToString());
                DebugMessage("hScrollBar1.SmallChange=" + hScrollBar1.SmallChange.ToString());
                DebugMessage("hScrollBar1.LargeChange=" + hScrollBar1.LargeChange.ToString());
                DebugMessage("hScrollBar1.Maximum=" + hScrollBar1.Maximum.ToString());
                DebugMessage("");
 */
            }

            //Configure the vertical scrollbar
            if (this.vScrollBar1.Visible)
            {
                // calc the number of pixels of our virtual plot would use
                // on the screen if it were possible to make it fully visible
                int virtualScreenHeightInScreenPixels = (int)((((float)this.virtualScreenSize.Height) * DotsPerAppUnitY * MagnificationLevel) / isoPlotPointsPerAppUnit);

                // set these. The Large Change will be the width of our scroll thumb
                workingSmallChange = virtualScreenHeightInScreenPixels / 20;
                workingLargeChange = virtualScreenHeightInScreenPixels / 10;
                /*
                DebugMessage("");
                DebugMessage("SetScrollBarMaxMinLimits()");
                DebugMessage("virtualScreenSize.Height=" + virtualScreenSize.Height.ToString());
                DebugMessage("workingSmallChange=" + workingSmallChange.ToString());
                DebugMessage("workingLargeChange=" + workingLargeChange.ToString());
                 */

                // calculate the working scrollbar maximum
                int workingMaximum = virtualScreenHeightInScreenPixels + workingLargeChange - (panel1.ClientSize.Height - hScrollBar1.Height);
                //DebugMessage("workingMaximum=" + workingMaximum.ToString());
                // is our current virtualScreenHeightInScreenPixels less than the 
                // actual panel+compensation factors? It is if the working maximum is less than zero
                if (workingMaximum <= workingLargeChange)
                {
                   // DebugMessage("noscroll mode");
                    // the virtualScreenHeightInScreenPixels is less than the screen size, we set defaults
                    // the call to SyncCurrentOriginToScrollBarSliderPositions() should notice that
                    // LargeChange>Maximum and adjust the offset to center the virtual screen
                    vScrollBar1.Minimum = 0;
                    vScrollBar1.Maximum = 100;
                    this.vScrollBar1.SmallChange = 100;
                    this.vScrollBar1.LargeChange = vScrollBar1.Maximum + 1;
                    vScrollBar1.Value = 0;
                    vScrollBar1.Tag = (panel1.ClientSize.Height - hScrollBar1.Height - virtualScreenHeightInScreenPixels) / 2;
                }
                else
                {
                    //DebugMessage("scroll mode");
                    vScrollBar1.Minimum = 0;
                    vScrollBar1.Maximum = workingMaximum;
                    vScrollBar1.SmallChange = workingSmallChange;
                    vScrollBar1.LargeChange = workingLargeChange;
                    //TODO set the value now
                }
                /*
                DebugMessage(" virtualScreenHeightInScreenPixels=" + virtualScreenHeightInScreenPixels.ToString());
                DebugMessage("hScrollBar1.Height=" + hScrollBar1.Height.ToString());
                DebugMessage("MagnificationLevel=" + MagnificationLevel.ToString());
                DebugMessage("panel1.ClientSize.Height=" + panel1.ClientSize.Height.ToString());
                DebugMessage("virtualScreenSize.Height=" + virtualScreenSize.Height.ToString());
                DebugMessage("vScrollBar1.SmallChange=" + vScrollBar1.SmallChange.ToString());
                DebugMessage("vScrollBar1.LargeChange=" + vScrollBar1.LargeChange.ToString());
                DebugMessage("vScrollBar1.Maximum=" + vScrollBar1.Maximum.ToString());
                DebugMessage("");
                 */
            }

            SyncCurrentOriginToScrollBarSliderPositions();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws the flip axis line - the line we flip around
        /// </summary>
        /// <param name="graphicsObj"> a valid graphics object</param>
        /// <param name="borderPen">a 1 pixel wide pen to draw the border with</param>
        private void DrawFlipAxis(Graphics graphicsObj, Pen originPen)
        {
            float flipAxisLen = (float)(maxPlotYCoord * IsoPlotPointsPerAppUnit);

            if (ShowFlipAxis == false) return;
            if (graphicsObj == null) return;
            if (originPen == null) return;

            //float xScreenScale = ConvertMagnificationLevelToXScreenScaleFactor(MagnificationLevel);
            //float yScreenScale = ConvertMagnificationLevelToYScreenScaleFactor(MagnificationLevel);
            int yLineLen = (int)(flipAxisLen);

            //  DebugMessage("xScreenScale = " + xScreenScale.ToString() + " xLineLen=" + xLineLen.ToString() + " MagnificationLevel=" + MagnificationLevel.ToString());

            // the zero in here just re-inforces that we are offsetting from the (0,0) plot position
            double xOrigin = 0 + Math.Round((this.midPlotXCoord * IsoPlotPointsPerAppUnit)); ;
           // double yOrigin = 0 + Math.Round((plotYOriginLocation * IsoPlotPointsPerAppUnit)); ;

            // DebugMessage("xOrigin = " + xOrigin.ToString() + " xScreenScale=" + xScreenScale.ToString());

            Point startPointY = new Point((int)xOrigin, 0);
            Point endPointY = new Point((int)xOrigin, yLineLen);

            // draw the flip axis line
            graphicsObj.DrawLine(originPen, startPointY, endPointY);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws the (0,0) position (the origin)
        /// </summary>
        /// <param name="graphicsObj"> a valid graphics object</param>
        /// <param name="borderPen">a 1 pixel wide pen to draw the border with</param>
        private void DrawOrigin(Graphics graphicsObj, Pen originPen)
        {
            const float ORIGIN_CROSSHAIR_LEN = 10;

            if (ShowOrigin == false) return;
            if (graphicsObj == null) return;
            if (originPen == null) return;

            float xScreenScale = ConvertMagnificationLevelToXScreenScaleFactor(MagnificationLevel);
            float yScreenScale = ConvertMagnificationLevelToYScreenScaleFactor(MagnificationLevel);
            int xLineLen = (int)(ORIGIN_CROSSHAIR_LEN / xScreenScale);
            int yLineLen = (int)(ORIGIN_CROSSHAIR_LEN / yScreenScale);

            //  DebugMessage("xScreenScale = " + xScreenScale.ToString() + " xLineLen=" + xLineLen.ToString() + " MagnificationLevel=" + MagnificationLevel.ToString());

            // the zero in here just re-inforces that we are offsetting from the (0,0) plot position
            double xOrigin = 0 + Math.Round((plotXOriginLocation * IsoPlotPointsPerAppUnit)); ;
            double yOrigin = 0 + Math.Round((plotYOriginLocation * IsoPlotPointsPerAppUnit)); ;

         //   DebugMessage("xOrigin = " + xOrigin.ToString() + " xScreenScale=" + xScreenScale.ToString());

            Point startPointX = new Point(((int)xOrigin)+(xLineLen * -1), (int)yOrigin);
            Point endPointX = new Point(((int)xOrigin)+xLineLen, (int)yOrigin);
            Point startPointY = new Point((int)xOrigin, ((int)yOrigin)+(yLineLen * -1));
            Point endPointY = new Point((int)xOrigin, ((int)yOrigin)+yLineLen);

            // draw the cross hair lines
            graphicsObj.DrawLine(originPen, startPointX, endPointX);
            graphicsObj.DrawLine(originPen, startPointY, endPointY);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws the GCode start position (the origin), can be (0,0) or it can be
        /// elsewhere
        /// </summary>
        /// <param name="graphicsObj"> a valid graphics object</param>
        /// <param name="borderPen">a 1 pixel wide pen to draw the border with</param>
        private void DrawGCodeOrigin(Graphics graphicsObj, Pen originPen)
        {
            const float ORIGIN_CROSSHAIR_LEN = 20;
            const float ORIGIN_CIRCLE_RADIUS = 10;
            double xOrigin = 0;
            double yOrigin = 0;

            if (ShowGCodeOrigin == false) return;
            if (graphicsObj == null) return;
            if (originPen == null) return;

            float xScreenScale = ConvertMagnificationLevelToXScreenScaleFactor(MagnificationLevel);
            float yScreenScale = ConvertMagnificationLevelToYScreenScaleFactor(MagnificationLevel);
            int xLineLen = (int)(ORIGIN_CROSSHAIR_LEN / xScreenScale);
            int yLineLen = (int)(ORIGIN_CROSSHAIR_LEN / yScreenScale);
            int circleRadiusX = (int)(ORIGIN_CIRCLE_RADIUS / xScreenScale);
            int circleRadiusY = (int)(ORIGIN_CIRCLE_RADIUS / yScreenScale);

            //  DebugMessage("xScreenScale = " + xScreenScale.ToString() + " xLineLen=" + xLineLen.ToString() + " MagnificationLevel=" + MagnificationLevel.ToString());

            if (gcodeOriginAtCenter == true)
            {
                float xCenterOffset = (float)(this.midPlotXCoord * IsoPlotPointsPerAppUnit);
                float yCenterOffset = (float)(this.midPlotYCoord * IsoPlotPointsPerAppUnit);

                xOrigin = xCenterOffset + Math.Round((plotXOriginLocation * IsoPlotPointsPerAppUnit)); ;
                yOrigin = yCenterOffset + Math.Round((plotYOriginLocation * IsoPlotPointsPerAppUnit)); ;
            }
            else
            {
                // the zero in here just re-inforces that we are offsetting from the (0,0) plot position
                xOrigin = 0 + Math.Round((plotXOriginLocation * IsoPlotPointsPerAppUnit)); ;
                yOrigin = 0 + Math.Round((plotYOriginLocation * IsoPlotPointsPerAppUnit)); ;
            }

            // DebugMessage("xOrigin = " + xOrigin.ToString() + " xScreenScale=" + xScreenScale.ToString());

            Point startPointX = new Point(((int)xOrigin) + (xLineLen * -1), (int)yOrigin);
            Point endPointX = new Point(((int)xOrigin) + xLineLen, (int)yOrigin);
            Point startPointY = new Point((int)xOrigin, ((int)yOrigin) + (yLineLen * -1));
            Point endPointY = new Point((int)xOrigin, ((int)yOrigin) + yLineLen);

            // draw the cross hair lines
            graphicsObj.DrawLine(originPen, startPointX, endPointX);
            graphicsObj.DrawLine(originPen, startPointY, endPointY);
            // draw the circle, feed it the upper xy corner and the height/width
            graphicsObj.DrawEllipse(originPen, ((int)xOrigin) + (circleRadiusX * -1), ((int)yOrigin) + (circleRadiusY * -1), (circleRadiusX * 2), (circleRadiusY * 2));

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draw some diagnostic boxes in the corner of the virtualPlot. We use
        /// these for testing the scroll etc
        /// </summary>
        /// <param name="graphicsObj"> a valid graphics object</param>
        private void DrawDiagnosticCornerBoxes(Graphics graphicsObj)
        {
            const int TEST_LINELEN = 10;

            if (graphicsObj == null) return;

            Pen myPen = new Pen(System.Drawing.Color.Red, 1);
            myPen.Alignment = PenAlignment.Center;


            // box at minx, miny
            graphicsObj.DrawLine(myPen, 0, 0, TEST_LINELEN, 0);
            graphicsObj.DrawLine(myPen, 0, 2, TEST_LINELEN, 2);
            graphicsObj.DrawLine(myPen, 0, 4, TEST_LINELEN, 4);
            graphicsObj.DrawLine(myPen, 0, 6, TEST_LINELEN, 6);
            graphicsObj.DrawLine(myPen, 0, 8, TEST_LINELEN, 8);
            graphicsObj.DrawLine(myPen, 0, 10, TEST_LINELEN, 10);
            graphicsObj.DrawLine(myPen, 0, 0, 0, TEST_LINELEN);

            // box at maxx, miny
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN, 0, virtualPlotSize.Width, 0);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN, 2, virtualPlotSize.Width, 2);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN, 4, virtualPlotSize.Width, 4);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN, 6, virtualPlotSize.Width, 6);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN, 8, virtualPlotSize.Width, 8);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN, 10, virtualPlotSize.Width, 10);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width, 0, virtualPlotSize.Width, TEST_LINELEN);

            // box at minx, maxy
            graphicsObj.DrawLine(myPen, 0, virtualPlotSize.Height - TEST_LINELEN, 0, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, 2, virtualPlotSize.Height - TEST_LINELEN, 2, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, 4, virtualPlotSize.Height - TEST_LINELEN, 4, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, 6, virtualPlotSize.Height - TEST_LINELEN, 6, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, 8, virtualPlotSize.Height - TEST_LINELEN, 8, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, 10, virtualPlotSize.Height - TEST_LINELEN, 10, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, 0, virtualPlotSize.Height, TEST_LINELEN, virtualPlotSize.Height);

            // box at maxx, maxy
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN + 0, virtualPlotSize.Height - TEST_LINELEN, virtualPlotSize.Width - TEST_LINELEN + 0, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN + 2, virtualPlotSize.Height - TEST_LINELEN, virtualPlotSize.Width - TEST_LINELEN + 2, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN + 4, virtualPlotSize.Height - TEST_LINELEN, virtualPlotSize.Width - TEST_LINELEN + 4, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN + 6, virtualPlotSize.Height - TEST_LINELEN, virtualPlotSize.Width - TEST_LINELEN + 6, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN + 8, virtualPlotSize.Height - TEST_LINELEN, virtualPlotSize.Width - TEST_LINELEN + 8, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN + 10, virtualPlotSize.Height - TEST_LINELEN, virtualPlotSize.Width - TEST_LINELEN + 10, virtualPlotSize.Height);
            graphicsObj.DrawLine(myPen, virtualPlotSize.Width - TEST_LINELEN + 0, virtualPlotSize.Height, virtualPlotSize.Width - TEST_LINELEN + TEST_LINELEN, virtualPlotSize.Height);

            myPen.Dispose();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draw a border around the plot
        /// </summary>
        /// <param name="graphicsObj"> a valid graphics object</param>
        /// <param name="borderPen">a 1 pixel wide pen to draw the border with</param>
        private void DrawBorder(Graphics graphicsObj, Pen borderPen)
        {
            if (graphicsObj == null) return;
            if (borderPen == null) return;

            // box 
            graphicsObj.DrawRectangle(borderPen, 0, 0, virtualPlotSize.Width, virtualPlotSize.Height);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draw the virtualPlot in a different colour
        /// </summary>
        /// <param name="graphicsObj"> a valid graphics object</param>
        private void DrawBackground(Graphics graphicsObj, Brush backgroundBrush)
        {
            if (graphicsObj == null) return;
            if (backgroundBrush == null) return;

            graphicsObj.FillRectangle(backgroundBrush, (float)0, (float)0, (float)virtualPlotSize.Width, (float)virtualPlotSize.Height);
        }

        // ####################################################################
        // ##### Test and Diagnostic Junk
        // ####################################################################
        #region Test and Diagnostic Junk

 
        public void Test()
        {
            hScrollBar1.SmallChange=1;
            hScrollBar1.LargeChange = 50;
            hScrollBar1.Minimum = 0;
            hScrollBar1.Maximum = 100;
            hScrollBar1.Value = 10;
        }

        #endregion


    }
}

