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
    /// A class to Gerber Contour Line for comparisons
    /// </summary>
    public class GerberContourLineContainer
    {
        private float xStart;
        private float yStart;
        private float xEnd;
        private float yEnd;

        private GerberLine_DCode dCodeObj = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xStartIn">xStart coord</param>
        /// <param name="yStartIn">yStart coord</param>
        /// <param name="xEndIn">xEnd coord</param>
        /// <param name="yEndIn">yEnd coord</param>
        /// <param name="dCodeObjIn">the DCodeObj representing these coords</param>
        public GerberContourLineContainer(float xStartIn, float yStartIn, float xEndIn, float yEndIn, GerberLine_DCode dCodeObjIn)
        {
            xStart = xStartIn;
            yStart = yStartIn;
            xEnd = xEndIn;
            yEnd = yEndIn;

            dCodeObj = dCodeObjIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the dCodeObject. Will return null.
        /// </summary>
        public GerberLine_DCode DCodeObj
        {
            get
            {
                return dCodeObj;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the xStartVar
        /// </summary>
        public float XStart
        {
            get
            {
                return xStart;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the yStartVar
        /// </summary>
        public float YStart
        {
            get
            {
                return yStart;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the xEndVar
        /// </summary>
        public float XEnd
        {
            get
            {
                return xEnd;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the yEndVar
        /// </summary>
        public float YEnd
        {
            get
            {
                return yEnd;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if this line is horizontal or vertical
        /// </summary>
        /// <returns>true - is horiz or vert, false - is not</returns>
        public bool IsHorizontalOrVertical()
        {
            // easy test
            if (XStart == XEnd) return true;
            if (YStart == YEnd) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if this line is coincident with the input line.
        /// </summary>
        /// <returns>true - is coincident, false - is not</returns>
        public bool IsCoincidentLine(GerberContourLineContainer containerIn)
        {
            if (containerIn == null) return false;
            if (containerIn.DCodeObj == null) return false;

            // do the endpoints match exactly? compare this way first it
            // is the most likely way to match
            if ((containerIn.XStart == XEnd) &&
                (containerIn.YStart == YEnd) &&
                (containerIn.XEnd == XStart) &&
                (containerIn.YEnd == YStart)) return true;

            // also try this way
            if ((containerIn.XStart == XStart) &&
                (containerIn.YStart == YStart) &&
                (containerIn.XEnd == XEnd) &&
                (containerIn.YEnd == YEnd)) return true;

             // they do not match. The lines are not coincident
             return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Custom ToString()
        /// </summary>
        public override string ToString()
        {
            return "x1=" + xStart.ToString() + ", y1=" + yStart.ToString() + ", x2=" + xEnd.ToString() + ", y2=" + yEnd.ToString();
        }
    }
}

