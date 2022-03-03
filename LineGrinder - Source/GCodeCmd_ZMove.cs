using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
    /// A class to encapsulate a GCode move in Z axis object. The height is 
    /// dependent on the MoveMode
    /// </summary>
    public class GCodeCmd_ZMove : GCodeCmd
    {
        // this tells the gcode interpreter how high we move when we move
        public enum GCodeZMoveHeightEnum
        {
            GCodeZMoveHeight_ZCoordForMove,
            GCodeZMoveHeight_ZCoordForClear,
            GCodeZMoveHeight_ZCoordForCut,
            GCodeZMoveHeight_ZCoordForAlt1Cut,
        }
        public const GCodeZMoveHeightEnum DEFAULT_ZMOVEHEIGHT = GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForClear;
        private GCodeZMoveHeightEnum zMoveHeight = DEFAULT_ZMOVEHEIGHT;

        // these are only used for GCode plots. The GCode cmd does not need them
        private float drillDiameter = 0f;
        private int x0 = -1;
        private int y0 = -1;

        // if true we output a G1 (linear interpolation) rather than a G0 (rapid move) 
        private bool wantLinearMove = false;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="zMoveHeightIn">where to move the bit</param>
        public GCodeCmd_ZMove(GCodeZMoveHeightEnum zMoveHeightIn)
            : base()
        {
            zMoveHeight = zMoveHeightIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="zMoveHeightIn">where to move the bit</param>
        /// <param name="commentTextIn">comment text</param>
        public GCodeCmd_ZMove(GCodeZMoveHeightEnum zMoveHeightIn, string commentTextIn)
            : base()
        {
            zMoveHeight = zMoveHeightIn;
            CommentText = commentTextIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets some values we use when plotting this GCode. We do not need these
        /// to generate the GCode line.
        /// </summary>
        public void SetGCodePlotDrillValues(int x0In, int y0In, float drillDiameterIn)
        {
            x0 = x0In;
            y0 = y0In;
            drillDiameter = drillDiameterIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the wantLinearMove value. If true, we output a G1 (linear 
        /// interpolation) rather than a G0 (rapid move)
        /// </summary>
        public bool WantLinearMove
        {
            get
            {
                return wantLinearMove;
            }
            set
            {
                wantLinearMove = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Overrides ToString()
        /// </summary>
        public override string ToString()
        {
            return "GCodeCmd_ZMove " + " zMoveHeight=" + zMoveHeight.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current rapid move height
        /// </summary>
        public GCodeZMoveHeightEnum ZMoveHeight
        {
            get
            {
                return zMoveHeight;
            }
            set
            {
                zMoveHeight = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GCode lines for this object as they would be written to the text file. Will 
        /// never return a null value. Can often return more than one line if the 
        /// current context in the stateMachine requires it
        /// </summary>
        /// <param name="stateMachine">the stateMachine</param>
        public override string GetGCodeCmd(GCodeFileStateMachine stateMachine)
        {
            StringBuilder sb = new StringBuilder();
            float zCoordForMove = -1;

            if (stateMachine == null)
            {
                LogMessage("GetGCodeCmd: stateMachine == null");
                return "M5 ERROR: stateMachine==null";
            }

            // set the height we wish to move at
            if (ZMoveHeight == GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForClear)
            {
                zCoordForMove = stateMachine.ZCoordForClear;
            }
            else if (ZMoveHeight == GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForCut)
            {
                zCoordForMove = stateMachine.ZCoordForCut;
            }
            else if (ZMoveHeight == GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForAlt1Cut)
            {
                zCoordForMove = stateMachine.ZCoordForAlt1Cut;
            }
            else
            {
                zCoordForMove = stateMachine.ZCoordForMove;
            }

            // set z axis depth now - we always write it out
            if (stateMachine.GCodeFileManager.ShowGCodeCmdNumbers == true) sb.Append(stateMachine.BuildNextLineNumberString() + " ");
            if (wantLinearMove == true)
            {
                sb.Append(GCODEWORD_MOVEINLINE + " " + GCODEWORD_ZAXIS + zCoordForMove.ToString());
                stateMachine.LastGCodeZCoord = zCoordForMove;
                // do we need to adjust the feedrate?
                if (stateMachine.LastFeedRate != stateMachine.CurrentZFeedrate)
                {
                    // yes we do 
                    sb.Append(" " + GCODEWORD_FEEDRATE + stateMachine.CurrentZFeedrate.ToString());
                    // remember this now
                    stateMachine.LastFeedRate = stateMachine.CurrentZFeedrate;
                }
            }
            else
            {
                sb.Append(GCODEWORD_MOVERAPID + " " + GCODEWORD_ZAXIS + zCoordForMove.ToString());
                stateMachine.LastGCodeZCoord = zCoordForMove;
              // no feedrates on MOVERAPID, these are set by the machine controller
            }

            sb.Append(stateMachine.LineTerminator);
            return sb.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Performs the action the plot requires based on the current context
        /// </summary>
        /// <param name="graphicsObj">a graphics object on which to plot</param>
        /// <param name="stateMachine">the gcode plot state machine</param>
        /// <param name="errorString">the error string we return on fail</param>
        /// <param name="errorValue">the error value we return on fail, z success, nz fail </param>
        /// <param name="wantEndPointMarkers">if true we draw the endpoints of the gcodes
        /// in a different color</param>
        /// <returns>an enum value indicating what next action to take</returns>
        public override PlotActionEnum PerformPlotGCodeAction(Graphics graphicsObj, GCodeFileStateMachine stateMachine, bool wantEndPointMarkers, ref int errorValue, ref string errorString)
        {
            Brush workingBrush = null;

            if (stateMachine == null)
            {
                errorValue = 999;
                errorString = "PerformPlotGCodeAction (Line) stateMachine == null";
                return PlotActionEnum.PlotAction_FailWithError;
            }

            // we only plot this on cuts or touchdowns
            if (((zMoveHeight == GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForCut) ||
                 (zMoveHeight == GCodeZMoveHeightEnum.GCodeZMoveHeight_ZCoordForAlt1Cut)) == false)
            {
                return PlotActionEnum.PlotAction_Continue;
            }
            // if the drill width <=0 we do not plot
            if(drillDiameter<=0)
            {
                 return PlotActionEnum.PlotAction_Continue;
            }

            // get the brush
             workingBrush = stateMachine.PlotBorderBrush;

            // now draw in a circle of appropriate diameter
            MiscGraphicsUtils.FillEllipseCenteredOnPoint(graphicsObj, workingBrush, x0, y0 , drillDiameter, drillDiameter);

            return PlotActionEnum.PlotAction_Continue;

        }

    }
}

