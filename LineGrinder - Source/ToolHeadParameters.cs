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
    /// A class to maintain tool head parameters (feed rates etc) used during
    /// the generation of the GCode 
    /// </summary>
    public class ToolHeadParameters : OISObjBase
    {
        int idVar = 0;

        // NOTE: the defaults here are the ISO defaults from the FileManager.
        //       There is no particular reason for this choice other than there
        //       has to be some default values and these seem reasonable

        // this is the distance into the material we cut this will be negative 
        // because zero is the surface of the pcb we are cutting
        private float zCutLevel = FileManager.DEFAULT_ISOZCUTLEVEL;

        // this is the an alternative distance into the material we cut
        private float zAlt1CutLevel = FileManager.DEFAULT_ISOALT1ZCUTLEVEL;

        // this is the distance above the PCB we move the z axis to
        // so that we can hop from cut to cut. It will be positive
        // because it is above the surface of the pcb we are cutting
        private float zMoveLevel = FileManager.DEFAULT_ISOZMOVELEVEL;

        // this is the distance above the PCB we move the z axis to
        // so that we can move about. It will be positive
        // because it is above the surface of the pcb we are cutting
        // and should be large enough to clear all hold downs and clamps etc
        private float zClearLevel = FileManager.DEFAULT_ISOZCLEARLEVEL;

        // this is the speed at which the tool (in application units per second) moves vertically into the work.
        private float zFeedRate = FileManager.DEFAULT_ISOZFEEDRATE;

        // this is the speed at which the tool (in application units per second) moves horizontally over the work.
        private float xyFeedRate = FileManager.DEFAULT_ISOXYFEEDRATE;

        // this is essentially the diameter of the line the isolation milling bit cuts at the ZCutLevel
        private float toolWidth = FileManager.DEFAULT_ISOCUT_WIDTH;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="idVarIn">id value for diagnostics</param>
        public ToolHeadParameters(int idVarIn)
        {
            idVar = idVarIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="idVarIn">id value for diagnostics</param>
        /// <param name="fileManagerObj">the file manager</param>
        /// <param name="toolHeadMode">the mode</param>
        public ToolHeadParameters(int idVarIn, FileManager fileManagerObj, ToolHeadParametersModeEnum toolHeadMode)
        {
            idVar = idVarIn;
            // set up the parameters
            SetToolHeadParametersFromMode(fileManagerObj, toolHeadMode);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets all toolhead parameters to the defaults. 
        /// </summary>
        public void Reset()
        {
            zCutLevel = FileManager.DEFAULT_ISOZCUTLEVEL;
            zAlt1CutLevel = FileManager.DEFAULT_ISOALT1ZCUTLEVEL;
            zMoveLevel = FileManager.DEFAULT_ISOZMOVELEVEL;
            zClearLevel = FileManager.DEFAULT_ISOZCLEARLEVEL;
            zFeedRate = FileManager.DEFAULT_ISOZFEEDRATE;
            xyFeedRate = FileManager.DEFAULT_ISOXYFEEDRATE;
            toolWidth = FileManager.DEFAULT_ISOCUT_WIDTH;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the toolhead parameters according to a specified mode. Things are
        /// setup slightly differently sometimes depending on the mode
        /// </summary>
        /// <param name="fileManagerObj">the file manager, if null just resets</param>
        /// <param name="toolHeadMode">the mode for this toolhead</param>
        private void SetToolHeadParametersFromMode(FileManager fileManagerObj, ToolHeadParametersModeEnum toolHeadMode)
        {
            if(fileManagerObj==null)
            {
                Reset();
                return;
            }
            switch (toolHeadMode)
            {
                case ToolHeadParametersModeEnum.ISOCUT:
                  //  DebugMessage("isocutwidth d=" + fileManagerObj.IsoCutWidth.ToString());
                    toolWidth = fileManagerObj.IsoCutWidth;
                    xyFeedRate = fileManagerObj.IsoXYFeedRate;
                    zFeedRate = fileManagerObj.IsoZFeedRate;
                    zCutLevel = fileManagerObj.IsoZCutLevel;
                    zAlt1CutLevel = fileManagerObj.IsoPadTouchDownZLevel;
                    zMoveLevel = fileManagerObj.IsoZMoveLevel;
                    zClearLevel = fileManagerObj.IsoZClearLevel;
                    return;
                case ToolHeadParametersModeEnum.TEXTANDLABELS:
                    toolWidth = fileManagerObj.IsoCutWidth;
                  //  DebugMessage("isocutwidth e=" + fileManagerObj.IsoCutWidth.ToString());
                    xyFeedRate = fileManagerObj.IsoXYFeedRate;
                    zFeedRate = fileManagerObj.IsoZFeedRate;
                    zCutLevel = fileManagerObj.IsoZCutLevel;
                    zAlt1CutLevel = fileManagerObj.IsoZCutLevel;
                    zMoveLevel = fileManagerObj.IsoZMoveLevel;
                    zClearLevel = fileManagerObj.IsoZClearLevel;
                    return;
                case ToolHeadParametersModeEnum.EDGEMILL:
                    toolWidth = fileManagerObj.EdgeMillCutWidth;
                    xyFeedRate = fileManagerObj.EdgeMillXYFeedRate;
                    zFeedRate = fileManagerObj.EdgeMillZFeedRate;
                    zCutLevel = fileManagerObj.EdgeMillZCutLevel;
                    zAlt1CutLevel = fileManagerObj.EdgeMillZCutLevel;
                    zMoveLevel = fileManagerObj.EdgeMillZMoveLevel;
                    zClearLevel = fileManagerObj.EdgeMillZClearLevel;
                    return;
                case ToolHeadParametersModeEnum.REFPINS:
                    toolWidth = fileManagerObj.ReferencePinPadDiameter;
                    xyFeedRate = fileManagerObj.ReferencePinsXYFeedRate;
                    zFeedRate = fileManagerObj.ReferencePinsZFeedRate;
                    zCutLevel = fileManagerObj.ReferencePinsZDrillDepth;
                    zAlt1CutLevel = fileManagerObj.ReferencePinsZDrillDepth;
                    zMoveLevel = fileManagerObj.ReferencePinsZClearLevel; // no move level here
                    zClearLevel = fileManagerObj.ReferencePinsZClearLevel;
                    return;
                case ToolHeadParametersModeEnum.PADTOUCHDOWNS:
                    toolWidth = fileManagerObj.IsoPadTouchDownZLevel;
                    xyFeedRate = fileManagerObj.IsoXYFeedRate;
                    zFeedRate = fileManagerObj.IsoZFeedRate;
                    zCutLevel = fileManagerObj.IsoPadTouchDownZLevel;
                    zAlt1CutLevel = fileManagerObj.IsoPadTouchDownZLevel;
                    zMoveLevel = fileManagerObj.IsoZMoveLevel; // use iso move level
                    zClearLevel = fileManagerObj.IsoZClearLevel; // use iso clear level
                    return;
                case ToolHeadParametersModeEnum.BEDFLATTENING:
                    toolWidth = fileManagerObj.BedFlatteningMillWidth;
                    xyFeedRate = fileManagerObj.BedFlatteningXYFeedRate;
                    zFeedRate = fileManagerObj.BedFlatteningZFeedRate;
                    zCutLevel = fileManagerObj.BedFlatteningZCutLevel;
                    zAlt1CutLevel = fileManagerObj.BedFlatteningZCutLevel;
                    zMoveLevel = fileManagerObj.BedFlatteningZClearLevel; // no move level here
                    zClearLevel = fileManagerObj.BedFlatteningZClearLevel;
                    return;
                case ToolHeadParametersModeEnum.EXCELLONDRILL:
                    toolWidth = FileManager.DEFAULT_ISOCUT_WIDTH;
                    xyFeedRate = fileManagerObj.DrillingXYFeedRate;
                    zFeedRate = fileManagerObj.DrillingZFeedRate;
                    zCutLevel = fileManagerObj.DrillingZDepth;
                    zAlt1CutLevel = fileManagerObj.DrillingZDepth;
                    zMoveLevel = fileManagerObj.DrillingZClearLevel; // no move level here
                    zClearLevel = fileManagerObj.DrillingZClearLevel; 
                    return;
                case ToolHeadParametersModeEnum.DEFAULT:
                default:
                    Reset();
                    return;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the zCutLevel. 
        /// </summary>
        public float ZCutLevel
        {
            get
            {
                return zCutLevel;
            }
            set
            {
                zCutLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the zAlt1CutLevel. 
        /// </summary>
        public float ZAlt1CutLevel
        {
            get
            {
                return zAlt1CutLevel;
            }
            set
            {
                zAlt1CutLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the zMoveLevel. 
        /// </summary>
        public float ZMoveLevel
        {
            get
            {
                return zMoveLevel;
            }
            set
            {
                zMoveLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the zClearLevel. 
        /// </summary>
        public float ZClearLevel
        {
            get
            {
                return zClearLevel;
            }
            set
            {
                zClearLevel = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the zFeedRate. Will never get or set a negative
        /// or zero value. 
        /// </summary>
        public float ZFeedRate
        {
            get
            {
                if (zFeedRate <= 0) zFeedRate = FileManager.DEFAULT_ISOZFEEDRATE;
                return zFeedRate;
            }
            set
            {
                zFeedRate = value;
                if (zFeedRate <= 0) zFeedRate = FileManager.DEFAULT_ISOZFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the xyFeedRate. Will never get or set a negative
        /// or xyero value. 
        /// </summary>
        public float XYFeedRate
        {
            get
            {
                if (xyFeedRate <= 0) xyFeedRate = FileManager.DEFAULT_ISOXYFEEDRATE;
                return xyFeedRate;
            }
            set
            {
                xyFeedRate = value;
                if (xyFeedRate <= 0) xyFeedRate = FileManager.DEFAULT_ISOXYFEEDRATE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the toolWidth. Will never get or set a negative
        /// or zero value. 
        /// </summary>
        public float ToolWidth
        {
            get
            {
                if (toolWidth <= 0) toolWidth = FileManager.DEFAULT_ISOCUT_WIDTH;
                return toolWidth;
            }
            set
            {
                toolWidth = value;
                if (toolWidth <= 0) toolWidth = FileManager.DEFAULT_ISOCUT_WIDTH;
            }
        }

    }
}

