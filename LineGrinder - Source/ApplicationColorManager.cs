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
    /// <history>
    ///    21 Aug 10  Cynic - Started
    /// </history>
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
        public static readonly Pen DEFAULT_PLOT_ORIGIN_PEN = Pens.Maroon;

        // ####################################################################
        // ##### Items used by gerber plots
        // ####################################################################
        #region Items used by gerber plots

        // this is the color in which we draw the gerber plot lines
        public static readonly Color DEFAULT_GERBERPLOT_LINE_COLOR = Color.Black;

        // this is the color of the centerlines on the gerber plot (if drawn)
        public static readonly Pen DEFAULT_GERBERPLOT_CENTERLINE_PEN = Pens.Red;

        // this plotBrush should be the same colour as the plotLineColor if
        // we are not showing apertures
        public static readonly Brush DEFAULT_GERBERPLOT_BRUSH_SHOW_APERTURES = Brushes.Gray;
        public static readonly Brush DEFAULT_GERBERPLOT_BRUSH_NOSHOW_APERTURES = Brushes.Black;

        // this plotBrush is the background color of the plot, should be different
        // than the panels background color
        public static readonly Brush DEFAULT_GERBERPLOT_BACKGROUND_BRUSH = DEFAULT_PLOT_BACKGROUND_BRUSH;

        #endregion

        // ####################################################################
        // ##### Items used by iso plots
        // ####################################################################
        #region Items used by iso plots

        // color of intersections on isoplots
        public static Color ISOINTERSECTION_COLOR = Color.White;
        public const uint ISOINTERSECTION_COLOR_AS_UINT = 0xffffffff;

        // color of edges on isoplots
        public static Color ISOEDGE_COLOR = Color.Red;
        public const uint ISOEDGE_COLOR_AS_UINT = 0xffff0000;

        // color of the object interiors on isoplots. This is the 
        // fill color inside of the lines and circles we draw
        public static Color ISOINTERIOR_COLOR = Color.Blue;
        public const uint ISOINTERIOR_COLOR_AS_UINT = 0xff0000ff;
 
        // color of pixels which have one or more object backgrounds or edges
        // using them. Hence the term overlay
        public static Color ISOOVERLAY_COLOR = Color.Green;
        public const uint ISOOVERLAY_COLOR_AS_UINT = 0xff00ff00;

        // color of pixels which are not involved in an GCodeBuilderObject
        // in any way - the general visible background. Should be the same
        // color as the DEFAULT_PLOT_BACKGROUND_BRUSH
        public static Color ISOBACKGROUND_COLOR = Color.Goldenrod;
        public const uint ISOBACKGROUND_COLOR_AS_UINT = 0xDAA520;
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

        // this is the color of the centerlines on the excellon plot (if drawn)
        public static readonly Pen DEFAULT_EXCELLONPLOT_CENTERLINE_PEN = Pens.Red;

        // this plotBrush should be the same colour as the plotLineColor if
        // we are not showing apertures
        public static readonly Brush DEFAULT_EXCELLONPLOT_BRUSH_SHOW_APERTURES = Brushes.Gray;
        public static readonly Brush DEFAULT_EXCELLONPLOT_BRUSH_NOSHOW_APERTURES = Brushes.Black;

        // this plotBrush is the background color of the plot, should be different
        // than the panels background color
        public static readonly Brush DEFAULT_EXCELLONPLOT_BACKGROUND_BRUSH = DEFAULT_PLOT_BACKGROUND_BRUSH;

        #endregion

    }
}
