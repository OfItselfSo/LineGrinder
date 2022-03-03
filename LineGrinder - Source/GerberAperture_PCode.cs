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
    /// A base class describing a Gerber Aperture: Polygon
    /// </summary>
    public class GerberAperture_PCode : GerberAperture_Base
    {
        // These values are the decimal scaled values from the DCode itself. They
        // are not yet scaled to plot coordinates.
        private float outsideDimension = 0;
        private int numSides = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberAperture_PCode() : base(ApertureTypeEnum.APERTURETYPE_POLYGON)
        {

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates maximum width of the aperture in any direction
        /// from a point. 
        /// </summary>
        public override float GetApertureDimension()
        {
            // the incremental dimension for an polygon is outsideDimension
            return OutsideDimension;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the outer diameter of the polygon. There is no set accessor. This is 
        /// done in construction.
        /// </summary>
        public float OutsideDimension
        {
            get
            {
                return outsideDimension;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the numSides of the polygon. There is no set accessor. This is 
        /// done in construction.
        /// </summary>
        public int NumSides
        {
            get
            {
                return numSides;
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
            // note we are not doing this quite correctly. We treat the polygon as a circle and 
            // just use the outer diameter. This is slightly too large for some angles but as the number 
            // of sides increases, this error reduces considerably. The true math for calculating this
            // for an arbitrary number of sides and/or rotation angles is very complicated.

            // the aperture of a circle for any angle is just the diameter
            return (int)Math.Round((OutsideDimension * stateMachine.IsoPlotPointsPerAppUnit));
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
            MiscGraphicsUtils.FillPolygonCenteredOnPoint(graphicsObj, workingBrush, x1, y1, NumSides, workingPen.Width, DegreesRotation);
            MiscGraphicsUtils.FillPolygonCenteredOnPoint(graphicsObj, workingBrush, x2, y2, NumSides, workingPen.Width, DegreesRotation);
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

            // scale the outside diameter
            int circleDiameter = (int)Math.Round((OutsideDimension * stateMachine.IsoPlotPointsPerAppUnit));
            // do we need to draw in the hole?
            if (HoleDiameter > 0)
            {
                // yes we do, do it this way
                hDia = (int)Math.Round((this.HoleDiameter * stateMachine.IsoPlotPointsPerAppUnit));
                MiscGraphicsUtils.FillPolygonCenteredOnPoint(graphicsObj, workingBrush, x1, y1, NumSides, circleDiameter, DegreesRotation, hDia);
            }
            else
            {
                // no we do not. Do it this way
                MiscGraphicsUtils.FillPolygonCenteredOnPoint(graphicsObj, workingBrush, x1, y1, NumSides, circleDiameter, DegreesRotation);
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
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flash an aperture for a Polygon on a GCode Plot
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
            if (isoPlotBuilder == null) return;
            if (stateMachine == null) return;

            if (stateMachine.BackgroundFillModeAccordingToPolarity == GSFillModeEnum.FillMode_BACKGROUND)
            {
                int workingOuterDiameter = (int)Math.Round((OutsideDimension * stateMachine.IsoPlotPointsPerAppUnit));
                int builderID = isoPlotBuilder.DrawGSPolygonOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_NORMALEDGE, x1, y1, NumSides, workingOuterDiameter + xyComp, DegreesRotation, stateMachine.BackgroundFillModeAccordingToPolarity);
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

                        // remove everything belonging to the outer circle from the circle we just drew, this
                        // leaves anything else which might be in there in there
                        isoPlotBuilder.EraseABuilderIDFromRegionUsingABuilderID(holeBuilderObjID, builderID, x1 - holeRadius - 1, y1 - holeRadius - 1, x1 + holeRadius + 1, y1 + holeRadius + 1);

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

                    int workingOuterDiameter = (int)Math.Round((OutsideDimension * stateMachine.IsoPlotPointsPerAppUnit));
                    int builderID = isoPlotBuilder.DrawGSPolygonOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x1, y1, NumSides, workingOuterDiameter + xyComp, DegreesRotation, GSFillModeEnum.FillMode_NONE);

                    // now do the hole we use invert edges so anything we draw over a background will get drawn, there is no fill
                    hDia = (int)Math.Round((HoleDiameter * stateMachine.IsoPlotPointsPerAppUnit));
                    int holeRadius = (hDia - xyComp) / 2;
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
                    // now erase the background
                    isoPlotBuilder.BackgroundFillGSByBoundaryComplexVert(builderIDList, IsoPlotObject.DEFAULT_ISOPLOTOBJECT_ID, x1 - workingOuterDiameter - 1, y1 - workingOuterDiameter - 1, x1 + workingOuterDiameter + 1, y1 + workingOuterDiameter + 1, -1);
                }
                else
                {
                    // draw the polygon, we use invert edges here so we draw only if it is on some other background, we still erase everything under it though
                    int workingOuterDiameter = (int)Math.Round((OutsideDimension * stateMachine.IsoPlotPointsPerAppUnit));
                    int builderID = isoPlotBuilder.DrawGSPolygonOutLine(IsoPlotUsageTagFlagEnum.IsoPlotUsageTagFlag_INVERTEDGE, x1, y1, NumSides, workingOuterDiameter + xyComp, DegreesRotation, GSFillModeEnum.FillMode_ERASE);
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
                LogMessage("PopulateFromParameter (P) malformed adParamBlock. No data after comma");
                return 722;
            }

            // set this now
            DefinitionBlock = adParamBlock;

            // OutsideDimension x NumSides X <degreesRotation> x <HoleDiameter> x <YAxisHoleDimension>
            retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
            if (retBool != true)
            {
                LogMessage("PopulateFromParameter (Pa) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 163;
            }
            else
            {
                // set the value now
                outsideDimension = outFloat;
            }
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                LogMessage("PopulateFromParameter (Pb) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 1631;
            }
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                LogMessage("PopulateFromParameter (Pc) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 1631;
            }
            retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
            if (retBool != true)
            {
                LogMessage("PopulateFromParameter (Pd) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 164;
            }
            else
            {
                int workingNumSides = (int)outFloat;
                if (workingNumSides<3)
                {
                    LogMessage("PopulateFromParameter (Pe) failed numSides < 3 is "+workingNumSides.ToString());
                    return 165;

                }
                // set the value now
                numSides = workingNumSides;
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
                LogMessage("PopulateFromParameter (Pe) failed on call to ParseNumberFromString_TillNonDigit_RetFloat");
                return 166;
            }
            else
            {
                // set the value now
                DegreesRotation = outFloat;
            }
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                // this is not an error - just means we are all done
                return 0;
            }
            nextStartPos = GerberParseUtils.FindCharacterReturnNextPos(adParamBlock, 'X', nextStartPos);
            if ((nextStartPos < 0) || (nextStartPos > adParamBlock.Length))
            {
                // this is not an error - just means we are all done
                return 0;
            }
            retBool = GerberParseUtils.ParseNumberFromString_TillNonDigit_RetFloat(adParamBlock, nextStartPos, ref outFloat, ref nextStartPos);
            if (retBool != true)
            {
                // this is not an error - just means we are all done
                return 0;
            }
            else
            {
                // set the value now
                HoleDiameter = outFloat;
            }

            return 0;
        }

    }
}

