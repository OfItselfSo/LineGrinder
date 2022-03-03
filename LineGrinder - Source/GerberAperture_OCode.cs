using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LineGrinder
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A base class describing a Gerber Aperture: OBRound
    /// </summary>
    public class GerberAperture_OCode : GerberAperture_Base
    {
        // These values are the decimal scaled values from the DCode itself. They
        // are not yet scaled to plot coordinates.
        private float xAxisDimension = 0;
        private float yAxisDimension = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberAperture_OCode() : base(ApertureTypeEnum.APERTURETYPE_OVAL)
        {

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates maximum width of the aperture in any direction
        /// from a point. 
        /// </summary>
        public override float GetApertureDimension()
        {
            // the incremental dimension for an ellipse is the larger of the
            // two axis dimensions 
            if (XAxisDimension > YAxisDimension) return XAxisDimension;
            else return YAxisDimension;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the xAxisDimension of the oval. There is no set accessor. This is 
        /// done in construction.
        /// </summary>
        public float XAxisDimension
        {
            get
            {
                return xAxisDimension;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the yAxisDimension of the oval. There is no set accessor. This is 
        /// done in construction.
        /// </summary>
        public float YAxisDimension
        {
            get
            {
                return yAxisDimension;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the width a pen should be for a specific angle.  The width
        /// is returned in pixel units which means all dimensions are scaled up
        /// by 
        /// </summary>
        /// <param name="stateMachine">the state machine with gerber file details</param>
        /// <param name="workingAngle">the angle the aperture will move at in degrees</param>
        public override int GetPenWidthForAngle(GerberFileStateMachine stateMachine, int workingAngle)
        {
            float retVal;
            float workingAngleInRadians;

            // the formula for this is 1/SQRT(((Cos(theta)**2)/(a^2)) + ((sin(theta)**2)/(b^2)) 
            // a is the length of the major axis in the xdirection, b is the axis in the y direction
            float a2 = xAxisDimension * xAxisDimension;
            float b2 = yAxisDimension * yAxisDimension;
            workingAngleInRadians = (float)(((float)workingAngle) * Math.PI / 180);
            float cosTheta = (float)Math.Cos(workingAngleInRadians);
            float sinTheta = (float)Math.Sin(workingAngleInRadians);
            retVal = (float)(1 / Math.Sqrt((cosTheta * cosTheta) / b2 + (sinTheta * sinTheta) / a2));
            if (retVal <= 0) return 1;
            else return (int)Math.Round((retVal * stateMachine.IsoPlotPointsPerAppUnit));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If we draw a line with an aperture it will not always have linear ends
        /// for example if the aperture is a circle it will have half circle ends. Each
        /// line that is drawn calls this to make it look right visually
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="workingPen">the pen used for the line, we get the width from this</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <param name="x2">the second x value</param>
        /// <param name="y2">the second y value</param>
        /// <returns>z success, nz fail</returns>
        public override void FixupLineEndpointsForGerberPlot(GerberFileStateMachine stateMachine, Graphics graphicsObj, Brush workingBrush, Pen workingPen, float x1, float y1, float x2, float y2)
        {
            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            if (workingPen == null) return;

            // fix up the two end points
            MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x1, y1, workingPen.Width, workingPen.Width);
            MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x2, y2, workingPen.Width, workingPen.Width);
            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If we fill a shape with the aperture at the current coordinates. This 
        /// simulates the aperture flash of a Gerber plotter
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="x1">the x center value</param>
        /// <param name="y1">the y center value</param>
        /// <returns>z success, nz fail</returns>
        public override void FlashApertureForGerberPlot(GerberFileStateMachine stateMachine, Graphics graphicsObj, Brush workingBrush, float x1, float y1)
        {
            bool ellipseIsVertical = false;
            int workingOuterDiameter = int.MinValue;
            float majorOBRoundAxis = float.NegativeInfinity;
            float minorOBRoundAxis = float.NegativeInfinity;
            float majorOBRoundAxisAdjustedForCircle = float.NegativeInfinity;
            float x1_OBRoundCompensatedEndpoint;
            float y1_OBRoundCompensatedEndpoint;
            float x2_OBRoundCompensatedEndpoint;
            float y2_OBRoundCompensatedEndpoint;
            float majorOBRoundAxisEndpointCompensation = 0;
            float minorOBRoundAxisEndpointCompensation = 0;
            int hDia;

            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            if (stateMachine == null) return;

            // NOTE: it is not possible to encode an Oval or ellipse in Gerber, you have to
            //       represent this with a line segment which has two circles at the end
            //       the total length should be equal the longest dimension. The shortest
            //       dimension will be used as the diameter of the circles.

            // NOTE: we need not deal with ellipses in here which do not have major and minor
            //       axis parallel to the X and Y axis. There is no way to represent this in
            //       Gerber code and they are usually encoded as an AM macro which is a 
            //       combination of two circles and a line - just as we are doing (see above)
            //       and for exactly the same reasons.

            // find the shortest and longest dimensions
            if (xAxisDimension > yAxisDimension)
            {
                majorOBRoundAxis = xAxisDimension;
                minorOBRoundAxis = yAxisDimension;
                ellipseIsVertical = false;
            }
            else
            {
                majorOBRoundAxis = yAxisDimension;
                minorOBRoundAxis = xAxisDimension;
                ellipseIsVertical = true;
            }
            // now figure out the length of the line between the circles
            majorOBRoundAxisAdjustedForCircle = majorOBRoundAxis - minorOBRoundAxis;
            // sanity checks
            if (majorOBRoundAxisAdjustedForCircle < 0)
            {
                // what the hell? we got problems
                throw new Exception("Cannot draw OBRound adjusted axis less than zero.");
            }
            else if (majorOBRoundAxisAdjustedForCircle == 0)
            {
                // we must be dealing with a circle, just draw one
                workingOuterDiameter = (int)Math.Round((minorOBRoundAxis * stateMachine.IsoPlotPointsPerAppUnit));
                // do we have an internal hole?
                if (HoleDiameter > 0)
                {
                    // yes we do
                    hDia = (int)Math.Round((this.HoleDiameter * stateMachine.IsoPlotPointsPerAppUnit));
                    MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x1, y1, workingOuterDiameter, workingOuterDiameter, hDia);
                }
                else
                {
                    MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x1, y1, workingOuterDiameter, workingOuterDiameter);
                }
                return;
            }
            // now figure out the diameter of the endpoint circles
            workingOuterDiameter = (int)Math.Round((minorOBRoundAxis * stateMachine.IsoPlotPointsPerAppUnit));

            // calc the amount we have to add/subtract on the center point to get to the endpoints
            majorOBRoundAxisEndpointCompensation = (int)Math.Round(((majorOBRoundAxisAdjustedForCircle * stateMachine.IsoPlotPointsPerAppUnit) / 2));
            minorOBRoundAxisEndpointCompensation = (int)Math.Round(((minorOBRoundAxis * stateMachine.IsoPlotPointsPerAppUnit) / 2));

            // get the lengths of the rectangle joining the circles
            int majorOBRoundAxisLen = (int)Math.Round(majorOBRoundAxisAdjustedForCircle * stateMachine.IsoPlotPointsPerAppUnit);
            int minorOBRoundAxisLen = (int)Math.Round(minorOBRoundAxis * stateMachine.IsoPlotPointsPerAppUnit);

            // compensate the endpoints, the way we do this is dependent on whether
            // the ellipse is horizontal or vertical
            if (ellipseIsVertical == false)
            {
                x1_OBRoundCompensatedEndpoint = x1 - majorOBRoundAxisEndpointCompensation;
                x2_OBRoundCompensatedEndpoint = x1 + majorOBRoundAxisEndpointCompensation;
                y1_OBRoundCompensatedEndpoint = y1;
                y2_OBRoundCompensatedEndpoint = y1;
            }
            else
            {
                x1_OBRoundCompensatedEndpoint = x1;
                x2_OBRoundCompensatedEndpoint = x1;
                y1_OBRoundCompensatedEndpoint = y1 - majorOBRoundAxisEndpointCompensation;
                y2_OBRoundCompensatedEndpoint = y1 + majorOBRoundAxisEndpointCompensation;
            }

            // do we need to draw in the hole?
            if (HoleDiameter > 0)
            {
                // yes we do
                hDia = (int)Math.Round((this.HoleDiameter * stateMachine.IsoPlotPointsPerAppUnit));
                // draw the first circle endpoint
                MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x1_OBRoundCompensatedEndpoint, y1_OBRoundCompensatedEndpoint, workingOuterDiameter, workingOuterDiameter, hDia, x1, y1);
                // draw the second circle endpoint
                MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x2_OBRoundCompensatedEndpoint, y2_OBRoundCompensatedEndpoint, workingOuterDiameter, workingOuterDiameter, hDia, x1, y1);
                // draw the connecting rectangle between the circle centers, we already know the centerpoint (x1, y1)
                if (ellipseIsVertical == false)
                {
                    MiscGraphicsUtils.FillRectangleCenteredOnPoint(graphicsObj, workingBrush, x1, y1, minorOBRoundAxisLen, majorOBRoundAxisLen, hDia);
                }
                else
                {
                    MiscGraphicsUtils.FillRectangleCenteredOnPoint(graphicsObj, workingBrush, x1, y1, majorOBRoundAxisLen, minorOBRoundAxisLen, hDia);
                }
            }
            else
            {
                // draw the first circle endpoint
                MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x1_OBRoundCompensatedEndpoint, y1_OBRoundCompensatedEndpoint, workingOuterDiameter, workingOuterDiameter);
                // draw the second circle endpoint
                MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x2_OBRoundCompensatedEndpoint, y2_OBRoundCompensatedEndpoint, workingOuterDiameter, workingOuterDiameter);
                if (ellipseIsVertical == false)
                {
                    MiscGraphicsUtils.FillRectangleCenteredOnPoint(graphicsObj, workingBrush, x1, y1, majorOBRoundAxisLen, workingOuterDiameter);
                }
                else
                {
                    MiscGraphicsUtils.FillRectangleCenteredOnPoint(graphicsObj, workingBrush, x1, y1, workingOuterDiameter, majorOBRoundAxisLen);
                }
            }
            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If we draw a line with an aperture it will not always have linear ends
        /// for example if the aperture is a circle it will have half circle ends. Each
        /// line that is drawn calls this to make it look right visually
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        /// <param name="workingBrush">a brush to draw with</param>
        /// <param name="workingPen">the pen used for the line, we get the width from this</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <param name="x2">the second x value</param>
        /// <param name="y2">the second y value</param>
        /// <param name="xyComp">the xy compensation factor</param>
        /// <returns>z success, nz fail</returns>
        public override void FixupLineEndpointsForGCodePlot(IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int x2, int y2, int radius, int xyComp)
        {
            IsoPlotUsageTagFlagEnum usageMode = IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE;
            if (isoPlotBuilder == null) return;
            if (stateMachine == null) return;

            // set the usage mode
            if (stateMachine.GerberFileLayerPolarity == GerberLayerPolarityEnum.CLEAR)
            {
                usageMode = IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE;
            }
            else
            {
                usageMode = IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE;
            }

            // fix up the two end points
            isoPlotBuilder.DrawGSCircle(usageMode, x1, y1, radius, stateMachine.BackgroundFillModeAccordingToPolarity, stateMachine.WantClockWise);
            isoPlotBuilder.DrawGSCircle(usageMode, x2, y2, radius, stateMachine.BackgroundFillModeAccordingToPolarity, stateMachine.WantClockWise);
            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flash an aperture for a OBRound on a GCode Plot
        ///
        /// </summary>
        /// <param name="isoPlotBuilder">the builder opbject</param>
        /// <param name="stateMachine">the statemachine</param>
        /// <param name="xyComp">the xy compensation factor</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <returns>z success, nz fail</returns>
        public override void FlashApertureForGCodePlot(IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int xyComp)
        {
            int holeBuilderObjID = 0;
            int hDia;
            bool ellipseIsVertical = false;
            int workingOuterDiameter = int.MinValue;
            float majorEllipseAxis = float.NegativeInfinity;
            float minorEllipseAxis = float.NegativeInfinity;
            float majorEllipseAxisAdjustedForCircle = float.NegativeInfinity;
            int x1EllipseCompensatedCenterpoint;
            int y1EllipseCompensatedCenterpoint;
            int x2EllipseCompensatedCenterpoint;
            int y2EllipseCompensatedCenterpoint;
            float circle1StartDegrees = 90;
            float circle2StartDegrees = 270;
            float circle1SweepDegrees = 180;
            float circle2SweepDegrees = 180;
            int majorEllipseAxisEndpointCompensation = 0;

            int outerBoundingBoxLL_X = 0;
            int outerBoundingBoxLL_Y = 0;
            int outerBoundingBoxUR_X = 0;
            int outerBoundingBoxUR_Y = 0;

            if (isoPlotBuilder == null) return;
            if (stateMachine == null) return;

            // NOTE: it is not possible to encode an Oval or Ellipse in GCode, you have to
            //       represent this with a line segment which has two circles at the end
            //       the total length should be equal the longest dimension. The shortest
            //       dimension will be used as the diameter of the circles.

            // NOTE: we need not deal with ellipses in here which do not have major and minor
            //       axis parallel to the X and Y axis. There is no way to represent this in
            //       Gerber code and they are usually encoded as an AM macro which is a 
            //       combination of two circles and a line - just as we are doing (see above)
            //       and for exactly the same reasons.

            // find the shortest and longest dimensions. NOTE according to the spec OBRounds
            // cannot have a rotation. They are either Horizontal or Vertical
            if (xAxisDimension > yAxisDimension)
            {
                majorEllipseAxis = xAxisDimension;
                minorEllipseAxis = yAxisDimension;
                ellipseIsVertical = false;
            }
            else
            {
                majorEllipseAxis = yAxisDimension;
                minorEllipseAxis = xAxisDimension;
                ellipseIsVertical = true;
            }
            // now figure out the length of the line between the circles
            majorEllipseAxisAdjustedForCircle = majorEllipseAxis - minorEllipseAxis;
            // sanity checks
            if (majorEllipseAxisAdjustedForCircle < 0)
            {
                // what the hell? we got problems
                throw new Exception("Cannot draw ellipse adjusted axis less than zero.");
            }
            else if (majorEllipseAxisAdjustedForCircle == 0)
            {
                // special case, we must be dealing with a circle, just draw one
                workingOuterDiameter = (int)Math.Round((majorEllipseAxis * stateMachine.IsoPlotPointsPerAppUnit));
                // now do the flash
                GerberAperture_CCode.PerformCircularApertureFlashWithHole(isoPlotBuilder, stateMachine, x1, y1, xyComp, workingOuterDiameter, HoleDiameter);
                return;
            }

            // we are not dealing with a circle. We are dealing with an OBRound

            // now figure out the diameter of the endpoint circles
            workingOuterDiameter = (int)Math.Round((minorEllipseAxis * stateMachine.IsoPlotPointsPerAppUnit));
            // calc the amount we have to add/subtract on the center point to get to the 
            // endpoints
            majorEllipseAxisEndpointCompensation = (int)Math.Round(((majorEllipseAxisAdjustedForCircle * stateMachine.IsoPlotPointsPerAppUnit) / 2));
            majorEllipseAxisEndpointCompensation += (xyComp / 2);

            // compensate the endpoints, the way we do this is dependent on whether
            // the ellipse is horizontal or vertical. NOTE according to the spec OBRounds
            // cannot have a rotation. They are either Horizontal or Vertical
            if (ellipseIsVertical == false)
            {
                x1EllipseCompensatedCenterpoint = x1 - majorEllipseAxisEndpointCompensation;
                x2EllipseCompensatedCenterpoint = x1 + majorEllipseAxisEndpointCompensation;
                y1EllipseCompensatedCenterpoint = y1;
                y2EllipseCompensatedCenterpoint = y1;
                circle1StartDegrees = 90;
                circle1SweepDegrees = 180;
                circle2StartDegrees = 270;
                circle2SweepDegrees = 180;
            }
            else
            {
                x1EllipseCompensatedCenterpoint = x1;
                x2EllipseCompensatedCenterpoint = x1;
                y1EllipseCompensatedCenterpoint = y1 - majorEllipseAxisEndpointCompensation;
                y2EllipseCompensatedCenterpoint = y1 + majorEllipseAxisEndpointCompensation;
                circle1StartDegrees = 180;
                circle1SweepDegrees = 180;
                circle2StartDegrees = 0;
                circle2SweepDegrees = 180;
            }

            // calculate the endpoints of the wide line rectangle. 
            MiscGraphicsUtils.GetWideLineEndPoints(x1EllipseCompensatedCenterpoint, y1EllipseCompensatedCenterpoint, x2EllipseCompensatedCenterpoint, y2EllipseCompensatedCenterpoint, (workingOuterDiameter + xyComp), out Point ptLL, out Point ptUL, out Point ptUR, out Point ptLR);
            // now set the dimensions of the bounding box
            if (ellipseIsVertical == false)
            {
                outerBoundingBoxLL_X = ptLL.X - x1EllipseCompensatedCenterpoint;
                outerBoundingBoxLL_Y = ptLL.Y;
                outerBoundingBoxUR_X = ptUR.X + x2EllipseCompensatedCenterpoint;
                outerBoundingBoxUR_Y = ptUR.Y;
            }
            else
            {
                outerBoundingBoxLL_X = ptLL.X;
                outerBoundingBoxLL_Y = ptLL.Y - y1EllipseCompensatedCenterpoint;
                outerBoundingBoxUR_X = ptUR.X;
                outerBoundingBoxUR_Y = ptUR.Y + y2EllipseCompensatedCenterpoint;
            }

            // now we draw according to the polarity

            if (stateMachine.BackgroundFillModeAccordingToPolarity == GSFillModeEnum.FillMode_BACKGROUND)
            {
                // draw the first arc
                int circle1BuilderID = isoPlotBuilder.DrawGSContourArc(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, x1EllipseCompensatedCenterpoint, y1EllipseCompensatedCenterpoint, circle1StartDegrees, circle1SweepDegrees, ((workingOuterDiameter + xyComp) / 2), false, true, false);
                // draw the second arc
                int circle2BuilderID = isoPlotBuilder.DrawGSContourArc(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, x2EllipseCompensatedCenterpoint, y2EllipseCompensatedCenterpoint, circle2StartDegrees, circle2SweepDegrees, ((workingOuterDiameter + xyComp) / 2), false, true, false);
                // draw in the top line
                int line1BuilderID = isoPlotBuilder.DrawGSLineContourLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, ptUL.X, ptUL.Y, ptUR.X, ptUR.Y);
                // draw in the bottom line
                int line2BuilderID = isoPlotBuilder.DrawGSLineContourLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, ptLL.X, ptLL.Y, ptLR.X, ptLR.Y);

                // create a list of the builder IDs used
                List<int> builderIDList = new List<int>();
                builderIDList.Add(circle1BuilderID);
                builderIDList.Add(circle2BuilderID);
                builderIDList.Add(line1BuilderID);
                builderIDList.Add(line2BuilderID);

                // now background fill the complex object, note how we only fill with the circle1BuilderID, This will be important when filling in the hole
                isoPlotBuilder.BackgroundFillGSByBoundaryComplexVert(builderIDList, circle1BuilderID, outerBoundingBoxLL_X, outerBoundingBoxLL_Y, outerBoundingBoxUR_X, outerBoundingBoxUR_Y, -1);

                // do we need to do a hole? 
                if (HoleDiameter > 0)
                {
                    // yes we do
                    hDia = (int)Math.Round((HoleDiameter * stateMachine.IsoPlotPointsPerAppUnit));
                    int holeRadius = (hDia - xyComp) / 2;
                    if (holeRadius > 0)
                    {
                        // draw the circle for the hole 
                        holeBuilderObjID = isoPlotBuilder.DrawGSCircle(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, x1, y1, holeRadius, GSFillModeEnum.FillMode_BACKGROUND, stateMachine.WantClockWise);

                        // There is more going on here than is immediately obvious
                        // we want the inside of the hole to not be filled with isocells from the holeBuilderObjID
                        // we could just have not filled it and only placed edge isocells
                        // but this causes complications on the erase. It is better to fill it then remove the outer
                        // object then remove the background only pixels of the holeBuilderObjID. This makes sure we 
                        // get all of the outer objects isocells rather than relying on hitting the edge to 
                        // figure out where to start. 

                        // remove everything belonging to the obRoundObj we just drew, note we only filled with circle1BuilderID above so that is all we need to remove
                        isoPlotBuilder.EraseABuilderIDFromRegionUsingABuilderID(holeBuilderObjID, circle1BuilderID, x1 - holeRadius - 1, y1 - holeRadius - 1, x1 + holeRadius + 1, y1 + holeRadius + 1);

                        // remove all background only pixels belonging to the hole circle leaving just the edge pixels
                        isoPlotBuilder.EraseBackgroundOnlyIsoCellsByBuilderID(holeBuilderObjID, x1 - holeRadius - 1, y1 - holeRadius - 1, x1 + holeRadius + 1, y1 + holeRadius + 1);
                    }
                }
            } // bottom of if (stateMachine.BackgroundFillModeAccordingToPolarity == GSFillModeEnum.FillMode_BACKGROUND)
            else if (stateMachine.BackgroundFillModeAccordingToPolarity == GSFillModeEnum.FillMode_ERASE)
            {
                // we have to decide whether we are drawing a hole or not. In Clear Polarity holes do not come in as solid DARK spot like you
                // think they would. Instead they just "do nothing" on the area the contents below them remain there and there is no fill or erase

                if (HoleDiameter > 0)
                {
                    // we are dealing with a erase flash with a transparent hole

                    // draw the first arc
                    int circle1BuilderID = isoPlotBuilder.DrawGSContourArc(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x1EllipseCompensatedCenterpoint, y1EllipseCompensatedCenterpoint, circle1StartDegrees, circle1SweepDegrees, ((workingOuterDiameter + xyComp) / 2), false, true, false);
                    // draw the second arc
                    int circle2BuilderID = isoPlotBuilder.DrawGSContourArc(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x2EllipseCompensatedCenterpoint, y2EllipseCompensatedCenterpoint, circle2StartDegrees, circle2SweepDegrees, ((workingOuterDiameter + xyComp) / 2), false, true, false);
                    // draw in the top line
                    int line1BuilderID = isoPlotBuilder.DrawGSLineContourLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, ptUL.X, ptUL.Y, ptUR.X, ptUR.Y);
                    // draw in the bottom line
                    int line2BuilderID = isoPlotBuilder.DrawGSLineContourLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, ptLL.X, ptLL.Y, ptLR.X, ptLR.Y);
                   
                    // create a list of the builder IDs used
                    List<int> builderIDList = new List<int>();
                    builderIDList.Add(circle1BuilderID);
                    builderIDList.Add(circle2BuilderID);
                    builderIDList.Add(line1BuilderID);
                    builderIDList.Add(line2BuilderID);

                    // note that at this point we effectively have an invert edge OBRound with FillMode_NONE

                    // now do the hole we use invert edges so anything we draw over a background will get drawn, there is no fill
                    hDia = (int)Math.Round((HoleDiameter * stateMachine.IsoPlotPointsPerAppUnit));
                    int holeRadius = (hDia - xyComp) / 2;
                    if (holeRadius > 0)
                    {
                        // draw the circle for the hole using no fill. Whatever is in there is in there
                        holeBuilderObjID = isoPlotBuilder.DrawGSCircle(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x1, y1, holeRadius, GSFillModeEnum.FillMode_NONE, stateMachine.WantClockWise);
                    }
                    builderIDList.Add(holeBuilderObjID);

                    // now erase between the outer and inner circles
                    isoPlotBuilder.BackgroundFillGSByBoundaryComplexVert(builderIDList, IsoPlotObject.DEFAULT_ISOPLOTOBJECT_ID, outerBoundingBoxLL_X, outerBoundingBoxLL_Y, outerBoundingBoxUR_X, outerBoundingBoxUR_Y, -1);
                }
                else
                {
                    // draw the obRound, we use invert edges here so we draw only if it is on some other background, we still erase everything under it though
                    // draw the first arc
                    int circle1BuilderID = isoPlotBuilder.DrawGSContourArc(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x1EllipseCompensatedCenterpoint, y1EllipseCompensatedCenterpoint, circle1StartDegrees, circle1SweepDegrees, ((workingOuterDiameter + xyComp) / 2), false, true, false);
                    // draw the second arc
                    int circle2BuilderID = isoPlotBuilder.DrawGSContourArc(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x2EllipseCompensatedCenterpoint, y2EllipseCompensatedCenterpoint, circle2StartDegrees, circle2SweepDegrees, ((workingOuterDiameter + xyComp) / 2), false, true, false);
                    // draw in the top line
                    int line1BuilderID = isoPlotBuilder.DrawGSLineContourLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, ptUL.X, ptUL.Y, ptUR.X, ptUR.Y);
                    // draw in the bottom line
                    int line2BuilderID = isoPlotBuilder.DrawGSLineContourLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, ptLL.X, ptLL.Y, ptLR.X, ptLR.Y);

                    // create a list of the builder IDs used
                    List<int> builderIDList = new List<int>();
                    builderIDList.Add(circle1BuilderID);
                    builderIDList.Add(circle2BuilderID);
                    builderIDList.Add(line1BuilderID);
                    builderIDList.Add(line2BuilderID);

                    // now erase the background
                    isoPlotBuilder.BackgroundFillGSByBoundaryComplexVert(builderIDList, IsoPlotObject.DEFAULT_ISOPLOTOBJECT_ID, outerBoundingBoxLL_X, outerBoundingBoxLL_Y, outerBoundingBoxUR_X, outerBoundingBoxUR_Y, -1);
                }
            }


            /*
                                DebugMessage("");
                                DebugMessage("majorEllipseAxis=" + (majorEllipseAxis * stateMachine.IsoPlotPointsPerAppUnit));
                                DebugMessage("minorEllipseAxis =" + (minorEllipseAxis * stateMachine.IsoPlotPointsPerAppUnit).ToString());
                                DebugMessage("y1EllipseCompensatedCenterpoint =" + (y1EllipseCompensatedCenterpoint).ToString());
                                DebugMessage("y2EllipseCompensatedCenterpoint =" + (y2EllipseCompensatedCenterpoint).ToString());
                                DebugMessage("y2EllipseCompensatedCenterpoint-y1EllipseCompensatedCenterpoint =" + (y2EllipseCompensatedCenterpoint - y1EllipseCompensatedCenterpoint).ToString());
                                DebugMessage("xyComp =" + xyComp.ToString());
                                DebugMessage("workingOuterDiameter =" + workingOuterDiameter.ToString());
                                DebugMessage("");
            */
            return;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Populate the object from an Aperture Definition pararameter
        /// </summary>
        /// <param name="adParamBlock">The AD string param block, no param, or block
        /// delimiters. Just the AD block</param>
        /// <param name="nextStartPos">the next start position after all of the ADX leading text</param>
        /// <returns>z success, nz fail</returns>
        public override int PopulateFromParameter(string adParamBlock, int nextStartPos)
        {
            float outFloat = 0;
            bool retBool;

            if (adParamBlock == null) return 100;
            if (adParamBlock.StartsWith(GerberFile.RS274_AD_CMD) == false) return 200;

            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                LogMessage("PopulateFromParameter (O) malformed adParamBlock. No data after comma");
                return 722;
            }

            // set this now
            DefinitionBlock = adParamBlock;

            // XAxisDimension x YAxisDimension <HoleDiameter> x <YAxisHoleDimension>
            retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
            if (retBool != true)
            {
                LogMessage("PopulateFromParameter (Oa) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 163;
            }
            else
            {
                // set the value now
                xAxisDimension = outFloat;
            }
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                LogMessage("PopulateFromParameter (Ob) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 1631;
            }
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                LogMessage("PopulateFromParameter (Oc) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 1631;
            }
            retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
            if (retBool != true)
            {
                LogMessage("PopulateFromParameter (Od) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 164;
            }
            else
            {
                // set the value now
                yAxisDimension = outFloat;
            }
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                // we are done
                return 0;
            }
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                // this is not an error - just means we are all done
                return 0;
            }
            // ###
            // These are optional
            // ###
            retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
            if (retBool != true)
            {
                LogMessage("PopulateFromParameter (Oe) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 165;
            }
            else
            {
                // set the value now
                HoleDiameter = outFloat;
            }
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                // this is not an error - just means we are all done
                return 0;
            }

            return 0;

        }

    }
}

