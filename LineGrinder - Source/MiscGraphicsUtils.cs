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
    public class MiscGraphicsUtils
    {

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills a region specified by an array of points. The array should be closed
        /// In other words, the first and last point should be the same. If this is not
        /// true the graphics object will close it for you. 
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="vertexPoints">the array of vertex points. Expects all rotations to have been done previously</param>
        public static void FillOutlineCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, PointF[] vertexPoints)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            // at minimum we expect to see 3 vertex points
            if (vertexPoints == null) return;
            if (vertexPoints.Length < 3) return;

            // use the standard GDI fill polygon  
            graphicsObj.FillPolygon(workingBrush, vertexPoints);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills an polygon which is centered on a point
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="xCenter">the x centerpoint</param>
        /// <param name="yCenter">the x centerpoint</param>
        /// <param name="numSides">the number of sides</param>
        /// <param name="diameter">the diameter of the circle enclosing the polygon</param>
        /// <param name="rotationAngleInDegrees">the rotation angle of the polygon in degrees</param>
        public static void FillPolygonCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, float xCenter, float yCenter, int numSides, float diameter, float rotationAngleInDegrees)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            if (numSides < 3) return;
            if (diameter <= 0) return;

            // get the vertexes
            System.Drawing.PointF[] vertexPoints = MiscGraphicsUtils.CalcPolygonVertexPoints(xCenter, yCenter, numSides, diameter, rotationAngleInDegrees);

            // use the standard GDI fill polygon  
            graphicsObj.FillPolygon(workingBrush, vertexPoints);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills an polygon which is centered on a point, with centered circular clipping region
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="xCenter">the x centerpoint</param>
        /// <param name="yCenter">the x centerpoint</param>
        /// <param name="numSides">the number of sides</param>
        /// <param name="diameter">the diameter of the circle enclosing the polygon</param>
        /// <param name="rotationAngleInDegrees">the rotation angle of the polygon in degrees</param>
        /// <param name="clipDia">the clipping diameter</param>
        public static void FillPolygonCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, float xCenter, float yCenter, int numSides, float diameter, float rotationAngleInDegrees, float clipDia)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;

            // do we actually have a centered clipping region?
            if (clipDia <= 0)
            {
                // justs call the regular one
                FillPolygonCenteredOnPoint(graphicsObj, workingBrush, xCenter, yCenter, numSides, diameter, rotationAngleInDegrees);
                return;
            }

            // just call this 
            FillPolygonCenteredOnPoint(graphicsObj, workingBrush, xCenter, yCenter, numSides, diameter, rotationAngleInDegrees, clipDia, xCenter, yCenter);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills an polygon which is centered on a point, with specified circular clipping region
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="xCenter">the x centerpoint</param>
        /// <param name="yCenter">the x centerpoint</param>
        /// <param name="numSides">the number of sides</param>
        /// <param name="diameter">the diameter of the circle enclosing the polygon</param>
        /// <param name="rotationAngleInDegrees">the rotation angle of the polygon in degrees</param>
        /// <param name="clipDia">the clipping diameter</param>
        /// <param name="clipXCenter">the X centerpoint for the clip region</param>
        /// <param name="clipYCenter">the Y centerpoint for the clip region</param>
        public static void FillPolygonCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, float xCenter, float yCenter, int numSides, float diameter, float rotationAngleInDegrees, float clipDia, float clipXCenter, float clipYCenter)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            if (clipXCenter < 0) return;
            if (clipYCenter < 0) return;

            // do we actually have a clipping region?
            if (clipDia <= 0)
            {
                // justs call the regular one
                FillPolygonCenteredOnPoint(graphicsObj, workingBrush, xCenter, yCenter, numSides, diameter, rotationAngleInDegrees);
                return;
            }

            // get the vertexes
            System.Drawing.PointF[] vertexPoints = MiscGraphicsUtils.CalcPolygonVertexPoints(xCenter, yCenter, numSides, diameter, rotationAngleInDegrees);

            // note the AddPolygon uses the upper left point of the rectangle bounding the polygon not the centerpoint
            // calc the clip upperLeft point
            float xClipUpperLeft = clipXCenter - (clipDia / 2);
            float yClipUpperLeft = clipYCenter - (clipDia / 2);

            GraphicsPath pathObj = new GraphicsPath();
            pathObj.AddEllipse(xClipUpperLeft, yClipUpperLeft, clipDia, clipDia);
            graphicsObj.SetClip(pathObj, CombineMode.Exclude);
            // fill the polygon as normal
            graphicsObj.FillPolygon(workingBrush, vertexPoints);
            // remove the clip from the graphics object
            graphicsObj.ResetClip();
            // dispose of the clip
            pathObj.Dispose();
        }

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
        public static void FillEllipseCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, float xCenter, float yCenter, float hLen, float vLen)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;

            // the standard GDI fill elipse draws the elipse inside a rectangle. We drop the bottom left coordinate down 
            // so that it places the xCenter and yCenter in the exact center
            float xBottom = xCenter - (hLen / 2);
            float yBottom = yCenter - (vLen / 2);
            graphicsObj.FillEllipse(workingBrush, xBottom, yBottom, hLen, vLen);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills an ellipse which is centered on a point, with centered circular clipping region
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="xCenter">the x centerpoint</param>
        /// <param name="yCenter">the x centerpoint</param>
        /// <param name="hLen">the horiz len</param>
        /// <param name="vLen">the vertical len</param>
        /// <param name="clipDia">the clipping diameter</param>
        public static void FillEllipseCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, float xCenter, float yCenter, float hLen, float vLen, float clipDia)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;

            // do we actually have a centered clipping region?
            if (clipDia <= 0)
            {
                // justs call the regular one
                FillEllipseCenteredOnPoint(graphicsObj, workingBrush, xCenter, yCenter, hLen, vLen);
                return;
            }

            // just call this 
            FillEllipseCenteredOnPoint(graphicsObj, workingBrush, xCenter, yCenter, hLen, vLen, clipDia, xCenter, yCenter);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills an ellipse which is centered on a point, with specified circular clipping region
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="xCenter">the x centerpoint</param>
        /// <param name="yCenter">the x centerpoint</param>
        /// <param name="hLen">the horiz len</param>
        /// <param name="vLen">the vertical len</param>
        /// <param name="clipDia">the clipping diameter</param>
        /// <param name="clipXCenter">the X centerpoint for the clip region</param>
        /// <param name="clipYCenter">the Y centerpoint for the clip region</param>
        public static void FillEllipseCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, float xCenter, float yCenter, float hLen, float vLen, float clipDia, float clipXCenter, float clipYCenter)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            if (clipXCenter < 0) return;
            if (clipYCenter < 0) return;

            // do we actually have a clipping region?
            if (clipDia <= 0)
            {
                // justs call the regular one
                FillEllipseCenteredOnPoint(graphicsObj, workingBrush, xCenter, yCenter, hLen, vLen);
                return;
            }

            // the standard GDI fill elipse draws the elipse inside a rectangle. We drop the bottom left coordinate down 
            // so that it places the xCenter and yCenter in the exact center
            float xBottom = xCenter - (hLen / 2);
            float yBottom = yCenter - (vLen / 2);

            // note the AddEllipse uses the upper left point of the rectangle bounding the ellipse not the centerpoint
            // calc the clip upperLeft point
            float xClipUpperLeft = clipXCenter - (clipDia / 2);
            float yClipUpperLeft = clipYCenter - (clipDia / 2);

            GraphicsPath pathObj = new GraphicsPath();
            pathObj.AddEllipse(xClipUpperLeft, yClipUpperLeft, clipDia, clipDia);
            graphicsObj.SetClip(pathObj, CombineMode.Exclude);
            // fill the elipse as normal
            graphicsObj.FillEllipse(workingBrush, xBottom, yBottom, hLen, vLen);
            // remove the clip from the graphics object
            graphicsObj.ResetClip();
            // dispose of the clip
            pathObj.Dispose();
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
        public static void FillRectangleCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, float xCenter, float yCenter, float hLen, float vLen)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;

            // We calc the x and y coords of the bottom
            float xBottom = xCenter - (hLen / 2);
            float yBottom = yCenter - (vLen / 2);
            graphicsObj.FillRectangle(workingBrush, xBottom, yBottom, hLen, vLen);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills a rectangle which is centered on a point, with centered circular clipping region
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="xCenter">the x centerpoint</param>
        /// <param name="yCenter">the x centerpoint</param>
        /// <param name="hLen">the horiz len</param>
        /// <param name="vLen">the vertical len</param>
        /// <param name="clipDia">the clipping diameter</param>
        public static void FillRectangleCenteredOnPoint(Graphics graphicsObj, Brush workingBrush, float xCenter, float yCenter, float hLen, float vLen, float clipDia)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;

            // do we actually have a centered clipping region?
            if (clipDia <= 0)
            {
                // justs call the regular one
                FillRectangleCenteredOnPoint(graphicsObj, workingBrush, xCenter, yCenter, hLen, vLen);
                return;
            }

            // We calc the x and y coords of the bottom
            float xBottom = xCenter - (hLen / 2);
            float yBottom = yCenter - (vLen / 2);

            GraphicsPath pathObj = new GraphicsPath();
            pathObj.AddEllipse(xCenter - (clipDia / 2), yCenter - (clipDia / 2), clipDia, clipDia);
            graphicsObj.SetClip(pathObj, CombineMode.Exclude);
            // fill the elipse as normal
            graphicsObj.FillRectangle(workingBrush, xBottom, yBottom, hLen, vLen);
            // remove the clip from the graphics object
            graphicsObj.ResetClip();
            // dispose of the clip
            pathObj.Dispose();
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
        public static int GetBoundingRectangleEndPointsFrom2Points(PointF pt1, PointF pt2, out PointF ptLL, out PointF ptUR)
        {
            float lx, ly;
            float hx, hy;

            // yes, we do, figure out the smallest and largest XY for our bounding box
            lx = float.MaxValue;
            ly = float.MaxValue;
            hx = float.MinValue;
            hy = float.MinValue;
            if (pt2.X < lx) lx = pt2.X;
            if (pt1.X < lx) lx = pt1.X;
            if (pt2.Y < ly) ly = pt2.Y;
            if (pt1.Y < ly) ly = pt1.Y;
            if (pt2.X > hx) hx = pt2.X;
            if (pt1.X > hx) hx = pt1.X;
            if (pt2.Y > hy) hy = pt2.Y;
            if (pt1.Y > hy) hy = pt1.Y;

            ptLL = new PointF(lx, ly);
            ptUR = new PointF(hx, hy);
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
        /// <param name="x0">x0 coord</param>
        /// <param name="y0">y0 coord</param>
        /// <param name="x1">x1 coord</param>
        /// <param name="y1">y1 coord</param>
        /// <returns>angle in degrees</returns>
        public static float GetCCWAngleOfLineToXAxis(float x0, float y0, float x1, float y1)
        {
            return (360 - GetCWAngleOfLineToXAxis(x0, y0, x1, y1))%360;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the clockwise angle between the X axis and the specified line
        /// in degrees.
        /// </summary>
        /// <param name="x0">x0 coord</param>
        /// <param name="y0">y0 coord</param>
        /// <param name="x1">x1 coord</param>
        /// <param name="y1">y1 coord</param>
        /// <returns>angle in degrees</returns>
        public static float GetCWAngleOfLineToXAxis(float x0, float y0, float x1, float y1)
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
                if (xLen > 0) return 0f;
                else return 180f;
            }

            double radAngle = Math.Atan2(yLen, xLen);
            // figure out the quadrant
            if ((xLen > 0) && (yLen > 0))
            {
                // UR quadrant
                int degAngle = (int)Math.Round((radAngle * 180) / Math.PI);
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

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a list of the vertex points for the Polygon. 
        /// </summary>
        /// <param name="x1">the x translation</param>
        /// <param name="y1">the y translation</param>
        /// <param name="numSides">the number of sides</param>
        /// <param name="diameter">the diameter of the circle the polygon is inside of</param>
        /// <param name="rotationAngleInDegrees">the rotation angle of the polygon in degrees</param>
        /// <returns>an array of PointF objects describing the polygon</returns>
        public static System.Drawing.PointF[] CalcPolygonVertexPoints(float x1, float y1, int numSides, float diameter, float rotationAngleInDegrees)
        {
            float xCoord = 0F;
            float yCoord = 0F;

            if (numSides < 3) return new System.Drawing.PointF[0];

            // define our vertex array
            System.Drawing.PointF[] vertexPoints = new System.Drawing.PointF[numSides];
            if (diameter <= 0) return vertexPoints;

            // assume this for now
            double currentAngleInRadians = MiscGraphicsUtils.DegreesToRadians(rotationAngleInDegrees);
            double angleDivisorInRadians = MiscGraphicsUtils.DegreesToRadians(360F / (double)numSides);

            // loop through each side
            for (int i = 0; i < numSides; i++)
            {
                // note we divide by 2 here because we have to use the radius
                xCoord = (float)(diameter * Math.Cos(currentAngleInRadians) / 2);
                yCoord = (float)(diameter * Math.Sin(currentAngleInRadians) / 2);

                // translate
                xCoord = xCoord + x1;
                yCoord = yCoord + y1;

                // set the vertex points
                vertexPoints[i] = new PointF(xCoord, yCoord);
                currentAngleInRadians += angleDivisorInRadians;
            }
            return vertexPoints;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts an array of PointF's to points
        /// </summary>
        /// <returns>the converted array</returns>
        public static System.Drawing.Point[] ConvertPointFArrayToPoint(System.Drawing.PointF[] pointFArray)
        {
            if(pointFArray==null) return new System.Drawing.Point[0];
            // set up the array
            System.Drawing.Point[] outArray = new System.Drawing.Point[pointFArray.Length];
            // loop thorough
            for(int i=0; i<pointFArray.Length; i++)
            {
                // round the PointF this converts it to Point
                outArray[i] = Point.Round(pointFArray[i]);                
            }
            return outArray;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <returns>angle in radians</returns>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the Distance between two points. 
        /// </summary>
        /// <param name="point1">point1</param>
        /// <param name="point2">point2</param>
        public static float GetDistanceBetweenTwoPoints(PointF point1, PointF point2)
        {
            return GetDistanceBetweenTwoPoints(point1.X, point1.Y, point2.X, point2.Y);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the Distance between two points. Not terribly efficient, rewrite 
        /// to remove Pow if it is called frequently
        /// </summary>
        /// <param name="x0">x0 val</param>
        /// <param name="y0">y0 val</param>
        /// <param name="x1">x1 val</param>
        /// <param name="y1">y1 val</param>
        public static float GetDistanceBetweenTwoPoints(float x0, float y0, float x1, float y1)
        {
            return (float)Math.Sqrt((Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2)));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the Distance between two points. Not terribly efficient, rewrite 
        /// to remove Pow if it is called frequently
        /// </summary>
        /// <param name="x0">x0 val</param>
        /// <param name="y0">y0 val</param>
        /// <param name="x1">x1 val</param>
        /// <param name="y1">y1 val</param>
        public static float GetDistanceBetweenTwoPoints(int x0, int y0, int x1, int y1)
        {
            return (float)Math.Sqrt((Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2)));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the start angle and a sweep angle from two points and a center point.
        /// Counter Clockwise Version
        /// </summary>
        /// <remarks>
        /// The start point and end point are assumed to be on the same circle but this is not checked
        /// </remarks>
        /// <param name="startPoint">the start point</param>
        /// <param name="endPoint">the second point</param>
        /// <param name="centerPoint">the center point</param>
        public static void GetCounterClockwiseStartAndSweepAnglesFrom2PointsAndCenter(PointF startPoint, PointF endPoint, PointF centerPoint, out float startAngleInDegrees, out float sweepAngleInDegrees)
        {
            startAngleInDegrees = MiscGraphicsUtils.GetCCWAngleOfLineToXAxis(centerPoint.X, centerPoint.Y, startPoint.X, startPoint.Y);
            float aEnd = MiscGraphicsUtils.GetCCWAngleOfLineToXAxis(centerPoint.X, centerPoint.Y, endPoint.X, endPoint.Y);
            
            // hard to tell here what is going on it is up to the caller to interpret this as either 0 or 360
            if (startAngleInDegrees == aEnd) sweepAngleInDegrees = 0;
            else if (startAngleInDegrees > aEnd) sweepAngleInDegrees = aEnd - startAngleInDegrees + 360;
            else sweepAngleInDegrees = aEnd - startAngleInDegrees;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Convert counter clockwise sweep angle to clockwise, this can go negative
        /// </summary>
        /// <param name="angleIn">the sweep angle</param>
        public static float ConvertCounterClockwiseWSweepAngleToClockwise(float angleIn)
        {
            return ((360 - angleIn) %360) *-1;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the start angle and a sweep angle from two points and a center point.
        /// Clockwise Version
        /// </summary>
        /// <remarks>
        /// The start point and end point are assumed to be on the same circle but this is not checked
        /// </remarks>
        /// <param name="startPoint">the start point</param>
        /// <param name="endPoint">the second point</param>
        /// <param name="centerPoint">the center point</param>
        //public static void GetClockwiseStartAndSweepAnglesFrom2PointsAndCenter(PointF startPoint, PointF endPoint, PointF centerPoint, out float startAngleInDegrees, out float sweepAngleInDegrees)
        //{
        //    startAngleInDegrees = MiscGraphicsUtils.GetCWAngleOfLineToXAxis(centerPoint.X, centerPoint.Y, startPoint.X, startPoint.Y);
        //    float aEnd = MiscGraphicsUtils.GetCWAngleOfLineToXAxis(centerPoint.X, centerPoint.Y, endPoint.X, endPoint.Y);

        //    // thisis probably not correct below
        //    // hard to tell here what is going on it is up to the caller to interpret this as either 0 or 360
        //    if (startAngleInDegrees == aEnd) sweepAngleInDegrees = 0;
        //    else if (startAngleInDegrees > aEnd) sweepAngleInDegrees = aEnd - startAngleInDegrees + 360;
        //    else sweepAngleInDegrees = aEnd - startAngleInDegrees;
        //}

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a list of ArcPointMap objects each representing the skip and draw
        /// percentages of a half quadrant. The order of the output is always sequential
        /// starting +ve X 
        /// </summary>
        /// <param name="startAngleInDegrees">the start angle in degrees</param>
        /// <param name="sweepAngleInDegrees">the sweep angle in degrees</param>
        /// <returns>A list of ArcPointMap objects representing the </returns>
        public static ArcPointMap[] GetArcPointMapListFromStartAndSweepAngles(float startAngleInDegrees, float sweepAngleInDegrees, bool wantClockWise)
        {
            const int NUM_HALF_QUADRANTS = 8;
            const float NUM_DEGREES_INHALF_QUADRANT = 45;
            const float NUM_DEGREES_FULL_CIRCLE = 360;

            float cleanedStartAngle = 0;
            float cleanedStopAngle = 0;
            ArcPointMap[] outPointMapArr = new ArcPointMap[NUM_HALF_QUADRANTS];

            // get a clean start angle. For example if we start at -90 we get 270 out here, this makes comparisons consistent
            cleanedStartAngle = startAngleInDegrees + NUM_DEGREES_FULL_CIRCLE;
            if (cleanedStartAngle >= NUM_DEGREES_FULL_CIRCLE) cleanedStartAngle = cleanedStartAngle % NUM_DEGREES_FULL_CIRCLE;

            // get a clean stop angle. this will be the start angle plus the sweep angle but cleaned up
            cleanedStopAngle = cleanedStartAngle + sweepAngleInDegrees + NUM_DEGREES_FULL_CIRCLE;
            if (cleanedStopAngle >= NUM_DEGREES_FULL_CIRCLE) cleanedStopAngle = cleanedStopAngle % NUM_DEGREES_FULL_CIRCLE;

            // we do counter clockwise by default. Do we want clockwise?
            if (wantClockWise == true)
            {
                // yes, we do. Just switch around the start and stop that does it nicely
                float tmpAngle = cleanedStartAngle;
                cleanedStartAngle = cleanedStopAngle;
                cleanedStopAngle = tmpAngle;
            }

            //DebugMessage("GetArcPointMap cleanedStartAngle=" + cleanedStartAngle.ToString() + ", cleanedStopAngle=" + cleanedStopAngle.ToString());

            // set up our arc point map array
            if (sweepAngleInDegrees == 360)
            {
                // the algorythm breaks down if the sweepangle == 360 so we just do it manually here
                for (int i = 0; i < NUM_HALF_QUADRANTS; i++)
                {
                    // everything is a 0% skip, 100% draw by for a 360 draw
                    outPointMapArr[i] = new ArcPointMap(0, 1);
                }
                // default start and stop
                outPointMapArr[0].ThisIsTheStartOctant = true;
                outPointMapArr[7].ThisIsTheStopOctant = true;
                // we return now there is no point in processing further everthing is set up correctly
                return outPointMapArr;
            }
            else
            {
                for (int i = 0; i < NUM_HALF_QUADRANTS; i++)
                {
                    // everything is a 100% skip, no draw by default
                    outPointMapArr[i] = new ArcPointMap(1, 0);
                }
                // no start or stop
                outPointMapArr[0].ThisIsTheStartOctant = false;
                outPointMapArr[7].ThisIsTheStopOctant = false;
            }

            // now run through the half quadrants - there are 8
            float angleAccumulator = cleanedStartAngle;
            for (int i = 0; i < NUM_HALF_QUADRANTS; i++)
            {
                bool startAngleIsInThisQuadrant = false;
                bool stopAngleIsInThisQuadrant = false;

                // NOTE: the i variable here in this loop is NOT the quadrant number it
                //       is simply a loop counter that lets us count from 0 to 7

                // get the quadrant we are working on
                int workingQuadrantNumber = (int)(angleAccumulator / NUM_DEGREES_INHALF_QUADRANT);
                // calc the start and stop angles
                float workingQuadrantStartAngle = (workingQuadrantNumber) * NUM_DEGREES_INHALF_QUADRANT;
                float workingQuadrantStopAngle = (workingQuadrantNumber + 1) * NUM_DEGREES_INHALF_QUADRANT;

                //DebugMessage("HQ" + workingQuadrantNumber.ToString() + ", start=" + workingQuadrantStartAngle.ToString() + ", stop=" + workingQuadrantStopAngle.ToString());

                // set this now for the next loop
                angleAccumulator = (angleAccumulator + NUM_DEGREES_INHALF_QUADRANT) % NUM_DEGREES_FULL_CIRCLE;

                // at this point we KNOW we are in the quadrant where the draw is running. This
                // is because the first angle we start at is the one that contains it and we leave
                // when we find the stop angle. However, there may be a gap between the start of 
                // the quadrant and the start of the draw or the draw may be 100%
                // this is our skip

                // is our start angle in this quadrant
                if ((workingQuadrantStartAngle <= cleanedStartAngle) && (cleanedStartAngle < workingQuadrantStopAngle)) startAngleIsInThisQuadrant = true;  // yes it is
                // is our stop angle in this quadrant
                if ((workingQuadrantStartAngle <= cleanedStopAngle) && (cleanedStopAngle < workingQuadrantStopAngle)) stopAngleIsInThisQuadrant = true;  // yes it is

                // there are four possible combinations 
                if ((startAngleIsInThisQuadrant == true) && (stopAngleIsInThisQuadrant == true))
                {
                    // we start and stop in a single quadrant
                    outPointMapArr[workingQuadrantNumber].PercentSkip = (cleanedStartAngle % NUM_DEGREES_INHALF_QUADRANT) / NUM_DEGREES_INHALF_QUADRANT;  // skip all up to the start
                    outPointMapArr[workingQuadrantNumber].PercentDraw = (cleanedStopAngle % NUM_DEGREES_INHALF_QUADRANT) / NUM_DEGREES_INHALF_QUADRANT;  // draw all up to the end
                    //DebugMessage("(true,true) skip=" + outPointMapArr[workingQuadrantNumber].PercentSkip.ToString() + ", draw=" + outPointMapArr[workingQuadrantNumber].PercentDraw.ToString());

                    outPointMapArr[workingQuadrantNumber].ThisIsTheStartOctant = true;
                    outPointMapArr[workingQuadrantNumber].ThisIsTheStopOctant = true;

                    // we have seen the stop angle we leave now
                    break;
                }
                else if ((startAngleIsInThisQuadrant == true) && (stopAngleIsInThisQuadrant == false))
                {
                    // we start in this quadrant but stop in a subsequent one
                    outPointMapArr[workingQuadrantNumber].PercentSkip = (cleanedStartAngle % NUM_DEGREES_INHALF_QUADRANT) / NUM_DEGREES_INHALF_QUADRANT;  // skip all up to the start
                    outPointMapArr[workingQuadrantNumber].PercentDraw = 1 - outPointMapArr[workingQuadrantNumber].PercentSkip;  // draw all up to the end
                    //DebugMessage("(true,false) skip=" + outPointMapArr[workingQuadrantNumber].PercentSkip.ToString() + ", draw=" + outPointMapArr[workingQuadrantNumber].PercentDraw.ToString());
                    outPointMapArr[workingQuadrantNumber].ThisIsTheStartOctant = true;
                    // we have not seen the stop angle we go to the next
                    continue;
                }
                else if ((startAngleIsInThisQuadrant == false) && (stopAngleIsInThisQuadrant == true))
                {
                    // we started in a previous quadrant but stop in this one
                    outPointMapArr[workingQuadrantNumber].PercentSkip = 0; // skip nothing
                    outPointMapArr[workingQuadrantNumber].PercentDraw = (cleanedStopAngle % NUM_DEGREES_INHALF_QUADRANT) / NUM_DEGREES_INHALF_QUADRANT;  // draw all up to the end
                    //DebugMessage("(false,true) skip=" + outPointMapArr[workingQuadrantNumber].PercentSkip.ToString() + ", draw=" + outPointMapArr[workingQuadrantNumber].PercentDraw.ToString());
                    outPointMapArr[workingQuadrantNumber].ThisIsTheStopOctant = true;
                    // we have seen the stop angle we leave now
                    break;
                }
                else if ((startAngleIsInThisQuadrant == false) && (stopAngleIsInThisQuadrant == false))
                {
                    // we started in a previous quadrant and stop in a subsequent one
                    outPointMapArr[workingQuadrantNumber].PercentSkip = 0;  // skip none
                    outPointMapArr[workingQuadrantNumber].PercentDraw = 1;  // draw all
                    //DebugMessage("(false,false) skip=" + outPointMapArr[workingQuadrantNumber].PercentSkip.ToString() + ", draw=" + outPointMapArr[workingQuadrantNumber].PercentDraw.ToString());
                    // we have not seen the stop angle we go to the next
                    continue;
                }
            }

            // ok at this point we have to set our Start and Stop Octant Flags. We cannot do this above because
            // the algo will sometimes set a start or stop flag in a 0% draw octant if it is right on the border
            for (int i = 0; i < NUM_HALF_QUADRANTS; i++)
            {
                // are we on the marked start octant?
                if(outPointMapArr[i].ThisIsTheStartOctant == true)
                {
                    // yes, we are. Is anything drawing in it
                    if(outPointMapArr[i].PercentDraw==0)
                    {
                        // no, set the octant forward to the next one
                        int nextOctantID = i + 1;
                        if (nextOctantID > 7) nextOctantID = 0;
                        outPointMapArr[i].ThisIsTheStartOctant = false;
                        outPointMapArr[nextOctantID].ThisIsTheStartOctant = true;
                    }
                }

                // are we on the marked stop octant?
                if (outPointMapArr[i].ThisIsTheStopOctant == true)
                {
                    // yes, we are. Is anything drawing in it
                    if (outPointMapArr[i].PercentDraw == 0)
                    {
                        // no, set the octant back to the previous one
                        int previousOctantID = i - 1;
                        if (previousOctantID < 0) previousOctantID = 7;
                        outPointMapArr[i].ThisIsTheStopOctant = false;
                        outPointMapArr[previousOctantID].ThisIsTheStopOctant = true;
                    }
                }

            }

            return outPointMapArr;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Fills a rectangle using a clipping region based on a graphics path
        /// </summary>
        /// <param name="startAngleInDegrees">the start angle in degrees</param>
        /// <param name="sweepAngleInDegrees">the sweep angle in degrees</param>
        /// <returns>A list of ArcPointMap objects representing the </returns>
        public static void FillRectangleUsingGraphicsPath(Graphics graphicsObj, GraphicsPath gPath, Rectangle boundingRect, SolidBrush fillBrush)
        {
            if (graphicsObj == null) return;
            if (gPath == null) return;
            if (fillBrush == null) return;
            if (boundingRect.Width <= 0) return;
            if (boundingRect.Height <= 0) return;

            // Fill rectangle using graphics path as a clip region
            graphicsObj.SetClip(gPath);
            graphicsObj.FillRectangle(fillBrush, boundingRect);
            // we MUST remove the clipping region
            graphicsObj.ResetClip();

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a sweep angle to a clockwise sweep angle.
        /// </summary>
        /// <param name="isClockwise">if true we are already clockwise, false we are not</param>
        /// <param name="sweepAngleInDegrees">the sweep angle in degrees</param>
        /// <returns>the sweep angle converted to clockwise</returns>
        public static float ConvertSweepAngleToClockwise(bool isClockwise, float sweepAngleInDegrees)
        {
            if (isClockwise == true) return sweepAngleInDegrees;
            else return (360 - sweepAngleInDegrees) % 360;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a sweep angle to a counter clockwise sweep angle.
        /// </summary>
        /// <param name="isClockwise">if true we are already clockwise, false we are not</param>
        /// <param name="sweepAngleInDegrees">the sweep angle in degrees</param>
        /// <returns>the sweep angle converted to clockwise</returns>
        public static float ConvertSweepAngleToCounterClockwise(bool isClockwise, float sweepAngleInDegrees)
        {
            if (isClockwise == false) return sweepAngleInDegrees;
            else return (360 - sweepAngleInDegrees) % 360;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If we have two concentric circles (or arcs) what is the maximum chord 
        /// length which can fit between the inner and outer one. This is needed
        /// when filling in the between them so we can detect if we have too long a fill
        /// line
        /// </summary>
        /// <param name="outerRadius">the radius of the outer circle</param>
        /// <param name="innerRadius">the radius of the inner circle</param>
        /// <returns>the sweep angle converted to clockwise</returns>
        public static float CalcMaxInnerChordForTwoConcentricCircles(float outerRadius, float innerRadius)
        {
            // this is just pythagoras theorm, we multiply by 2 to get the full chord length
            return (float)(2 * (Math.Sqrt((outerRadius * outerRadius) - (innerRadius * innerRadius))));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a RectangleF in Gerber coords to the same RectF in screen coords
        /// line
        /// </summary>
        /// <param name="rectFIn">the rectF to convert</param>
        /// <param name="stateMachine">the state machine</param>
        /// <returns>the converted RectF</returns>
        public static RectangleF ConvertRectFToScreenCoordinates(RectangleF rectFIn, GerberFileStateMachine stateMachine)
        {
            // it is a simple conversion but we do it often enough it is worth factoring it out
            return new RectangleF(rectFIn.X * stateMachine.IsoPlotPointsPerAppUnit, rectFIn.Y * stateMachine.IsoPlotPointsPerAppUnit, rectFIn.Width * stateMachine.IsoPlotPointsPerAppUnit, rectFIn.Height * stateMachine.IsoPlotPointsPerAppUnit);
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Rotates a point around another point by a specified number of degrees. 
        /// Adapted from generic-user's answer on 
        /// https://stackoverflow.com/questions/2259476/rotating-a-point-about-another-point-2d
        /// </summary>
        /// <param name="centerPoint">the center point</param>
        /// <param name="degreesRotation">the degrees rotation</param>
        /// <param name="pointToRotate">the point to rotate</param>
        /// <returns>the converted RectF</returns>
        public static PointF RotatePointAboutPointByDegrees(PointF centerPoint, float degreesRotation, PointF pointToRotate)
        {
            double angleInRadians = MiscGraphicsUtils.DegreesToRadians(degreesRotation);
            double s = Math.Sin(angleInRadians);
            double c = Math.Cos(angleInRadians);
            // translate point back to origin:
            double pX = pointToRotate.X - centerPoint.X;
            double pY = pointToRotate.Y - centerPoint.Y;
            // rotate point
            double Xnew = pX * c - pY * s;
            double Ynew = pX * s + pY * c;
            // translate point back:
            pX = Xnew + centerPoint.X;
            pY = Ynew + centerPoint.Y;
            return new PointF((float)pX, (float)pY);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if a point is on a line, to within a specified number of decimal places
        /// Adapted from user2571999's answer on 
        /// https://stackoverflow.com/questions/7050186/find-if-point-lies-on-line-segment
        /// </summary>
        /// <param name="lineX0">the line x0 point</param>
        /// <param name="lineY0">the line Y0 point</param>
        /// <param name="lineX1">the line x1 point</param>
        /// <param name="lineY1">the line Y1 point</param>
        /// <param name="ptX">the point x coord</param>
        /// <param name="ptY">the point y coord</param>
        /// <param name="numDecimals">the number of decimals to round to when checking</param>
        /// <returns>true is on line, false is not</returns>
        public static bool IsPointOnLine(int ptX, int ptY, int lineX0, int lineY0, int lineX1, int lineY1, int numDecimals)
        {
            // Find the distance of point P from both the line end points A, B. If AB = AP + PB, then P lies on the line segment AB.
            double AB = Math.Sqrt((lineX1 - lineX0) * (lineX1 - lineX0) + (lineY1 - lineY0) * (lineY1 - lineY0));
            double AP = Math.Sqrt((ptX - lineX0) * (ptX - lineX0) + (ptY - lineY0) * (ptY - lineY0));
            double PB = Math.Sqrt((lineX1 - ptX) * (lineX1 - ptX) + (lineY1 - ptY) * (lineY1 - ptY));

            // test, with rounding
            if (Math.Round(AB,numDecimals) == Math.Round((AP + PB),numDecimals)) return true;
            return false;
        }
    }
}

