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
    /// A class to encapsulate a gerber G37 Code (Turn off Polygon Area Fill)
    /// </summary>
    public class GerberLine_G37Code : GerberLine
    {

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawLineStrIn">The raw line string</param>
        /// <param name="processedLineStrIn">The processed line string</param>
        public GerberLine_G37Code(string rawLineStrIn, string processedLineStrIn, int lineNumberIn)
            : base(rawLineStrIn, processedLineStrIn, lineNumberIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the action the plot requires based on the current context
        /// </summary>
        /// <param name="graphicsObj">a graphics object on which to plot</param>
        /// <param name="stateMachine">the gerber plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <returns>an enum value indicating what next action to take</returns>
        public override GerberLine.PlotActionEnum PerformPlotGerberAction(Graphics graphicsObj, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {

            // one would think that turning off the contouring would be all that was necessary. However
            // contours are complex object and sometime we have to finish off the object usually this 
            // means fill the existing contour object .
            if ((stateMachine.ContourDrawingModeEnabled == true) && (stateMachine.ComplexObjectList_DCode.Count != 0))
            {
                // we have an existing complex object, we process it there

                // Create solid brush.
                SolidBrush fillBrush = (SolidBrush)stateMachine.GerberContourFillBrush; 
                // Get the bounding box
                Rectangle boundingRect = stateMachine.GetBoundingRectangleFromComplexObjectList_DCode();
                // get a graphics path defining this object
                GraphicsPath gPath = stateMachine.GetGraphicsPathFromComplexObjectList_DCode();
                MiscGraphicsUtils.FillRectangleUsingGraphicsPath(graphicsObj, gPath, boundingRect, fillBrush);
                // reset the complex object list
                stateMachine.ComplexObjectList_DCode = new List<GerberLine_DCode>();
            }

            // enable contour drawing mode
            stateMachine.ContourDrawingModeEnabled = false;
            return GerberLine.PlotActionEnum.PlotAction_Continue;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the plot Isolation Object actions required based on the current context
        /// </summary>
        /// <param name="isoPlotBuilder">A GCode Builder object</param>
        /// <param name="stateMachine">the gerber plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <returns>an enum value indicating what next action to take</returns>
        public override GerberLine.PlotActionEnum PerformPlotIsoStep1Action(IsoPlotBuilder isoPlotBuilder, GerberFileStateMachine stateMachine, ref int errorValue, ref string errorString)
        {

            // one would think that turning off the contouring would be all that was necessary. However
            // contours are complex object and sometime we have to finish off the object usually this 
            // means fill the existing contour object .
            if ((stateMachine.ContourDrawingModeEnabled == true) && (stateMachine.ComplexObjectList_DCode.Count != 0))
            {
                // process the complex object by drawing in the background 
                
                // Get the bounding box
                Rectangle boundingRect = stateMachine.GetBoundingRectangleFromComplexObjectList_DCode();
                // get a list of builderIDs in the complex object list
                List<int> builderIDList = stateMachine.GetListOfIsoPlotBuilderIDsFromComplexObjectList_DCode();
                if(builderIDList.Count>0)
                {
                    if (stateMachine.BackgroundFillModeAccordingToPolarity == GSFillModeEnum.FillMode_BACKGROUND)
                    {
                        // Normal Dark polarity. Just fill it in with a backgroundpixel
                        isoPlotBuilder.BackgroundFillGSRegionComplex(builderIDList, builderIDList[0], boundingRect.X, boundingRect.Y, boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height, -1);
                    }
                    else if (stateMachine.BackgroundFillModeAccordingToPolarity == GSFillModeEnum.FillMode_ERASE)
                    {
                        // we have reverse polarity. The object we just drew must be cleaned out and everything underneath it removed
                        isoPlotBuilder.BackgroundFillGSByBoundaryComplexVert(builderIDList, IsoPlotObject.DEFAULT_ISOPLOTOBJECT_ID, boundingRect.X, boundingRect.Y, boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height, -1);
                    }
                }
            }
            // reset, we have processed the complex list
            stateMachine.ComplexObjectList_DCode = new List<GerberLine_DCode>();
            // disable contour drawing mode
            stateMachine.ContourDrawingModeEnabled = false;
            return GerberLine.PlotActionEnum.PlotAction_Continue;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Parses out the line and gets the required information from it
        /// </summary>
        /// <param name="processedLineStr">a line string without block terminator or format parameters</param>
        /// <param name="stateMachine">The state machine containing the implied modal values</param>
        /// <returns>z success, nz fail</returns>
        public override int ParseLine(string processedLineStr, GerberFileStateMachine stateMachine)
        {
            //LogMessage("ParseLine(G37) started");

            if (processedLineStr == null) return 100;
            if (processedLineStr.StartsWith("G37") == false)
            {
                return 200;
            }
 
            return 0;
        }

    }
}

