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
    /// A base class describing a Gerber Aperture: Circle
    /// </summary>
    public class GerberAperture_CCode : GerberAperture_Base
    {
        // These values are the decimal scaled values from the DCode itself. They
        // are not yet scaled to plot coordinates.
        private float outerDiameter = 0;
        
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberAperture_CCode() : base(ApertureTypeEnum.APERTURETYPE_CIRCLE)
        {

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates maximum width of the aperture in any direction
        /// from a point. 
        /// </summary>
        public override float GetApertureDimension()
        {
            // the incremental dimension for a circle is the radius
            return OuterDiameter;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the outer diameter of the circle. There is no set accessor. This is 
        /// done in construction.
        /// </summary>
        public float OuterDiameter
        {
            get
            {
                return outerDiameter;
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
            // the aperture of a circle for any angle is just the diameter
            return (int)Math.Round((this.outerDiameter * stateMachine.IsoPlotPointsPerAppUnit));
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
            int hDia;
            if (graphicsObj == null) return;
            if (workingBrush == null) return;
            if (stateMachine == null) return;

            int circleDiameter = (int)Math.Round((this.outerDiameter * stateMachine.IsoPlotPointsPerAppUnit));
            // do we need to draw in the hole?
            if (HoleDiameter > 0)
            {
                // yes we do, do it this way
                hDia = (int)Math.Round((this.HoleDiameter * stateMachine.IsoPlotPointsPerAppUnit));
                MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x1, y1, circleDiameter, circleDiameter, hDia);
            }
            else
            {
                // no we do not. Do it this way
                MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x1, y1, circleDiameter, circleDiameter);
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
        /// Flash an aperture for a Circle on a GCode Plot
        /// </summary>
        /// <param name="isoPlotBuilder">the builder opbject</param>
        /// <param name="stateMachine">the statemachine</param>
        /// <param name="xyComp">the xy compensation factor</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <returns>z success, nz fail</returns>
        public override void FlashApertureForGCodePlot(IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int xyComp)
        {
            int workingOuterDiameter = int.MinValue;

            if (isoPlotBuilder == null) return;
            if (stateMachine == null) return;

            // figure out the diameter
            workingOuterDiameter = (int)Math.Round((outerDiameter * stateMachine.IsoPlotPointsPerAppUnit));
            // now do the flash
            PerformCircularApertureFlashWithHole(isoPlotBuilder, stateMachine, x1, y1, xyComp, workingOuterDiameter, HoleDiameter);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Perform a circular aperture flash. This is a static call so other things can call it 
        /// if they need to do so.
        /// </summary>
        /// <param name="isoPlotBuilder">the builder opbject</param>
        /// <param name="stateMachine">the statemachine</param>
        /// <param name="xyComp">the xy compensation factor</param>
        /// <param name="x1">the first x value</param>
        /// <param name="y1">the first y value</param>
        /// <returns>z success, nz fail</returns>
        public static void PerformCircularApertureFlashWithHole(IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, int x1, int y1, int xyComp, int workingOuterDiameter, float holeDia)
        {
            int hDia;
            int holeRadius = 0;
            int builderID = 0;
            int holeBuilderObjID = 0;

            if (stateMachine.BackgroundFillModeAccordingToPolarity == GSFillModeEnum.FillMode_BACKGROUND)
            {
                // draw the circle
                builderID = isoPlotBuilder.DrawGSCircle(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, x1, y1, ((workingOuterDiameter + xyComp) / 2), GSFillModeEnum.FillMode_BACKGROUND, stateMachine.WantClockWise);
                // do we need to do a hole? 
                if (holeDia > 0)
                {
                    // yes we do
                    hDia = (int)Math.Round((holeDia * stateMachine.IsoPlotPointsPerAppUnit));
                    holeRadius = (hDia - xyComp) / 2;
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

                        // remove everything belonging to the outer circle from the circle we just drew, this
                        // leaves anything else which might be in there in there
                        isoPlotBuilder.EraseABuilderIDFromRegionUsingABuilderID(holeBuilderObjID, builderID, x1 - holeRadius - 1, y1 - holeRadius - 1, x1 + holeRadius + 1, y1 + holeRadius + 1);

                        // remove all background only pixels belonging to the hole circle leaving just the edge pixels
                        isoPlotBuilder.EraseBackgroundOnlyIsoCellsByBuilderID(holeBuilderObjID, x1 - holeRadius - 1, y1 - holeRadius - 1, x1 + holeRadius + 1, y1 + holeRadius + 1);
                    }
                }
            }
            else if (stateMachine.BackgroundFillModeAccordingToPolarity == GSFillModeEnum.FillMode_ERASE)
            {
                // we have to decide whether we are drawing a hole or not. In Clear Polarity holes do not come in as solid DARK spot like you
                // think they would. Instead they just "do nothing" on the area the contents below them remain there and there is no fill or erase

                if (holeDia > 0)
                {
                    // we are dealing with a erase flash with a hole

                    // draw the circle, we use invert edges here so we draw the circle only if it is on some other background, use no fill. We will erase between it and the hole later
                    int outerRadius = (workingOuterDiameter + xyComp) / 2;
                    builderID = isoPlotBuilder.DrawGSCircle(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x1, y1, outerRadius, GSFillModeEnum.FillMode_NONE, stateMachine.WantClockWise);
                    
                    // now do the hole we use invert edges so anything we draw over a background will get drawn, there is no fill
                    hDia = (int)Math.Round((holeDia * stateMachine.IsoPlotPointsPerAppUnit));
                    holeRadius = (hDia - xyComp) / 2;
                    if (holeRadius > 0)
                    {
                        // draw the circle for the hole using no fill. Whatever is in there is in there
                        holeBuilderObjID = isoPlotBuilder.DrawGSCircle(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x1, y1, holeRadius, GSFillModeEnum.FillMode_NONE, stateMachine.WantClockWise);
                    }

                    // now erase between the outer and inner circles
                    // we are dealing with a simple erase flash, no hole
                    List<int> builderIDList = new List<int>();
                    builderIDList.Add(builderID);
                    builderIDList.Add(holeBuilderObjID);
                    isoPlotBuilder.BackgroundFillGSByBoundaryComplexVert(builderIDList, IsoPlotObject.DEFAULT_ISOPLOTOBJECT_ID, x1 - outerRadius - 1, y1 - outerRadius - 1, x1 + outerRadius + 1, y1 + outerRadius + 1, -1);

                }
                else
                {
                    // draw the circle, we use invert edges here so we draw only if it is on some other background, we still erase everything under it though
                    int outerRadius = (workingOuterDiameter + xyComp) / 2;
                    builderID = isoPlotBuilder.DrawGSCircle(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x1, y1, outerRadius, GSFillModeEnum.FillMode_ERASE, stateMachine.WantClockWise);
                }
            }

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
                LogMessage("PopulateFromParameter (C) malformed adParamBlock. No data after comma");
                return 722;
            }

            // set this now
            DefinitionBlock = adParamBlock;

            // od x <XAxisHoleDimension> x <YAxisHoleDimension>
            retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
            if (retBool != true)
            {
                LogMessage("PopulateFromParameter (Ca) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 133;
            }
            else
            {
                // set the value now
                outerDiameter = outFloat;
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
                LogMessage("PopulateFromParameter (Ce) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 135;
            }
            else
            {
                // set the value now
                HoleDiameter= outFloat;
            }
 
            return 0;
        }


    }
}

