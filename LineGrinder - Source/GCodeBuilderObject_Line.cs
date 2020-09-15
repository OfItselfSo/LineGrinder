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
    /// A class for line objects the GCodeBuilder creates to 
    /// convert Gerber drawing primitives into GCodes
    /// </summary>
    /// <history>
    ///    26 Jul 10  Cynic - Started
    /// </history>
    public class GCodeBuilderObject_Line : GCodeBuilderObject
    {

        // NOTE: In general, if a coordinate is an int it has been scaled and it represents
        //       a value in plot coordinates. If it is a float it represents an unscaled
        //       value from the gerber file or gCode file

        // these are the endpoints of the centerline line
        private int x0 = -1;
        private int y0 = -1;
        private int x1 = -1;
        private int y1 = -1;
        private int width = -1;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="builderObjectIDIn">the builderObjectID for this object</param>
        /// <param name="x0">X0 coord</param>
        /// <param name="y0">Y0 coord</param>
        /// <param name="x1">X1 coord</param>
        /// <param name="y1">Y1 coord</param>
        /// <param name="width">The perpendicular width of the line</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public GCodeBuilderObject_Line(int builderObjectIDIn, int x0In, int y0In, int x1In, int y1In, int widthIn)
            : base(builderObjectIDIn)
        {
            x0 = x0In;
            y0 = y0In;
            x1 = x1In;
            y1 = y1In;
            width = widthIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current width value
        /// </summary>
        /// <history>
        ///    03 Aug 10  Cynic - Started
        /// </history>
        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current x0 value
        /// </summary>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public int X0
        {
            get
            {
                return x0;
            }
            set
            {
                x0 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current y0 value
        /// </summary>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public int Y0
        {
            get
            {
                return y0;
            }
            set
            {
                y0 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current x1 value
        /// </summary>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public int X1
        {
            get
            {
                return x1;
            }
            set
            {
                x1 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current y1 value
        /// </summary>
        /// <history>
        ///    30 Jul 10  Cynic - Started
        /// </history>
        public int Y1
        {
            get
            {
                return y1;
            }
            set
            {
                y1 = value;
            }
        }
    }
}
