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
    /// A class to provide a single place to define all of the colors we use
    /// </summary>
    public static class ApplicationColorManager
    {
        // this color is the color of the panel on which the plot sits when
        // the plot is drawn, it will form a small border around the plot
        public static readonly Color DEFAULT_PLOT_PANEL_COLOR = Color.Goldenrod;

        // this brush is the generic background color for the plot
        // it should be used by gerber, iso and gcode plots
        public static readonly Brush DEFAULT_PLOT_BACKGROUND_BRUSH = Brushes.Goldenrod;

        // this pen is the one which we use to draw the border around the 
        // plots
        public static readonly Pen DEFAULT_PLOT_BORDER_PEN = Pens.Black;

        // this pen is the one which we use to draw the origin crosshairs
        public static readonly Pen DEFAULT_PLOT_GCODE_ORIGIN_PEN = Pens.Maroon;
        public static readonly Pen DEFAULT_PLOT_ORIGIN_PEN = Pens.SeaGreen;
        public static readonly Pen DEFAULT_PLOT_FLIPAXIS_PEN = Pens.YellowGreen;

        // ####################################################################
        // ##### Items used by gerber plots
        // ####################################################################
        #region Items used by gerber plots

        // this is the color in which we draw the gerber plot lines
        public static readonly Color DEFAULT_GERBERPLOT_FOREGROUND_COLOR = Color.Black;

        // this is the color in which we draw the gerber plot backgrounds
        public static readonly Color DEFAULT_GERBERPLOT_BACKGROUND_COLOR = DEFAULT_PLOT_PANEL_COLOR;

        // this is a color used when drawing macros. It never makes it onto the actual display
        public static readonly Color DEFAULT_MACRO_TRANSPARENT_COLOR = Color.AliceBlue;
        public static readonly Brush DEFAULT_MACRO_TRANSPARENT_COLOR_Brush = Brushes.AliceBlue;

        // this is the color of the centerlines on the gerber plot (if drawn)
        public static readonly Pen DEFAULT_GERBERPLOT_CENTERLINE_PEN = Pens.Red;

        // this is the color in which we draw the gerber plot contour lines
        public static readonly Pen DEFAULT_GERBERPLOT_CONTOURLINE_PEN = Pens.Black;

        // if we show distinct apertures we use this brush otherwise it is just the foreground brush
        public static readonly Brush DEFAULT_GERBERPLOT_SHOWAPERTURE_BRUSH = Brushes.Gray;

        // the foreground (line) color of gerber plots. Notre should be the same colour as the plotLineColor 
        public static readonly Brush DEFAULT_GERBERPLOT_FOREGROUND_BRUSH = Brushes.Black;

        // this background brush is just the background color of the plot
        public static readonly Brush DEFAULT_GERBERPLOT_BACKGROUND_BRUSH = DEFAULT_PLOT_BACKGROUND_BRUSH;

        // this is the color we use to fill in gerber contour regions
        public static readonly Brush DEFAULT_GERBERPLOT_CONTOURFILL_Brush = Brushes.Black;

        #endregion

        // ####################################################################
        // ##### Items used by iso plots
        // ####################################################################
        #region Items used by iso plots

        // color of intersections on isoplots
        public static Color ISOINTERSECTION_COLOR = Color.White;
        public const uint ISOINTERSECTION_COLOR_AS_UINT = 0xffffffff;

        // color of normal edges on isoplots
        public static Color ISONORMALEDGE_COLOR = Color.Red;
        public const uint ISONORMALEDGE_COLOR_AS_UINT = 0xffff0000;

        // color of not drawn normal edges on isoplots
        public static Color ISONORMALEDGE_NOTDRAWN_COLOR = Color.DarkRed;
        public const uint ISONORMALEDGE_NOTDRAWN_COLOR_AS_UINT = 0xff8b0000;

        // color of contour edges on isoplots
        public static Color ISOCONTOUREDGE_COLOR = Color.Green;
        public const uint ISOCONTOUREDGE_COLOR_AS_UINT = 0xff00ff00;

        // color of not drawn contour edges on isoplots
        public static Color ISOCONTOUREDGE_NOTDRAWN_COLOR = Color.DarkGreen;
        public const uint ISOCONTOUREDGE_NOTDRAWN_COLOR_AS_UINT = 0xff006400;

        // color of invert edges on isoplots
        public static Color ISOINVERTEDGE_COLOR = Color.Turquoise;
        public const uint ISOINVERTEDGE_COLOR_AS_UINT = 0xff30D5C8;

        // color of not drawn invert edges on isoplots
        public static Color ISOINVERTEDGE_NOTDRAWN_COLOR = Color.DarkTurquoise;
        public const uint ISOINVERTEDGE_NOTDRAWN_COLOR_AS_UINT = 0xff00ced1;

        // color of the object interiors on isoplots. This is the 
        // fill color inside of the lines and circles we draw
        public static Color ISOINTERIOR_COLOR = Color.Blue;
        public const uint ISOINTERIOR_COLOR_AS_UINT = 0xff0000ff;
 
        // color of pixels which have one or more object backgrounds or edges
        // using them. Hence the term overlay
        public static Color ISOOVERLAY_COLOR = Color.Yellow;
        public const uint ISOOVERLAY_COLOR_AS_UINT = 0xffffff00;

        // color of pixels which are not involved in an IsoPlotObject
        // in any way - the general visible background. Should be the same
        // color as the DEFAULT_PLOT_BACKGROUND_BRUSH
        public static Color ISOBACKGROUND_COLOR = Color.Gray;
        public const uint ISOBACKGROUND_COLOR_AS_UINT = 0xFF808080;

        // color for errors
        public static Color ISOERROR_COLOR = Color.Pink;
        public const uint ISOERROR_COLOR_AS_UINT = 0xffffc0cb;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts an Iso RGB value to a Color
        /// </summary>
        public static Color ConvertIsoBackgroundRGBToColor(uint rgbVal)
        {
            switch(rgbVal)
            {
                case ISOINTERSECTION_COLOR_AS_UINT:
                    return ISOINTERSECTION_COLOR;

                case ISOINTERIOR_COLOR_AS_UINT:
                    return ISOINTERIOR_COLOR;

                case ISONORMALEDGE_COLOR_AS_UINT:
                    return ISONORMALEDGE_COLOR;

                case ISONORMALEDGE_NOTDRAWN_COLOR_AS_UINT:
                    return ISONORMALEDGE_NOTDRAWN_COLOR;

                case ISOOVERLAY_COLOR_AS_UINT:
                    return ISOOVERLAY_COLOR;

                case ISOCONTOUREDGE_COLOR_AS_UINT:
                    return ISOCONTOUREDGE_COLOR;

                case ISOBACKGROUND_COLOR_AS_UINT:
                    return ISOBACKGROUND_COLOR;

                case ISOINVERTEDGE_COLOR_AS_UINT:
                    return ISOINVERTEDGE_COLOR;

                case ISOINVERTEDGE_NOTDRAWN_COLOR_AS_UINT:
                    return ISOINVERTEDGE_NOTDRAWN_COLOR;

                case ISOCONTOUREDGE_NOTDRAWN_COLOR_AS_UINT:
                    return ISOCONTOUREDGE_NOTDRAWN_COLOR;                    
                default:
                    return ISOERROR_COLOR;
            }
        }

        #endregion

        // ####################################################################
        // ##### Items used by gcode plots
        // ####################################################################
        #region Items used by gcode plots

        // this plotBrush is the background color of the plot, should be different
        // than the panels background color
        public static readonly Brush DEFAULT_GCODEPLOT_BACKGROUND_BRUSH = DEFAULT_PLOT_BACKGROUND_BRUSH;

        // this is the color we use
        public static readonly Color DEFAULT_GCODEPLOT_LINE_COLOR = Color.White;

        // this is the pen we use to plot the gcode lines
        public static readonly Pen DEFAULT_GCODEPLOT_BORDER_PEN = Pens.White;

        // this plotBrush should be the same colour as the plotLineColor
        public static readonly Brush DEFAULT_GCODEPLOTBORDER_BRUSH = Brushes.White;

        // this is the pen we use to plot the gcode line centercuts
        public static readonly Pen DEFAULT_GCODEPLOT_CUTLINE_PEN = Pens.Aquamarine;

        #endregion

        // ####################################################################
        // ##### Items used by excellon plots
        // ####################################################################
        #region Items used by excellon plots

        // this is the color in which we draw the excellon plot lines
        public static readonly Color DEFAULT_EXCELLONPLOT_LINE_COLOR = Color.Black;

        // this is the color for the excellon drill holes
        public static readonly Brush DEFAULT_EXCELLONPLOT_DRILLHOLE_BRUSH = Brushes.Black;

        // this plotBrush is the background color of the plot, should be different
        // than the panels background color
        public static readonly Brush DEFAULT_EXCELLONPLOT_BACKGROUND_BRUSH = DEFAULT_PLOT_BACKGROUND_BRUSH;

        #endregion

    }
}

