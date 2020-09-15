using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
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
    /// A class to contain some useful graphic utilities
    /// </summary>
    /// <history>
    ///    06 Jul 10  Cynic - Started
    /// </history>
    public class MiscGraphicsUtils
    {

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills an ellipse which is centered on a point
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="xCenter">the x centerpoint</param>
        /// <param name="yCenter">the x centerpoint</param>
        /// <param name="hLen">the horiz len</param>
        /// <param name="vLen">the vertical len</param>
        /// <history>
        ///    15 Jul 10  Cynic - Started
        /// </history>
        public static void FillElipseCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, float xCenter, float yCenter, float hLen, float vLen)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;

            // the standard GDI fill elipse draws the elipse inside a rectangle. We drop the bottom left coordinat down 
            // so that it places the xCenter and yCenter in the exact center
            float xBottom = xCenter - (hLen / 2);
            float yBottom = yCenter - (vLen / 2);
            graphicsObj.FillEllipse(workingBrush, xBottom, yBottom, hLen, vLen);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills a rectangle which is centered on a point
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="xCenter">the x centerpoint</param>
        /// <param name="yCenter">the x centerpoint</param>
        /// <param name="hLen">the horiz len</param>
        /// <param name="vLen">the vertical len</param>
        /// <history>
        ///    09 Aug 10  Cynic - Started
        /// </history>
        public static void FillRectangleCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, float xCenter, float yCenter, float hLen, float vLen)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;

            // the standard GDI fill elipse draws the elipse inside a rectangle. We drop the bottom left coordinat down 
            // so that it places the xCenter and yCenter in the exact center
            float xBottom = xCenter - (hLen / 2);
            float yBottom = yCenter - (vLen / 2);
            graphicsObj.FillRectangle(workingBrush, xBottom, yBottom, hLen, vLen);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts 4 points and figures out the upper and lower coordinates of 
        /// the smallest rectangle which encloses those 4 points.
        /// </summary>
        /// <param name="pt1">a point</param>
        /// <param name="pt2">a point</param>
        /// <param name="pt3">a point</param>
        /// <param name="pt4">a point</param>
        /// <param name="ptLL">lower left point of rect</param>
        /// <param name="ptUR">upper right point of rect</param>
        /// <returns>z succes, nz fail</returns>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        ///    03 Aug 10  Cynic - ReFactored into here
        /// </history>
        public static int GetBoundingRectangleEndPointsFrom4Points(Point pt1, Point pt2, Point pt3, Point pt4, out Point ptLL, out Point ptUR)
        {
            int lx, ly;
            int hx, hy;

            // yes, we do, figure out the smallest and largest XY for our bounding box
            lx = int.MaxValue;
            ly = int.MaxValue;
            hx = int.MinValue;
            hy = int.MinValue;
            if (pt2.X < lx) lx = pt2.X;
            if (pt1.X < lx) lx = pt1.X;
            if (pt4.X < lx) lx = pt4.X;
            if (pt3.X < lx) lx = pt3.X;
            if (pt2.Y < ly) ly = pt2.Y;
            if (pt1.Y < ly) ly = pt1.Y;
            if (pt4.Y < ly) ly = pt4.Y;
            if (pt3.Y < ly) ly = pt3.Y;
            if (pt2.X > hx) hx = pt2.X;
            if (pt1.X > hx) hx = pt1.X;
            if (pt4.X > hx) hx = pt4.X;
            if (pt3.X > hx) hx = pt3.X;
            if (pt2.Y > hy) hy = pt2.Y;
            if (pt1.Y > hy) hy = pt1.Y;
            if (pt4.Y > hy) hy = pt4.Y;
            if (pt3.Y > hy) hy = pt3.Y;

            ptLL = new Point(lx, ly);
            ptUR = new Point(hx, hy);
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts the coords of a line and a width and returns the endpoints of
        /// the rectangle which has a perpendicular width equal to the specified
        /// width.
        /// </summary>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <param name="ptLL">lower left point of rect</param>
        /// <param name="ptLR">lower right point of rect</param>
        /// <param name="ptUL">upper left point of rect</param>
        /// <param name="ptUR">upper right point of rect</param>
        /// <returns>z succes, nz fail</returns>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        ///    03 Aug 10  Cynic - ReFactored into here
        /// </history>
        public static int GetWideLineEndPoints(int x0, int y0, int x1, int y1, int width, out Point ptLL, out Point ptUL, out Point ptUR, out Point ptLR)
        {
            double dx2;
            double dy2;

            ptUL = new Point(int.MinValue, int.MinValue);
            ptLL = new Point(int.MinValue, int.MinValue);
            ptLR = new Point(int.MinValue, int.MinValue);
            ptUR = new Point(int.MinValue, int.MinValue);

            if (width <= 0) return 0;

            // first find the run and rise of the input line
            double dx = x1 - x0;
            double dy = y1 - y0;

            // figure out the lengths of the perpendiculars, this
            // takes into account odd widths.
            int l1 = width / 2;
            int l2 = width - l1;

            if (dy == 0)
            {
                dy2 = 1;
                // this means input line is horizontal, therefore perpendicular
                // line is vertical, figure out our four corner end points
                ptUL = new Point(x0, (int)Math.Round((y0 + (dy2 * l1))));
                ptLL = new Point(x0, (int)Math.Round((y0 - (dy2 * l2))));
                ptLR = new Point(x1, (int)Math.Round((y1 - (dy2 * l2))));
                ptUR = new Point(x1, (int)Math.Round((y1 + (dy2 * l1))));
            }
            if (dx == 0)
            {
                dx2 = 1;
                // this means input line is vertical, therefore perpendicular
                // line is horizontal, figure out our four corner end points
                ptUL = new Point((int)Math.Round((x0 - (dx2 * l1))), y0);
                ptLL = new Point((int)Math.Round((x0 + (dx2 * l2))), y0);
                ptLR = new Point((int)Math.Round((x1 + (dx2 * l2))), y1);
                ptUR = new Point((int)Math.Round((x1 - (dx2 * l1))), y1);
            }
            else
            {
                // figure out the angle the line is at
                double theta = Math.Atan(dy / dx);
                // figure out the run and rise of the perpendicular
                // Sin and Cos are switched here to give us this
                dx2 = Math.Sin(theta);
                dy2 = Math.Cos(theta);

                // figure out our four corner end points
                ptUL = new Point((int)Math.Round((x0 - (dx2 * l1))), (int)Math.Round((y0 + (dy2 * l1))));
                ptLL = new Point((int)Math.Round((x0 + (dx2 * l2))), (int)Math.Round((y0 - (dy2 * l2))));
                ptLR = new Point((int)Math.Round((x1 + (dx2 * l2))), (int)Math.Round((y1 - (dy2 * l2))));
                ptUR = new Point((int)Math.Round((x1 - (dx2 * l1))), (int)Math.Round((y1 + (dy2 * l1))));
            }
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Utility code to return a Rect which bounds a circle.
        /// </summary>
        /// <returns>bounding rect or default rect for fail</returns>
        /// <history>
        ///    04 Aug 10  Cynic - Started
        /// </history>
        public static Rectangle GetBoundingRectForCircle(int cx, int cy, int radius)
        {
            if (cx < 0) return new Rectangle();
            if (cy < 0) return new Rectangle();
            if (radius < 0) return new Rectangle();

            // this is the bounding box for the circle
            return new Rectangle(cx - radius, cy - radius, radius * 2, radius * 2);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the counter clockwise angle between the X axis and the specified line
        /// in degrees.
        /// </summary>
        /// <returns>angle in degrees</returns>
        /// <history>
        ///    05 Aug 10  Cynic - Started
        /// </history>
        public static float GetCCWAngleOfLineToXAxis(int x0, int y0, int x1, int y1)
        {
            return (360 - GetCWAngleOfLineToXAxis(x0, y0, x1, y1))%360;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the clockwise angle between the X axis and the specified line
        /// in degrees.
        /// </summary>
        /// <returns>angle in degrees</returns>
        /// <history>
        ///    04 Aug 10  Cynic - Started
        /// </history>
        public static float GetCWAngleOfLineToXAxis(int x0, int y0, int x1, int y1)
        {
            // get x and y components
            double xLen = (double)(x1 - x0);
            double yLen = (double)(y1 - y0);

            // some simple cases
            if (xLen == 0)
            {
                if (yLen > 0) return 270f;
                else return 90f;
            }
            if (yLen == 0)
            {
                if(xLen>0) return 0f;
                else return 180f;
            }

            double radAngle = Math.Atan2(yLen, xLen);
            // figure out the quadrant
            if ((xLen > 0) && (yLen > 0))
            {
                // UR quadrant
                int degAngle = (int)Math.Round((radAngle*180)/Math.PI);
                int convAngle = ((360 - degAngle) % 360);
                return convAngle;
            }
            else if ((xLen > 0) && (yLen < 0))
            {
                // LR quadrant
                int degAngle = (int)Math.Round((radAngle * 180) / Math.PI);
                int convAngle = ((360 - degAngle) % 360);
                return convAngle;
            }
            else if ((xLen < 0) && (yLen < 0))
            {
                // LL quadrant
                int degAngle = (int)Math.Round((radAngle * 180) / Math.PI);
                int convAngle = ((360 - degAngle) % 360);
                return convAngle;
            }
            else if ((xLen < 0) && (yLen > 0))
            {
                // UL quadrant
                int degAngle = (int)Math.Round((radAngle * 180) / Math.PI);
                int convAngle = ((360 - degAngle) % 360);
                return convAngle;
            }

            return 0;
        }
    }
}
