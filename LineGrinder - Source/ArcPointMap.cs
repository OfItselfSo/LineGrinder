using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// A container used to track percentages of a half quadrant we draw for an arc
    /// </summary>
    public class ArcPointMap
    {
        // This code is specifically designed to work with the gcode draw arc functions
        // this code calc's the cells to use for 8 half quadrants. The values in here
        // represent the parts of the quadrant we draw or the parts of the quadrant we skip

        // percentSkip is the percentage of the arc from the start we skip before we start 
        // drawing

        // percentDraw is the percentage of the arc from the start we draw after we are 
        // finished skipping

        // note that the skip and draw can happen in different directions depending on if 
        // we draw clockwise or counter clockwise

        private float percentSkip = 0;
        private float percentDraw = 0;

        // these are the percent values converted to cells once we know how many cells
        // are represented in the arc
        private int cellsToSkip = 0;
        private int cellsToDraw = 0;

        // these tell us the start and stop octants
        private bool thisIsTheStartOctant = false;
        private bool thisIsTheStopOctant = false;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="percentSkipIn">percent of arc we skip from start</param>
        /// <param name="percentDrawIn">percent of arc we draw after we skip</param>
        public ArcPointMap(float percentSkipIn, float percentDrawIn)
        {
            percentSkip = percentSkipIn;
            percentDraw = percentDrawIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the cell counts
        /// </summary>
        /// <param name="numCellsInArc">the number of cells in the arc</param>
        public void SetCellCounts(int numCellsInArc)
        {
            if (numCellsInArc <= 0) return;
            cellsToSkip = (int)(percentSkip * numCellsInArc);
            cellsToDraw = (int)(percentDraw * numCellsInArc);
    }

    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// Gets/Sets the start octant flag
    /// </summary>
    public bool ThisIsTheStartOctant
        {
            get
            {
                return thisIsTheStartOctant;
            }
            set
            {
                thisIsTheStartOctant = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the stop octant flag
        /// </summary>
        public bool ThisIsTheStopOctant
        {
            get
            {
                return thisIsTheStopOctant;
            }
            set
            {
                thisIsTheStopOctant = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the percent skip value
        /// </summary>
        public float PercentSkip
        {
            get
            {
                return percentSkip;
            }
            set
            {
                percentSkip = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the percent fill value
        /// </summary>
        public float PercentDraw
        {
            get
            {
                return percentDraw;
            }
            set
            {
                percentDraw = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the cells to skip value
        /// </summary>
        public float CellsToSkip
        {
            get
            {
                return cellsToSkip;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the cells to draw value
        /// </summary>
        public float CellsToDraw
        {
            get
            {
                return cellsToDraw;
            }

        }
    }
}

