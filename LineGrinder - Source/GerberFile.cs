using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OISCommon;
using System.ComponentModel;

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
    /// A class to encapsulate a Gerber file in an easily manipulable manner
    /// </summary>
    public class GerberFile : OISObjBase
    {
        // this is the path and name of the source Gerber file
        private string gerberFilePathAndName = null;

        // this is the current file source
        private List<GerberLine> sourceLines = new List<GerberLine>();

        // this is the list of lines for each command. Some can be multi line
        private List<GerberCommandSourceLines> gerberCommandLineList = new List<GerberCommandSourceLines>();

        // our state machine. Each line of a Gerber File can assume many things based
        // on the state of the previously executed commands
        GerberFileStateMachine stateMachine = new GerberFileStateMachine();

        public const string RS274_AB_CMD = @"AB";
        public const string RS274_AD_CMD = @"AD";
        public const string RS274_AM_CMD = @"AM";
        public const string RS274_D_CMD = @"D";
        public const string RS274_FS_CMD = @"FS";

        public const string RS274_G01_CMD = @"G01";
        public const string RS274_G02_CMD = @"G02";
        public const string RS274_G03_CMD = @"G03";
        public const string RS274_G04_CMD = @"G04";
        public const string RS274_G36_CMD = @"G36";
        public const string RS274_G37_CMD = @"G37";
        public const string RS274_G54_CMD = @"G54";
        public const string RS274_G70_CMD = @"G70";
        public const string RS274_G71_CMD = @"G71";
        public const string RS274_G74_CMD = @"G74";
        public const string RS274_G75_CMD = @"G75";
        public const string RS274_G90_CMD = @"G90";
        public const string RS274_G91_CMD = @"G91";

        public const string RS274_IN_CMD = @"IN";
        public const string RS274_IP_CMD = @"IP";
        public const string RS274_LM_CMD = @"LM";
        public const string RS274_LN_CMD = @"LN";
        public const string RS274_LP_CMD = @"LP";
        public const string RS274_LR_CMD = @"LR";
        public const string RS274_LS_CMD = @"LS";

        public const string RS274_MO_CMD = @"MO";

        public const string RS274_M00_CMD = @"M00";
        public const string RS274_M01_CMD = @"M01";
        public const string RS274_M02_CMD = @"M02";
        public const string RS274_M30_CMD = @"M30";

        public const string RS274_OF_CMD = @"OF";
        public const string RS274_SF_CMD = @"SF";
        public const string RS274_SR_CMD = @"SR";
        public const string RS274_TA_CMD = @"TA";
        public const string RS274_TD_CMD = @"TD";
        public const string RS274_TF_CMD = @"TF";
        public const string RS274_TO_CMD = @"TO";
        public const string RS274_X_CMD = @"X";
        public const string RS274_Y_CMD = @"Y";
        public const string RS274_SFA1B1_CMD = @"SFA1B1";

        public const string RS274_CMD_DELIMITER = @"%";
        public const char RS274_CMD_DELIMITER_CHAR = '%';

        public const string RS274_BLOCK_TERMINATOR = @"*";
        public const char RS274_BLOCK_TERMINATOR_CHAR = '*';

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value derived from the gerber file or gCode file although possibly
        //       it may be origin compensated

        // These are the values we ADD to the existing X and Y coords from the DCodes
        // in order to set the origin approximately at zero
        private float plotXCoordOriginAdjust = 0;
        private float plotYCoordOriginAdjust = 0;

        // we track these as we add lines in order to build an appropriately sized plot
        private float minDCodeXCoord = float.MaxValue;
        private float minDCodeYCoord = float.MaxValue;
        private float maxDCodeXCoord = float.MinValue;
        private float maxDCodeYCoord = float.MinValue;
        private float midDCodeXCoord = 0;      // will be set by refPins if refPins exist otherwise set from max/min
        private float midDCodeYCoord = 0;      // will be set by refPins if refPins exist otherwise set from max/min

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public GerberFile()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets whether the file is populated. We assume it is populated if there
        /// is at least one source line
        /// </summary>
        public bool IsPopulated
        {
            get
            {
                if (SourceLines.Count > 0) return true;
                else return false;;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/sets the current gerber source files path and name
        /// </summary>
        public string GerberFilePathAndName
        {
            get
            {
                return gerberFilePathAndName;
            }
            set
            {
                gerberFilePathAndName = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs fixups after the file has been opened and read to 
        /// adjust things so that code originally intended for a plotter works
        /// better in an isolation mode environment.
        /// </summary>
        /// <param name="errStr">we return the error string in here</param>
        /// <returns>true all ok, false a failure was detected</returns>
        public bool PerformOpenPostProcessingFixups(out string errStr)
        {
            errStr = "";

            float lastDCodeXCoord = 0;
            float lastDCodeYCoord = 0;

            bool contourModeActive = false;
            List<GerberContourLineContainer> contourList = new List<GerberContourLineContainer>();

            // get all of the DCode lines into the list
            foreach (GerberLine gLineObj in SourceLines)
            {
                if ((gLineObj is GerberLine_G36Code) == true) contourModeActive = true;
                if ((gLineObj is GerberLine_G37Code) == true) contourModeActive = false;
                    
                // we are in a D code
                if((gLineObj is GerberLine_DCode) == true)
                {
                    // set this now for convenience
                    GerberLine_DCode dCodeObj = (GerberLine_DCode)gLineObj;

                    // if it is just an aperture define ignore it
                    if (dCodeObj.DCode > 3) continue;

                    // make sure we fill in defaults if necessary
                    dCodeObj.SetDefaultCoordsIfRequired(lastDCodeXCoord, lastDCodeYCoord);

                    if (contourModeActive == true)
                    {
                        // are we a line draw DCode?
                        if (dCodeObj.DCode == 1)
                        {
                            // if we get here we have a line D code inside a contourMode statement
                            contourList.Add(new GerberContourLineContainer(lastDCodeXCoord, lastDCodeYCoord, dCodeObj.DCodeCoord_X, dCodeObj.DCodeCoord_Y, dCodeObj));
                        }
                        else if (dCodeObj.DCode == 2)
                        {
                            // are we a D02 move
                            // coincident lines only matter to each other within a D02 section so check now
                            MarkCoincidentLinesInContourList(contourList);
                            // reset now
                            contourList = new List<GerberContourLineContainer>();
                        }
                    }

                    // now update the last coords 
                    lastDCodeXCoord = dCodeObj.DCodeCoord_X;
                    lastDCodeYCoord = dCodeObj.DCodeCoord_Y;
                }
            } // bottom of foreach (GerberLine gLineObj in SourceLines)

            // at this point we may have DCodes in a contour list which have not been processed 
            // as a final D02 is not required. we check what we have now
            MarkCoincidentLinesInContourList(contourList);

            errStr = "";
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Revisits the gerber lines and figures out the minimum and maximum coords
        /// each will use. Will set the max/min values in this gerber file
        /// </summary>
        public void FindAllMinMaxPlotSizesAndSetInGerberFile()
        {
            PointF ptLL = new PointF(0, 0);
            PointF ptUR = new PointF(0, 0);

            // create a dummy statemachine We can only get a lot of the information from the DCodes
            // by simulating a run. They use the results of the previous DCode a lot
            GerberFileStateMachine workingStateMachine = new GerberFileStateMachine();
            workingStateMachine.MacroCollection = stateMachine.MacroCollection;
            workingStateMachine.ApertureCollection = stateMachine.ApertureCollection;

            // get all of the DCode lines into the list
            foreach (GerberLine gLineObj in SourceLines)
            {
                // Check what kind of code we are in, we need some of these to properly set the stateMachine
                // note that we are using the workingSateMachine here
                if (gLineObj is GerberLine_G01Code)
                {
                    workingStateMachine.GerberFileInterpolationMode = GerberInterpolationModeEnum.INTERPOLATIONMODE_LINEAR;
                    continue;
                }
                if (gLineObj is GerberLine_G02Code)
                {
                    workingStateMachine.GerberFileInterpolationMode = GerberInterpolationModeEnum.INTERPOLATIONMODE_CIRCULAR;
                    workingStateMachine.GerberFileInterpolationCircularDirectionMode = GerberInterpolationCircularDirectionModeEnum.DIRECTIONMODE_CLOCKWISE;
                    continue;
                }
                if (gLineObj is GerberLine_G03Code)
                {
                    workingStateMachine.GerberFileInterpolationMode = GerberInterpolationModeEnum.INTERPOLATIONMODE_CIRCULAR;
                    workingStateMachine.GerberFileInterpolationCircularDirectionMode = GerberInterpolationCircularDirectionModeEnum.DIRECTIONMODE_COUNTERCLOCKWISE;
                    continue;
                }
                if (gLineObj is GerberLine_AMCode)
                {
                    GerberLine_AMCode gMacroObj = (GerberLine_AMCode)gLineObj;
                    // make a new one of these
                    GerberMacroVariableArray dummyArray = new GerberMacroVariableArray();
                    // get the max dimensions from the macro. At this point the macro variable array has not been resolved so we 
                    // can only base this on the variables that are hard coded.
                    gMacroObj.GetMaxMinXAndYValuesForMacro(dummyArray, out ptUR, out ptLL);
                    RecordMaxMinXYCoords(ptUR.X, ptLL.X, ptUR.Y, ptLL.Y);
                    continue;
                }


                // we only process D Codes beyond this point
                if ((gLineObj is GerberLine_DCode) == false) continue;

                // set this now for convenience
                GerberLine_DCode dCodeObj = (GerberLine_DCode)gLineObj;
                // is it an aperture define
                if (dCodeObj.DCode > 3)
                {
                    // simulate the selection of an aperture, note we are populating the workingStateMachine from the real statemachine
                    workingStateMachine.CurrentAperture = stateMachine.ApertureCollection.GetApertureByID(dCodeObj.DCode);
                    continue;
                }

                // record the values, the DCode object knows how best to calculate these, this is not as simple as it 
                // seems because we also have to keep track of the aperture diameters, arcs and macros
                bool retBool = dCodeObj.GetMaxMinXAndYValues(workingStateMachine, workingStateMachine.LastDCodeXCoord, workingStateMachine.LastDCodeYCoord, out ptUR, out ptLL);
                if (retBool == true)
                {
                    RecordMaxMinXYCoords(ptUR.X, ptLL.X, ptUR.Y, ptLL.Y);
                    //DebugMessage("SetMinMaxPlotSizes ptURX=" + ptUR.ToString() + ", ptLL=" + ptLL.ToString());
                }

                // get the pad centerpoint list. Not all flashes will be a pad. For example macros will not
                if ((dCodeObj.DCode == 3) && (workingStateMachine.CurrentAperture.ApertureType != ApertureTypeEnum.APERTURETYPE_MACRO))
                {
                    // record these centerpoints to our list, note that we save to the statemachine but use the aperture from the working one
                    stateMachine.PadCenterPointList.Add(new GerberPad(dCodeObj.DCodeCoord_X, dCodeObj.DCodeCoord_Y, (workingStateMachine.CurrentAperture.GetApertureDimension())));
                }

                // now remember the last coords 
                workingStateMachine.LastDCodeXCoord = dCodeObj.DCodeCoord_X;
                workingStateMachine.LastDCodeYCoord = dCodeObj.DCodeCoord_Y;

            } // bottom of foreach (GerberLine gLineObj in SourceLines)
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Revisits the gerber lines and figures out the absolute X and Y offsets
        /// The coordinates in the file will have been much adjusted so as to make them
        /// all positive and near the plot 0,0 origin point. The absolute offset
        /// is how these modified points relate to the original hard coords of the
        /// gerber file. This enables us to line up the gcode in different files
        /// such as edgeCuts, drills and isoCuts etc.
        /// 
        /// NOTE: this code only works after the gerber file has been plotted. Before
        /// then we do not have enough information
        /// 
        /// </summary>
        public bool GetAbsoluteOffsets(out float absoluteOffset_X, out float absoluteOffset_Y)
        {
            absoluteOffset_X = 0;
            absoluteOffset_Y = 0;

            // run through all of the DCode lines
            foreach (GerberLine gLineObj in SourceLines)
            {
                // we only process D Codes 
                if ((gLineObj is GerberLine_DCode) == false) continue;
                // set this now for convenience
                GerberLine_DCode dCodeObj = (GerberLine_DCode)gLineObj;
                // is it an aperture define we cannot use these
                if (dCodeObj.DCode > 3) continue;

                // we use the first nonzero plottable DCode we find. The end value is always the transformation of the original DCode
                // coordinates into plot coordinates.
                if ((dCodeObj.LastPlotXCoordEnd == 0) && (dCodeObj.LastPlotYCoordEnd == 0)) continue;

                // ok we know we have one and we can figure out a transformation, scale the dcodeXY to plot coords
                float scaledOriginalDCode_X = dCodeObj.DCodeCoord_X * StateMachine.IsoPlotPointsPerAppUnit;
                float scaledOriginalDCode_Y = dCodeObj.DCodeCoord_Y * StateMachine.IsoPlotPointsPerAppUnit;

                // the difference between the what we ended up as and what we started with is indicative of the absolute offset of that 
                // point in the gerber file. These should all be the same
                absoluteOffset_X = scaledOriginalDCode_X - (float)dCodeObj.LastPlotXCoordEnd;
                absoluteOffset_Y = scaledOriginalDCode_Y - (float)dCodeObj.LastPlotYCoordEnd;

                //DebugMessage("dCodeObj.LastPlotXCoordEnd=" + dCodeObj.LastPlotXCoordEnd.ToString() + ", dCodeObj.LastPlotYCoordEnd=" + dCodeObj.LastPlotYCoordEnd.ToString());
                //DebugMessage("scaledOriginalDCode_X=" + scaledOriginalDCode_X.ToString() + ", scaledOriginalDCode_Y=" + scaledOriginalDCode_Y.ToString() + ", absoluteOffset_X=" +absoluteOffset_X.ToString() + ", absoluteOffset_Y="+ absoluteOffset_Y.ToString());
                return true;

            }

            // if we get here we failed to find an absoluet offset
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Accepts a list of contour line containers and marks all coincident lines
        /// in the container.
        /// </summary>
        /// <param name="contourList">the list of GerberContourLineContainer's</param>
        public void MarkCoincidentLinesInContourList(List<GerberContourLineContainer> contourList)
        {
            // do we have anything to check, if there is only one we cannot check that
            if (contourList.Count <= 1) return;

            //DebugMessage("MarkCoincidentLinesInContourList called");

            for (int i = 0; i < contourList.Count; i++)
            {
                // coincident lines must (according to spec) be horizontal or vertical
                if (contourList[i].IsHorizontalOrVertical() == false) continue;
                for (int j = i + 1; j < contourList.Count; j++)
                {
                    // coincident lines must (according to spec) be horizontal or vertical
                    if (contourList[j].IsHorizontalOrVertical() == false) continue;

                    // we have two lines (horiz or vert) check to see if they are coincident
                    if (contourList[i].IsCoincidentLine(contourList[j]) == true)
                    {
                        // we are a coincident lines. Mark both of these
                        contourList[i].DCodeObj.IsCoincidentLineInFillRegion = true;
                        contourList[j].DCodeObj.IsCoincidentLineInFillRegion = true;
                        //DebugMessage("Dcode="+ contourList[j].DCodeObj.DCode.ToString()+" (" +contourList[i].XStart.ToString()+","+ contourList[i].YStart.ToString() + "), ("+ contourList[i].XEnd.ToString() + "," + contourList[i].YEnd.ToString() + ")");
                    }
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs checks after the file has been opened and read to 
        /// see if it is valid and useable
        /// </summary>
        /// <param name="errStr">we return the error string in here</param>
        /// configured for</param>
        /// <returns>true all ok, false a failure was detected</returns>
        public bool PerformOpenPostProcessingChecks(out string errStr)
        {
            // check to make sure there are valid Min and Max coodinates
            if (minDCodeXCoord == float.MaxValue)
            {
                errStr = "minimum X coordinate not found in D Codes - something is wrong with this file.";
                return false;
            }
            if (minDCodeYCoord == float.MaxValue)
            {
                errStr = "minimum Y coordinate not found in D Codes - something is wrong with this file.";
                return false;
            }
            if (maxDCodeXCoord == float.MinValue)
            {
                errStr = "maximum X coordinate not found in D Codes - something is wrong with this file.";
                return false;
            }
            if (maxDCodeYCoord == float.MinValue)
            {
                errStr = "maximum Y coordinate not found in D Codes - something is wrong with this file.";
                return false;
            }
            // we have to make sure the UNITS used by the Gerber file are the 
            // same as the UNITS currently set in this application
            if (this.GerberFileUnits == ApplicationUnitsEnum.INCHES)
            {
                if (StateMachine.GerberFileManager.FileManagerUnits == ApplicationUnitsEnum.MILLIMETERS)
                {
                    errStr = "File Manager units set to millimeters and the Gerber file uses inches. This should have automatically adjusted. Please report this error.";
                    return false;
                }
            }
            else if (this.GerberFileUnits == ApplicationUnitsEnum.MILLIMETERS)
            {
                if (StateMachine.GerberFileManager.FileManagerUnits == ApplicationUnitsEnum.INCHES)
                {
                    errStr = "File Manager set to inches and the Gerber file uses millimeters. This should have automatically adjusted. Please report this error.";
                    return false;
                }
            }
            if (this.StateMachine.GerberFileCoordinateMode == GerberCoordinateModeEnum.COORDINATE_INCREMENTAL)
            {
                errStr = "Incremental coordinate mode is not supported. Please use absolute coordinates.";
                return false;
            }

            //TODO test these
            /*
             * 
                        if (SourceLines.Count ==0)
                        {
                            errStr = "PerformGerberToGCodeStep1: SourceLines.Count ==0";
                            LogMessage(errStr);
                            return 100;
                        }
                        if (plotSize.Width <= 0)
                        {
                            errStr = "PerformGerberToGCodeStep1: plotSize.Width <= 0";
                            LogMessage(errStr);
                            return 101;
                        }
                        if (plotSize.Height <= 0)
                        {
                            errStr = "PerformGerberToGCodeStep1: plotSize.Height <= 0";
                            LogMessage(errStr);
                            return 102;
                        }
                        if (isoPlotPointsPerAppUnit <= 0)
                        {
                            errStr = "PerformGerberToGCodeStep1: isoPlotPointsPerAppUnit <= 0";
                            LogMessage(errStr);
                            return 103;
                        }

             * 
             */

            errStr = "";
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Keeps a record of the max and min XY coords. 
        /// </summary>
        public void RecordMaxMinXYCoords(float maxXCoord, float minXCoord, float maxYCoord, float minYCoord)
        {

            // min values
            if (minXCoord < this.minDCodeXCoord) this.minDCodeXCoord = minXCoord;
            if (minYCoord < this.minDCodeYCoord) this.minDCodeYCoord = minYCoord;

            // max values
            if (maxXCoord > this.maxDCodeXCoord) this.maxDCodeXCoord = maxXCoord;
            if (maxYCoord > this.maxDCodeYCoord) this.maxDCodeYCoord = maxYCoord;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the max plot X Coordinate value There is no set accessor
        /// this value is derived from the max/min X coord and the origin compensation
        /// </summary>
        public float MaxPlotXCoord
        {
            get
            {
                return maxDCodeXCoord + plotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the max plot Y Coordinate value There is no set accessor
        /// this value is derived from the max/min Y coord and the origin compensation
        /// </summary>
        public float MaxPlotYCoord
        {
            get
            {
                return maxDCodeYCoord + plotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the min plot X Coordinate value There is no set accessor
        /// this value is derived from the max/min X coord and the origin compensation
        /// </summary>
        public float MinPlotXCoord
        {
            get
            {
                return minDCodeXCoord + plotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the min plot Y Coordinate value There is no set accessor
        /// this value is derived from the max/min Y coord and the origin compensation
        /// </summary>
        public float MinPlotYCoord
        {
            get
            {
                return minDCodeYCoord + plotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the mid plot X Coordinate value There is no set accessor
        /// this value is derived from the mid X coord and the origin compensation
        /// </summary>
        public float MidPlotXCoord
        {
            get
            {
                return midDCodeXCoord + plotXCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the mid plot Y Coordinate value There is no set accessor
        /// this value is derived from the mid Y coord and the origin compensation
        /// </summary>
        public float MidPlotYCoord
        {
            get
            {
                return midDCodeYCoord + plotYCoordOriginAdjust;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the min X Coordinate value
        /// </summary>
        public float MinDCodeXCoord
        {
            get
            {
                return minDCodeXCoord;
            }
            set
            {
                minDCodeXCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the min Y Coordinate value
        /// </summary>
        public float MinDCodeYCoord
        {
            get
            {
                return minDCodeYCoord;
            }
            set
            {
                minDCodeYCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the mid X Coordinate value. These are derived from the reference
        /// pins or the best guess center of the plot if refPins are not set
        /// </summary>
        public float MidDCodeXCoord
        {
            get
            {
                return midDCodeXCoord;
            }
            set
            {
                midDCodeXCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the mid Y Coordinate value. These are derived from the reference
        /// pins or the best guess center of the plot if refPins are not set
        /// </summary>
        public float MidDCodeYCoord
        {
            get
            {
                return midDCodeYCoord;
            }
            set
            {
                midDCodeYCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the max X Coordinate value
        /// </summary>
        public float MaxDCodeXCoord
        {
            get
            {
                return maxDCodeXCoord;
            }
            set
            {
                maxDCodeXCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the max Y Coordinate value
        /// </summary>
        public float MaxDCodeYCoord
        {
            get
            {
                return maxDCodeYCoord;
            }
            set
            {
                maxDCodeYCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current X Coordinate value with origin compensation
        /// </summary>
        public float ConvertXCoordToOriginCompensated(float x0)
        {
            return x0 + PlotXCoordOriginAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current Y Coordinate value with origin compensation
        /// </summary>
        public float ConvertYCoordToOriginCompensated(float y0)
        {
            return y0 + PlotYCoordOriginAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current X Coordinate value with flipping applied (if necessary)
        /// </summary>
        public float ConvertXCoordToOriginCompensatedFlipped(float xCoordToFlip)
        {
            // Just return this
            return ConvertXCoordToOriginCompensated(xCoordToFlip);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the current Y Coordinate value with flipping applied (if necessary)
        /// </summary>
        public float ConvertYCoordToOriginCompensatedFlipped(float yCoordToFlip)
        {
            // Just return this
            return ConvertYCoordToOriginCompensated(yCoordToFlip);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the currently set flip mode. There is no set this comes out of the 
        /// current file manager.
        /// </summary>
        public FlipModeEnum FlipMode
        {
            get
            {
                // this is safe to do. None of these properties return null
                if(StateMachine.GerberFileManager.OperationMode == FileManager.OperationModeEnum.BoardEdgeMill)
                {
                    return StateMachine.GerberFileManager.EdgeMillFlipMode;
                }
                else return StateMachine.GerberFileManager.IsoFlipMode;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a flag indicating the current GCode origin. There is no set this comes out of the 
        /// current file manager.
        /// </summary>
        public bool GCodeOriginAtCenter
        {
            get
            {
                // this is safe to do. None of these properties return null
                return StateMachine.GerberFileManager.GCodeOriginAtCenter;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the currently set File Units. There is no set this comes out of the 
        /// current gerber file.
        /// </summary>
        public ApplicationUnitsEnum GerberFileUnits
        {
            get
            {
                // this is safe to do. None of these properties return null
                return StateMachine.GerberFileUnits;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets state machine. Will never set or get a null value.
        /// </summary>
        public GerberFileStateMachine StateMachine
        {
            get
            {
                if (stateMachine == null) stateMachine = new GerberFileStateMachine();
                return stateMachine;
            }
            set
            {
                stateMachine = value;
                if (stateMachine == null) stateMachine = new GerberFileStateMachine();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the aperture collection. 
        /// </summary>
        public GerberApertureCollection ApertureCollection
        {
            get
            {
                return StateMachine.ApertureCollection;
            }
            set
            {
                StateMachine.ApertureCollection = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the macro collection. 
        /// </summary>
        public GerberMacroCollection MacroCollection
        {
            get
            {
                return StateMachine.MacroCollection;
            }
            set
            {
                StateMachine.MacroCollection = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Gerber file source. Will never set or get a null value.
        /// </summary>
        [BrowsableAttribute(false)]
        public List<GerberLine> SourceLines
        {
            get
            {
                if (sourceLines == null) sourceLines = new List<GerberLine>();
                return sourceLines;
            }
            set
            {
                sourceLines = value;
                if (sourceLines == null) sourceLines = new List<GerberLine>();
                LogMessage("SourceLines Set, lines =" + sourceLines.Count.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns all lines as a string array. Will never get a null value.
        /// </summary>
        public string[] GetRawSourceLinesAsArray()
        {
            List<string> retObj = new List<string>();

            // get all of the lines into the list
            foreach (GerberLine gLineObj in SourceLines)
            {
                retObj.Add(gLineObj.RawLineStr);
            }

            return retObj.ToArray();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Adds a Gerber file line to the SourceLines
        /// </summary>
        /// <param name="lineStr">The line to add</param>
        /// <param name="lineNumber">The line number</param>
        public int AddLine(string lineStr, int lineNumber)
        {
            string[] blockArray = null;
            char[] blockSplitChars = { RS274_BLOCK_TERMINATOR_CHAR };

            if (lineStr == null) return 100;

            string trimmedStr = lineStr.Trim();
            if (trimmedStr.Length == 0) return 0;

            // do we end with a RS274_BLOCK_TERMINATOR_CHAR value on this line? We always should
            // except for macros which can sometimes do this
            if (trimmedStr.Contains(RS274_BLOCK_TERMINATOR) == false)
            {
                // assume we have an odd macro continuation, bang it on the end of the last one
                gerberCommandLineList[gerberCommandLineList.Count - 1].AppendLine(trimmedStr);
                return 0;
            }

            // we may also have multiple command blocks on the same line
            blockArray = trimmedStr.Split(blockSplitChars, StringSplitOptions.RemoveEmptyEntries);
            // this should never be null
            if (blockArray == null) return 200;

            foreach (string workingLine1 in blockArray)
            {
                // we must strip off any parameter delimiters - these will either be
                // the first or last. 

                // Is it just a trailing delim? This could mean termination 
                // of previous command?
                if (workingLine1 == RS274_CMD_DELIMITER) continue;

                // trim off before and after
                //tmpLine1 = workingLine1.Trim(paramCharsToTrim);

                if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_AB_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_AD_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_AM_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1 + RS274_BLOCK_TERMINATOR, lineNumber));
                //else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_D_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_FS_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G01_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G02_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G03_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G04_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G36_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G37_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G54_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G70_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G71_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G74_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G75_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G90_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_G91_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_IN_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_IP_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_LM_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_LN_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_LP_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_LR_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_LS_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_MO_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_M00_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_M01_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_M02_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_M30_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_OF_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_SF_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_SR_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_TA_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_TD_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_TF_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_TO_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                //else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_X_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                //else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_Y_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if (workingLine1.StartsWith(RS274_CMD_DELIMITER + RS274_SFA1B1_CMD) == true) gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                else if ((workingLine1.StartsWith(RS274_X_CMD) == true) || (workingLine1.StartsWith(RS274_Y_CMD) == true) || (workingLine1.StartsWith(RS274_D_CMD) == true))
                {
                    // special case, these are all parts one command but the order can be mixed up on same line. They do not have a delimiter in front of them either
                    gerberCommandLineList.Add(new GerberCommandSourceLines(workingLine1, lineNumber));
                }
                else
                {
                    // if we get here, we either have a command we do not know about or we have a multiline continuation of a previous
                    // command. True commands we do not know about will start with the RS274_CMD_DELIMITER otherwise it is a 
                    // continuation
                    if (workingLine1.StartsWith(RS274_CMD_DELIMITER) == true)
                    {
                        // if we get here we are a line of unknown type
                        LogMessage("Unknown Gerber line type >" + lineStr + "< on line " + lineNumber.ToString());
                        if (lineStr.Length > 20)
                        {
                            throw new NotImplementedException("Cannot cope with unknown Gerber code on line " + lineNumber.ToString() + " code is >" + lineStr.Substring(0, 20) + "<");
                        }
                        else
                        {
                            throw new NotImplementedException("Cannot cope with unknown Gerber code on line " + lineNumber.ToString() + " code is >" + lineStr + "<");
                        }
                    }
                    // if we get here we assume we have a continuation. We just add this line to the last one we placed in the gerberCommandLineList
                    if (gerberCommandLineList.Count == 0)
                    {
                        // no previous command this must be garbage
                        if (lineStr.Length > 20)
                        {
                            throw new NotImplementedException("Cannot cope with unknown Gerber data on line " + lineNumber.ToString() + " line is >" + lineStr.Substring(0, 20) + "<");
                        }
                        else
                        {
                            throw new NotImplementedException("Cannot cope with unknown Gerber data on line " + lineNumber.ToString() + " line is >" + lineStr + "<");
                        }
                    }
                    // just add it, with a terminator. 
                    gerberCommandLineList[gerberCommandLineList.Count - 1].AddLine(workingLine1+ RS274_BLOCK_TERMINATOR);
                }

            }

            //temp
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Processes the commands in the gerber file
        /// </summary>
        public int ProcessGerberCommands()
        {
            string tmpLine1;
            int retInt;
            char[] paramCharsToTrim = { RS274_CMD_DELIMITER_CHAR, ' ' };

            // for each block in the command object
            foreach (GerberCommandSourceLines cmdLineObj in gerberCommandLineList)
            {
                // get the line
                string lineStr = cmdLineObj.GetFirstLine();
                int lineNumber = cmdLineObj.LineNumber;

                // we must strip off any parameter delimiters - these will either be
                // the first or last. 
                // trim off fore and after
                tmpLine1 = lineStr.Trim(paramCharsToTrim);

                // we must detect the line type based on the first two letters
                // Are we a FORMAT PARAMETER?
                if (tmpLine1.StartsWith(RS274_FS_CMD) == true)
                {
                    // we are a FORMAT PARAMETER
                    GerberLine_FSCode fsObj = new GerberLine_FSCode(lineStr, tmpLine1, lineNumber);
                    retInt = fsObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(fs), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 501;
                    }
                    // it is good, add it
                    sourceLines.Add(fsObj);
                    // record this as well
                    StateMachine.FormatParameter = fsObj;
                    continue;
                }
                // Are we a AD PARAMETER?
                else if (tmpLine1.StartsWith(RS274_AD_CMD) == true)
                {
                    // we are a AD PARAMETER
                    GerberLine_ADCode adObj = new GerberLine_ADCode(lineStr, tmpLine1, lineNumber);
                    retInt = adObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(ad), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 502;
                    }
                    // it is good, add it
                    sourceLines.Add(adObj);
                    // add this aperture object to the collection
                    (ApertureCollection as ICollection<GerberLine_ADCode>).Add(adObj);
                    continue;
                }
                // Are we a IN PARAMETER?
                else if (tmpLine1.StartsWith(RS274_IN_CMD) == true)
                {
                    // we are a IN PARAMETER
                    GerberLine_INCode lnObj = new GerberLine_INCode(lineStr, tmpLine1, lineNumber);
                    retInt = lnObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(IN), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 503;
                    }
                    // it is good, add it
                    sourceLines.Add(lnObj);
                    continue;
                }
                // Are we a IP PARAMETER?
                else if (tmpLine1.StartsWith(RS274_IP_CMD) == true)
                {
                    // we are a IP PARAMETER
                    GerberLine_IPCode ipObj = new GerberLine_IPCode(lineStr, tmpLine1, lineNumber);
                    retInt = ipObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(IP), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 504;
                    }
                    // it is good, add it
                    sourceLines.Add(ipObj);
                    continue;
                }
                // Are we a LP PARAMETER?
                else if (tmpLine1.StartsWith(RS274_LP_CMD) == true)
                {
                    // we are a LP PARAMETER
                    GerberLine_LPCode lpObj = new GerberLine_LPCode(lineStr, tmpLine1, lineNumber);
                    retInt = lpObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(LP), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 505;
                    }
                    // it is good, add it
                    sourceLines.Add(lpObj);
                    continue;
                }
                // Are we a LN PARAMETER?
                else if (tmpLine1.StartsWith(RS274_LN_CMD) == true)
                {
                    // we are a LN PARAMETER
                    GerberLine_LNCode lnObj = new GerberLine_LNCode(lineStr, tmpLine1, lineNumber);
                    retInt = lnObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(LN), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 506;
                    }
                    // it is good, add it
                    sourceLines.Add(lnObj);
                    continue;
                }
                // Are we a OF PARAMETER?
                else if (tmpLine1.StartsWith(RS274_OF_CMD) == true)
                {
                    // we are a OF PARAMETER
                    GerberLine_OFCode ofObj = new GerberLine_OFCode(lineStr, tmpLine1, lineNumber);
                    retInt = ofObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(OF), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 507;
                    }
                    // it is good, add it
                    sourceLines.Add(ofObj);
                    continue;
                }
                // Are we a SFA1B1 PARAMETER? This is the only type of SF parameter we support
                // if it is an SF of any other type we generate an error
                else if (tmpLine1.StartsWith(RS274_SF_CMD) == true)
                {
                    // we are a SFA1B1 PARAMETER
                    GerberLine_SFCode lnObj = new GerberLine_SFCode(lineStr, tmpLine1, lineNumber);
                    retInt = lnObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(SFA1B1), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 508;
                    }
                    // it is good, add it
                    sourceLines.Add(lnObj);
                    continue;
                }
                // Are we a D Code? These can start with D, X or Y depending if the line is modal
                else if ((tmpLine1.StartsWith(RS274_D_CMD) == true) || (tmpLine1.StartsWith(RS274_X_CMD) == true) || (tmpLine1.StartsWith(RS274_Y_CMD) == true)) 
                {
                    // we are a D Code
                    GerberLine_DCode dObj = new GerberLine_DCode(lineStr, tmpLine1, lineNumber);
                    retInt = dObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(d), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 509;
                    }
                    // it is good, add it
                    sourceLines.Add(dObj);
                    continue;
                }
                // Are we a MODE PARAMETER?
                else if (tmpLine1.StartsWith(RS274_MO_CMD) == true)
                {
                    // we are a MODE PARAMETER
                    GerberLine_MOCode moObj = new GerberLine_MOCode(lineStr, tmpLine1, lineNumber);
                    retInt = moObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(mo), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 510;
                    }
                    // it is good, add it
                    sourceLines.Add(moObj);
                    // record this as well
                    StateMachine.GerberFileUnits = moObj.GerberFileUnits;
                    continue;
                }
                // are we a M00 code
                else if (tmpLine1.StartsWith(RS274_M00_CMD) == true)
                {
                    // we are a M00 Code
                    GerberLine_M00Code gObj = new GerberLine_M00Code(lineStr, tmpLine1, lineNumber);
                    retInt = gObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(M00), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 520;
                    }
                    // it is good, add it
                    sourceLines.Add(gObj);
                    // this is program terminiation. We do not process further same as m02
                    break;
                    //continue;
                }
                // are we a M01 code
                else if (tmpLine1.StartsWith(RS274_M01_CMD) == true)
                {
                    // we are a M01 Code
                    GerberLine_M01Code gObj = new GerberLine_M01Code(lineStr, tmpLine1, lineNumber);
                    retInt = gObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(M01), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 530;
                    }
                    // it is good, add it
                    sourceLines.Add(gObj);
                    continue;
                }
                // are we a M02 code
                else if (tmpLine1.StartsWith(RS274_M02_CMD) == true)
                {
                    // we are a M02 Code
                    GerberLine_M02Code gObj = new GerberLine_M02Code(lineStr, tmpLine1, lineNumber);
                    retInt = gObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(M02), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 540;
                    }
                    // it is good, add it
                    sourceLines.Add(gObj);
                    // this is program terminiation. We do not process further
                    break;
                    //continue;
                }
                // are we a M30 code
                else if (tmpLine1.StartsWith(RS274_M30_CMD) == true)
                {
                    // we are a M30 Code
                    GerberLine_M30Code gObj = new GerberLine_M30Code(lineStr, tmpLine1, lineNumber);
                    retInt = gObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(M30), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 545;
                    }
                    // it is good, add it
                    sourceLines.Add(gObj);
                    // this is program terminiation. We do not process further
                    break;
                    //continue;
                }
                // Are we a G01 Code? These can start with G01
                else if (tmpLine1.StartsWith(RS274_G01_CMD) == true)
                {
                    // we are a G01 Code
                    GerberLine_G01Code g01Obj = new GerberLine_G01Code(lineStr, tmpLine1, lineNumber);
                    retInt = g01Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g01), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 550;
                    }
                    // it is good, add it
                    sourceLines.Add(g01Obj);
                    continue;
                }
                // Are we a G02 Code? These can start with G02
                else if (tmpLine1.StartsWith(RS274_G02_CMD) == true)
                {
                    // we are a G02 Code
                    GerberLine_G02Code g02Obj = new GerberLine_G02Code(lineStr, tmpLine1, lineNumber);
                    retInt = g02Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g02), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 550;
                    }
                    // it is good, add it
                    sourceLines.Add(g02Obj);
                    continue;
                }
                // Are we a G03 Code? These can start with G03
                else if (tmpLine1.StartsWith(RS274_G03_CMD) == true)
                {
                    // we are a G03 Code
                    GerberLine_G03Code g03Obj = new GerberLine_G03Code(lineStr, tmpLine1, lineNumber);
                    retInt = g03Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g03), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 550;
                    }
                    // it is good, add it
                    sourceLines.Add(g03Obj);
                    continue;
                }
                // Are we a G04 Code? These can start with G04
                else if (tmpLine1.StartsWith(RS274_G04_CMD) == true)
                {
                    // we are a G04 Code
                    GerberLine_G04Code g04Obj = new GerberLine_G04Code(lineStr, tmpLine1, lineNumber);
                    retInt = g04Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g04), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 560;
                    }
                    // it is good, add it
                    sourceLines.Add(g04Obj);
                    continue;
                }
                // Are we a G36 Code? These can start with G36
                else if (tmpLine1.StartsWith(RS274_G36_CMD) == true)
                {
                    // we are a G36 Code
                    GerberLine_G36Code g36Obj = new GerberLine_G36Code(lineStr, tmpLine1, lineNumber);
                    retInt = g36Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g36), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 570;
                    }
                    // it is good, add it
                    sourceLines.Add(g36Obj);
                    continue;
                }
                // Are we a G37 Code? These can start with G37
                else if (tmpLine1.StartsWith(RS274_G37_CMD) == true)
                {
                    // we are a G37 Code
                    GerberLine_G37Code g37Obj = new GerberLine_G37Code(lineStr, tmpLine1, lineNumber);
                    retInt = g37Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g37), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 580;
                    }
                    // it is good, add it
                    sourceLines.Add(g37Obj);
                    continue;
                }
                // Are we a G54 Code? These can start with G54
                else if (tmpLine1.StartsWith(RS274_G54_CMD) == true)
                {
                    // we are a G54 Code
                    GerberLine_G54Code g54Obj = new GerberLine_G54Code(lineStr, tmpLine1, lineNumber);
                    retInt = g54Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g54), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 590;
                    }
                    // it is good, add it
                    sourceLines.Add(g54Obj);
                    continue;
                }
                // Are we a G70 Code? These can start with G70
                else if (tmpLine1.StartsWith(RS274_G70_CMD) == true)
                {
                    // we are a G70 Code
                    GerberLine_G70Code g70Obj = new GerberLine_G70Code(lineStr, tmpLine1, lineNumber);
                    retInt = g70Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g70), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 600;
                    }
                    // it is good, add it
                    sourceLines.Add(g70Obj);
                    continue;
                }
                // Are we a G71 Code? These can start with G71
                else if (tmpLine1.StartsWith(RS274_G71_CMD) == true)
                {
                    // we are a G71 Code
                    GerberLine_G71Code g71Obj = new GerberLine_G71Code(lineStr, tmpLine1, lineNumber);
                    retInt = g71Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g71), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 610;
                    }
                    // it is good, add it
                    sourceLines.Add(g71Obj);
                    continue;
                }
                // Are we a G74 Code? These can start with G74
                else if (tmpLine1.StartsWith(RS274_G74_CMD) == true)
                {
                    // we are a G74 Code
                    GerberLine_G74Code g74Obj = new GerberLine_G74Code(lineStr, tmpLine1, lineNumber);
                    retInt = g74Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g74), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 550;
                    }
                    // it is good, add it
                    sourceLines.Add(g74Obj);
                    continue;
                }
                // Are we a G75 Code? These can start with G75
                else if (tmpLine1.StartsWith(RS274_G75_CMD) == true)
                {
                    // we are a G75 Code
                    GerberLine_G75Code g75Obj = new GerberLine_G75Code(lineStr, tmpLine1, lineNumber);
                    retInt = g75Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g75), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 620;
                    }
                    // it is good, add it
                    sourceLines.Add(g75Obj);
                    continue;
                }
                // Are we a G90 Code? These can start with G90
                else if (tmpLine1.StartsWith(RS274_G90_CMD) == true)
                {
                    // we are a G90 Code
                    GerberLine_G90Code g90Obj = new GerberLine_G90Code(lineStr, tmpLine1, lineNumber);
                    retInt = g90Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g90), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 630;
                    }
                    // it is good, add it
                    sourceLines.Add(g90Obj);
                    continue;
                }
                // Are we a G91 Code? These can start with G91
                else if (tmpLine1.StartsWith(RS274_G91_CMD) == true)
                {
                    // we are a G91 Code
                    GerberLine_G91Code g91Obj = new GerberLine_G91Code(lineStr, tmpLine1, lineNumber);
                    retInt = g91Obj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(g91), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 640;
                    }
                    // it is good, add it
                    sourceLines.Add(g91Obj);
                    continue;
                }
                // are we a TA code
                else if (tmpLine1.StartsWith(RS274_TA_CMD) == true)
                {
                    // we are a TA Code
                    GerberLine_TACode gObj = new GerberLine_TACode(lineStr, tmpLine1, lineNumber);
                    retInt = gObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(TA), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 650;
                    }
                    // it is good, add it
                    sourceLines.Add(gObj);
                    continue;
                }
                // are we a TD code
                else if (tmpLine1.StartsWith(RS274_TD_CMD) == true)
                {
                    // we are a TD Code
                    GerberLine_TDCode gObj = new GerberLine_TDCode(lineStr, tmpLine1, lineNumber);
                    retInt = gObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(TD), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 660;
                    }
                    // it is good, add it
                    sourceLines.Add(gObj);
                    continue;
                }
                // are we a TF code
                else if (tmpLine1.StartsWith(RS274_TF_CMD) == true)
                {
                    // we are a TF Code
                    GerberLine_TFCode gObj = new GerberLine_TFCode(lineStr, tmpLine1, lineNumber);
                    retInt = gObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(TF), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 670;
                    }
                    // it is good, add it
                    sourceLines.Add(gObj);
                    continue;
                }
                // are we a TO code
                else if (tmpLine1.StartsWith(RS274_TO_CMD) == true)
                {
                    // we are a TO Code
                    GerberLine_TOCode gObj = new GerberLine_TOCode(lineStr, tmpLine1, lineNumber);
                    retInt = gObj.ParseLine(tmpLine1, StateMachine);
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(TO), call to ParseLine returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 680;
                    }
                    // it is good, add it
                    sourceLines.Add(gObj);
                    continue;
                }
                // Are we a LM PARAMETER?
                else if (tmpLine1.StartsWith(RS274_LM_CMD) == true)
                {
                    LogMessage("Load Mirroring (%LM) >" + lineStr + "< Specified on line " + lineNumber.ToString());
                    throw new NotImplementedException("Unsupported Load Mirroring (%LM) code on line " + lineNumber.ToString() + ".\n\nPlease configure your software to not output aperture macros in the Gerber code - most software can do this.\nEagle users please see the \"Configuring Eagle\" help page as it is a bit tricky.");
                }
                // Are we a LR PARAMETER?
                else if (tmpLine1.StartsWith(RS274_LR_CMD) == true)
                {
                    LogMessage("Load Rotation (%LR) >" + lineStr + "< Specified on line " + lineNumber.ToString());
                    throw new NotImplementedException("Unsupported Load Rotation (%LR) code on line " + lineNumber.ToString() + ".\n\nPlease configure your software to not output aperture macros in the Gerber code - most software can do this.\nEagle users please see the \"Configuring Eagle\" help page as it is a bit tricky.");
                }
                // Are we a LS PARAMETER?
                else if (tmpLine1.StartsWith(RS274_LS_CMD) == true)
                {
                    LogMessage("Load Scaling (%LS) >" + lineStr + "< Specified on line " + lineNumber.ToString());
                    throw new NotImplementedException("Unsupported Load Scaling (%LS) code on line " + lineNumber.ToString() + ".\n\nPlease configure your software to not output aperture macros in the Gerber code - most software can do this.\nEagle users please see the \"Configuring Eagle\" help page as it is a bit tricky.");
                }
                // Are we a SR PARAMETER?
                else if (tmpLine1.StartsWith(RS274_SR_CMD) == true)
                {
                    LogMessage("Step and Repeat (%SR) >" + lineStr + "< Specified on line " + lineNumber.ToString());
                    throw new NotImplementedException("Unsupported Step and Repeat (%SR) code on line " + lineNumber.ToString() + ".\n\nPlease configure your software to not output aperture macros in the Gerber code - most software can do this.\nEagle users please see the \"Configuring Eagle\" help page as it is a bit tricky.");
                }
                // Are we a AB PARAMETER?
                else if (tmpLine1.StartsWith(RS274_AB_CMD) == true)
                {
                    // if we get here we are an block aperture
                    LogMessage("Block Apertures (%AB) >" + lineStr + "< Specified on line " + lineNumber.ToString());
                    throw new NotImplementedException("Unsupported Block Aperture (%AB) code on line " + lineNumber.ToString() + ".\n\nPlease configure your software to not output aperture macros in the Gerber code - most software can do this.\nEagle users please see the \"Configuring Eagle\" help page as it is a bit tricky.");
                }
                // Are we a AM PARAMETER?
                else if (tmpLine1.StartsWith(RS274_AM_CMD) == true)
                {
                    // if we get here we are an aperture macro
                    LogMessage("Aperture Macro (%AM) >" + lineStr + "< Specified on line " + lineNumber.ToString());
                    GerberLine_AMCode gObj = new GerberLine_AMCode(cmdLineObj);
                    retInt = gObj.ProcessCommand();
                    if (retInt != 0)
                    {
                        LogMessage("lineStr(AM), call to ProcessCommand returned " + retInt.ToString() + " Error on line " + lineNumber.ToString());
                        return 690;
                    }
                    // it is good, add it
                    sourceLines.Add(gObj);
                    // add this macro object to the collection
                    (MacroCollection as ICollection<GerberLine_AMCode>).Add(gObj);
                }
                else
                {
                    // if we get here we are a line of unknown type
                    LogMessage("Unknown Gerber line type >" + lineStr + "< on line " + lineNumber.ToString());
                    if (lineStr.Length > 20)
                    {
                        throw new NotImplementedException("Cannot cope with unknown Gerber code on line " + lineNumber.ToString() + " code is >" + lineStr.Substring(0, 20) + "<");
                    }
                    else
                    {
                        throw new NotImplementedException("Cannot cope with unknown Gerber code on line " + lineNumber.ToString() + " code is >" + lineStr + "<");
                    }
                }

            }

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Figures out the mid point of all pads flagged as reference pins.
        /// </summary>
        /// <param name="errStr">an error string</param>
        /// 
        /// <returns>z success, -ve err not reported, +ve err reported</returns>
        public int GetMidPointFromReferencePins(out float midX, out float midY, ref string errStr)
        {
            int refPinCount = 0;
            float minX=float.MaxValue;
            float maxX=float.MinValue;
            float minY=float.MaxValue;
            float maxY=float.MinValue;

            errStr="";
            midX = 0;
            midY = 0;

            // go through all the pads, we just look for the min and max and 
            // assume those are the extremities
            foreach (GerberPad padObj in StateMachine.PadCenterPointList)
            {
                // test it 
                if(padObj.IsRefPin == false) continue;
                refPinCount++;
                if(padObj.X0 < minX) minX = padObj.X0;
                if(padObj.X0 > maxX) maxX = padObj.X0;
                if(padObj.Y0 < minY) minY = padObj.Y0;
                if(padObj.Y0 > maxY) maxY = padObj.Y0;
            }

            // did we find at least two?
            if(refPinCount<2)
            {
                errStr = refPinCount.ToString()+ " ref pin pads were found in the Gerber file. At least two are required.\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                LogMessage("GetMidPointFromReferencePins: " + errStr);
                return -201;
            }

            // these are the midpoints
            midX = minX + ((maxX - minX) / 2);
            midY = minY + ((maxY - minY) / 2);
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flags all of the reference pin pads in the PadCenterPointList of this 
        /// object and performs some checks.
        /// </summary>
        /// <param name="errStr">an error string</param>
        /// <returns>z success, -ve err not reported, +ve err reported</returns>
        public int SetReferencePins(ref string errStr)
        {
            errStr = "";

            LogMessage("SetReferencePins called");

            // can we find any reference pins? The PadCenterPoint list will have these from the Gerber file
            if (StateMachine.PadCenterPointList.Count == 0)
            {
                errStr = "No pads were found in the Gerber file.\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                LogMessage("SetReferencePins: " + errStr);
                return -201;
            }
            // ok we have some pads have we got any of the required size?
            List<GerberPad> refPadsList = new List<GerberPad>();
            foreach (GerberPad padObj in StateMachine.PadCenterPointList)
            {
                //DebugMessage("padObj.PadDiameter = " + padObj.PadDiameter.ToString());
                // reset it 
                padObj.IsRefPin = false;
                // test it, round to 2 decimals that should be sufficent
                if (Math.Round(padObj.PadDiameter,2) != Math.Round(StateMachine.GerberFileManager.ReferencePinPadDiameter,2)) continue;
                // mark it
                padObj.IsRefPin = true;
                // add it
                refPadsList.Add(padObj);
            }
            if (refPadsList.Count == 0)
            {
                errStr = "No Reference Pin pads were found in the Gerber file. The File Manager thinks they should have diameter: " + StateMachine.GerberFileManager.ReferencePinPadDiameter.ToString() + "\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                LogMessage("SetReferencePins: " + errStr);
                return -202;
            }
            // we have to have at least 2 pins
            if (refPadsList.Count < 2)
            {
                errStr = "Only one Reference Pin pad of diameter: " + StateMachine.GerberFileManager.ReferencePinPadDiameter.ToString() + " was found in the Gerber file. At least two are required.\n\nHave Reference Pin Pads been placed on the PCB schematic?";
                LogMessage("SetReferencePins: " + errStr);
                return -203;
            }
            if (refPadsList.Count > StateMachine.GerberFileManager.ReferencePinsMaxNumber)
            {
                errStr = refPadsList.Count.ToString() + " Reference Pin pads of diameter: " + StateMachine.GerberFileManager.ReferencePinPadDiameter.ToString() + " were found in the Gerber file. According to the File Manager ReferencePinsMaxNumber setting this is unlikely to be correct.\n\nDo other pins on PCB schematic have the Reference Pin Diameter of " + StateMachine.GerberFileManager.ReferencePinPadDiameter.ToString() + "?";
                LogMessage("SetReferencePins: " + errStr);
                return -204;
            }
            // are the reference pins all co-linear with at least one other 
            foreach (GerberPad gcPadObj in refPadsList)
            {
                bool axiallyCoLinear = false;
                foreach (GerberPad testObj in refPadsList)
                {
                    // is it the same point? do not test
                    if ((gcPadObj.X0 == testObj.X0) && (gcPadObj.Y0 == testObj.Y0)) continue;
                    // do the YCoords match
                    if (gcPadObj.Y0 == testObj.Y0)
                    {
                        // flag it
                        axiallyCoLinear = true;
                        continue;
                    }
                    // if we get here the gcPadObj is not axially colinear with the testObj
                }
                // is the gcPadObj axially colinear with any other object?
                if (axiallyCoLinear == false)
                {
                    // no, we could not find any other objects. This is an error
                    errStr = "The Reference Pin pad at postion (" + gcPadObj.X0.ToString() + "," + gcPadObj.Y0.ToString() + ") is not horizontally co-linear (has the same Y value) with any other Reference Pin pad.\n\nAll Reference Pin Pads on the PCB schematic must be in one or more lines parallel to the X axis.";
                    LogMessage("SetReferencePins: " + errStr);
                    return -206;

                }
            }
            // OK, everything looks good, the reference pin pads seem to be setup correctly
            LogMessage("SetReferencePins successful completion");
            return 0;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flags all of the ignore pin pads in the PadCenterPointList of this 
        /// object 
        /// </summary>
        /// <param name="errStr">an error string</param>
        /// <returns>z success, -ve err not reported, +ve err reported</returns>
        public int SetIgnorePins(ref string errStr)
        {
            errStr = "";

            LogMessage("SetIgnorePins called");

            // can we find any ignore pins? The PadCenterPoint list will have these from the Gerber file
            if (StateMachine.PadCenterPointList.Count == 0) return 0;

            // ok we have some pads have we got any of the required size?
            List<GerberPad> refPadsList = new List<GerberPad>();
            foreach (GerberPad padObj in StateMachine.PadCenterPointList)
            {
                //DebugMessage("padObj.PadDiameter = " + padObj.PadDiameter.ToString());
                // reset it 
                padObj.IgnoreDueToSize = false;
                // test it, round to 3 decimals that should be sufficent
                if (Math.Round(padObj.PadDiameter, 3) != Math.Round(StateMachine.GerberFileManager.IgnorePadDiameter, 3)) continue;
                // mark it
                padObj.IgnoreDueToSize = true;
            }
            LogMessage("SetIgnorePins successful completion");
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current X Coord Origin Adjust value. These are compensating
        /// values we apply to the current coordinates in order to make the
        /// smallest X coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// </summary>
        public float PlotXCoordOriginAdjust
        {
            get
            {
                return plotXCoordOriginAdjust;
            }
            set
            {
                plotXCoordOriginAdjust = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current Y Coord Origin Adjust value. These are compensating
        /// values we apply to the current coordinates in order to make the
        /// smallest Y coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// </summary>
        public float PlotYCoordOriginAdjust
        {
            get
            {
                return plotYCoordOriginAdjust;
            }
            set
            {
                plotYCoordOriginAdjust = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Applies the current coordinate Origin Adjust value. These are compensating
        /// values we ADD to the current coordinates in order to make the
        /// smallest XY coordinate specified in the plot approximately zero but 
        /// definitely non-negative (which totally complicates the isoPlotSegments);
        /// </summary>
        /// <param name="xCoordAdjust">x origin adjuster</param>
        /// <param name="yCoordAdjust">y origin adjuster</param>
        public void SetGerberPlotOriginCoordinateAdjustments(float xCoordAdjust, float yCoordAdjust)
        {
            LogMessage("Gerber Origin coord adjustments: xCoordAdjust=" + xCoordAdjust.ToString() + " yCoordAdjust=" + yCoordAdjust.ToString());
            // just run through and apply it to each GerberLine whether it uses it or not
            foreach (GerberLine gLine in SourceLines)
            {
                gLine.PlotXCoordOriginAdjust = xCoordAdjust;
                gLine.PlotYCoordOriginAdjust = yCoordAdjust;
            }
            PlotXCoordOriginAdjust = xCoordAdjust;
            PlotYCoordOriginAdjust = yCoordAdjust;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the gerber file options to use. Never gets/sets a null value
        /// </summary>
        public FileManager GerberFileManager
        {
            get
            {
                return StateMachine.GerberFileManager;
            }
            set
            {
                StateMachine.GerberFileManager = value;
            }
        }
 
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Plots the Gerber file contents on the designated graphics object
        /// </summary>
        /// <param name="graphicsObj">a graphics object to draw on</param>
        public void PlotGerberFile(Graphics graphicsObj)
        {
            int errInt=0;
            string errStr="";
            GerberLine.PlotActionEnum errAction = GerberLine.PlotActionEnum.PlotAction_End;

            // set the StateMachine
            StateMachine.ResetForPlot();

            foreach (GerberLine lineObj in SourceLines)
            {
                lineObj.ResetForPlot();
                errAction = lineObj.PerformPlotGerberAction(graphicsObj, StateMachine, ref errInt, ref errStr);
                if (errAction == GerberLine.PlotActionEnum.PlotAction_Continue)
                {
                    // all is well
                    continue;
                }
                if (errAction == GerberLine.PlotActionEnum.PlotAction_End)
                {
                    // we are all done
                    return;
                }
                else if (errAction == GerberLine.PlotActionEnum.PlotAction_FailWithError)
                {
                    // handle this error
                    LogMessage("Plot Failed on obj:" + lineObj.ToString() + " at line number " + lineObj.LineNumber.ToString());
                    return;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Processes a gerber file into GCode
        /// </summary>
        /// <param name="errStr">we return error messages here</param>
        /// <param name="plotSize">the size of the plot we create our GCode templates on. This is
        /// <param name="isoPlotObjOut">the IsoPlotBuilder object we create in this step</param>
        /// the graphical object we use to figure out our isolation paths. These size should be
        /// able to take the max gerberCoord*virtualCoordPerPlotUnit listed in the Gerber file</param>
        /// <param name="virtualCoordPerPlotUnit">the number of coordinates in the GerberFile per plot unit. For example
        /// if the GerberFile says X=1.456 and this is 1000 then we would goto X=1456 in the plot</param>
        /// <remarks>
        /// NOTE: this is Step1 in the Graphical Stigmergy (GS) process. After this function completes
        ///       we will have an IsoPlotBuilder object which contains an array of ints (the isolation plot)
        ///       and the values in that array (called pixels) will be annotated with values which indicate
        ///       which Gerber Objects use them, how they use them (edge or background). If other
        ///       Gerber Objects share the same pixel the pixel contains the ID of an overlay object
        ///       which then contains the above information for one or more Gerber objects.
        /// </remarks>
        public int PerformGerberToGCodeStep1(out IsoPlotBuilder isoPlotObjOut, Size plotSize, float virtualCoordPerPlotUnit, ref string errStr)
        {
            int errInt = 0;
            int lineCount = 0;
            GerberLine.PlotActionEnum errAction = GerberLine.PlotActionEnum.PlotAction_End;

            // init this
            isoPlotObjOut = null;

            // set the StateMachine
            StateMachine.ResetForPlot();
            StateMachine.IsoPlotPointsPerAppUnit = virtualCoordPerPlotUnit;

            // we gotta have one
            if (SourceLines.Count ==0)
            {
                errStr = "PerformGerberToGCodeStep1: SourceLines.Count ==0";
                LogMessage(errStr);
                return 100;
            }
            if (plotSize.Width <= 0)
            {
                errStr = "PerformGerberToGCodeStep1: plotSize.Width <= 0";
                LogMessage(errStr);
                return 101;
            }
            if (plotSize.Height <= 0)
            {
                errStr = "PerformGerberToGCodeStep1: plotSize.Height <= 0";
                LogMessage(errStr);
                return 102;
            }
            if (virtualCoordPerPlotUnit <= 0)
            {
                errStr = "PerformGerberToGCodeStep1: virtualCoordPerPlotUnit <= 0";
                LogMessage(errStr);
                return 103;
            }

            // create IsoPlotBuilder with a 2D array equivalent to the plotsize
            isoPlotObjOut = new IsoPlotBuilder(plotSize.Width, plotSize.Height);
            // give the manager to the builder object now
            isoPlotObjOut.CurrentFileManager = GerberFileManager;

            // now we carefully draw each item on the array using our custom
            // drawing tools. See the alogrythm document for what is going on here
            foreach (GerberLine lineObj in this.SourceLines)
            {
                lineCount++;
                lineObj.ResetForPlot();
              //  DebugMessage("Processing Line" + lineCount.ToString());
                errAction = lineObj.PerformPlotIsoStep1Action(isoPlotObjOut, StateMachine, ref errInt, ref errStr);
                if (errAction == GerberLine.PlotActionEnum.PlotAction_Continue)
                {
                    // all is well
                    continue;
                }
                if (errAction == GerberLine.PlotActionEnum.PlotAction_End)
                {
                    // we are all done for this stage
                    break;
                }
                else if (errAction == GerberLine.PlotActionEnum.PlotAction_FailWithError)
                {
                    // handle this error
                    errStr = "GCode Plot Failed on obj:" + lineObj.ToString();
                    LogMessage(errStr);
                    return 201;
                }
            }

            //isoPlotObjOut.DumpIsoPlotArrayToLog();

            return 0;
        }

    }
}

